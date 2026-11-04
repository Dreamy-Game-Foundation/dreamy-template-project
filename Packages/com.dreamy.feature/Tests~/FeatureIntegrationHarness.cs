using System;
using System.Linq;
using System.Reflection;
using Dreamy.UI;
using NUnit.Framework;
using Dreamy.Progression;

internal static class FeatureIntegrationHarness
{
    private sealed class View { }
    private sealed class Ticked : ITickedPanelPresenter
    {
        public int Shows, Ticks, Disposals;
        public void Show() => Shows++;
        public void Tick() => Ticks++;
        public void Dispose() => Disposals++;
    }
    private sealed class ProgressService : IProgressionService
    {
        public event Action<ProgressionState> ProgressChanged;
        public int Grants;
        public ProgressionState GetState() => default;
        public ProgressionAdvanceResult AdvanceNext(string id) { Grants++; return default; }
        public ProgressionAdvanceResult GrantExperience(string id, long amount) { Grants++; return default; }
        public void Notify() => ProgressChanged?.Invoke(default);
    }
    private sealed class ProgressView : IProgressionView
    {
        public int Renders;
        public void Render(ProgressionState state) => Renders++;
    }
    public static void Main()
    {
        var factory = new PanelPresenterFactory();
        Ticked current = null;
        factory.Register<View>(_ => current = new Ticked());
        var host = new PanelPresenterHost(factory, new View());
        host.Show(); host.Show(); host.Tick();
        Assert.That(current.Shows, Is.EqualTo(1));
        Assert.That(current.Ticks, Is.EqualTo(1));
        Console.WriteLine("PASS ticking host binds once and forwards tick");
        var first = current;
        host.Dispose(); host.Tick();
        Assert.That(first.Disposals, Is.EqualTo(1));
        Assert.That(first.Ticks, Is.EqualTo(1));
        Console.WriteLine("PASS disposed host no longer ticks the presenter");
        host.Show(); host.Tick();
        Assert.That(current, Is.Not.SameAs(first));
        Assert.That(current.Ticks, Is.EqualTo(1));
        Console.WriteLine("PASS reopening host creates a fresh ticking presenter");
        host.Dispose();

        var progressService = new ProgressService();
        var progressView = new ProgressView();
        var progressFactory = new PanelPresenterFactory();
        progressFactory.Register<ProgressView>(view => new ProgressionPresenter(progressService, view));
        var progressHost = new PanelPresenterHost(progressFactory, progressView);
        progressHost.Show(); progressHost.Show(); progressService.Notify();
        Assert.That(progressView.Renders, Is.EqualTo(2));
        Console.WriteLine("PASS progression factory subscribes once and renders service notifications");
        progressHost.Dispose(); progressService.Notify();
        Assert.That(progressView.Renders, Is.EqualTo(2));
        Console.WriteLine("PASS progression presenter unsubscribes when HUD closes");
        progressHost.Show(); progressService.Notify();
        Assert.That(progressView.Renders, Is.EqualTo(4));
        Assert.That(progressService.Grants, Is.EqualTo(0));
        Console.WriteLine("PASS progression reopen rebinds without granting XP or rewards");
        progressHost.Dispose();

        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            foreach (var method in type.GetMethods())
            {
                var attributes = method.GetCustomAttributes(false);
                var cases = attributes.OfType<TestCaseAttribute>().ToArray();
                if (cases.Length == 0 && !attributes.OfType<TestAttribute>().Any()) continue;
                var inputs = cases.Length > 0 ? cases.Select(c => c.Arguments).ToArray() : new[] { Array.Empty<object>() };
                foreach (var input in inputs)
                {
                    var fixture = Activator.CreateInstance(type);
                    var setup = type.GetMethods().Where(m => m.GetCustomAttributes(typeof(SetUpAttribute), true).Length > 0);
                    var teardown = type.GetMethods().Where(m => m.GetCustomAttributes(typeof(TearDownAttribute), true).Length > 0);
                    foreach (var action in setup) action.Invoke(fixture, null);
                    try { method.Invoke(fixture, input); }
                    finally { foreach (var action in teardown) action.Invoke(fixture, null); }
                    Console.WriteLine("PASS " + type.Name + "." + method.Name + (input.Length > 0 ? "(" + string.Join(",", input) + ")" : ""));
                }
            }
        }
    }
}
