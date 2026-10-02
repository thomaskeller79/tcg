# Interaction & Speed

*Players can act on the opponent's turn (MTG-style instants). One primitive covers instants, combat tricks, and traps: a **Speed** tag per card/ability, checked live against what's sitting in **Pending**, the Aether's next-to-resolve zone.*

**Decisions:** D6, D16, D35, D38, D45, D46, D68, D70, D75, D83, D85, D91, D92, D94, D95, D100 (`history/decisions.md`)

---

## The primitive
> **Speed.** Every card/ability has one of four speeds — **Slow, Quick, Reactive, Instant** (D45) — and legality to play it is a live query against the current contents of **Pending** (the Aether's next-to-resolve zone, D38).

| Speed | Playable whenever… | What happens after |
|---|---|---|
| **Slow** | it's the controller's main phase, and Pending is empty. | Enters Pending, waits its turn, resolves and fades normally. |
| **Quick** | only **Physical Traces** are in Pending — the trace left by Move, Attack, Ascend, or Descend. | Same as Slow. |
| **Reactive** | no **Instant Trace** is in Pending. | Same as Slow — a normal Pending entry, respondable by anything whose Speed currently permits it. |
| **Instant** | no **Instant Trace** is in Pending. | **Blocks all active play until it has resolved**: while it sits in Pending, nothing can be actively put into the Aether (no cast, no activation — **no response window for anyone, not even another Instant**) and `Now` advances automatically. Triggers are still added and resolve normally. |

**Move/Attack/Ascend/Descend are Slow (D75), not Quick** — Quick describes what *other* cards can play while a Physical Trace already sits in Pending (combat tricks, below), not the physical action's own speed. Being Slow means a physical action can't be declared while Pending is non-empty, so **at most one Physical Trace ever sits in Pending at a time** — an actual enforced consequence of the Speed system, not merely how things happen to play out.

Pending is the zone just ahead of `Now` — `Now` itself is **not a zone**, just the moving point dividing Pending from Past. **`Now` advances past whatever's next in Pending when both Champions pass priority in succession** (neither has anything they want to play). The sole exception is an Instant: while one is in Pending, `Now` advances automatically, never waiting for both Champions to pass, until the Instant has resolved. As things resolve they cross `Now` and become past **traces**, which then fade (except Physical Traces, which get zero Past residency at all).

## Kept tame
1. **Nothing can respond to an Instant**, not even another Instant.
2. **Most default cards are Slow** (no Pending interaction at all). Reactive play is invisible until a player picks up a Quick/Reactive/Instant card.
3. **Quick and Reactive give real depth** against everything below Instant speed — combat tricks, movement responses, traps.

## What Speed + Pending covers
| Feature | Mechanism |
|---|---|
| **Instants / reactions** | A card/ability whose Speed currently permits playing it. |
| **Combat tricks** | Any Quick/Reactive/Instant-speed card, played while an Attack's **Physical Trace** sits in Pending, before it resolves. |
| **Traps** (D6) | A pre-committed, **hidden** triggered ability (pillar 6) that enters Pending when its condition fires. |
| **Triggered abilities** | "When X happens, do Y" — enters Pending when X occurs; if X occurs while a trace resolves, once that trace has finished (§Resolution). |

## Triggered vs. activated abilities (D35)
These are the two fundamental ability shapes, and neither is tied to card Type — a Creature, Structure, Champion, or Companion can carry either directly, and an Item can grant either to its carrier via equip (ordinary D26 funding for whatever it grants).
- **Activated:** a deliberate choice — pay a cost upfront to put it in Pending at all. This is what `overview.md` §4's `mana + AP` cost model describes.
- **Triggered:** "when X happens, do Y" (table above) — enters Pending automatically the instant its condition is met, no activation choice involved. A triggered ability can still carry its own cost, paid **at resolution** rather than upfront, typically as an optional clause ("whenever a creature enters within distance 2, **you may pay 2 mana**; if you do, draw a card"). Funding for that resolution-time cost follows the ordinary Ancestry payment walk (D26) like any other cost; only the controller (D28) decides whether to pay.

## Combat integration
An Attack is a **Physical Trace** (D45): it enters Pending exactly like any other trace, so it just sits there, respondable, until `Now` reaches it. Attack declared → it's a Physical Trace in Pending → any Quick/Reactive/Instant-speed card can be played in response (a defender's trick, then the attacker's counter-trick, and so on, gated by each response's own Speed) → the Attack resolves.

