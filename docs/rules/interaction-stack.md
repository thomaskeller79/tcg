# Interaction & Speed

*Players can act on the opponent's turn (MTG-style instants). One primitive covers instants, combat tricks, and traps: a **Speed** tag per card/ability, checked live against what's sitting in **Pending**, the Aether's next-to-resolve zone.*

**Decisions:** D6, D16, D35, D38, D45, D46 (`history/decisions.md`)

---

## The primitive
> **Speed.** Every card/ability has one of four speeds — **Slow, Quick, Reactive, Instant** (D45) — and legality to play it is a live query against the current contents of **Pending** (the Aether's next-to-resolve zone, D38).

| Speed | Playable whenever… | What happens after |
|---|---|---|
| **Slow** | it's the controller's main phase, and Pending is empty. | Enters Pending, waits its turn, resolves and fades normally. |
| **Quick** | only **Physical Traces** are in Pending — the trace left by Move, Attack, Ascend, or Descend. | Same as Slow. |
| **Reactive** | no **Instant Trace** is in Pending. | Same as Slow — a normal Pending entry, respondable by anything whose Speed currently permits it. |
| **Instant** | any time, no condition. | **Resolves fully atomically**: enters Pending and `Now` is advanced past it as part of the same action — zero duration, **no response window for anyone, not even another Instant.** |

Pending is the zone just ahead of `Now` — `Now` itself is **not a zone**, just the moving point dividing Pending from Past. **`Now` advances past whatever's next in Pending when both players pass priority in succession** (neither has anything they want to play). The sole exception is an Instant, which advances `Now` past itself immediately and atomically as part of being played, never waiting for both players to pass. As things resolve they cross `Now` and become past **traces**, which then fade (except Physical Traces, which get zero Past residency at all).

## Kept tame
1. **Instant is fully atomic.** Nothing can ever respond to an Instant, not even another Instant.
2. **Most default cards are Slow** (no Pending interaction at all). Reactive play is invisible until a player picks up a Quick/Reactive/Instant card.
3. **Quick and Reactive give real depth** against everything below Instant speed — combat tricks, movement responses, traps.

## What Speed + Pending covers
| Feature | Mechanism |
|---|---|
| **Instants / reactions** | A card/ability whose Speed currently permits playing it. |
| **Combat tricks** | Any Quick/Reactive/Instant-speed card, played while an Attack's **Physical Trace** sits in Pending, before it resolves. |
| **Traps** (D6) | A pre-committed, **hidden** triggered ability (pillar 6) that enters Pending when its condition fires. |
| **Triggered abilities** | "When X happens, do Y" — enters Pending when X occurs. |

## Triggered vs. activated abilities (D35)
These are the two fundamental ability shapes, and neither is tied to card Type — a Creature, Structure, Champion, or Companion can carry either directly, and an Item can grant either to its carrier via equip (ordinary D26 funding for whatever it grants).
- **Activated:** a deliberate choice — pay a cost upfront to put it in Pending at all. This is what `overview.md` §4's `mana + AP` cost model describes.
- **Triggered:** "when X happens, do Y" (table above) — enters Pending automatically the instant its condition is met, no activation choice involved. A triggered ability can still carry its own cost, paid **at resolution** rather than upfront, typically as an optional clause ("whenever a creature enters within distance 2, **you may pay 2 mana**; if you do, draw a card"). Funding for that resolution-time cost follows the ordinary ownership-tree payment walk (D26) like any other cost; only the controller (D28) decides whether to pay.

## Combat integration
An Attack is a **Physical Trace** (D45): it enters Pending exactly like any other trace, so it just sits there, respondable, until `Now` reaches it. Attack declared → it's a Physical Trace in Pending → any Quick/Reactive/Instant-speed card can be played in response (a defender's trick, then the attacker's counter-trick, and so on, gated by each response's own Speed) → the Attack resolves.

## Resolution: illegal targets fizzle per-instruction (D33); cost is paid before targets are chosen (D46)
When a trace resolves (crosses `Now`), any target it committed to is re-checked for legality — the board may have changed while it sat in Pending (opponent responses, contested terrain, a slot filling up). **If a target is no longer legal, only that instruction fails to happen — the rest of the card still resolves.** Paid cost is never refunded either way.

Playing a card or activating an ability pays its complete cost **before** choosing targets/modes. A card/ability is only ever offered as a legal play at all if at least one legal cost-and-target combination exists — no paying into a target-less cast.

## Costs
- **AI** must evaluate responses for Quick/Reactive plays (search over "respond vs. pass"); not for Instant, which never opens a response tree.
- **Netcode** must handle Quick/Reactive response windows over the wire (whose turn to respond, timeouts); not for Instant.
- **State-based checks** (e.g. destroy 0-HP units) run between Pending resolutions — needs a defined checkpoint.
- **Timing/UX**: players need clear, fast "respond or pass" prompts; auto-pass when a player has no legal response keeps it snappy.

## Invariant vs. mutable
- **Invariant:** the Pending zone and the Speed check always exist; resolution order within Pending is LIFO; deterministic. Instant is always fully atomic.
- **Mutable (card-driven):** what Speed a given card/ability has, what triggers exist, extra priority a card grants.

## Open questions
1. **Timeouts:** in real-time online play, how long is a response window before auto-pass?
2. **Trap limits:** how many traps can be pre-set? Are they revealed on trigger only, or can they be scried/detected (pillar 6 interplay)?
3. **Depth cap within Pending:** allow unlimited Quick/Reactive responses-to-responses, or cap depth for simplicity/UX? (Moot for Instant, which never stacks at all.)
