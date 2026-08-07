# Design — Resources & Terrain (the "lands" system)

*The signature economy: a separate terrain deck laid out on the board, connected outward from the Champion turn by turn. Goal — keep lands powerful and thought-after (MTG's best quality) while eliminating mana/color screw *except* as a deliberate opponent strategy.*

**Status:** Design note · **Date:** 2026-07-23 · Decision: D8

---

## Goals
- **Fancy, desirable lands** (MTG's most thought-after cards) that reward deckbuilding and planning.
- **No mana/color screw** from bad luck — only if the *opponent* engineers it.
- Economy that is a **live tactical object**, tied to the board and the Champion.

## The system
- **Separate terrain deck.** A legal setup requires a main deck **and** a terrain deck (and a Champion). Terrain-deck **size varies by format** — larger for long PC matches, smaller for quick mobile games.
- **Start layout (D11).** The **map card** defines each player's **home zone** around their start, sized to **exactly the terrain-deck size**. At game start a player's terrain deck is **distributed randomly** across those home-zone cells — a **terrain is a cell's property** (1 terrain = 1 hex; the terrain network is a subgraph of the board graph). Present from turn 1, not yet *bonded*. Beyond the home zone, the map's **neutral area** carries predefined/generated terrain to bond outward into.
- **Connection / bonding (the ramp).** **Up to once per turn**, a player may **bond one additional terrain** reachable from their Champion through already-bonded terrain. Bonding is **permanent**; bonded terrains form a contiguous network rooted at the Champion.
- **Mana.** A bonded terrain produces mana **only while a path of enemy-free terrain connects it back to the Champion** (see severing rule). Bonding +1/turn ≈ a guaranteed, smooth ramp curve (no flood/screw); *drawing* that mana is conditional on the path staying clear.
- **8 colors.** Eight basic lands, one per color, each producing 1 mana of that color and nothing else. **Any number of basics** allowed in a deck.
- **Non-basic terrains.** More powerful "fancy" lands, **restricted via a property on the card** (deckbuilding limits). These are what players reach for.
- **Visibility (RESOLVED 2026-08-07): terrain is visible by default**, extending D7 — even an unbonded terrain sitting in a player's home zone is public information, not hidden pending connection. A terrain's *true* identity can still be hidden by a card (Mimic, reusing D18's `face` primitive — e.g. a trap terrain disguised as a basic), same mechanism as any other permanent, not a terrain-specific carve-out. Example: `docs/cards/card-ideas.md`.
- **Bond cost for non-basics (RESOLVED 2026-08-07): same as a basic by default** — bonding any terrain uses the single per-turn Bond action regardless of power level. A non-basic *may* print a surcharge on that specific bond (e.g. `+4!AP`) as a card effect (pillar 5), not a base-rule difference. Example: `docs/cards/card-ideas.md`.
- **Terrain abilities (RESOLVED 2026-08-07): a terrain card may carry abilities beyond mana production** — static buffs to occupants, movement-cost modifiers, or printed activated abilities — using the same effect/query system as any other card (pillar 5); no new mechanism needed (cell properties are already queryable/mutable data, `rules-structure.md` §1). Activated by the **player** directly (like casting a spell) — terrain has no AP pool of its own.
- **Terrain control (RESOLVED 2026-08-07):** a player (or Companion, D22) **controls** a terrain iff it's currently **bonded into their leyline network**. An unbonded terrain (nobody has bonded it) is uncontrolled — **no one** may activate its ability. **Funding follows D22's bonder-owns-the-pool split:** a Champion-bonded terrain's activated ability is paid from the **shared** pool; a Companion-bonded terrain's is paid **only** from that **same Companion's private pool** — never the shared pool, never another Companion's. Not a new mechanism — this is D22's "mana never crosses the shared/private boundary" rule applied to terrain abilities. *(Scoped to activated abilities specifically; whether a terrain's static/continuous effects — e.g. a combat buff to defenders — also require control, or apply to any occupant regardless of owner like a natural battlefield feature, is a separate, not-yet-asked question.)* See `docs/cards/card-ideas.md` for examples.

## Why it avoids screw
- The Champion starts with ~6 neighbor hexes; the connectable **frontier grows for many turns**, so early color/mana screw is unlikely (though enough spatial variance that running all 8 colors is impractical — a good tension).
- Access to the terrain deck is **deterministic** (you connect what you have), not draw-dependent. Late-game you can almost always connect *something*.
- **Screw only happens if the opponent forces it** by blocking connection paths — a positional action with counterplay, not random variance.

## The greed / risk curve
Chasing many colors or powerful non-basics may require connecting **outward in a thin, non-circular line** rather than safely around the Champion. A thin network is **exposed**: the opponent can occupy a node to **put the connection on hold** (see severing rule). Greed = reach = risk. This is the MTG "powerful lands demand commitment" feel, recast spatially.

## Emergent strengths (why this is more than an anti-screw fix)
- **Champion = economic root *and* win anchor.** Advancing the Champion for offense endangers the mana base; every Champion move is an economic decision. (Also answers: *is the Champion the resource engine?* → **yes**.)
- **Denial is positional, not random.** Mana attack = a creature you position, giving board presence economic purpose and making chokepoints economically meaningful (pillar 2). The victim has counterplay: kill the blocker or reroute.
- **Mana bluffing (pillar 6).** If terrains are hidden until connected, opponents can't fully plan denial and players can bluff their development.

## Severing behavior — RESOLVED (a) pause
**A Champion draws mana from every terrain it has *bonded* with that is currently connected back to the Champion via a path of terrains *not occupied by an enemy creature*.** Bonding is permanent (a terrain, once bonded, stays bonded); *drawing* mana from it is conditional on a clear path.

Consequences of the confirmed rule:
- **Pause, not sever (reversible).** An enemy creature anywhere on the path between a bonded terrain and the Champion **pauses** all mana downstream of it (relative to the Champion). Remove/kill the blocker, or reroute along another bonded path, and the mana resumes. Denial can never become permanent screw.
- **Path-blocking, not node-blocking.** The blocker does **not** need to stand on a *producing* terrain — occupying any intermediate node on the only path pauses everything behind it. This makes chokepoints in a thin network economically decisive (pillar 2).
- **Rejected:** *(b)* block only *new* connections → too weak (no real denial); *(c)* permanent downstream severing → too swingy (one creature deletes half an economy).

**Balance dial:** *how much mana can one blocker pause?* — governed by how branchy (reroutable) vs. thin (greedy) a player builds their network. Redundant paths cost board space and time; a thin reach for fancy colors is exposed. Greed = reach = risk.

*(Resolved questions are cut once closed — the rule lives in the sections above and, for decision-grade calls, in `decisions.md`. Only genuinely open items stay here.)*

## Open questions
1. **Color-cost model** — do card costs demand specific colored pips (MTG-style), generic + color requirements, or something else? *(Lean: colored pips — open for a fuller pros/cons discussion before locking in.)*
2. **Denial balance** *(tuning)* — how much economic damage should one blocker be able to inflict relative to its own cost, and how reroute-friendly do boards need to be by default so a single chokepoint isn't a hard lock (links to the still-open board-size question, `rules-structure.md` §Open questions)? A numeric/playtest question, not a structural one — distinct from the terrain-*ability* design space above (what a terrain card can print), which is separately captured in `docs/cards/card-ideas.md`.

## Architecture notes
- Connectivity is a **graph query** over board state (contiguous network rooted at Champion, minus enemy-occupied nodes), re-evaluated as the board changes — deterministic, and **per-observer** if terrains are hidden. Fits the query layer (pillar 5) cleanly; non-trivial but bounded work.
- Terrain, connection, and mana production are all **effects/queries**, so cards can bend them (extra connections, connect-at-range, ignore blockers, etc.).

## Invariant vs. mutable
- **Invariant:** a separate terrain deck exists; the network is rooted at the Champion; connectivity/mana are deterministic queries.
- **Mutable (card-driven):** connection count per turn, reachability rules, what blocks/severs, what a terrain produces, color requirements.
