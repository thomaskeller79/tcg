# Asymmetric Information (Pillar 6)

*Champions do not share one view of the board. What each Champion perceives is itself a manipulable, card-driven property.*

**Decisions:** D7, D12, D18, D41, D42, D67, D71, D102, D118, D131 (`history/decisions.md`)

---

## Why this exists
A digital TCG can do something a physical one fundamentally cannot: **give the two Champions genuinely different, actively-manipulated views of the same board.** This is largely unexploited. We treat it as a core design axis, not a garnish:
- **Mimic** — a unit appears to the opponent as a different unit (lies about its identity/stats).
- **Submerged** — units that move "below the surface," invisible to the opponent by default until detected.
- **Mist** — a summoned region where one Champion has vision and the other does not.
- **Bluffs, decoys, hidden deployment, fog** — all as first-class mechanics.

## The core model
> **The engine holds one authoritative *true state*. Each Champion sees a *view* — a projection of true state through a per-observer perception layer.**

```
                 ┌───────────────┐
                 │  TRUE STATE   │   (authoritative, deterministic)
                 └───────┬───────┘
              perception layer (per observer)
              ┌──────────┴──────────┐
              ▼                     ▼
      ┌──────────────────┐      ┌──────────────────┐
      │ View: Champion A │      │ View: Champion B │   (may disagree!)
      └──────────────────┘      └──────────────────┘
```

**A view is not just a masked subset of true state.** The perception layer doesn't only hide/reveal parts of the truth — it can also inject content a card actively *claims*, which may be false (D18's `face`/claim model, below). So true state and each Champion's view are independently-tracked values that can **all three differ simultaneously**, not one ground truth plus redacted copies of it.

**Worked example:** a creature with 2 power. Champion A casts a secret effect: `Target creature loses 1 power. Secret.` Champion B then casts a secret effect: `Target creature gains 2 power. Secret.` True state: 2 − 1 + 2 = **3** power. Champion A, unaware of Champion B's effect, believes the power is **1**. Champion B, unaware of Champion A's effect, believes it's **4**. Three different values, live at once, none of them a subset of either of the others — see the belief model below for how such claims are authored and kept consistent until a hard fact collapses them.

## Perception is just another query axis
We already committed (pillar 5) to *never reading raw values — always querying*. Asymmetric info extends the query layer with a **perception dimension**. Three query kinds:

| Perception query | Question | Example modifier |
|---|---|---|
| **Visibility** | Does observer P see object O at all? | Mist hides a region from B; Submerged hides a unit until detected. |
| **Appearance / identity** | *What does O look like* to observer P? (may differ from truth) | Mimic makes O report a decoy's name/stats to the opponent. |
| **Knowledge** | What does P know about a hidden zone / hidden facts? | "Reveal the top card of enemy deck to you only." |

**Cards modify perception with the same modifier system as everything else.** A Mimic effect adds an appearance-modifier to O for the opponent-observer. Mist adds a visibility-modifier to a region for one observer. A Detection ability adds a visibility-modifier that *removes* a Submerged unit's concealment. No special-case subsystem — it's the modifier/query engine applied to perception.

## The Root Slice is structural hidden space (D12, D78)
A hex has three vertical Slices, bottom to top (Root / Ground / **Sky**); the **Root Slice is hidden by default** — its occupancy appears only in its owner's view. This makes concealment a *structural* part of the board, not only a card effect: Submerged movement simply *lives* in the Root Slice, and Detection/reveal are visibility-modifiers that expose it. It's the clearest worked example of perception-as-query.

**Second worked example (D42):** the **Subterranean** keyword bundles this same structural concealment together with the ability to be in the Root Slice at all — a Unit without Subterranean simply cannot go there by default, so the concealment and the access gate are one package, not two independently-toggleable questions. A card effect can still force a non-Subterranean unit into the Root Slice as an explicit exception, at which point the same structural hiding applies to it exactly as it would to a Subterranean unit — concealment is a property of the *Slice*, not of the keyword.

