# Neutral Permanents

*A third control state — **Neutral** — alongside Player A/Player B, giving every permanent type a defined answer for "what happens when nobody controls this." For Terrain/Structure/Item with no Behavior, Neutral just means inert (`ownership.md`). For any permanent capable of acting on its own, Neutral means it acts under a **Behavior** instead of a player.*

**Decisions:** D54–D58 (`history/decisions.md`)

---

## Control: A / B / Neutral, for every permanent type

Fully covered in `ownership.md` — every permanent's controller is derived by climbing its parent chain to the root, resolving to Player A, Player B, or Neutral. This doc only covers what happens once a permanent is Neutral and capable of acting.

## Behavior

A Neutral permanent capable of acting — it holds its own Activation Points (`economy.md`) and has at least one legal action available to it — follows a **Behavior**: a named keyword (e.g. `Aggressive toward X`, `Patrol`, `Flee toward X`, `Produce`) that expands to a complete, deterministic decision algorithm, the same way any other keyword (Flying, Ranged N) expands to fixed rules text. Internally, a Behavior is a triggered ability keyed to "this permanent's assigned Neutral Phase begins" — this is never exposed as such on a card.

- **Who can have a Behavior:** any permanent holding Activation Points (i.e., anything but Item) with at least one legal action available to it — this includes Creature, Companion, Structure, and Terrain, not just Actors, provided the action it needs is actually payable. An Activation-Points-only cost is always self-payable by a Neutral permanent (it holds its own Activation Points); a cost that also includes mana still needs a live root supplying it (a Neutral Companion still holding its private pool, for instance) — a fully parentless Neutral permanent can never fund a mana-inclusive ability.
- **Every Behavior must define a complete algorithm** — every reachable case resolved explicitly, corner cases included (e.g. "my target exists but no hex currently makes progress toward it"). There is no engine-level default for an unhandled case; an incomplete Behavior is a bug in that keyword's own definition, not something the base rules patch over. Where multiple options are equally valid (a tied-closest target, equally-suitable hexes to step toward it), the keyword must state how the tie breaks — a stated priority, a deterministic tiebreak, or a random pick from the engine's standard seeded RNG (the same source every other randomized effect draws from — nothing bespoke).
- **Target selection and reconsideration reuse the standard legal-target query** — the same check any attack or ability already uses (existence, type, any stated condition, and visibility/concealment, D19). A Behavior re-runs this check on its current target after every action it takes; if the target is no longer legal, it's discarded and the Behavior returns to target selection. This single, reused check is enough to produce correct deception on its own: if a target's true stats are disguised (Mimic), the Behavior acts on the claimed value like anyone else would, and only "discovers" the truth when an actual interaction (e.g. resolving an attack) tests the claim (D18) — the same belief-consistency model that already governs players.
- **No generalized "perception" or "Belief State" system.** A keyword that wants a range restriction states a plain distance number directly in its own text (e.g. "a creature satisfying X within distance 3"), layered on top of the standard legal-target query — not a bespoke vision mechanic. Anything a specific Behavior needs to remember beyond what's live-derivable at each decision (a current target, a Patrol's progress along its route, a Flee's remembered destination) is explicit, stored data scoped to that keyword's own definition — never a generic mirror of the full observer-view system. Unlike a player, a Neutral permanent has no memory of its own backing it the way `asymmetric-information.md`'s belief model leans on human reasoning — whatever it needs to remember has to be named and stored explicitly by its Behavior.

## Neutral Phases

Each round contains two Neutral Phases, one attributed to each player (`A's Neutral Phase` / `B's Neutral Phase`). **Both always occur, every round, unconditionally** — regardless of whether anything is currently assigned to either — specifically so the phase's mere occurrence or timing never reveals whether a hidden Neutral permanent exists; any content that happens during it is redacted per-observer exactly like everything else hidden in the game (`asymmetric-information.md`), never by skipping the phase itself. *(Their exact position in the turn/round structure — a freestanding phase between the two players' turns, vs. nested inside each player's own turn before End — is not yet decided; see `overview.md` §3 and `PLAN.md`.)*

A permanent's assigned Neutral Phase is a full mini-turn for it: its Activation Points refresh, then its Behavior runs, until it has no legal action left or exhausts its Activation Points. A Behavior's actions go through the exact same pipeline a player's own actions would — Move and Attack create the same traces, open the same priority windows, and are equally respondable by both real players.

When a permanent becomes Neutral (or is created directly as one) via a card effect, that effect's controller chooses which of the two Neutral Phases it's assigned to; the assignment persists until it's reassigned or the permanent becomes controlled. When a Neutral permanent originates from mission/scenario content instead of a card effect, the mission/scenario makes this assignment (`PLAN.md` §9 — mission content itself is deferred, not designed here).

## Creating and converting Neutral permanents

An effect may create a new permanent directly as Neutral, or convert an already-controlled one to Neutral (this conversion is always effect-driven — never an automatic consequence of some other game state). Either way, if the permanent is capable of acting (see above), the effect must specify its Behavior and which Neutral Phase it's assigned to. A Structure/Terrain/Item created or converted to Neutral with no stated Behavior simply sits inert — the pre-existing rule that nobody may activate an uncontrolled Object's ability (`ownership.md`) already covers it.

A freshly cast Neutral Creature/Companion/Structure is summoning-sick like any other cast permanent (`economy.md`) and can't act until its first Activation Points refresh.

Gaining control of a Neutral permanent ends its Behavior immediately.

## Invariant vs. mutable
- **Invariant:** every permanent's controller is exactly one of Player A / Player B / Neutral; a permanent capable of acting while Neutral always follows a complete, deterministic Behavior — never a player's ad hoc choice on its behalf; both Neutral Phases always occur every round, unconditionally.
- **Mutable (card-driven):** which permanents can become Neutral and how; which Behavior a specific instance runs; which Neutral Phase it's assigned to.

*(Resolved questions are cut once closed — the rule lives above and, for decision-grade calls, in `history/decisions.md`. Only genuinely open items stay listed above.)*
