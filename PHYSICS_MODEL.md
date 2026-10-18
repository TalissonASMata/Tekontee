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

### PHY-014 — Shared morphology across all legs (DECIDED, 2026-10-18)

- **Option A adopted:** all legs of one Mon share the same leg morphology parameters (including **Massa, Couraça, Extensão and Arrasto**); leg quantity is a separate parameter.
- Each leg uses the shared per-leg morphology values. There is no independent morphology record per leg.
- Consequently, the legs' contribution to overall body mass is `Pernas.Massa × Pernas.Quantidade`, consistent with PHY-011/PHY-012.
- Do not automatically multiply properties such as extension, hardness, or drag by quantity; their effects depend on the mechanics and require explicit definitions.
- **Not yet decided:** how number of legs influences stability, locomotion and jumping; likewise how armor aggregates when repeated limbs contribute to `CouraçaGeral`.

### PHY-015 — Distinct gameplay effects of leg morphology (DECIDED, 2026-10-18)

The user defines **four separate roles** for leg properties, without automatically treating extra legs as extra movement speed:

- **`Pernas.Quantidade`**: contact/adhesion to surfaces, including **resistance to knockback (anti-repel)** and resistance to slipping on **vertical or inclined surfaces**. Whether a given surface is climbable and the precise adhesion formula remain open.
- **`Pernas.Massa`**: contributes to **damage of leg-based attacks**, alongside the **hardness of the leg**. It also contributes to global body mass as `Pernas.Massa × Pernas.Quantidade` per PHY-014.
- **`Pernas.Extensão`**: affects **locomotion/displacement and speed**; it should not merely scale the visible sprite.
- **`Pernas.Arrasto`**: affects **falling speed** (aerial resistance), consistent with universal gravity in PHY-002; it is not a change to each Mon's gravity.
- These are **directional mappings, not finalized equations**. Surface friction, adhesion, knockback, motion, air resistance and damage remain separately tunable derived mechanics.
- **Dependency families:** ground mobility/adhesion; air mobility/falling; striking with legs; defense/knockback. Validate individually with small Unity tests, preserving existing movement.
- **Schema note (resolved in PHY-016):** `Pernas.Dureza` is now part of the anatomical schema.
- **Not yet decided:** whether and how `Pernas.Quantidade` influences acceleration or stamina beyond surface adhesion; falling drag formula, and exact leg hardness effects.

### PHY-016 — Hardness on every anatomical region (DECIDED, 2026-10-18)

- **Confirmed:** `Dureza` is a morphology parameter in **every anatomical region**: `Cabeça.Dureza`, `Tronco.Dureza`, `Braços.Dureza`, `Pernas.Dureza` and `Cauda.Dureza`.
- Head and torso already had hardness in the working schema; this decision explicitly adds/standardizes it for **arms, legs and tail**, as well.
- **Arms and legs:** PHY-013 and PHY-014 apply; all limbs of the same type share their type's hardness value (no separate hardness stat per individual arm/leg).
- `Dureza` remains **distinct from `Couraça`**. PHY-010 and PHY-015 establish hardness as relevant to striking; do not assume it contributes directly to global armor or health without a later decision.
- No formula or Unity gameplay implementation is specified by this schema decision.

### PHY-017 — Tenacidade and Revestimento: distinct morphology concepts (DECIDED, 2026-10-18)

- **Terminology approved:** replace the previous anatomical numeric `Dureza` with **`Tenacidade`**, representing a body's ability to **withstand impact / absorb damage without breaking**. It is a physical resistance characteristic, not necessarily surface hardness in the strict material-science sense.
- Replace the previous `Couraça` concept with **`Revestimento`**, representing **surface/body covering traits** that can grant distinctive situational advantages. Examples: **espinhos** (spines), **penas** (feathers), **escamas** (scales), **ventosas** (suction cups). These are examples, not a finalized trait catalog.
- Both concepts belong to **each anatomical region**: head, torso, arms, legs and tail. Arms share their parameters across all arms; legs share theirs across all legs (PHY-013/014).
- **`Tenacidade` is quantitative; `Revestimento` is a categorical/configurable morphology feature**, not automatically a number or a flat defense multiplier. Exact support for multiple coverings, compatibility rules and their reaction mechanics are pending.
- **Supersedes terminology from PHY-010/011/013/014/015/016** wherever they call the prior concepts `Couraça` and `Dureza`; keep those prior entries as historical decision records, not current field names.
- **Important unresolved downstream impact:** PHY-010 used `CouraçaGeral` to derive HP and defense and PHY-011 defined `CouraçaGeral` as an additive sum. Since `Revestimento` is not a scalar, **do not sum coverings** or naively substitute `RevestimentoGeral` into those formulas. The global-life and defense derivations must be revisited before implementation. The established rule that numeric "Geral" totals are sums (e.g. mass) still stands.
- **Also pending:** whether/how `Tenacidade` modifies attack damage as formerly ascribed to `Dureza`, how it influences life/defense, and which covering traits affect each interaction. No equations or code changes are authorized yet.

