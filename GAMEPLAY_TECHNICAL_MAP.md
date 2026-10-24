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

## V1.7.5 — Weak punch / weak kick: real hit detection and test damage (UNITY VALIDATED; SCENE COMMITTED)

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

**Open limits:** damage=10 is a temporary test constant, not a mass/acceleration-derived final equation. Future slices must integrate the normal attacks into the shared universal move registry and AI, define morphology requirements/reinforcement, strong physical-elemental attacks G/B, PlayStation and Xbox controller hardware tests and their definitive jump action. **Validated in Unity:** user confirmed tests A–E: F and V cause 10 HP damage per contact, do no damage when out of range, and respond to Bracos.Extensao, Pernas.Extensao and Tronco.Extensao independently; the console excerpt captured `[V1.7.5 SOCO FRACO] ... resultado=CONTATO ... danoAplicado=10`, `[V1.7.5 CHUTE FRACO] ... resultado=CONTATO ... danoAplicado=10` and cooldown rejection logs. User committed the component into scene in `9e4e41d`; follow-up commit `ac245e0` restored the zeroed leg extension and re-enabled EnemyMon Auto Fighter. No further V1.7.5 scene tests are pending.

## V1.8.1 — Incremental anatomical reinforcement from successful basic strikes (UNITY VALIDATED; NO SCENE COMMIT NEEDED)

**Purpose:** validate the *first causal feedback link*, `valid physical hit -> corresponding anatomical extension increases -> subsequent physical reach increases`, on top of V1.7.5 without touching the V1.6 Attack/Defense/Air/Energy bank, other universal moves or the EnemyMon AI.

**Implementation:** `MonBasicStrikeSet` now has `Enable Anatomical Growth On Hit = true`, `Arm Extension Gain Per Punch Hit = 0.1`, `Leg Extension Gain Per Kick Hit = 0.1`. These are **temporary inspector-adjustable test increments, not locked per-move design rules**. A confirmed weak punch contact increases `MonAnatomy.Bracos.Extensao` by 0.1, while a confirmed weak kick increases `MonAnatomy.Pernas.Extensao` by 0.1, through the anatomical setters. It logs `[V1.8.1 REFORCO BRACOS]` or `[V1.8.1 REFORCO PERNAS]` with values before/after and the new respective reach. Each subsequent hitbox query already derives reach from the live anatomy, so no extra hitbox sync/reset is required. Whiffs and cooldown-rejected attempts **do not grant growth**, and the new change does not grant old MorphPointBank points or change sprites/colliders. The V1.7.5 damage remains 10 HP and its existing cooldown remains unchanged.

**Minimal Unity acceptance:** On branch `feature/v1.7-physics-foundation`, pull after latest GitHub source changes; `Mon Basic Strike Set` already exists on Mon in the committed scene, so no component installation is needed. In Play, temporarily disable the **EnemyMon Auto Fighter** only during the isolated test; Tab to Mon, position in contact; press F once, wait for cooldown, press V once. Observe `[V1.8.1 REFORCO BRACOS] extensao=0 -> 0.1; alcanceSoco=1.04` and `[V1.8.1 REFORCO PERNAS] extensao=0 -> 0.1; alcanceChute=1.04`. On a later failed F/V attempt beyond reach, neither attribute should increase. Repeating only F should increase arms without legs, repeating only V should increase legs without arms. Stop Play: temporary runtime growth returns to the serialized baseline zero. Confirm EnemyMon AI remains enabled outside Play. **Do not save anatomy growth test values into the scene.**

**Unity validation (2026-10-24):** user confirmed F weak punches grow Mon `Bracos.Extensao` (Inspector reached 0.7 during repeated punches), V weak kicks grow `Pernas.Extensao` (Inspector showed approximately 0.8000001 after repeated kicks); the extra float digits are ordinary floating-point approximation. A long-distance kick logged `[V1.7.5 CHUTE FRACO] ... resultado=FORA; alcance=1,32 u; danoAplicado=0` and the user confirmed legs remained at approximately 0.8. The user exited Play, confirmed both limb extensions reverted to their zero baseline and EnemyMon Auto Fighter was enabled. Because only the existing script was changed and test values were runtime-only, **no V1.8.1 scene modification or new scene commit is required**. V1.8.1 direct reinforcement is verified, but precise continuous reach change after each hit was derived from the same already-validated V1.7.4 hitbox formula rather than separately measured as a new distance-boundary test.

