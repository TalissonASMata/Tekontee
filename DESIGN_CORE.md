# Tekontee — Core Design Notes

This file records gameplay design decisions that should survive across conversations and implementation sessions. Treat it as canonical design context together with `AI_CONTEXT.md` and `ROADMAP.md`.

## 1. Design premise

Tekontee is not primarily about choosing a form from a list. The Mon's body should be shaped by how the player fights.

The defining loop is:

Current morphology -> move execution quality -> morphological reinforcement -> changed morphology -> changed future move execution.

The Tamer and arena systems feed and influence this loop rather than replacing it.

## 2. Universal moveset

All Mons conceptually have access to the same base catalog of moves.

Working examples:

- Headbutt
- Bite
- Peck
- Scream
- Guard
- Parry
- Jump
- Double jump
- Glide
- Brace / Retesar
- Low kick
- High kick
- Quick punch
- Heavy punch
- Dash / Advance
- Retreat
- Grab
- Special techniques

The identity of a Mon should come from how well its current body can execute these moves, not from an arbitrary species-specific move list.

### Input device compatibility (permanent requirement)

- The universal moveset must be fully playable with keyboard and modern gamepads, including Xbox and PlayStation (PS5/DualSense-class) controllers.
- Use Unity Input System actions and device-independent gameplay commands. Bind keyboard and gamepad controls to the same actions; do not hard-code keyboard keys in move definitions or duplicate move logic per controller type.
- Support coherent movement, attack selection, defensive actions, Tamer/Mon switching and gameplay interactions on both input methods as their features are implemented.
- Keep mappings configurable/remappable where practical, and use controller-agnostic action names rather than assuming Xbox or PlayStation button labels.
- **Hard input-complexity limit for individual normal moves:** one button or at most two simultaneous buttons (a two-button chord). No normal move requires three simultaneous buttons or fighting-game motion inputs.
- Examples of acceptable controller chords: `R1 + Square` on PlayStation, `LT + A` on Xbox; provide equivalent keyboard bindings via the same Input Actions.
- A two-button combination remains a **normal universal move**, not automatically a special technique. This applies equally to offensive, defensive, mobility and support moves.
- **Special techniques are compositions/sequences of normal moves or states**. A special may involve more than two actions across time, but must not require more than two buttons simultaneously at any instant. Do not conflate a multi-step special recipe with a three-button chord.
- Illustrative PlayStation mapping (not a final binding): Square = weak hit; R1 = dodge; R2 = scream; L1 = block; L2 = brace/retesar; R1 + Square = teleport strike; R2 + Square = Battle Cry; L1 + Square = parry; L2 + Square = grab. Xbox/keyboard must expose equivalent semantic commands; physical button labels are examples only.
- Illustrative special **Water Shot**: Brace/Retesar -> Battle Cry -> Triangle (elemental action). The recipe combines existing movements and an elemental action to launch a water sphere across the arena. Its timing/state windows and morphological prerequisites are future design decisions; it is **not** L2 + R2 + Square + Triangle held simultaneously.
- Maintain input priority/consumption so a chord does not accidentally trigger both individual moves. Special recognition should operate on semantic action/state events, independently of controller model.
- V1.6 should establish and validate the shared input path for the first two test moves, rather than postponing controller compatibility until the full catalog exists.

### Current approved keyboard layout (2026-10-21)

The following is a **user-selected control contract**, not proof all listed actions are implemented:

| PlayStation concept | Keyboard | Planned action |
|---|---|---|
| Square (□) | F | Weak punch |
| Cross (✕) | V | Weak kick |
| Triangle (△) | G | Strong punch **with intrinsic element** |
| Circle (○) | B | Strong kick **with intrinsic element** |
| R1 | / | Shoulder action, gameplay semantics pending |
| R2 | ; | Shoulder action, gameplay semantics pending |
| L1 | E | Shoulder action, gameplay semantics pending |
| L2 | R | Shoulder action, gameplay semantics pending |

