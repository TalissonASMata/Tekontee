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

## V1.7.1 — Shared Mon physics foundation (UNITY VALIDATED; SCENE COMMITTED)

- **Player-visible hypothesis:** both Mons use solid 2D bodies, shared grounding, gravity and collision, while manual movement retains rapid horizontal response. With a ground collider correctly configured, a Mon can jump over the other instead of walking through it. The AI and player must share the same physical movement owner.
- **Family:** ground locomotion / basic jumping / receiving knockback; no anatomical formula or new move yet.
- **Implementation:** `MonPhysicsController2D` on each Mon owns its `Rigidbody2D` and `CapsuleCollider2D` movement, ground probe, arena horizontal clamp and provisional knockback. `MonPlayerController` and `AutoFighter` submit movement intent and retain V1.6 fallback when the new motor is absent; `DamageFeedback` delegates physical displacement to the motor. `ArenaBounds2D` does not teleport a Mon with the motor. AutoFighter's attack-pulse root scaling is bypassed with physics active to avoid resizing the body collider.
- **Changed:** `Assets/_Project/Scripts/Control/MonPhysicsController2D.cs` (+ matching `.meta`), `MonPlayerController.cs`, `AutoFighter.cs`, `DamageFeedback.cs`, `ArenaBounds2D.cs`.
- **Verify / unchanged:** `UniversalMoveSet`, `CombatantStateMachine`, `Health`, `BattleManager`, `PlayerControlManager`, `GameplayCameraController`, `MorphPointBank`, `TamerController`, Tamer collection, HUD and Tamer arena remain untouched. Combat damage and morphology evaluation remain legacy V1.6 behavior.
- **Editor setup required** (not automatically serialized into ArenaPrototype): on **each** Mon, add `MonPhysicsController2D` (automatically requiring a Dynamic `Rigidbody2D` and `CapsuleCollider2D`); preserve root scale. For the current scene, Ground collider upper face is around world Y=-0.8; Mon roots begin at Y=-0.5. Suggested starting Capsule size is X=0.7, Y=1.1, center offset (0,0), which at 0.55 root scale places feet near the floor. Set ground object's layer to a dedicated `ArenaGround` layer and assign *only* that layer in both motors' `Ground Layers`; never include the Mon layer. UpperPlatform has no collider in this slice.
- **Acceptance in Unity:** with no component errors, both Mons rest on ground (do not fall through); manual A/D movement and jump work, including landing; attempting to walk through the opponent physically blocks; jumping above opponent can change sides; Tamer/Mon switching hands movement control to AI without jitter; HeavyStrike/AerialStrike/GuardBreak, damage, Tamer collection, both camera modes and restart continue working. Inspect Console, collisions and the gizmo foot probe; measure/test tuning before calling this validated. Hardware gamepad acceptance still deferred.
- **Known limitations:** provisional impulse/knockback values are not anatomical; no air-control model, platform one-way logic, polished recovery/animation, fixed collider changes on transformation, or height-based cap yet. No runtime test has occurred. If the grounded probe misses the Ground, verify the selected layer and capsule world-bottom geometry before changing movement code.
- **Validated:** user confirmed ground contact, jump, body blocking and jump-over; code plus scene/layer configurations committed in `997fde7`. Anatomical behavior is deferred to the later slices.

## V1.7.2 — Independent Mon anatomy storage (UNITY VALIDATED; SCENE COMMITTED)

