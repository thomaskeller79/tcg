# Ownership Tree & Cost Resolution

*Every permanent on the Island (other than a Champion) has **at most one** parent — the entity that produced, bonded, or currently carries it. Not every permanent has one: unbonded terrain and a loose item have none. One mechanism covers "who funds Terrain / Structure / Item" and "who controls it" together.*

**Note (D58):** Terrain, Structure, Ruins, and Graves each hold their own printed **Activation Points**, the same resource an Actor holds — self-gating "has this printed ability already fired this round" *and* self-funding that ability's Activation Points cost directly, no climbing. The parent chain below still answers who funds a *mana* cost. See `economy.md`, `resources-terrain.md`, `structures-items.md`.

**Decisions:** D26–D30, D47, D54–D55, D57–D58 (`history/decisions.md`)

---

## Payment vs. controller — two different walks, not one

- **Payment (who funds a specific cost):** starting at the entity itself, climb parent-links and stop at the **first** node that itself holds the needed resource — one general rule for every resource type, not a special case per type (D58). Activation Points and Life are held directly by whatever permanent carries them (D58 — Activation Points: every permanent but Item; Life: Champion/Companion/Creature/Structure), so those never climb. Mana is held only by a Champion or a **Companion** — the one non-Champion node capable of holding a resource itself. A mana cost climbs past Terrain/Structure/Item/Creature (none of which hold mana) and stops at whichever comes first: a Companion, or the Champion. This walk can legitimately stop at a Companion — that's the entire point of its private pool. A card may explicitly name a different payer than this default walk would reach (e.g. "this creature's **controller** pays 2 life," skipping past the creature's own Life to reach its Champion) — a deliberate override, not something the general rule does on its own.
- **Controller (whose side this is on):** climb parent-links **all the way to the top**, regardless of what's held along the way. The root is either a Champion (that Champion controls it) or an explicitly **Neutral** node — a Companion a card has made Neutral (D55), or a permanent with no parent at all (never bonded/funded, or created directly as Neutral). **A Companion is a root only when Neutral** (D55) — a Companion still controlled by a Champion is never a root; the climb continues past it to that Champion. This always resolves to exactly one of three values — Player A, Player B, or Neutral (D54) — see `neutral-permanents.md` for what a Neutral permanent does next.

A permanent's payer and controller can differ: anything rooted through a controlled Companion is *paid for* by that Companion but *controlled* by the Champion who cast it.

## The parent chain

| Entity | Parent |
|---|---|
| **Companion** | the Champion that cast it (only Champions cast spells — see below), **unless a card has made it Neutral (D55)** — a Neutral Companion has no parent and is its own root for ownership purposes. This is separate from its terrain-network root status (`companions.md`), which it holds either way. |
| **Creature** | resolved **directly, once, at creation** to whichever Champion or Companion recursively pays for it (even if the cost is 0) — **never another creature** — unless a card explicitly reassigns it afterward ("Gain control of target creature," D55). A creature is always a leaf for creation/funding purposes, no matter how many hops of creature-triggered creation produced it. |
| **Terrain** | the Champion or Companion currently **bonded** to it, if any — see below for how this bond record relates to the terrain's controller. No parent while unbonded. |
| **Structure** | the terrain cell it occupies — unaffected by *who built it* (even if a creature's ability built it, the structure's own ongoing parent is always its terrain, never the builder). |
| **Item, while equipped** | the actor (Champion, Companion, or Creature) that equipped it — a real parent-link, so the item is orphaned if that actor dies. |
| **Item, while unequipped (loose)** | no parent at all, and no usable ability either (see Item below). |

A creature can still be a parent of an **Item** it carries — carrying is a distinct relationship from creation/funding, established via the Equip ability (D25), not exempted by the "creature is always a leaf" rule above.

## Terrain's controller vs. its bond record (D54)

Terrain's **controller** is exactly the same three-valued thing as any other permanent's — Player A, Player B, or Neutral — derived by the same climb: does a live, enemy-free path currently connect this terrain back to its bonder? If yes, the climb reaches the bonder's Champion. If not, the climb fails and the terrain is **Neutral** — whether that's because the path is merely blocked right now, or because it was never bonded to begin with.

What still needs distinguishing is the terrain's **bond record** (its parent link) — a separate, persistent fact from the derived controller value, the same general parent-vs-controller split as above:
- **A bond record pointing to a live bonder, with the path currently clear:** controller = that bonder's Champion. Produces mana.
- **A bond record pointing to a live bonder, with the path currently blocked:** controller = Neutral, but the bond record is untouched — the instant the path clears, the controller-walk succeeds again automatically, with no new Bond action needed. This is a live, per-query result, never a stored flag.
- **No bond record at all:** controller = Neutral, and stays that way until someone performs a fresh Bond. Reachable via: the bonding Companion dying (reverts to unbonded, not inherited by the Champion); the Champion's own voluntary choice to drop one specific bond (a granular sibling to the all-at-once Collapse Network, D9); an additional cost printed on a card ("unbond from target land you control"); or a hostile targeted effect ("target Champion or Companion unbonds from target land").

