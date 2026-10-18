# Tekontee — AI Context

This file is the shared project context for the AI GitBridge Workflow (GitBridge). AI agents should read this file before planning or changing gameplay code.

## Product direction

Tekontee is a combat game built around a Tamer + Mon pair. The intended final experience is 2 vs 2 (four participants), but the current prototype validates the loop with one player-controlled pair and AI-controlled opponents.

The prototype should validate gameplay before production architecture, content scale, art polish or narrative systems.

## Core gameplay model

There are two simultaneous gameplay layers:

- Mon layer: traditional side-view fighting-game combat.
- Tamer layer: platforming / arena interaction used to collect, modify or create opportunities for morphological development.

The player switches control between Tamer and Mon. When the player controls the Tamer, the player's Mon can fight automatically. When the player controls the Mon, the Tamer can perform automated support behavior.

## Current validated baseline

Validated through V1.5:

- core combat loop;
- Tamer/Mon control switching;
- Morph Points;
- Tamer collection and automated support;
- EnemyTamer competing for arena resources;
- PlayerMon and EnemyMon both using morphology systems;
- morphology affecting combat stats;
- control-mode camera system;
- battle/restart loop;
- weighted multi-influence morphology reinforcement per successful combat action.

V1.5 validated in Unity: move requirement type can be independent from the current dominant form; execution quality (Fail / Effective / Critical) adjusts damage and weighted morphology reinforcement. Both automatic and manual paths use the shared evaluator.

V1.6 — Universal Moveset has been validated in Unity with keyboard (HeavyStrike, AerialStrike, GuardBreak; Shift+J chord precedence). Physical gamepad validation remains deferred. The active next milestone is V1.7 — Anatomical Morphology. Historical V1.6 validation criteria: Minimal validation: two distinct moves with independent requirement axes and weighted influences, accessible to the same Mon without being gated by current morphology; test manual and automatic selection before expanding the catalog. The moveset must work on both keyboard and modern gamepads (Xbox and PS5/DualSense-class). Route both device types through common Unity Input System actions, with no gameplay logic tied to specific key/button labels. Establish keyboard/gamepad parity for these two moves and preserve an extensible input mapping for later actions. **Permanent control constraint:** any individual move uses one button or a chord of at most two simultaneous buttons, on both keyboard and gamepad (e.g. PlayStation R1 + Square, Xbox LT + A). No normal move may require three simultaneous buttons. Special techniques can be time-ordered combinations of existing moves/states; at any moment they must still respect the two-simultaneous-button maximum. Keep actual button mappings device-specific, but gameplay commands device-independent.

## Morphology — current prototype

Current temporary morphology types:

- Attack
- Defense
- Air
- Energy

`MorphPointBank` currently stores absolute values and exposes a dominant type. `MonDominantFormVisual` handles visual feedback and `MonFormGameplayStats` handles gameplay effects.

V1.4 added weighted multi-influence reinforcement: a combat action can apply up to three `MorphInfluence` values in one atomic bank update. Player and automatic combat both use this path.

These four axes are prototype placeholders. They are not the intended final morphology model.

## Morphology — target direction

The intended system is anatomical and multidimensional, with approximately 18 parameters distributed across body regions. Current working design:

- Head: Weight, Hardness, Fang/Beak capability, Intellect.
- Torso: Weight, Hardness, Coverage.
- Arms: Weight, Extension, Drag, Quantity.
- Legs: Weight, Extension, Drag, Quantity.
- Tail: Weight, Extension.
- Element: elemental affinity/state, treated as a special layer rather than ordinary anatomy when useful.

The final morphology should emerge from the player's actions rather than from direct form selection.

## Universal moveset principle

All Mons should conceptually share the same base moveset. Examples include:

- headbutt;
- bite;
- peck;
- scream;
- guard;
- parry;
- jump;
- double jump;
- glide;
- brace/retesar;
- low kick;
- high kick;
- quick punch;
- heavy punch;
- dash/advance;
- retreat;
- grab;
- and later special techniques.

Moves are universal, but each move has minimum morphology requirements. A Mon may know the move conceptually while being physically unable to execute it well.

Example conceptual PlayStation inputs: Square = weak strike; R1 = dodge; R2 = scream; L1 = block; L2 = brace/retesar. R1+Square = teleport strike; R2+Square = Battle Cry; L1+Square = parry; L2+Square = grab. These **are all normal moves** even when using two buttons. Xbox and keyboard receive semantically equivalent mappings. Input chords must take precedence over their individual button actions to prevent accidental double-triggering.

Example future special: `Retesar -> Battle Cry -> Triangle (elemental action)` produces `Water Shot`, a water sphere that travels across the screen. This is a **sequence of regular moves and an elemental action**, not a three-or-more-button simultaneous chord. Special input timing, state requirements and morphology requirements remain to be designed; do not implement the special during V1.6.

## Move execution quality

A move should be evaluated against the Mon's current morphology and resolve as one of three qualities:

- Fail: morphology is below the effective requirement. The move still happens in a degraded form and grants only minimal morphological reinforcement.
- Effective: morphology satisfies the intended requirement. The move grants medium reinforcement.
- Critical: morphology strongly exceeds or optimally satisfies the requirement. The move grants maximum reinforcement.

This creates the intended feedback loop:

Current morphology -> move execution quality -> morphological reinforcement -> changed morphology -> changed future move execution quality.

V1.5 should validate this quality evaluation using the current temporary Attack/Defense/Air/Energy model and the weighted reinforcement architecture already proven in V1.4. Do not introduce final counter balance, anatomical morphology, or the universal moveset catalog yet unless strictly required for the smallest test.

