# Resources & Terrain (the "lands" system)

*The signature economy: a separate terrain deck laid out on the board, connected outward from the Champion turn by turn. Goal — keep lands powerful and thought-after (MTG's best quality) while eliminating mana/color screw *except* as a deliberate opponent strategy.*

**Decisions:** D8, D11, D22, D24, D27, D28, D47, D51, D54, D57, D58, D77, D87, D88, D100, D125 (`history/decisions.md`)

---

## Goals
- **Fancy, desirable lands** (MTG's most thought-after cards) that reward deckbuilding and planning.
- **No mana/color screw** from bad luck — only if the *opponent* engineers it.
- Economy that is a **live tactical object**, tied to the board and the Champion.

## The system
- **Separate terrain deck.** A legal setup requires a main deck **and** a terrain deck (and a Champion). Terrain-deck **size varies by format** — larger for long PC matches, smaller for quick mobile games.
- **Start layout (D11).** The **map card** defines each Champion's **home ground** around its start, sized to **exactly the terrain-deck size**; by default, a Champion's terrain deck is **distributed randomly** across those home-ground cells (a map may specify a different population rule, D53) — a **terrain is a cell's property** (1 terrain = 1 hex; the terrain network is a subgraph of the board graph). Present from turn 1, not yet *bonded*. Beyond the home ground, the map's **neutral ground** carries terrain to bond outward into. Each hex also carries a map-authored **Terrain Type**, independent of a card's **Element** — see `map.md`.
- **Connection / bonding (the ramp).** **Up to once per turn**, the Champion may **bond one additional terrain** reachable from itself through already-bonded terrain — an unbonded terrain, or an enemy terrain paused by a knot (theft, D122, see severing rule). A bond lasts until something removes it; bonded terrains form a contiguous network rooted at the Champion.
- **Mana.** A bonded terrain produces mana **only while a path of unknotted terrain connects it back to the Champion** (see severing rule). Bonding +1/turn ≈ a guaranteed, smooth ramp curve (no flood/screw); *drawing* that mana is conditional on the path staying clear. **Accounting (D77):** each mana-producing permanent carries its own **drawn-this-cycle flag**, not a stored amount — the instant it's reachable and still undrawn, its production credits directly into its bonder's pool (the Champion's, or a Companion's own pool, below) and the flag flips, once, until the owner's next Beginning phase resets both the flag and the pool. A drawn permanent renders its leyline connection **green**; a currently-blocked, still-undrawn one renders **red** — there's no third state. Credit, once given, is never revoked by a later block — severing only ever denies a permanent's *next* credit, reversible pause or hard unbond alike.
- **8 colors, named Elements (D51):** **Light, Fire, Metal, Earth, Darkness, Ice, Water, Air.** Eight basic lands, one per Element, each producing 1 mana of that Element and nothing else. **Any number of basics** allowed in a deck.
- **Non-basic terrains.** More powerful "fancy" lands, **restricted via a property on the card** (deckbuilding limits). These are what players reach for.
- **Visibility: terrain is visible by default**, extending D7 — even an unbonded terrain sitting in a Champion's home ground is public information, not hidden pending connection. A terrain's *true* identity can still be hidden by a card (Mimic, reusing D18's `face` primitive — e.g. a trap terrain disguised as a basic), same mechanism as any other permanent, not a terrain-specific carve-out. Example: `docs/cards/card-ideas.md`.
- **Bond cost for non-basics: same as a basic by default** — bonding any terrain uses the single per-turn Bond action regardless of power level. A non-basic *may* print a surcharge on that specific bond as a static ability ("bonding this costs an additional `4!AP`", pillar 5, D100) — part of the Bond action's cost, not a base-rule difference. Example: `docs/cards/card-ideas.md`.
- **Terrain abilities (D87):** a terrain card may carry **static** and **triggered** abilities beyond mana production — static buffs to occupants, movement-cost modifiers, "whenever a creature enters this…" triggers — using the same effect/query system as any other card (pillar 5); cell properties are already queryable/mutable data (`overview.md` §2). **Terrain has no activated abilities and holds no Activation Points.** A terrain card that wants activated power brings a **Structure** with it (a real card, D88), which carries that ability under the ordinary Structure rules (`structures-items.md`) and can be attacked and felled, leaving the plain terrain behind. How that Structure arrives at Setup is open (Open question 3).
- **Terrain funding:** a cost printed on a terrain's triggered ability ("…you may pay 1 AP; if you do…") is paid by whoever the terrain is currently **bonded** to (Champion or Companion, D22), mana and Activation Points alike — the ordinary Payment walk (`ancestry.md`), which climbs past the terrain because it holds neither resource. This is the *payment* relationship, not necessarily the terrain's strict **controller**: a Companion-bonded terrain's costs come from that Companion's own pool and AP, but its controller (for "you" and "you control" card text) is that Companion's own Champion. A Neutral terrain (unbonded, or bonded-but-path-blocked, D54) has neither payer nor controller: it is **dormant** (`neutral-permanents.md`) — its triggered abilities that need a choice or a payment don't happen, while its static abilities and choiceless triggers still apply (an unbonded swamp on neutral ground slows everyone). A once-per-round limit on a trigger is printed on the card where wanted. See `docs/cards/card-ideas.md` for examples.
- **Readability (D87):** the GUI marks only terrain that has static or triggered abilities, so basics stay visually quiet, and shows a static's effect where it lands (movement preview, affected creature's stats). The mark follows each observer's belief: a Mimic terrain presenting as a basic shows no mark to an observer who believes it basic.

## Mana costs and payment (D125)
- **Cost pips.** A mana cost is a list of pips of two kinds: **generic** (any mana pays it) and **colored** — a disjunction over Elements, paid by mana of any one of them. One Element is ordinary colored mana, two is hybrid, and so on.
- **Pool mana.** A mana in a pool is **colorless** or **colored** — again a disjunction over Elements, spendable as any one of them. Colorless pays only generic pips. A terrain's card says what it produces.
- **Domination.** One mana dominates another if it pays every pip the other pays — a colored mana dominates a mana whose Elements it includes, and every mana dominates colorless.
- **Semi-automatic payment.** Pips are paid from the most constrained to the least: one-Element pips first, then two-Element, …, generic last. Each pip takes the mana that every other mana able to pay it dominates; when no such mana exists, the paying player chooses among the tied ones. Identical mana is never a choice. Consequences: colorless always pays generic; a one-Element pool pays everything automatically; with two Elements, the only choice is which one pays a generic pip.

## Why it avoids screw
- The Champion starts with ~6 neighbor hexes; the connectable **frontier grows for many turns**, so early color/mana screw is unlikely (though enough spatial variance that running all 8 colors is impractical — a good tension).
- Access to the terrain deck is **deterministic** (you connect what you have), not draw-dependent. Late-game you can almost always connect *something*.
- **Screw only happens if the opponent forces it** by blocking connection paths — a positional action with counterplay, not random variance.

## The greed / risk curve
Chasing many colors or powerful non-basics may require connecting **outward in a thin, non-circular line** rather than safely around the Champion. A thin network is **exposed**: the opponent can occupy a node to **put the connection on hold**, and an opponent whose network reaches the paused branch can steal it (see severing rule). Greed = reach = risk. This is the MTG "powerful lands demand commitment" feel, recast spatially.

## Emergent strengths (why this is more than an anti-screw fix)
- **Champion = economic root *and* win anchor.** Advancing the Champion for offense endangers the mana base; every Champion move is an economic decision. (Also answers: *is the Champion the resource engine?* → **yes**.)
- **Denial is positional, not random.** Mana attack = a creature with Knotting you position, giving board presence economic purpose and making chokepoints economically meaningful (pillar 2). The victim has counterplay: kill the blocker or reroute.
- **Mana bluffing (pillar 6), card-driven only.** Terrain is public, so a network can't be bluffed by default; a card disguising a terrain's identity (Mimic) is the one source of it.

## Severing behavior: pause, or an outright unbond (D8, D54)
**A Champion draws mana from every terrain it has *bonded* with that is currently connected back to the Champion via a path of *unknotted* terrains.** A terrain, once bonded, stays bonded until something removes the bond — see below; *drawing* mana from it is conditional on a clear path.

Terrain's **controller** is just the general three-valued result (Champion A / Champion B / Neutral, D54) applied to this connectivity check — see `ancestry.md`'s "Terrain's controller vs. its bond record" for the full framing. What matters for the economy specifically is *why* a terrain is currently Neutral, since that governs how it comes back:
- **Bond record intact, path currently blocked** — the pause case below. Reverts to producing mana automatically the instant the path clears, provided this cycle's credit hasn't already been drawn (D77's per-permanent flag) — connectivity itself is still checked **live, per query** (walking the path), never cached.
- **No bond record at all (unbonded)** — the link is actually gone. Needs a fresh Bond action to restore. Reachable via the bonding **Companion ceasing to exist** (reverts to unbonded, not inherited by the Champion), a **cut**, or **Collapse Network** (below, `champions.md`).

**Knotting (D121).** A terrain is **knotted** for a side while an enemy permanent with the keyword **Knotting** stands on it, in any Slice. Nothing else knots a terrain: a permanent without Knotting never blocks a path, whatever its type. A Neutral permanent with Knotting is an enemy to both sides, so it knots for both. Knotting has no special case for concealment: a hidden permanent with Knotting still knots, and the paused leylines tell the opponent roughly where it stands — a cost its controller accepts. Knotting is a common keyword, the way Flying is, and appears more often in some Elements than others.

**Paths and cuts (D122).** A bonded terrain's path runs through terrain bonded by the same root, back to the root's current tile — which must itself be bonded by that root. A knot doesn't break this connection, it only stops the flow:
- **Knotted** — the terrain stays connected and keeps its bond record; it is paused (Neutral, no mana) until the knot is gone.
- **Cut** — a bonded terrain with no such path at all, even counting knotted terrain, loses its bond record immediately and is unbonded. Anything that removes a bond in the middle of a network cuts everything behind it: an unbond effect, or a theft (below). An unbond effect on the root's own tile cuts the whole network.

**Theft (D122).** Terrain paused by an enemy knot — the knotted terrain and every terrain behind it — can be bonded by the opponent with an ordinary Bond, if it is reachable from their network. Their bond replaces the old record. Stealing a chokepoint therefore cuts everything behind it from its old owner, leaving it unbonded and free to bond: a narrow branch reaching toward the enemy is fast but exposed.

Consequences of the pause rule specifically:
- **Pause, not sever (reversible).** An enemy permanent with Knotting anywhere on the path between a bonded terrain and the Champion **pauses** all mana downstream of it (relative to the Champion). Remove/kill the blocker, or reroute along another bonded path, and the mana resumes. A knot alone never removes a bond; turning it into a permanent loss takes the opponent's own Bond action (theft, above).
- **Path-blocking, not node-blocking.** The blocker does **not** need to stand on a *producing* terrain — occupying any intermediate node on the only path pauses everything behind it. This makes chokepoints in a thin network economically decisive (pillar 2), and a Structure standing on a paused (Neutral) terrain loses control too, opening area-control fights over contested ground.
- **Rejected:** *(b)* block only *new* connections → too weak (no real denial); *(c)* permanent downstream severing *from mere positional blocking* → too swingy (one creature deletes half an economy) — a knot alone only pauses; a permanent loss needs a theft, which costs the opponent a Bond and a network that reaches the branch.

**Balance dial:** *how much mana can one blocker pause?* — governed by how branchy (reroutable) vs. thin (greedy) a player builds their network. Redundant paths cost board space and time; a thin reach for fancy colors is exposed. Greed = reach = risk.

*(Resolved questions are cut once closed — the rule lives in the sections above and, for decision-grade calls, in `history/decisions.md`. Only genuinely open items stay here.)*

## Open questions
1. **Generic mana** — the cost model above (D125) supports generic pips; whether any card should carry them is open (dropping generic would make nearly every payment automatic). Tracked at `PLAN.md` Track A item 37.
2. **Denial balance** *(tuning)* — how much economic damage should one blocker be able to inflict relative to its own cost, and how reroute-friendly do boards need to be by default so a single chokepoint isn't a hard lock (links to the still-open board-size question, `overview.md` §Open questions)? A numeric/playtest question, not a structural one — distinct from the terrain-*ability* design space above (what a terrain card can print), which is separately captured in `docs/cards/card-ideas.md`.
3. **How a terrain's Structure arrives at Setup (D87).** Setup-placed permanents enter the Island like any other (D114, `setup.md`), so a terrain can bring its Structure with an ordinary "when this enters the Island, create …" trigger, resolved in Setup's trigger step; the Structure enters with 0 Activation Points unless it has Haste. Still open: which slot it takes and its creation triple (`object-properties.md` Open questions).

## Architecture notes
- Connectivity is a **graph query** over board state (contiguous network rooted at Champion, minus knotted nodes), re-evaluated as the board changes — deterministic, and **per-observer** if terrains are hidden. Fits the query layer (pillar 5) cleanly; non-trivial but bounded work.
- Terrain, connection, and mana production are all **effects/queries**, so cards can bend them (extra connections, connect-at-range, ignore blockers, etc.).

## Invariant vs. mutable
- **Invariant:** a separate terrain deck exists; the network is rooted at the Champion; connectivity/mana are deterministic queries; Terrain holds no Activation Points and has no activated abilities — activated power comes from a Structure (D87).
- **Mutable (card-driven):** connection count per turn, reachability rules, what blocks/severs, what a terrain produces, color requirements.