**Deferred:** full V1.8 requires moves to evaluate against **anatomical** requirements (Fail/Effective/Critical), weighted and potentially multi-region reinforcement, harmonization with/retirement of the provisional four-axis point bank, and behavior shared across AI/player moves. This step deliberately proves only the minimal direct-growth link before proposing a broader mapping or coefficients.

## V1.8.2 — Anatomical Fail / Effective; timing-based counter-hit Critical (UNITY VALIDATED)

**2026-10-24 user correction (supersedes previous implementation):** `Critical` is **not** earned merely because limb extension exceeds 0.75. That earlier interpretation was conceptually incorrect despite its previous Unity logs displaying the intended multipliers. `Fail` means the Mon **lacks the anatomy required to perform the movement**, so **zero damage** is dealt, but the attempt produces **small morphological reinforcement**, even if the attacker is too far from the target to geometrically connect. A non-Fail attempt that misses remains an ordinary miss, not a morphological Fail.

**Quality contract for the current F/V prototype:**
- `Fail`: relevant limb extension below the *provisional* `effectiveExtensionThreshold = 0.25`; zero outgoing damage even on geometric overlap, small growth (`0.1 * 0.5 = 0.05`) **per valid attempt**, with or without geometric contact. This does not grant legacy `MorphPointBank` points.
- `Effective`: sufficient anatomy (extension at least 0.25) and no valid critical opportunity. On geometric hit, deal base damage (`10 HP`) and apply base limb growth (`0.1`); on whiff, zero damage and **no growth**.
- `Critical`: the same anatomy eligibility as Effective, **and** a geometric hit during the opponent's explicit `attack startup` / commitment window. On hit, deal `10 * 1.5 = 15 HP` and grow limb extension `0.1 * 2 = 0.2`; the enemy's pending attack is interrupted by the normal TakingDamage transition. **No random critical roll, and no Critical solely from high extension**.

**Implementation touchpoints:** existing `MonBasicStrikeSet` distinguishes anatomy eligibility, overlap and counter-window, snapshots all inputs before damage, and emits one `[V1.8.2 GOLPE]` line covering quality/contact/window/damage/growth. `CombatantStateMachine.BeginAttackStartup` registers a timed attack preparation interval; `IsInAttackStartup` is true only while the state remains Attacking and before the startup interval expires. `AutoFighter` delays its existing universal automatic attacks by `attackStartupSeconds = 0.45 s` (provisional), using a coroutine and `CombatantStateMachine` lock; a registered health-damaging hit during startup changes the state to TakingDamage, cancels the pending attack before its damage, and creates a genuine counter-hit opportunity. A component disable during startup also cancels the pending attack. Outside this first AI startup slice, actual defensively committed windows, punish-on-recovery windows, animation telegraph, comprehensive input timing/cancel and shared timing for every player attack are **deferred**. V1.6 legacy generic move morphology still uses its original temporary points system, not this new anatomical quality contract. No additional scene component is needed.

**Immediate Unity acceptance (one test at a time):** Pull source changes; verify C# compilation. In Play, temporarily disable EnemyMon's Auto Fighter while verifying basic rules, with arms/legs extensions initially 0 and Tamer -> Mon control via Tab. In-range F once should log `[V1.8.2 GOLPE] tipo=SOCO; qualidade=Fail; dano=0; reforco=0,05; extensao=0->0,05`, leaving EnemyMon HP unchanged. Out-of-range F at low anatomy should also be `Fail`, zero damage, growth 0.05. For a quick test of Effective, temporarily set `Mon -> Mon Anatomy -> Bracos.Extensao = 0.25` **during Play only**, contact with F, and expect `Effective`, 10 HP, +0.1. Attempt a miss with sufficient anatomy: `Effective`, zero damage, zero growth. Independently confirm V/legs.

