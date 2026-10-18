# Tekontee — Gameplay Physics Model

Status: **iterative design contract**. Record decisions one at a time. Do not turn proposals into settled rules until the user explicitly chooses them.

## Decision log

### PHY-001 — Physics style (DECIDED, 2026-10-18)

- **Chosen baseline:** **stylized physics**.
- Physical properties and interactions should be **legible and internally coherent**, so players can infer how morphology changes movement, contacts and attacks.
- **Arcade tuning is allowed and encouraged when playtesting shows it improves the match**, execution feel, responsiveness, fairness or predictability.
- The priority is gameplay and the player's relationship with move execution, **not strict real-world simulation**.
- Do not choose a fixed percentage of “stylized versus arcade” before testing. Make exceptions locally and document why.
- **Not decided yet:** gravity model, units/scales, ground/platform/object collision contracts, mass-to-impulse mapping, drag, friction, restitution, detailed hitboxes and formulas.

### PHY-002 — Universal arena gravity (DECIDED, 2026-10-18)

- **One uniform, constant gravitational acceleration for all Mons in the arena.**
- Differences in jumping and falling behavior arise from **individual morphology and mechanics**, such as propulsion/impulse, aerodynamics/drag and controlled airborne actions, not per-Mon gravity values.
- Under the same gravity and neglecting drag, increased mass alone does **not** accelerate free fall.
- A future explicit power or environmental effect could alter local gravity as an exception; it is **not part of the baseline**.
- Gravity magnitude, coordinates, implementation and tuning **remain undecided**.

### PHY-003 — Collision normals with separate Mon/Tamer movement (DECIDED, 2026-10-18)

- **Option B adopted:** use contact normals to classify ground, walls, slopes and platforms; the first validation may use only horizontal surfaces.
- **Shared collision/contact principles** for Mon and Tamer (normal, support/groundedness, blocking/contact response), but **different gameplay movement controllers**.
- **Mon:** predictable side-view fighting-game movement and arena constraints, with Street Fighter as a reference for combat feel.
- **Tamer:** platforming movement with jumping between platforms and future possibilities such as sliding, ramps and momentum, with Mega Man X, Sonic and Symphony of the Night as experiential references.
- Contact normals **enable slope recognition** but do not automatically implement Sonic-like acceleration, friction, momentum or sliding; those mechanics must be decided and tested separately.
- Do not build all Tamer platforming mechanics now; preserve the current validated movement/camera/combat loop and add the smallest collision slice when implementation requires it.
- **Not yet decided:** walkable angle threshold, one-way platform behavior, grounding tolerances, collision layer policy, slope forces and friction models.

### PHY-004 — Platform collision types (DECIDED, 2026-10-18)

- **Option C adopted:** the arena supports both **fully solid platforms** (block top, bottom and sides) and **one-way platforms** (can pass upward from below and land from above).
- Platform collision behavior is an explicit per-platform property, not inferred from sprite/artwork or automatically tied to a character.
- This decision is a capability requirement; no need to implement all platform variants during the present V1.7 physics-design stage.
- **Not yet decided:** whether the player can deliberately drop through one-way platforms, and the relevant input and collision rules.

### PHY-005 — Intentional drop-through (DECIDED, 2026-10-18)

- **Option A adopted:** while standing on a **one-way platform**, Tamer can press **Down + Jump** to intentionally descend through it.
- The command must not cause a normal upward jump simultaneously.
- The one-way collision is ignored temporarily for the supporting platform until the Tamer has cleared it; do not globally disable collisions with other solid surfaces.
- Fully solid platforms remain solid and cannot be dropped through by this command.
- The precise input mapping for gamepad and detailed timing/collision implementation are postponed until this mechanic is implemented and tested.

### PHY-006 — Per-object collision and physical behavior (DECIDED, 2026-10-18)

- **Option C adopted:** objects in the arena can have distinct physical properties and reactions according to their type.
- A given object can be **static and solid** (blocks movement), **dynamic/movable** (responds to forces, pushing or impacts), or **non-blocking / pass-through** where appropriate.
- Object behavior is configured per object/type, not universally inferred from appearance.
- This is a capability and design requirement; no immediate demand to implement all categories or detailed interactions.
- **Not yet decided:** specific reactive object taxonomy, force formulas, damage/destruction, object-to-morphology conversion, and interactions triggered by Tamer versus Mon.

