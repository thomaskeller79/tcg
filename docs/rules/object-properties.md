# Properties of Game Objects

*What a game object is made of — Card, Trace, and Permanent as one connected chain — and how properties move (or don't) when one creates another, including backward: Bounce and Flicker.*

**Decisions:** D69, D70, D72, D73, D74, D75, D83, D88, D93, D97, D98, D99, D100, D101, D103 (`history/decisions.md`)

---

## 1. The object graph

Every game object belongs to exactly one domain, and each domain has exactly one kind of object in it (`overview.md` §1, D37/D59):

- **Mind** — **Card**. In Hand, Library, or Discard.
- **Aether** — **Trace**. In Past, Pending, or Future.
- **Matter** — **Permanent**. On the Island — an Actor (Champion, Companion, Creature, Structure) or an Object (Item, Terrain, Grave, Ruin) (D103).

Every game object — Card, Trace, and Permanent alike — carries a unique, monotonic **ID** (D61, D93; `neutral-permanents.md` §Permanent identity).

Two things that look like they might belong on this list don't:

- **Combat** is not a game object in this sense. It's a transient `{attacking units, target terrain, declared defenders}` record that's created and resolved in one step — it never occupies a zone, never has properties beyond that one resolution, and isn't part of the Card/Trace/Permanent chain.
- **Map** is not a Matter-domain object — it never becomes a Permanent and is explicitly excluded from the zone system (`overview.md` §1: "not part of any zone... cannot be referenced by other game objects"). It *is* a card with its own properties (Layout, home-ground/neutral-ground population, per-hex Terrain Type, landmarks, size), but its job is done once, at game setup, producing board data (and, per below, populating some starting permanents) rather than persisting as a queryable object during play.

**Three permanent-type cards skip the Mind domain entirely.** Champion and Terrain exist in a player's collection like any other card, but never occupy Hand/Library/Discard. **The exact mechanism by which they, and Map-populated Neutral Structures/Items/Creatures/Companions, become Permanents at setup is not yet decided** — it is *not* simply a direct Card→Permanent edge, since that would contradict Mind→Matter being forbidden (§2); it plausibly involves the Map itself as a participant in the process, not just the card and the permanent. See Open questions.

## 2. Domain transitions create objects; nothing ever moves

Casting a card doesn't relocate it into the Aether — it **creates** a new, linked Trace, while the card itself discharges into Discard (`glossary.md`'s **Zone** entry, D16). A resolving Trace doesn't relocate into Matter — it **creates** a new, linked Permanent. This is true in both directions: **Bounce** and **Flicker**, the two backward transitions, also create new objects rather than moving an existing one back. A bounced permanent's card can end up sitting in Hand while an entirely different, untouched card object from the same cast still sits in Discard — that's not a violation of "one object, one zone," because they're two separate objects, exactly the same way a cast card and the permanent it produced are two separate objects.

The six ordered cross-domain transitions this doc actually covers:

| Transition | Name | Status |
|---|---|---|
| Mind → Aether | **Cast** | Ordinary game progression |
| Aether → Matter | Trace resolves | Ordinary game progression |
| Matter → Aether | **Flicker** | Creates a new Trace |
| Matter → Mind | **Bounce** | Creates a new Card, sent to whichever Hand the effect's own text names (D74 — e.g. "to its controller's hand," "to your hand") — this doc's property model has no separate "owner" field; the destination is ordinary card-level data resolved via the Controller walk (`ancestry.md`), the same as any other controller-referencing effect. |
| Mind → Matter | — | **Forbidden, invariant.** Magic always leaves a trace — even an Instant-speed effect passes through the Aether (D45, D92) rather than skipping it. |
| Aether → Mind | **Remand** | Creates a new Card, destination resolved identically to Bounce (D74). For a permanent-producing card, follows the identical rule as Matter→Mind (Trace and Card are close enough in the property chain that no special case is needed). For a **Spell**, the general cost/choice-lock, target-reset rule (§4) applies unchanged — confirmed against a worked example (D70): cost, `X`, and every modal choice (including a repeated "choose N times" selection) survive; each instruction's target is stripped back to a bare placeholder. |

