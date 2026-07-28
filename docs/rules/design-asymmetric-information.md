# Design — Asymmetric Information (Pillar 6)

*Players do not share one view of the board. What each player perceives is itself a manipulable, card-driven property. This is a headline pillar and a deep architectural commitment — captured here so it shapes everything from day one.*

**Status:** Design note · **Date:** 2026-07-23

---

## Why this exists
A digital TCG can do something a physical one fundamentally cannot: **give two players genuinely different, actively-manipulated views of the same board.** This is largely unexploited. We treat it as a core design axis, not a garnish:
- **Mimic** — a unit appears to the opponent as a different unit (lies about its identity/stats).
- **Submerged** — units that move "below the surface," invisible to the opponent by default until detected.
- **Mist** — a summoned region where one player has vision and the other does not.
- **Bluffs, decoys, hidden deployment, fog** — all as first-class mechanics.

## The core model
> **The engine holds one authoritative *true state*. Each player sees a *view* — a projection of true state through a per-observer perception layer.**

```
                 ┌───────────────┐
                 │  TRUE STATE   │   (authoritative, deterministic)
                 └───────┬───────┘
              perception layer (per observer)
              ┌──────────┴──────────┐
              ▼                     ▼
      ┌──────────────┐      ┌──────────────┐
      │ View: Player A│      │ View: Player B│   (may disagree!)
      └──────────────┘      └──────────────┘
```

## Perception is just another query axis
We already committed (pillar 5) to *never reading raw values — always querying*. Asymmetric info extends the query layer with a **perception dimension**. Three query kinds:

| Perception query | Question | Example modifier |
|---|---|---|
| **Visibility** | Does observer P see object O at all? | Mist hides a region from B; Submerged hides a unit until detected. |
| **Appearance / identity** | *What does O look like* to observer P? (may differ from truth) | Mimic makes O report a decoy's name/stats to the opponent. |
| **Knowledge** | What does P know about a hidden zone / hidden facts? | "Reveal the top card of enemy deck to you only." |

**Cards modify perception with the same modifier system as everything else.** A Mimic effect adds an appearance-modifier to O for the opponent-observer. Mist adds a visibility-modifier to a region for one observer. A Detection ability adds a visibility-modifier that *removes* a Submerged unit's concealment. No special-case subsystem — it's the modifier/query engine applied to perception.

## The below layer is structural hidden space (D12)
A hex has three vertical layers (ground / above / **below**); the **below (submerged) layer is hidden by default** — its occupancy appears only in its owner's view. This makes concealment a *structural* part of the board, not only a card effect: Submerged movement simply *lives* in the below layer, and Detection/reveal are visibility-modifiers that expose it. It's the clearest worked example of perception-as-query.

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
- **No deduction engine.** The engine guarantees hard facts are truthful and enforces a **small, finite, documented set of conservation laws** (e.g. *sum of claimed mana costs this turn ≤ public network max* → auto-collapse over-claims; card-count / library-size conservation). **All other deduction is the human's** — the UI surfaces hard facts; players see through bluffs themselves. The mind-game lives in the player, not the CPU.
- **A lie is cheap until reality tests it.** A mimicked stat holds until combat makes true and fake outcomes diverge (deals unexpected damage, or dies when the fake said it lives); a mimicked cost holds until conservation can't reconcile it. Even pure hiding leaks through **counts** (know they have 5 units, see 4 → one is submerged).
- **Collapse → full truth (default).** When a claim breaks, the observer learns the *entire* truth, including which of a masked pool it really was (shown as 2/1 masking A-or-B → reveal which). A per-card partial-reveal is possible later but not the default.

## Resource observability — the hard/soft border (D18)
The border is a **principle, not a per-resource list**:
> **Standing state is public. The private-resource *ledger* (what a given action drew) is hidden. Any cost with a hard board footprint is self-revealing.**
> **⇒ A spell is mimickable exactly when its whole cost is payable from the hidden mana ledger.**

| Fact | Default | Why |
|---|---|---|
| **Mana network** (bonded terrain, max) | **Public** | must see it to deny it (D8) |
| **Mana spent / live balance** | **Hidden** | the engine of cost-deception; also "do they have mana for a trick?" bluffs (D5) |
| **Costs with a board footprint** (sacrifice, unbond, discard) | **Self-revealing** | can't be faked → that spell isn't mimickable, and that's fine |
| **Hand size (count)** | **Public** | legibility; the cast-A-as-B bluff needs only hidden mana-spend. Contents always private. A card may *obscure* the count (opt-in). |
| **Action Points (max & current)** | **Public** | board readability (pillar 3); AP surprises come from instant-speed **timing** (D5, the +1-AP ambush blocker), not hidden state |
| **Hidden-zone contents** (hand cards, submerged units, mimicked trace identities) | **Hidden** | core pillar 6 |
| **Unit stats/positions, terrain, buildings, graves, life** | **Public** | D7 legibility |

The **only** standing quantity hidden is the **live mana balance** (because per-spell spends are never shown) — one deliberate, minimal carve-out from D7, justified by deception being a headline pillar. Interacts with D8 open #4 (**bank** vs **refresh**: banking gives more bluff room).

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
- **Invariant (never removed):** a true state exists; a per-observer view layer always exists; every player always has *some* view; the server is the sole authority on true state.
- **Mutable (card-driven):** what any observer sees, what any object appears as, what any region reveals/conceals, what knowledge a player holds.

## Open questions
1. **Default visibility:** is the board **fully visible by default** (hidden info is opt-in via cards like Mist/Submerged), or is there baseline fog-of-war? *(Recommendation: full visibility by default — simplest default rule; concealment is added by cards. Matches the "simplest default that still exposes the hook" principle.)*
2. ~~**Reveal triggers**~~ — **largely resolved (D18):** a lie collapses when a **hard fact tests it** (combat outcome divergence, conservation-law violation) → **full truth** revealed. Detection is an explicit visibility-modifier on top. *(Remaining: exact list of hard "test" events.)*
2b. **Default spell-resolution visibility (open, lean Model 2):** when you cast a **non-mimic** spell, does the opponent see the **true card/resolution** (D7; `Mimic` overrides holistically — *recommended*), or only ever the *effect* (ambient fog, less legible)?
3. **Bluff economy:** can a player deploy face-down/unknown units generally, or only via specific cards?
4. **Fairness of information:** do players get told *that* they lack information (e.g., "there is fog here") or can information be hidden so completely they don't know to look?
5. **Reveal / view-merge (D12, user-flagged unsure):** when a "reveal" effect exposes the hidden **below** layer, how does the owner's hidden sub-view merge into the opponent's view — *scope* (one hex / region / all), *duration* (instant snapshot / lasting), *granularity* (existence only / full identity)? Structurally already supported (reveal = a visibility-modifier on the perception layer); the mechanic shape is the open part.