**Critical window acceptance (after Fail/Effective):** In Play, turn EnemyMon Auto Fighter back on and optionally increase `EnemyMon -> Auto Fighter -> Attack Startup Seconds` to `1.2` **during Play only** to make manual timing easy. With Mon's arms extension at least 0.25 and the characters within strike reach, wait for `[V1.8.2 JANELA] EnemyMon: INICIO`, then press F **while the startup is still open**. Expect `[V1.8.2 GOLPE] ... qualidade=Critical; contraJanela=SIM; dano=15; reforco=0,2`, followed by `[V1.8.2 JANELA] EnemyMon: ATAQUE INTERROMPIDO`. Outside the startup, even at very high extension, successful F/V strikes are Effective, **not** Critical.

**Unity validation, 2026-10-24:** user supplied runtime logs confirming (i) F/arms and V/legs Fail at extension below 0.25 caused 0 HP and +0.05 extension per valid attempt, both in-range and out-of-range; (ii) Effective at extension 0.25+ caused 10 HP and +0.1 extension on hit, but no growth/damage on whiff; (iii) repeated strikes at extension >0.75 remained Effective outside the critical window; (iv) EnemyMon AutoFighter startup at both extended **1.2 s** and default **0.45 s** yielded actual **Critical** F hits at appropriate windows: 15 HP and +0.2 extension, then `ATAQUE INTERROMPIDO` logged from AI; (v) normal F hits outside windows remained Effective at 10 HP. This verifies mechanical correctness, **not** visual readability: user expressly deferred startup tell/breach feedback to future character animations, because the current characters are primitive squares. The actual animation must eventually expose and own the action timing. **V1.8.2 validated functionally.** No commit of temporary Play Mode inspector values requested.

**Further V1.8 scope:** up-to-three anatomical influences per move, individualized anatomy requirements beyond extension, control-state/anticipation refinements, consistent AI/player move APIs, and conversion away from old 4-axis point bank. This slice is deliberately limited to functional F/V and the first real opponent startup window.

## V1.8.3 — Three weighted anatomical influences per basic strike (UNITY VALIDATED)

**1. Player-visible hypothesis:** F punch / V kick can now develop **up to three numerical anatomical parameters in one attempt**, without changing the character sprite, input, body collider, V1.8.2 Fail/Effective/Critical decision, or the four-axis V1.6 legacy bank. The **quality multiplier applies to all influences**, not just primary extension.

**2. Family and mechanic:** existing basic melee strike reinforcement, not a new move. It extends the existing feedback loop after move resolution. No special techniques, point conversion, visual transformation, new AI move catalog or input binding.

**3. Dependency and change map:** **change** anatomical storage setter/routing (`MonAnatomy`) and V1.8.2 reinforcement distribution (`MonBasicStrikeSet`); **verify** F/V contact/whiff/quality thresholds, dynamic punch/kick reach, leg-derived basic jump when leg mass increases, aggregate anatomy totals, EnemyMon startup/counter-interrupt, and unchanged scene references; **unaffected** input, Tamer/arena and point collection, main physics ownership, camera, sprite/collider, HP balance, existing generic J/K/L attacks and old visual form bank.

**4. Implementation:** no new components or `.meta` files. In existing `MonAnatomy.cs`, numeric parameter IDs currently cover `ArmsExtension`, `ArmsTenacity`, `ArmsMass`, `LegsExtension`, `LegsTenacity`, `LegsMass`, with `MonAnatomy.AddInfluence(id, delta)` routing through validated mutation/event logic. In `MonBasicStrikeSet.cs`, the existing serialized `Arm Extension Gain Per Punch Hit = 0.1` and `Leg Extension Gain Per Kick Hit = 0.1` remain the **primary** influence. Four new configurable secondary/tertiary influence definitions and `Enable Additional Anatomical Influences = true` provide an initial 3-influence proof. No double application of primary growth. Experimental defaults, **not fixed balance or a general rule that every punch must gain mass**:

| F/V move | Primary | Secondary | Tertiary |
|---|---|---|---|
| F / weak punch | `ArmsExtension +0.10` | `ArmsTenacity +0.04` | `ArmsMass +0.02` **per arm** |
| V / weak kick | `LegsExtension +0.10` | `LegsTenacity +0.04` | `LegsMass +0.02` **per leg** |

