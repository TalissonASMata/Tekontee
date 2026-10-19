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

### PHY-038 — Ignore defense input outside cancel windows (DECIDED, 2026-10-18)

- **Option A adopted:** pressing defense during an attack **outside the move's configured defensive-cancel window** has **no effect**; the input is **discarded** rather than buffered.
- The attack proceeds normally. If the player wants to cancel into defense, they must **press defense again during a permitted window**, as defined per move by PHY-036/037.
- No automatic defensive transition occurs later due to the rejected input.
- This covers defense input during a running attack and **does not imply a universal prohibition on input buffers elsewhere**.
- **Affected technical nodes:** attack-phase cancellation checks, defense-input validation, combat-state transitions and move data. No Unity code change yet.

### PHY-039 — Defensive cancellation may occur during an attack's active impact phase (DECIDED, 2026-10-18)

- **Option A adopted:** individual attacks **may define defensive-cancel windows during their active hit/impact phase**, not solely during startup or recovery.
- This is **permission at the move-design level**, not an always-active universal cancel. The move's expressly configured window determines eligibility (PHY-036–038).
- On a valid defensive cancel, attack execution transitions into defense according to the move's permitted cancel timing. **Not yet decided:** whether existing attack hitboxes disappear immediately, whether a registered hit remains valid, and detailed simultaneity/hit-confirm resolution.
- Pressing defense **outside** a permitted window remains ignored and unbuffered (PHY-038).
- **Affected nodes:** per-move cancel windows, attack active-frame state, hitbox lifecycle, strike resolution, combat-state transitions. No Unity implementation required yet.

### PHY-040 — Cancelled attack hitbox ends immediately (DECIDED, 2026-10-18)

- **Option A adopted:** when an attack is validly cancelled into defense during its active hit phase, **the attack's active hitbox disappears immediately when the defense starts**. No additional attack hits may register from that cancelled hitbox afterward.
- **Hits that were already resolved before the cancellation remain valid**; cancellation does not undo prior damage, knockback or hit results.
- The move's configurable defensive cancel eligibility/window (PHY-036–039) must be satisfied; outside a permitted window, defense input is discarded (PHY-038).
- Defense protection begins on the valid transition, with its immediately participating members under PHY-026–028.
- **Not yet decided:** how truly simultaneous attack/contact and cancel inputs are ordered within the simulation tick, and whether cancelling an attack incurs a separate recovery cost. Do not infer a residual impact window.
- **Affected technical nodes:** active hitbox lifecycle, move cancellation, damage event ordering, defense activation and combat-state transitions. No Unity gameplay code change requested yet.

### PHY-041 — Defensive cancel wins same-tick collision tie (DECIDED, 2026-10-18)

- **Option B adopted:** if an attack's hit/contact and a valid defensive-cancel input occur in the **same simulation tick**, resolve the **defensive cancel first**. Its attack hitbox is disabled before hit registration; that same-tick hit does **not** inflict damage.
- **Design rationale from the user:** prioritizing defense on an exact timing tie favors continued exchanges over ending a match faster through a simultaneous hit.
- **Only exact same-tick ties** follow this priority. Hits **already registered in an earlier tick remain valid** and are never undone (PHY-040).
- This priority applies **only if the defensive cancel is legal for that attack and timing window** (PHY-036–039); otherwise the attack proceeds according to the ordinary rules.
- Defense protection activates normally after successful cancellation (PHY-026–028). Do not conflate this tie-break with invincibility or with a universal priority for all defensive inputs.
- **Implementation requirement for a future vertical slice:** evaluate eligible cancel inputs before committing collision-generated damage events for the same fixed simulation step; test same-step, previous-step and invalid-window cases. No code changes yet.

### PHY-042 — Valid newly activated defense wins same-tick incoming-hit tie (DECIDED, 2026-10-18)

- **Option A adopted:** if an **incoming enemy hit** would register in the **same simulation tick** in which the defending Mon validly activates defense, **process the valid defense activation first**.
- The incoming attack is then resolved **against the newly active defensive state**, using participating members and derived defense values under PHY-019–028, instead of treating the Mon as undefended at that instant.
- **This does not mean the incoming attack is automatically nullified or deals zero damage**: the defensive calculation still determines how much damage/knockback, if any, gets through.
- The tie-break only applies to **valid defense inputs**, including defensive cancels allowed by the current move's windows (PHY-036–039); invalid defense inputs do not gain priority.
- This extends the **defense-first philosophy** behind PHY-041 to incoming hits, but preserves the distinction: in PHY-041, the Mon's **own outgoing hitbox is removed** on defensive cancel; in PHY-042, the **enemy's hitbox is not inherently removed** and is handled against active defense.
- Hits already registered in earlier simulation ticks remain final. Actual timing/input priority needs tests at fixed-step boundaries.
- **Affected nodes:** input activation order, attack collision/damage resolution, active defense participation, move-cancel windows. No Unity implementation yet.

### PHY-043 — Active defense reduces but does not automatically eliminate knockback (DECIDED, 2026-10-18)

- **Option A adopted:** when a Mon successfully blocks an incoming attack, **active defense can reduce the impact but does not inherently cancel all knockback**. The remaining impact can still displace the Mon.
- Knockback must consider the incoming strike's physical properties, the defender's morphology-derived defensive response and overall mass, and surface adhesion/anti-repel effects from leg quantity (PHY-009, PHY-015, PHY-019).
- **Do not equate defensive damage reduction with total impulse cancellation**: damage and knockback are distinct outputs that may need distinct calculations and tuning.
- A particularly effective defense, morphology, surface interaction or explicit special mechanic **may** result in zero effective knockback, but this is not the universal default.
- **Not yet decided:** equations, order of modifiers, thresholds for sliding/knockback, and any guard-break conditions. No Unity implementation requested yet.
- **Affected nodes:** attack impact evaluation, defense state/participants, mass and limb morphology, surface contact/adhesion and knockback movement.

### PHY-044 — Defense is overcome by sweeps and grabs, not raw impact strength (DECIDED, 2026-10-18)

- **User decision:** active defense **only loses to sweep (rasteira) and grab (agarrão) techniques**. A powerful ordinary strike does **not** break guard merely by exceeding an impact/strength threshold.
- Defended ordinary strikes may still cause **damage and knockback** according to PHY-042/043, but they do not automatically terminate the guard or create an independent guard-break state through brute force.
- **Sweeps and grabs are attack-category counters to defense**, not evidence of a generic power-based guard-break mechanic.
- **Not yet decided:** precise conditions for a sweep to beat guard (e.g. stance or height), grab range/escape, resulting stagger/downed states, whether passive defense differs, or any special move exceptions. Do not silently add high/low block rules or a grab escape system.
- **Compatibility note:** the V1.6 move named `GuardBreak` is a prototype action name; the approved target design **does not authorize universal raw-force guard breaking**. Revisit its function/name only when implementing these counter categories; do not change validated prototype code now.
- **Affected nodes:** move classifications and outcomes (sweep/grab), defensive state, combat resolution priority, knockback and animation/state transitions.

### PHY-045 — Successful sweep causes knockdown (DECIDED, 2026-10-18)

- **Option A adopted:** a successfully landed **rasteira (sweep)** against a defending Mon **causes knockdown (queda)**, following the familiar behavior of traditional fighting games.
- As established in PHY-044, sweep is a valid counter to active defense; this decision selects **knockdown**, rather than a configurable stagger/displacement-only outcome, as the standard successful result.
- Do not interpret “successful” as an unconditional hit: range, execution windows, collision and other attack-validity conditions still apply.
- **Not yet decided:** knockdown animation, duration, invulnerability during knockdown, wake-up options, damage amount, or how sweeps interact with a Mon that is not defending.
- **Affected technical nodes:** move classification, defense counter-resolution, knockdown combat state, animation and state recovery. No Unity gameplay code changes requested yet.

### PHY-046 — Player-controlled knockdown recovery constrained by morphology (DECIDED, 2026-10-18)

- **Option B adopted with a fundamental condition:** after a sweep knockdown, the player **may input a recovery action to rise sooner**, but the resulting recovery effectiveness/timing **depends on the Mon's morphology**.
- The player does **not** receive an identical fixed recovery advantage for every Mon regardless of its anatomy.
- A baseline automatic stand-up behavior may still exist, but its exact timing and relation to the manual recovery are not decided.
- **Not yet decided:** which anatomical morphology parameters affect recovery, precise formulas and limits, recovery input/action, invulnerability, and exact timing. Do not select parameters or invent numerical values without confirmation.
- **Affected nodes:** knockdown combat state, recovery input/eligibility, morphology-derived mobility and animation/state transitions. No Unity gameplay code changes requested yet.

### PHY-047 — Knockdown recovery depends on intellect and mass (DECIDED, 2026-10-18)

- **User-selected morphological inputs:** `Intelecto` and `Massa` determine the effectiveness/timing of player-controlled recovery after knockdown (PHY-046).
- This **replaces** the assistant's prior proposal involving `Pernas.Extensão` and `Braços.Extensão`; neither extension is confirmed as a knockdown-recovery input.
- `Intelecto` is currently a Head morphology parameter (`Cabeça.Intelecto`); **which mass measure** (`MassaGeral` versus another morphology mass value) and the exact recovery relationship remain to be confirmed. Do not silently impose a formula or assume a specific numeric scale.
- The recovery must remain sensitive to the player's recovery action, not be only a fixed morphology-driven automatic timer.
- **Not yet decided:** how intellect and mass interact, which mass aggregation is used, input timing, minimum/maximum recovery time and invulnerability.
- **Affected nodes:** morphology, knockdown state, player recovery input, combat-state recovery timing. No implementation requested yet.

### PHY-048 — Knockdown recovery uses total body mass (DECIDED, 2026-10-18)

- **User clarification:** the `Massa` input for knockdown recovery in PHY-047 is specifically **`MassaGeral`**, the Mon's total body mass.
- The two morphology-derived inputs are therefore **`Cabeça.Intelecto` and `MassaGeral`**, and player recovery action still affects the outcome (PHY-046).
- `MassaGeral` follows the established additive regional contributions, including repeated-member quantity (PHY-011/012/014).
- **Not yet decided:** the mathematical relationship between intellect, mass, input skill and recovery time. No numerical values or implementation are authorized by this decision.

