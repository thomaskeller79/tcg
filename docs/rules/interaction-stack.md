# Interaction & Speed

*Players can act on the opponent's turn (MTG-style instants). One primitive covers instants, combat tricks, and traps: a **Speed** tag per card/ability, checked live against what's sitting in **Pending**, the Aether's next-to-resolve zone.*

**Decisions:** D6, D16, D35, D38, D45, D46, D68, D70, D75, D83, D85, D91, D92, D94, D95, D100, D115, D116, D117, D118, D123, D124, D127, D128, D130, D132, D133, D134 (`history/decisions.md`)

---

## The primitive
> **Speed.** Every card/ability has one of four speeds — **Slow, Quick, Reactive, Instant** (D45) — and legality to play it is a live query against the current contents of **Pending** (the Aether's next-to-resolve zone, D38).

| Speed | Playable whenever… | What happens after |
|---|---|---|
| **Slow** | it's the controller's main phase, and Pending is empty. | Enters Pending, waits its turn, resolves and fades normally. |
| **Quick** | only **Physical Traces** are in Pending — the trace left by Move, Attack, Ascend, Descend, or Defend — including when Pending is empty (D124). | Same as Slow. |
| **Reactive** | no **Instant Trace** is in Pending. | Same as Slow — a normal Pending entry, respondable by anything whose Speed currently permits it. |
| **Instant** | no **Instant Trace** is in Pending. | **Blocks all active play until it has resolved**: while it sits in Pending, nothing can be actively put into the Aether (no cast, no activation — **no response window for anyone, not even another Instant**) and `Now` advances automatically. Triggers are still added and resolve normally. |

**Move/Attack/Ascend/Descend are Slow (D75), not Quick** — Quick describes what *other* cards can play while a Physical Trace already sits in Pending (combat tricks, below), not the physical action's own speed. Being Slow means such an action can't be declared while Pending is non-empty, so **at most one Slow physical action ever sits in Pending at a time** — an actual enforced consequence of the Speed system, not merely how things happen to play out. **Defend is the one Quick physical action (D117):** its traces stack on top of the Attack they answer.

Pending is the zone just ahead of `Now` — `Now` itself is **not a zone**, just the moving point dividing Pending from Past. **`Now` advances past whatever's next in Pending when both Champions pass priority in succession** (neither has anything they want to play). The sole exception is an Instant: while one is in Pending, `Now` advances automatically, never waiting for both Champions to pass, until the Instant has resolved. As things resolve they cross `Now` and become past **traces**, which then fade (except Physical Traces, which get zero Past residency at all).

**Priority (D123):** a Champion with priority may act as often as they wish; acting never passes priority, only passing does. A pass hands priority to the next Champion in turn order (APNAP). Whenever Pending is non-empty, in any phase (D128), the active Champion gets priority — in a neutral turn, the next Champion in turn order — and gets it again after each trace resolves. **Beginning and End** end as soon as Pending is empty. **The Action phase** also gives priority with Pending empty — that is where the active Champion acts — and ends only when every Champion has passed in succession with Pending empty: the active Champion ending its phase is a pass, and every other Champion then gets priority once with Pending empty, so Reactive, Instant and Quick cards can be played on the opponent's turn even when nothing is in Pending. In a neutral turn, this pass round runs once the Behaviors are done. Setup opens no priority (`setup.md`).

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
- **Triggered:** "when X happens, do Y" (table above) — enters Pending automatically the instant its condition is met, no activation choice involved. A triggered ability can still carry its own cost, paid **at resolution** rather than upfront, typically as an optional clause ("whenever a creature enters within distance 2, **you may pay 2 mana**; if you do, draw a card"). Funding for that resolution-time cost follows the ordinary Ancestry payment walk (D26) like any other cost; only the controller (D28) decides whether to pay. **A triggered ability is Reactive unless its card states another Speed (D128).** In Pending, a trigger's Speed only matters as Instant or not: an Instant trigger resolves with no response window; any other trigger is respondable by Reactive and Instant cards.

## Combat integration (D117)
An Attack is a **Physical Trace** (D45): it enters Pending exactly like any other trace, so it just sits there, respondable, until `Now` reaches it.

**Defend is an ordinary activated ability of Quick speed:** "Defend target attack" (`1~AP`; `2~AP` for a Champion or Companion while network-bonded, D116, D134). A permanent of the attacked entity that has Defend, in the attacked Slice (D115) — one whose own attack reach covers the attacker's distance (D127) — plays it in response to the Attack trace; it leaves its own Physical Trace on top. When that trace resolves, it **adds its creature to the Attack trace as a defender** — an ordinary change to the trace's state. Not defending is just passing.

**Defending bit by bit.** Defenders are not declared once: each Defend is its own response, so defenders accumulate one at a time, interleaved with tricks from both sides. The attacked entity can defend with one creature, see the attacker's trick (a pump, removal on the defender), and then add another defender; the attacker can answer each Defend with another trick, as its Speed allows. The exchange ends when everyone passes in a row and the Attack resolves. Consequences, accepted on purpose: a valuable creature never has to commit until it's needed; the defender can always answer last with another body while it has eligible creatures with AP, which weakens the attacker's tricks and removal aimed at defenders; together with gang-up (D13) this tilts combat toward the defender, braked by each Defend's AP cost and lock. A playtest point (`history/playtest-variants.md`).

**When the Attack resolves, it checks whether it still has a defender** (one that is still there and still eligible):
- **Defended:** the attacker splits its Attack freely among the remaining defenders; every defender deals its Attack back (D13).
- **Undefended** (no defender was added, or none remains — e.g. killed in a response): the attacker splits its damage freely among all legal targets — the attacked entity's permanents that have Life, in the attacked Slice of that terrain, in the attacking Champion's view (D133) — with no retaliation.

**The attacker stays where it is (D118)**, even when no enemy is left in the attacked Slice; advancing after combat is a keyword's job (pessimistic default). Attacking does not by itself reveal hidden permanents on the attacked hex — each concealment effect says what breaks it (`asymmetric-information.md`).

Tricks (Quick, Reactive, Instant) can be played at any point while the Attack is in Pending, as their Speed allows.

## Casting and activating: legality, amounts, choose, target, pay (D46, D70, D91, D130)
Playing a card or activating an ability is a strict procedure. No step may look ahead at a later one, and none is revisited once passed:

0. **Legality.** The card/ability is offered as a legal play only if its Speed permits it against Pending's current contents, its **cast condition** (if printed, e.g. "cast only if you control a Rebel") holds, and at least one legal cost-and-target combination exists — no paying into a target-less cast. The cast condition is checked here only: paying a cost that makes it false (felling the only Rebel) is legal.
1. **Amounts.** Every variable amount is fixed first — `X` in a cost, and "fell any number of creatures" written as `X` — because later choices, targets and the cost read it.
2. **Choose.** Every other choice is made: modal selections (below), and **every choice of how the cost will be paid** — which branch of each cost disjunct, which creature to fell, which card to discard — so a target may depend on it ("destroy target creature sharing a type with the felled creature"). A cost is a **CNF** (a conjunction of disjunctions): each disjunct is one of four kinds, evaluated and paid in this fixed order — **colored mana**, **mana with choices** (including hybrid mana, but also a disjunct that mixes a mana branch with a non-mana alternative, e.g. "3 fire mana or fell a creature you control"), **neutral mana**, then **additional costs with no mana alternative**. A disjunct may have an **empty branch (ε)**, which makes it optional. Branches are **labelled** (`c2 = (b1: 1 ∨ b2: 2 ∨ b3: 3)`), and an instruction's condition may test which branch was paid (`c2 = b1`) — by label, never by amount, since two branches can cost the same. This covers per-mode costs: kicker is an optional conjunct plus a condition; a "+{1}: A / +{2}: B, choose one or more" card is one disjunct with a branch per mode combination. Payment is **semi-automatic**: a disjunct with only one currently-legal branch resolves without a prompt *only when every branch in that disjunct is plain mana* — a disjunct mixing mana with any non-mana branch (fell, discard) always requires explicit confirmation, even when only one branch is legal, since silently consuming a permanent or card is never an acceptable default. The card itself is part of the cost (D37, D68): it leaves the Hand as payment starts but arrives in Discard only when payment ends, so it can never pay for itself (e.g. a "destroy a card in your Discard" cost). **An abort option is available at any point up until the trace actually enters the Aether** (see below). A modal selection may be a "choose from this menu `N` times, repeats allowed" structure, where `N` was already fixed in step 1. A choice may name its **chooser**, who need not be the one casting. Every value resolved in steps 1–2 — `X`, modes, payment choices — becomes an ordinary, fixed, durable property of the resulting trace from this point on (`object-properties.md` §4).
3. **Target.** Each target the card/ability requires is chosen after every other choice, against the game state as it now stands. **A target is required only if an instruction in a chosen option references it** — known by now, since every choice is already made. A target declared at card level is one shared instance that any instruction may reference ("that creature," "it" are references to it, not new targets); a target declared inside a modal option gets a fresh instance for every pick of that option. The same object may be chosen by two separate picks within one repeated-choice resolution — each pick is its own instance of a target requirement, the same way a card that says "target" twice may repeat a target across those two instances, but never within one. A target has a count `min..max` ("up to one target creature" allows none); an instruction left with no bound target does nothing. **Target is the only step whose output never locks in** — it's freshly re-derived at every future instantiation, in either direction (`object-properties.md` §4).
4. **Pay.** The cost is now fully known, including any part that depends on a target (e.g. "targeting this costs 1 more"), which is computed against the **caster's view** (D130), and it is paid with the choices made in step 2 — **paying itself makes no choices**, except for a surcharge another card adds (e.g. a creature's "targeting this costs a discard"): its choices are made when it is paid, here or at a top-up, since nothing on the casting card can read them. Paid cost becomes a durable property of the trace like the values of steps 1–2.
5. **Enter Pending.** The trace now exists in the Aether.

