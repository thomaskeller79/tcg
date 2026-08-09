# Architecture — Continuous Effects & the Modifier System

*How cards that modify game state over time (buffs, debuffs, "becomes X" effects) fit the M1 engine that's already built — a design discussion stress-testing the architecture against content beyond Move/Attack, not yet a locked decision.*

**Status:** Open discussion — exploratory, continue here · **Date:** 2026-08-05 · **Updated:** 2026-08-06, 2026-08-09

---

## What "continuous effects" means (the title, explained)

Card-game design vocabulary (borrowed from MTG) splits effects into two shapes: a **one-shot effect** happens once and is done (*"deal 3 damage"* — apply it, move on), while a **continuous effect** keeps holding true for as long as it's active and has to be *recomputed*, not just applied once (*"+1 Attack until end of turn"* — every time anything asks "what's this creature's Attack?", the answer has to account for it, for as long as it lasts). The doc's title names both halves of the same problem: **continuous effects** is the *design* category (which card effects need this ongoing-recompute shape, worked out via the Rite examples below), and **the Modifier System** is the *engine mechanism* that implements it (`IQueryModifier`, `TrueState.ActiveModifiers`, `Query.Fold` — "don't store the answer, fold every active modifier into the baseline on every ask"). One doc, because the design question ("what counts as continuous?") and the engineering answer ("here's the one mechanism that handles all of them") were worked out together in the same sitting.

## Resume here next session

This document is the continuation point. What's settled and what's still open:

- **Settled (leaning, not yet implemented):** continuous effects apply in **append/timestamp order** — the order spells actually resolved in — not an MTG-style layer/priority system. Rationale and implications below.
- **Settled:** "X becomes Y" is a **query modifier** (a "set" kind, alongside the existing "add" kind), not a replacement effect. Replacement effects are a different axis entirely (they intercept *events*, not *queries*) and weren't needed for anything discussed so far.
- **✅ Done (2026-08-06):** the mechanism to add/remove a modifier through the event pipeline, and end-of-turn expiration/cleanup. `IModifier`/`ModifierId`/`ModifierDuration` (`Queries/IQueryModifier.cs`), `AddModifierIntent`/`RemoveModifierIntent` + their events (`Events/EventIntent.cs`, `Events/Event.cs`), a minimal `Modifiers/ModifierPipeline.cs` direct entry point, and `Turns/ExpireModifiersEffect.cs` wired into `StandardPhases`'s `"End"` phase. `IQueryModifier.Priority` was dropped in favor of natural list-append order (the leaning below, now implemented and regression-tested — see `Modifiers/ModifierPipelineTests.cs`'s order-dependence test). Two reusable generic modifier kinds (`IntDeltaModifier`, `IntSetModifier`) ship in `Queries/GenericModifiers.cs` covering the "add"/"set" cases from the worked example below without a bespoke type per card.
- **✅ Done (since this doc's last update):** casting is real. `CastCreatureCommand`/`CastRiteCommand` (`Commands/Command.cs`) and `Spells/SpellPipeline.cs` implement the full pay-mana → apply-effect pipeline for both card shapes (`RulesEngine.LegalCommands`/`Apply` wire them in). A Rite's effect is `RiteEffectIds.Damage` (→ `DamageIntent`, pre-existing) or `RiteEffectIds.Heal` (→ the now-built `HealEvent`, `Events/Event.cs` — mirrors `DamageEvent` with the opposite sign, no overheal cap). This closes gaps #1 and #4 below; **this document's own modifier mechanism was never touched to build it** — Rite effects are one-shot events, not continuous modifiers, so nothing here needed to change.
- **Still genuinely open:** nothing discussed here is itself a *continuous* effect yet — the two Rites built (Firebolt/damage, Mend/heal) are both one-shot. The worked example below (+1 Attack until end of turn, "attack becomes 0") remains **unbuilt as card content** — the modifier mechanism it would use has existed and been tested since 2026-08-06, but no card actually exercises it yet.
- **Still not fully specified:** the general card schema for effects beyond damage/heal. `CardDefinition` (`State/CardDefinition.cs`) now carries `EffectId`/`EffectAmount` — but its own doc comment calls this "a small, explicitly-scoped placeholder... standing in for the general card-effect system `design-continuous-effects.md` flags as still undesigned," i.e. it's a two-case placeholder (damage/heal), not the Track A schema this document originally deferred to (`card-data-and-editor.md` steps 5–6, still not done).

Suggested next step unchanged in spirit, updated in specifics: the Rite/Spell *pipeline* is no longer the gap — the next thing that would actually exercise this document's modifier mechanism is a card whose effect is genuinely continuous (a buff/debuff Rite, "until end of turn" or permanent), which the current two Rites don't need.

---

## Context

M1 (headless rules core, all 6 slices) is built and tested — see `PLAN.md` §8. Before moving on to M1.5 (the debug UI), we stress-tested whether the architecture actually holds up once real card content (beyond the built-in Move/Attack abilities) gets added, by walking through concrete scenarios rather than reasoning abstractly. This document is the record of that walkthrough.

## The mechanisms already in the engine, and which effects use which

Three distinct axes exist in the code today, and picking the right one per effect matters:

**Query/modifier layer** — for values that are *continuously recalculated*, never stored. `Query.ResolveAttack`, `ResolveMaxAp`, `ResolveAbilityIds`, `IsVisibleTo` (`src/Leyline.RulesCore/Queries/Query.cs`) all fold a baseline through `TrueState.ActiveModifiers` via `Query.Fold`. `IQueryModifier<TResult>` (`Queries/IQueryModifier.cs`) is the interface — as of this writing it has **zero implementations anywhere in the codebase**; the fold mechanism has existed since Slice 1 but nothing has ever populated `ActiveModifiers`.

**Direct event mutation** — for values that are *persistent, accumulated state*, not derived fresh each time. `ActorState.Life` is the example: `DamageEvent` does `target.Life -= Amount` directly (`Events/Event.cs`); there's no `Query.ResolveLife`. This matches D14 (no auto-heal, damage persists). A "heal" or "set life to X" effect belongs here, as a new event type, not as a modifier.

**Replacement effects** — a third axis, for intercepting an *event* before it applies ("if this would take damage, prevent it"). `IReplacementEffect` (`Events/IReplacementEffect.cs`) already exists, already wired into `EventPipeline.FoldReplacements`, and — like `IQueryModifier` — has zero implementations yet. Nothing discussed so far needed this axis; it's worth naming explicitly because "X becomes Y" *sounds* like a replacement ("attack becomes 0") but isn't one — it's a continuous answer to a query, not an interception of an event.

## Worked example: two Rites on a 2/2/6 creature

Scenario: a 2/2/6 creature (Attack/Life/AP). Ally casts a Rite: *"target creature gains 1 attack until end of turn and 2 life (permanently)."* Opponent then casts a Rite: *"target creature's attack becomes 0 and its life becomes 1."*

- **+1 Attack until end of turn** → a new `IQueryModifier<int>` instance (`QueryKind="Attack"`, `Resolve(ctx, current, state) => current + 1`), added to `ActiveModifiers`, removed at end of turn.
- **+2 Life, permanent** → a new `HealEvent` (mirrors `DamageEvent`, opposite sign), applied once, done. No modifier, no expiration.
- **Attack becomes 0** → also an `IQueryModifier<int>`, same `QueryKind="Attack"`, but `Resolve(ctx, current, state) => 0` (ignores `current`). Same interface, different kind of modifier — no interface change needed.
- **Life becomes 1** → a `SetLifeEvent` (or a `HealEvent` variant that assigns instead of adds) — `target.Life = 1`. One-time, not continuous, same reasoning as the +2 case.

## Ordering continuous effects: append order, not MTG-style layers

The open design question was: when "+1 Attack" and "Attack becomes 0" are both active, which wins, and does it depend on cast order?

**MTG's answer** is a layer system: "set" effects (layer 7b) always apply before "+X" effects (layer 7c), *regardless of cast order* — so the result is always the same (net +1 over the set value) no matter which spell was cast first. This buys order-independence across effect *categories*, at real cost: it's one of the most famously confusing parts of Magic's rules, and every card author has to know which layer their effect lives in.

**Decision (leaning): use append/timestamp order instead** — effects apply in the order they actually resolved, full stop. Worked through by hand:
- Ally's +1 first, then opponent's "becomes 0": `2 → +1 → 3 → becomes 0 → 0`. At end of turn the +1 modifier expires; recompute: `2 → becomes 0 → 0`. Final: 0.
- Opponent's "becomes 0" first, then ally's +1: `2 → becomes 0 → 0 → +1 → 1`. At end of turn: `2 → becomes 0 → 0`. Final during the turn was 1, but 0 again after cleanup.

So the outcome *can* depend on cast order (0 vs. 1), unlike MTG's layer-based result (always 1 during that window). We're accepting that tradeoff deliberately:
- It matches pillar 3 (`PLAN.md`) — "complexity lives in card combinations, not in fiddly rules." A layer system is exactly the kind of fiddly-rules complexity that pillar warns against.
- It's not an unprincipled shortcut — MTG *already* uses timestamp order as the tie-breaker within a single layer. This proposal just drops the layer/category step and uses timestamp as the *only* rule.
- It's simpler to implement *and* simpler than what was originally sketched: because only one spell resolves at a time (the priority/stack already guarantees no true simultaneity), insertion order into `ActiveModifiers` already *is* timestamp order — no explicit timestamp field needed. That means `IQueryModifier.Priority` and `Query.Fold`'s `.OrderBy(m => m.Priority)` (`Queries/Query.cs`) are unnecessary and could be dropped in favor of natural list order.

Not adding an escape hatch for "this effect applies before others regardless of order" speculatively — if a specific future card needs it, design it then.

## Extending the type system: Item and Building (brief recap)

Also discussed, more briefly:
- **Building/Structure** → a third `ActorState` subclass (`State/ActorState.cs`), same move as `ChampionState` in Slice 2: reuses Combat, board occupancy, and destruction with no changes, just never has Move/Attack in its `AbilityIds` and AP stays 0. Cost: `Query.cs`'s three `CreatureState`/`ChampionState` pattern-matches become a three-way switch — worth factoring into a shared `IHasCardDefinition` interface at that point, not before.
- **Item** → doesn't fit `ActorState` (not an independent board actor). Would be the first real consumer of the modifier layer: equipping registers an `IQueryModifier` on the carrier; unequipping removes it. Needs a lightweight `ItemState` and an `Items/ItemPipeline.cs`, following the same per-subsystem-folder pattern as `Combat`/`Terrain`/`Champions`.

## Concrete gaps this exposed

Status as of 2026-08-09 (originally written 2026-08-06):

1. **✅ Done.** Casting is real: `CastCreatureCommand`/`CastRiteCommand` (`Commands/Command.cs`), `Spells/SpellPipeline.cs`. `DeclareCombatCommand` is no longer the only "activate an effect" pathway.
2. **✅ Done.** `AddModifierEvent`/`RemoveModifierEvent` (`Events/Event.cs`) now add/remove from `TrueState.ActiveModifiers` (`List<IModifier>`) through the normal intent → pipeline → event flow.
3. **✅ Done.** `ExpireModifiersEffect` (`Turns/ExpireModifiersEffect.cs`), wired into `StandardPhases`'s End phase, sweeps `UntilEndOfTurn`-duration modifiers generically.
4. **✅ Done (Heal), still open (Set).** `HealEvent` (`Events/Event.cs`) now exists — mirrors `DamageEvent`, opposite sign, no overheal cap. `SetLifeEvent` (an assign-rather-than-add variant) still doesn't exist; nothing has needed it yet.
5. **Partially done.** `CardDefinition` (`State/CardDefinition.cs`) gained `EffectId`/`EffectAmount` — enough to express the two Rites built (`RiteEffectIds.Damage`/`Heal`). Its own doc comment is explicit that this is "a small, explicitly-scoped placeholder... standing in for the general card-effect system this document flags as still undesigned" — the real Track A schema (`card-data-and-editor.md` steps 5–6) is still not done.

Gaps #1–#3 required no changes to `EventPipeline`, `TrueState`'s shape, `Query`, or `Combat` — additive, following patterns already established (per-subsystem pipeline folders, per-phase effect lists, intent → event mapping), confirming the prediction below. #4/#5 are genuinely new surface area (a new event type, two new `CardDefinition` fields) but still additive in the same sense — nothing existing had to change shape to accommodate them.