### PHY-049 — Conditional early knockdown recovery by morphology thresholds (DECIDED IN PRINCIPLE, 2026-10-18)

- The user's proposed unlock condition for **player-triggered early recovery from a knockdown** is **`Cabeça.Intelecto > 40%` AND `MassaGeral (PesoGeral) < 40%`**.
- The recovery action is **allowed only if both conditions hold**. This is a **threshold-gated capability**, not the previously suggested continuous formula where more intellect always reduces recovery time and more mass always increases it.
- The two comparisons are **strict inequalities** (> and <), not inclusive at 40%.
- **The morphology point ranges, percentage denominators, and normalization rules have not yet been designed.** Therefore the percentages are **provisional design thresholds**, not executable checks against raw point values. Do not implement them before defining how percentage is measured.
- `MassaGeral` remains the total body mass confirmed in PHY-048. The user's word `peso` refers to this total, rather than to a particular limb.
- A Mon that does not qualify for early recovery still needs a normal knockdown recovery path; its exact duration and input handling have not been defined.
- **Not yet decided:** what counts as 100% for intellect or mass, whether these thresholds are final after morphology scaling is defined, the recovery input/timing and the normal rise duration.
- **Affected nodes:** morphology normalization and total mass aggregation, knockdown state, input gating and recovery action. No Unity code implementation requested yet.

### PHY-049 — Knockdown early-recovery eligibility and weight-dependent benefit (DECIDED, 2026-10-18)

- **Every Mon can recover from knockdown through its standard recovery animation at normal speed**, regardless of morphology. High body weight must never permanently prevent standing back up.
- **Player-triggered early recovery is available only if both conditions hold:** `Cabeça.Intelecto > 40%` of its eventual morphology scale **AND** `MassaGeral < 40%` of its eventual morphology scale. Both percentages are relative to scales **not yet defined**, not fixed raw point values.
- When eligible, a recovery input can **shorten the time spent knocked down** compared with standard recovery, potentially helping the player avoid subsequent hits/combos.
- **MassaGeral determines the size of this time advantage** for eligible Mons: increasing weight reduces the possible early-recovery benefit; at the **40% weight threshold the early-recovery benefit reaches zero** and the Mon uses standard recovery.
- At **MassaGeral >= 40%**, standard recovery is always available, but no accelerated player-controlled recovery applies. Likewise if `Cabeça.Intelecto <= 40%`, accelerated recovery is unavailable even when mass is low.
- **Not yet decided:** actual numeric morphology ranges, normalization, early-recovery duration curve for mass below 40%, animation/input timing, minimum duration, and combo interruption/invulnerability rules. Avoid interpreting “avoid combos” as guaranteed invulnerability.
- **Supersedes any earlier proposal** of continuous intellect-derived recovery speed: intellect is an eligibility threshold under this decision; mass governs the early-recovery time benefit.
- **Affected nodes:** morphology normalization, knockdown state, recovery input eligibility, animation duration/recovery window, attack follow-up timing. No Unity code changed yet.

### PHY-050 — Linear reduction of early-recovery benefit with mass (DECIDED, 2026-10-18)

- **Option A adopted:** for eligible Mons under PHY-049 (`Cabeça.Intelecto > 40%` and `MassaGeral < 40%` of their respective future normalized morphology ranges), the time advantage from player-triggered early knockdown recovery **decreases linearly as normalized total mass rises**.
- **At the 40% total-mass boundary, the early-recovery advantage is zero**. At and above that boundary the Mon recovers only through the normal animation and timing; no Mon is permanently unable to stand.
- The slope and maximum time saved cannot be numerically determined until the morphology scales, normal recovery duration and maximum early-recovery benefit are designed. No arbitrary constants are approved.
- **Intelecto governs eligibility, not the magnitude of the linear mass-dependent benefit** under PHY-049.
- **Affected nodes:** normalized morphological values, early-recovery eligibility, knockdown timer, animation timing and combat follow-up/combos. No Unity implementation approved yet.

### PHY-051 — Double-tap away from opponent for early knockdown recovery (DECIDED, 2026-10-18)

- **User-selected input:** while knocked down, initiate eligible early recovery by **quickly tapping twice in the direction away from the opponent** (relative back direction), rather than using a dedicated recovery button or button mashing.
- If opponent is **to the right**, press **Left, Left** quickly; if opponent is **to the left**, press **Right, Right** quickly.
- The direction is **relative to opponent position**, not permanently mapped to the screen's left or right. It must be evaluated using current combat orientation/positions.
- This gesture attempts early recovery only if PHY-049 conditions hold (`Cabeça.Intelecto > 40%` and normalized `MassaGeral < 40%`). The recovery time benefit still decreases linearly with mass under PHY-050.
- Otherwise the Mon stands using ordinary automatic recovery; the gesture never prevents normal recovery.
- **Not yet decided:** maximum interval between taps, whether the input is accepted during the entire knockdown or a specific window, which opponent is the reference in multi-combatant situations, and interruption/reversal edge cases.
- **Affected nodes:** relative combat direction detection, double-tap input recognition, knockdown recovery eligibility, morphology, animation and state timing. No Unity implementation requested yet.

### PHY-052 — Early-recovery input window begins on ground contact (DECIDED, 2026-10-18)

- The knockdown has a **falling animation/phase before the Mon touches the floor**. During that airborne/falling phase, the early-recovery input is **not accepted**.
- The valid input window **opens when the knocked-down Mon makes contact with the ground** and enters the grounded knockdown/recovery phase.
- From ground contact onward, the player may perform the PHY-051 **double-tap away from the opponent** to attempt early recovery, provided the PHY-049 morphology eligibility thresholds are met.
- The normal recovery animation remains available to every Mon. The early-recovery window naturally ends when normal recovery is complete; no additional arbitrary narrow reaction window has been confirmed.
- **This combines the user's intent behind options A and B:** a specific **state-gated** window, rather than any moment of falling or an unnecessarily short timed cue.
- **Not yet decided:** exact ground-contact validation, knockdown animation lengths, whether input pairs crossing the contact boundary count, and whether the early-recovery command can be repeated after an invalid attempt.
- **Affected nodes:** knockdown falling vs grounded phases, ground contact/collision normals, double-tap input handling, recovery animation and eligibility. No Unity implementation yet.

### PHY-053 — Both backward taps must occur after ground contact (DECIDED, 2026-10-18)

- **Option A adopted:** to trigger early knockdown recovery, **both directional taps** of the PHY-051 backward double-tap must happen **after the knocked-down Mon contacts the ground** and the valid window opens (PHY-052).
- A tap made while the Mon is still falling **does not count toward the pair**, even if the second tap happens after contact.
- Ground contact clears/discards any pre-contact tap sequence for purposes of early-recovery detection. Once grounded, the player must make a fresh two-tap input away from the opponent.
- Eligibility thresholds and mass-dependent benefit remain unchanged (PHY-049/050).
- **Not yet decided:** maximum interval between taps, and whether invalid/too-slow input sequences can be retried while grounded. Do not invent these numbers or restrictions.
- **Affected nodes:** knockdown phase transition, grounded contact detection, direction-relative double-tap recognizer, input-state reset and recovery eligibility. No Unity implementation yet.

### PHY-054 — Retry early-recovery double-tap during grounded knockdown (DECIDED, 2026-10-18)

- **Option A adopted:** if a player's backward double-tap attempt is invalid or too slow, they **may try the input again while their Mon remains grounded and knocked down**.
- Each attempt must still contain **two valid taps away from the opponent after ground contact** (PHY-051–053). Do not count a tap made during the falling phase.
- The available time to retry is naturally limited by the **remaining grounded knockdown period**, after which ordinary recovery completes.
- Retry permission **does not bypass morphology eligibility**: `Cabeça.Intelecto > 40%` and `MassaGeral < 40%`, with linear mass-based early-recovery benefit (PHY-049/050).
- **Not yet decided:** numerical double-tap timing threshold, whether a failed second tap can seed the next attempt, and the exact handling of directional changes. No Unity code implementation yet.
- **Affected nodes:** relative-direction double-tap input recognition, grounded knockdown input window, failed-attempt reset/retry, recovery eligibility and timer.

### PHY-055 — Universal double-tap recognition window (DECIDED, 2026-10-18)

- **Option A adopted:** the maximum time interval allowed between the two backward-direction taps for early knockdown recovery is **the same for every Mon**.
- The player's input execution skill is therefore assessed using a **universal timing window**, independent of `Cabeça.Intelecto`, `MassaGeral`, or other morphology attributes.
- Morphology still determines **eligibility** for early recovery and **how much recovery time is saved** under PHY-049/050, but does not widen or narrow the double-tap recognition interval.
- Both taps must occur after ground contact (PHY-052/053), and failed attempts may be retried while grounded (PHY-054).
- **Not yet decided:** exact time-window duration in seconds/frames, input device tolerances, or edge handling around direction changes; calibrate against responsive fighting-game controls.
- **Affected nodes:** shared relative-direction double-tap recognizer, grounded knockdown input state, morphology eligibility check and recovery-time calculation. No Unity code change requested yet.

### PHY-056 — Shared timing window for all double-tap commands (DECIDED, 2026-10-18)

- **Option A adopted:** all gameplay commands that use two rapid directional taps share **one universal maximum inter-tap interval**.
- This includes early knockdown recovery (PHY-051–055) and any future double-tap movement techniques such as dodge/dash, **if those techniques are explicitly added later**.
- The same timing window improves input consistency; it does **not** mean every double-tap triggers every action. Execution remains conditioned on combat state, relative direction, move availability, morphology eligibility and other applicable rules.
- The chosen window is **not dependent on Mon morphology** (PHY-055). Morphology may still determine whether a particular action is available and its effectiveness.
- **Not yet decided:** the numerical timing threshold and device/input calibration; no future dodge mechanic or binding is automatically approved by this decision.
- **Affected nodes:** shared double-tap input recognizer, context-specific move eligibility, grounded knockdown recovery, future mobility inputs. No Unity implementation requested yet.

### PHY-057 — Calibrate universal double-tap interval through Unity playtests (DECIDED, 2026-10-18)