- **Contract:** `MonAnatomy` stores `Cabeca`, `Tronco`, shared `Bracos`, shared `Pernas`, `Cauda` and `Elemento` independent of V1.6's `MorphPointBank` and visual forms.
- **Parameters:** each anatomical region exposes categorical `revestimento`, numerical `massa` and `tenacidade`; `cabeca` adds `presaOuBico` and `intelecto`; `tronco` adds `extensao` (PHY-098) and still-pending `cobertura`; shared arms and legs add `extensao`, `arrasto` and even `quantidade`; `cauda` adds `extensao`; `elemento` has an uncommitted-catalog `afinidadeOuEstado` text field. Categorical placeholders DO NOT affect combat formulas.
- **Derived read-only aggregates:** `MassaGeral` and `TenacidadeGeral` sum numeric regional contributions. Arms and legs store **per-limb** mass and tenacity and contribute per-limb value × quantity. Repeated-limb quantities default to two and are kept even. Numeric starting values default to zero **as unconfigured placeholders, NOT an approved balance or Mon design**.
- **Mutation pathway:** dedicated setters for leg mass/extension, arm extension, torso extension and arm/leg quantities notify `OnAnatomyChanged`. Direct Inspector edits are intended for editor configuration; do not rely on an event from them during Play. Aggregates always recompute on read.
- **No behavior change yet:** no conversion of V1.6 Attack/Defense/Air/Energy points; no gameplay integration; no automatic changes in jump, attack damage, sprite, physical collider, arena, HP or AI. The conversion from move/collection influence to anatomical points is a **separate implementation task**, not assumed here.
- **Files:** `Assets/_Project/Scripts/Morphology/MonAnatomy.cs` and `.meta`. `DESIGN_CORE.md` reconciles `Tronco.Extensão` with PHY-098.
- **Unity acceptance:** add `Mon Anatomy` to `Mon` and `EnemyMon` (outside Play), save scene, ensure no Console compilation errors. Use component context menu **V1.7.2 - Mostrar resumo anatomico** to inspect current aggregate values. As a temporary calculation check, enter `tronco.massa=2`, `bracos.massa=1` with quantity 2, and `pernas.massa=1` with quantity 2, keeping other masses 0: expect `MassaGeral=6`; restore test values to 0 afterward. Combat, jump and cameras should behave identically to validated V1.7.1.
- **Validated in Unity:** user confirmed `MassaGeral=6` with per-limb quantities, and committed `ArenaPrototype.unity` in `fd1dc79`. V1.7.3 independently tests anatomy-driven jump apex without resizing sprite or body collider.

## V1.7.3 — Continuous anatomy-derived Mon jump (UNITY VALIDATED; SCENE COMMITTED)

**1. Player-visible hypothesis:** changing `Pernas.Massa` and `Pernas.Extensão` gradually changes the height of the basic jump; increasing `MassaGeral` creates an inertia penalty. No corresponding sprite/form/physical body collider resize; gravity remains universal. The coefficients are **test tuning, not a locked physics decision**.

**2. Family:** existing *basic ground jump* only. No double-jump, glide, special move, new input, auto-jumping AI or elemental effects.

**3. Dependency nodes:** **change** morphology-to-gameplay derivation and player jump impulse, plus diagnostic height logging. **Verify** Rigidbody2D landing and physics collision, Mon/Tamer control switching, V1.6 three attacks, automatic fighting and form visual regression. **Unaffected** Tamer/EnemyTamer controllers, Mon auto attack selection, damage calculation, health, camera, collection and transformation.

**4. Actual source and scene touchpoints:** new `Assets/_Project/Scripts/Morphology/MonAnatomicalJumpStats.cs` (+ Unity `.meta`); modified `MonPlayerController.cs` and `MonPhysicsController2D.cs`. Existing `MonAnatomy.cs` is the source of truth; currently both Mons already have `MonAnatomy`, dynamic Rigidbody2D and CapsuleCollider2D in `ArenaPrototype.unity`. **Unity Editor step:** add `Mon Anatomical Jump Stats` to Mon and optionally EnemyMon; test only the manually controlled Mon. Scene modification is intentionally not committed before local validation.

**5. Minimum vertical slice:** `MonAnatomicalJumpStats` calculates a multiplier from `Pernas.Massa`, `Pernas.Extensão` and `MassaGeral`:

