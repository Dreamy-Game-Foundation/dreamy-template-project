# Dreamy Template Project - Technical & Architectural Guidelines

This document outlines the coding standards, architectural workflows, and best practices for Unity developers working in internal Dreamy games. It acts as the technical manual to ensure code quality, testability, reusability, and clean extensibility.

---

## 1. Coding & Naming Conventions

To keep the codebase consistent and readable, all C# scripts MUST adhere to the following standards:

### Naming Conventions

| Language Element | Case Style | Example | Notes |
| :--- | :--- | :--- | :--- |
| **Classes, Structs, Enums** | PascalCase | `PlayerController`, `GameState` | Cohesive, noun-based names. |
| **Interfaces** | PascalCase | `IDatasaveService`, `IDataConfig` | MUST begin with a capital `I`. |
| **Public Methods & Properties**| PascalCase | `InitializeAsync()`, `CurrentScore` | Methods should use action verbs. |
| **Private & Protected Fields** | `_camelCase` | `_saveData`, `_isTransitioning` | MUST start with a leading underscore. |
| **Local Variables & Parameters**| camelCase | `targetPosition`, `preloadCount` | Keep names descriptive. |
| **Constants & Static Readonly**| PascalCase | `MaxPlayerHealth`, `GravityConstant` | Or `UPPER_SNAKE_CASE` if standard. |

### Unity-Specific Formatting Rules
* **Encapsulation of Inspector Fields**: DO NOT use public fields for Inspector variables. Use `[SerializeField] private` to expose fields to the Unity Editor while keeping them private:
  ```csharp
  [SerializeField] private Button _submitButton; // Correct
  public Button SubmitButton; // Avoid unless absolutely necessary
  ```
* **Namespace Organization**: Every script MUST reside within a proper namespace matching its folder location:
  ```csharp
  namespace Dreamy.Template.UI
  {
      public class MainMenuPanel : MonoBehaviour { }
  }
  ```
* **Attribute Usage**: Use attributes like `[DisallowMultipleComponent]`, `[RequireComponent(typeof(T))]`, and `[Header("...")]` to safeguard component integrity in the Editor.

---

## 2. SOLID Principles in Unity Context

Applying SOLID principles to Unity prevents the common "Massive MonoBehaviour" anti-pattern and maintains a highly modular architecture.

### 2.1 Single Responsibility Principle (SRP)
* **Rule**: A MonoBehaviour script should only handle **one** of three things: rendering/UI updates, detecting user inputs, or simple physical/collision responses. 
* **Implementation**: Keep business rules, calculations, save serialization, and backend network operations out of MonoBehaviours. Move them into pure C# classes or services registered with the `ServiceLocator`.

### 2.2 Open/Closed Principle (OCP)
* **Rule**: Classes/modules should be open for extension but closed for modification.
* **Implementation**: Rather than altering a core gameplay script to add new behavior, define interfaces or use virtual methods. For instance, define an `IDamageable` interface:
  ```csharp
  public interface IDamageable
  {
      void TakeDamage(float amount);
  }
  ```
  This allows adding new enemies or destructible objects without changing the combat controller script.

### 2.3 Liskov Substitution Principle (LSP)
* **Rule**: Subclasses must be completely substitutable for their parent classes/interfaces.
* **Implementation**: Avoid overriding a method to throw `NotImplementedException`. If a subclass cannot implement a behavior, the interface should be split (see ISP).

### 2.4 Interface Segregation Principle (ISP)
* **Rule**: Large interfaces should be broken down into smaller, highly focused ones.
* **Implementation**: Do not create a single `IGameService` with fifty unrelated methods. Split it into focused interfaces like `IDatasaveService`, `IDataConfigService`, and `IPoolService`.

### 2.5 Dependency Inversion Principle (DIP)
* **Rule**: High-level modules must depend on abstractions (interfaces), not concrete classes.
* **Implementation**: High-level controllers (e.g. `FoundationDemoRoot`) must not instantiate concrete services. Instead, resolve dependencies from the `ServiceLocator` using interfaces:
  ```csharp
  // Depend on interface IDatasaveService, not concrete DatasaveService
  IDatasaveService datasave = ServiceLocator.Get<IDatasaveService>();
  ```