- **Option A adopted:** do **not** assign an arbitrary fixed millisecond/frame threshold to double-tap commands yet.
- Implement a **single shared, configurable double-tap recognition interval** when the relevant input mechanic reaches its smallest Unity vertical slice, and **tune the value by playtesting actual responsiveness and execution feel**.
- The calibrated timing threshold applies consistently across double-tap mechanics (PHY-055/056), including early knockdown recovery. It is independent of morphology, which governs eligibility and effect, not input difficulty.
- Document the final chosen value and test context **only after real gameplay validation**. Do not represent an untested placeholder as an approved balancing constant.
- **Affected nodes:** shared input/double-tap detector, knockdown recovery, future mobility inputs and playtest instrumentation. This decision itself does not request a Unity code change.

### PHY-058 — Double-tap requires release between presses (DECIDED, 2026-10-18)

- **Option A adopted:** a valid double-tap directional command requires **press → release → press** of the same direction within the shared configured input window.
- Holding a direction continuously counts as **one press**, not as repeated taps. The recognizer must observe a release before the second press.
- This applies to early knockdown recovery input (PHY-051–055) and to any other explicitly implemented double-tap commands sharing the timing rule (PHY-056/057).
- The recognition window's numerical duration will be calibrated in Unity playtests (PHY-057), not chosen arbitrarily now.
- Early knockdown recovery still requires **both taps after ground contact** (PHY-052/053), with retries allowed while grounded (PHY-054).
- **Not yet decided:** minimum release duration (if any), how analog-stick return-to-neutral thresholds are handled, and whether direction-relative side changes reset the input sequence.
- **Affected nodes:** shared double-tap recognizer, directional press/release state, combat-facing orientation, knockdown recovery input validation. No Unity code changes yet.

### PHY-059 — Reset backward double-tap if opponent changes sides (DECIDED, 2026-10-18)

- **Option A adopted:** if the adversary crosses to the opposite side **between the first and second taps** of a relative-backward double-tap, **discard/reset the current input sequence**.
- The player must then execute a **new pair of taps away from the adversary's current side**. Example: opponent moves from right to left during an attempted `Left, Left`; the attempted sequence is invalid and the new required sequence is `Right, Right`.
- Do not preserve the first tap's old relative-backward direction after a side switch. Input release between two presses remains mandatory (PHY-058).
- For knockdown early recovery, the two valid taps still have to occur after ground contact (PHY-052/053), under the shared universal timing interval to be tuned in Unity (PHY-055–057).
- **Not yet decided:** exact implementation tolerances for near-overlapping character positions, and which opponent to reference in multi-combatant situations.
- **Affected nodes:** opponent-relative direction recognition, side-change detection, shared double-tap gesture state, grounded knockdown recovery input. No Unity code change yet.

### PHY-060 — Relative backward input uses opponent's current side (DECIDED, 2026-10-18)

- **Option A adopted:** when a Mon is knocked down, the direction counted as **backward / away from the opponent** is based on **the opponent's current position at the time the player begins the recovery double-tap**, not their side when the knockdown originally happened.
- Example: if the opponent crossed from right to left before the first input, the valid attempt is now `Right, Right`, not the former `Left, Left`.
- PHY-059 still applies: if the opponent switches sides **between taps**, reset the attempt and require two new taps in the updated backward direction.
- Both inputs must occur after the Mon touches the ground (PHY-052/053); timing is universal and calibrated in Unity (PHY-055–058).
- This is a relative-direction rule, not a new facing lock, animation override or teleport behavior.
- **Affected technical nodes:** opponent-relative input direction, grounded knockdown detection, double-tap recognizer and recovery eligibility. No Unity gameplay code change yet.

### PHY-061 — Early knockdown recovery includes backward knockback-like movement (DECIDED, 2026-10-18)

- **Option B adopted:** on successful early knockdown recovery, the Mon **moves backward, away from the opponent, while recovering**, rather than merely standing up in place.
- The movement should **behave like a knockback/repel displacement** (a physical-looking impulse or displacement response), not a teleport. Keep it compatible with arena collision and existing knockback behavior.
- The backward direction uses **the opponent's current side** (PHY-059/060), while eligibility, grounded double-tap input and morphology-dependent recovery benefit remain as decided in PHY-049–060.
- This is an early-recovery repositioning behavior; it should not be confused with the opponent's attack knockback, and it does not automatically imply damage, invulnerability or guaranteed escape from combos.
- **Not yet decided:** magnitude, speed, duration, dependence on mass/morphology, whether movement and stand-up animation overlap fully, edge/wall collision, and invulnerability/hit vulnerability during movement.
- **Affected nodes:** grounded knockdown recovery state, directional double-tap recognition, backward physical displacement, collisions and animation transitions. No Unity implementation requested yet.

### PHY-062 — Fixed backward recovery recoil for all eligible Mons (DECIDED, 2026-10-18)

- **Option A adopted:** every Mon eligible for early knockdown recovery uses **the same backward recoil intensity**, independent of `MassaGeral` or other morphology parameters.
- Treat this as a **shared gameplay movement response** with fixed intended strength/displacement, rather than scaling the recovery recoil by body mass. Preserve the physical-looking knockback-style motion of PHY-061, not teleportation.
- Morphology **still determines eligibility** and the **time benefit** of early recovery (PHY-049/050); this decision only fixes the backward recoil across eligible Mons.
- The actual numerical distance, speed, animation timing, handling of collisions, and whether a fixed impulse or fixed travel distance best preserves the identical recoil result remain **to be established through playtesting**.
- **Affected nodes:** knockdown early-recovery action, backward-recoil movement, body/environment collision, animation/state transitions. No Unity implementation requested yet.

### PHY-063 — Brief invulnerability during early-recovery backward recoil (DECIDED, 2026-10-18)

- **Option B adopted:** a Mon performing the backward recoil of a successful early knockdown recovery receives a **short invulnerability window against incoming attacks** during that recovery movement.
- This is a **temporary, explicitly timed protection**, not permanent invulnerability for the entire knockdown or an automatic escape from every combo.
- Only Mons that satisfy early-recovery eligibility and correctly execute the grounded backward double-tap can access this movement (PHY-049–061). Recoil intensity remains the same for all eligible Mons (PHY-062).
- **Not yet decided:** exact invulnerability duration, where in the recoil animation the protected frames begin/end, whether grabs and sweeps bypass the invulnerability, whether environmental hazards/collision are affected, and how this interacts with the normal hurtbox.
- **Affected nodes:** early-recovery state, animation timing, attack-vulnerability flags/hurtbox evaluation, combat hit resolution and recoil movement. Do not implement speculative frame counts.

### PHY-064 — Invulnerability only during the initial part of early-recovery recoil (DECIDED, 2026-10-18)

- **Option B adopted:** the backward recoil of a successful early knockdown recovery grants invulnerability **only during an initial, limited window**, not throughout the full backward movement.
- Once that window ends, the Mon **can be hit again even if its backward recoil animation/displacement is still in progress**.
- The recoil remains a fixed-intensity backward displacement (PHY-061/062), and it is only accessible after the valid grounded recovery double-tap and morphology eligibility checks (PHY-049–060).
- **Not yet decided:** exact protected-frame duration, how the transition from invulnerable to vulnerable aligns with animation stages, interaction with throws/sweeps/environmental damage, and cancellation after the recovery movement. These are tuning/implementation details, not implicit approvals.
- **Affected nodes:** knockdown early-recovery action, recoil timeline, hurtbox/vulnerability state, animation, attack-hit resolution. No Unity code changes requested yet.

### PHY-065 — No attacks until early-recovery recoil finishes (DECIDED, 2026-10-18)

- **Option B adopted:** the Mon **cannot start an attack until the entire backward early-recovery movement/animation has completed**, even if the initial invulnerability window has already ended.
- During the later part of the recoil, the Mon is **vulnerable to incoming attacks** per PHY-064, while still being **unable to attack** until recoil recovery completes.
- A valid early-recovery recoil remains a fixed-intensity backward displacement (PHY-061/062), and only an initial portion of it is invulnerable (PHY-063/064).
- **Not yet decided:** whether ordinary movement, defense, or special actions can cancel/interrupt the recoil; how hitstun affects the unfinished recoil; exact animation and invulnerability durations.
- **Affected technical nodes:** early-recovery recoil state, attack availability, hurtbox vulnerability timing, animation and combat-state transitions. No Unity gameplay code changes requested yet.

### PHY-066 — Cannot defend again until early-recovery recoil finishes (DECIDED, 2026-10-18)

- **Option B adopted:** during the backward recoil animation/movement of an early knockdown recovery, the Mon **cannot start another active defense**; the entire recoil must finish first.
- Defense input during the recoil does **not cancel or interrupt the recoil**. Whether to buffer invalid commands is not separately approved; do not queue an automatic defense after the animation.
- Existing restrictions also prohibit **attacks until recoil completion** (PHY-065).
- The initial recoil portion retains its limited invulnerability window, while the later portion remains vulnerable to hits (PHY-063/064). Passive defense remains a separate morphology-derived baseline, not an active move activation.
- **Not yet decided:** whether normal movement inputs may alter the ongoing recoil, whether incoming hitstun interrupts the recoil, and exact animation/invulnerability lengths.
- **Affected nodes:** early-recovery recoil state, active-defense eligibility, attack eligibility, passive defense fallback, animation/state transitions. No Unity implementation requested yet.

### PHY-067 — Fixed trajectory during early-recovery backward recoil (DECIDED, 2026-10-18)

- **Option A adopted:** the early-knockdown-recovery backward recoil follows an **automatic, fixed backward trajectory**; directional inputs cannot steer, accelerate, slow or reverse it while the recoil is executing.
- This preserves the universal recoil behavior of PHY-061/062; the actual recoil strength/path is tuned later. The movement is physical-looking and respects arena collisions, **not a teleport or guaranteed fixed position regardless of obstacles**.
- Active attacks (PHY-065) and new active defenses (PHY-066) remain unavailable until recoil ends. The initial portion grants limited invulnerability, while the later portion is vulnerable (PHY-063/064).
- **Not yet decided:** how collisions with walls/obstacles affect the remaining recoil animation, whether incoming hitstun can interrupt the recoil, and exact travel speed/duration.
- **Affected nodes:** early recovery recoil controller, movement-input gating, arena collision resolution, combat state and animation. No Unity implementation authorized yet.