### PHY-018 — Maximum HP from total mass and total tenacity (DECIDED, 2026-10-18)

- **Approved:** `VidaMaxima = f(MassaGeral, TenacidadeGeral)`. Both are morphology-derived numerical inputs to the global health pool, not independently leveled RPG stats.
- `MassaGeral` is the sum of anatomical mass contributions, counting repeated limbs using their per-limb mass × quantity (PHY-012/014).
- `TenacidadeGeral` is **the sum of regional tenacity contributions**, not a mean or weighted average. Where limb tenacity is expressed per repeated limb, aggregate its contribution by quantity; the precise representation and balancing coefficient are implementation decisions.
- `Revestimento` remains a separate categorical/configurable trait for situational effects (e.g., spines, scales, suction cups), **not a numeric term in the HP formula and not a simple additive `RevestimentoGeral`**.
- Keep **one global HP pool** (PHY-010). Do not add separate limb HP.
- **Formula `f`, normalization, numerical bounds and tuning have not been decided.** This is an agreed dependency, not a final numeric equation.
- **Supersedes the obsolete HP inputs** involving `CouraçaGeral` in historical PHY-010/011, in line with PHY-017.

### PHY-019 — Defense determined by participating limbs and overall mass (DECIDED, 2026-10-18)

- **There is no damage localization by body part.** A Mon has one global HP pool; receiving a hit does not route damage to the anatomical region struck or track separate limb health.
- For each **defensive action/stance**, the designer specifies **which anatomical members participate in that defense**. These are the defending members, not a struck-body-part detection system.
- **Defense is a function of the sum of the tenacity of the members participating in the defense plus overall body mass**: `Defesa = f(SomaTenacidadeMembrosDaDefesa, MassaGeral)`.
- The values for repeated limbs are shared by type under PHY-013/014. How a defense selects one, several or all repeated limbs, and any action coefficients, are to be specified with the move; do not silently assume all limbs participate.
- `Revestimento` may create separate situational defense effects if individually designed, but is not inherently a flat numeric `Couraça` modifier.
- Supersedes historical PHY-010 text about defense depending on the **body region struck**; no such hit-location mechanic is part of the agreed design.
- **Not yet decided:** baseline/default defense behavior while not performing a defense action, formula scaling, and damage reduction bounds.

### PHY-020 — Passive natural defense without active blocking (DECIDED, 2026-10-18)

- **Option B adopted:** the Mon has **passive/natural defense even while not performing a defensive action**.
- This default defense uses **the tenacity of a predefined set of anatomical members** and the **Mon's overall body mass**, consistent with PHY-019: `DefesaPassiva = f(SomaTenacidadeMembrosDefesaPassiva, MassaGeral)`.
- An explicitly executed defensive move can designate **its own participating members**, providing a defense value distinct from passive defense. Do not assume a specific numerical bonus or calculation until defined.
- No hit-location detection or limb-specific HP; damage continues to affect the single global HP pool.
- **Not yet decided:** which limbs belong to the passive-defense set, whether they are defined per Mon/form or shared by all Mons, and the precise scaling of passive versus active defense.

### PHY-021 — Form-dependent passive defense anatomy (DECIDED, 2026-10-18)

