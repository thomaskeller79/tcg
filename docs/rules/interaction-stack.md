# Interaction & Speed

*Players can act on the opponent's turn (MTG-style instants). Rather than three separate features — instants, combat tricks, traps — one primitive: a **Speed** tag per card/ability, checked live against what's sitting in the Aether's Pending region. Everything reactive is a use of it.*

**Decisions:** D6, D16, D35, D38, D45, D46 (`history/decisions.md`)

---

## Why this exists
Reactive play (MTG-style instants and a stack) is a design goal. Instead of bolting on ad-hoc reaction rules, one mechanism covers all reactive gameplay and keeps it consistent.

## The primitive
> **Speed.** Every card/ability has one of four speeds — **Slow, Quick, Reactive, Instant** (D45) — and legality to play it is a live query against the current contents of the Aether's **Pending** region (D38), not a fixed list of window-opening events.

| Speed | Playable whenever… | What happens after |
|---|---|---|
| **Slow** | it's the controller's main phase, and Pending is empty. (≈ the old sorcery-speed default.) | Enters Pending, waits its turn, resolves and fades normally. |
| **Quick** | only **Physical Traces** are in Pending — the trace left by Move, Attack, Ascend, or Descend (confirmed set, extensible). | Same as Slow. |
| **Reactive** | no **Instant Trace** is in Pending. Kept as a real check even though an Instant is never actually observable sitting in Pending (see Instant, below) — a robustness safeguard for a possible future card that makes an Instant linger. | Same as Slow — a normal Pending entry, respondable by anything whose Speed currently permits it. |
| **Instant** | any time, no condition. | **Resolves fully atomically**: enters Pending and `Now` is advanced past it as part of the same action — zero duration, **no response window for anyone, not even another Instant.** |

**This *is* the front of the Aether (D16, restructured D38).** Pending is the region just ahead of `Now`; as things resolve they cross `Now` and become past **traces** (which then fade, D50 — except Physical Traces, which get zero Past residency at all). So reactive play, the history log, and the old "spell-graveyard" concept are one timeline, viewed per-observer.

**Why this replaces the old priority+stack model (D5/D23) instead of just adding a fourth speed to it:** the old model needed an explicit, maintained list of *which events open a priority window* (spell/ability additions, Combat declaration, …) — an enumeration that had to grow every time a new kind of reactive moment showed up. Speed needs no such list: legality is just "check Pending's current contents," so a Quick-speed card automatically gets its window around *any* Physical action (Move as well as Attack) with no separate rule required, and a future Physical ability (say, a new movement-type keyword) is covered for free.

## Kept tame (the simplicity levers)
The whole point is MTG-like depth *without* MTG-like overwhelm on turn one:
1. **Instant is fully atomic — the single biggest simplification here.** Nothing can ever respond to an Instant, not even another Instant, so the entire top speed tier needs no unbounded-depth stack, no priority-passing negotiation, no "hold priority" concept. This directly avoids what was otherwise flagged as the largest AI/netcode cost in the old model (see Costs, below).
2. **Most default cards are Slow** (no Pending interaction at all). Reactive play is *invisible* to a new player until they pick up a Quick/Reactive/Instant card. Depth is opt-in.
3. **Quick and Reactive still give real depth** against everything below Instant speed — combat tricks, movement responses, traps — without needing Instant's full unboundedness.

## What one mechanism unifies
| Feature | How it's just "Speed + Pending" |
|---|---|
| **Instants / reactions** | A card/ability whose Speed currently permits playing it. |
| **Combat tricks** | Any Quick/Reactive/Instant-speed card, played while an Attack's **Physical Trace** sits in Pending, before it resolves — no bespoke "Combat window" needed (see Combat integration, below). |
| **Traps** (D6) | A pre-committed, **hidden** triggered ability (pillar 6) that enters Pending when its condition fires. |
| **Triggered abilities** | "When X happens, do Y" — enters Pending when X occurs. |