**A trace does not exist in the Aether — isn't visible, isn't respondable-to, isn't reachable by anything including Remand — until step 5.**

**Casting is a transaction (D94).** Nothing done in steps 0–4 — mana spent, a creature felled, a card discarded, the card itself leaving the Hand — is visible to the opponent before step 4, and triggers it fires (e.g. "whenever a creature falls") are collected and enter Pending only together with the trace, on top of it. **Abort rolls the whole transaction back**: every paid cost is returned and the collected triggers are dropped, leaving the state exactly as it was before step 0.

**A trace's displayed text is generated by collapsing the card's own template as each step resolves**, not by revealing a fixed hidden block: an unpaid cost alternative disappears once its sibling branch is chosen (e.g. "3 fire mana or fell a creature you control" becomes just "fell a creature you control"); the effect menu is replaced by whichever instructions were actually chosen once step 2 completes, and a clause conditioned on a now-permanently-settled fact (e.g. "if a creature was felled to pay for this, also...") collapses to a flat, unconditional clause; each `target X` placeholder becomes an annotated binding (e.g. `target creature (Grunt #145)`) once step 3 resolves. **The annotation is live game data, not decoration** — resolution reads it directly to determine what the trace actually affects.

## Binding: every reference is fixed at declaration (D83, D91)
Every reference in the trace's text is bound before the trace enters the Aether, **as early as possible**: each binds as a side effect of whichever step fixes what it depends on. Source-relative references ("you," the payer, an inherited Behavior and neutral turn) bind right after step 0 — before Pay, which can change the Ancestry (a "fell this creature" cost removes the source). Target-relative references ("target creature's controller," a chooser named that way) bind right after step 3.

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

