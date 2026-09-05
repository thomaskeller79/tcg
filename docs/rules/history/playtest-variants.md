# Playtest Variants — Alternatives Worth Trying

*`history/decisions.md` records the **current** rule for each decision — one answer, the one the game
actually runs on. This document is different: it's where we park **alternatives that lost (or
haven't been decided) but are worth keeping on hand** to actually playtest later, with the
reasoning for/against each, so a promising idea doesn't get lost the moment a default is picked.
Promote an entry into `history/decisions.md` the moment it's actually adopted; leave it here otherwise.*

**Status:** Living list · **Date:** 2026-08-09

---

## Champion realm-constraint movement (D9)

**Current rule:** while connected, the Champion may only move onto hexes it has itself bonded,
at `2AP`; `0AP` Collapse Network drops the whole bond set and frees movement. See `../champions.md`.

### Variant 1 — network follows the Champion at a AP premium · SUPERSEDED
The Champion is the network's root; a pricier `2AP` move keeps the network **active** as it
relocates (nothing pauses), while the cheap `1AP` move (same as any creature) lets the network
**collapse** instead — every bonded terrain no longer reachable is reversibly paused, restored
the moment a path reconnects (same mechanic as enemy blocking, just self-inflicted).

**Why replaced:** never actually restricted *where* a disconnecting move could go, so "keep the
network" was rarely worth the 2AP premium in practice.

### Variant 2 — "reachable through" bonded cells, not "is" a bonded cell · SUPERSEDED
The first lock-in draft required the destination to merely be *reachable through* bonded,
enemy-free cells — in practice this meant any hex adjacent to a bonded cell qualified, not just
the bonded cells themselves.

**Why replaced:** a Champion could still drift one hex outside its actual claimed territory while
"staying connected" — playtesting showed this reads as "I can still move outside of the network."
The current rule requires the destination to *be* a bonded cell, full stop.

## Defend cost (D15)

**Current rule:** `0*AP` — free, but at most once per turn per actor, completely decoupled from
remaining AP. See `history/decisions.md` D15 for the full rationale.

### Variant 1 — `1!AP` (Exhaust) · REJECTED, concrete bug found
Defending costs `1!AP` (exhaust to 0), at most once per turn, gated on having AP left. **Why it
failed** (the scenario that killed it): two mirrored creatures (5 Life / 3 Power / full AP)
trade blows — attacker attacks (exhausts itself, default `3!AP`), defender defends (exhausts
itself too, `1!AP`), both take 3 damage. Both are now at 0 AP, but AP only refreshes on its own
controller's *own* Beginning phase — the defender's controller acts next, so it refreshes almost
immediately; the attacker's controller doesn't act again for a full round. In that window the
former defender attacks the still-exhausted former attacker for a free, undefended kill. Net
effect: **attacking first is strictly worse than being attacked** — an incentive against
attacking, the opposite of the intended design. Not a balance nitpick, a structural bug in how
the rule interacts with turn-based AP refresh.

### Variant 2 — the "recovery pulse" idea · proposed 2026-08-09, not evaluated or implemented
Instead of decoupling Defend from AP (what `0*AP` does), fix the *refresh timing* that actually
caused Variant 1's bug, and let Defend stay a normal AP-gated action:
- **(a)** Defend (and other actions) go back to being priced with an ordinary AP cost, e.g.
  `1!AP` again — no bespoke rule for Defend specifically.
- **(b)** New general rule: every actor regains **`1AP`** at the **beginning of the opponent's
  turn** too, on top of the existing full refresh on its own Beginning phase — a second, smaller
  refresh pulse every round instead of one big one. This is what closes Variant 1's gap: an
  exhausted attacker isn't stuck at 0 AP for the opponent's *entire* turn, just until that small
  trickle arrives.
- **(c)** New cost flavor, **`x!!AP`** ("double-exhaust"): like `x!AP`, but the actor also
  **skips the opponent's-turn recovery pulse from (b)** — its exhaustion genuinely lasts the
  full round, not just until the next small trickle. Flavor: the action was so costly it doesn't
  even recover for the quick pulse. `!` vs `!!` becomes a real per-card dial: "this drains you
  for a moment" vs. "this drains you for the whole cycle."

**Argued for (the idea's own appeal):** doesn't special-case Defend at all — a creature "can't
defend" as a *consequence* of being out of AP, exactly like every other AP-gated action, matching
MTG's tapped state (tapped means *both* "can't block" *and* "can't activate tap abilities," one
unified state, not two separate rules). Also opens design space elsewhere: any `1!AP`-costed
ability becomes meaningfully riskier once there's a recovery pulse to weigh against it (skip the
pulse via `!!` for a stronger effect, or take the plain `!` and stay defensible sooner).