**Proximity exception (D67):** "hidden by default" is narrower than "hidden from everyone but the owner." An observer sees a hex's Root-Slice occupants if **any permanent they control** has Root-Slice presence on that hex or on a neighboring hex — matching-Slice presence specifically, not just any presence (a Ground or Sky unit standing on a hex gains no Root-Slice vision of that same hex at all), and it's a per-*observer* fact, not a per-*creature* one: it can come from any of the observer's own permanents, not specifically the one about to act. Root-Slice vision is a **live, continuously-recomputed query**, exactly like every other perception query on this page, not a one-time reveal — it can be lost again the moment the qualifying presence leaves or ceases to exist. Range is exactly one hex, and it costs a real, killable creature to establish, so it's a bounded, costed way to "bring light into the dark," not a free scouting tool.

**This is why entering any Slice is blind exactly when the acting Champion has no qualifying presence already covering the destination** (`overview.md` §4) — not an "Ascend is safe, Descend isn't" rule. A creature Ascending is *usually* safe only because its controller is usually already standing right there; Ascending into a Mist-concealed Ground hex is exactly as blind as any Descend. A creature Descending is *usually* blind only because a controller usually has no independent Root-Slice presence at that hex yet — but if a second Subterranean scout is already stationed there or on a neighboring hex, the Descend is perfectly safe, because the visibility belongs to the Champion, not to whichever specific creature is moving.

## Concealment effects specify their own breaking (D118)

There is **no base rule** that attacking or entering a hex reveals what is hidden there. The default Root concealment breaks only by D67's proximity exception above — which in practice breaks it on contact, since a Melee attacker or a mover is in or next to the hex. **Every other concealment effect** (Mist, Submerged, further Root effects, Mimic) must state on its card or keyword what happens when:

- **C-a.** its hex is attacked in its Slice group,
- **C-b.** an enemy enters its hex in its Slice group,
- **C-c.** the concealed creature acts itself (attacks, defends, uses an ability).

**Slice groups:** Root alone; Ground and Sky together (Sky defends Ground, D115). Recommended default for C-a/C-b, which a card follows unless it says otherwise: the concealment breaks — attacking or entering Root reveals Root occupants, attacking or entering Ground or Sky reveals Ground and Sky occupants. Breaking concealment lifts only the concealment, not a Mimic face (a Mimic collapses by its own rules, e.g. when it acts), and like Reveal (D102) it is a one-time event.

## Example mechanics (seed — flesh out later)
| Mechanic | Truth | What the opponent perceives |
|---|---|---|
| **Mimic** | A 5/5 dragon | Appears as a common 1/1, until it acts/is revealed |
| **Submerged** | A unit on hex X moving under the surface | No unit visible unless opponent has Detection nearby |
| **Mist (region)** | Real units maneuvering inside | Opponent sees only "fog" over those hexes |
| **Decoy** | Nothing / a token | Appears as a threatening unit |
| **Scry/Spy** | Opponent's hidden info | *You* gain knowledge the opponent doesn't know you have |

## The consistency problem & belief model (D18)
The hard core of pillar 6 is not *hiding* a fact but keeping a *false view internally consistent* — and defining what happens when reality contradicts it.

**Governing principle:**
> **A soft claim is cheap and holds until a *hard fact* tests it. Then the truth is applied, and the lie collapses.**