**Top-up (D130).** A cost part that depends on a target was paid against the caster's view. If against the true state that target costs more, the caster **may** pay the difference at the start of resolution, before the first instruction runs, separately for each target; a target that isn't topped up becomes illegal for this trace, so every instruction bound to it fizzles. A lower true cost is not refunded. The mismatch is a hard fact against the caster's view: the hidden-information ability collapses as its own text states (`asymmetric-information.md`, D131).

**Falling is immediate.** A permanent whose current Life is 0 or less falls immediately after the instruction that caused it; "fell" and reaching 0 Life behave identically, and the next instruction already sees the result. "Destroy" also takes effect immediately but leaves nothing and fires no fall triggers. Damage dealt by one instruction is simultaneous — "deal 1 damage to each creature" and a Combat's damage (D13) apply fully before anything falls. Text order therefore matters: "deal 3 damage to target creature; it gets +3 max-Life" fells a 3-Life creature, and the second instruction fizzles: the creature no longer exists (identity check, D85).

**Immediate consequences (D112).** What the rules themselves make happen because of an instruction happens in the same step, right after it — never as a trigger and never as a trace, so there is no Pending entry, no ordering among triggers, and no gap the rest of the trace could observe; it is part of the instruction's output (D96). This replaces MTG's state-based actions: there is no separate check phase. Falling's immediate consequences: the permanent ceases to exist, and by type —
- **Creature, Companion:** a Remnant is created on its terrain (D110).
- **Structure:** nothing (D111).
- **Champion:** the match ends (D9). If both Champions fall from the same instruction, the match is a **draw** (D113).