## Triggered vs. activated abilities (D35)
These are the two fundamental ability shapes, and neither is tied to card Type — a Creature, Structure, Champion, or Companion can carry either directly, and an Item can grant either to its carrier via equip (ordinary D26 funding for whatever it grants).
- **Activated:** a deliberate choice — pay a cost upfront to put it in Pending at all. This is what `overview.md` §4's `mana + AP` cost model describes.
- **Triggered:** "when X happens, do Y" (table above) — enters Pending automatically the instant its condition is met, no activation choice involved. A triggered ability isn't necessarily free, though: it can carry its own cost, paid **at resolution** rather than upfront, typically as an optional clause ("whenever a creature enters within distance 2, **you may pay 2 mana**; if you do, draw a card") rather than a mandatory activation cost. Funding for that resolution-time cost follows the ordinary ownership-tree payment walk (D26) like any other cost; only the controller (D28) decides whether to pay.

## Combat integration (recovers the "surprise pump," now for free)
An Attack is a **Physical Trace** (D45): it enters Pending exactly like any other trace, so it just sits there, respondable, until `Now` reaches it — no bespoke "pre-resolution priority window" needs to be declared for Combat specifically, the way the old model required. Attack declared → it's a Physical Trace in Pending → any Quick/Reactive/Instant-speed card can be played in response (a defender's trick, then the attacker's counter-trick, and so on, gated by each response's own Speed) → the Attack resolves. This delivers the *"all creatures +1 attack"* blowout the user wanted — the defender commits, then you reveal — **per-combat**, without needing full board-wide simultaneous combat, and without a Combat-specific rule at all: it's the same mechanism every other Physical action already uses.

## Resolution: illegal targets fizzle per-instruction, not a whole-spell counter (D33); cost is paid before targets are chosen (D46)
When a trace resolves (crosses `Now`), any target it committed to is re-checked for legality — the board may have changed while it sat in Pending (opponent responses, contested terrain, a slot filling up). **If a target is no longer legal, only that instruction fails to happen — the rest of the card still resolves**, unlike MTG's whole-spell counter-on-illegal-target. For a single-target card (e.g. today's Creature/Structure/Item casts, `structures-items.md`) this looks identical in outcome to a counter, but the principle is general: a future multi-target card only loses the parts that went illegal. Paid cost is never refunded either way.

**D46 reorders when that target is chosen relative to cost:** playing a card or activating an ability now pays its complete cost **before** choosing targets/modes — reversing this doc's earlier target-then-pay assumption. Necessary for X-costs and cost-scaled modal effects, where the available choices depend on what was actually paid. A card/ability is only ever offered as a legal play at all if at least one legal cost-and-target combination exists — no paying into a target-less cast.

## Costs (accepted — smaller than before, thanks to atomic Instant)
- **AI** must evaluate responses for Quick/Reactive plays (search over "respond vs. pass"), but **not** for Instant — atomic resolution means there's no response tree to search there at all.
- **Netcode** must handle Quick/Reactive response windows over the wire (whose turn to respond, timeouts), but again not for Instant.
- **State-based checks** (e.g. destroy 0-HP units) run between Pending resolutions — needs a defined checkpoint.
- **Timing/UX**: players need clear, fast "respond or pass" prompts; auto-pass when a player has no legal response keeps it snappy.

## Invariant vs. mutable
- **Invariant:** the Aether's Pending region and the Speed check always exist; resolution order within Pending is LIFO; deterministic. Instant is always fully atomic.
- **Mutable (card-driven):** what Speed a given card/ability has, what triggers exist, extra priority a card grants.

## Open questions
1. **Timeouts:** in real-time online play, how long is a response window before auto-pass?
2. **Trap limits:** how many traps can be pre-set? Are they revealed on trigger only, or can they be scried/detected (pillar 6 interplay)?
3. **Depth cap within Pending:** allow unlimited Quick/Reactive responses-to-responses, or cap depth for simplicity/UX? (Moot for Instant, which never stacks at all.)
