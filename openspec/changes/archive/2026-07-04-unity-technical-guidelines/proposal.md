## Why

Unity projects easily degenerate into spaghetti code and tight coupling if clear technical standards, SOLID principles, and design patterns are not established early. This proposal establishes a comprehensive technical workflow and coding standard framework to ensure the Dreamy Template Project remains clean, modular, testable, and highly extensible as a foundation for multiple internal games.

## What Changes

- Establish C# coding and style conventions tailored specifically for Unity development.
- Outline rules for applying SOLID and OOP principles to Unity-centric systems (avoiding pitfalls like Monobehaviour bloating and tight scene dependencies).
- Standardize on core design patterns (Service Locator, Event Bus, BindableProperty, State Pattern, Factory) with clear usage guidelines.
- Define architecture guidelines for reusability (ScriptableObjects, Assembly Definitions, clean decoupled component designs).
- Formulate a clear developer workflow for extending the Dreamy foundation packages (Core, DataSave, DataConfig, UI, Assets).

## Capabilities

### New Capabilities

- `unity-technical-standards`: The formal specification documenting coding standards, Unity-specific SOLID/OOP guidelines, reusable design patterns, and structural workflows.

### Modified Capabilities

<!-- None -->

## Impact

- Applies to all future scripts, prefabs, scene designs, and feature architectures added to the template.
- Serves as the ultimate source of truth for code quality, PR reviews, and AI-driven code generation.