**Argued against (the idea's own cost):** touches a foundational rule — AP refresh was, until
now, a single well-understood event (your own Beginning phase, full refresh, no partial states).
Adding a second, smaller refresh point makes "how much AP does an actor have right now" depend on
*two* timing rules instead of one, for every actor, every turn — a permanent complexity increase
across the whole engine, not a one-off card effect. Plus a third cost-flavor axis (`!!`, next to
`!`/`*`/`^`) for players and card designers to track.

**Not decided.** `0*AP` is the live rule (fixes the same bug with a smaller, more contained
change: one existing mechanism reused, no new refresh timing, no new cost flavor). Variant 2 stays
here as a real alternative worth an actual playtest if `0*AP`'s side effects (e.g. Defend being
fully AP-independent) turn out to feel wrong once more content exists.

---

## Structure control (D24)

**Current rule:** dynamic — a Structure is funded/controlled by whoever *currently* bonds the
terrain cell it sits on, tracking bond changes live, exactly mirroring Terrain's own rule (funding
can be a Companion; the strict controller is that Companion's own Champion, D28). See
`history/decisions.md` D24.

### Variant — fixed at build time · REJECTED in favor of dynamic
Whoever bonded the terrain when the Structure was cast keeps control permanently, even if that
bond later breaks or the cell gets re-bonded by someone else — Structures behave like Creatures,
owned for life once built.

**Argued for:** avoids a real board consequence the dynamic rule introduces — re-bonding
contested or newly-unbonded terrain (e.g. after the Companion that bonded it dies) lets the
opposing root **capture an enemy Structure positionally, without ever attacking it**. A player who
built a Structure might reasonably expect to keep it regardless of what happens to the ground
under it later.

**Argued against (why dynamic won):** fixed-at-build-time is a special case — every other
non-actor permanent (Terrain) already has control track the current bonder live; carving out an
exception for Structure just because it happens to sit on top of one breaks that symmetry for no
new capability. The "capture via re-bond" consequence isn't a bug either — it fits D8's existing
"denial is positional and reversible" shape and gives Structures a distinct vulnerability from
creatures (starve the land under it, don't fight it), which reads as a feature once framed that
way.

**Worth revisiting if:** playtesting finds Structure-capture-via-re-bond feels too strong (a
"free" removal with no combat, no cost) or too situational to matter (contested terrain rarely
gets fought over enough for this to come up) — either direction could argue for reverting to
fixed control.

---

## Creature/permanent location model (D31–D34)

**Current rule:** every spatial permanent's location is uniformly (terrain, layer); "vehicle" and
"transport" flavor (e.g. a Helicopter Item) is achieved through ordinary ability-granting
(Flying, cost changes) on the carrier, never by moving a creature off the board into a container.
See `history/decisions.md` D32.

### Variant — true containment ("inside" a Vehicle/Structure) · REJECTED, not built
A permanent could have a third kind of location beyond (terrain, layer): "inside" another
permanent (a Vehicle, or a Structure hiding a creature within it), removing it from the hex grid
entirely — with its own embark/disembark actions, capacity limits, visibility rules, and rules
for what happens to the contents if the container dies.

**Argued for:** more realistic for genuine transport/ambush flavor — a creature loaded into a
Vehicle would be fully off-grid and untargetable until it disembarks, and a container's death
could threaten its cargo, which ability-granting alone can't express.

**Argued against (why rejected):** doubles the location model for a flavor payoff most cards
don't need — a full containment system needs its own capacity, visibility, and
embark/disembark cost rules, all new surface area. Everything asked for so far (a Cannon
granting Ranged Attack, a Helicopter granting Flying + speed, "hidden in a building") is already
covered by the existing Item-grants-ability pattern plus Below-layer concealment (D31) — no
card yet needs a creature to actually leave the grid.

**Worth revisiting if:** a future card wants a creature to be genuinely untargetable/off-grid
while "loaded," or wants container-death-destroys-cargo — that's the point where ability-granting
stops being enough and true containment earns its complexity.

---

## Item Life (D34)

**Current rule:** an Item has no Life at all — never a combat participant, removed from play only
via an explicit printed effect (destroy/sacrifice), never by combat or splash damage. See
`history/decisions.md` D34.

### Variant — Item has Life, destroyable by damage · REJECTED
Give Item a Life total like Structure, so generic damage/AoE effects could destroy a loose item
lying on a hex (e.g. a Spell that burns everything on a cell).

**Argued for:** more uniform with Structure (which does have Life); opens a design space where
area-damage effects also threaten loose equipment, not just creatures.

**Argued against (why rejected):** Structure's Life exists specifically because it's a combat
participant (attackable, D24) — an Item never is, so a Life total would only ever matter for a
niche AoE-destroys-loose-items interaction nobody has asked for. Simpler and more legible to keep
Item removal strictly card-driven ("destroy target Item" as a printed effect) than to give every
Item a hidden Life stat on the chance a future card wants to burn the ground.

**Worth revisiting if:** a real card idea wants area-damage to sweep loose items off a contested
hex as a genuine tactical option.
