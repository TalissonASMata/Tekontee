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
| V1.6 | Universal Moveset | Validated in Unity with keyboard: HeavyStrike (Attack), AerialStrike (Air), GuardBreak (Defense); manual and automatic execution share UniversalMoveSet; chord Shift+J resolves only AerialStrike; third move added without changing shared TryExecute combat calculations. Physical Xbox/PS5 controller testing deferred. |
| V1.7 | Anatomical Morphology foundations | Validated in Unity and scene committed: shared physics/collisions (V1.7.1), anatomical storage (V1.7.2), anatomical jump (V1.7.3), punch/kick reach hitboxes (V1.7.4), F/V weak strikes applying damage on contact with dynamically extended limbs/torso (V1.7.5; baseline scene corrected in `ac245e0`). |

## Active roadmap

| Version | Milestone | Validation target |
|---|---|---|
| V1.8 | Full Morphology Feedback Loop | **In progress.** V1.8.1 validated (F/V contact develops arm/leg extension). V1.8.2 **Unity validated on 2026-10-24**, including Fail 0 damage / minimal attempt growth, Effective normal hit, and interrupting Critical counter-hits at 0.45 s AI startup. V1.8.3 **Unity validated on 2026-10-24**: F/V reinforce three weighted anatomical parameters (extension primary, tenacity secondary, per-limb mass tertiary) at Fail / Effective / Critical multipliers; tested numerical deltas and aggregate totals. User also confirmed that changing leg mass with leg extension held constant changes basic jump behavior, linking combat to movement. Weights remain provisional; no exact jump-height metrics supplied. V1.8.4 **Unity validated on 2026-10-24**: F/V eligibility requires extension AND tenacity AND per-limb mass together, with provisional thresholds 0.25 / 0.10 / 0.05 and no compensation; user confirmed isolated missing requirements, natural Fail-to-Effective progression, 10 HP Effective and 15 HP interrupting Critical. V1.8.5 **Unity functionally validated on 2026-10-24**: shared physics motor scales grounded walking speed from leg extension AND general mass for both manual and AI control. Logged player baseline 2.990 u/s at extension 0 / mass 0.08, 4.486 u/s at extension 2 / mass 0.08, and 3.348 u/s at extension 2 / mass 10.08; AI logged 1.858 u/s at extension 2 / mass 20.08 (user confirmed changing mass per leg from 5 to 10). V1.8.6 **Unity functionally validated on 2026-10-24**: airborne input release retains horizontal momentum with gradual decay (2.99 -> 2.95 u/s), air direction reversal preserves previous travel direction initially (input right, -2.99 -> -2.83 u/s, targeting +2.99), no-input landing stops immediately (2.67 -> 0), and held-direction landing immediately resumes normal walking speed (-1.71 -> +2.99). Shares anatomy-derived speed target with player/AI. Coefficients remain experimental. V1.8.7 **Unity functionally validated on 2026-10-24**: voluntary morphology-gated gliding using provisional Down Arrow control, arms drag threshold, harmless Fail with minimal growth, Effective drag-dependent descent speed while retaining universal gravity and prior jump/air control. V1.8.8 **mass-resisted physical knockback functionally validated in Unity on 2026-10-24**: basic F/V Effective/Critical contact now applies a small horizontal 2D Rigidbody impulse; target `.MassaGeral` reduces its received velocity impulse (experimental factor `1/sqrt(1 + mass*0.08)`, lower clamp 0.25); Fail/whiff never push; original hit damage/quality and movement baseline remain unchanged. Ground adhesion and terrain-specific traction deferred by design. Further slices: shared player/AI anatomical move semantics and remaining morphology effects. |
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
