using System;
using System.Reflection;
using Dreamy.Core;
using NUnit.Framework;
using UnityEngine;

namespace Dreamy.Template.Tests
{
    public sealed class InstallerOwnershipTests
    {
        private GameObject root;
        private GameInstaller installer;

        private interface IFixtureService { }
        private sealed class FixtureService : IFixtureService, IDisposable
        {
            public int DisposeCount { get; private set; }
            public void Dispose() => DisposeCount++;
        }

        [SetUp]
        public void SetUp()
        {
            // Keep the fixture inactive so Awake cannot start the real bootstrap.
            root = new GameObject("Installer ownership fixture");
            root.SetActive(false);
            installer = root.AddComponent<GameInstaller>();
        }

        [TearDown]
        public void TearDown()
        {
            TearDownServices();
            ServiceLocator.Unregister<IFixtureService>();
            UnityEngine.Object.DestroyImmediate(root);
        }

        [Test]
        public void TearDown_DoesNotUnregisterAReplacement_AndDisposesOwnedInstanceOnce()
        {
            var owned = new FixtureService();
            var replacement = new FixtureService();
            Register(owned, ownsInstance: true);
            ServiceLocator.Register<IFixtureService>(replacement);

            TearDownServices();
            TearDownServices();

            Assert.That(ServiceLocator.Get<IFixtureService>(), Is.SameAs(replacement));
            Assert.That(owned.DisposeCount, Is.EqualTo(1));
            Assert.That(replacement.DisposeCount, Is.Zero);
        }

        [Test]
        public void TearDown_RestoresThePreviousRegistration()
        {
            var previous = new FixtureService();
            var owned = new FixtureService();
            ServiceLocator.Register<IFixtureService>(previous);
            Register(owned, ownsInstance: true);

            TearDownServices();

            Assert.That(ServiceLocator.Get<IFixtureService>(), Is.SameAs(previous));
            Assert.That(owned.DisposeCount, Is.EqualTo(1));
            Assert.That(previous.DisposeCount, Is.Zero);
        }

        [Test]
        public void TearDown_UnregistersButDoesNotDisposeABorrowedService()
        {
            var borrowed = new FixtureService();
            Register(borrowed, ownsInstance: false);

            TearDownServices();

            Assert.That(ServiceLocator.IsRegistered<IFixtureService>(), Is.False);
            Assert.That(borrowed.DisposeCount, Is.Zero);
        }

        private void Register(IFixtureService service, bool ownsInstance)
        {
            typeof(GameInstaller).GetMethod("RegisterService", BindingFlags.Instance | BindingFlags.NonPublic)
                .MakeGenericMethod(typeof(IFixtureService)).Invoke(installer, new object[] { service, ownsInstance });
        }

        private void TearDownServices()
        {
            typeof(GameInstaller).GetMethod("TearDownServices", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(installer, null);
        }
    }
}
