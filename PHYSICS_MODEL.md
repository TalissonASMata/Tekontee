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

- PHY-050: When designing the morphology range, should the percentage thresholds for Intelecto and Peso be relative to a fixed maximum for each parameter, or to the current Mon/form's morphology distribution?
- Groundedness and normals: solid floor, walls, slopes, one-way platforms.
- Player-versus-environment collision and Mon-versus-Mon physical interaction.
- How anatomical parameters produce jump impulse, punch reach/hitbox and knockback without sprite changes.
- Units, timescale and playtest instrumentation.

## Immediate V1.7 hypotheses, not yet implemented

- Jump apex grows incrementally as selected anatomical capabilities increase while world gravity remains coherent.
- Punch attack reach/hitbox grows incrementally with anatomy while body collision and sprite remain unchanged.

These are test hypotheses, not fixed final formulas or balance targets.