- The engine **never fabricates a consistent alternate reality** (rejected: infinite regress, unbalanceable, not authorable). **Cards author *claims*;** the engine projects them and does nothing to back them up.
- **No deduction engine.** The engine guarantees hard facts are truthful and enforces a **small, finite, documented set of conservation laws** (e.g. *sum of claimed mana costs this turn ≤ public network max* → auto-collapse over-claims). **All other deduction is the human's** — the UI surfaces hard facts; players see through bluffs themselves. The mind-game lives in the player, not the CPU.
- **A lie is cheap until reality tests it.** A mimicked stat holds until combat makes true and fake outcomes diverge (deals unexpected damage, or falls when the fake said it survives); a mimicked cost holds until conservation can't reconcile it. Even pure hiding leaks through **counts** (know they have 5 units, see 4 → one is submerged).
- **Every hidden-information ability states its own collapse (D131).** A concealment, a Mimic face or any other ability that creates hidden or false information states on its card when it collapses and what part of it collapses. What is revealed need not be the full truth, but it must fit what the observer saw — the game must never make a player remember that a creature shown as 2 power really dealt 4.
- **Collapse → full truth (fallback).** For a broken claim its card doesn't cover, the observer learns the *entire* truth, including which of a masked pool it really was (shown as 2/1 masking A-or-B → reveal which).
- **Reveal (keyword, D102)** forces the same collapse on purpose: it sets one player's belief about an object equal to its true state, lifting any Mimic face. It is a one-time event, not a lasting state — an object that becomes hidden again afterwards is not tracked.

## Beliefs and reveal points (D137)
**Every view is a belief.** An observer's view is what they observed, with claims taken as true. A player's view of what they own is true unless a card changes it.

- **Private to the owner:** the true numbers — a permanent's stats and resources (Power, Life, AP) and every pool and count (mana, Hand, Library, Discard sizes) — and the contents of the Mind zones.
- **Visible by default:** positions, identities, terrain, bonds and the mana network (D8: denial needs it), Structures, Remnants.
- **A change is seen by every player whose view contains the object.** A fell is seen by those who see the permanent; an unbond by everyone; a discard by no one but its owner.

**Reveal points** are always true; no card can fake them:

| Reveal point | Shows | To whom |
|---|---|---|
| A permanent is dealt damage | its true Life | every player who sees it |
| A permanent deals damage | its true Power | every player who sees it |
| A permanent's AP reaches 0 | that it is exhausted | every player who sees it |
| A Hand becomes empty | that it is empty | everyone |
| A mana pool refreshes (Beginning, D77) | its true amount | everyone |
| A cost is paid | the true value of the paid resource | the payer's controller |

Library and Discard sizes have no reveal point; a card reveals them. Damage is the only way an instruction lowers Life; paying Life as a cost is not damage.

**Costs and Mimic.** A cost is paid from private resources, so the spend doesn't contradict a claimed cost; a cost that changes something an observer sees (felling a visible creature, an unbond) shows that change. A face whose Life or Power differs from the truth collapses at the first combat; a face copying the same stats survives it.

**GUI support:** the UI shows each observer an expected value per number (their view) and marks the resources a hidden trace's possible payers could have spent (payment walk, `ancestry.md`). Effects of a hidden trace are not marked.

## Representing deception: claims, Mimic, ChooseOne (D18)
**The atomic primitive is a `face`:** a per-observer overlay (claimed identity + stat modifiers) that a **permanent** carries. Truth lives in true state; a face is the per-observer lie laid over it. This **unifies** the two "mimic" ideas — a creature *entering with a face* and a spell *applying a face to its target* are the **same operation**; deception cards are just "attach / alter a face." A card's own Aether trace shows the **same** face (board + trace consistent for free — one authored thing). Any face is governed by the belief model above (holds until a hard fact collapses it).