Either kind of Neutral terrain strips a Structure on that cell of its payer and controller alike — opens area-control fights over contested terrain (block it briefly, or sever the bond outright; either way the Structure is cut loose) — and produces no mana while Neutral, same as any uncontrolled Object.

## Removing an inner node

Removing an inner node breaks the chain for all of its successors:

1. **Companion dies.** Its bonded terrain reverts to unbonded. Any creature it's the parent of loses both its payer and its controller — the climb to find a controller no longer reaches a live Champion, so there isn't one.
2. **Creature dies.** Any Item it carried becomes ownerless (parent removed, reverts to loose). There is no creature-parents-creature case, so nothing else cascades from a creature's death.
3. **Terrain's controller becomes Neutral** (its path is blocked, or it's unbonded outright). Any Structure on it loses both payer and controller — the basis for area-control play over contested terrain (above).

Champion death isn't an inner-node case — it ends the match outright (D9). Structure and Item never have children under the present design (nothing attaches to a Structure; nothing attaches to an Item).

## Control changes and occupancy (D66)

A hex-level's occupants must all share one controller (`overview.md` §2). Any effect that reassigns a permanent's controller — "gain control of target creature," "target creature becomes Neutral," or any other control-change effect, targeted or not — rechecks this at resolution: if completing it would leave a level with occupants under different controllers, the effect **fails to complete instead (fizzles)**, the same way an illegal target fizzles rather than countering a whole spell (D33). Paid cost is never refunded. A card may still choose to restrict its own targeting as a courtesy (e.g. "target creature that is alone on its hex-level") — this is never required, and can only ever be a hint, not a guarantee, since a hidden creature sharing that level may be invisible to the caster.

## Paying a cost

A cost is paid by the **nearest node — starting at the entity itself — that directly holds that resource** (the payment walk above, not the controller walk):

- Activation Points and Life are held directly by every permanent that carries them (D58 — Activation Points: every permanent but Item; Life: Champion/Companion/Creature/Structure) — an ability costs its own object's Activation Points or Life, no climbing. Life is not a third resource alongside mana/Activation Points (`economy.md`) — it's the same counter combat damage reduces.
- Mana, discarding a card, sacrificing a permanent, unbonding a terrain (and any other pooled/hand/board resource) climb the parent chain, live, stopping at the first Champion or Companion found.
- A card may explicitly name the **controller** as payer instead — e.g. "this creature's controller must pay 2 life" skips the creature's own Life *and* skips past any Companion in its chain, reaching the Champion specifically.

## Only Champions cast spells

Casting (playing a card from Hand) is exclusively a Champion action — a Companion has no Hand (`companions.md`). This is why a Companion's parent is always the Champion, and why a discard cost printed on a Companion's ability climbs past it to the Champion, who actually has cards to discard.

## Companion funding scope

A Companion funds everything in its own subtree (creatures it produces, terrain/structures it bonds, items equipped to those creatures) from its own private pool. It cannot fund a **different** controller's own abilities or spells directly. "Private" means "scoped to this Companion's own subtree," not "usable only by the Companion card itself."

## Item funding

Every eligible actor gets a generic ability, **`2AP: Equip target Item sharing this location`** (tuning baseline), plus its inverse, **`0AP: Un-equip`** (D30 — free and uncapped, drops the Item back to loose on the actor's current hex). An unequipped Item has no usable ability at all, so there is never a "who funds a loose item" question. Both are removable/re-priceable per creature (pillar 5); stealing an item is a costed action via the same Equip ability; exclusivity (one holder at a time) falls out automatically since an equipped Item has exactly one parent.

## Invariant vs. mutable
- **Invariant:** every non-Champion entity has at most one parent; a creature never parents another creature or structure (unless a card reassigns it, D55); a Companion is a root only when Neutral (D55); Activation Points and Life are always self-paid (D58); mana never crosses two different payers' pools; removing an inner node breaks payment *and* controller for its successors; every permanent's controller resolves to exactly Player A, Player B, or Neutral (D54); a control-change effect that would leave a hex-level's occupants under different controllers fails to complete instead (D66).
- **Mutable (card-driven):** which node pays a given cost (a card may name the controller explicitly), whether a creature has the Equip ability at all and at what cost, per-item equip surcharges, cards that add new ways to interrupt/unbond terrain, cards that reassign a Creature/Companion's parent or make a Companion Neutral.

## What an uncontrolled (Neutral) permanent does

Terrain, Structure, and Item stay simply inert while Neutral — nobody may activate their ability. A **Creature or Companion** (or a Structure/Terrain with a self-payable ability) still holds and refills its own Activation Points regardless of control, so it follows a **Behavior** instead of sitting inert — a complete, deterministic algorithm standing in for a player's decisions. Full detail, including which permanents are eligible and how Behaviors are scoped and funded: `neutral-permanents.md`.