### PHY-007 — Object reactions by contact and/or explicit actions (DECIDED, 2026-10-18)

- **Option C adopted:** each object may react to **physical contact/forces**, **explicit Tamer/Mon actions**, or **both**, according to its defined properties.
- Contact-driven responses (e.g. being pushed or struck) and action-driven responses (e.g. activating an object to release morphology opportunities) are distinct triggering mechanisms and may coexist on the same object.
- Avoid implementing an interaction separately for each object; use reusable trigger/response capabilities where appropriate.
- This is a design contract, **not** authorization to implement every reaction now.
- **Not yet decided:** the full list of physical reaction types, interactions by Mon vs Tamer, and any object damage/destruction model.

### PHY-008 — Conditional collision between Mons (DECIDED, 2026-10-18)

- **Option C adopted:** by default, Mon bodies physically block each other, as in traditional fighting games; specific abilities/states can temporarily disable this body-to-body blocking so the Mons can pass through each other.
- User examples: **invisibility or camouflage techniques that also remove physical contact**. This is the intended gameplay effect of those techniques, not an assertion that optical invisibility or ordinary camouflage physically causes intangibility.
- Keep **visibility**, **targeting/detection**, **body collision**, and **attack hitboxes/hurtboxes** as independently configurable gameplay effects. An ability may combine multiple effects when its design explicitly requires it.
- Whether the pass-through state also prevents receiving attacks, colliding with objects/walls, or being detected by AI is **not yet decided**. Do not silently infer general invulnerability from no body contact.
- No need to implement special abilities in the current PHY decision stage.

### PHY-009 — Morphology-derived impact and gameplay attributes (DECIDED, 2026-10-18)

- **Option C adopted:** an attack defines a tunable baseline impact; the resulting impact and knockback depend on **attacker morphology and motion** as well as **defender morphology and physical response**.
- Relevant attacker inputs include **mass/weight of the striking body part** and **its velocity/acceleration during the attack**. Defender inputs include **mass/weight**, **hardness**, and potentially other applicable defensive/body parameters.
- **Crucial design principle:** gameplay attributes such as **health, attack, defense, speed and agility must be derived from morphology** (possibly alongside action and environmental conditions), not advanced as unrelated independent point systems. Do not equate an anatomical parameter to an abstract stat 1:1 without a reason.
- Distinguish physical quantities: **mass** changes inertial response; **hardness** affects contact/damage response and does **not automatically imply larger resistance to displacement**. Strike impulse, damage and knockback may be related but are distinct outputs.
- Baseline action properties and tuning factors can exist for stylized/arcade playability; they are not independently accumulated RPG attack/defense progression.
- **Not yet decided:** formulas/units, whether to use acceleration directly or derive contact speed, exact damage/health scaling, damage distribution by body part, and which additional morphology parameters influence each derived stat. Decide through vertical-slice tests rather than arbitrary assumptions.
- **No change to the existing V1.6 combat code yet:** this is the physical/gameplay architecture target for the V1.7+ evolution.

### PHY-010 — Global health, localized defense and attack (DECIDED, 2026-10-18)

The user's chosen model deliberately avoids separate per-limb constitution/HP tracking:

- **Life / Max HP:** derived from **overall shell/armor (couraça geral)** and **overall body mass (massa geral)**.
- **Defense:** derived from **shell/armor of the involved defending body part**, **the defender's defensive action/state**, and **overall body mass**.
- **Attack:** derived from **mass of the attacking body part** and **hardness of that same part**.
- These are morphology-derived gameplay results, **not separately earned Attack/Defense/HP progression points**.
- There is **one global life/HP measure**, not a separate HP pool for each body part. Limb-specific parameters may still affect action/defense quality.
- Movement velocity/acceleration and impact dynamics remain relevant under PHY-009; PHY-010 identifies core attribute inputs, not a complete damage equation.
- **Not yet decided:** formulas, aggregation of per-part shell into general shell, choice of defensive body part, armor penetration, and balance coefficients. Avoid silently inventing these rules.

### PHY-011 — Regional armor and additive global totals (DECIDED, 2026-10-18)

