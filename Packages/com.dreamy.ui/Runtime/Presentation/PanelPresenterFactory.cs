using System;
using System.Collections.Generic;

namespace Dreamy.UI
{
    /// <summary>Register presenter constructors once in the composition root.</summary>
    public sealed class PanelPresenterFactory
    {
        private readonly Dictionary<Type, Func<object, IPanelPresenter>> factories =
            new Dictionary<Type, Func<object, IPanelPresenter>>();

        public void Register<TView>(Func<TView, IPanelPresenter> create) where TView : class
        {
            if (create == null) throw new ArgumentNullException(nameof(create));
            if (factories.ContainsKey(typeof(TView)))
                throw new InvalidOperationException($"Presenter already registered for {typeof(TView).Name}.");
            factories.Add(typeof(TView), view => create((TView)view));
        }

        /// <summary>Unregistered views work without a presenter.</summary>
        public IPanelPresenter Create(object view)
        {
            if (view == null) throw new ArgumentNullException(nameof(view));
            if (!factories.TryGetValue(view.GetType(), out var create)) return null;
            return create(view) ?? throw new InvalidOperationException(
                $"Presenter factory for {view.GetType().Name} returned null.");
        }
    }
}