### PHY-068 — Backward recovery recoil stops at solid obstacles (DECIDED, 2026-10-18)

- **Option A adopted:** when the Mon's fixed backward early-recovery recoil collides with a solid wall, obstacle, or arena boundary, **horizontal displacement stops at the obstruction**. There is **no automatic rebound or opposite knockback**.
- **The recovery animation/state continues until its normal completion**, even when the backward translation is blocked early. The animation must not get stuck waiting for travel distance that can no longer be achieved.
- The fixed recoil remains non-steerable (PHY-067). Active attack and defense remain unavailable until the movement/recovery state ends (PHY-065/066), with early limited invulnerability followed by vulnerability (PHY-063/064).
- Arena collisions remain authoritative: do not move the Mon through solid geometry to satisfy recoil distance.
- **Not yet decided:** edge cases involving moving obstacles, wall-contact effects and whether the recoil's original animation duration is ever modified by other hitstun states. No Unity implementation requested yet.
- **Affected nodes:** recoil movement, solid collision resolution, animation/state timer, combat input lockouts.

### PHY-069 — An incoming hit interrupts vulnerable early-recovery recoil (DECIDED, 2026-10-18)

- **Option A adopted:** if an enemy attack successfully hits the Mon **after the initial invulnerability window of early-recovery recoil has ended** (PHY-063/064), the hit **interrupts the remaining recoil movement and recovery animation immediately**.
- The Mon enters its **normal damage reaction / hitstun or knockdown state**, according to the incoming attack's established effects. Do not force every attack to cause a knockdown.
- The recovery recoil does **not** continue playing through a valid damaging hit. Remaining fixed recoil displacement is canceled, then normal hit response/knockback collision rules apply.
- Hits during the valid initial invulnerability window are still prevented according to PHY-063/064; an attack must actually resolve as a hit to trigger interruption.
- **Not yet decided:** exact hitstun durations, animation blending, knockdown chaining, combo scaling and invulnerability interaction with grab/sweep categories.
- **Affected nodes:** early-recovery state machine, vulnerability timeline, hit resolution, hitstun/knockdown transitions and recoil movement. No Unity gameplay changes requested yet.

### PHY-070 — Fresh early-recovery opportunity after each new knockdown (DECIDED, 2026-10-18)

- **Option A adopted:** if a Mon is hit during the vulnerable portion of an early-recovery recoil, and the incoming attack causes another knockdown (PHY-069), that **new knockdown creates a fresh opportunity for early recovery** after the Mon makes ground contact again.
- The same conditions apply on every new knockdown: `Cabeça.Intelecto > 40%`, normalized `MassaGeral < 40%` (PHY-049); mass-dependent linear early-recovery benefit (PHY-050); and two quick backward taps **both after the new ground contact** (PHY-051–060).
- The new falling/grounded knockdown cycle resets the double-tap recognizer; inputs from the earlier recovery or in-flight phase do not count.
- The successfully executed new recovery follows the same backward recoil, fixed intensity, short initial invulnerability and vulnerability/interruption rules (PHY-061–069).
- **Do not add a one-time-per-match or one-time-per-combo early-recovery restriction.** There is also no guarantee of escaping subsequent combos.
- **Not yet decided:** any repeated-knockdown damage/combo scaling, exact recovery animation duration and invulnerability frame counts.
- **Affected nodes:** knockdown state reset, ground-contact recovery window, input gesture recognizer, eligibility and hitstun/recoil transitions. No Unity gameplay code changes requested yet.

### PHY-071 — Full attack-category invulnerability during the initial recovery recoil window (DECIDED, 2026-10-18)

- **Option A (“Total”) adopted:** during the **initial invulnerability window** of a successful early knockdown recovery recoil, the Mon cannot be hit by **ordinary strikes, sweeps or grabs**. The window covers all these enemy attack categories, not only direct strikes.
- This is a **temporary window**, not invulnerability throughout the full recoil or normal knockdown. After the window ends, the Mon becomes vulnerable again under PHY-064/069.
- This does **not** change PHY-044: sweeps and grabs still defeat an active guard, but **an invulnerable recovery state is different from guarding**.
- Eligibility and execution still require the morphology thresholds and grounded backward double-tap (PHY-049–060); recoil properties and attacks/defense lockout remain as established in PHY-061–070.
- **Not yet decided:** interactions with environmental hazards, stage damage, or non-attack special effects; exact invulnerability frames/timing. Do not expand the approved immunity to arbitrary environmental effects without a decision.
- **Affected nodes:** early-recovery invulnerability flag/hurtbox, strike/sweep/grab hit resolution, recoil animation phases and knockdown recovery state. No Unity gameplay implementation requested yet.

### PHY-072 — Full damage invulnerability includes environmental hazards (DECIDED, 2026-10-18)

- **Option A adopted:** during the **initial invulnerability window of successful early-recovery recoil**, the Mon is protected from **environmental damage** as well as all enemy attack categories (PHY-071).
- Environmental examples include damaging spikes, fire and other stage hazards. While the window lasts, they **do not deal damage** to the recovering Mon.
- This is a **brief invulnerability window**, not persistent immunity to environmental hazards after the window ends (PHY-064).
- **Important distinction:** immunity to damage does **not** implicitly disable solid collision, walls, boundaries or physical blocking. PHY-068 still requires recoil movement to stop at solid obstacles.
- **Not yet decided:** whether non-damaging environmental effects (e.g. slowdown, status conditions or forced movement) are suppressed, and exactly how long invulnerability lasts.
- **Affected nodes:** early-recovery invulnerability status, environmental damage resolution, hazard contact, recoil animation timing and physical collision. No Unity gameplay changes requested yet.

### PHY-073 — Ignore nondamaging environmental effects during initial recovery invulnerability (DECIDED, 2026-10-18)

- **Option A adopted:** during the **brief initial invulnerability window** of a successful early-knockdown-recovery recoil, the Mon also **ignores nondamaging environmental effects** such as slowdown, surface-induced movement penalties and external environmental push forces.
- Combined with PHY-071/072, the same window blocks **enemy attacks (including sweeps and grabs), environmental damage, and nondamaging environmental status/force effects**. After that window closes, these environmental effects apply normally again.
- **Solid geometry still blocks movement.** Invulnerability does **not** grant intangibility or allow clipping through walls/obstacles; the recoil still stops at a solid barrier (PHY-068).
- The recovery recoil itself continues along its preset path (PHY-067). Ignore external effects, not the movement generated by the recovery action.
- **Not yet decided:** the exact invulnerability duration, which kinds of non-damaging statuses might persist after the window, and interaction with non-environmental force effects not otherwise covered. No Unity code changes requested yet.
- **Affected nodes:** early-recovery invulnerability window, environmental effects application, forced movement/slowdown filters, damage resolution, solid-contact collision. Record only; implement in the smallest later test slice.

### PHY-074 — Hazard effects resume immediately when invulnerability ends (DECIDED, 2026-10-18)

- **Option A adopted:** if a Mon remains in contact with a hazardous or effect-producing environmental surface when the early-recovery invulnerability window ends, **the hazard/effect becomes applicable immediately**, without requiring the Mon to leave and make a new contact.
- While the initial invulnerability window is active, enemy attacks and all damaging/nondamaging environmental effects are ignored as established in PHY-071–073. **When the window closes, evaluate the Mon's current contacts and apply their ordinary effects again.**
- Example: if the Mon is still touching spikes, spikes may deal damage as soon as invulnerability finishes, subject to any normal hazard tick/cooldown rules established later.
- **No collision bypass:** walls and other solid objects continue to block movement at all times (PHY-068).
- **Not yet decided:** precise hazard tick rates, repeated-contact cooldowns, status-effect duration or immunity-window length. “Immediate” means no new-entry requirement, not overriding an as-yet-undefined periodic damage schedule.
- **Affected nodes:** early-recovery vulnerability transition, persistent environmental contact tracking, hazard/effect evaluation, damage/status application and collision resolution. No Unity implementation requested yet.

### PHY-075 — Universal early-recovery invulnerability duration (DECIDED, 2026-10-18)

- **Option A adopted:** every Mon eligible for early knockdown recovery receives **the same fixed-duration invulnerability window** at the beginning of the backward recoil, independent of morphology.
- Morphology still controls **eligibility** and **time saved** in early recovery (PHY-049/050); it does not change the duration of the invulnerability window.
- The invulnerability remains confined to the **initial portion of the recovery recoil** (PHY-063/064), protecting against enemy attacks, grabs, sweeps and environmental damage/effects (PHY-071–073).
- Once this universal window ends, current environmental contacts and incoming hits are evaluated normally (PHY-069/074). Solid collision always remains active (PHY-068).
- **Not yet decided:** actual window length in seconds/frames and its precise placement in animation; tune/validate these in a dedicated gameplay test rather than invent numbers.
- **Affected nodes:** early-recovery timing, invulnerability/vulnerability state, hit and hazard evaluation, animation. No Unity code changes requested yet.

### PHY-076 — Calibrate the universal invulnerability duration in Unity (DECIDED, 2026-10-18)

- **Option A adopted:** do **not** select an arbitrary numerical invulnerability duration at the design stage.
- When the early-recovery recoil mechanic is implemented and tested in Unity, make the shared initial invulnerability window **configurable** and **calibrate it through actual gameplay tests**.
- The playtest goal is to balance a reliable opportunity to escape follow-up combos against vulnerability during the remainder of the recoil (PHY-063–075).
- The duration remains **universal across all eligible Mons**, not modified by morphology (PHY-075). Its exact frame count/time will be recorded only after test validation.
- Keep solid collisions enabled and reapply current environmental effects as soon as invulnerability ends, per PHY-068/074.
- **Affected nodes:** recovery recoil timeline, invulnerability flag, collision/hit/hazard resolution, animation playtesting. This records a future implementation validation criterion; it does not authorize unrequested Unity code changes.

### PHY-077 — Invulnerability starts when the valid early-recovery input is recognized (DECIDED, 2026-10-18)