- Every anatomical region has an explicit `Couraça` morphology parameter: `Cabeça.Couraça`, `Tronco.Couraça`, `Braços.Couraça`, `Pernas.Couraça`, `Cauda.Couraça`.
- **Anything described as “Geral” is a sum of the corresponding regional values, not an average or weighted average.**
- Thus `CouraçaGeral = Cabeça.Couraça + Tronco.Couraça + Braços.Couraça + Pernas.Couraça + Cauda.Couraça`. Likewise `MassaGeral` sums the regions' masses.
- PHY-010 still applies: global HP uses global mass and armor; localized defense uses the involved region's armor together with defending state and total mass; attack uses attacking-part mass and hardness.
- This is a **schema change** to the prior provisional anatomy table, not merely a calculation change. Keep `Couraça` distinct from `Dureza` until their precise mechanical distinction is defined; do not silently merge them or rename Torso.Coverage.
- **Not yet decided:** whether paired limbs' regional values are totals across individual limbs, and how absent/added body parts contribute. Do not assume a distribution formula.

### PHY-012 — Shared per-arm mass and arm count (DECIDED, 2026-10-18)

- `Braços.Massa` is the mass of **one arm**, not the sum of all arms.
- **All arms have the same mass** as defined by that single morphology value.
- Therefore, the arms' contribution to total body mass is `Braços.Massa × Braços.Quantidade`. `MassaGeral` sums the total mass contributed by each anatomical region, including this quantity-based calculation.
- Do **not** infer that armor, hardness, reach, or other limb parameters are necessarily identical across limbs from this mass decision. Their representation remains to be decided.
- Do not instantiate per-arm morphology records for mass; the shared value and count suffice.

### PHY-013 — Shared morphology across all arms (DECIDED, 2026-10-18)

- **Option A adopted:** all arms of the same Mon share the same morphology values for **mass, armor (Couraça), hardness (Dureza), extension (Extensão) and drag (Arrasto)**.
- Arm quantity is tracked separately. There is **one shared set of arm parameters**, not individual values per arm.
- `Braços.Massa` remains mass per arm; `MassaTotalBraços = Braços.Massa × Braços.Quantidade` (PHY-012).
- Do not assume that global armor, effective drag or reach simply multiply by arm quantity: those aggregations depend on each property's physical meaning and should be decided explicitly. Regional totals that are additive must sum contributions correctly under PHY-011.
- This shared-arm choice does not yet establish whether legs and other repeatable anatomical structures follow the same representation.

## Existing architectural boundary

- A **Move** is an executable action (e.g. punch or jump).
- A **Mechanic** is a rule governing action (e.g. double jump, groundedness).
- A **Physical Property** is a numerical or geometric quantity used by mechanics (e.g. gravity, vertical velocity, impulse, collision normal, attack reach).
- A **Morphology Parameter** is a body characteristic that may affect derived physical properties.
- A **System** implements one or more mechanics. These names describe concepts, not necessarily new MonoBehaviour classes.
- **Incremental morphology affects gameplay continuously** (for example jump height or punch attack reach), without resizing/changing the sprite.
- **Visual transformation is separate** and occurs only when explicit morphology thresholds/pattern conditions are met later.

## Decision workflow

For every new physics decision:
1. State **one precise question**, with 2–3 sensible options only if needed.
2. Record the user's chosen rule here, with an ID PHY-002, PHY-003, etc.; distinguish a decision from an unvalidated implementation hypothesis.
3. Name the affected movement families and technical dependency nodes using `GAMEPLAY_TECHNICAL_MAP.md`.
4. Defer equations, tuning and code until the smallest vertical-slice test requires them.
5. Do not expand lore, art or the move catalog as a substitute for validating the gameplay.

## Pending design questions (not decisions)

- PHY-014: Should legs follow the same shared-per-limb morphology model as arms?
- Groundedness and normals: solid floor, walls, slopes, one-way platforms.
- Player-versus-environment collision and Mon-versus-Mon physical interaction.
- How anatomical parameters produce jump impulse, punch reach/hitbox and knockback without sprite changes.
- Units, timescale and playtest instrumentation.

## Immediate V1.7 hypotheses, not yet implemented

- Jump apex grows incrementally as selected anatomical capabilities increase while world gravity remains coherent.
- Punch attack reach/hitbox grows incrementally with anatomy while body collision and sprite remain unchanged.

These are test hypotheses, not fixed final formulas or balance targets.
