using System;
using Dreamy.UI;

internal static class BindingHarness
{
    private sealed class TestView { }
    private sealed class Presenter : IPanelPresenter
    {
        public int Shows, Disposals;
        public bool FailShow, FailDispose;
        public void Show() { Shows++; if (FailShow) throw new InvalidOperationException(); }
        public void Dispose() { Disposals++; if (FailDispose) throw new InvalidOperationException(); }
    }
    private static void Check(bool condition, string label)
    { if (!condition) throw new Exception(label); Console.WriteLine("PASS " + label); }

    public static void Run()
    {
        var factory = new PanelPresenterFactory();
        Check(factory.Create(new TestView()) == null, "unregistered view needs no presenter");
        Presenter current = null;
        int created = 0;
        factory.Register<TestView>(_ => { created++; return current = new Presenter(); });
        var host = new PanelPresenterHost(factory, new TestView());
        host.Show(); host.Show();
        Check(created == 1 && current.Shows == 1, "repeated show creates and binds once");
        var first = current;
        host.Dispose(); host.Dispose();
        Check(first.Disposals == 1, "hide and disable dispose only once");
        host.Show();
        Check(created == 2 && current != first, "cached reopen creates a fresh presenter");
        host.Dispose();
        Check(current.Disposals == 1, "destroy releases reopened presenter");
        bool duplicate = false;
        try { factory.Register<TestView>(_ => new Presenter()); }
        catch (InvalidOperationException) { duplicate = true; }
        Check(duplicate, "duplicate registration fails explicitly");

        var failingFactory = new PanelPresenterFactory();
        var failed = new Presenter { FailShow = true };
        failingFactory.Register<TestView>(_ => failed);
        var retry = new PanelPresenterHost(failingFactory, new TestView());
        try { retry.Show(); } catch (InvalidOperationException) { }
        Check(failed.Disposals == 1, "failed show rolls back subscriptions");
        failed.FailShow = false;
        retry.Show(); retry.Dispose();
        Check(failed.Shows == 2 && failed.Disposals == 2, "failed show can retry");

        var nullFactory = new PanelPresenterFactory();
        nullFactory.Register<TestView>(_ => null);
        bool rejectedNull = false;
        try { nullFactory.Create(new TestView()); } catch (InvalidOperationException) { rejectedNull = true; }
        Check(rejectedNull, "registered factory cannot silently return null");

        var throwingFactory = new PanelPresenterFactory();
        var failingDispose = new Presenter { FailDispose = true };
        throwingFactory.Register<TestView>(_ => failingDispose);
        var throwing = new PanelPresenterHost(throwingFactory, new TestView());
        throwing.Show();
        try { throwing.Dispose(); } catch (InvalidOperationException) { }
        throwing.Dispose();
        Check(failingDispose.Disposals == 1, "failed dispose still clears host ownership");
    }
}
