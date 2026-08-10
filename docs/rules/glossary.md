# Glossary

*Shared vocabulary. Keep terms consistent everywhere — code, docs, cards. Seeded by Claude; correct freely.*

**Status:** Draft (seed) · **Date:** 2026-07-23

| Term | Meaning |
|---|---|
| **Match** | One complete game between players, ending when a win condition is met. |
| **Turn** | One player's full sequence of phases. Players alternate turns. |
| **Phase** | An ordered segment of a turn (e.g. Start, Draw, Action, End). The phase list is **data**, so cards can add/skip phases. |
| **Board** | The hex grid where the match is played, produced from a **Map** card (D11). A set of hex **cells** keyed by cube/axial coordinate. |
| **Hex / Cell** | A single tile. Has queryable properties (move-cost, terrain, per-layer occupants) and **three vertical layers** (D12). |
| **Map** | A **card** (a fourth match component alongside card deck, terrain deck, Champion) defining the board: form, start positions, home zones, neutral area, landmarks. Comes in **sizes**; map size = terrain-deck size (D11). |
| **Layer** | One of a hex's three vertical occupancy spaces — **ground / above (flying) / below (submerged)** — each capacity 3 (D12). The below layer is hidden by default. |
| **Void / Aether** | The terrain type filling a map's "holes" — present as data with special functionality, not a missing cell (D11). |
| **Home zone** | The map-defined region around a player's start, sized to the terrain deck, randomly filled with their terrain at game start (D11; = the D8 "home base"). |
| **Neutral area** | Map region outside the home zones, terrain set by a predefined generation procedure; the space players bond outward into (D11). |
| **Landmark** | A map hex with special rules (e.g., first creature to enter → its Champion draws) (D11). |
| **Zone** | A place a card can be: **Hand, Library, Play, Aether** (D16). A card may occupy **multiple zones at once** (digital-only). |
| **Library** | A player's draw deck (formerly "Deck"). |
| **Play** | The zone of permanents on the Board (creatures, buildings, items). A card "in Play" occupies the Board. |
| **Aether** | A per-observer **timeline** zone: past **traces** behind a **`now` marker**, the **stack** (LIFO, pending) growing in front of it. Unifies stack + spell-graveyard + history. Traces **fade** over a configurable window. |
| **Trace** | An Aether record left by a resolved spell/ability. A cast Play-permanent also leaves an **anchored summon/build trace** linked to it. Traces fade; interacting with a trace (e.g. **un-summon**) works only before it fades. |
| **Now marker** | The present, dividing the Aether's past **traces** from the future-facing **stack**. Distance along the timeline is mechanical (e.g. push a trace forward to re-cast). |
| **Grave** | A dead permanent's marker left **in Play on its hex** — no slot consumed, view-toggleable. Replaces the graveyard for creatures/buildings (D16). |
| **Un-summon** | Removing a permanent by deleting its anchored Aether trace — a **rewind** (no death, no grave, no death-triggers), distinct from **kill**; only works while the trace hasn't faded. |
| **Home tile** | The Champion's starting board location (usually a landmark terrain). Not a separate objective — there is **no Base** (D9). |
| **Unit / Creature** | A card summoned onto the Board, defined by **three numbers: Attack / Life / Action Points** (D10), plus abilities/keywords. |
| **Life (persistent)** | A creature's health. **Damage persists between turns — no automatic healing** (D14); healing is a special ability; 0 Life → destroyed. |
| **Retaliation** | Damage a declared defender deals **back** to the attacker; combat is **mutual by default** (D13). Ranged creatures take no retaliation (keyword). |
| **Defend** | Declaring a creature (or Champion) as a defender of an attacked hex (D4). Costs `0*AP` (D15, resolved 2026-08-09) — free, but at most once per turn per actor, regardless of remaining AP. |
| **Pessimistic default** | Design principle (D14): a rule's default is the weakest still-functional-and-fun case; cards improve it via **positive** lines, not default restrictions. |
| **Spell** | **Umbrella term** (D17): anything cast through the Aether (creatures, structures, items, rites). |
| **Rite** | The one-shot spell type (D17): resolves to an Aether **trace**, leaves no permanent. |
| **Parent** | The ownership-tree link (D26/D27): the entity that produced, bonded, or currently carries a permanent. **Not every permanent has one** (unbonded terrain, a loose Item) — a creature never parents another creature or structure (D27). A **Companion is never itself a root** — it always has a live Champion parent. See `ownership.md`. |
| **Payment (walk)** | To fund a cost, climb a permanent's parent chain and stop at the **first** node that holds the needed resource — AP/Life stop immediately (self); mana can stop at a Companion (D28). A shorter walk than finding the **controller** — not the same operation. |
| **Controller** | Climb a permanent's parent chain **all the way to the top**. Since a Companion is never a root, this always resolves to a **Champion** if it resolves at all. **If the root is a Champion, that Champion is the controller** (D28); if a Companion mid-chain has died, the climb doesn't reach one, so there is no controller — a direct consequence of the definition, not a separate ruling (D29). **Open:** what an uncontrolled-but-still-living creature actually does (Terrain/Structure are simply inert when uncontrolled; a creature still has its own AP) — see `ownership.md`. |
| **Structure** | Stationary, **non-carryable** Play-permanent (D17); subtypes e.g. *Building*. Absorbs non-equippable "artifacts." Location is (terrain, layer) like a Creature, but in its own dedicated slot, capacity **1 per layer** (up to 3 per hex, D31) — separate from creature layer capacity. Has Life, attackable (D24); its parent is the terrain it occupies, so its ability is **funded** by whoever currently bonds that cell while active (Champion or Companion) — D24, D27 — and its **controller** (D28) is always that bonder's own Champion. See `structures-items.md`. |
| **Item** | **Carryable** Play-permanent (D17): picked up via a generic `2AP: Equip` ability every eligible actor holds, dropped via its free `0AP: Un-equip` inverse (D30); unequipped, it has no usable ability at all. Location while loose is (terrain, layer), capacity-exempt (D32, D34); while equipped, its location is its carrying actor, not spatial. Has no Life — never a combat participant, removed only via an explicit effect (D34). Its parent (and payer) is whoever equipped it; becomes ownerless again if its carrier dies or un-equips it (D25, D27, D30). See `structures-items.md`. |
| **Summon** | Playing a creature onto a **bonded terrain in your realm** for its mana cost (D20). Summoning-sick by default (D14). |
| **Mana** | The **single shared pool per player**, filled by channeling bonded terrain (D8). Spent on spells, summons, and any actor's mana-abilities. Global. |
| **Action Points (AP)** | A **private per-turn budget** (D10), held by each **creature** and, as of the 2026-08-06 revision, by the **Champion** too (D9) — same mechanism, not an analog. Creatures spend it by default on `1AP: Move` and `3!AP: Attack`; the Champion's own action costs are bespoke (see `champions.md`). Refills each turn, no carryover. Subsumes the old Movement stat. |
| **`!` cost (`x!AP`)** | A cost flavor: **require x AP, then consume all remaining AP** (vs. plain `xAP` which spends exactly x and leaves the rest). Makes "no multi-attack" emergent — default attack is `3!AP` (D10). |
| **`*` cost (`x*AP`)** | *(Adopted 2026-08-06 for the Champion; generalized to Defend 2026-08-09, D15.)* A cost flavor: **spend exactly x AP; the action may be used at most once per turn**, regardless of leftover or later-refilled AP — unlike `!`, it does **not** drain the rest of the pool. Used for the Champion's `5*AP: Draw` / `2*AP: Bond` actions so both stay usable in the same turn while each staying capped once/turn, and for **every actor's** `0*AP: Defend` (D15) — free, but once per turn, and (at `x=0`) completely decoupled from remaining AP. See `economy.md`, `champions.md`. |
| **Default ability** | An ability every creature has unless replaced. The set: `1AP: Move`, `3!AP: Attack`, `0*AP: Defend` (D15), and — for any eligible actor (Champion/Companion/Creature) — `2AP: Equip` / `0AP: Un-equip` (D26, D30). Base rules expressed as replaceable abilities (pillar 5), not hardcoded logic. **Zone of Control is deliberately not on this list** (D30) — a positive keyword only, per the pessimistic-default principle. |
| **Effect** | A discrete, structured change to game state. **Base rules and card text are both expressed as effects** (pillar 5). |
| **Modifier** | A continuous effect that changes the answer to a query (e.g. "+1 Movement"). |
| **Keyword** | A named, reusable ability/property from the keyword library (e.g. Flying, **Ranged N** — range is a keyword, not a core stat, D10). |
| **Query** | A question the engine asks to get a current value (cost, movement, legality). Never read a raw stat — always query. |
| **Event** | An applied state change that flows through the pipeline where cards can intercept it. |
| **True State** | The single authoritative, deterministic game state held by the engine/server. |
| **View** | What one player perceives — a projection of True State through the perception layer. Two players' views may disagree. |
| **Perception** | The query axis answering, per observer: is this visible? what does it look like? what is known? Modified by cards (Mimic, Mist, Submerged). |
| **Mimic / Submerged / Mist** | Example asymmetric-info mechanics: a unit that looks like something else / a unit hidden until detected / a region only one player can see into. See `asymmetric-information.md`. |
| **Claim** | A card-authored, possibly-false projection into an observer's view (soft channel). Holds until a **hard fact** contradicts it; then it **collapses** (D18). |
| **Hard fact / Hard channel** | Information the engine always projects truthfully (mana network, hand size, AP, positions, deaths, combat outcomes) — what falsifies claims. Opposite: **soft channel** (identity/appearance/hidden-zone contents), which cards may fake. |
| **Collapse** | When a claim is falsified by a hard fact, the observer learns the **full truth** (default), incl. which of a masked pool it was (D18). |
| **Conservation law** | One of a small, finite set of engine-enforced checks (e.g. claimed mana ≤ public max) that auto-collapse mechanically-impossible claims. The engine does **no** other deduction — humans do (D18). |
| **Live mana ledger** | The hidden running record of mana actually spent — the one standing quantity hidden by default (network is public), enabling cost-deception (D18). |
| **Face** | The atomic deception primitive (D18): a **per-observer overlay** (claimed identity + stat modifiers) a **permanent** carries over its true state. A creature can *enter with* a face; a spell can *apply* one. Governed by the belief model (collapses on a hard fact). |
| **Mimic (effect) / ChooseOne** | Effect combinators (D18): `Mimic(face)` **applies a `face`** to a permanent (and shows it on the trace); `ChooseOne[…]` is a modal effect the caster picks privately (→ true state). |
| **Champion** | *(Provisional term; "Channeler" leans in.)* A player's single on-board avatar — the only entity that draws mana from the land **and shares it**. It is the resource-network **root** (D8), the **win condition** (kill it to win, D9), and an attackable piece. Runs mana + its own **AP** like a creature (D9, revised 2026-08-06) — draw a card, bond a terrain, move, fight, collapse its network, activate abilities. Evolves between matches along paths. See `champions.md`. |
| **Collapse Network** | *(D9, under test, added 2026-08-08.)* A Champion's `0AP` ability: drops every one of its bonds outright (not a reversible pause — they're gone, re-bond from scratch). The escape valve for the **Realm** lock below — the only way to move onto unbonded ground while a network exists. |
| **Companion** | *(D22.)* A new card type between Creature and Champion: same three stats (Attack/Life/AP) as a Creature, but can **bond terrain like a channeler** — its draw stays in a **private mana pool** spendable only on its own printed abilities, never shared. Deckbuilding-gated to specific Champion(s). Cast like a Creature (D20); acts as a **second network root** alongside the Champion (D8). See `companions.md`. |
| **Realm** | The contiguous set of terrains the Champion has bonded. *(Under test, tightened 2026-08-08, D9):* while connected, the Champion is **confined** to its realm — it may reposition onto any hex it has bonded (`2AP`) but may not step onto unbonded ground; leaving requires **Collapse Network** (`0AP`, drops every bond outright), after which it moves freely (`1AP`) like any creature. |
| **Loadout** | A Champion's fully-resolved configuration for one match (level, chosen path, unlocked abilities), produced by the meta-layer and consumed by the match core. |
| **Path** | A branch of a Champion's between-match progression tree; committing to a path gives a Champion a distinct identity. |
| **Meta-layer** | Systems *outside* the deterministic match core that persist across matches (Champion progression, unlocks, collection). |
| **Band** | A level range (e.g. 1–5) that defines a match/tournament format. Champions only face others in the same band. Power is horizontal within a band, stepping up between bands. |
| **Ground slot** | One of the 3 creature positions on a hex's **ground** layer. Each of the three layers (ground/above/below, D12) holds up to 3 creatures. |
| **Combat** | An explicit fight object: `{attacking units, target hex, declared defenders}` that resolves. Scheduled sequentially by default; potentially phased later. |
| **Stack** | The LIFO region of the **Aether** in front of the `now` marker, where spells/abilities/responses wait and resolve top-down (D16). See `interaction-stack.md`. |
| **Priority** | The right to act at a defined window; how players respond during the opponent's turn. |
| **Instant-speed / Sorcery-speed** | Card-level split (D5): a card playable in any priority window (reactive) vs. only during your own turn's Action phase (most default cards). |
| **`^` marker (`^xAP`)** | *(D23.)* Generalizes instant/sorcery-speed to the **ability** level: `^` prefixed on an AP cost makes that one ability instant-speed (usable in any priority window, either player's turn). Default (no `^`) is sorcery-speed — own Action phase only, and only while the stack is empty. Composes freely with `!`/`*` (e.g. `^2*AP`) — independent axes. |
| **Trap** | A pre-committed, hidden triggered ability (pillar 6) that goes on the stack when its condition fires. |
| **Terrain deck** | A separate deck (distinct from the main deck) of terrain/land cards, laid out in the home base at game start. Size varies by format. |
| **Terrain** | A land card occupying board space that produces mana when connected. Basic (8 colors, unrestricted) or non-basic (restricted, more powerful). |
| **Connection / Bond** | The AP-costed act (`x*AP`, at most once/turn) of adding one terrain to a network **rooted at the Champion or a Companion** (D22), via already-bonded terrain reachable from that root. |
| **Bonded** | A terrain added to a player's network by whichever root performed the bond. **Three states (D27):** **active** (a live enemy-free path to its root, producing mana); **interrupted** (D8's pause — an enemy sits on the path; behaves as uncontrolled for every purpose, including a Structure built on it, D24; reverts to active automatically once the path clears); **unbonded** (the link is actually gone — via the bonding Companion dying, D22; the Champion's own voluntary choice; an additional cost printed on a card; or a hostile targeted effect — needs a fresh Bond to restore). Feeds the shared pool if Champion-bonded, or that Companion's private pool if Companion-bonded (D22). |
| **Color** | One of 8 mana types. Basic lands each produce one color. |
| **Home base** | Synonym for **Home zone** (D11) — kept for continuity with D8. Where the realm begins; not a special/destructible place. |
| **On hold** | A bonded terrain whose mana is **reversibly paused** because an enemy creature occupies a node on its only path back to **its owning root** (Champion or Companion, D8/D22). Positional, non-permanent mana denial; resumes when the path clears. *(Distinct from a Companion's death, D22, which un-bonds its terrain outright rather than pausing it.)* |
