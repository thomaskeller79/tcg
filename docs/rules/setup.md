# Match Setup

*How a legal match's four components (card deck, terrain deck, Champion, Map) combine into the actual starting board state — and why this isn't a domain transition, even though Champion and Terrain are printed and collected like any other card.*

**Decisions:** D76 (`history/decisions.md`)

---

## Setup is not a domain transition

Champion and Terrain are printed and collected like any other card, but never occupy Hand/Library/Discard, ever (`object-properties.md` §1) — they are a different kind of game object from Creature/Spell/Structure/Item/Companion despite sharing the word "card" for the deckbuilding-tier concept (a real naming collision, tracked at `PLAN.md` §8 item 13, deliberately not resolved here). Because they never pass through a Mind-domain zone, their arrival on the Island at Setup is not a Mind→Matter transition at all — there is no Card object for them to skip the Aether from. Setup is a distinct, more primitive kind of event: it reads directly from a player's collection and creates Permanents, with no Card or Trace ever created for Champion or Terrain specifically.

## Setup is invisible and non-interactive

Setup runs entirely in the background, before either player's first real decision point. Players cannot observe it, interfere with it, or respond to anything that happens during it — there is no priority, no Pending, no response window at all while it runs. This is a structural property of Setup itself, not a targeted ruling on any one sub-question (it's what makes the placement-order question below inconsequential, without needing its own rule).

**Consequence: "enters the Island" is the wrong trigger for a Setup-placed permanent.** That trigger presupposes a real Aether trace and a real response window, neither of which exist during Setup. Instead, such an ability is written as **"at the beginning of your first turn"** — the same existing beginning-of-turn trigger mechanism (`overview.md` §3), just restricted to firing once, after Setup has finished and normal turn structure (and its normal response window) is running. Mid-game entries (Cast, Flicker) are unaffected and keep using "enters the Island" as normal.

## Procedure

1. **Determine the Island's shape from the Map's Layout** (D11, D52).
2. **Populate every hex with Terrain**, per rules the Map itself specifies:
   - **Home ground:** default **uniform random** from that player's own terrain deck (D53, unchanged). A Map may specify an alternative rule instead.
   - **Neutral ground: no default (D76, supersedes D53).** Every Map must explicitly specify which population algorithm it uses, plus that algorithm's own parameters, chosen from a growing, open-ended library rather than one hardcoded default. "Fixed assignment" (the Map lists exactly what sits on each neutral hex) is one such algorithm, not a privileged fallback. Other algorithms are expected — e.g. a **gradient/blend** algorithm placing terrain similar to each player's own home base near that player and blending toward the middle for a natural feel; a **clustered-distribution** algorithm scattering specific special terrain randomly within a bounded sub-region rather than uniformly across the whole map. These are illustrations of the library's shape, not adopted named mechanics.
3. **Place each Champion on its home tile** (D11) **with 4 Activation Points** (supersedes D48's asymmetric split — both Champions now enter identically; the first-move asymmetry moves to step 5 instead). Champion starts **bonded to its home tile** (`champions.md`) — current working default, **provisional**, pending `PLAN.md` §8 item 14's broader investigation into mana-generation timing; kept here only so this procedure has a complete, working answer today.
4. **Initialize Neutral permanents** — out of scope here; a direct follow-up subtask (`PLAN.md` §8 item 9(e)).
5. **Begin Player A's turn.** Their first Beginning phase runs with **Activation-Points refresh skipped** — mana-refresh and beginning-of-turn trigger-firing (including any "at the beginning of your first turn" ability) still run normally. In practice this only ever affects the Champion today: Terrain already gets full Activation Points immediately on creation regardless of Beginning-phase timing (`resources-terrain.md`), and nothing else with Activation Points exists yet. Player B's own first turn, later in the round, runs its Beginning phase completely normally — their Champion reaches full Activation Points on their own first turn, same net effect as D48's original asymmetry, reached a different way.

## Invariant vs. mutable

- **Invariant:** Champion and Terrain never occupy a Mind-domain zone, and their Setup placement is not a domain transition; Setup has no response window and cannot be observed or interfered with by either player; a Setup-placed permanent cannot use "enters the Island" as its own trigger.
- **Mutable (map-driven):** home-ground and neutral-ground population rules (algorithm + parameters); Layout; landmarks.

## Open questions

1. **Whether Champion pre-bonds to its home tile at all is provisional**, pending `PLAN.md` §8 item 14 (how mana is obtained, in general — this may also turn out to affect other mana-generating permanents/abilities, not just this one case).
2. **Neutral-permanent/scenario placement** (`PLAN.md` §8 item 15) — not designed here.
3. **Placement order (Terrain before Champion) is arbitrary**, kept only because Setup is fully non-observable and nothing currently reads D61 timestamp order between them — revisit only if something someday needs to distinguish it.