All three apply the same multiplier from V1.8.2: **Fail x0.5 per valid attempt even on whiff**, **Effective x1 on hit only**, **Critical x2 on hit only**. Therefore initial Fail F gives respectively `+0.05/+0.02/+0.01`, Effective F hit gives `+0.10/+0.04/+0.02`, Critical F hit `+0.20/+0.08/+0.04`; V/legs use equivalent values. Because arm/leg mass is stored **per member** and quantity normally equals 2, an F Fail gain of `ArmsMass +0.01` increases `MassaGeral` by **0.02**. No automatic effect on weak-strike damage beyond V1.8.2; leg mass **already** changes `MonAnatomicalJumpStats` if active and should be checked as an expected downstream functional side effect, not suppressed.

**5. Observability:** filter Console for `V1.8.3 MORFOLOGIA`; each valid anatomical reinforcement logs move type, quality, all three applied deltas, MassaGeral, and TenacidadeGeral. V1.8.2 `[GOLPE]` log remains for contact/damage and its `reforco` field continues to mean **primary limb extension only**, not sum across heterogeneous units. Duplicate influences targeting the same parameter, if later configured in Inspector, naturally accumulate; there is no artificial normalization or total pool yet.

**6. First Unity acceptance (one action at a time):** after `git pull --ff-only`, confirm no Unity compile errors and `Mon -> Mon Basic Strike Set` lists secondary `ArmsTenacity, Amount 0.04`, tertiary `ArmsMass, Amount 0.02` and checkbox enabled. In Play Mode only, turn off EnemyMon AutoFighter, ensure Mon Bracos.Extensao/Massa/Tenacidade all start at zero, switch to Mon and press **F once**, preferably outside enemy contact. Expect `[V1.8.3 MORFOLOGIA]` with `qualidade=Fail; primaria=ArmsExtension +0.05; secundaria=ArmsTenacity +0.02; terciaria=ArmsMass +0.01; massaGeral=0.02; tenacidadeGeral=0.04` (assuming other mass/tenacity sources zero and arm quantity two). Enemy HP stays unchanged. Failing this first step should be diagnosed **before** other cases. Later validate V independently, normal Effective hit, Critical startup and relevant jump side effect. Exit Play and check that original scene values revert. Do not commit temporary Inspector modifications.

**7. Versioning / known limits:** same branch `feature/v1.7-physics-foundation`; only existing `MonAnatomy.cs` and `MonBasicStrikeSet.cs` changed. Until Unity confirmation status remains source-committed/unvalidated. Current primary slot is intentionally fixed to the corresponding limb extension; secondary/tertiary are inspector-configurable over six numerical limb parameters; full cross-regional typed morphology routing, per-move data catalog, growth balancing, growth from other movement families, future concentration and AI anatomy use remain deferred. This is a bounded technical proof that avoids claiming the full anatomical influence system is complete.

**Unity validation (2026-10-24):** User's test logs confirmed F/arms Fail `+0.05 extension / +0.02 tenacity / +0.01 mass`, Effective `+0.10 / +0.04 / +0.02` and Critical `+0.20 / +0.08 / +0.04`. V/legs logs confirmed both Fail and Effective, with Inspector showing per-leg mass 0.19, tenacity 0.38, extension approximately 0.95 after 5 Fail + 7 Effective attempts. `MassaGeral`/`TenacidadeGeral` logs tracked the expected contributions times limb quantity. User separately confirmed by Play Mode comparison with **leg extension held fixed** that changing leg mass changes the basic jump response. No quantitative jump apices or purely isolated mass changes captured in logs, so this is a **qualitative functional validation**, not final propulsion/inertia calibration. V1.8.3 is validated for F/V weighted reinforcement and downstream leg-mass jump linkage. No requirement to save temporary runtime scene values.

**Next V1.8 question (not implemented):** define how multiple anatomical dimensions gate move capability, rather than using only limb extension for F/V Fail-to-Effective. Keep this separate from distributing rewards: a move may require one or several independent body capabilities; deciding exact dependency and threshold should precede implementing a generic evaluator. Do not modify damage/critical timing until that is defined.

## V1.8.4 — All-required anatomical prerequisites for F/V (UNITY VALIDATED)