```text
propulsao = 1 + 0.25 * Pernas.Massa + 0.25 * Pernas.Extensao
inercia   = sqrt(1 + 0.08 * MassaGeral)
fator     = clamp(propulsao / inercia, 0.5, 2.25)
impulso   = MonPlayerController.jumpForce * fator
```

This explicitly provisional mapping is **not** a fixed real-world physical equation; coefficients and limits are adjustable in the Inspector. With the component enabled, the provisional `MonFormGameplayStats.AirJumpMultiplier` does **not** influence the basic jump, decoupling functional anatomical growth from the previous four-axis discrete form. With it absent/disabled, the old V1.7.1 jump path remains unchanged. `MonPhysicsController2D.TryJump` continues to own Rigidbody2D velocity. No anatomy point awards or cross-conversion implemented.

**6. Unity acceptance:** preserve the original `Mon` Inspector `jumpForce=4`, gravity `18`, and other masses `0`; stand away from the opponent to avoid physical interference. Edit `MonAnatomy` values **outside Play**, press Play, switch to Mon, jump once with Space, observe `[V1.7.3 SALTO]` and `[V1.7.3 APEX]`, then exit Play to configure the next case. Three cases:

| Case | Pernas.Massa (each) | Pernas.Extensão | MassaGeral | Expected provisional multiplier (approx) |
|---|---:|---:|---:|---:|
| A: base | 0 | 0 | 0 | 1.000 |
| B: medium | 1 | 0.5 | 2 | 1.277 |
| C: advanced | 2 | 1 | 4 | 1.523 |

These **are sample test inputs, not permanent character stats**. The logged real apex should increase A < B < C while same gravity, same sprite and same body collider are maintained. Actual peak depends on physical contacts; tests should avoid attacks and opponent interference. A `MonAnatomicalJumpStats` context menu **V1.7.3 - Mostrar calculo de salto** logs current morphology multiplier. Revert temporary anatomy values to zero after acceptance. Verify that controls, landing, collision, auto combat and three universal attacks still work. Gamepad testing remains deferred.

**7. Versioning & known limits:** user confirmed three Unity jump configurations, restored provisional anatomy to zero, and committed/synced the scene in `c1f2ffa`. Logs show A: `4.000 u/s`, apex `0.405 u`; B: `5.107 u/s`, repeated apex `0.674 u`; C: `6.093 u/s`, repeated apex `0.970 u`. **Continuous anatomy-to-jump impulse is runtime-validated.** Additional outlying heights (B up to `1.102 u`, C up to `1.421 u`) in combat indicate external interaction/interference; isolated physics calibration remains future work. The inherited `Air` form's temporary `Jump x2` no longer influences the anatomical basic jump while the new component is enabled. No jump animation blending, partial air steering, physical leg strength calibration, dynamic morphology gains, AI jumping, airborne secondary action Fail recoil, or global altitude cap. The coefficient formula remains experimental.

## V1.7.4 — Anatomical punch/kick hitbox probe (UNITY VALIDATED; SCENE COMMITTED)

**Goal:** verify that **functional attack contact geometry** changes gradually with anatomical extension without changing the sprite, root `CapsuleCollider2D`, `Rigidbody2D`, gravity, damage formulas or the legacy universal move input/AI. This is the minimum safe first half of the punch/kick reach slice. It is not yet a playable new punch/kick move.

**Why separate:** V1.6 `HeavyStrike`, `AerialStrike`, `GuardBreak` only check `abs(target.x - attacker.x) <= move.range` and immediately apply damage. They are generic prototype moves, *not* defined physical punches/kicks. The player controller also prechecks their range, and the AutoFighter uses range for chasing. Retrofitting only one of these checks could make AI or manual behavior inconsistent; reinterpreting the existing moves as punches violates the recorded universal-moveset / face-button semantics.

