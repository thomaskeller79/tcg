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

## Open questions
1. ~~**Severing rule**~~ — **RESOLVED (a) pause.** Mana flows from bonded terrain only along an enemy-free path to the Champion; blocking any path node pauses downstream mana reversibly. See "Severing behavior" above.
2. **Public vs. hidden terrain** — are unconnected terrains visible to the opponent, or hidden until connected (pillar 6 bluffing)?
3. ~~**Terrain ↔ hex mapping**~~ — **RESOLVED (D11):** a terrain is a **cell property** (1 terrain = 1 hex); home-zone cell count **= terrain-deck size**; the mana network is a subgraph of the board graph.
4. ~~**Mana refresh vs. bank**~~ — **RESOLVED: refresh** (mana refills to max each Beginning phase, no banking; D21). Uncertainty about hidden spend resets each turn (D18).
5. **Do non-basics cost the connection?** Does connecting a powerful terrain use the single per-turn connection, or is it gated differently?
6. **Color-cost model** — do cards demand specific colored pips (MTG-style), generic + color requirements, or something else? Does the Champion's color identity restrict which colors a deck may run?
7. **Denial balance** — cost of a blocker vs. mana denied; how reroute-friendly must boards be (links to board-size question)?
8. ~~**Home base**~~ — **RESOLVED (D11):** defined by the **map card** — a home zone around the start, sized to the terrain deck, randomly filled at game start.

## Architecture notes
- Connectivity is a **graph query** over board state (contiguous network rooted at Champion, minus enemy-occupied nodes), re-evaluated as the board changes — deterministic, and **per-observer** if terrains are hidden. Fits the query layer (pillar 5) cleanly; non-trivial but bounded work.
- Terrain, connection, and mana production are all **effects/queries**, so cards can bend them (extra connections, connect-at-range, ignore blockers, etc.).

## Invariant vs. mutable
- **Invariant:** a separate terrain deck exists; the network is rooted at the Champion; connectivity/mana are deterministic queries.
- **Mutable (card-driven):** connection count per turn, reachability rules, what blocks/severs, what a terrain produces, color requirements.
