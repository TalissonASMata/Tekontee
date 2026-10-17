# Tekontee — Prototype Roadmap

This roadmap tracks prototype validation milestones. Keep each version small enough to test independently in Unity.

## Validated baseline

| Version | Milestone | Status |
|---|---|---|
| V0.x → V1.0 | Core playable loop: combat, Tamer/Mon switching, Morph Points, collection, arguments, form gameplay effects and battle loop | Validated incrementally |
| V1.1 | Camera by control mode: strategic Tamer camera + fighting-game Mon camera | Validated |
| V1.2 | EnemyTamer | Validated |
| V1.3 | EnemyMon Morphology | Validated |
| V1.4 | Weighted Morph Influence | Validated |
| V1.5 | Move Morphology Requirements | Validated in Unity: Fail / Effective / Critical, damage/reinforcement multipliers, requirements independent from dominant form (Attack versus Air) |

## Active roadmap

| Version | Milestone | Validation target |
|---|---|---|
| V1.6 | Universal Moveset | All Mons share universal moves; morphology determines Fail / Effective / Critical rather than availability. Validate two different moves, manual/automatic selection, and shared keyboard + modern gamepad (Xbox / PS5) input via Unity Input System actions; every move requires at most two simultaneous buttons (single press or two-button chord), with no mandatory complex input sequences. |
| V1.7 | Anatomical Morphology | Replace the temporary four-axis model with the real multidimensional anatomical model: Head, Torso, Arms, Legs, Tail and Element, totaling approximately 18 parameters. |
| V1.8 | Full Morphology Feedback Loop | Validate the complete cycle: morphology -> move quality -> weighted reinforcement -> changed morphology -> changed future move quality. |
| V1.9 | Special Techniques | Add special techniques that require more advanced combinations of morphology parameters so they cannot be trivially spammed to feed themselves indefinitely. |
| V1.10 | Original Morphology Baseline | Store the morphology each Mon had when entering the battle as `OriginalMorphology`. |
| V1.11 | Morph Reversion / Debuff | Add combat effects that pull current morphology back toward `OriginalMorphology`, partially dismantling morphology built during the match. |
| V1.12 | Morph Concentration | Measure morphology distribution/concentration so specialization can be distinguished from broad development rather than relying only on accumulated totals. |
| V1.13 | Corrupted / Balanced / Extremist | Introduce the high-level development axis only after real morphology behavior exists. Corrupted should relate to persistent unsupported execution, Balanced to broad controlled development, and Extremist to concentration in a small number of parameters. Exact formulas remain to be designed. |
| V1.14 | Advanced Transformations | Use morphology plus the Corrupted/Balanced/Extremist state to drive higher-level transformations. |
| V1.15 | Tamer ↔ Arena Expansion | Validate interactions where the Tamer can change/expand the arena, create objects/platforms or generate/potentiate morphology opportunities. |
| V1.16 | Competitive EnemyTamer | Improve EnemyTamer decisions so it strategically contests valuable morphology opportunities instead of only collecting opportunistically. |
| V1.17 | Pair Interaction | Validate stronger tactical interdependence between Tamer actions and Mon combat outcomes. |
| V2.0 | Target Prototype Loop | Tamer + Mon vs Tamer + Mon with contested arena resources, universal moveset, morphology-driven move capability, morphology feedback, reversion and dynamic development as the defining gameplay loop. |

## Target anatomical morphology — current working model

- Head: Weight, Hardness, Fang/Beak capability, Intellect.
- Torso: Weight, Hardness, Coverage.
- Arms: Weight, Extension, Drag, Quantity.
- Legs: Weight, Extension, Drag, Quantity.
- Tail: Weight, Extension.
- Element: elemental affinity/state.

This list is a design direction, not yet a locked production schema.

## Core loop to preserve

Player input -> universal move -> morphology requirements -> Fail / Effective / Critical -> gameplay effect -> up to three weighted morphology influences -> updated morphology -> changed future move execution.

Opponent reversion effects run in parallel and push morphology toward the Mon's original battle baseline.

## Future concepts — not active unless promoted to a milestone

- Four-player / 2 vs 2 final match structure.
- More complete move presets (2 moves + 1 special before battle).
- Special move combinations.
- Music/rhythm layer where correctly timed moves generate different morphology influences according to synchronized instruments.
- Larger arena evolution / spatial expansion systems.
- Production AI, networking, progression, narrative, content pipeline and art polish.

## Scope rule

Do not implement future concepts during an earlier milestone unless they are strictly required to validate that milestone. Prefer a working, inspectable prototype over anticipatory architecture.