**New component:** `Assets/_Project/Scripts/Combat/MonAnatomicalStrikeHitboxes.cs` (+ `.meta`), to be added to the **Mon** in Unity after local pull. It derives **punch reach** from `Braços.Extensão` and `Tronco.Extensão`; **kick reach** from `Pernas.Extensão` and `Tronco.Extensão`. Default experimental formulas:

```text
alcanceSoco = 1.0 + 0.40 * Bracos.Extensao + 0.25 * Tronco.Extensao
alcanceChute = 1.0 + 0.40 * Pernas.Extensao + 0.25 * Tronco.Extensao
```

These lengths start **at the forward outside edge of the current body collider**, not at its center. A rectangular attack box has adjustable height 0.65 u and vertical offsets +0.08 u (punch) or -0.15 u (kick). The direction is computed toward the opposing Mon at the time of the probe (PHY-097). Calls use `Physics2D.OverlapBoxAll`, matching a target's `Health` component on the collider or its parent; the geometry is a **query**, not an actual trigger / physical collider. Scene View with the Mon selected and Gizmos enabled draws **cyan punch** / **yellow kick** outlines. Reach is calculated on demand, no visual transformation or anatomy event subscription necessary.

**Unity check (no Play binding change):**
1. Pull branch and open Unity, add **Mon Anatomical Strike Hitboxes** to Mon, leaving defaults; select Mon with Gizmos shown in the Scene View. The target is automatically derived from MonPlayerController / AutoFighter if the explicit Target field is empty.
2. Keep all anatomy extensions at zero. Play, switch to Mon manual control, and temporarily disable EnemyMon's **Auto Fighter** component for predictable positioning. Walk Mon toward EnemyMon so the **space between body colliders** is between 1.0 and 1.4 u (approx. 1.2 u), then release movement.
3. With the game still in Play and Mon selected, open the **Mon Anatomical Strike Hitboxes component context menu** and run **V1.7.4 - Testar hitbox de SOCO**. Expect `[V1.7.4 SOCO] ... resultado=FORA; alcance=1...` and inspect the printed `espacoEntreCorpos` rather than relying on transform-center distance. Do not move either character.
4. Still in the **same Play session**, in Mon Anatomy set **Bracos.Extensao=1**, then invoke the same context menu again. Expect `resultado=CONTATO` and `alcance=1.4`, at unchanged center distance. Restore arms extension to 0, set **Pernas.Extensao=1**, and invoke **V1.7.4 - Testar hitbox de CHUTE**: expect `CONTATO` and `alcance=1.4`. `Tronco.Extensao=1` should increase either reach a further 0.25 u independently (optional additional test).
5. Stop Play. Experimental anatomy values edited during Play revert automatically. Keep the new component attached to Mon, save scene. Confirm old J/K/L attacks still operate before closing the slice; do not rename or remap them yet.

**Scope and limitations:** these queries only log `CONTATO` / `FORA`; **they do not inflict damage**, grant points, invoke cooldown, drive AI or create a playable attack. Input mapping (□ weak punch, ✕ weak kick, △ elemental strong punch, ○ elemental strong kick), state animations, strike damage scaling, contact timing, and elemental reach belong to later controlled slices. Do not confuse the existing V1.6 range-only attacks with validated strike hitboxes. **Unity validated:** The user confirmed baseline punch/kick out-of-range at 1.141 u between bodies with reach=1.0; with `Bracos.Extensao=1`, punch reached 1.4 and contacted, kick stayed 1.0/OUT at unchanged distance; with `Pernas.Extensao=1`, kick reached 1.4 and contacted while punch stayed OUT (distance changed during this separate probe); with `Tronco.Extensao=1` and both limb extensions zero, *both* attacks changed from OUT at reach 1.0 to CONTACT at reach 1.25, at fixed 1.175 u between bodies. The query geometry reacts to anatomy without changing the body distance in controlled probes. New Mon component saved in scene commit `9013816`, and EnemyMon `AutoFighter` re-enabled in follow-up scene commit `348783d`. No damage or gameplay input was part of V1.7.4.