## Casting and activating: legality, pay, choose, target (D46, D70, D91)
Playing a card or activating an ability is a strict procedure. No step may look ahead at a later one, and none is revisited once passed:

0. **Legality.** The card/ability is offered as a legal play only if its Speed permits it against Pending's current contents, its **cast condition** (if printed, e.g. "cast only if you control a Rebel") holds, and at least one legal cost-and-target combination exists — no paying into a target-less cast. The cast condition is checked here only: paying a cost that makes it false (sacrificing the only Rebel) is legal.
1. **Pay.** The complete cost is paid, including choosing any variable cost `X` — `X` is fixed here because it determines what's owed, not later. A cost is a **CNF** (a conjunction of disjunctions): each disjunct is one of four kinds, evaluated and paid in this fixed order — **colored mana**, **mana with choices** (including hybrid mana, but also a disjunct that mixes a mana branch with a non-mana alternative, e.g. "3 fire mana or sacrifice a creature"), **neutral mana**, then **additional costs with no mana alternative**. A disjunct may have an **empty branch (ε)**, which makes it optional. Branches are **labelled** (`c2 = (b1: 1 ∨ b2: 2 ∨ b3: 3)`), and an instruction's condition may test which branch was paid (`c2 = b1`) — by label, never by amount, since two branches can cost the same. This covers per-mode costs: kicker is an optional conjunct plus a condition; a "+{1}: A / +{2}: B, choose one or more" card is one disjunct with a branch per mode combination. Payment is **semi-automatic**: a disjunct with only one currently-legal branch resolves without a prompt *only when every branch in that disjunct is plain mana* — a disjunct mixing mana with any non-mana branch (sacrifice, discard) always requires explicit confirmation, even when only one branch is legal, since silently consuming a permanent or card is never an acceptable default. The card itself is part of the cost (D37, D68): it leaves the Hand as payment starts but arrives in Discard only when payment ends, so it can never pay for itself (e.g. an "exile a card from your discard" cost). **An abort option is available at any point up until the trace actually enters the Aether** (see below).
2. **Choose.** Any modal selection is made, including a "choose from this menu `N` times, repeats allowed" structure, where `N` was already fixed in step 1. A choice may name its **chooser**, who need not be the one casting. Every value resolved in steps 1–2 — cost, `X`, modes — becomes an ordinary, fixed, durable property of the resulting trace from this point on (`object-properties.md` §4).
3. **Target.** Each target the card/ability requires is chosen last, against the game state as it now stands. **A target is required only if an instruction in a chosen option references it** — known by now, since every choice is already made. A target declared at card level is one shared instance that any instruction may reference ("that creature," "it" are references to it, not new targets); a target declared inside a modal option gets a fresh instance for every pick of that option. The same object may be chosen by two separate picks within one repeated-choice resolution — each pick is its own instance of a target requirement, the same way a card that says "target" twice may repeat a target across those two instances, but never within one. A target has a count `min..max` ("up to one target creature" allows none); an instruction left with no bound target does nothing. **Target is the only step whose output never locks in** — it's freshly re-derived at every future instantiation, in either direction (`object-properties.md` §4).
4. **Enter Pending.** The trace now exists in the Aether.

**A trace does not exist in the Aether — isn't visible, isn't respondable-to, isn't reachable by anything including Remand — until step 4.**

**Casting is a transaction (D94).** Nothing done in steps 0–3 — mana spent, a creature sacrificed, a card discarded, the card itself leaving the Hand — is visible to the opponent before step 4, and triggers it fires (e.g. "whenever a creature is sacrificed") are collected and enter Pending only together with the trace, on top of it. **Abort rolls the whole transaction back**: every paid cost is returned and the collected triggers are dropped, leaving the state exactly as it was before step 0. The one exception is the redirect loop below: once a target attempt has failed against true state, the cost is committed and no longer returned.

**A trace's displayed text is generated by collapsing the card's own template as each step resolves**, not by revealing a fixed hidden block: an unpaid cost alternative disappears once its sibling branch is chosen (e.g. "3 fire mana or sacrifice a creature" becomes just "sacrifice a creature"); the effect menu is replaced by whichever instructions were actually chosen once step 2 completes, and a clause conditioned on a now-permanently-settled fact (e.g. "if a creature was sacrificed to pay for this, also...") collapses to a flat, unconditional clause; each `target X` placeholder becomes an annotated binding (e.g. `target creature (Grunt #145)`) once step 3 resolves. **The annotation is live game data, not decoration** — resolution reads it directly to determine what the trace actually affects.

