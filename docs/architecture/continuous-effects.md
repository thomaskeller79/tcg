# Architecture — Continuous Effects & the Modifier System

*How cards that modify game state over time (buffs, debuffs, "becomes X" effects) fit the engine.*

**Related:** `PLAN.md` §8 Track B for implementation status.

---

## What "continuous effects" means

Card-game design vocabulary (borrowed from MTG) splits effects into two shapes: a **one-shot effect** happens once and is done (*"deal 3 damage"* — apply it, move on), while a **continuous effect** keeps holding true for as long as it's active and has to be *recomputed*, not just applied once (*"+1 Attack until end of turn"* — every time anything asks "what's this creature's Attack?", the answer has to account for it, for as long as it lasts). **Continuous effects** is the *design* category; **the Modifier System** is the *engine mechanism* that implements it (`IQueryModifier`, `TrueState.ActiveModifiers`, `Query.Fold` — don't store the answer, fold every active modifier into the baseline on every ask).

## Three mechanisms, and which effects use which

**Query/modifier layer** — for values that are *continuously recalculated*, never stored. `Query.ResolveAttack`, `ResolveMaxAp`, `ResolveAbilityIds`, `IsVisibleTo` (`src/Leyline.RulesCore/Queries/Query.cs`) all fold a baseline through `TrueState.ActiveModifiers` via `Query.Fold`. `IQueryModifier<TResult>` (`Queries/IQueryModifier.cs`) is the interface.

**Direct event mutation** — for values that are *persistent, accumulated state*, not derived fresh each time. `ActorState.Life` is the example: `DamageEvent` does `target.Life -= Amount` directly (`Events/Event.cs`); there's no `Query.ResolveLife`. This matches D14 (no auto-heal, damage persists). A "heal" or "set life to X" effect belongs here, as an event type, not as a modifier.

**Replacement effects** — a third axis, for intercepting an *event* before it applies ("if this would take damage, prevent it"). `IReplacementEffect` (`Events/IReplacementEffect.cs`) is wired into `EventPipeline.FoldReplacements`. "X becomes Y" *sounds* like a replacement ("attack becomes 0") but isn't one — it's a continuous answer to a query, not an interception of an event.

## Worked example: two Rites on a 2/2/6 creature

Scenario: a 2/2/6 creature (Attack/Life/AP). Ally casts a Rite: *"target creature gains 1 attack until end of turn and 2 life (permanently)."* Opponent then casts a Rite: *"target creature's attack becomes 0 and its life becomes 1."*

- **+1 Attack until end of turn** → a new `IQueryModifier<int>` instance (`QueryKind="Attack"`, `Resolve(ctx, current, state) => current + 1`), added to `ActiveModifiers`, removed at end of turn.
- **+2 Life, permanent** → a new `HealEvent` (mirrors `DamageEvent`, opposite sign), applied once, done. No modifier, no expiration.
- **Attack becomes 0** → also an `IQueryModifier<int>`, same `QueryKind="Attack"`, but `Resolve(ctx, current, state) => 0` (ignores `current`). Same interface, different kind of modifier.
- **Life becomes 1** → a `SetLifeEvent` (or a `HealEvent` variant that assigns instead of adds) — `target.Life = 1`. One-time, not continuous, same reasoning as the +2 case.

## Ordering continuous effects: append order, not MTG-style layers

When "+1 Attack" and "Attack becomes 0" are both active, which wins, and does it depend on cast order?

**MTG's answer** is a layer system: "set" effects (layer 7b) always apply before "+X" effects (layer 7c), *regardless of cast order* — order-independence across effect *categories*, at real cost: one of the most famously confusing parts of Magic's rules.

**This engine uses append/timestamp order instead** — effects apply in the order they actually resolved, full stop:
- Ally's +1 first, then opponent's "becomes 0": `2 → +1 → 3 → becomes 0 → 0`. At end of turn the +1 modifier expires; recompute: `2 → becomes 0 → 0`. Final: 0.
- Opponent's "becomes 0" first, then ally's +1: `2 → becomes 0 → 0 → +1 → 1`. At end of turn: `2 → becomes 0 → 0`. Final during the turn was 1, but 0 again after cleanup.

So the outcome *can* depend on cast order (0 vs. 1), unlike MTG's layer-based result (always 1 during that window) — a deliberate tradeoff:
- Matches pillar 3 (`PLAN.md`) — "complexity lives in card combinations, not in fiddly rules." A layer system is exactly the kind of fiddly-rules complexity that pillar warns against.
- Not an unprincipled shortcut — MTG *already* uses timestamp order as the tie-breaker within a single layer; this just drops the layer/category step and uses timestamp as the *only* rule.
- Simpler to implement: because only one spell resolves at a time (the priority/stack already guarantees no true simultaneity), insertion order into `ActiveModifiers` already *is* timestamp order — no explicit timestamp field needed.

No escape hatch for "this effect applies before others regardless of order" exists speculatively — if a specific future card needs it, design it then.

## Item and Structure/Building

The base `ActorState` shape (Slice-2 pattern: reuse Combat, board occupancy, and destruction with no changes) extends to Structure the same way it extended to Champion — see `structures-items.md`/`ownership.md` for the current Structure/Item design; this doc covers only the modifier-system mechanics those types build on. Equipping an Item registers an `IQueryModifier` on the carrier; unequipping removes it — the first real non-Rite consumer of the modifier layer described above.
