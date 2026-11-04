#!/usr/bin/env python3
"""Compile runtime/sample against project assemblies and run focused haptic/review regressions.
Run from the Unity project root after it has generated Library/ScriptAssemblies.
"""
from pathlib import Path
import re, subprocess, tempfile, argparse
import xml.etree.ElementTree as ET
parser = argparse.ArgumentParser()
parser.add_argument('--ui-assembly', type=Path, help='Override a stale generated Dreamy.UI.Runtime assembly.')
parser.add_argument('--shop', action='store_true', help='Also run Shop presenter and model regressions.')
parser.add_argument('--features', action='store_true', help='Run additional feature model/presenter regressions.')
args = parser.parse_args()
root = Path.cwd()
package = Path(__file__).resolve().parents[1]
sample = package / 'Samples~/Settings Feature'
imported = root / 'Assets/Samples/Dreamy Settings/0.2.0/Settings Feature'
manifest = __import__('json').loads((package / 'package.json').read_text())
assert manifest['samples'][0]['path'] == 'Samples~/Settings Feature'
assembly = __import__('json').loads((package / 'Runtime/Dreamy.Settings.Runtime.asmdef').read_text())
assert assembly['noEngineReferences']
for source in (package / 'Runtime').rglob('*.cs'):
    assert not re.search(r'using Unity(?:Editor|Engine)', source.read_text()), source
for directory in (sample, imported):
    assert not list(directory.glob('*Launcher.cs'))
    assert not list(directory.glob('*Demo.cs'))
    assert not list(directory.glob('*Controller.cs'))
    for name in ['SettingsPanel', 'RateUsPanel']:
        path = directory / 'Prefabs' / (name + '.prefab')
        source = path.read_text()
        ids = re.findall(r'^--- !u!\d+ &(\d+)(?: stripped)?$', source, re.M)
        assert len(ids) == len(set(ids)), path
        for line in source.splitlines():
            if 'guid:' not in line:
                for ref in re.findall(r'fileID: (\d+)', line):
                    assert ref == '0' or ref in ids, (path, ref)
        assert 'Integration.' + name in source, path
        assert 'Controller' not in source, path
        assert 'Launcher' not in source and 'Integration.RateUsDemo' not in source, path
for path in sample.glob('*.cs'):
    assert path.read_bytes() == (imported / path.name).read_bytes(), path
for name in ['SettingsPanel.cs', 'RateUsPanel.cs']:
    source = (sample / name).read_text()
    for forbidden in ['new SettingsPresenter', 'new RateUsPresenter', 'SettingsFeatureInstaller', 'ServiceLocator', 'PanelManager.Instance']:
        assert forbidden not in source, (name, forbidden)