## Binding: every reference is fixed at declaration (D83, D91)
Every reference in the trace's text is bound before the trace enters the Aether, **as early as possible**: each binds as a side effect of whichever step fixes what it depends on. Source-relative references ("you," the payer, an inherited Behavior and neutral turn) bind right after step 0 — before Pay, which can change the Ancestry (a "sacrifice this creature" cost removes the source). Target-relative references ("target creature's controller," a chooser named that way) bind right after step 3.

- **A target** is replaced by the bound object **together with every qualifier that belongs only to that target** — "target creature you control," "target creature within distance 2 of this," "target creature at a hex within distance 2" all become just the chosen creature. A qualifier belongs to its own target unless it names a *different target*; "you" and "this" are references, not targets.
- **A clause relating two targets stays in the trace text.** "Deal 3 damage to target creature at target terrain" becomes "Deal 3 damage to Imp 16 at terrain (4,7)."
- **"You"/"your"** binds by the walk it already uses (`ancestry.md`): the Controller walk for "creatures you control," the Payment walk for "your mana pool" or "you draw." "Add 1 mana to your mana pool" on a Companion's creature becomes "Companion X adds 1 mana to its mana pool."
- **A choice made at resolution** ("you may," "a creature of your choice on target terrain") binds its chooser now; the choice itself is still made at resolution.
- **An instruction that makes a permanent Neutral** (creating it Neutral, or converting it) binds its Behavior and neutral turn now (`neutral-permanents.md` §Becoming Neutral).

Remand deletes bindings together with target annotations, since none of them has a Card-tier slot (`object-properties.md` §3); the next declaration binds them fresh. Card data must therefore represent which qualifiers belong to which target, and which clauses relate two targets.

## Resolution: the remaining text is checked, per instruction (D33, D83, D85, D92)
When a trace resolves (crosses `Now`), its instructions run **in card order**, and each is checked **immediately before it runs** against what the trace still states and the current state — the board may have changed while the trace sat in Pending, and earlier instructions of the same trace may have changed it too. A choice made at resolution is made when its instruction is reached. An instruction **fails to happen (fizzles)** if a relation still in the text is false (the creature is no longer at the bound hex), if carrying it out would violate an invariant (a Slice filling up), or if a bound object fails any of the conditions implicitly on every trace:

1. **Identity** — the same instance still exists. A Flickered or Bounced object is gone, even if it has returned.
2. **Visibility** — it is visible in the live view of the Champion bound to the trace, or, for a trace from a Neutral source, of that source permanent (`asymmetric-information.md`).
3. **Protection** — it doesn't have "can't be affected by [this kind of effect]." "Can't be targeted" is not checked here: targeting is a step of casting, so gaining it in response doesn't affect a trace already in Pending.
4. **Verb applicability** — it is still something the instruction's verb can act on.

Qualifiers consumed by binding are never re-checked, and there is no implicit location condition: a creature-targeting effect still hits a target that moved or changed control in response, unless it moved somewhere its caster can't see; an effect acting on a terrain — or relating its creature to a target terrain — can be dodged by moving. **Only the failing instruction fizzles — the rest of the card still resolves.** Paid cost is never refunded either way.

**Death is immediate.** A permanent whose current Life is 0 or less dies immediately after the instruction that caused it; "destroy" and reaching 0 Life behave identically, and the next instruction already sees the result. Damage dealt by one instruction is simultaneous — "deal 1 damage to each creature" and a Combat's damage (D13) apply fully before anything dies. Text order therefore matters: "deal 3 damage to target creature; it gets +3 max-Life" kills a 3-Life creature, and the second instruction fizzles.

**An instruction can bind an output** — a named result, such as the objects it killed — that later instructions in the same option can reference: `killed := damage(T1, 3)`, then `[if killed ≠ ∅] draw(1)`. This is what "if it dies this way" means, as opposed to "if T1 is dead" (which something else may have caused). An output is unbound if its instruction didn't run, empty if it ran or fizzled without affecting anything; an instruction reading an unbound output doesn't run either. Scope and states: `effect-form.md`.

**Every state change is an instruction**, not only a trace's text: paying a cost and a phase-boundary event (an "until end of turn" effect ending) are instructions too, so the death rule above applies to them unchanged. An instruction does not require a trace. A trace is one way instructions are executed — queued in Pending, respondable; the casting procedure and the turn structure execute their instructions directly, never through Pending, so paying a cost or an effect expiring can't be responded to.

