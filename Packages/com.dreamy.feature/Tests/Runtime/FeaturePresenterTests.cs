using System;
using NUnit.Framework;

namespace Dreamy.Feature.Tests
{
    public sealed class FeaturePresenterTests
    {
        [Test]
        public void RepeatedShowAndDispose_KeepOneSubscription_AndAllowReopen()
        {
            var source = new Source();
            var presenter = new Presenter(source);
            IFeaturePresenter contract = presenter;
            contract.Show();
            contract.Show();
            Assert.That(source.ListenerCount, Is.EqualTo(1));
            Assert.That(presenter.Renders, Is.EqualTo(2));
            source.Raise();
            Assert.That(presenter.Renders, Is.EqualTo(3));
            presenter.Dispose();
            presenter.Dispose();
            source.Raise();
            Assert.That(source.ListenerCount, Is.Zero);
            Assert.That(presenter.Renders, Is.EqualTo(3));
            Assert.That(presenter.Unbinds, Is.EqualTo(1));
            contract.Show();
            Assert.That(source.ListenerCount, Is.EqualTo(1));
            Assert.That(presenter.Renders, Is.EqualTo(4));
            presenter.Dispose();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void FailedShow_ReleasesPartialBinding_AndCanRetry(bool failBinding)
        {
            var source = new Source();
            var presenter = new Presenter(source)
            {
                FailBinding = failBinding,
                FailRefresh = !failBinding
            };
            Assert.Throws<InvalidOperationException>(() => presenter.Show());
            Assert.That(source.ListenerCount, Is.Zero);
            Assert.That(presenter.Bound, Is.False);
            presenter.FailBinding = false;
            presenter.FailRefresh = false;
            presenter.Show();
            Assert.That(source.ListenerCount, Is.EqualTo(1));
            presenter.Dispose();
            Assert.That(source.ListenerCount, Is.Zero);
        }

        [Test]
        public void FailedRefreshOnRepeatedShow_AlsoReleasesExistingBinding()
        {
            var source = new Source();
            var presenter = new Presenter(source);
            presenter.Show();
            presenter.FailRefresh = true;
            Assert.Throws<InvalidOperationException>(() => presenter.Show());
            Assert.That(source.ListenerCount, Is.Zero);
            Assert.That(presenter.Unbinds, Is.EqualTo(1));
        }

        private sealed class Source
        {
            public event Action Changed;
            public int ListenerCount => Changed?.GetInvocationList().Length ?? 0;
            public void Raise() => Changed?.Invoke();
        }

        private sealed class Presenter : FeaturePresenter
        {
            private readonly Source source;
            public Presenter(Source source) => this.source = source;
            public int Renders { get; private set; }
            public int Unbinds { get; private set; }
            public bool FailBinding { get; set; }
            public bool FailRefresh { get; set; }
            public bool Bound => IsBound;

            protected override void Bind()
            {
                source.Changed += Refresh;
                if (FailBinding) throw new InvalidOperationException("Injected bind failure.");
            }

            public override void Refresh()
            {
                if (FailRefresh) throw new InvalidOperationException("Injected render failure.");
                Renders++;
            }

            protected override void Unbind()
            {
                source.Changed -= Refresh;
                Unbinds++;
            }
        }
    }
}
