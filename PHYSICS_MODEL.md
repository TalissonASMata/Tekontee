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

- PHY-004: Should platforms permit passing upward through them and landing from above (one-way), or remain solid on all sides by default?
- Groundedness and normals: solid floor, walls, slopes, one-way platforms.
- Player-versus-environment collision and Mon-versus-Mon physical interaction.
- How anatomical parameters produce jump impulse, punch reach/hitbox and knockback without sprite changes.
- Units, timescale and playtest instrumentation.

## Immediate V1.7 hypotheses, not yet implemented

- Jump apex grows incrementally as selected anatomical capabilities increase while world gravity remains coherent.
- Punch attack reach/hitbox grows incrementally with anatomy while body collision and sprite remain unchanged.

These are test hypotheses, not fixed final formulas or balance targets.