- **Option B adopted:** the anatomical members participating in **passive defense are defined by each Mon's current form**, not by one universal selection for all Mons.
- Each Mon/form may have a distinct body structure, so its passive-defense participation set must be configurable for that specific form.
- This feeds the established passive-defense calculation from PHY-020: `DefesaPassiva = f(SomaTenacidadeMembrosParticipantesDaForma, MassaGeral)`.
- Transforming can change which members contribute to passive defense **without** introducing per-part health or localized damage (PHY-019).
- Keep the distinction between a **form's default passive-defense set** and the **members used by a particular active defensive move**.
- **Not yet decided:** how forms declare anatomical participation, how missing body regions or number of repeated limbs are counted within this defensive set, and whether default passive sets change during non-transformative morphology growth.

### PHY-022 — Configurable number of defending limbs per form/move (DECIDED, 2026-10-18)

- **Option B adopted:** when a form or defensive move employs a repeated limb type (e.g. arms, tentacles), **the number of limbs that contribute to defense is explicitly configurable**, and does not automatically equal all available limbs.
- This applies to the form-dependent passive-defense participation model (PHY-020/021) and enables different defensive moves or stances to recruit different numbers of limbs.
- Participants contribute their shared anatomical tenacity according to the actual number involved; never count more participating limbs than the form has available. The precise defense equation and tuning remain pending.
- **Future gameplay idea (NOT YET APPROVED AS A FULL MECHANIC):** a chargeable defense reminiscent of Mega Man weak/charged shots, where **holding the defense button progressively recruits more arms or octopus tentacles**, increasing defensive participation as charge builds. The user supplied this as an illustrative possibility, not a mandate to implement now.
- **Undecided for chargeable defense:** input behavior, activation timing, ramp/steps, maximum charge, vulnerability, trade-offs, resource costs and whether passive defense itself can charge. Keep chargeable defense as an exploration candidate, not a coded commitment.
- **Affected nodes:** morphology shared per-limb values and quantity; form definition; passive-defense participation; defensive move configurations; future hold/charge input and move-execution state.

### PHY-023 — Defensive charge triggered by any participating member type with Quantity > 2 (DECIDED; clarified 2026-10-18)

- **Corrected trigger:** charging becomes available if **at least one anatomical member type referenced by the defense** has the morphology property `Quantidade > 2`. The **total number of member types or combined participants is irrelevant** to charge eligibility.
- Examples: `Tronco + Braços` may charge when `Braços.Quantidade = 4`; `Tronco + Braço` with `Braços.Quantidade = 2` does not. `Braços.Quantidade = 4` alone permits 2 → 4 arms.
- **Member quantities must be even**. Odd arm/leg/tentacle quantities such as 3 or 5 are not valid in this design; therefore the previous question about a partial odd stage is canceled.
- For an eligible repeated member group, defensive participation **charges by pairs (2, 4, 6, ...)** up to the configured eligible quantity. Other referenced anatomical types can participate according to the defense definition, but their mere presence does not count toward the >2 trigger.
- Defense at each stage uses the tenacity of actually participating limbs and overall body mass, under PHY-019; exact coefficients and timing are not yet decided.
- **Not yet decided:** how multiple qualifying repeated member types charge simultaneously, hold durations, activation/release timing, and whether charging is restricted to active defensive moves. Do not assume these details.
- **Technical nodes:** morphology member-type quantities, move participation data, charge stages/input, defense evaluation. No gameplay code change yet.

### PHY-024 — Simultaneous charging of multiple qualifying limb types (DECIDED, 2026-10-18)

- **Option A adopted:** when a defensive move includes multiple distinct member types whose respective `Quantidade > 2`, **all qualifying types advance together during one shared defense charge**, not as separate independent charges.
- At each charge stage, **add two participating members of each qualifying type** (e.g. with 4 arms and 4 tentacles: stage 1 = 2 arms + 2 tentacles; stage 2 = 4 arms + 4 tentacles).
- Member types with `Quantidade <= 2` do not independently enable charging; they participate according to the configured move, not as additional charge tracks.
- The existing even-count requirement (PHY-023) still applies.
- **Not yet decided:** how to handle distinct maxima (such as 4 arms and 6 tentacles), timing per stage, whether the base stage starts instantly, and on-release behavior. Do not invent these rules.
- **Affected nodes:** defensive move participation, grouped charging state, morphology counts and tenacity-derived defense evaluation. This is a documented design decision, **not an instruction to implement now**.

### PHY-025 — Continue charging until each member type reaches its own maximum (DECIDED, 2026-10-18)