**Decision, 2026-10-24:** user chose **every mandatory requirement** of a particular movement (logical AND, never compensation). A highly developed extension cannot cover inadequate tenacity or mass. This does **not** mean every parameter in the whole Mon is a prerequisite for each action.

**1. Player-visible hypothesis:** The F punch is `Fail` until **arms extension AND arms tenacity AND per-arm mass** all meet the provisional F criteria. V kick independently uses the corresponding leg parameters. A strike can have high extension but still deliver **0 HP** if tenacity or mass is lacking. Repeated Fail attempts (including misses) still provide small three-parameter development, ultimately unlocking Effective after all minima are met. A fully capable strike meeting the enemy's attack startup still becomes Critical by timing; it never becomes Critical just from abundant anatomy.

**2. Movement/mechanic:** existing weak punch and weak kick; **only eligibility calculation changes**. Inputs F/V, physical hitbox and reach, damage multipliers, weighted reinforcement, cooldown, character visuals, transformations, physics, camera, Tamer systems, enemy automatic attacks and legacy V1.6 universal move scoring are unchanged.

**3. Source touchpoints:** `Assets/_Project/Scripts/Combat/MonBasicStrikeSet.cs` reads `MonAnatomy`'s per-limb `extensao`, `tenacidade`, `massa` **before** any damage/reinforcement and applies logical AND in the existing `EvaluateStrikeQuality`. No new component, script, or `.meta` is required. There are two new Inspector-configurable test minima beside the existing extension minimum. `[V1.8.4 REQUISITOS]` shows actual vs minimum for each parameter, OK/FALTA, and eligible SIM/NAO. A very small comparison tolerance (`0.00001`) covers float rounding after repeated incremental gains.

| Move | Extension >= | Tenacity >= (per limb) | Mass >= (per limb) |
|---|---:|---:|---:|
| F soco, Bracos | 0.25 | 0.10 | 0.05 |
| V chute, Pernas | 0.25 | 0.10 | 0.05 |

**All numerical requirements are provisional test defaults, not biological rules or final balance.** With the already validated V1.8.3 Fail gains `+0.05, +0.02, +0.01`, all three minima are normally crossed after five unsupported attempts from a zero baseline, allowing the **sixth** contact to be Effective. Turning off secondary/tertiary growth in Inspector can leave the missing required parameter permanently under its minimum until some other action provides it; this is expected under the non-compensating rule.

**4. First Unity acceptance, single missing attribute:** with Unity closed, `git pull --ff-only`, then check compile and `Mon Basic Strike Set` thresholds. Enter Play; temporarily disable EnemyMon AutoFighter and select Mon through Tab. While **in Play only**, set `Mon -> Mon Anatomy -> Bracos.Extensao = 0.25`, `Bracos.Tenacidade = 0.10`, and `Bracos.Massa = 0.04`. Position Mon close enough to hit EnemyMon; press F **once**. Expected `[V1.8.4 REQUISITOS] golpe=SOCO; extensao=0.25/0.25 OK; tenacidade=0.10/0.10 OK; massa=0.04/0.05 FALTA; elegivel=NAO`. The corresponding `[V1.8.2 GOLPE]` should say `qualidade=Fail; dano=0` even on contact; the already-validated `[V1.8.3 MORFOLOGIA]` should award `+0.05/+0.02/+0.01`, bringing per-arm mass to 0.05 (and other parameter gains). After cooldown, another F in range should be eligible `SIM` and `Effective` with 10 damage. Test only first Fail before proceeding, and share one requirements and one hit log. Stop Play to revert Inspector edits. Do not save temporary scene data or modify unrelated TextMesh Pro files.

**5. Later checks:** keep extension under threshold but other two >=, and repeat with tenacity missing; V/legs independent; eligible whiff still no damage or reinforcement; legitimate counter-hit remains Critical with correct prerequisites. Special/composite moves with richer requirements, deciding physiological meaning of thresholds, coefficient tuning and validated visuals remain future work. Once user confirms all V1.8.4 tests, update this note to UNITY VALIDATED.

