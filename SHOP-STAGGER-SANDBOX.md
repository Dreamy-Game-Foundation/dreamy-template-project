# Shop stagger handoff

Target: `D:/Game/dreamy-base-project/DreamyTemplateProject`.

## Applied in this project

- HomeNavigator creates the hidden shop, binds its presenter and renders offers before transitioning to it. PanelManager.Create reuses the registered panel during Transition.
- ShopPanel removes surplus items from the hierarchy, rebuilds the offer layout, applies item delays and refreshes the tween cache when the offer count changes.
- Ordinary purchase refreshes with the same count only update content; they do not replay the opening animation.

## Verify in the sandbox / Unity Editor

1. ShopOfferItem must inherit or contain an enabled UITweenBase effect (for example scale or fade) with auto-run enabled. TweenDelayByIndex alone does not animate anything; UIScalable is button feedback, not an opening tween.
2. The panel UITweenPlayer must use Auto collection and own the item effects. A nested UITweenPlayer forms a separate ownership boundary.
3. TweenDelayControl should cover the offer container and use a visible interval (start with 0.08 seconds). Use item slots for ordering; avoid adding another stagger delay in a different player implementation.
4. Confirm the prefab used by Address.ShopPanel and rebuild Addressables if it loads an old built prefab.
5. Run first open, close/reopen, purchase refresh, zero offers, count growth/shrink and close during animation. Check Console and actual item start times.

## Dreamy UI package follow-up

No package change is required for the initial-open ordering fix with the installed APIs.

- Consider a preparation callback overload for PanelManager.Transition, invoked after Create but before Show under the transition lock. This makes preparation atomic for other callers and avoids a separate Create/Transition pair.
- UITweenPlayer only invalidates its cache on its own direct child changes. Add an explicit supported refresh contract or descendant registration for dynamic lists, preserving nested-player ownership.
- RebuildCache currently kills cached tweens. Define and test behavior for list changes during an active transition; the current project fix does not guarantee animation for newly inserted items while the panel is already open.
- Test inactive dynamically spawned slots, per-item delay across multiple effects, nested players, and destroyed targets.

## Validation

CLI compile: `dotnet build Dreamy.Template.Runtime.csproj --verbosity quiet` succeeded with zero errors and two existing obsolete API warnings in CW.Common. The initial no-restore attempt was blocked by missing generated project.assets.json; restore resolved it. Unity Editor compile and Play Mode remain unverified; there is no Unity Editor bridge in this session.
