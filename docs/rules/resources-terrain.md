# Resources & Terrain (the "lands" system)

*The signature economy: a separate terrain deck laid out on the board, connected outward from the Champion turn by turn. Goal — keep lands powerful and thought-after (MTG's best quality) while eliminating mana/color screw *except* as a deliberate opponent strategy.*

**Decisions:** D8, D22, D24, D27, D28, D47 (`history/decisions.md`)

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
- **Visibility: terrain is visible by default**, extending D7 — even an unbonded terrain sitting in a player's home zone is public information, not hidden pending connection. A terrain's *true* identity can still be hidden by a card (Mimic, reusing D18's `face` primitive — e.g. a trap terrain disguised as a basic), same mechanism as any other permanent, not a terrain-specific carve-out. Example: `docs/cards/card-ideas.md`.
- **Bond cost for non-basics: same as a basic by default** — bonding any terrain uses the single per-turn Bond action regardless of power level. A non-basic *may* print a surcharge on that specific bond (e.g. `+4!AP`) as a card effect (pillar 5), not a base-rule difference. Example: `docs/cards/card-ideas.md`.
- **Terrain abilities:** a terrain card may carry abilities beyond mana production — static buffs to occupants, movement-cost modifiers, or printed activated abilities — using the same effect/query system as any other card (pillar 5); cell properties are already queryable/mutable data (`overview.md` §2). Activated by the **player** directly (like playing any card) — terrain has no *funding* AP pool of its own, so the **default** printed cost is mana-only. A specific card may additionally print an AP cost on its activated ability, spent from the activating **payer**'s own AP (D28 — the entity actually funding it, which can be a Companion, not necessarily the terrain's strict controller) — the ordinary `mana + AP` ability shape (`overview.md` §4) extended to this case. Structure's abilities follow the identical rule — see `structures-items.md`.
- **Terrain's own Activation Capacity (D47):** independent of the funding question above, Terrain also carries a small self-gating Activation Capacity (default 1) — purely to track "has this printed ability already fired this round," reusing the standard AP-refresh mechanism instead of a bespoke once-per-turn clause on every such card. Terrain gets full Activation Capacity immediately on entering play (no summoning-sickness delay). This is **additive** to the funding model above, not a replacement — the payer above still funds the mana/AP cost; the Activation Capacity only gates re-use.
- **Terrain funding:** a terrain's activated ability is **funded** by whoever it's currently **bonded** to (Champion or Companion, D22) — this is the *payment* relationship (`ownership.md`), not necessarily the terrain's strict **controller**: a Companion-bonded terrain is funded from that Companion's private pool, but its controller (for "you control" card text) is that Companion's own Champion. An unbonded terrain has neither payer nor controller — **no one** may activate its ability. **Funding follows D22's bonder-owns-the-pool split:** a Champion-bonded terrain's activated ability is paid from the **shared** pool; a Companion-bonded terrain's is paid **only** from that **same Companion's private pool**. Funded/controlled specifically requires **bonded and active** — see the three-state model below; a **bonded-and-interrupted** terrain has neither payer nor controller for every downstream purpose too, same as fully unbonded. See `docs/cards/card-ideas.md` for examples.

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

## Severing behavior: pause; three bond states (D27)
**A Champion draws mana from every terrain it has *bonded* with that is currently connected back to the Champion via a path of terrains *not occupied by an enemy creature*.** Bonding is permanent by default (a terrain, once bonded, stays bonded unless something explicitly unbonds it — see below); *drawing* mana from it is conditional on a clear path.

**Three named bond states**, since "bonded" alone isn't precise enough once Structure control (D24) depends on it too:
- **Bonded and active** — the normal case, a live enemy-free path exists, producing mana.
- **Bonded and interrupted** — the pause case below. The bond record itself is untouched, but for **every** downstream purpose (mana, and a Structure's control, `structures-items.md`) it behaves exactly like having no parent at all. **Reverts to active automatically** the instant the path clears — no new Bond action needed. This is a **live, per-query** status (walking the path), not a stored flag.
- **Unbonded** — the link is actually gone. Needs a fresh Bond action to restore. Reachable via: the bonding **Companion dying** (reverts to unbonded, not inherited by the Champion); the Champion's **own voluntary choice** to drop one specific bond (a more granular sibling to the existing all-at-once **Collapse Network**, D9; exact cost/mechanism not designed yet); **as an additional cost** printed on a card ("unbond from target land you control" — a new cost primitive alongside mana/AP/life/discard/sacrifice); or **as a hostile targeted effect** ("target Champion or Companion unbonds from target land") — a new attack-the-economy effect archetype, distinct from the positional pause below since it doesn't need ongoing board presence to hold.

Consequences of the pause (interrupted) rule specifically:
- **Pause, not sever (reversible).** An enemy creature anywhere on the path between a bonded terrain and the Champion **pauses** all mana downstream of it (relative to the Champion). Remove/kill the blocker, or reroute along another bonded path, and the mana resumes. Denial can never become permanent screw this way — permanent severing now exists too, but only via the explicit unbonded-state triggers above, never merely from positional blocking.
- **Path-blocking, not node-blocking.** The blocker does **not** need to stand on a *producing* terrain — occupying any intermediate node on the only path pauses everything behind it. This makes chokepoints in a thin network economically decisive (pillar 2), and — since D27 — a Structure standing on interrupted terrain loses control too, opening area-control fights over contested ground.
- **Rejected:** *(b)* block only *new* connections → too weak (no real denial); *(c)* permanent downstream severing *from mere positional blocking* → too swingy (one creature deletes half an economy) — this is why permanent severing was deliberately kept to the explicit unbonded-state triggers above, not folded into the pause mechanic itself.

**Balance dial:** *how much mana can one blocker pause?* — governed by how branchy (reroutable) vs. thin (greedy) a player builds their network. Redundant paths cost board space and time; a thin reach for fancy colors is exposed. Greed = reach = risk.

*(Resolved questions are cut once closed — the rule lives in the sections above and, for decision-grade calls, in `history/decisions.md`. Only genuinely open items stay here.)*

## Open questions
1. **Color-cost model** — do card costs demand specific colored pips (MTG-style), generic + color requirements, or something else? *(Lean: colored pips — open for a fuller pros/cons discussion before locking in.)*
2. **Denial balance** *(tuning)* — how much economic damage should one blocker be able to inflict relative to its own cost, and how reroute-friendly do boards need to be by default so a single chokepoint isn't a hard lock (links to the still-open board-size question, `overview.md` §Open questions)? A numeric/playtest question, not a structural one — distinct from the terrain-*ability* design space above (what a terrain card can print), which is separately captured in `docs/cards/card-ideas.md`.

## Architecture notes
- Connectivity is a **graph query** over board state (contiguous network rooted at Champion, minus enemy-occupied nodes), re-evaluated as the board changes — deterministic, and **per-observer** if terrains are hidden. Fits the query layer (pillar 5) cleanly; non-trivial but bounded work.
- Terrain, connection, and mana production are all **effects/queries**, so cards can bend them (extra connections, connect-at-range, ignore blockers, etc.).

## Invariant vs. mutable
- **Invariant:** a separate terrain deck exists; the network is rooted at the Champion; connectivity/mana are deterministic queries.
- **Mutable (card-driven):** connection count per turn, reachability rules, what blocks/severs, what a terrain produces, color requirements.