## V1.7.5 — Weak punch / weak kick: real hit detection and test damage (CODE COMMITTED; UNITY TEST PENDING)

**Goal:** turn the validated V1.7.4 anatomical hitboxes into **player-executed** weak punch and weak kick, without converting V1.6 generic prototype moves to punches/kicks. New normal attacks use input system actions:
- **F / PlayStation Square / Gamepad buttonWest** = WeakPunch;
- **V / PlayStation Cross / Gamepad buttonSouth** = WeakKick.

**Implementation:** new `Assets/_Project/Scripts/Combat/MonBasicStrikeSet.cs` (+ `.meta`), to be added to the **Mon** after pulling the feature branch. It requires the existing `MonAnatomicalStrikeHitboxes` component and takes the target `Health` from `MonPlayerController`. On input, calls `Physics2D.SyncTransforms()` then the previously validated `OverlapsTarget(punch, target)`. If contact exists, applies prototype `10 HP` damage (`weakPunchDamage`/`weakKickDamage` configurable). Each attempt, **including misses**, consumes the existing Mon attack cooldown and `Attacking` lock; it logs `[V1.7.5 SOCO FRACO]` or `[V1.7.5 CHUTE FRACO]`, contact, reach and damage. A hit triggers existing `Health.OnDamaged` responses. Two simultaneous attack inputs deterministically prioritize punch. No damage on whiff, no automatic morphological points, no elemental effects, no new knockback or mass scaling yet.

**Compatibility:** manual J/K/L + keyboard Shift+J V1.6 test attacks remain available. Previous provisional gamepad `buttonSouth` HeavyStrike, `rightShoulder+buttonSouth` AerialStrike and `buttonEast` GuardBreak bindings are removed, to avoid double-activating the new Cross button and to reserve Circle for strong kick. The old attacks still exist and the existing `AutoFighter` logic still uses the `UniversalMoveSet`. G/B strong strikes and shoulder commands are **not implemented**. Mon still jumps only with Up Arrow on keyboard; gamepad jump awaits assignment. Enter restarts post-battle only. Neither Tamer movement nor Tamer collection changes here.

**Unity acceptance (manual):**
1. Pull; ensure Unity Console compiles; outside Play add **Mon Basic Strike Set** to the **Mon**, alongside **Mon Anatomical Strike Hitboxes**. Keep anatomy extensions 0. Save only after runtime validation.
2. Enter Play, switch from Tamer to Mon, disable EnemyMon Auto Fighter **during Play** only to isolate enemy action. Move until the space between body colliders is below 1.0 u and the fighters are at the same grounded height; press F once and V once (respect attack cooldown). Expect distinct `CONTATO` logs and **10 HP damage each**.
3. Move away beyond 1.0 u but below 1.4 u between colliders. F/V must show `FORA` and apply zero damage at extensions zero.
4. In **the same Play session and without moving characters**, set `Bracos.Extensao=1`; F should contact and apply damage, V should remain outside. Reset arms, set `Pernas.Extensao=1`; V should contact and apply damage, F should remain outside. Distances must be shown/checked when evaluating unexpected boundary behavior.
5. Verify J/K/L keyboard actions still work, Tab still switches modes, arrows/up jump, Enter after defeat, and EnemyMon normal automatic combat resumes on next Play.
6. Stop Play, check that the temporary extension values reverted to 0, new component stays installed, EnemyMon Auto Fighter is **enabled outside Play**, save scene, commit only after validation.

**Open limits:** damage=10 is a temporary test constant, not a mass/acceleration-derived final equation. Future slices must integrate the normal attacks into the shared universal move registry and AI, define morphology requirements/reinforcement, strong physical-elemental attacks G/B, PlayStation and Xbox controller hardware tests and their definitive jump action. V1.7.5 is **not validated in Unity** until the user confirms the real hits/whiffs and scene commit.

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