Cards that react to falling ("when a creature falls…") are ordinary triggers and go through Pending; when one resolves, the Remnant already exists.

**An instruction can bind an output** — a named result, such as the objects it felled — that later instructions in the same option can reference: `felled := damage(T1, 3)`, then `[if felled ≠ ∅] draw(1)`. This is what "if it falls this way" means, as opposed to "if T1 has fallen" (which something else may have caused). An output is unbound if its instruction didn't run, empty if it ran or fizzled without affecting anything; an instruction reading an unbound output doesn't run either. Scope and states: `effect-form.md`.

**Every state change is an instruction**, not only a trace's text: paying a cost and a phase-boundary event (an "until end of turn" effect ending) are instructions too, so the falling rule above applies to them unchanged. An instruction does not require a trace. A trace is one way instructions are executed — queued in Pending, respondable; the casting procedure and the turn structure execute their instructions directly, never through Pending, so paying a cost or an effect expiring can't be responded to.

**Triggers fired during resolution are collected.** When the last instruction has run, the trace crosses into Past (its final step, so a trigger that looks at Past sees it), and then the collected triggers enter Pending so that **the triggers of the earliest instruction end up on top** and resolve first. Triggers from the same instruction enter Pending in this order, each later rule breaking ties in the earlier ones:

1. **APNAP over the four seats** (`Champion A, Neutral A, Champion B, Neutral B`, starting from the active seat, `neutral-permanents.md`) — the active seat's triggers enter first and so resolve last;
2. **source ID**, oldest first (`neutral-permanents.md` §Permanent identity);
3. **printed ability order** on the source;
4. **the order of events inside the instruction** (e.g. creation order).

Fully identical triggers are interchangeable and need no order. The order is always automatic, never chosen by a Champion, and is visible in Pending before anyone has to respond.

## Hidden occupants never block a destination (D132)
A destination-choosing action (Move, Summon, Relocate/push) checks its destination at declaration against the **acting Champion's own view**, like any target: the Slice must have room and every *visible* occupant must share the actor's controller (`overview.md` §2). Nothing at casting is checked against true state. A hidden occupant of another controller doesn't block entry — the actor enters and the Slice becomes mixed. At resolution only the 3-slot capacity is checked, against true state: a Slice that is truly full makes the instruction fizzle (§Resolution), its cost spent. An enemy that becomes visible or enters in response doesn't stop the move; only filling the Slice to capacity does.

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