- **Option A adopted:** the brief early-recovery invulnerability begins **immediately when the second valid backward-direction tap is recognized**, provided the Mon is grounded and meets the morphology eligibility requirements (PHY-049–060).
- The Mon **does not wait for a visible backward-recoil animation frame** before gaining the invulnerability granted by PHY-063–076.
- Invulnerability duration remains **universal for eligible Mons**, with the precise duration to be calibrated through Unity playtests (PHY-075/076).
- The valid command also initiates the fixed backward recoil (PHY-061/062/067). Solid collisions stay active; protection includes attack and environmental effects as defined in PHY-071–074.
- **Not yet decided:** how to resolve a valid recovery input and incoming hit in the exact same simulation tick, nor exact animation synchronization and frame duration. Do not invent a frame delay between accepted input and invulnerability.
- **Affected nodes:** grounded knockdown input recognition, state transitions, invulnerability start, attack/hazard event ordering and recoil animation. No Unity code change requested yet.

### PHY-078 — Valid early recovery invulnerability wins same-tick incoming-hit tie (DECIDED, 2026-10-18)

- **Option A adopted:** when a valid backward double-tap for early knockdown recovery and an incoming enemy hit occur during **the same simulation tick**, process the **successful recovery input and resulting invulnerability first**.
- The Mon thus enters the early-recovery invulnerable window immediately and the same-tick incoming attack **does not cause damage or hitstun**, consistent with PHY-077 and the general defensive tie-break principle of PHY-042.
- This applies only when all recovery requirements are actually satisfied: grounded after knockdown, both taps after ground contact in the correct relative direction and timing, and morphological eligibility (PHY-049–060).
- If the input is invalid or comes in a later tick, a previously registered hit remains valid; do not roll back earlier damage. Environmental effects are likewise ignored during the new invulnerability window under PHY-072/073, while solid collisions remain active.
- **Implementation test later:** valid same-tick input + hit versus hit on preceding tick versus invalid recovery command; ensure input and eligibility resolve before committing same-tick damage.
- **Not yet decided:** exact fixed-step implementation mechanics and invulnerability duration (to be playtested under PHY-076). No Unity code change requested yet.
- **Affected nodes:** input evaluation, knockdown recovery, simulation event ordering, invulnerability state and attack resolution.

### PHY-079 — Recovery recoil displacement begins immediately upon valid input (DECIDED, 2026-10-18)

- **Option A adopted:** on recognition of the successful early-knockdown-recovery backward double-tap, **the backward recoil movement begins immediately**, at the same moment the initial invulnerability window starts (PHY-077/078).
- There is **no separate animation startup delay before the recoil starts moving**. Animation and physical displacement should respond together to the valid input.
- The recoil maintains its fixed non-steerable trajectory/intensity (PHY-061/062/067) and respects solid obstacles/walls, stopping translation on collision while recovery animation completes (PHY-068).
- Invulnerability applies only during the initial globally timed window (PHY-063/064/075/076). After that window expires, the Mon is hittable and an incoming hit interrupts the recoil (PHY-069), while attacking or defending remains unavailable until normal recoil completion (PHY-065/066).
- **Not yet decided:** numeric speed/distance, movement interpolation and animation duration; these require Unity playtesting, not arbitrary constants.
- **Affected nodes:** grounded recovery command recognition, recoil movement initiation, invulnerability transition, animation timeline and collision response. No Unity code change requested yet.

### PHY-080 — Early-recovery recoil uses an initial impulse with deceleration (DECIDED, 2026-10-18)

- **Option A adopted:** the backward displacement during early knockdown recovery begins **fast** and **progressively decelerates**, conveying the physical feel of a knockback/repel impulse.
- The motion begins immediately upon successful recovery input, together with invulnerability (PHY-077–079), and follows a **fixed, non-steerable backward trajectory** for all eligible Mons (PHY-061/062/067).
- As before, a solid wall or obstacle stops horizontal travel without rebounding, while the recovery animation runs to completion (PHY-068). Attacks and new active defenses remain unavailable until recovery finishes (PHY-065/066).
- This **does not authorize a specific friction curve, easing formula, velocity, distance or duration yet**. Calibrate physical/arcade feel in a Unity gameplay test, preserving the same intended movement across eligible Mons.
- **Affected nodes:** early-recovery recoil movement profile, physics velocity/deceleration, fixed-direction collision response, animation synchronization and invulnerability timing. No Unity implementation requested yet.

### PHY-081 — Ease-out deceleration for early-recovery recoil (DECIDED, 2026-10-18)

- **Option B adopted:** the backward early-recovery recoil uses **ease-out deceleration**, losing speed more quickly near the beginning and tapering toward a progressively smaller movement at the end, rather than reducing speed by a constant linear amount throughout.
- The recoil still **starts immediately with a fast impulse** on the valid early-recovery command (PHY-079/080). Its direction is fixed and non-steerable (PHY-067), with a common intended movement response across eligible Mons (PHY-062).
- **Ease-out describes the intended motion profile, not a final mathematical easing curve**. The specific curve, duration, maximum velocity and displacement require tuning/validation in Unity.
- If a wall or solid obstacle blocks the recoil, horizontal displacement stops without rebounding, while recovery animation continues (PHY-068).
- Invulnerability remains limited to the initial universal window (PHY-063/064/075), and attack/defense inputs cannot interrupt the active recoil (PHY-065/066).
- **Affected nodes:** recoil velocity profile, movement animation synchronization, collision handling, early recovery state and gameplay tuning. No Unity gameplay code modification requested yet.

### PHY-082 — Calibrate recovery recoil motion parameters in Unity (DECIDED, 2026-10-18)

- **Option A adopted:** the backward early-recovery recoil's **distance, initial speed and duration** will be **calibrated through gameplay testing in Unity**, not fixed to arbitrary numbers in advance.
- The tuning must preserve PHY-061/062/067/079–081: a common recoil for all eligible Mons; starts immediately with the valid command; fixed direction; initial fast impulse followed by **ease-out deceleration**.
- Playtesting must evaluate camera framing, arena dimensions, combat spacing, reach of enemy moves, recovery responsiveness and wall collisions. The movement must stop at solid obstacles while the recovery animation may finish (PHY-068).
- Keep parameters configurable until testing validates values; document chosen settings afterward. Do not imply that numeric balance targets are already decided.
- The universal initial invulnerability duration is calibrated separately per PHY-075/076 and does not follow Mon morphology.
- **Affected nodes:** recovery recoil movement parameters, animation timing, combat spacing, camera and collision response, playtest instrumentation. **This is a documentation decision, not authorization to modify Unity gameplay code now.**

### PHY-083 — Responsive fighting movement and arena-bounded Mon flight (DECIDED, 2026-10-18)

- **Mon baseline ground movement:** responsive, practically immediate horizontal walking speed, inspired by traditional Street Fighter; no long gradual acceleration ramp for standard directional walking. Morphology (e.g. `Pernas.Extensão`) may still determine walking speed. Special moves may use impulses or distinctive acceleration profiles.
- **Tamer movement:** a separate responsive platforming controller inspired by Mega Man X, including platform traversal and eventual movement techniques, consistent with PHY-003. This does not authorize implementation of a full dash/move catalog now.
- **Mon can jump and fly** as abilities/mechanics allow, **but remains inside the fight arena / fighting-screen play space**. Jumping or flying does not transition the Mon to the Tamer's platforming space or enable travel outside the bounded fighting area.
- **Arena limits must constrain Mon movement in the relevant horizontal/vertical dimensions**, including air movement and flight. Do not assume the current camera rectangle is literally the physical collider boundary; distinguish camera framing from configurable arena bounds in implementation.
- **Not yet decided:** maximum flight altitude, whether ceiling bounds are physical collisions or movement limits, flight control model, speed formulas, and any details of Tamer dash/jump. Preserve the validated dual-controller camera and switching model.
- **Affected nodes:** Mon movement (ground/air/flight), fighting arena bounds, camera framing, morphology-derived locomotion, Tamer controller separation. No Unity code change authorized by this design confirmation.

### PHY-084 — Soft altitude cap without physical ceiling collision (DECIDED, 2026-10-18)

- **Option B adopted:** Mon flight and upward airborne movement are constrained by a **maximum permitted altitude within the fight arena**, but the upper limit is **not represented as a physical solid ceiling collider**.
- At the upper boundary, upward movement is limited/clamped by the arena's air-movement rule, without producing a fake ceiling-impact response or rebound.
- This preserves the PHY-083 principle that Mons can jump/fly but cannot leave the fighting play space. Camera framing and actual arena altitude bounds are separate configurations.
- **Not yet decided:** whether the limit applies identically to jumps and sustained flight, how upward velocity/force is treated when the limit is reached, and numerical altitude values. Define only what the next test requires.
- **Affected nodes:** arena vertical bounds, Mon airborne controller, flight ability, camera verification. No Unity code change requested yet.

### PHY-085 — One shared arena altitude cap; morphology determines attainable height (DECIDED, 2026-10-18)

- **Option A adopted:** every Mon shares **one maximum altitude limit for the combat arena**, whether it reaches height through jumping or flying.
- The common cap is a world/arena constraint **without a solid ceiling collision** (PHY-084). A Mon's morphology, movement capabilities, and moves determine **how high it can actually rise**, potentially below the shared limit.
- Do **not** define a different artificial height cap for jumping versus flight. Both remain restricted to the fighting arena and do not move into the Tamer's platform layer (PHY-083).
- This does not grant every Mon access to flight: flight depends on its own capability/mechanic, still to be defined.
- **Not yet decided:** maximum numerical height, how vertical velocity behaves at the soft cap, mapping of morphology to jump impulse and flight control; tune when Unity vertical-slice tests require them.
- **Affected nodes:** shared arena bounds, Mon jump/air/flight movement, morphology-derived movement, camera framing verification. No Unity gameplay code changes requested yet.

### PHY-086 — Basic aerial glide governed by morphology drag (DECIDED, 2026-10-18)

- **Scope correction by user:** do **not** expand this into a detailed sustained-flight/propulsion system now. The initial airborne capability should feel like **Mario's cape glide**: the Mon jumps, then uses an aerial action to slow its descent.
- **Core physical effect:** greater applicable morphology **Arrasto** means **lower falling speed / longer airborne time** while gliding, under the already-shared arena gravity (PHY-002). The Mon still descends; sustained powered flight, indefinite hovering and rising by propulsion are **not** established by this decision.
- The user referenced a command involving **jump plus another action** (wording needs clarification when implementing controls). Do **not** invent an exact key binding, input chord or activation window at this design stage.
- Preserve the previously defined shared arena altitude cap for jumps and aerial movement (PHY-084/085). No transition to the Tamer's platforming space.
- **Morphology input details** (which regional drag parameters contribute and how they combine), glide steering, maximum descent speed, animations and tuning are postponed until the minimal Unity test actually needs them.
- This decision replaces the premature PHY-086 choice between continuous flight propulsion and repeated impulses. **Prioritize the simple glide vertical slice over speculative air-movement feature trees.**
- **Affected nodes:** Mon jump/fall controller, morphology-derived drag, air-action input, arena bounds, animation verification. No Unity code change requested yet.

