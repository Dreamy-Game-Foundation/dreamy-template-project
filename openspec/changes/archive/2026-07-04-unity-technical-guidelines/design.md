## Context

The Dreamy Template Project is a Unity starter project containing git-based Dreamy packages (Core, DataSave, DataConfig, UI, Assets, Editor Tools). It currently includes a simple demo of service registration (`GameInstaller`), saving data (`TemplateSave`), and loading configuration (`TemplateConfig`).

Without explicit rules, architectural patterns, and coding workflows, team members can end up creating disjointed scripts, direct monolithic couplings, and script execution order bugs. A formal, senior-level design standard is required to establish how engineers should build UI, state flows, configurations, and core gameplay features using SOLID principles and OOP.

## Goals / Non-Goals

**Goals:**
- Define a strict architectural pattern for UI separation (Model-View-Presenter).
- Standardize on event-driven state binding using the provided `BindableProperty<T>` and `MyEventBus<T>`.
- Outline a guidelines framework for extending and customizing the Dreamy Core package services.
- Establish rules for Assembly Definitions (`.asmdef`) file structures and dependency graphs.
- Establish standard design patterns (State, Factory, Command) to handle common game loop situations.

**Non-Goals:**
- Implementing a full-blown gameplay system or custom gameplay logic.
- Replacing the existing `ServiceLocator` or `Datasave` frameworks with external libraries (e.g., Extenject/Zenject, EasySave).
- Creating automated test suites (e.g., Unity Test Framework tests) as part of this guidelines proposal.

## Decisions

### Decision 1: UI Architecture – MVP (Model-View-Presenter) with Passive View
- **Approach**: All UI systems SHALL implement the MVP pattern. The `MonoBehaviour` script attached to the prefab acts as the **View** (Passive View). It exposes UI widgets (buttons, text) and invokes events when user interactions occur. A plain C# class serves as the **Presenter**, which binds to gameplay services (the **Model**), listens to input events from the View, and updates the View's visuals.
- **Rationale**: Keeps UI views easily testable, decouples layout mechanics (Unity text/images) from game rules, and makes UI components easily refactorable or swappable.
- **Alternatives Considered**: 
  - *MVC*: Difficult in Unity since the View and Controller are often fused in MonoBehaviours.
  - *MVVM*: Requires a heavy data-binding system (e.g., Unity UI Toolkit Data Binding or custom reactive binders), which adds unnecessary overhead.

### Decision 2: Service Locator vs. DI Framework
- **Approach**: Continue using the lightweight `ServiceLocator` provided in `Dreamy.Core` instead of importing Zenject/Extenject or VContainer.
- **Rationale**: The template project already has a functional, zero-allocation `ServiceLocator` that compiles fast. A full-blown DI framework introduces external dependencies, learning overhead, and increases initialization times.
- **Alternatives Considered**:
  - *Zenject/Extenject*: Rejected due to codebase complexity, reflection performance costs, and learning curve.
  - *VContainer*: Lightweight alternative, but since `ServiceLocator` is already embedded and sufficient, standardizing on it keeps things simple.

### Decision 3: Modular State Management via State Pattern
- **Approach**: For stateful behaviors (e.g., Game Loop, Enemy AI, Tutorial Flow), developers MUST use the **State Pattern** with distinct class-based states representing states (e.g., `MainMenuState`, `GameplayState`, `PauseState`), instead of massive switch-case statements in a manager.
- **Rationale**: Switch-cases scale poorly and violate the Single Responsibility and Open/Closed principles. The State Pattern separates state transition logic and state execution logic.

### Decision 4: Event-Driven UI Sync using BindableProperty
- **Approach**: UI elements MUST register callbacks to `BindableProperty<T>.OnValueChanged` during panel showing/initialization and deregister during hiding/destruction.
- **Rationale**: Eliminates the need for polling logic in `Update()` loops, saving CPU cycles and ensuring immediate, reactive UI refreshes.

## Risks / Trade-offs

### Risk 1: Memory Leaks from Unsubscribed Events
- **Description**: Developers might subscribe to `BindableProperty.OnValueChanged` or `MyEventBus` in UI Panels/Views and forget to unsubscribe, preventing GC from cleaning up destroyed objects.
- **Mitigation**: Standardize on the UI Panel lifecycle: always unsubscribe inside the UI's `OnDestroy` or when the panel is closed/hidden (`UnbindPanel`).

### Risk 2: Service Locator Bloat (Service Abuse)
- **Description**: Accessing `ServiceLocator.Get<T>()` deep inside lower-level entities/prefabs (e.g., a bullet prefab resolving `IDamageService` on spawn) leads to hidden dependencies and untestable code.
- **Mitigation**: Lower-level game objects and prefabs MUST receive their dependencies passed via constructor or initialization parameters (e.g., `Initialize(IDamageService damageService)`) rather than fetching them directly from the `ServiceLocator`.