**This table is incomplete on purpose, not by oversight.** An existing Permanent using one of its own abilities (including the base Move/Attack/Ascend/Descend defaults, or any printed activated ability) *also* creates a Trace — call it **Activate** — and this is the routine, constant case during play, not a rare one. It is not simply "Matter → Aether" in the same sense as Flicker: Flicker **removes** the Permanent as part of creating the linked Trace, while Activate leaves the acting Permanent completely untouched — same object, same zone, unconsumed, free to act again. That makes Activate closer in shape to Cast (the source object isn't destroyed either way) than to Flicker, but not identical to Cast either (Cast at least relocates/spends the card to Discard; Activate doesn't even do that).

**Resolved (D75): Activate needs no rule of its own for either of its two open pieces.** Its Trace is created by the identical pay → choose → target procedure Cast already uses (`interaction-stack.md`, D70) — cost, `X`, and modal shape are read off the ability currently sitting on the Permanent (already a stat the Permanent carries, §3) rather than off a Card, but the mechanism is the same. Duration is likewise ordinary per-ability data (§3) — the four base defaults (Move/Attack/Ascend/Descend) happen to be printed at Duration 0 (D45's "Physical Trace"); any other activated ability defaults like an ordinary Cast-Trace. Its back-reference to the activating Permanent is its `source` (below, D97).

**An object that leaves every zone ceases to exist (D97).** A trace fading from Past, a card removed from Discard — there is no exile zone and no record state; nothing can bring it back. Every object's **`source`** links to its immediate predecessor (a permanent → its trace → its card, or → the permanent that activated the ability). Only this backward link is stored; "what this trace created" is a query over `source`. When an object ceases to exist, links through it are **joined up**: whatever had it as `source` takes over its `source`. So a card and the permanent it produced stay linked while both exist, but a permanent can be reached through its trace only while the trace lasts — permanents get harder to reach this way as they age. Card text wanting "the card that created it" follows `source` until it reaches a card; a link to an object that no longer exists resolves to nothing. `source` is not the Ancestry parent: a Trace's parent is flattened to a Champion or Companion for payment (D83), discarding exactly the producing permanent `source` keeps.

Bounce/Flicker/Remand never need to locate the permanent's originating card or trace — they read only the object being transformed, in its *current* state. That's what makes them well-defined even when there's no ancestor to find: a Neutral, Map-placed creature with no card in any collection; a permanent whose originating card has since been removed from Discard entirely. Neither case is special — both fall straight out of §3.

**Champion and Terrain can never be bounced, full stop** — a separate restriction, not derived from the property model. **A Neutral permanent, Map-placed or otherwise, is a legal Bounce/Remand target like any other (D74)** — §3's model never required an ancestor card to exist, and there's no base-rule reason to exclude one. What a *specific* effect actually does to it depends entirely on that effect's own destination wording: an effect that returns a permanent "to its controller's hand" has no controller's hand to reach for a Neutral permanent and simply fizzles against it (the same per-instruction fizzle as an illegal target, D33) — that's a property of the effect's wording, not a targeting restriction the rules impose.

## 3. Markovian object creation

**A newly created object's properties are a pure function of its immediate predecessor's *current* state — nothing further back, in either direction.** This is what makes Bounce/Flicker well-defined (§2) and what governs how much of a permanent's history survives leaving the Island.

**Property tiers.** Card and Trace both carry only the **max**-tier of every stat, plus cost and the ability list — this is what "printed on a card" means. Permanent adds the **current**-tier on top: current-Attack, current-Life, current-AP alongside max-Attack, max-Life, max-AP. Abilities (static, triggered, activated) are stats in this same sense — they inherit and extend the same way numeric stats do.

**A Trace's own properties include Duration** — how long it persists in Past before fading (default 5 rounds, card-overridable via `Duration X`, D50). **Physical / non-physical** is a Trace property of its own (D98): the base Move/Attack/Ascend/Descend defaults (D45) produce physical traces, which are printed at Duration 0 (zero Past residency). It is not a Duration value, so a card changing a physical ability's Duration doesn't make its trace non-physical.

A backward creation event (Bounce, Flicker) reads the predecessor's current state and keeps only whichever properties the *new* object's own type actually has room for. Current-tier values are dropped every time, because neither Card nor Trace has a current-tier slot at all — not because they're specially excluded, but because that field was never part of their schema in the first place (the same reason a Card has no board position). Concretely: a Permanent whose max-Life was genuinely, permanently raised during play (not merely healed) carries that higher max-Life into a bounced/flickered card; ordinary combat damage, and any effect that only ever modified a *current* value, does not survive. This asymmetry is intentional — it's exactly what makes a truly permanent buff a more expensive, more consequential effect than a temporary one, and letting a temporary buff get "banked" through a bounce is accepted design space, priced accordingly, not something the rules engine needs to prevent.

**Recovery policy is a separate, per-stat property.** The (max, current) shape is uniform across stats, but how current recovers toward max is not: current-Life never recovers on its own (persists until an explicit heal, D14); current-AP fully recovers every Beginning phase (existing rule). A future stat could need a third policy — this is deliberately left per-stat, not hardcoded once for all of them.

**Forward instantiation and backward creation are the same table, read in opposite directions (D70).** Whether a property survives a transition is never decided per-transition — one call for Cast, a separate call for Bounce, another for Remand — it's decided once, per *property*, by which tier(s) that property's slot exists at (Card / Trace / Permanent, above). Forward, a transition fills in whichever fields the destination tier's schema adds that the source didn't have. Backward, a transition keeps whichever fields the destination tier's schema still has room for, and drops the rest — the exact same lookup against the identical table, run the other way, not a second rule. Cost, `X`, and a resolved mode sit at Card-tier-and-up, so they survive every direction; target has no Card-tier slot at all, so it's dropped going backward and freshly re-chosen going forward; current-Life/current-AP are Permanent-tier only. The complete table — one row per property, across every object type — is §5, still being built (`PLAN.md` Track A item 4).

**There are no tokens (D88).** Anything created in the game — by a card effect, a Structure's ability, or the Map — references a real card, named on the creating card (never "create a 2/2 creature with…"), so every permanent carries every property its type needs (a real mana cost included) and Bounce/Flicker need no special case for it. A created object's card (e.g. Fire Warrior) is a normal card, legal in a deck; bouncing a created object gives its Champion a real, castable Card.

**A Bounce/Flicker-created object is likewise not a token.** It still carries the same name — and the name is the reference to the card definition (D99) — as any other instance of that card, so it's a real instance of a real, deckbuilt card type — just a *new* physical object. The original (discarded) card, if one exists, is untouched and left exactly where it was; the number of card objects that exist can grow (via Bounce) or shrink (a card ceasing to exist, D97) over the course of a match. This is accepted as fine — deck legality is fixed at construction, and on-Island uniqueness restrictions only ever care about live Permanents, never about how many inert copies are sitting in Discard.

## 4. What locks in vs. what always resets

Once a variable value is resolved — an X-cost, an X/X/X stat line, a modal card's chosen mode (including a "choose `N` times from this menu, repeats allowed" structure, where each pick locks in independently) — it becomes a fixed, ordinary property of the object from that point on, exactly like a value that was printed from the start. This applies identically to a permanent-producing card and a Spell — a modal choice on a permanent (e.g. which of two entirely different stat/ability bundles it becomes) uses the same "choose" step (D72), not a separate mechanism. It flows forward and, per §3, backward: a creature cast for X=3 that gets bounced comes back as a flatly-costed, flatly-statted 3-mana 3/3/3 card, no longer variable. This holds because cost (even printed only as "X") and a chosen mode are properties of the object's *own* nature — the Card's schema always had a slot for them, unresolved or not.

**This is also why a lock-in decision is never made per-property in isolation — it follows directly from *when* a trace comes into existence at all (D70, `interaction-stack.md`).** Casting/activating is a strict pay → choose → target procedure, and a trace isn't a real, reachable object in the Aether until all three steps finish — so cost and every modal choice are *always* already resolved by the time anything (Remand included) can act on a trace; only target, the final step, can ever still be "live" enough to reset.

**A cost that consumes specific objects to be paid — sacrifice X creatures, discard N cards, pay N life — only ever locks in the magnitude (X, N), never the identity of what was consumed.** The specific creatures sacrificed are never tracked as a property of anything created afterward.

**Target never locks in.** No counterexample has been found to a single flat rule: whatever a card, trace, or triggered ability targets — a location, a creature, a player — resets, requiring a fresh choice at every instantiation, regardless of direction. A card's declared number/type of targets is ordinary Card-level metadata used to check cast legality, but it never holds an assigned target value at the Card level, in any form — that's the actual difference from cost, which does hold a value-shaped slot at the Card level even before it's resolved. A targetless Spell has nothing to reset, so Remand (§2) locks in everything about it (cost, mode) with nothing left over.

**Location is target's Matter-side identity, not a separate property.** For any permanent-producing card, the (single, mandatory, implicit) target *is* the destination — introduced first at the Trace level, inherited into the Permanent as **location**, the same underlying slot under a different name appropriate to a live, freely-mutable, continuously-updated fact (every Move/Ascend/Descend changes it) rather than a frozen, one-time choice. This is why location and target flow in *both* directions between Trace and Permanent specifically — forward at an ordinary cast, backward at Flicker, each time freshly read from whichever of the two objects currently exists — but never reach Card, which never had this field to begin with. A flickered permanent's new Trace gets its target from the permanent's *current* location, not from wherever it originally started — and this is a forced, single-candidate read, not a player choice, unless a specific card states otherwise.

## 5. Property inventory

One list per object type, built from two shared groups (D103). **Stored** properties are state; **derived** ones are queries over other state and never stored (one fact, one home, D101); **definition-only** ones live on the card definition, reached through the name, never on a game object. **Properties are never added or removed during play**, only their values change; "none" is an ordinary value (a Champion's parent, a Map-placed permanent's `source`). A type lacks a property only where giving it a value would change the rules (Terrain with Activation Points could act).

**General rules (all types).**
- **Name is immutable** and is the reference to the card definition; names are unique across card definitions (D99). The definition supplies printed originals and same-card identity. Every other property can be changed.
- **ID and timestamp** (D61, D93) and **`source`** (§2, D97) on every game object.
- **Subtypes** on every type that has a card (D99).
- **Abilities** are kept as ordered lists per kind: static, triggered, activated.
- **Copy restriction** is definition-only (D99): it is irrelevant once the game has started.
- **Elements** are always derived (D100): the Elements in the mana an object produces and in its rules text.
- **Short-term history** ("attacked this turn") is a query over the history track, not a property (D98; `glossary.md` **History track**).

**Card** (Mind). Stored: name, cost (CNF, D91), Speed, cast condition (D91), subtypes, rules text (effects in the normal form, `effect-form.md`, D95), ID, `source`. Spell-specific properties: `PLAN.md` Track A item 33.

**Permanent** (Champion, Companion, Creature, Structure, Item, Terrain).
- Stored: name; subtypes; static abilities; triggered abilities; ID and timestamp; location; parent; `source`; cost, Speed, cast condition (copied along the chain, unused on the Island; none for Champion and Terrain, which are placed at Setup).
- Derived: controller (D54), payer, Elements.
- **Location** is (terrain, Slice) — for an equipped Item, its carrying Actor. **A Terrain's location is fundamentally different**: its position in the hex grid, fixed, and itself what every other location points to. Terrain is the odd type in general; kept as is for now.
- **Parent** is stored for Terrain (its bond record), Creature and Companion (the payer, fixed at creation, changed only by an explicit control change, D55); read off the location for Structure (its terrain) and Item (its carrier, none while loose) (D101); always none for the Champion.

**Actor** (Champion, Companion, Creature, Structure) = Permanent plus:
- Stored: activated abilities; max- and current-Activation Points (no default, enter at 0, D89); max- and current-Life; Behavior assignment (Behavior plus neutral turn, one property, `neutral-permanents.md`; always none for the Champion, which never becomes Neutral). Only Actors can carry a Behavior.
- Derived: carried Items (from the Items' locations; non-empty only for an Actor with Equip).

**Per type.**
- **Terrain** = Permanent + mana production (amount and kind — coloured, hybrid, generic…); move-cost (base 1) and per-Slice capacity (base 3), both changed by static abilities; drawn-this-cycle flag (D77). Derived: occupants and Structure-slot contents. Definition-only: a Structure the terrain card brings (`resources-terrain.md` Open question 3), once on the Island an ordinary Structure. No activated abilities (D87), no Activation Points or Life. Terrain Type is flavour only — it names the terrain and nothing can query it (D100). Bond surcharges and Mimic are static abilities.
- **Item** = Permanent + activated abilities (usable only while equipped; an equip surcharge is a static ability).
- **Structure** = Actor. Slice = Ground or Root, in the Structure slot. No Attack (D24) — whether a Structure can ever fight is open (Open question 7).
- **Creature** = Actor + max- and current-Attack. Slice = Root, Ground or Sky, capacity 3 per Slice. Keywords (Flying, Subterranean, Haste, Ranged N…) are static abilities; the defaults (Move, Attack, Defend, Equip, Un-equip, Ascend/Descend) are ordinary activated abilities (D10). A Creature losing its Champion controller receives a Behavior assignment (D83). Defend's once-per-turn limit is expected to be a history-track query (`PLAN.md` Track A item 34).
- **Companion** = Creature + mana pool (its shape waits for the colour-cost model, `PLAN.md` Track A item 37). Derived: bonded terrains. Bond is one of its default activated abilities. Definition-only: which Champion(s) may run it. Its parent is a Champion or none, never another Companion.
- **Champion** = Companion's property list, with parent, `source`, cost, Speed, cast condition and Behavior always none. Same stats as a Creature: Attack, Life, Activation Points. Default activated abilities: Draw, Bond, Move (confined to its realm while connected), Collapse Network, Attack, Defend, Equip, Un-equip, plus its signature abilities. Its card definition is the **customised card from the player's loadout**, not the generic printed card (progression changes it between matches). Hand, Library and Discard are zones it owns, not properties.

**Grave/Ruin:** `PLAN.md` Track A item 27. **Still to inventory:** the Card objects of the non-spell types (Creature, Companion, Structure, Item), and Trace (including Duration and physical / non-physical, D98).

## Open questions

1. **Card-level continuous effects** (e.g. a hypothetical "target creature card in your hand gains +1/+1/+1"), tracked at `PLAN.md` §8 item 11 — this doc describes how such a modifier would propagate *if* it existed, not whether/how one gets attached to a Card in the first place.
2. **The per-type property inventory (§5) is incomplete** — the non-spell Card objects and Trace remain (`PLAN.md` Track A item 4). Grave/Ruin's properties are `PLAN.md` Track A item 27.
3. **This doc only covers the six *cross-domain* transitions**, tracked at `PLAN.md` §8 item 5. Untouched: transitions *within* a domain (Library→Hand/draw, Hand→Discard/discard, Pending→Past/resolve, Pending↔Future/delay, mill, etc.) — some are ordinary game progression, others (discard, mill) are card-grantable actions in their own right and may need their own version of the "what does the created/moved object keep" question. An object leaving every zone ceases to exist (§2, D97); how a creature ceases to exist outright, given that destroying it leaves a Grave, is `PLAN.md` Track A item 27; what removal effects are called is item 39.
4. **How do Map, the Champions' terrain decks, and the Champions themselves actually combine to produce the initial board state?** Champion and Terrain plausibly reach the Island through a mechanism that also involves the Map, not a plain Card→Permanent read — genuinely open, not just unstated. Tracked at `PLAN.md` §8 item 12.
5. **Following `source` into a hidden zone.** A link from a visible permanent to a card in an opponent's Discard reaches hidden information; whether card text may follow it, and what it must reveal, is undecided (the opponent's cards are expected to be reachable only at the cost of a reveal).
6. **The creation triple (D83).** Every instruction that creates a permanent binds, at declaration, the new permanent's controller, Behavior, and neutral turn. Defaults: a card source → the casting Champion; a controlled permanent's ability → its controller; a source with no controller → Neutral, with the source's Behavior and neutral turn; card text creating a Neutral permanent must state its Behavior and neutral turn, or the card is invalid. Controller = a Champion → the new permanent's parent is the resolved payer; Neutral → it is created as a parentless root. Still open: a name for this field that doesn't clash with the Trace's own controller, and its place in the per-type inventory (`PLAN.md` Track A item 4).
7. **Can a Structure ever fight?** A Structure has no Attack property (D103), and properties are never added during play, so "this Structure gains Attack 3" can't exist. A fighting Structure would need either a separate type ("structure creature") or Attack on every Structure after all (printed 0 by default). Tracked in `PLAN.md` Track A item 4.
