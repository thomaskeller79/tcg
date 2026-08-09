# Playtest Variants — Alternatives Worth Trying

*`decisions.md` records the **current** rule for each decision — one answer, the one the game
actually runs on. This document is different: it's where we park **alternatives that lost (or
haven't been decided) but are worth keeping on hand** to actually playtest later, with the
reasoning for/against each, so a promising idea doesn't get lost the moment a default is picked.
Promote an entry into `decisions.md` the moment it's actually adopted; leave it here otherwise.*

**Status:** Living list · **Date:** 2026-08-09

---

## Defend cost (D15)

**Current rule:** `0*AP` — free, but at most once per turn per actor, completely decoupled from
remaining AP. See `decisions.md` D15 for the full rationale.

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