- **Option A adopted:** when a defense uses multiple charge-eligible member types with different even quantities, a single charge **continues until every type has reached its own available maximum**, even after one type has stopped gaining participants.
- At each stage, a qualifying type increases its number of participating members by **two**, capped at its configured available count; types at their maximum **stay at that maximum** while others continue charging.
- Example with `Braços.Quantidade = 4` and `Tentáculos.Quantidade = 6`: **stage 1 = 2 arms + 2 tentacles; stage 2 = 4 arms + 4 tentacles; stage 3 = 4 arms + 6 tentacles**.
- One shared charge progression still applies (PHY-024), not separate independent inputs or meters.
- Defense at each stage incorporates the tenacity of the actual participating members and global mass (PHY-019), without inventing a numeric formula yet.
- **Not yet decided:** time needed for each stage, initial activation timing, release behavior, and whether charging consumes resources. No gameplay implementation requested at this point.

### PHY-026 — Immediate baseline defense, near-instant incremental reinforcement (DECIDED, 2026-10-18)

- **Option A, with clarification:** defensive protection **starts immediately** when the player presses defense, based on the **participating members/components that do not require charging**.
- **Additional charge-eligible limbs** are progressively recruited **in pairs**, according to PHY-023/024/025, increasing defense in very rapid successive stages. The purpose is **reaction timing / how early the player commits to the defensive action before an incoming hit**, not a prolonged power-up.
- The defense is **active and protective during the progression**: there is no need to release the button to start protecting.
- If a defensive move has **no charge-eligible member type**, it simply applies its defined protection immediately without stages.
- **Important interpretation of earlier examples:** tables that start with a 2+2 charged stage describe the *first added pair stage*, not necessarily the entire instantaneous baseline. Baseline participation comes from the specific move's non-charging defensive members. Do not automatically count an initial pair from every charging type as instantaneous unless explicitly designed.
- **No fixed charging duration or step interval has been decided**. These should be short and tuned for fighting-game readability/responsiveness.
- **Pending:** behavior when the defense uses only charging member types with no fixed baseline members, and whether/how defense ends on button release or interruption.
- **Affected nodes:** move participant definitions (immediate versus chargeable); defense activation state; staged limb contribution; collision/strike timing; derived defense formula. No code change required yet.

### PHY-027 — Immediate first pair for defenses made only of chargeable limbs (DECIDED, 2026-10-18)

- **Option A adopted:** if a defensive move contains **only a charge-eligible repeated member type** (e.g. four arms) and has no separately designated fixed/non-charging members, **its first pair of members protects immediately** when defense is pressed.
- Subsequent pairs join after the brief progressive charge intervals established conceptually in PHY-026. Example for four arms: **2 arms immediately → 4 arms shortly after**.
- This avoids an unprotected startup gap for a defense composed exclusively of repeated member types.
- For mixed moves, members explicitly configured as non-charging still provide immediate baseline protection (PHY-026); how an initial pair from each charge-eligible group combines with that baseline is a separate detail to clarify if relevant.
- Damage remains global, and defense at any instant depends on the participating members' tenacity and the Mon's total mass, not on a hit-location system.
- Exact stage intervals, interruption rules and hold/release behavior remain unconfirmed; **do not implement speculative timing**.

### PHY-028 — Immediate first pair alongside fixed defensive members (DECIDED, 2026-10-18)

- **Option A adopted:** when a defense includes **fixed/non-chargeable members** alongside a charge-eligible repeated member type (`Quantidade > 2`), the fixed members **and the first pair of each chargeable type protect immediately** on defense input.
- Subsequent pairs join after the brief staged intervals from PHY-026/027; the defense remains protective throughout.
- Example: `Tronco + Braços (4)`: immediate defense = **torso + 2 arms**; next stage = **torso + 4 arms**.
- Defense effectiveness derives from the tenacity of the members actually defending at each moment and `MassaGeral`, per PHY-019.
- This clarifies and supersedes the earlier PHY-026 implementation ambiguity about whether the first pair from chargeable groups could be instantaneous when fixed members are present: **yes, it is instantaneous**.
- Stage timing, resource consumption and release/interruption behaviors remain open; do not invent them or change Unity code yet.