**Unity validation (2026-10-24):** User confirmed a punch whose arm mass alone fell below threshold logged `elegivel=NAO` despite sufficient extension and tenacity, and the next punch after minimal Fail growth logged `Effective`, contact, 10 HP damage and +0.1 extension. Kicks from a zero baseline logged 5 successive `Fail` attempts and became eligible on attempt 6; the contact was `Effective` at 10 HP. Additional inspector-only tests verified extension alone below its raised minimum blocked kicks (`0.55/0.60 FALTA`) and tenacity alone below its raised minimum blocked kicks (`0.24/0.30 FALTA`). Finally, with all baseline requirements restored, enemy AI attack startup at 1.2 s produced repeated actual kick `Critical` hits (`contraJanela=SIM`, damage 15, extension +0.2) and `ATAQUE INTERROMPIDO`. Functional scope validated; thresholds/weighting remain experimental, inspector changes in Play should not be saved.

## V1.8.5 — Ground walking speed derived from leg extension and total mass (UNITY FUNCTIONALLY VALIDATED)

**Purpose:** validate PHY-087 with the smallest playable slice: leg extension improves normal grounded locomotion speed while whole-body mass impedes it. The motor must apply this for **both** manual and automatic movement without requiring duplicative computation in `MonPlayerController` or `AutoFighter`.

**Change scope:** only `Assets/_Project/Scripts/Control/MonPhysicsController2D.cs` code changes. Existing `MonAnatomy` is read live on every physics step through `pernas.extensao` and `MassaGeral`. The motor still receives direction plus the caller's already-modified requested speed; it scales that speed **only when grounded**, before writing `Rigidbody2D.linearVelocity.x`. Existing gravity, jump impulse, attack hitboxes, transformation, knockback-control window, collider and scene are unaffected; airborne horizontal control is deliberately unchanged pending PHY-089 validation. Works without adding new components or serializing scene changes.

**Provisional equation, not final physiological/balance model:** `groundFactor = clamp((1 + pernas.extensao * legExtensionSpeedGain) / sqrt(1 + MassaGeral * generalMassSpeedPenalty), minimumGroundSpeedMultiplier, maximumGroundSpeedMultiplier)`. Defaults: `legExtensionSpeedGain=0.25`, `generalMassSpeedPenalty=0.08`, min `0.5`, max `2.0`. A zero-anatomy baseline gives x1. A player with default base speed 3 and extension 2, mass zero gets x1.5 (4.5 u/s); adding 5 mass **per leg** with 2 legs makes total mass 10, so factor is about x1.118 (3.354 u/s). These are illustrative sandbox comparisons, **not target movement balance**. The old `MonFormGameplayStats` speed modifier continues to multiply requested speed upstream, so this new anatomical factor composes with, rather than silently replacing, V1.6 features.

**Diagnostics:** `[V1.8.5 CAMINHADA]` logs leg extension, total mass, input/controller-requested base speed, current factor and grounded desired speed on first grounded walk and when parameters change enough to affect it. `MonPhysicsController2D` also exposes a context-menu action `V1.8.5 - Mostrar velocidade de caminhada` for spot inspection without constant Update logs.

**First Unity acceptance, ONE variable at a time:** close Unity before `git pull --ff-only`; open ArenaPrototype and verify no compilation errors. During **Play only**, disable EnemyMon AutoFighter to simplify positioning, use Tab to control Mon and ensure `Mon Anatomy -> Pernas -> Extensao = 0` and whole-body mass baseline = 0. Hold right arrow on grounded Mon: expected `[V1.8.5 CAMINHADA]` factor x1 and speed 3 u/s (provided `moveSpeed=3` and legacy form modifier x1). Then **without leaving Play**, set `Pernas.Extensao = 2`, hold right again, expect factor x1.5 and speed 4.5 u/s. **Only after confirming extension**, set `Pernas.Massa = 5` (2 legs, other region masses zero); expect `MassaGeral=10`, factor about x1.118 and speed 3.354 u/s. Compare actual horizontal travel/velocity as practical. Changes must revert after exiting Play; do not commit inspector test values. Later check AI also uses same motor, current combat/air/jump regressions and no compile errors.