**Current prototype behavior (control correction preceding V1.7.5):**
- Horizontal movement for **both Mon and Tamer**: only keyboard **Left Arrow / Right Arrow**; WASD keys must not move either character.
- **Mon jump:** exclusively **Up Arrow on keyboard**. Space, W, and the old gamepad Triangle / `buttonNorth` shortcut no longer cause Mon jump. The definitive gamepad jump action is still **unassigned**, rather than invented or overlapping the planned strong punch (Triangle).
- **Tamer jump:** existing **Up Arrow or Space** retained, but **W** removed. Its future definitive mapping can be reviewed separately without changing the Mon rule.
- **Battle restart:** **Enter** only after the fight has ended; remove legacy R-restart. Enter is no longer a manual HeavyStrike alias.
- **Mode switch:** Tab stays. Existing J/K/L universal test strikes and their gamepad prototypes stay temporarily for V1.6 regressions until the new attack actions are validated.
- **Existing Tamer collection:** E still collects in the current prototype; reserving E as L1 is a future semantic control-layer migration, not an implicit removal of collection. R is reserved for L2 and currently does not trigger a gameplay action. Q's prototype Intensify remains.
- **F/V/G/B and shoulder commands** are the agreed **future bindings** for the new moves, not implemented attacks yet. New bindings should be routed through semantic Input System actions and tested for keyboard/gamepad equivalence, not directly conflated with the three V1.6 test moves.

## 3. Morphology requirements and execution quality

Every move may define morphology requirements.

A move resolves in one of three quality levels:

### Fail

The Mon attempts the move without enough supporting morphology.

- The action still happens in degraded form when appropriate.
- It grants only minimal morphology reinforcement.
- Persistent reliance on unsupported moves is intended to contribute to the future Corrupted direction.

### Effective

The Mon meets the intended morphology requirement.

- The move behaves normally.
- It grants medium morphology reinforcement.

### Critical

The Mon strongly exceeds or optimally matches the morphology needed for the action.

- The move reaches its strongest controlled result.
- It grants maximum morphology reinforcement.
- Repeated highly concentrated development contributes conceptually toward the future Extremist direction, but this relationship should not be implemented as a simplistic direct counter.

## 4. Weighted morphology reinforcement

A move or special must be able to reinforce up to three morphology parameters at once.

Conceptual tiers:

- Primary / intense influence
- Secondary / medium influence
- Tertiary / basic influence

Example only:

Critical Headbutt:

- Head.Weight — intense
- Head.Hardness — medium
- Torso.Weight — basic

The amount actually applied depends on execution quality: Fail < Effective < Critical.

This architecture is important because future morphology is multidimensional. Avoid designs where every move can only award one attribute.

## 5. Target anatomical morphology

Current working model is evolving; the former approximately 18-parameter sketch is **not a fixed count**. PHY-017 establishes **Tenacidade** (quantitative impact resistance) and **Revestimento** (surface traits, not a numeric armor stat) in every anatomical region. Earlier terms `Dureza` and `Couraça` are superseded.

### Head

- Revestimento
- Weight
- Tenacidade
- Fang/Beak capability
- Intellect

### Torso

- Revestimento
- Weight
- Tenacidade
- Extension / `Tronco.Extensão` (PHY-098: body projection contributes to punch and kick reach)
- Coverage (pending: distinguish this from Revestimento)

### Arms

- Revestimento
- Weight (mass **per arm**, identical for all arms; total arms mass = per-arm mass × Quantity)
- Tenacidade
- Extension
- Drag
- Quantity

### Legs

- Revestimento
- Weight
- Tenacidade
- Extension
- Drag
- Quantity

### Tail

- Revestimento
- Weight
- Tenacidade
- Extension

### Element

- Elemental affinity/state

Confirmed leg gameplay mappings (PHY-015):
- `Legs.Quantity` → surface adhesion, knockback resistance and slipping on slopes/vertical surfaces.
- `Legs.Weight` → leg-attack damage together with leg tenacidade; total leg mass also contributes to global mass.
- `Legs.Extension` → displacement and movement speed.
- `Legs.Drag` → falling speed/aerial resistance, **not** personal gravity.
- `Legs.Tenacidade` and tenacidade for every anatomical region are confirmed under the renamed PHY-017 schema. Arms and legs share tenacidade across their respective repeated limbs.

Element may need different rules from anatomical values and should not be forced into exactly the same mathematical model if gameplay testing shows that is inappropriate.