### PHY-087 — Horizontal locomotion depends on general mass and leg extension (DECIDED, 2026-10-18)

- **User-confirmed rule:** the Mon's horizontal displacement/walking speed must depend jointly on **`.MassaGeral`** and **`Pernas.Extensão`**. Do not treat leg extension as its sole determinant or confine mass effects to impact/knockback only.
- Preserve the responsive **immediate walking input** chosen in PHY-083: morphology determines achievable movement speed, but normal walking need not introduce a long acceleration ramp.
- **Do not invent** the weighting, mathematical formula, normalization, numerical ranges or extreme-case balance now. Those are calibrated with the smallest playable morphology-movement test.
- **Affected nodes:** morphology-derived locomotion attributes, Mon ground movement, speed/debug display and Unity playtest parameters. This does not yet mandate changing code.

### PHY-088 — Jump impulse depends on leg mass, leg extension and total mass (DECIDED, 2026-10-19)

- **Option B confirmed:** Mon jump capability/height depends jointly on **`Pernas.Massa`**, **`Pernas.Extensão`** and **`.MassaGeral`**.
- Leg mass/structure contributes to the ability to produce the jump impulse; extension contributes to the movement's mechanical reach/impulse potential; total mass influences the resulting motion. This is a **gameplay design mapping**, not a claim that limb mass alone is a real-world physiological force.
- Maintain the universal gravity and shared combat-arena altitude limit (PHY-084/085). A morphology's attainable jump height may be below the arena maximum.
- **Do not preselect a formula, weights, minimum jump height, or numeric limits**. Validate the mapping and tune through the smallest Unity jump morphology test.
- **Affected nodes:** anatomy/morphology derived jumping, Mon jump impulse, common gravity, arena bounds and test/debug UI. No gameplay implementation requested.

### PHY-089 — Partial horizontal steering during jumps (DECIDED, 2026-10-19)

- **Option B confirmed:** while airborne after a jump, the Mon can **correct its horizontal trajectory**, but **retains part of its existing horizontal momentum** rather than having the same instant acceleration, braking and direction reversal as when grounded.
- This is consistent with responsive ground walking (PHY-083), morphology-derived horizontal movement (`.MassaGeral` and `Pernas.Extensão`, PHY-087), and morphology-derived jumping (PHY-088). It should preserve a sense of weight without making air control unavailable.
- **Do not define numerical steering coefficients, drag, maximum aerial speed or reversal timing yet.** Test/calibrate in Unity when implementing the minimal jump-movement slice.
- **Affected nodes:** Mon airborne movement, horizontal velocity handling, shared physics/gravity, morphology movement modifiers and playtesting. No Unity code change requested.

### PHY-090 — Normal landing stops horizontal momentum when no input is held (DECIDED, 2026-10-19)

- **Option A confirmed:** when a Mon lands after normal aerial movement **without an active horizontal movement input**, its horizontal locomotion stops immediately on ground contact, rather than adding a mass-dependent landing slide.
- Preserve responsive on-ground fighting movement (PHY-083), partial aerial steering with retained momentum (PHY-089), and morphology-derived normal walking speed (PHY-087).
- This rule applies to **ordinary locomotion and landing**. It does **not** eliminate specific slides, knockback, forced movement, active attacks, or terrain-dependent physical interactions; those remain governed by their own mechanics.
- No landing friction coefficient, transitional frame count or animation-specific edge cases need definition at this fundamentals stage.
- **Affected nodes:** Mon ground/air movement transition, horizontal velocity/input resolver, knockback state separation, ground detection. No Unity gameplay changes requested yet.

### PHY-091 — Ground adhesion uses leg count, leg covering and general mass (DECIDED, 2026-10-19)

- **User-confirmed option B, plus explicit addition:** Mon ground adhesion/stability depends jointly on **`Pernas.Quantidade`**, **`Pernas.Revestimento`** (categorical features such as claws or suction cups, with context-specific effects), **and `.MassaGeral`**.
- The three contributions are considered for traction, slipping and resistance to external pushes/repel while in contact with ground; their precise relative impact and interaction with particular surfaces/terrain must be validated during the smallest Unity test, not assumed numerically.
- Do not reduce the categorical `Revestimento` to an unconditional scalar armor/grip coefficient: its effect must make sense for the contact surface and morphology. Do not assume more mass always produces a specific numeric grip multiplier without tests.
- Preserve PHY-090: ordinary landing without directional input halts horizontal locomotion immediately; separate forced displacement, knockback, environmental slip and special terrain behavior.
- **Affected nodes:** morphology/anatomy, contact and grounding, terrain/surface properties, external forces/knockback, locomotion stability. **No Unity code implementation authorized yet.**

### PHY-092 — Ground adhesion is contextual to terrain and morphology (DECIDED, 2026-10-19)

- **Option B confirmed:** ground adhesion depends on the interaction of **the ground/surface type** with Mon morphology, not morphology alone.
- The previously established `Pernas.Quantidade`, `Pernas.Revestimento` and `.MassaGeral` contributions (PHY-091) still apply. Categorical coverings such as claws and suction cups can provide different traction depending on which surface they contact.
- Terrain may therefore meaningfully alter slipping, stability and resistance to pushes; do not assign universal numerical bonuses to specific coverings or enumerate every terrain type at this fundamentals stage.
- **Deferred until a minimum test requires it:** terrain catalog, covering-versus-surface matrix, coefficients and exceptions. Preserve normal input-responsive ground control and PHY-090's stopping rule, with separately modeled forced displacement or slippery surfaces.
- **Affected nodes:** surface/contact classification, anatomy covering and leg-count morphology, general mass, traction and knockback response; no gameplay code change requested yet.

### PHY-093 — Grounded knockback depends on mass and surface adhesion (DECIDED, 2026-10-19)

- **Option B confirmed:** for a given external strike/impact, the Mon's physical knockback/displacement response depends on **`.MassaGeral`** and, **when grounded**, its **effective adhesion to the current surface**.
- Effective ground adhesion is contextual: `Pernas.Quantidade`, `Pernas.Revestimento`, `.MassaGeral` and terrain/contact type, per PHY-091/092. Avoid accidentally applying grounded traction as an additional resistance while fully airborne.
- **Airborne knockback still depends on mass and strike force**, but **ground adhesion is not applied while there is no ground contact**. Grounded and airborne reactions may therefore differ to the same hit.
- Do not predefine coefficients, knockback equations, per-move multipliers or animation exceptions now; validate the minimum playable hit/repel slice when implementation is requested.
- **Affected nodes:** combat impact/knockback, morphology-derived mass, surface traction and ground contact, airborne movement. No Unity code changes authorized by this documentation decision.

### PHY-094 — Hits use directional vector knockback (DECIDED, 2026-10-19)

- **Option B confirmed:** a combat impact can impart a **two-dimensional knockback vector**, with horizontal and vertical components. Depending on the particular move, this permits pushing, lifting, or launching an opposing Mon, rather than limiting ordinary impact responses to horizontal displacement.
- This reuses the same physical response model for grounded and airborne hits, while respecting grounded effective adhesion and general mass (PHY-093). Ground-contact traction may resist horizontal displacement when grounded; it must not be applied while airborne.
- No default launch angle, per-move vectors, strengths, input special cases, extra animation states or detailed combos are decided now. Tune and specify only when the smallest Unity strike/knockback slice requires it.
- Preserve the shared arena movement boundaries (PHY-084/085) and common gravity.
- **Affected nodes:** combat hit response/knockback, 2D motion, ground contact/traction, morphology-derived mass, arena bounds. This records design only; no Unity code implementation requested.

### PHY-095 — Solid body collision without passive body pushing (DECIDED, 2026-10-19)

- **Option A confirmed:** under ordinary movement, two Mons' solid bodies **block one another**; neither can pass through the other and **walking into an opponent does not passively shove or displace them according to mass/traction**.
- Distinguish this from **explicit combat knockback or other forces**, which can displace a Mon according to their separate vector-impact physics (PHY-093/094). Avoid turning ordinary locomotion collision into an unintended attack.
- Existing intentionally defined intangibility/phasing techniques remain possible exceptions, and do not alter the default solid-body rule.
- Do not decide collision shape details, per-animation contact reactions, or separation tolerances now; tune them in the minimum Unity collision test.
- **Affected nodes:** Mon–Mon solid collision, walking movement constraints, combat force/knockback separation and any explicit phase-through state. No gameplay code change requested yet.

### PHY-096 — Lateral body collision during jumps; aerial crossovers permitted (DECIDED, 2026-10-19)

- **Option B confirmed:** opposing Mons' bodies block lateral overlap during normal movement, including airborne lateral contact, but **one Mon may jump above and cross over the other** as in conventional fighting games.
- An opposing Mon's body **does not act as a standable platform**. Passing above an opponent must not cause the airborne Mon to land on the opponent's head.
- This refines the normal rigid Mon–Mon collision decision (PHY-095), preserving blocking on comparable lateral/body heights while permitting jump-over positioning. Explicit strike/knockback physics and intentionally defined intangibility remain independent.
- Do not decide collider geometry, height thresholds, exact overhead crossing frames or implementation details before the minimal Unity test.
- **Affected nodes:** Mon–Mon body collision/filtering, jumping/groundedness detection, jump-over positioning and landing, combat hit detection separation. No Unity code change authorized.

### PHY-097 — Automatic facing toward opposing Mon (DECIDED, 2026-10-19)