## Weighted morphological influence

A successful gameplay event must not be restricted to only one morphology attribute.

A move or special may reinforce up to three morphology dimensions at different intensities, conceptually:

- primary/intense influence;
- secondary/medium influence;
- tertiary/basic influence.

Example concept only:

Headbutt critical -> strong Head.Weight, medium Head.Hardness, basic Torso.Weight.

V1.4 validated this architecture using the current temporary Attack/Defense/Air/Energy axes.

## Specials

Special techniques are higher-order sequences/compositions of regular moves or states, not two-button normal-move combinations. Special techniques should require more advanced morphology combinations. This is important to avoid a simple projectile-spam feedback loop where repeatedly using one easy special endlessly feeds its own prerequisites.

## Original morphology and reversion

Each Mon enters a battle with an `OriginalMorphology` baseline.

Some opponent effects should act as morphological debuffs by pulling the target's current morphology back toward that original baseline. They do not push the target farther away from its original form.

Conceptually:

CurrentMorphology -> toward OriginalMorphology.

This lets an opponent partially dismantle a morphology build created during the current match.

## Corrupted / Balanced / Extremist meta-axis — future

Future high-level transformations should use a separate meta-axis:

Corrupted <-> Balanced <-> Extremist

Current design intent:

- Corrupted: associated with persistent use of moves that the current morphology cannot properly support; exact unique gameplay benefit is still undefined and must not be treated merely as a punishment.
- Balanced: broad control and versatility, conceptually supporting up to about 9 parameters at medium development.
- Extremist: highly concentrated development, conceptually maximizing up to about 3 parameters, with reduced behavioral/control stability in exchange for extreme performance.

Do not implement this axis until the underlying morphology and move-quality systems are mature enough for the state to emerge from actual gameplay patterns.

## Tamer

The current player Tamer has:

- horizontal movement and jump;
- manual point collection;
- automatic point collection while the player controls the Mon;
- argument/intervention mechanics affecting collectibles;
- a target MorphPointBank that receives collected points.

EnemyTamer is validated and competes for resources in the arena layer.

## Combat

The current prototype already contains:

- health and death flow;
- combat state machine;
- automatic fighter behavior;
- manual Mon control;
- damage feedback;
- HP UI;
- restart/battle loop;
- arena bounds;
- control switching between Tamer and Mon;
- shared morphology behavior for PlayerMon and EnemyMon;
- weighted morphology reinforcement on successful hits in both manual and automatic Mon combat.

## Camera

Current camera behavior is validated:

- Tamer mode: strategic camera follows the Tamer horizontally.
- Mon mode: fighting-game camera centers between PlayerMon and EnemyMon with controlled zoom.

Do not replace this behavior casually when implementing unrelated features.

## Technical dependency map and AI change-planning rule

Before planning or coding **any new gameplay feature**, read `GAMEPLAY_TECHNICAL_MAP.md`. List movement family, affected technical nodes (change / verify / unaffected), inspected script and scene dependencies, smallest vertical slice, testable Unity acceptance criteria, and explicitly deferred systems. Do not use lore, art or feature-catalog expansion as a substitute for gameplay validation.

V1.7 must separate continuous **functional** morphology (jump height, strike reach / hitbox) from **discrete visual transformation** at later morphology thresholds. Morphology point gains do not resize or replace the sprite themselves. Start with one jump and one punch-reach hypothesis; plan the movement taxonomy, but do not implement the entire catalog.

## Prototype development principles

1. Make the smallest testable change that validates the next mechanic.
2. Do not implement future systems just because they are known.
3. Reuse systems between player and enemy whenever their rules are conceptually the same.
4. Avoid copying logic into separate Player/Enemy classes when composition or shared components can solve it.
5. Preserve already validated gameplay unless the requested milestone intentionally changes it.
6. Prefer inspector-configurable values for prototype tuning.
7. Keep code understandable and direct; avoid production-scale abstractions before they are needed.
8. Unity scene/prefab references matter. When code changes require inspector setup, explicitly document every required Unity Editor change.
9. Design new morphology mechanics so the current 4-axis prototype can later expand to the anatomical parameter set without rebuilding the entire gameplay loop.

## GitBridge workflow

The human defines desired gameplay behavior, sensations and acceptance criteria. ChatGPT translates that intent into mechanics and architecture. Coding agents execute against the repository. Unity remains the final runtime validation environment.

Typical loop:

1. Read `AI_CONTEXT.md`, `DESIGN_CORE.md`, `ROADMAP.md`, `GAMEPLAY_TECHNICAL_MAP.md` and `.github/copilot-instructions.md`.
2. Inspect current code before proposing changes.
3. Plan the smallest implementation for the active milestone.
4. Work on a feature branch.
5. Produce a clear diff and list any Unity Editor setup.
6. Human tests the behavior in Unity.
7. Iterate from observed gameplay, not assumptions.

## Current repository organization

Main gameplay code lives under `Assets/_Project/Scripts/`:

- `Arena/` — arena limits and arena management.
- `Camera/` — gameplay camera behavior.
- `Combat/` — fighter, battle, health, states and damage feedback.
- `Control/` — player control switching and manual Mon control.
- `Morphology/` — Morph Points, form visuals and form gameplay stats.
- `Preset/` — Mon automatic preset behavior.
- `Tamer/` — Tamer movement, collection and arguments.
- `UI/` — gameplay status UI.

Main prototype scene: `Assets/_Project/Scenes/ArenaPrototype.unity`.
