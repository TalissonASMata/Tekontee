# Tekontee — Prototype Roadmap

This roadmap tracks prototype validation milestones. Keep each version small enough to test independently in Unity.

## Validated baseline

| Version | Milestone | Status |
|---|---|---|
| V0.x → V1.0 | Core playable loop: combat, Tamer/Mon switching, Morph Points, collection, arguments, form gameplay effects and battle loop | Validated incrementally |
| V1.1 | Camera by control mode: strategic Tamer camera + fighting-game Mon camera | Validated |

## Active roadmap

| Version | Milestone | Validation target |
|---|---|---|
| V1.2 | EnemyTamer | Add an opponent Tamer that acts in the platform/arena layer and creates real competition for available Morph Point resources. Reuse player Tamer systems where practical; avoid duplicating movement/collection rules unnecessarily. |
| V1.3 | EnemyMon Morphology | EnemyMon gains and uses Morph Points and can change dominant form/gameplay effects using shared morphology systems. Player must adapt to an opponent whose capabilities change during the battle. |
| V1.4 | Morph concentration/proportion | Replace or extend simple raw dominant totals with morphology based on point concentration/proportion. Validate whether composition produces more dynamic and readable form changes. |
| V1.5 | Tamer ↔ arena expansion interactions | Validate interactions where the Tamer can change/expand the arena, generate new objects/platforms or create/potentiate point opportunities. Keep scope deliberately small. |
| V1.6 | Competitive Tamer behavior | Improve EnemyTamer decisions so it contests valuable resources rather than only wandering/collecting opportunistically. |
| V1.7 | Pair interaction | Validate clearer tactical interdependence between Tamer actions and Mon combat outcomes. |

## Future concepts — not active unless promoted to a milestone

- Four-player / 2 vs 2 final match structure.
- More complete move presets (2 moves + 1 special before battle).
- Special move combinations.
- Music/rhythm layer where correctly timed moves generate different Morph Point types according to synchronized instruments.
- Larger arena evolution / spatial expansion systems.
- Production AI, networking, progression, narrative, content pipeline and art polish.

## Scope rule

Do not implement future concepts during an earlier milestone unless they are strictly required to validate that milestone. Prefer a working, inspectable prototype over anticipatory architecture.
