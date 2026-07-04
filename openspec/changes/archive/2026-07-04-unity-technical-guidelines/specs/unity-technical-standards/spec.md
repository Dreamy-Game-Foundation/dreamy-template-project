## ADDED Requirements

### Requirement: Coding and Naming Conventions
To maintain clean, readable, and consistent code, all scripts written for the project SHALL conform to the standard C# styling guidelines. Specifically:
- Class, Struct, Interface, Enum, and Public Method/Property names MUST use PascalCase (e.g., `PlayerController`, `InitializeAsync()`).
- Private and Protected fields MUST use camelCase with a leading underscore (e.g., `_saveData`, `_isTransitioning`).
- Local variables and parameters MUST use camelCase (e.g., `targetIndex`, `preloadCount`).
- Interfaces MUST prefix their name with `I` (e.g., `IDatasaveService`).
- Constant fields MUST use PascalCase or UPPER_SNAKE_CASE.

#### Scenario: Script code convention inspection
- **WHEN** a new code file is analyzed or committed
- **THEN** the naming conventions for classes, interfaces, properties, and private variables MUST match the PascalCase, prefix-I, and leading-underscore camelCase formatting rules.

### Requirement: SOLID Principles Application
All developers SHALL adhere to SOLID principles to maintain high modularity and prevent the classic Unity "Massive MonoBehaviour" anti-pattern:
- **Single Responsibility Principle (SRP)**: Monobehaviours MUST focus strictly on visual presentation, user input detection, or simple component lifecycle hooks. Complex logic, data calculations, and network interactions MUST be delegated to non-MonoBehaviour service classes.
- **Open/Closed Principle (OCP)**: Systems SHALL be open for extension but closed for modification. Developers SHALL use base interfaces and polymorphism to extend logic rather than editing core scripts directly.
- **Liskov Substitution Principle (LSP)**: Derived classes MUST be substitutable for their base types without altering system correctness.
- **Interface Segregation Principle (ISP)**: Interfaces MUST be small, cohesive, and feature-specific. Developers SHALL avoid creating large, multi-purpose interfaces.
- **Dependency Inversion Principle (DIP)**: High-level modules and UI controllers SHALL NOT depend on low-level concrete scripts. They MUST resolve their dependencies via abstractions using the `ServiceLocator`.

#### Scenario: Service resolution via ServiceLocator
- **WHEN** a class (like `FoundationDemoRoot`) requires a dependency (such as `IDatasaveService`)
- **THEN** it SHALL resolve it via `ServiceLocator.Get<T>()` and rely on its interface, preventing direct instantiation or coupling to a concrete implementation.

### Requirement: Object-Oriented Programming (OOP) and Reusability
The project architecture SHALL prioritize composition over inheritance for MonoBehaviour script layout.
- ScriptableObjects MUST be utilized for static read-only data, configuration parameters, and shared assets to reduce runtime memory overhead.
- Features and distinct modules (e.g., Core, UI, Save, Config) MUST be encapsulated in their own Assembly Definitions (`.asmdef`) to ensure strict compile-time isolation and reduce compilation times.
- Monobehaviours SHALL use `[SerializeField] private` fields instead of `public` fields for Inspector reference assignment, preserving encapsulation.

#### Scenario: Assembly references checking
- **WHEN** compiling code across package domains
- **THEN** references MUST run in one direction from high-level features/UI to low-level core libraries, enforced by assembly definitions to prevent cyclic dependencies.

### Requirement: Event-Driven Communication
Components and systems SHALL interact using event-driven or reactive bindings rather than polling or direct tight coupling.
- The project SHALL use `BindableProperty<T>` for local state synchronizations (e.g., health, score, coin count).
- The project SHALL use `MyEventBus<T>` or C# `event` delegates for globally distributed events or decoupled notification cycles.

#### Scenario: UI update via event bindings
- **WHEN** player health is updated inside a gameplay service
- **THEN** the health UI panel SHALL receive the new value automatically via `BindableProperty<T>.OnValueChanged` callback, without needing an `Update()` loop check.
