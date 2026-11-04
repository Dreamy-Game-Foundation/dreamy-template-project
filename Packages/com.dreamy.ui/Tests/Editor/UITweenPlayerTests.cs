using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Dreamy.UI.Tests.Editor
{
    public sealed class UITweenPlayerTests
    {
        private GameObject root;
        private UITweenPlayer player;
        private TweenSettings preset;
        private readonly List<Object> assets = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("Tween test root");
            player = root.AddComponent<UITweenPlayer>();
            preset = ScriptableObject.CreateInstance<TweenSettings>();
            assets.Add(preset);
            preset.DelayIn = 0.4f;
            preset.DelayOut = 0.3f;
            preset.DurationIn = preset.DurationOut = 0.1f;
            preset.EaseIn = preset.EaseOut = Ease.Linear;
        }

        [TearDown]
        public void TearDown()
        {
            player.Kill();
            Object.DestroyImmediate(root);
            foreach (Object asset in assets) Object.DestroyImmediate(asset);
            assets.Clear();
        }

        [TestCase(TweenCollectionMode.Auto)]
        [TestCase(TweenCollectionMode.Manual)]
        public void Playback_RespectsPresetShowAndHideDelay(TweenCollectionMode mode)
        {
            Transform target = AddTween(mode);
            player.Init();
            UniTask show = player.ShowTween(CancellationToken.None);
            MakeManual(target);
            DOTween.ManualUpdate(0.39f, 0.39f);
            Assert.That(target.localScale, Is.EqualTo(Vector3.zero));
            DOTween.ManualUpdate(0.12f, 0.12f);
            show.GetAwaiter().GetResult();
            Assert.That(target.localScale, Is.EqualTo(Vector3.one));

            UniTask hide = player.HideTween(CancellationToken.None);
            MakeManual(target);
            DOTween.ManualUpdate(0.29f, 0.29f);
            Assert.That(target.localScale, Is.EqualTo(Vector3.one));
            DOTween.ManualUpdate(0.12f, 0.12f);
            hide.GetAwaiter().GetResult();
            Assert.That(target.localScale, Is.EqualTo(Vector3.zero));
        }

        [TestCase(TweenCollectionMode.Auto)]
        [TestCase(TweenCollectionMode.Manual)]
        public void IndexDelay_ReplacesConfiguredDelay_AndClearRestoresPreset(TweenCollectionMode mode)
        {
            Transform target = AddTween(mode);
            TweenDelayByIndex index = target.gameObject.AddComponent<TweenDelayByIndex>();
            player.RebuildCache();
            index.Apply(2, 0.05f, 0.03f, 1);
            AssertTiming(player.Tweens[0], 0.1f, 0.03f);
            index.Clear();
            AssertTiming(player.Tweens[0], 0.4f, 0.3f);
            index.Apply(-1, -1f, -1f, -1);
            AssertTiming(player.Tweens[0], 0f, 0f);
        }

        [TestCase(TweenCollectionMode.Auto)]
        [TestCase(TweenCollectionMode.Manual)]
        public void SlotDelay_AppliesToInactiveDescendants_AndClearRestoresPreset(TweenCollectionMode mode)
        {
            GameObject slotObject = new GameObject("Slot");
            slotObject.transform.SetParent(root.transform);
            TweenDelayByIndex slot = slotObject.AddComponent<TweenDelayByIndex>();
            Transform target = AddTween(mode);
            target.SetParent(slotObject.transform);
            target.gameObject.SetActive(false);
            player.RebuildCache();
            slot.OverrideDelay(true, 0.2f, 0f);
            AssertTiming(player.Tweens[0], 0.2f, 0f);
            slot.OverrideDelay(false, 0f, 0f);
            AssertTiming(player.Tweens[0], 0.4f, 0.3f);
        }

        [Test]
        public void DelayControl_AppliesShowAndReverseHideDelays_ByHierarchyGroup()
        {
            TweenDelayControl control = root.AddComponent<TweenDelayControl>();
            Set(control, "startDelay", 0.1f);
            Set(control, "showInterval", 0.05f);
            Set(control, "hideInterval", 0.03f);
            GameObject firstGroup = new GameObject("First group");
            firstGroup.transform.SetParent(root.transform);
            TweenDelayByIndex firstSlot = firstGroup.AddComponent<TweenDelayByIndex>();
            Transform first = AddTween(TweenCollectionMode.Auto);
            first.SetParent(firstGroup.transform);
            Transform second = AddTween(TweenCollectionMode.Auto);
            second.SetParent(firstGroup.transform);
            GameObject secondGroup = new GameObject("Second group");
            secondGroup.transform.SetParent(root.transform);
            TweenDelayByIndex secondSlot = secondGroup.AddComponent<TweenDelayByIndex>();
            Transform third = AddTween(TweenCollectionMode.Auto);
            third.SetParent(secondGroup.transform);
            third.gameObject.SetActive(false);
            control.ApplyStaggerDelays();
            AssertTiming(first.GetComponent<UITweenScale>(), 0.1f, 0.03f);
            AssertTiming(second.GetComponent<UITweenScale>(), 0.1f, 0.03f);
            AssertTiming(third.GetComponent<UITweenScale>(), 0.15f, 0f);
            Assert.That(firstSlot.Index, Is.Zero);
            Assert.That(secondSlot.Index, Is.EqualTo(1));
            control.ClearDelays();
            AssertTiming(first.GetComponent<UITweenScale>(), 0.4f, 0.3f);
            AssertTiming(second.GetComponent<UITweenScale>(), 0.4f, 0.3f);
            AssertTiming(third.GetComponent<UITweenScale>(), 0.4f, 0.3f);
        }

        [Test]
        public void SlotAwake_RecalculatesDelaysForNewSlot()
        {
            TweenDelayControl control = root.AddComponent<TweenDelayControl>();
            Transform first = AddTween(TweenCollectionMode.Auto);
            first.gameObject.AddComponent<TweenDelayByIndex>();
            control.ApplyStaggerDelays();
            Transform spawned = AddTween(TweenCollectionMode.Auto);
            spawned.SetAsFirstSibling();
            TweenDelayByIndex slot = spawned.gameObject.AddComponent<TweenDelayByIndex>();
            typeof(TweenDelayByIndex).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(slot, null);
            AssertTiming(spawned.GetComponent<UITweenScale>(), 0f, 0.03f);
            AssertTiming(first.GetComponent<UITweenScale>(), 0.05f, 0f);
        }

        [TestCase(TweenCollectionMode.Auto)]
        [TestCase(TweenCollectionMode.Manual)]
        public void ExplicitZeroDelay_OverridesPreset(TweenCollectionMode mode)
        {
            AddTween(mode);
            player.RebuildCache();
            object settingsOwner = mode == TweenCollectionMode.Auto
                ? (object)player.Tweens[0]
                : Get(player.Tweens[0], "overrideSettings");
            Set(settingsOwner, "overrideDelayIn", true);
            Set(settingsOwner, "overrideDelayOut", true);
            AssertTiming(player.Tweens[0], 0f, 0f);
        }

        [Test]
        public void LegacyComponentDelay_IsPreserved_AndNegativeValuesAreClamped()
        {
            AddTween(TweenCollectionMode.Auto);
            player.RebuildCache();
            object tween = player.Tweens[0];
            Set(tween, "delayIn", 0.15f);
            Set(tween, "delayOut", -1f);
            AssertTiming(tween, 0.15f, 0f);
        }

        [Test]
        public void AutoCollection_ExcludesNestedPlayer_AndPrunesDestroyedEffects()
        {
            Transform target = AddTween(TweenCollectionMode.Auto);
            GameObject nested = new GameObject("Nested player");
            nested.transform.SetParent(root.transform);
            nested.AddComponent<UITweenPlayer>();
            nested.AddComponent<UITweenScale>();
            player.RebuildCache();
            Assert.That(player.Tweens.Count, Is.EqualTo(1));
            Object.DestroyImmediate(target.GetComponent<UITweenScale>());
            player.Kill();
            Assert.That(player.Tweens.Count, Is.Zero);
        }

        [UnityTest]
        public IEnumerator RebuildCache_KillsPlaybackOfRemovedManualTarget()
        {
            AddTween(TweenCollectionMode.Manual);
            player.Init();
            UniTask task = player.ShowTween(CancellationToken.None);
            ((List<TweenTargetGroup>)Get(player, "manualTargets")).Clear();
            player.RebuildCache();
            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Succeeded));
            yield return task.ToCoroutine();
            Assert.That(player.Tweens.Count, Is.Zero);
        }

        [TestCase(typeof(UITweenMoveFade), TweenEffectType.MoveFade)]
        [TestCase(typeof(UITweenScaleFade), TweenEffectType.ScaleFade)]
        [TestCase(typeof(UITweenScale), TweenEffectType.Scale)]
        [TestCase(typeof(UITweenFade), TweenEffectType.Fade)]
        [TestCase(typeof(UITweenMove), TweenEffectType.Move)]
        [TestCase(typeof(UITweenRotate), TweenEffectType.Rotate)]
        [TestCase(typeof(UITweenSize), TweenEffectType.Size)]
        public void Reset_AssignsResourcePreset_ForEveryComponent(Type type, TweenEffectType effectType)
        {
            TweenPresetLibrary previous = (TweenPresetLibrary)GetStatic(typeof(TweenPresetLibrary), "cached");
            TweenPresetLibrary library = ScriptableObject.CreateInstance<TweenPresetLibrary>();
            assets.Add(library);
            TweenDefaultPreset mapping = new TweenDefaultPreset();
            Set(mapping, "type", effectType);
            Set(mapping, "preset", preset);
            Set(library, "presets", new List<TweenDefaultPreset> { mapping });
            SetStatic(typeof(TweenPresetLibrary), "cached", library);
            try
            {
                GameObject target = new GameObject("Reset target", typeof(RectTransform));
                target.transform.SetParent(root.transform);
                Component tween = target.AddComponent(type);
                type.GetMethod("Reset", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(tween, null);
                Assert.That(Get(tween, "settings"), Is.SameAs(preset));
                Assert.That(TweenPresetLibrary.Resolve(effectType), Is.SameAs(preset));
            }
            finally { SetStatic(typeof(TweenPresetLibrary), "cached", previous); }
        }

        [TestCase(TweenCollectionMode.Auto, false)]
        [TestCase(TweenCollectionMode.Auto, true)]
        [TestCase(TweenCollectionMode.Manual, false)]
        [TestCase(TweenCollectionMode.Manual, true)]
        public void CombinedFade_AnimatesBothChannels_WithDelayAndInputGating(TweenCollectionMode mode, bool move)
        {
            Transform target = AddCombinedTween(mode, move);
            CanvasGroup group = target.GetComponent<CanvasGroup>();
            player.Init();
            Assert.That(group.alpha, Is.Zero);
            Assert.That(group.blocksRaycasts, Is.False);
            UniTask show = player.ShowTween(CancellationToken.None);
            MakeManual(target);
            DOTween.ManualUpdate(0.39f, 0.39f);
            Assert.That(group.alpha, Is.Zero);
            DOTween.ManualUpdate(0.06f, 0.06f);
            Assert.That(group.alpha, Is.InRange(0.01f, 0.59f));
            if (move)
                Assert.That(((RectTransform)target).anchoredPosition.y, Is.InRange(-99f, -1f));
            else
                Assert.That(target.localScale.x, Is.InRange(0.01f, 1.99f));
            Assert.That(group.interactable, Is.False);
            DOTween.ManualUpdate(0.1f, 0.1f);
            show.GetAwaiter().GetResult();
            Assert.That(group.alpha, Is.EqualTo(0.6f).Within(0.001f));
            Assert.That(group.interactable && group.blocksRaycasts, Is.True);
            Assert.That(target.localScale, Is.EqualTo(Vector3.one * 2f));
            Assert.That(((RectTransform)target).anchoredPosition, Is.EqualTo(Vector2.zero));

            UniTask hide = player.HideTween(CancellationToken.None);
            Assert.That(group.interactable || group.blocksRaycasts, Is.False);
            MakeManual(target);
            DOTween.ManualUpdate(0.29f, 0.29f);
            Assert.That(group.alpha, Is.EqualTo(0.6f).Within(0.001f));
            DOTween.ManualUpdate(0.12f, 0.12f);
            hide.GetAwaiter().GetResult();
            Assert.That(group.alpha, Is.Zero);
            if (move)
                Assert.That(((RectTransform)target).anchoredPosition, Is.EqualTo(new Vector2(0f, -100f)));
            else
                Assert.That(target.localScale, Is.EqualTo(Vector3.zero));
        }

        [TestCase(TweenCollectionMode.Auto, false)]
        [TestCase(TweenCollectionMode.Auto, true)]
        [TestCase(TweenCollectionMode.Manual, false)]
        [TestCase(TweenCollectionMode.Manual, true)]
        public void CombinedFade_KillStopsMotionAndFade_AndSettlesPlayback(TweenCollectionMode mode, bool move)
        {
            Transform target = AddCombinedTween(mode, move);
            player.Init();
            UniTask show = player.ShowTween(CancellationToken.None);
            MakeManual(target);
            DOTween.ManualUpdate(0.45f, 0.45f);
            player.Kill();
            float alpha = target.GetComponent<CanvasGroup>().alpha;
            Vector3 scale = target.localScale;
            Vector2 position = ((RectTransform)target).anchoredPosition;
            DOTween.ManualUpdate(1f, 1f);
            Assert.That(show.Status, Is.EqualTo(UniTaskStatus.Succeeded));
            Assert.That(target.GetComponent<CanvasGroup>().alpha, Is.EqualTo(alpha));
            Assert.That(target.localScale, Is.EqualTo(scale));
            Assert.That(((RectTransform)target).anchoredPosition, Is.EqualTo(position));
            Assert.That(DOTween.IsTweening(target, true), Is.False);
        }

        private Transform AddCombinedTween(TweenCollectionMode mode, bool move)
        {
            Set(player, "collectionMode", mode);
            GameObject target = new GameObject("Combined target", typeof(RectTransform), typeof(CanvasGroup));
            target.transform.SetParent(root.transform);
            target.transform.localScale = Vector3.one * 2f;
            target.GetComponent<CanvasGroup>().alpha = 0.6f;
            if (mode == TweenCollectionMode.Auto)
            {
                Component tween = target.AddComponent(move ? typeof(UITweenMoveFade) : typeof(UITweenScaleFade));
                Set(tween, "settings", preset);
            }
            else
            {
                UITweenDefinition tween = move ? (UITweenDefinition)new MoveFadeTweenEffect() : new ScaleFadeTweenEffect();
                Set(tween, "settings", preset);
                TweenTargetGroup group = new TweenTargetGroup();
                Set(group, "target", target.transform);
                Set(group, "tweens", new List<UITweenDefinition> { tween });
                ((List<TweenTargetGroup>)Get(player, "manualTargets")).Add(group);
            }
            return target.transform;
        }

        private Transform AddTween(TweenCollectionMode mode)
        {
            Set(player, "collectionMode", mode);
            GameObject target = new GameObject("Tween target", typeof(RectTransform));
            target.transform.SetParent(root.transform);
            if (mode == TweenCollectionMode.Auto)
            {
                UITweenScale component = target.AddComponent<UITweenScale>();
                Set(component, "settings", preset);
            }
            else
            {
                ScaleTweenEffect effect = new ScaleTweenEffect();
                Set(effect, "settings", preset);
                TweenTargetGroup group = new TweenTargetGroup();
                Set(group, "target", target.transform);
                Set(group, "tweens", new List<UITweenDefinition> { effect });
                ((List<TweenTargetGroup>)Get(player, "manualTargets")).Add(group);
            }
            return target.transform;
        }

        private static void MakeManual(Transform target)
        {
            List<Tween> tweens = DOTween.TweensByTarget(target);
            Assert.That(tweens, Is.Not.Null);
            foreach (Tween tween in tweens) tween.SetUpdate(UpdateType.Manual);
        }

        private static void AssertTiming(object tween, float show, float hide)
        {
            Type type = tween is UITweenBase ? typeof(UITweenBase) : typeof(UITweenDefinition);
            TweenTimingData timing = (TweenTimingData)type.GetMethod("ResolveTiming", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(tween, null);
            Assert.That(timing.DelayIn, Is.EqualTo(show).Within(0.00001f));
            Assert.That(timing.DelayOut, Is.EqualTo(hide).Within(0.00001f));
        }

        private static FieldInfo Field(Type type, string name)
        {
            while (type != null)
            {
                FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
                if (field != null) return field;
                type = type.BaseType;
            }
            throw new MissingFieldException(name);
        }
        private static object Get(object target, string name) => Field(target.GetType(), name).GetValue(target);
        private static void Set(object target, string name, object value) => Field(target.GetType(), name).SetValue(target, value);
        private static object GetStatic(Type type, string name) => Field(type, name).GetValue(null);
        private static void SetStatic(Type type, string name, object value) => Field(type, name).SetValue(null, value);
    }
}
