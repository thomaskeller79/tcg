# Interaction & Speed

*Players can act on the opponent's turn (MTG-style instants). One primitive covers instants, combat tricks, and traps: a **Speed** tag per card/ability, checked live against what's sitting in **Pending**, the Aether's next-to-resolve zone.*

**Decisions:** D6, D16, D35, D38, D45, D46, D68, D70 (`history/decisions.md`)

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

## Casting and activating: pay, then choose, then target (D46, D70)
Playing a card or activating an ability is a strict three-step procedure. No step may look ahead at a later one, and none is revisited once passed:

1. **Pay.** The complete cost is paid, including choosing any variable cost `X` — `X` is fixed here because it determines what's owed, not later. A cost is a **CNF** (a conjunction of disjunctions): each disjunct is one of four kinds, evaluated and paid in this fixed order — **colored mana**, **mana with choices** (including hybrid mana, but also a disjunct that mixes a mana branch with a non-mana alternative, e.g. "3 fire mana or sacrifice a creature"), **neutral mana**, then **additional costs with no mana alternative**. Payment is **semi-automatic**: a disjunct with only one currently-legal branch resolves without a prompt *only when every branch in that disjunct is plain mana* — a disjunct mixing mana with any non-mana branch (sacrifice, discard) always requires explicit confirmation, even when only one branch is legal, since silently consuming a permanent or card is never an acceptable default. **An abort option is available at any point up until the trace actually enters the Aether.**
2. **Choose.** Any modal selection is made, including a "choose from this menu `N` times, repeats allowed" structure, where `N` was already fixed in step 1. Every value resolved in steps 1–2 — cost, `X`, modes — becomes an ordinary, fixed, durable property of the resulting trace from this point on (`object-properties.md` §4).
3. **Target.** Each target the card/ability requires is chosen last, against the game state as it now stands. The same object may be chosen by two separate picks within one repeated-choice resolution — each pick is its own instance of a target requirement, the same way a card that says "target" twice may repeat a target across those two instances, but never within one. **Target is the only step whose output never locks in** — it's freshly re-derived at every future instantiation, in either direction (`object-properties.md` §4).

**A trace does not exist in the Aether — isn't visible, isn't respondable-to, isn't reachable by anything including Remand — until all three steps are complete.** A card/ability is only ever offered as a legal play at all if at least one legal cost-and-target combination exists — no paying into a target-less cast.

**A trace's displayed text is generated by collapsing the card's own template as each step resolves**, not by revealing a fixed hidden block: an unpaid cost alternative disappears once its sibling branch is chosen (e.g. "3 fire mana or sacrifice a creature" becomes just "sacrifice a creature"); the effect menu is replaced by whichever instructions were actually chosen once step 2 completes, and a clause conditioned on a now-permanently-settled fact (e.g. "if a creature was sacrificed to pay for this, also...") collapses to a flat, unconditional clause; each `target X` placeholder becomes an annotated binding (e.g. `target creature (Grunt #145)`) once step 3 resolves. **The annotation is live game data, not decoration** — resolution reads it directly to determine what the trace actually affects.

## Resolution: illegal targets fizzle per-instruction (D33)
When a trace resolves (crosses `Now`), any target it committed to is re-checked for legality — the board may have changed while it sat in Pending (opponent responses, contested terrain, a slot filling up). **If a target is no longer legal, only that instruction fails to happen — the rest of the card still resolves.** Paid cost is never refunded either way.

## Instantiate is iterative when belief and true state disagree (D68)
The choice-of-target/destination step above (the "instantiate" half of D46) is checked at declare time against the **acting player's own belief state** — an ordinary targeting rule, no different from any other legal-target query. That check can still turn out wrong for a destination-choosing action (Move, Summon, Relocate/push) whose true legality depends on a concealed level's actual occupants (controller-uniformity or the 3-slot cap, `overview.md` §2) — the only way belief and truth can disagree here, since an unconcealed level's occupancy is always accurately known.

When instantiating the chosen destination discovers it's illegal against true state, the step **repeats** rather than failing outright: the player picks a different destination from the action's remaining legal-per-belief candidates, or explicitly **cancels**. This continues until a destination succeeds (proceeds into Pending normally) or the player stops (cancels, or every candidate is exhausted) — a cancel option is always available, distinct from exhaustion, so a player is never forced into a worse-than-doing-nothing legal choice just because one exists. **Paid cost — mana and the card alike — is never returned in any outcome**, matching D33/D46's existing no-refund rule; card discard stays at the normal cast-commitment point (D37), not deferred to a successful instantiate (deliberately rejected — see D68, it would make narrowly-targeted spells a repeatable, low-cost "probe this hex" tool).

This is a distinct check from the one two paragraphs up (D33): that one covers a *previously legal* target going illegal later, while sitting in Pending, from a visible board change (an opponent's response, a race for the same slot). This one covers the *initial* instantiate attempt discovering it was never legal to begin with, because of something hidden.

**The redirect loop only ever applies to a *location*-typed choice (a hex, or (hex, level) address) — never to a *permanent*-typed choice (a specific named creature/permanent), regardless of what the card's own text calls either one, and regardless of how many candidates that choice had.** "Gain control of target creature" targets a creature, not a location; redirecting to a *different* creature would change what the spell does, not offer a graceful same-intent pivot, so it never gets one — this holds even when several creatures were initially eligible. **Card text can't be used to tell which is which** — a card like "Move target creature to target location" calls both a "target," but only the location half redirects; the creature stays whatever was first chosen even when the location fails and gets redirected, and if the creature-target were what failed instead, that's a clean hard-fail for that instruction, never a fallback to a different creature. Move/Summon/Relocate's destination is always location-shaped, so nothing changes for them. Actions with only one possible location (Descend, when blind — D67) or with a permanent-typed choice instead of a location (a control-change's named creature, D66) have nothing to redirect to and reduce to a single attempt — legal, or a clean hard-fail with cost sunk.

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