**Global **numeric** totals are sums of the contributions of their anatomical regions**, not averages. For repeated arms, total arms mass = per-arm mass × arm quantity (PHY-012); do not treat the per-arm value itself as the region's total. Repeated legs follow the same per-leg mass × leg quantity rule (PHY-014). TenacidadeGeral also sums the tenacity contributions of anatomical regions including repeated members (PHY-018). Revestimento and Tenacidade are separate placeholders until their respective effects are defined. The exact parameter list is still designable. The architectural requirement is that the system support many independent morphology dimensions grouped by body region.

## 6. Morphology should emerge from action

The player should not directly press a button to select a morphology form.

Instead, the player chooses behavior and actions. Those actions reinforce morphology. The resulting body then changes which actions become effective or critical.

Two players using offensive strategies may therefore produce very different Mons if their movement patterns reinforce different body parameters.

## 7. Special techniques

Special techniques should require more advanced morphology combinations than basic moves. They are sequences/compositions of normal universal moves or states, not merely any two-button chord. Each step respects the maximum two simultaneous buttons rule.

This prevents a simple self-feeding loop such as repeatedly using a projectile to gain the same attribute required to keep spamming that projectile.

Specials should be unlocked or become effective because the body has developed enough relevant morphology, potentially across multiple parameters.

## 8. Original morphology

Each Mon enters a battle with a morphology baseline called `OriginalMorphology`.

This baseline represents the form/state at the start of the match and must be stored separately from the morphology developed during the battle.

## 9. Morphological reversion / debuff

Certain opponent effects should pull the target's current morphology back toward `OriginalMorphology`.

Important: this is a return force toward the baseline, not a force pushing the target farther away from its original morphology.

Example:

Original Arms.Extension = 0.30
Current Arms.Extension = 0.80
A reversion effect moves 0.80 toward 0.30.

If current value is below the original baseline, reversion moves it upward toward the original value.

This mechanic lets players partially dismantle an opponent's in-match specialization.

## 10. Corrupted / Balanced / Extremist meta-axis

Future high-level transformation should use a separate meta-axis:

Corrupted <-> Balanced <-> Extremist

This axis is not just another body parameter. It describes the pattern by which the Mon has developed.

### Corrupted

Associated with persistent use of actions the current morphology cannot properly support.

Exact gameplay identity and benefit are intentionally unresolved. Corrupted must not become merely a punishment state; it should ultimately have a distinct strategic identity.

### Balanced

Broad, controlled development and reliable command over many moves.

Current concept: able to maintain approximately up to 9 parameters at medium development with strong control over actions.

### Extremist

Very concentrated development in a small number of dimensions.

Current concept: maximize approximately up to 3 parameters, gaining extreme performance at the cost of reduced control or more autonomous/uncontrolled action tendencies.

Do not implement this axis until the base morphology, weighted reinforcement and move-quality systems produce enough real behavior for the axis to be derived meaningfully.

## 11. Concentration

Simple percentage normalization of four point totals is not sufficient by itself because the highest absolute total remains the highest percentage.

Long-term concentration should measure how morphology is distributed across many dimensions and how specialized the Mon has become.

Avoid one single global pool if it causes unrelated anatomy to compete artificially, such as Tail.Extension reducing Head.Hardness merely because both share one normalized budget.

Concentration may later be evaluated by region, by groups of compatible parameters, globally for meta-state, or by a combination of these approaches.

## 12. Current prototype compatibility

Current temporary morphology uses four axes:

- Attack
- Defense
- Air
- Energy

These are placeholders used to validate architecture.

V1.4 should first prove weighted multi-influence using these four axes. Later versions can replace them with anatomical dimensions without rewriting the core move -> evaluation -> reinforcement pipeline.

## 13. Tamer relationship

The Tamer operates in a parallel arena/platform layer and affects morphology opportunities through collection and future environment interactions.

Current validated EnemyTamer makes arena resources contested.

Future Tamer systems may:

- create or expose morphology opportunities;
- expand or alter the arena;
- potentiate resources;
- support the Mon;
- interfere with the opponent pair.

The Tamer should reinforce the main morphology loop, not become an unrelated minigame.

## 14. Development rule

Implement the smallest milestone that proves the next mechanic.

Do not prematurely implement:

- full 18-parameter anatomy before weighted influences and execution quality are validated;
- final Corrupted/Balanced/Extremist formulas before the underlying behavior exists;
- production-scale data architecture before the prototype proves the loop.

When making gameplay code changes, preserve already validated camera, combat, control-switching and battle behavior unless the active milestone intentionally changes them.
