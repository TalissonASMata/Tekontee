# Gameplay Technical Dependency Map

Purpose: prevent feature creep and lost dependencies as Tekontee expands. This is an engineering planning map, **not** a commitment to implement the full move catalog or new lore/art.

## Non-negotiable separation

1. **Move definition**: what an action does, its input-independent ID, timing, morphology requirements and weighted influences.
2. **Input resolution**: keyboard/gamepad -> semantic command. Chords take priority over singles. Physical gamepad testing is deferred, not certified.
3. **Morphology storage**: independent anatomical parameter values and point changes.
4. **Gameplay derivation**: continuously converts morphology into properties such as jump impulse, attack reach/hitbox, movement and resistance.
5. **Move execution / hit detection**: applies the derived gameplay properties and resolves Fail/Effective/Critical, hit, damage, reinforcement and cooldown.
6. **Transformation**: separate threshold/pattern-based decision to replace sprite/form. **Continuous morphology changes must not continuously alter sprites.** Transformation rules come later.
7. **AI and Tamer**: use shared move semantics, morphology rules and collection channels without duplicating combat formulas.
8. **Presentation and observability**: UI, feedback and diagnostic logs display state; they do not own combat/morphology rules.

A visual sprite can remain fixed while jump height or hitbox changes smoothly. A transformation may later select a different sprite when its explicit criteria are met.

## Candidate movement families (taxonomy for planning, NOT a locked catalog)

| Family | Examples | Core technical nodes |
|---|---|---|
| Ground locomotion | walk, advance, retreat, dash | movement controller, speed/acceleration, collision, states |
| Air mobility | jump, double jump, glide | grounded detection, gravity, impulse, air control, morphology |
| Melee strikes | punch, kick, headbutt, bite, peck | input command, strike geometry/hitbox, reach, hit detection, damage, move quality |
| Defense | guard, evade, parry | states, timing window, damage/knockback response |
| Grapples and manipulation | grab, push, throw | proximity/overlap, temporary target attachment, forces, interruption |
| Waves and emissions | scream, breath, projectile | projectile or area geometry, lifetime, collision, elemental evaluation |
| Preparation/support | brace, charge, focus | state duration, modifiers, cancel/consumption |
| Specials (future) | multi-step recipes | semantic action history, state windows, morphology prerequisites, precedence |

One concrete move can share nodes with several families. Do not create a new physics or damage pipeline per move. A taxonomy is a navigation aid, not a class inheritance requirement.

## V1.7.1 — Shared Mon physics foundation (IMPLEMENTED IN CODE; UNITY TEST PENDING)