### PHY-029 — Offensive recovery after charged defense (PARTIALLY DECIDED, 2026-10-18)

- **Confirmed by user:** if a defensive action **has used its charging progression**, there must be a **delay before the Mon can attack** after that defense. This is the offensive trade-off for taking time to recruit additional defending limbs.
- The cost applies specifically when **charge was actually used**, not merely because a defense is theoretically charge-capable.
- **Not decided:** whether releasing the defense stops defensive protection immediately or after a recovery interval; whether attacks are delayed by a fixed or charge-level-dependent interval; whether movement is affected; exact timing, cancellation rules and other follow-up actions.
- Do not treat the assistant's preceding Option A (instant end of defense on release) as confirmed: the user instead specified the **attack delay** requirement.
- **Technical nodes:** defense charge progression/state, defense exit state, attack eligibility/input buffering, combat-state transitions. No implementation or timing values yet.

### PHY-030 — Attack recovery duration equals defense charging duration (DECIDED, 2026-10-18)

- **User decision:** after using charged defense, the delay before the Mon can attack is **equal to the time spent charging that defense**, a 1:1 time relationship.
- Example for illustration only: if a defense has spent 0.2 seconds progressing through charge, the post-defense offensive recovery is 0.2 seconds. **No fixed numeric charge duration has been approved.**
- A defense that **did not actually use charging** does not incur this charge-derived attack delay (PHY-029).
- This replaces both proposed PHY-030 alternatives (“same fixed recovery whenever charged” or “arbitrary scaling by number of stages”): **recovery follows elapsed charging time**, not a separate duration chosen by charge level.
- Recovery limits **attacking**; effects on movement, remaining defensive protection, input buffering and interruptions are still undecided.
- Precise charge-time measurement and the start of the post-defense recovery interval must be settled during implementation, without guessing the move's timeline.

### PHY-031 — Defense ends immediately on release; charged attack recovery persists (DECIDED, 2026-10-18)

- **Option A adopted:** releasing the defense input ends its active defensive protection **immediately**; there is no additional protected exit window.
- If charge actually occurred, the Mon **cannot attack until its charge-matched recovery time finishes** (PHY-029/030), even though its active defense has already ended.
- If no charge occurred, there is **no charge-derived offensive recovery penalty**; ordinary move/action restrictions, if any, are separate.
- **The Mon is therefore exposed between release and the end of any remaining attack recovery**. This does not remove the form's separately defined passive defense (PHY-020/021).
- Effects on movement, attack input buffering, and interruptions are still not decided; do not infer extra penalties.
- **Technical nodes:** defensive input and active-protection state, charge-time tracking, attack availability/recovery, passive defense fallback. No Unity implementation requested yet.

### PHY-032 — Unrestricted mobility during post-defense attack recovery (DECIDED, 2026-10-18)

- **Option A adopted:** after releasing a charged defense, the Mon may **move, jump and reposition normally** during its charge-matched attack recovery.
- The recovery restriction applies **only to attacking**; it does not impose a movement-speed penalty, jump lock or other generic mobility restriction.
- Active defensive protection still ends immediately on release (PHY-031). Passive natural defense remains available.
- Attack availability returns after the recovery period equal to the actual elapsed defense charge time (PHY-030).
- **Not yet decided:** whether a new defensive action can be started during the attack-recovery interval; handling of attack input buffering, and interruption by hitstun/other states.
- **Affected technical nodes:** attack eligibility/recovery, Mon movement controller, defense state and input. No gameplay code changes yet.

### PHY-033 — Defense may be restarted during charged-defense attack recovery (DECIDED, 2026-10-18)

- **Option A adopted:** a Mon may **start another defensive action while its previous charged-defense attack-recovery timer is still active**.
- The existing **offensive restriction remains in effect**: restarting defense does not cancel, bypass, or automatically clear the time that must elapse before attacking.
- Movement remains unrestricted under PHY-032; each new defense provides its normal immediate protection and may recruit further limbs according to PHY-023–028.
- **Not yet decided:** whether a new charge stacks, extends, resets or otherwise interacts with an earlier outstanding attack-recovery timer; do not assume a stacking formula or allow a loophole to attack early.
- **Affected technical nodes:** defense activation, charge timer, attack lockout, input transitions and movement. This is a design rule, not authorization for new Unity implementation.

