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

Current working model is approximately 18 parameters:

### Head

- Weight
- Hardness
- Fang/Beak capability
- Intellect

### Torso

- Weight
- Hardness
- Coverage

### Arms

- Weight
- Extension
- Drag
- Quantity

### Legs

- Weight
- Extension
- Drag
- Quantity

### Tail

- Weight
- Extension

### Element

- Elemental affinity/state

Element may need different rules from anatomical values and should not be forced into exactly the same mathematical model if gameplay testing shows that is inappropriate.

The exact parameter list is still designable. The architectural requirement is that the system support many independent morphology dimensions grouped by body region.

## 6. Morphology should emerge from action

The player should not directly press a button to select a morphology form.

Instead, the player chooses behavior and actions. Those actions reinforce morphology. The resulting body then changes which actions become effective or critical.

Two players using offensive strategies may therefore produce very different Mons if their movement patterns reinforce different body parameters.

## 7. Special techniques

Special techniques should require more advanced morphology combinations than basic moves.

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