**Known limits:** direct inspector changes are read live even without `OnAnatomyChanged`; current motor still permits immediate aerial horizontal reversal inherited from V1.7.1, to be addressed in a separate PHY-089/090 slice. Terrain-dependent traction, drag/gliding, additional jumps, a unified anatomical move-evaluation registry, final movement curves/caps and visuals are deferred.

**Unity functional validation (2026-10-24):** On grounded manually controlled Mon, logs showed `pernasExtensao=0; massaGeral=0.08; velocidadeBase=3; fator=x0.997; velocidadeSolo=2.990 u/s`. At unchanged mass 0.08 and extension 2, speed became 4.486 u/s (factor x1.495). At unchanged extension 2 and mass 10.08, speed became 3.348 u/s (factor x1.116). When auto-controlled, a log with `velocidadeBase=2; pernasExtensao=2; massaGeral=20.08` showed factor x0.929 and speed 1.858 u/s; user confirmed intentionally increasing per-leg mass from 5 to 10, accounting for the extra 10 in total mass. Core locomotion/morphology relationships and shared motor path are validated through telemetry. Still pending separate air-steering, landing and broader combat regressions; the coefficients are not final balance.

## V1.8.6 — Partial airborne steering and normal landing stop (CODE COMMITTED; UNITY TEST PENDING)

**User-established rules:** PHY-089: Mon can correct horizontal trajectory during jumps but retains existing momentum; on-ground inputs remain immediate. PHY-090: normal landing with no horizontal input stops horizontal movement immediately, without adding a mass-dependent slide. Existing combat knockback and state locks retain priority. Test only the shared motor for now, without adding double jump, glide, terrain adhesion or new commands.

**Implementation:** only `Assets/_Project/Scripts/Control/MonPhysicsController2D.cs`. The existing motor continues to use the **same live anatomy-derived target horizontal speed** from V1.8.5 (leg extension and total mass) for both manual and AI control. The difference is **response**, not a second movement formula: when grounded (including landing), write target speed directly; in air, gradually approach target via `Mathf.MoveTowards(currentX, desiredX, rate * Time.fixedDeltaTime)`. With directional input, rate is the experimental `airSteeringAcceleration = 8 u/s²`; when released, use the slower `airReleaseDeceleration = 2 u/s²`, retaining some horizontal momentum. These are adjustable Inspector hypotheses, NOT locked balance numbers or literal air drag physics. Do not change jump/gravity formulas, body colliders, sprites, hitboxes, Tamer movement, input mapping, or AI/Player controller ownership. Normal combat state lock still stops input-driven motion; temporary forced knockback control still bypasses the new interpolation until its lock ends.

**Observability:** `[V1.8.6 AR]` logs on first airborne physics tick and input changes (direction/neutral/opposite): `entrada`, horizontal `velAntes` and `velDepois`, anatomy-adjusted target and rate. `[V1.8.6 POUSO]` logs one landing transition and reveals whether zero input causes `velDepois=0`. V1.8.5 ground-walk logs continue unchanged in meaning. No per-frame new log stream.

**Unity test (one at a time):** close Unity, `git pull --ff-only`, check compile and scene. Play with Mon's anatomical values restored to ordinary baseline; disable EnemyMon AutoFighter temporarily if useful. Switch to Mon by Tab, stand away from walls, **hold Right** until moving and then, **while keeping Right pressed, press Up to jump**. Filter Console `V1.8.6 AR`. First airborne entry should show positive nonzero `velAntes` and a new `velDepois` close to it rather than abruptly zero. Share that first `AR` log. After this passes, separately test releasing Right mid-air (speed decays gradually, not instantaneous zero), reversing to Left (speed changes gradually), and landing with no input (`POUSO semComando=SIM; velDepois=0`). On the ground, immediate input reversal and V1.8.5 anatomy speed must still work. Physics collision and jumping over EnemyMon, camera switching and AI movement should also remain intact. Some tests need sufficient runway/jump duration to observe reversal; avoid walls and opponents. Stop Play before committing scene modifications.

**Known limitations:** no measured tuning of acceleration/deceleration yet; airborne steering is not an independent morphology ability, but its horizontal target retains V1.8.5 anatomy influence. No powered flight, air drag taxonomy, extra jump or glide; no new visual animation. Hardware gamepad input validation remains pending. This slice is not complete until actual Unity validation.

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