**Triggers fired during resolution are collected.** When the last instruction has run, the trace crosses into Past (its final step, so a trigger that looks at Past sees it), and then the collected triggers enter Pending so that **the triggers of the earliest instruction end up on top** and resolve first. Triggers from the same instruction enter Pending in this order, each later rule breaking ties in the earlier ones:

1. **APNAP over the four seats** (`Champion A, Neutral A, Champion B, Neutral B`, starting from the active seat, `neutral-permanents.md`) — the active seat's triggers enter first and so resolve last;
2. **source ID**, oldest first (`neutral-permanents.md` §Permanent identity);
3. **printed ability order** on the source;
4. **the order of events inside the instruction** (e.g. creation order).

Fully identical triggers are interchangeable and need no order. The order is always automatic, never chosen by a Champion, and is visible in Pending before anyone has to respond.

## Instantiate is iterative when belief and true state disagree (D68)
The choice-of-target/destination step above (the "instantiate" half of D46) is checked at declare time against the **acting Champion's own belief state** — an ordinary targeting rule, no different from any other legal-target query. That check can still turn out wrong for a destination-choosing action (Move, Summon, Relocate/push) whose true legality depends on a concealed Slice's actual occupants (controller-uniformity or the 3-slot cap, `overview.md` §2) — the only way belief and truth can disagree here, since an unconcealed Slice's occupancy is always accurately known.

When instantiating the chosen destination discovers it's illegal against true state, the step **repeats** rather than failing outright: the player picks a different destination from the action's remaining legal-per-belief candidates, or explicitly **cancels**. This continues until a destination succeeds (proceeds into Pending normally) or the player stops (cancels, or every candidate is exhausted) — a cancel option is always available, distinct from exhaustion, so a player is never forced into a worse-than-doing-nothing legal choice just because one exists. **Once an attempt has failed, paid cost — mana and the card alike — is never returned in any outcome**, including an abort (D94): the failure revealed information, so the transaction is committed; card discard stays part of Pay (D37, D91), not deferred to a successful instantiate (deliberately rejected — see D68, it would make narrowly-targeted spells a repeatable, low-cost "probe this hex" tool).

This is a distinct check from the one two paragraphs up (D33): that one covers a *previously legal* target going illegal later, while sitting in Pending, from a visible board change (an opponent's response, a race for the same slot). This one covers the *initial* instantiate attempt discovering it was never legal to begin with, because of something hidden.

**The redirect loop only ever applies to a *location*-typed choice (a hex, or (hex, Slice) address) — never to a *permanent*-typed choice (a specific named creature/permanent), regardless of what the card's own text calls either one, and regardless of how many candidates that choice had.** "Gain control of target creature" targets a creature, not a location; redirecting to a *different* creature would change what the spell does, not offer a graceful same-intent pivot, so it never gets one — this holds even when several creatures were initially eligible. **Card text can't be used to tell which is which** — a card like "Move target creature to target location" calls both a "target," but only the location half redirects; the creature stays whatever was first chosen even when the location fails and gets redirected, and if the creature-target were what failed instead, that's a clean hard-fail for that instruction, never a fallback to a different creature. Move/Summon/Relocate's destination is always location-shaped, so nothing changes for them. Actions with only one possible location (Descend, when blind — D67) or with a permanent-typed choice instead of a location have nothing to redirect to and reduce to a single attempt — legal, or a clean hard-fail with cost sunk. (A control change never fails on occupancy at all, D86.)

## Costs
- **AI** must evaluate responses for Quick/Reactive plays (search over "respond vs. pass"); not for Instant, which never opens a response tree.
- **Netcode** must handle Quick/Reactive response windows over the wire (whose turn to respond, timeouts); not for Instant.
- **Timing/UX**: players need clear, fast "respond or pass" prompts; auto-pass when a player has no legal response keeps it snappy.

## Invariant vs. mutable
- **Invariant:** the Pending zone and the Speed check always exist; resolution order within Pending is LIFO; deterministic. Nothing can respond to an Instant.
- **Mutable (card-driven):** what Speed a given card/ability has, what triggers exist, extra priority a card grants.

## Open questions
1. **Timeouts:** in real-time online play, how long is a response window before auto-pass?
2. **Trap limits:** how many traps can be pre-set? Are they revealed on trigger only, or can they be scried/detected (pillar 6 interplay)?
3. **Depth cap within Pending:** allow unlimited Quick/Reactive responses-to-responses, or cap depth for simplicity/UX? (Moot for Instant, which nothing can respond to.)