Deception is authored with **composable effect combinators**, projected through a per-observer layer:
- **`ChooseOne[…]`** — modal effect; the caster picks **privately** → hits **true state** + caster's view.
- **`Mimic(face)`** — the effect that **applies a `face`** to a permanent (and shows that face on the resolution's trace), covering the object's **whole life in the opponent's Aether** (stack *and* trace — hides the true card identity too, else the mimic leaks via the card name).

**Key engine capability:** *events/effects carry a per-observer projected form, not just objects.* On resolve: true form → true state + caster's view; `face` → each opponent's view as a **soft claim** governed by the belief model above.

**The claim mutates view *state*, board included — not just the trace.** Because a view is a full projected game state and `face` is itself an effect, projecting `face` **applies it to the target object in the opponent's view**: the buffed creature ends up with a *true* modifier (`+2 Attack`, true state) and a *claimed* modifier (`+3 Life`, opponent's view) simultaneously. So **modifiers/buffs can be per-observer** (the appearance axis extended from identity to stats). Trace and board stay consistent automatically — both derive from the one authored `face`. The engine does **not** actively maintain believability: real later changes apply to both views, and the fixed divergence simply **rides along until a hard fact** (combat damage dealt/taken, death) can't reconcile → collapse to full truth.

**Worked card:**
```
Deceptive Boon   cost {mana 2}
  Mimic(face: Buff target +3 Life)         # opponent perceives exactly this, always
    over  ChooseOne[ Buff +2 Attack , Buff +3 Life ]   # caster picks privately → true state
```
Because the face is *always* +3 Life, the honest cast and the +2-Attack bluff are **indistinguishable** to the opponent — they can't tell a lie is even present. Plausibility of `face` is the **designer's** job (pick something a real card could do); the engine only enforces hard-fact conservation.

## Architectural consequences (real costs, accepted now)
1. **Networking is server-authoritative and view-redacted.** Clients receive only their own view — never true state. A modified client must be unable to reveal hidden info. (Reinforces the deterministic-core + server-authority design; kills any trust-the-client shortcut.)
2. **The engine emits per-observer event streams.** One true event → up to N transformed/redacted events, one per observer. "Unit moved into mist" may be a full event to its owner and absent (or a vague signal) to the opponent.
3. **AI must handle imperfect information.** Proper play needs belief states over hidden facts — genuinely hard. Prototype AIs may see true state to function, but that is *unfair by construction* for a deception game; the AI's perception must eventually be gated to its own view.
4. **UI renders a view, never true state.** Two clients, two renderings. Replays are also per-observer (or an omniscient "director" view for post-game review).
5. **Hotseat caveat.** Shared-screen hotseat conflicts with hidden info (both players see everything). Options: a "pass-the-device / hide screen" step, or degrade hidden-info fidelity in hotseat only. Affects mode ordering (see PLAN §5).

## Invariants vs. mutable
- **Invariant (never removed):** a true state exists; a per-observer view layer always exists; every Champion always has *some* view; the server is the sole authority on true state.
- **Mutable (card-driven):** what any observer sees, what any object appears as, what any region reveals/conceals, what knowledge a Champion holds.

## Open questions
1. **Default visibility:** is the board **fully visible by default** (hidden info is opt-in via cards like Mist/Submerged), or is there baseline fog-of-war? *(Recommendation: full visibility by default — simplest default rule; concealment is added by cards.)*
2. **Exact list of hard "test" events:** D18 settles that a lie collapses when a hard fact tests it; beyond the reveal points (D137), the catalog of qualifying events isn't enumerated yet.
3. **Default spell-resolution visibility (lean Model 2):** when you cast a **non-mimic** spell, does the opponent see the **true card/resolution** (D7; `Mimic` overrides holistically — *recommended*), or only ever the *effect* (ambient fog, less legible)?
4. **Bluff economy:** can a player deploy face-down/unknown units generally, or only via specific cards?
5. **Fairness of information:** do players get told *that* they lack information (e.g., "there is fog here") or can information be hidden so completely they don't know to look?
6. **Reveal / view-merge (D12):** when a "reveal" effect exposes the hidden **below** layer, how does the owner's hidden sub-view merge into the opponent's view — *scope* (one hex / region / all), *duration* (instant snapshot / lasting), *granularity* (existence only / full identity)? Structurally already supported (reveal = a visibility-modifier on the perception layer); the mechanic shape is the open part.