- **Player-visible hypothesis:** both Mons use solid 2D bodies, shared grounding, gravity and collision, while manual movement retains rapid horizontal response. With a ground collider correctly configured, a Mon can jump over the other instead of walking through it. The AI and player must share the same physical movement owner.
- **Family:** ground locomotion / basic jumping / receiving knockback; no anatomical formula or new move yet.
- **Implementation:** `MonPhysicsController2D` on each Mon owns its `Rigidbody2D` and `CapsuleCollider2D` movement, ground probe, arena horizontal clamp and provisional knockback. `MonPlayerController` and `AutoFighter` submit movement intent and retain V1.6 fallback when the new motor is absent; `DamageFeedback` delegates physical displacement to the motor. `ArenaBounds2D` does not teleport a Mon with the motor. AutoFighter's attack-pulse root scaling is bypassed with physics active to avoid resizing the body collider.
- **Changed:** `Assets/_Project/Scripts/Control/MonPhysicsController2D.cs` (+ matching `.meta`), `MonPlayerController.cs`, `AutoFighter.cs`, `DamageFeedback.cs`, `ArenaBounds2D.cs`.
- **Verify / unchanged:** `UniversalMoveSet`, `CombatantStateMachine`, `Health`, `BattleManager`, `PlayerControlManager`, `GameplayCameraController`, `MorphPointBank`, `TamerController`, Tamer collection, HUD and Tamer arena remain untouched. Combat damage and morphology evaluation remain legacy V1.6 behavior.
- **Editor setup required** (not automatically serialized into ArenaPrototype): on **each** Mon, add `MonPhysicsController2D` (automatically requiring a Dynamic `Rigidbody2D` and `CapsuleCollider2D`); preserve root scale. For the current scene, Ground collider upper face is around world Y=-0.8; Mon roots begin at Y=-0.5. Suggested starting Capsule size is X=0.7, Y=1.1, center offset (0,0), which at 0.55 root scale places feet near the floor. Set ground object's layer to a dedicated `ArenaGround` layer and assign *only* that layer in both motors' `Ground Layers`; never include the Mon layer. UpperPlatform has no collider in this slice.
- **Acceptance in Unity:** with no component errors, both Mons rest on ground (do not fall through); manual A/D movement and jump work, including landing; attempting to walk through the opponent physically blocks; jumping above opponent can change sides; Tamer/Mon switching hands movement control to AI without jitter; HeavyStrike/AerialStrike/GuardBreak, damage, Tamer collection, both camera modes and restart continue working. Inspect Console, collisions and the gizmo foot probe; measure/test tuning before calling this validated. Hardware gamepad acceptance still deferred.
- **Known limitations:** provisional impulse/knockback values are not anatomical; no air-control model, platform one-way logic, polished recovery/animation, fixed collider changes on transformation, or height-based cap yet. No runtime test has occurred. If the grounded probe misses the Ground, verify the selected layer and capsule world-bottom geometry before changing movement code.
- **Next:** once Unity verifies V1.7.1, begin V1.7.2 anatomy storage, then morphological jump and punch tests independently. Do not call this version validated until user confirms results.

## V1.7.2 — Independent Mon anatomy storage (IMPLEMENTED IN CODE; UNITY TEST PENDING)

- **Contract:** `MonAnatomy` stores `Cabeca`, `Tronco`, shared `Bracos`, shared `Pernas`, `Cauda` and `Elemento` independent of V1.6's `MorphPointBank` and visual forms.
- **Parameters:** each anatomical region exposes categorical `revestimento`, numerical `massa` and `tenacidade`; `cabeca` adds `presaOuBico` and `intelecto`; `tronco` adds `extensao` (PHY-098) and still-pending `cobertura`; shared arms and legs add `extensao`, `arrasto` and even `quantidade`; `cauda` adds `extensao`; `elemento` has an uncommitted-catalog `afinidadeOuEstado` text field. Categorical placeholders DO NOT affect combat formulas.
- **Derived read-only aggregates:** `MassaGeral` and `TenacidadeGeral` sum numeric regional contributions. Arms and legs store **per-limb** mass and tenacity and contribute per-limb value × quantity. Repeated-limb quantities default to two and are kept even. Numeric starting values default to zero **as unconfigured placeholders, NOT an approved balance or Mon design**.
- **Mutation pathway:** dedicated setters for leg mass/extension, arm extension, torso extension and arm/leg quantities notify `OnAnatomyChanged`. Direct Inspector edits are intended for editor configuration; do not rely on an event from them during Play. Aggregates always recompute on read.
- **No behavior change yet:** no conversion of V1.6 Attack/Defense/Air/Energy points; no gameplay integration; no automatic changes in jump, attack damage, sprite, physical collider, arena, HP or AI. The conversion from move/collection influence to anatomical points is a **separate implementation task**, not assumed here.
- **Files:** `Assets/_Project/Scripts/Morphology/MonAnatomy.cs` and `.meta`. `DESIGN_CORE.md` reconciles `Tronco.Extensão` with PHY-098.
- **Unity acceptance:** add `Mon Anatomy` to `Mon` and `EnemyMon` (outside Play), save scene, ensure no Console compilation errors. Use component context menu **V1.7.2 - Mostrar resumo anatomico** to inspect current aggregate values. As a temporary calculation check, enter `tronco.massa=2`, `bracos.massa=1` with quantity 2, and `pernas.massa=1` with quantity 2, keeping other masses 0: expect `MassaGeral=6`; restore test values to 0 afterward. Combat, jump and cameras should behave identically to validated V1.7.1.
- **Not yet validated in Unity**; close this slice only after user confirmation and scene commit. Next V1.7.3 is anatomy-driven jump apex using `Pernas.Massa`, `Pernas.Extensão`, `MassaGeral` without changing sprite or body collider.

