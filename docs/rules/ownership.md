# Ownership Tree & Cost Resolution

*Every permanent in play (other than a Champion) has **at most one** parent — the entity that produced, bonded, or currently carries it. Not every permanent has one: unbonded terrain and a loose item have none. One mechanism covers "who funds Terrain / Structure / Item" and "who controls it" together.*

**Decisions:** D26–D30 (`history/decisions.md`)

---

## Payment vs. controller — two different walks, not one

- **Payment (who funds a specific cost):** starting at the entity itself, climb parent-links and stop at the **first** node that itself holds the needed resource. AP and Life are held by the entity itself (D10), so those never climb. Mana is held only by a Champion or a **Companion** — the one non-Champion node capable of holding a resource itself. A mana cost climbs past Terrain/Structure/Item/Creature (none of which hold mana) and stops at whichever comes first: a Companion, or the Champion. This walk can legitimately stop at a Companion — that's the entire point of its private pool.
- **Controller (whose side this is on):** climb parent-links **all the way to the top**, regardless of what's held along the way. **A Companion is never itself a root** — it always has a parent (the Champion that cast it) — so if this climb resolves at all, it resolves to a Champion. If the root is a Champion, that Champion is the permanent's controller.

A permanent's payer and controller can differ: anything rooted through a Companion is *paid for* by that Companion but *controlled* by the Champion who cast the Companion. A card that costs "this creature's **controller** pays 2 life" always hits the Champion's Life, even for a Companion-summoned creature.

## The parent chain

| Entity | Parent |
|---|---|
| **Companion** | the Champion that cast it (only Champions cast spells — see below). A Companion is always an inner node, never a root — it always has a live Champion parent for as long as it's in play. |
| **Creature** | resolved **directly, once, at creation** to whichever Champion or Companion recursively pays for it (even if the cost is 0) — **never another creature**. A creature is always a leaf for creation/funding purposes, no matter how many hops of creature-triggered creation produced it. |
| **Terrain** | the Champion or Companion currently **bonded** to it — three states, not two (see below). No parent while unbonded. |
| **Structure** | the terrain cell it occupies — unaffected by *who built it* (even if a creature's ability built it, the structure's own ongoing parent is always its terrain, never the builder). |
| **Item, while equipped** | the actor (Champion, Companion, or Creature) that equipped it — a real parent-link, so the item is orphaned if that actor dies. |
| **Item, while unequipped (loose)** | no parent at all, and no usable ability either (see Item below). |

A creature can still be a parent of an **Item** it carries — carrying is a distinct relationship from creation/funding, established via the Equip ability (D25), not exempted by the "creature is always a leaf" rule above.

## Terrain's three states

- **Bonded and active:** a live, enemy-free path connects it back to its bonding Champion or Companion, producing mana.
- **Bonded and interrupted:** an enemy occupies a node on its path back to its bonder. The bond record itself is untouched, but for every downstream purpose (payment, and Structure control) it behaves exactly like having no parent. Reverts to active automatically the instant the path clears — no new Bond action needed. Live, per-query status, not a stored flag.
- **Unbonded:** the bond itself is gone. Requires a fresh Bond action to restore. Reachable via: the bonding Companion dying (reverts to unbonded, not inherited by the Champion); the Champion's own voluntary choice to drop one specific bond (a granular sibling to the all-at-once Collapse Network, D9); an additional cost printed on a card ("unbond from target land you control"); or a hostile targeted effect ("target Champion or Companion unbonds from target land").

Both interrupted and unbonded strip a Structure on that cell of its payer and its controller alike — opens area-control fights over contested terrain (interrupt it briefly, or sever it outright, either way the structure is cut loose).

## Removing an inner node

Removing (or interrupting) an inner node breaks the chain for all of its successors:

1. **Companion dies.** Its bonded terrain reverts to unbonded. Any creature it's the parent of loses both its payer and its controller — the climb to find a controller no longer reaches a live Champion, so there isn't one.
2. **Creature dies.** Any Item it carried becomes ownerless (parent removed, reverts to loose). There is no creature-parents-creature case, so nothing else cascades from a creature's death.
3. **Terrain becomes interrupted or unbonded.** Any Structure on it loses both payer and controller — the basis for area-control play over contested terrain (above).

Champion death isn't an inner-node case — it ends the match outright (D9). Structure and Item never have children under the present design (nothing attaches to a Structure; nothing attaches to an Item).

## Paying a cost

A cost is paid by the **nearest node — starting at the entity itself — that directly holds that resource** (the payment walk above, not the controller walk):

- AP and Life are held directly by every actor/creature (D10) — an ability costs its own object's AP or Life, no climbing. Life is not a third resource alongside mana/AP (`economy.md`) — it's the same counter combat damage reduces.
- Mana, discarding a card, sacrificing a permanent, unbonding a terrain (and any other pooled/hand/board resource) climb the parent chain, live, stopping at the first Champion or Companion found.
- A card may explicitly name the **controller** as payer instead — e.g. "this creature's controller must pay 2 life" skips the creature's own Life *and* skips past any Companion in its chain, reaching the Champion specifically.

## Only Champions cast spells

Casting (playing a card from Hand) is exclusively a Champion action — a Companion has no Hand (`companions.md`). This is why a Companion's parent is always the Champion, and why a discard cost printed on a Companion's ability climbs past it to the Champion, who actually has cards to discard.

## Companion funding scope

A Companion funds everything in its own subtree (creatures it produces, terrain/structures it bonds, items equipped to those creatures) from its own private pool. It cannot fund a **different** controller's own abilities or spells directly. "Private" means "scoped to this Companion's own subtree," not "usable only by the Companion card itself."

## Item funding

Every eligible actor gets a generic ability, **`2AP: Equip target Item sharing this location`** (tuning baseline), plus its inverse, **`0AP: Un-equip`** (D30 — free and uncapped, drops the Item back to loose on the actor's current hex). An unequipped Item has no usable ability at all, so there is never a "who funds a loose item" question. Both are removable/re-priceable per creature (pillar 5); stealing an item is a costed action via the same Equip ability; exclusivity (one holder at a time) falls out automatically since an equipped Item has exactly one parent.

## Invariant vs. mutable
- **Invariant:** every non-Champion entity has at most one parent; a creature never parents another creature or structure; a Companion is never a root; AP and Life are always self-paid; mana never crosses two different payers' pools; removing an inner node breaks payment *and* controller for its successors.
- **Mutable (card-driven):** which node pays a given cost (a card may name the controller explicitly), whether a creature has the Equip ability at all and at what cost, per-item equip surcharges, cards that add new ways to interrupt/unbond terrain.

## Open question — what does an uncontrolled creature do?

Losing a controller is well-defined for Terrain and Structure: they're inert, so "uncontrolled" just means nobody may activate them. A **creature is different** — it still has and refills its own AP (D10, always self-paid, never climbed), so an uncontrolled creature isn't obviously inert the same way. Not designed yet:
- Can an uncontrolled creature still act at all (move/attack/defend) if nobody has authority to direct it, given it still holds spendable AP?
- If it can act, who or what decides its actions?

Connects to a larger, deliberately-parked design space: mission/PvE content with NPC-like creatures that follow an AI-driven routine rather than belonging to either player, with an uncontrolled creature as one possible source of such an NPC. See `PLAN.md` §9.