print('PASS MVP view boundary, prefab references, sample synchronization and runtime boundary', flush=True)
with tempfile.TemporaryDirectory(prefix='dreamy-settings-') as temp:
    temp = Path(temp)
    ui = root / 'LocalPackages/com.dreamy.ui'

    def create_project(name, sources, excluded, overrides=None, executable=False, reference_names=None, precompiled_names=None):
        directory = temp / name
        directory.mkdir()
        project = ET.Element('Project', Sdk='Microsoft.NET.Sdk')
        props = ET.SubElement(project, 'PropertyGroup')
        for key, value in {'TargetFramework': 'net10.0', 'OutputType': 'Exe' if executable else 'Library',
                           'AssemblyName': name, 'GenerateDependencyFile': 'true' if executable else 'false', 'EnableDefaultCompileItems': 'false', 'NoWarn': '0067;0649'}.items():
            ET.SubElement(props, key).text = value
        items = ET.SubElement(project, 'ItemGroup')
        for source in sources:
            ET.SubElement(items, 'Compile', Include=str(source))
        refs = ET.SubElement(project, 'ItemGroup')
        paths = {dll.stem: dll for dll in (root / 'Library/ScriptAssemblies').glob('*.dll') if dll.stem not in excluded and '.Editor' not in dll.stem and '.Tests' not in dll.stem and dll.stem != 'Dreamy.Template.Runtime'}
        if reference_names is not None:
            paths = {name_ref: target for name_ref, target in paths.items() if name_ref in reference_names}
        # Engine and plugin references come from Unity's generated project.
        for generated in ['Dreamy.Audio.Runtime.csproj', 'Dreamy.UI.Runtime.csproj', 'Dreamy.Template.Runtime.csproj']:
            for reference in ET.parse(root / generated).findall('.//Reference'):
                hint = reference.find('HintPath')
                if hint is not None:
                    target = Path(hint.text)
                    if not target.is_absolute(): target = root / target
                    name_ref = reference.attrib['Include']
                    if name_ref.startswith('UnityEngine') or (target.name in {'DOTween.dll', 'Newtonsoft.Json.dll'}
                            and (precompiled_names is None or target.name in precompiled_names)):
                        paths[name_ref] = target
        paths.update({name_ref: target for name_ref, target in (overrides or {}).items()
                      if reference_names is None or name_ref in reference_names})
        for name_ref, target in paths.items():
            reference = ET.SubElement(refs, 'Reference', Include=name_ref)
            ET.SubElement(reference, 'HintPath').text = str(target)
        path = directory / 'Harness.csproj'
        ET.ElementTree(project).write(path)
        return path, directory / 'bin/Debug/net10.0' / (name + '.dll')

    contract_project, contract_dll = create_project('Dreamy.UI.Presentation',
        (ui / 'Runtime/Presentation').glob('*.cs'), {'Dreamy.UI.Presentation'})
    subprocess.run(['dotnet', 'build', str(contract_project), '--verbosity', 'quiet'], check=True)
    ui_project, ui_dll = create_project('Dreamy.UI.Runtime',
        [p for p in (ui / 'Runtime').rglob('*.cs') if 'Presentation' not in p.parts],
        {'Dreamy.UI.Runtime', 'Dreamy.UI.Presentation'}, {'Dreamy.UI.Presentation': contract_dll})
    subprocess.run(['dotnet', 'build', str(ui_project), '--verbosity', 'quiet'], check=True)
    overrides = {'Dreamy.UI.Presentation': contract_dll, 'Dreamy.UI.Runtime': args.ui_assembly or ui_dll}
    settings_project, settings_dll = create_project('Dreamy.Settings.Runtime',
        (package / 'Runtime').rglob('*.cs'), {'Dreamy.Settings.Runtime', 'Dreamy.UI.Presentation'}, overrides)
    subprocess.run(['dotnet', 'build', str(settings_project), '--verbosity', 'quiet'], check=True)
    overrides['Dreamy.Settings.Runtime'] = settings_dll
    sample_project, sample_dll = create_project('Dreamy.Feature.Settings.Integration.Runtime',
        sample.glob('*.cs'), {'Dreamy.Feature.Settings.Integration.Runtime', 'Dreamy.UI.Presentation'}, overrides)
    subprocess.run(['dotnet', 'build', str(sample_project), '--verbosity', 'quiet'], check=True)
    overrides['Dreamy.Feature.Settings.Integration.Runtime'] = sample_dll
    harness_project, _ = create_project('SettingsHarness', (package / 'Tests~').glob('*.cs'),
        {'Dreamy.UI.Presentation'}, overrides, executable=True)
    subprocess.run(['dotnet', 'run', '--project', str(harness_project), '--verbosity', 'quiet'], check=True)
    # Compile focused Unity tests; execution still requires the Editor.
    nunit = next(h.text for h in ET.parse(root / 'Dreamy.UI.Editor.Tests.csproj').findall('.//HintPath')
                 if h.text.endswith('/nunit.framework.dll'))
    test_assembly = __import__('json').loads((ui / 'Tests/Editor/Dreamy.UI.Editor.Tests.asmdef').read_text())
    test_refs = set(test_assembly['references']) | {'nunit.framework'}
    tests_project, _ = create_project('PresenterEditorTests',
        [ui / 'Tests/Editor/PanelPresenterTests.cs'], {'Dreamy.UI.Presentation'},
        dict(overrides, **{'nunit.framework': root / nunit}), reference_names=test_refs)
    subprocess.run(['dotnet', 'build', str(tests_project), '--verbosity', 'quiet'], check=True)
    shop = root / 'LocalPackages/com.dreamy.feature.shop'
    shop_sample = shop / 'Samples~/Shop Feature'
    for directory in [shop_sample, root / 'Assets/Samples/Dreamy Shop/0.1.1/Shop Feature']:
        assert not (directory / 'ShopDemo.cs').exists()
        assert 'new ShopPresenter' not in (directory / 'ShopPanel.cs').read_text()
    assert __import__('json').loads((shop / 'Runtime/Dreamy.Shop.Runtime.asmdef').read_text())['noEngineReferences']
    shop_project, shop_dll = create_project('Dreamy.Shop.Runtime', (shop / 'Runtime').rglob('*.cs'),
        {'Dreamy.Shop.Runtime', 'Dreamy.UI.Presentation'}, overrides)
    subprocess.run(['dotnet', 'build', str(shop_project), '--verbosity', 'quiet'], check=True)
    overrides['Dreamy.Shop.Runtime'] = shop_dll
    shop_sample_project, shop_sample_dll = create_project('Dreamy.Feature.Shop.Integration.Runtime',
        shop_sample.glob('*.cs'), {'Dreamy.Feature.Shop.Integration.Runtime', 'Dreamy.UI.Presentation'}, overrides)
    subprocess.run(['dotnet', 'build', str(shop_sample_project), '--verbosity', 'quiet'], check=True)
    overrides['Dreamy.Feature.Shop.Integration.Runtime'] = shop_sample_dll
    if args.shop:
        shop_harness_project, _ = create_project('ShopHarness',
            list((shop / 'Tests~').glob('*.cs')) + list((shop / 'Tests/Runtime').glob('*.cs')),
            {'Dreamy.UI.Presentation'}, dict(overrides, **{'nunit.framework': root / nunit}), executable=True)
        subprocess.run(['dotnet', 'run', '--project', str(shop_harness_project), '--verbosity', 'quiet'], check=True)
    extra_features = [
        ('daily-reward', 'DailyReward', 'Daily Reward Feature', 'Dreamy Daily Reward', '0.1.0'),
        ('missions', 'Missions', 'Mission Feature', 'Dreamy Missions', '0.1.0'),
        ('lucky-wheel', 'LuckyWheel', 'Lucky Wheel Feature', 'Dreamy Lucky Wheel', '0.1.0'),
        ('progression', 'Progression', 'Basic Progression/UI', 'Dreamy Progression', '0.1.1'),
        ('tutorial', 'Tutorial', 'Tutorial Feature/Runtime', 'Dreamy Tutorial', '0.1.0')]
    feature_tests = []
    for slug, name, folder, display, version in extra_features:
        feature = root / ('LocalPackages/com.dreamy.feature.' + slug)
        definition = __import__('json').loads((feature / 'Runtime' / ('Dreamy.' + name + '.Runtime.asmdef')).read_text())
        assert definition['noEngineReferences'], feature
        for source in (feature / 'Runtime').rglob('*.cs'):
            assert not re.search(r'using Unity(?:Editor|Engine)', source.read_text()), source
        runtime_project, runtime_dll = create_project('Dreamy.' + name + '.Runtime',
            (feature / 'Runtime').rglob('*.cs'), {'Dreamy.' + name + '.Runtime', 'Dreamy.UI.Presentation'}, overrides,
            reference_names=set(definition['references']),
            precompiled_names=set(definition.get('precompiledReferences', [])) if definition.get('overrideReferences') else None)
        subprocess.run(['dotnet', 'build', str(runtime_project), '--verbosity', 'quiet'], check=True)
        overrides['Dreamy.' + name + '.Runtime'] = runtime_dll
        if slug == 'tutorial':
            integration_def = __import__('json').loads((feature / 'Integration/Runtime/Dreamy.Tutorial.Integration.Runtime.asmdef').read_text())
            integration_project, integration_dll = create_project('Dreamy.Tutorial.Integration.Runtime',
                (feature / 'Integration/Runtime').glob('*.cs'), {'Dreamy.Tutorial.Integration.Runtime', 'Dreamy.UI.Presentation'},
                overrides, reference_names=set(integration_def['references']),
                precompiled_names=set(integration_def.get('precompiledReferences', [])) if integration_def.get('overrideReferences') else None)
            subprocess.run(['dotnet', 'build', str(integration_project), '--verbosity', 'quiet'], check=True)
            overrides['Dreamy.Tutorial.Integration.Runtime'] = integration_dll
        feature_sample = feature / 'Samples~' / folder
        definition_path = next(feature_sample.glob('*.asmdef'))
        sample_def = __import__('json').loads(definition_path.read_text())
        imported_feature = root / 'Assets/Samples' / display / version / folder
        sample_sources = list(feature_sample.glob('*.cs'))
        if imported_feature.exists():
            sample_sources.extend(p for p in imported_feature.glob('*.cs') if not (feature_sample / p.name).exists())
        sample_project, sample_dll = create_project(sample_def['name'], sample_sources,
            {sample_def['name'], 'Dreamy.UI.Presentation'}, overrides, reference_names=set(sample_def['references']),
            precompiled_names=set(sample_def.get('precompiledReferences', [])) if sample_def.get('overrideReferences') else None)
        subprocess.run(['dotnet', 'build', str(sample_project), '--verbosity', 'quiet'], check=True)
        overrides[sample_def['name']] = sample_dll
        if imported_feature.exists():
            for source in feature_sample.glob('*.cs'):
                assert source.read_bytes() == (imported_feature / source.name).read_bytes(), source
        if slug == 'tutorial':
            feature_tests.extend(feature / 'Tests/Editor' / n for n in
                ['TutorialModelTests.cs', 'TutorialPresenterTests.cs', 'TutorialTestStore.cs'])
        else: feature_tests.extend((feature / 'Tests/Runtime').glob('*.cs'))
    # Compile native test sources against each asmdef's direct references too.
    for slug, name, folder, display, version in extra_features:
        feature = root / ('LocalPackages/com.dreamy.feature.' + slug)
        for definition_path in (feature / 'Tests').rglob('*.asmdef'):
            native_def = __import__('json').loads(definition_path.read_text())
            native_refs = set(native_def['references']) | {'nunit.framework'}
            # Unity injects these references for the legacy TestAssemblies option.
            test_overrides = dict(overrides, **{'nunit.framework': root / nunit})
            if 'TestAssemblies' in native_def.get('optionalUnityReferences', []):
                for runner in ['UnityEngine.TestRunner', 'UnityEditor.TestRunner']:
                    native_refs.add(runner)
                    test_overrides[runner] = root / 'Library/ScriptAssemblies' / (runner + '.dll')
            native_project, _ = create_project(native_def['name'], definition_path.parent.glob('*.cs'),
                {native_def['name'], 'Dreamy.UI.Presentation'}, test_overrides,
                reference_names=native_refs,
                precompiled_names=set(native_def.get('precompiledReferences', [])) if native_def.get('overrideReferences') else None)
            subprocess.run(['dotnet', 'build', str(native_project), '--verbosity', 'quiet'], check=True)
    if args.features:
        feature_tests.extend((root / 'LocalPackages/com.dreamy.feature/Tests~').glob('FeatureIntegrationHarness.cs'))
        integration_harness_project, _ = create_project('FeatureIntegrationHarness', feature_tests,
            {'Dreamy.UI.Presentation'}, dict(overrides, **{'nunit.framework': root / nunit}), executable=True)
        subprocess.run(['dotnet', 'run', '--project', str(integration_harness_project), '--verbosity', 'quiet'], check=True)
    # Additional integrations brought in by the template's main branch.
    for directory in [root / 'LocalPackages/com.dreamy.feedback/Runtime',
                      root / 'Assets/Samples/Dreamy Feedback/0.2.0/Basic Feedback',
                      root / 'Assets/Samples/Dreamy Feedback/0.2.0/Feedback Economy',
                      root / 'Assets/Samples/Dreamy Economy Contracts/0.2.0/Wallet Demo']:
        if not directory.exists(): continue
        definition = __import__('json').loads(next(directory.glob('*.asmdef')).read_text())
        sources = directory.rglob('*.cs') if directory.name == 'Runtime' else directory.glob('*.cs')
        extra_project, extra_dll = create_project(definition['name'], sources,
            {definition['name'], 'Dreamy.UI.Presentation'}, overrides,
            reference_names=set(definition['references']))
        subprocess.run(['dotnet', 'build', str(extra_project), '--verbosity', 'quiet'], check=True)
        overrides[definition['name']] = extra_dll
    template_project, _ = create_project('Dreamy.Template.Runtime',
        (root / 'Assets/_Project/Scripts').rglob('*.cs'), {'Dreamy.UI.Presentation'}, overrides)
    subprocess.run(['dotnet', 'build', str(template_project), '--verbosity', 'quiet'], check=True)