## Minimal anatomical gameplay hypotheses for V1.7

**H1 — Jump:** an anatomical parameter (or derived combination of leg parameters, exact mapping to be decided) incrementally increases jump height, with **no sprite change**. Preserve current jump input and game loop. Test measurable jump apex at at least three parameter values, including baseline.

**H2 — Punch:** an anatomical parameter such as `Arms.Extension` incrementally increases the effective punch hitbox/reach, with **no sprite change**. Test two target distances before/after point gain, including outside initial range but inside upgraded range. Other moves do not automatically inherit the punch hitbox.

Existing V1.6 HeavyStrike/AerialStrike/GuardBreak are generic test attacks, not proof of physical punch geometry. A real punch test needs an explicitly defined attack geometry, with directional behavior and debug visualization when practical.

V1.7 is a **functional morphology** milestone. Do not implement all 18 derived gameplay effects, final transformation criteria, rhythm, lore, special recipes or production assets simply because they appear in the map.

## Technical change-impact matrix

| New behavior | Inspect/extend | Ensure still works |
|---|---|---|
| New universal move | move catalog/definition, semantic input mapping, attack/defense execution, morphology requirements and influences | manual and AI actions, cooldown, one command -> one move |
| Jump parameter | anatomical bank, derived jump stat, Mon/Tamer movement ownership where applicable, diagnostics | ground detection, control switching, camera |
| Punch reach parameter | anatomical bank, derived hitbox shape, target detection, facing/direction, diagnostics | existing moves, range and damage, AI behavior |
| New anatomical parameter | parameter ID/group, storage, update events, move requirements/reinforcement, debug UI | serialized Unity fields and all consumers |
| Morphology collectible | collector/spawner, mapping to parameter, bank event | Tamer auto/manual collection, competition |
| Transformation | threshold/pattern evaluator, form state, sprite change on transformation **only** | gradual stats/hitboxes remain independent |
| New AI move | shared catalog, AI move choice and validity checks | player move outcomes use same resolver |
| New input chord | semantic input action mapping and precedence | base press does not double-fire |
| New gameplay feedback | UI/FX and events | no gameplay values calculated solely in view layer |

Class names are **current implementation details**, not sacred architectural boundaries. Find call sites and scene references before changing them.

## Mandatory AI feature-planning protocol

Before proposing edits, provide this compact report:

1. **Player-visible hypothesis:** what observable behavior and what *not* to change.
2. **Movement family and mechanic:** existing family vs new mechanism; is this a normal move, state or special sequence?
3. **Dependency nodes:** input, movement/physics, attack geometry, combat, morphology, transformation, AI/Tamer, UI/debug, scene/serialization. Mark each **change / verify / unaffected** with rationale.
4. **Existing code touchpoints:** actual inspected scripts, components, bindings, scene/prefab inspector changes. No guessed paths.
5. **Smallest vertical slice:** minimum implementation needed to prove the hypothesis. Defer unrelated branches.
6. **Unity acceptance test:** exact setup, steps, expected values/logs and regression checks. Explicitly flag untested hardware.
7. **Versioning:** proposed branch/commit, documentation updates, Unity-generated `.meta` files, and known limitations.

If a feature demands a new technical node, document it here and explain its integration before coding. If information is undecided, present choices instead of silently fixing game-design decisions. Avoid narrative/lore/art ideation unless explicitly requested.

## Future feature request template

- Feature / movement:
- Player action and desired observable result:
- Morphology influencing execution (read):
- Morphology reinforced by the action (write; max 3 weighted):
- Relevant geometry / physics:
- Input semantics (max 2 simultaneous buttons for a normal move):
- AI equivalent:
- Interaction with transformation (usually **none** until threshold):
- Technical nodes change / verify / unaffected:
- Editor setup and data migration:
- Objective Unity test + regression:
- Explicit non-goals:
- Version, branch and validation status:
