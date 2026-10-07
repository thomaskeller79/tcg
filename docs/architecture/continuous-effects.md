# Architecture — Continuous Effects & the Modifier System

*How cards that modify game state over time (buffs, debuffs, "becomes X" effects) fit the engine.*

**Related:** `PLAN.md` §8 Track B for implementation status.

---

## What "continuous effects" means

Card-game design vocabulary (borrowed from MTG) splits effects into two shapes: a **one-shot effect** happens once and is done (*"deal 3 damage"* — apply it, move on), while a **continuous effect** keeps holding true for as long as it's active and has to be *recomputed*, not just applied once (*"+1 Attack until end of turn"* — every time anything asks "what's this creature's Attack?", the answer has to account for it, for as long as it lasts).

## Three places a value can come from

**Queried values** — continuously recalculated, never stored. `Rules/Stats.cs` answers `Attack`, `MaxLife`, `MaxAp`, keywords and abilities by folding: the printed value from the card definition, the permanent's own max-tier changes (below), its carried Items' bonuses, and every active `Modifier` (`State/TrueState.cs`) for that subject and stat, in append order. A modifier is a plain record (`Subject`, `Stat`, `Delta`, `UntilEndOfTurn`); "until end of turn" ones are removed in the End phase (`Rules/Turns.cs`).

**Max-tier changes stored on the permanent** — a *permanent* change to a printed value ("+2 Life permanently") is stored as `Permanent.MaxLifeDelta` / `AttackDelta`. These are max-tier properties (`docs/rules/object-properties.md` §3), so they survive Bounce and Flicker into the new Card or Trace (`CardObject`/`TraceObject` carry the same two fields), while modifiers and current values do not.

**Current values stored on the permanent** — persistent, accumulated state: `CurrentLife` (damage persists, D14), `CurrentAp`. Instructions change them directly (`Rules/Resolution.cs`); immediate consequences follow right after each instruction (`Rules/Consequences.cs`, D112).

**Not built yet:** replacement effects ("if this would be dealt damage, prevent it"), "becomes X" set-effects, per-observer claimed stats (D18 faces). The fold has room for a set-modifier the same way MTG's timestamp order works within one layer.

## Ordering: append order, not MTG-style layers

When "+1 Attack" and "Attack becomes 0" are both active, effects apply in the order they resolved — insertion order in `TrueState.Modifiers` is timestamp order, since only one trace resolves at a time. So the outcome can depend on cast order (0 vs. 1), unlike MTG's layer system — a deliberate tradeoff for pillar 3 ("complexity lives in card combinations, not in fiddly rules"). MTG itself uses timestamp order within a layer; this drops the layer step and uses timestamp as the only rule. No escape hatch exists speculatively — if a future card needs "applies before others regardless of order", design it then.

## Items

An equipped Item's bonus (`carrierAttack`, `carrierLife` in the card data) is folded into its carrier's queried stats while it is carried — no modifier object is registered; un-equipping or the carrier ceasing to exist simply ends it.