---

## 3. Core Design Patterns

Standardizing on core patterns reduces architecture ambiguity and makes code easy to understand.

### 3.1 Model-View-Presenter (MVP) for UI
We utilize the **MVP Pattern with a Passive View** for all user interface structures:

```text
               ┌───────────────┐
               │     Model     │ (Config / Service / SaveData)
               └───────┬───────┘
                       │ notifies (via BindableProperty / events)
                       ▼
               ┌───────────────┐
               │   Presenter   │ (Pure C# class - binds & coordinates)
               └───────┬───────┘
                 ▲           │
  user inputs    │           │ updates visuals
  (events/clicks)│           ▼
               ┌───────┴───────┐
               │     View      │ (MonoBehaviour - UI widgets / Text / Button)
               └───────────────┘
```

#### Guidelines:
* **The View (MonoBehaviour)**: Owns references to UnityEngine UI elements (e.g. `TextMeshProUGUI`, `Button`, `Slider`). It registers callbacks to button clicks and triggers C# events. It has no business logic.
* **The Presenter (Pure C#)**: Listens to UI events from the View, calls business logic or services (Model), listens to data changes, and calls public methods on the View to update visuals.
* **Lifecycle Binding**: Always subscribe to events when the panel/view is created/activated, and **unsubscribe** when it is deactivated/destroyed to avoid memory leaks:
  ```csharp
  // Inside UI Controller/Presenter Bind/Unbind methods:
  private void BindPanel(FoundationDemoPanel panel)
  {
      panel.AddScoreRequested += AddScore;
      panel.Destroyed += OnPanelDestroyed;
  }
  
  private void UnbindPanel(FoundationDemoPanel panel)
  {
      if (panel == null) return;
      panel.AddScoreRequested -= AddScore;
      panel.Destroyed -= OnPanelDestroyed;
  }
  ```

### 3.2 State Pattern for Complex Workflows
Avoid massive enum switch-cases for game states, AI, or tutorial flows. Use the State Pattern:
* Define an `IState` interface with `Enter()`, `Update()`, and `Exit()` methods.
* A `StateMachine` class controls transitions and delegates updates to the current active state class.
* Each state is isolated in its own file (e.g., `MainMenuState`, `GameplayState`, `PauseState`), satisfying SRP.

### 3.3 Event-Driven Communications (BindableProperty & EventBus)
* Use `BindableProperty<T>` for state binding (e.g., player score, coins, health). Presenters subscribe to `OnValueChanged` callbacks to reactively update the View.
* Use `MyEventBus<T>` for globally broadcasted events (e.g., `LevelCompletedEvent`, `PlayerSpawnedEvent`) where sender and receiver are completely decoupled.

---

## 4. Reusability & Extensibility

### 4.1 ScriptableObjects as Authoring Catalogs
* ScriptableObjects MUST be used to specify static data configurations, asset catalogs (Addressable keys), and scene references.
* Keep ScriptableObjects read-only at runtime. Never write game save progress into ScriptableObjects; use the `IDatasaveService` and `SaveData` structures instead.

### 4.2 Assembly Definitions (.asmdef)
To reduce compilation times and enforce strict modular boundary layers, organize the codebase using Assembly Definitions:
* **Domain Assembly**: Contains pure C# logic, models, and interfaces. No references to Unity libraries.
* **Application/Services Assembly**: Contains business use cases, service implementations, and adapters.
* **UI/Presentation Assembly**: Contains views, UI panels, and presenters.
* **Bootstrap Assembly**: The composition root that registers services and starts the app.

Reference cycles are prevented because dependencies compile in a single direction:
`Bootstrap -> UI/Presentation -> Application -> Domain`.

### 4.3 Dependency Propagation Rules
* **Global Services**: Resolve only major top-level manager/controller dependencies via the static `ServiceLocator`.
* **Local Propagation**: Low-level prefabs or entities (e.g., spawnable targets, bullets, item pickups) MUST NOT call `ServiceLocator.Get<T>()`. Pass dependencies down explicitly via constructor parameters, factories, or initialization methods (e.g., `bullet.Initialize(damageAmount)`).
