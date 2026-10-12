# Copilot Instructions — Tekontee

Before changing gameplay code, read `AI_CONTEXT.md` and `ROADMAP.md`.

## Primary rules

- Work only on the active roadmap milestone unless explicitly told otherwise.
- Inspect existing code before creating new systems.
- Prefer reuse/composition over duplicated Player/Enemy implementations.
- Keep changes small, testable and easy to revert.
- Preserve validated behavior unless the task intentionally changes it.
- Do not perform broad refactors during a feature unless they are required for that feature.
- Avoid speculative architecture for networking, production AI, progression or future content.
- Prefer simple C# and inspector-configurable prototype values.

## Unity-specific rules

- Main gameplay scene: `Assets/_Project/Scenes/ArenaPrototype.unity`.
- Main code: `Assets/_Project/Scripts/`.
- Do not modify Unity `.meta` GUIDs manually.
- Be careful when editing `.unity` YAML directly. Prefer code-only changes when the feature can be wired in the Unity Editor safely.
- If a change requires adding a component, GameObject, reference, layer/tag, collider or inspector value, provide explicit setup instructions.
- Never assume scene references are automatically assigned after adding a script.
- Preserve the current Tamer-mode and Mon-mode camera behavior unless camera changes are part of the task.

## Architecture tendencies

- Morphology should be reusable by PlayerMon and EnemyMon.
- `MorphPointBank` is the current source of Morph Point state.
- `MonDominantFormVisual` handles provisional visual form feedback.
- `MonFormGameplayStats` handles gameplay multipliers derived from the current form.
- Player control switching is coordinated by `PlayerControlManager`.
- Tamer collection currently targets a `MorphPointBank`; enemy collection should use its own target bank when introduced.
- Do not hard-code future player/enemy assumptions into generic morphology or collectible systems if a shared reference/configuration already solves the problem.

## Implementation workflow

For each feature:

1. State the current behavior found in the repository.
2. State the minimal intended behavior change.
3. List files to create/modify.
4. Implement the smallest coherent change.
5. Call out Unity Editor setup separately.
6. Provide concrete manual test steps and expected results.
7. Note risks or assumptions instead of silently guessing.

## Coding style

- Match the existing straightforward MonoBehaviour style.
- Use descriptive public inspector fields with `[Header]` where useful.
- Avoid unnecessary interfaces, dependency-injection frameworks or abstract base hierarchies for prototype-only needs.
- Reuse existing events and components before introducing global managers.
- Keep debug logs useful for prototype verification, but avoid excessive per-frame logging.

## GitBridge

This repository is developed through the AI GitBridge Workflow (GitBridge): gameplay intent is planned in ChatGPT, implementation can be delegated to coding agents, and Unity runtime testing by the human is the acceptance gate.