- **Option A confirmed:** a Mon **automatically faces the opposing Mon** when the combatants switch left/right positions, like a traditional Street Fighter duel.
- Facing is determined by relative opponent position, not independently chosen using a manual turn command. Horizontal movement away from the opponent therefore remains compatible with facing the opponent.
- Keep this as a core combat-space rule across ground and air positioning, including jump-over crossovers (PHY-096); no extra animation frames, directional-input exceptions or turnaround windows are prescribed now.
- **Affected nodes:** opponent relative positioning, Mon orientation/facing, attack direction/hitboxes, input movement interpretation and camera verification. This records design only, not authorization to implement code.

### PHY-098 — Punch reach uses arm and torso extension; kicks require analogous anatomy (DECIDED, 2026-10-19)

- **User confirms option B:** punch reach depends on **`Braços.Extensão` and `Torso.Extensão`**. The torso's contribution allows reach via body projection rather than relying on arm length alone.
- **User adds that kicks also exist:** kick reach should analogously reflect **`Pernas.Extensão` and `Torso.Extensão`**. This is a core distinction in the basic moveset, not a punch-only game.
- **User-defined core face-button layout (PlayStation labels):** □ = weak punch; ✕ = weak kick; △ = **strong punch that intrinsically incorporates the Mon's element**; ○ = **strong kick that intrinsically incorporates the Mon's element**. The strong physical/elemental attack is a unified move, **not** a normal strong strike plus an optional elemental variant or separate activation command. The physical strike remains rooted in anatomical reach and contact, with its element integrated into execution/effects. Do not silently remap the current prototype's keyboard/controller bindings before implementing and testing the new layout. Exact elemental manifestations, damage and availability require future implementation design.
- Avoid prescribing attack damage, hitbox equations, combo strings or animation timing now; they should follow the smallest testable basic-moveset slice.
- **Affected nodes:** punch/kick move categories, morphology-derived move range, attack hitbox geometry, control input action mapping, future elemental move variants; existing universal moveset requires regression verification. No Unity code modifications authorized.

### PHY-099 — Elemental strong strikes can extend beyond physical contact (DECIDED, 2026-10-19)

- **Option B confirmed:** the elemental part of a **strong punch (△)** or **strong kick (○)** may **extend the effective range beyond direct physical limb contact**, depending on the Mon's element and the move.
- These remain unified **physical + elemental** moves (see PHY-098); an elemental reach extension does not create a separate optional attack command or remove the strike's physical/anatomical basis.
- Distinguish the **anatomical contact range** (arm/leg plus torso extension, PHY-098) from the **elemental effect/reach geometry**, so each can be tuned independently without relying on visual sprite changes.
- This is an **ability**, not a blanket rule that all elements increase reach. No default projectile, exact elemental shapes, range bonuses, hitboxes or effect timing are specified now.
- **Affected nodes:** strong-strike definitions, physical versus elemental reach/hit detection, anatomy and element morphology, combat effects, controller button semantics. No Unity code changes requested.

### PHY-100 — Physical strike potency incorporates limb mass, total mass and movement dynamics (DECIDED, 2026-10-19)

- **Option B confirmed, with explicit user addition:** the physical potency/impact of punches and kicks depends on **the striking limb's mass**, **`.MassaGeral`** and **movement dynamics** at impact. The Mon's **acceleration may contribute** to the resulting strike, as well as motion/speed.
- For a punch consider the relevant arm mass; for a kick consider the relevant leg mass. Their anatomy and movement participate in the physical component of the attack. The existing strong elemental strike remains a unified physical-plus-elemental move (PHY-098/099).
- Distinguish **movement acceleration** from current velocity conceptually; **do not assume an arbitrary real-world equation or that merely accelerating in any direction buffs an attack**. The contribution of movement and acceleration needs to respect the strike's relevant direction and be validated through a small gameplay test.
- **No formula, coefficients, frame sampling, numeric damage values or animation-specific modifiers are selected yet.** They belong to implementation/tuning, not fundamental design.
- **Affected nodes:** strike potency calculation, arm/leg anatomy, Mon movement velocity/acceleration, combat impact vector/knockback, eventual element effects. No Unity code implementation requested.

### PHY-101 — Damage and physical knockback are distinct outcomes (DECIDED, 2026-10-19)

- **Option B confirmed:** an attack's **damage** and **physical knockback/displacement** are distinct results, not obligatorily proportional. An attack may cause high damage with little displacement, or low damage with strong repel/launch.
- Physical knockback retains its established dependence on impact vector, receiving Mon mass, and grounded traction where applicable (PHY-093/094); damage can follow its own combat-effect rules. Shared move parameters may influence both, but **neither outcome is derived solely from the other**.
- Do not establish damage equations, stun duration, combo implications, numerical coefficients, or special-move exceptions now; those are later test/tuning topics.
- **Affected nodes:** combat hit resolution (damage versus impulses), health, knockback/vector motion, morphology-driven attack potency, terrain contact. **No Unity code change requested.**

### PHY-102 — Tenacity affects maximum health; layered armor and recoverable health concept (PARTLY DECIDED, 2026-10-19)

- **Option A explicitly confirmed:** `TenacidadeGeral` influences **maximum HP**, but does **not** also apply a generic passive damage-reduction multiplier to each incoming hit. Active defense and other specific mechanics remain separate.
- **User-proposed design direction, not yet completely specified or committed as a mechanical contract:** a separate **outer armor bar** and an **inner life/health bar**. Health lost to damage would **recover gradually by itself**, with the **recovery becoming slower after successive damage events / accumulated damage**. This is a candidate high-level combat survivability loop, not permission to invent timings, rules or exact resource equations.
- **User raised an open architecture question:** whether **damage to armor** and **damage to health** should be separate outputs so that **armor-piercing effects** can be added **in the future**. Do not establish armor piercing, damage classes, bypass percentages or a full damage taxonomy now.
- To minimize future rework, consider **separating armor and health as damage destinations/resources** without mandating distinct physical/elemental damage types. This is an architectural recommendation awaiting user confirmation, not a settled PHY-102 rule.
- **Reconcile later when implementing:** previous design established a single HP pool (no per-limb HP). A separate armor bar would be an independent defensive resource, not per-limb HP, and its relationship to active guarding/tenacity must be expressly confirmed.
- **Affected nodes:** maximum HP derived from tenacity; possible armor resource and health regeneration; combat hit resolution; future armor piercing. Only tenacity's absence of passive per-hit damage mitigation is fully decided here; no Unity code change requested.

### PHY-103 — Damage category determines priority between health and armor (DECIDED, 2026-10-19)

- **User's explicit option C:** incoming damage is routed by its **damage type** to one of the two defensive resources as its **priority destination**.
- **Piercing damage (`perfurante`): prioritize the health/life bar.**
- **All non-piercing damage: prioritize the armor bar.**
- **Priority does not yet prescribe exclusive targeting, overflow, bypass percentages, splitting between bars, or what happens if a preferred bar is exhausted.** Decide such details only when implementing and testing the health/armor damage slice.
- This supersedes the premature binary options “armor first for every strike” versus “both simultaneously for every strike.” Keep damage and physical knockback independent (PHY-101).
- The two-bar health/armor concept from PHY-102 is now **necessary to this selected routing model**; the exact source of armor maximum, regeneration/depletion mechanics, and how slower health recovery accumulates with damage are still undecided. The health pool remains global, not per limb.
- **Affected nodes:** damage type/category, combat hit routing, separate armor and health resources, UI bars and eventual recovery, future move damage definitions. No Unity gameplay code change requested.

### PHY-104 — Armor does not regenerate naturally during combat (DECIDED, 2026-10-19)

- **Option A confirmed:** the outer **armor resource does not regenerate on its own** during the current fight; damage/depletion to armor persists rather than gradually refilling through passive recovery.
- The inner health/life resource is conceptually recoverable over time, with its recovery becoming slower as damage is accumulated (PHY-102); timing, slowdown curve, cap and triggers are not decided.
- **Do not infer that armor can never be restored by an explicit future ability/item or between fights**: the decision concerns **natural regeneration during combat only**.
- Preserve damage-type priority: piercing strikes prioritize life; other types prioritize armor (PHY-103). Routing overflow and exceptions remain undecided.
- **Affected nodes:** separate armor and health resources, passive regeneration rules, UI displays, combat damage routing. No Unity code changes requested.

### PHY-105 — Armor depletion exposes health without additional stagger penalty (DECIDED, 2026-10-19)

- **Option A confirmed:** once armor reaches zero, subsequent non-piercing strikes that would normally prioritize armor **apply their damage directly to health**.
- **No automatic extra knockback, vulnerability multiplier, stun or special armor-break penalty** is introduced simply because armor was depleted.
- Piercing strikes already prioritize health regardless of remaining armor (PHY-103). Damage and knockback remain separately calculated outcomes (PHY-101).
- The armor bar still does not regenerate naturally during a fight (PHY-104); the health bar's gradual recovery and slowing after accumulated damage remain a separately defined concept (PHY-102).
- No numerical coefficients, excess-damage/overflow rules for a single strike that depletes armor, or armor-break animations are specified now.
- **Affected nodes:** damage destination selection at zero armor, health/armor resource state, combat hit routing and UI. No gameplay implementation authorized by this documentation update.

### PHY-106 — Excess non-piercing damage carries from armor into health (DECIDED, 2026-10-19)

- **Option A confirmed:** when a non-piercing strike prioritizes armor and **its damage exceeds the remaining armor**, the remaining armor is depleted and **all excess damage from that same hit is applied to health**.
- Example: with 10 armor remaining, a 30-damage non-piercing hit deals **10 to armor + 20 to health** (before any other explicitly defined move-specific rules). Excess damage is **not discarded**.
- Consistent with PHY-103/105: piercing attacks prioritize health, while damage prioritizing armor routes directly to health if armor is already zero. Armor does not recover passively (PHY-104).
- Does not decide special piercing overflow handling, alternate damage distributions, armor max source, recovery rate or numerical balancing; defer until needed.
- **Affected nodes:** damage distribution/overflow, combat hit resolution, armor and health resource tracking, damage UI feedback. No gameplay implementation authorized.

### PHY-107 — Maximum armor equals total tenacity (DECIDED, 2026-10-19)

