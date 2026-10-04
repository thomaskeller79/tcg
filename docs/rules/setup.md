# Match Setup

*How a legal match's four components (card deck, terrain deck, Champion, Map) combine into the actual starting board state — and why this isn't a domain transition, even though Champion and Terrain are printed and collected like any other card.*

**Decisions:** D76, D77, D87, D114 (`history/decisions.md`)

---

## Setup is not a domain transition

Champion and Terrain are printed and collected like any other card, but never occupy Hand/Library/Discard, ever (`object-properties.md` §1) — they are a different kind of game object from Creature/Spell/Structure/Item/Companion despite sharing the word "card" for the deckbuilding-tier concept (a real naming collision, tracked at `PLAN.md` §8 item 6, deliberately not resolved here). Because they never pass through a Mind-domain zone, their arrival on the Island at Setup is not a Mind→Matter transition at all — there is no Card object for them to skip the Aether from. Setup is a distinct, more primitive kind of event: it reads directly from a player's collection and creates Permanents, with no Card or Trace ever created for Champion or Terrain specifically.

## Setup is Champion A's first Beginning phase (D114)

Setup takes the place of Champion A's turn-1 Beginning phase: Champion A is the active seat, and the procedure below replaces that phase's refresh. **Setup opens no priority** — nobody can cast, activate or respond while it runs, including to the triggers it fires. Players can observe it, and make the mulligan decisions it asks for.

**Setup-placed permanents enter the Island** under the ordinary rules: they enter with 0 Activation Points unless they have Haste (`economy.md`), and their "enters the Island" triggers fire as usual, resolving in the trigger step (S9).

## Procedure

- **S1. Island shape** from the Map's Layout (D11, D52).
- **S2. Deal terrain** face up onto the hexes. Dealt terrain is not on the Island yet and fires nothing.
  - **Home ground** is dealt for each player who hasn't decided to keep yet. Default: **uniform random** from that Champion's own terrain deck (D53); the Map, the Champion or another rule may change it.
  - **Neutral ground** is dealt on the first pass only. **No default (D76):** every Map must explicitly specify which population algorithm it uses, plus that algorithm's own parameters, chosen from a growing, open-ended library rather than one hardcoded default. "Fixed assignment" (the Map lists exactly what sits on each neutral hex) is one such algorithm, not a privileged fallback. Other algorithms are expected — e.g. a **gradient/blend** algorithm placing terrain similar to each Champion's own home base near that Champion and blending toward the middle for a natural feel; a **clustered-distribution** algorithm scattering specific special terrain randomly within a bounded sub-region rather than uniformly across the whole map. These are illustrations of the library's shape, not adopted named mechanics.
- **S3. Terrain mulligan** (home ground only). Each player who hasn't kept yet either keeps or takes a mulligan, at a cost. If anyone takes one, go back to S2.
- **S4. All terrain enters the Island.**
- **S5. Each Champion enters on its home tile** (D11). **Each Champion then bonds its home tile** — an instruction, not the Champion's Bond ability: no cost, no trace, and it doesn't use the once-per-turn Bond. Mana credits immediately (D77). **Each Champion's Activation Points become the higher of its current Activation Points and 4** (placeholder number; a Champion with Haste may already have more). The pre-bond is kept deliberately (D77): it speeds up the opening, and it taxes any strategy that wants to rush the Champion out of its own realm (Collapse Network required first).
- **S6. Shuffle and draw.** Each player who hasn't kept yet shuffles their Library and draws an opening hand (size TBD, strawman 5 — `overview.md` §6).
- **S7. Hand mulligan.** Each player who hasn't kept yet either keeps or takes a mulligan, at a cost. If anyone takes one, go back to S6.
- **S8. Neutral permanents** placed by the Map enter — placement not yet designed (`PLAN.md` §8 item 7).
- **S9. Trigger step.** Every trigger fired during S1–S8 enters Pending as one batch, in the automatic order of `interaction-stack.md` §Resolution (seats APNAP from Champion A, source ID, printed ability order, event order), and all of them resolve without priority.

Then Champion A's Action phase begins, with the match's first priority window. Champion B's first turn, later in the round, runs its Beginning phase completely normally and refreshes to full Activation Points. Champion A gets no refresh on turn 1, because its first Beginning phase is Setup — this is the first-move-advantage asymmetry between the two Champions.

## Invariant vs. mutable

- **Invariant:** Champion and Terrain never occupy a Mind-domain zone, and their Setup placement is not a domain transition; Setup is Champion A's first Beginning phase and opens no priority; Setup-placed permanents enter the Island under the ordinary rules.
- **Mutable (map-driven):** home-ground and neutral-ground population rules (algorithm + parameters); Layout; landmarks.

## Open questions

1. **Neutral-permanent/scenario placement** (`PLAN.md` §8 item 7) — not designed here.
2. **Mulligan rules — to be settled by playtesting.** Every mulligan must have a cost, possibly increasing, so the loops end. Candidate currencies: the starting mana (1), the opening hand size (5), the starting Activation Points (4). A cost in mana or Activation Points exists only from S5 on, so a terrain mulligan's cost would be recorded at S3 and charged at S5. Also open: whether players decide simultaneously or in turn order (deciding in turn order shows the later player what the earlier one did).
3. **S8's position relative to S6/S7** — players currently decide on their hand without seeing the Map's Neutral permanents; may move after playtesting.