### PHY-034 — Non-stacking recovery deadlines for consecutive charged defenses (DECIDED, 2026-10-18)

- **Option B adopted:** if another charged defense occurs while an earlier attack-recovery interval is still running, **their recovery durations do not add together**.
- Each defense sets its **own attack-availability deadline** relative to the end of that defense, based on the time actually spent charging (PHY-030).
- The Mon cannot attack until **the latest outstanding deadline** has passed: `AtaqueLiberadoEm = max(prazosDeRecuperaçãoPendentes)`.
- Example only: first deadline at t=1.0s, second at t=1.3s → attack is permitted from t=1.3s; no sum of pending delays.
- Restarting defense is still allowed (PHY-033), and movement remains free (PHY-032). Defense itself ends immediately on release (PHY-031).
- **Not yet decided:** exact timings/charge durations, queued attack-input behavior, interruptions and cancellation. Do not introduce arbitrary additive or reset penalties.
- **Affected nodes:** charge-duration tracking, attack recovery deadlines, defense state transitions and attack eligibility.

### PHY-035 — Ignore attack inputs during post-defense recovery (DECIDED, 2026-10-18)

- **Option A adopted:** while the Mon is still subject to attack recovery from charged defense, **attack inputs are ignored**; they are **not buffered or automatically executed** after the lockout.
- To attack once recovery ends, the player must issue a **new attack input**.
- This applies specifically to recovery caused by charged defense (PHY-029–034), including successive defenses using the latest non-stacked deadline.
- Movement remains free (PHY-032), and new defensive actions remain available (PHY-033).
- The input must not be retroactively interpreted as a charge or queued attack; no automatic attack should fire without a new deliberate command.
- **Not yet decided:** global attack-input buffering policy for unrelated states, attack button holds, and interruption by hitstun or other systems.
- **Affected nodes:** input evaluation, attack availability checks, combat recovery state and move execution. No Unity implementation requested yet.

### PHY-036 — Defensive cancellation depends on the attack move (DECIDED, 2026-10-18)

- **Option C adopted:** while an attack is executing, whether the Mon can cancel that attack to enter defense is **defined by the specific attack move**.
- Some moves may allow a defensive cancel; other moves must finish their relevant committed portion before defense can start.
- This is a **move-level execution rule**, not a universal always-allowed or always-forbidden defense cancel.
- **Not yet decided:** cancellation windows, timing, effects on hitboxes, stamina/costs, recovery from interrupted attacks, and input buffering. Do not assume instant cancellation across all animations.
- **Affected nodes:** move definitions, combat-state transitions, attack execution phases, defensive input gating and charge-start timing. Preserve existing V1.6 controls until a test slice is approved.

### PHY-037 — Configurable defense-cancel windows for each attack (DECIDED, 2026-10-18)

- **Option B adopted:** an attack that permits defensive cancellation (PHY-036) defines **explicit windows in its execution timeline** during which the player may cancel into defense.
- A defense input outside an allowed window **does not interrupt the running attack**. Do not assume the command is queued, buffered or later executed without a separate decision.
- Each move owns its cancel eligibility and its allowed timing windows; there is **no global unrestricted instant defense-cancel rule**.
- **Not yet decided:** exact timeline boundaries, windows during startup/active/recovery phases, whether windows depend on contact (hit/block/whiff), and whether canceling imposes additional costs or recovery.
- **Affected nodes:** attack move data, attack-phase timing, cancellation eligibility, defense state transition and input evaluation. No Unity implementation requested yet.

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

- PHY-038: When defense is pressed outside an attack's permitted cancel window, should the input be ignored, or buffered briefly for the next valid window?
- Groundedness and normals: solid floor, walls, slopes, one-way platforms.
- Player-versus-environment collision and Mon-versus-Mon physical interaction.
- How anatomical parameters produce jump impulse, punch reach/hitbox and knockback without sprite changes.
- Units, timescale and playtest instrumentation.

## Immediate V1.7 hypotheses, not yet implemented

- Jump apex grows incrementally as selected anatomical capabilities increase while world gravity remains coherent.
- Punch attack reach/hitbox grows incrementally with anatomy while body collision and sprite remain unchanged.

These are test hypotheses, not fixed final formulas or balance targets.