- **Explicit user decision:** **`ArmaduraMáxima = TenacidadeGeral`**. The Mon's maximum armor resource is the **sum of the anatomical regions' tenacity contributions**, following the established global-addition rule, including the established repeated-limb accounting.
- `Revestimento` remains a **categorical surface/anatomy trait**, not the direct numeric source of the armor bar. `Tenacidade` is the quantitative structural resistance property formerly called `Dureza` (PHY-017).
- This **does not modify** the existing rule **`VidaMáxima = f(MassaGeral, TenacidadeGeral)`** or create passive percentage-based damage reduction from tenacity (PHY-102). A single morphological parameter can currently contribute to both maximum resources; revisiting that coupling requires a separate explicit decision.
- Armor still **does not regenerate naturally during combat** (PHY-104); non-piercing damage prioritizes armor and overflows into health (PHY-103/106), while piercing damage prioritizes health.
- Numerical normalization and scaling from morphology values to UI resource points are deferred until the Unity vertical slice. **No code change requested.**
- **Affected nodes:** morphology aggregation, derived maximum armor, global health/armor resource model, combat hit routing, UI bars.

### PHY-108 — Armor is form-specific, not incrementally evolved (DECIDED, 2026-10-19)

- **User correction to the prior proposed question:** **armor does not evolve incrementally** when morphology points are earned. Armor is defined by the Mon's **current form**.
- The current form's `TenacidadeGeral` determines that form's maximum armor, consistently with PHY-107. Changes to armor capacity are associated with **actual form changes**, not gradual morphology gain.
- **When changing form, damage suffered by the previous form does not carry over to the newly assumed form.** Do not treat armor as one continuously degraded resource pooled across different forms.
- Preserve the already decided general rules: armor does not passively regenerate during combat (PHY-104), and non-piercing damage prioritizes armor with excess reaching health (PHY-103/106).
- **Still unresolved:** if returning to a previously used form restores that form's previously damaged armor, or starts it in another condition; whether the user's “damage from another form doesn't persist” applies exclusively to the armor resource or to both armor and health. **Do not invent either rule.**
- **Affected nodes:** form transformation, per-form armor maxima and damage state, health/armor UI switching and damage routing. No code implementation requested.

### PHY-109 — Full armor on every entry into a form (DECIDED, 2026-10-19)

- **User's explicit option C:** when a Mon returns to a previously used form (e.g. A → B → A), it enters that form with **the form's armor at full capacity**, even if that same form had lost armor earlier in the battle.
- More generally, on **each actual form change**, the entering form starts with its full current-form armor, derived from that form's `TenacidadeGeral` (PHY-107/108). Damage/armor depletion from earlier uses of this form or from other forms is **not retained** across form entry.
- Within any one continuous stay in a form, armor **does not naturally regenerate** (PHY-104). This form-change restoration is **not** passive armor regeneration, and gaining morphology points without changing form does not restore armor.
- **Still undecided:** whether life/HP damage persists across transformations. This decision specifically governs **armor**; do not infer a health reset.
- **Affected nodes:** form entry/switching, armor current/max initialization, defensive resource state and UI. No Unity gameplay code changes requested.

### PHY-110 — Health damage persists across transformations (DECIDED, 2026-10-19)

- **Option A confirmed:** changing forms **fully restores the entering form's armor** (PHY-109), but **does not heal or reset damage already suffered by the Mon's health/life bar**.
- Health is a persistent combat resource across form changes; armor is a fresh, form-specific resource on each form entry. Health may continue its separately proposed gradual natural recovery (PHY-102), but transformation itself is **not an instant health heal**.
- This maintains consequences for damage that bypasses or exceeds armor, including piercing priority (PHY-103) and non-piercing overflow (PHY-106).
- **Not yet decided:** if different forms have different derived maximum HP, the exact rule for carrying current HP across a change in its maximum (fixed damage, fixed percentage, current points/clamping etc.). Do not invent a conversion rule.
- **Affected nodes:** persistent health combat state, form switch resource initialization, armor reset, max-health recalculation and health/armor UI. No gameplay implementation requested.

### PHY-111 — Preserve health percentage across form changes (DECIDED, 2026-10-19)

- **Option B confirmed:** when a form change alters `VidaMáxima`, preserve the Mon's **current health fraction**, then express it against the new form's maximum HP.
- Example: 60/100 HP (60%) in form A becomes **120/200 HP (60%)** in form B. Transformation **does not reset the health fraction** or heal the Mon to full.
- This refines PHY-110: damage to life persists **as a fraction of the life bar**, not as an unchanged absolute damage-point total. The armor bar independently resets to the entering form's full `TenacidadeGeral` (PHY-107–109).
- No numerical rounding, minimum/maximum HP or regeneration timing details are specified; defer them to Unity validation.
- **Affected nodes:** form-specific derived max HP, health fraction mapping on transformation, armor resource reset, persistent battle health state, health/armor UI. No Unity gameplay code changes requested.

### PHY-112 — Health regeneration slows by hit count, not damage magnitude (DECIDED, 2026-10-19)

- **Explicit user correction:** the progressive reduction in the health regeneration rate depends on the **number of hits received**, **not the intensity of damage** dealt by each strike.
- The accumulated **hit count** is the design driver for regeneration slowdown; a strong hit does not count as proportionally more hits merely because it deals more damage. Do not substitute total damage taken or current health percentage as the driver.
- **Not yet decided:** whether a hit that damages **only armor** also counts toward this accumulated hit count, or only a hit that actually damages **health**; whether wholly blocked/missed attacks count (do not assume they do); the precise regeneration rate/curve and any reset rule.
- Health still recovers gradually per PHY-102; armor does not naturally regenerate (PHY-104). Changes in form preserve the current health percentage (PHY-111).
- **Affected nodes:** combat event/hit counting, health regeneration rate, armor-versus-health damage routing, debug feedback. No Unity code changes requested.

### PHY-113 — Only hits damaging health slow natural health regeneration (DECIDED, 2026-10-19)

- **Option B explicitly confirmed:** only a hit that **actually removes health/life** counts toward the cumulative hit count used to slow natural health regeneration (PHY-112).
- Hits whose damage is **entirely absorbed by armor** do **not** add to the regeneration-slowdown count. A single hit that exhausts armor and carries excess damage into life **does count once**, because it damaged health (PHY-106).
- Each qualifying hit counts as **one hit irrespective of how much health damage it causes**; the slowdown is based on hit quantity, not damage magnitude or the remaining life percentage.
- Maintain established routing: piercing damage prioritizes health (PHY-103), and non-piercing damage prioritizes armor with overflow to life (PHY-106). No passive armor regeneration (PHY-104).
- **Not yet decided:** whether accumulated health-hit count and its slowdown persist through transformations, the exact regeneration curve and reset conditions. Do not invent numeric penalties or timings.
- **Affected nodes:** hit-resolution event, health-vs-armor damage accounting, regenerative slowdown state, form transitions and debug/UI. No Unity code implementation requested.

### PHY-114 — Health regeneration slowdown persists through form changes (DECIDED, 2026-10-19)

- **Option A confirmed:** the cumulative number of **hits that actually damaged health** and its consequent slowdown of natural health regeneration **persist when the Mon changes form**.
- Transforming does not reset the regeneration penalty. Life retains its percentage of the current form's maximum (PHY-110/111), while armor is restored to the newly entered form's full capacity (PHY-108/109).
- Hits absorbed completely by armor do not increase the life-hit count (PHY-113), and each life-damaging hit counts once regardless of damage magnitude (PHY-112).
- **Not yet decided:** the slowdown curve, base regeneration speed, and any reset outside the ongoing battle. Avoid unnecessary balance questions until Unity testing requires them.
- **Affected nodes:** persistent combat health and regeneration hit-count state, form changes, armor reset, UI/debug. No Unity gameplay implementation requested.

### PHY-115 — Health regenerates continuously during combat (DECIDED, 2026-10-19)

- **Option A confirmed:** the Mon's health **recovers continuously during combat**, including while it moves, attacks, or is otherwise fighting. Regeneration does **not** require waiting for a damage-free interval.
- The rate of recovery progressively slows based on the **number of hits that actually dealt health damage** (PHY-112/113), irrespective of damage intensity. That accumulated slowdown persists across form changes (PHY-114).
- This does not regenerate armor, which stays depleted during a continuous period in the same form and is fully restored only on changing forms (PHY-104/109). Form changes preserve the health **percentage** (PHY-111).
- The precise regeneration rate/curve, frame processing and exceptional states are **not** defined now; tune them with a Unity prototype slice instead of extending speculative edge-case discussions.
- **Affected nodes:** continuously ticking health recovery, health-damaging hit counter and slowdown, transformation state, health UI. No Unity gameplay code requested.

### PHY-116 — Universal starting health regeneration rate (DECIDED, 2026-10-19)

- **Option A confirmed:** all Mons start combat with the **same baseline natural health regeneration rate**, rather than deriving the initial regeneration speed from their morphology or current form.
- During combat, life regenerates **continuously**, including while moving or attacking (PHY-115). The regeneration rate decreases as the **count of hits that actually damaged health** increases (PHY-112/113); the accumulated hit count and slowdown persist across form changes (PHY-114).
- A transformation **does not reset regeneration wear**, and only remaps the current health percentage to the new form's maximum HP (PHY-111). Armor remains a separate, form-specific non-passively-regenerating resource (PHY-104/109).
- **Deferred to actual Unity tuning:** whether the universal base rate is represented as HP per second or max-HP percentage per second, the slowdown curve and numerical values. Do not invent these or ask additional speculative balance questions now.
- **Affected nodes:** health regeneration baseline/configuration, hit-count slowdown state, form change health conversion, health UI. No Unity gameplay code requested.

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

- Fundamentals next: establish a minimal, testable mapping from anatomy to horizontal movement and falling drag; defer detailed glide inputs and timing until that Unity slice.
- Deferred morphology-scale question: when implementation needs it, define fixed maxima vs per-form normalization for Intelecto and Peso thresholds.
- Groundedness and normals: solid floor, walls, slopes, one-way platforms.
- Player-versus-environment collision and Mon-versus-Mon physical interaction.
- How anatomical parameters produce jump impulse, punch reach/hitbox and knockback without sprite changes.
- Units, timescale and playtest instrumentation.

## Immediate V1.7 hypotheses, not yet implemented

- Jump apex grows incrementally as selected anatomical capabilities increase while world gravity remains coherent.
- Punch attack reach/hitbox grows incrementally with anatomy while body collision and sprite remain unchanged.

These are test hypotheses, not fixed final formulas or balance targets.
