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
| **Pessimistic default** | Design principle (D14): a rule's default is the weakest still-functional-and-fun case; cards improve it via **positive** lines, not default restrictions. |
| **Spell** | **Umbrella term** (D17): anything cast through the Aether (creatures, structures, items, rites). |
| **Rite** | The one-shot spell type (D17): resolves to an Aether **trace**, leaves no permanent. |
| **Structure** | Stationary, **non-carryable** Play-permanent (D17); subtypes e.g. *Building*. Absorbs non-equippable "artifacts." |
| **Item** | **Carryable** Play-permanent (D17): can be picked up / equipped. |
| **Summon** | Playing a creature onto a **bonded terrain in your realm** for its mana cost (D20); no Channel. Summoning-sick by default (D14). |
| **Mana** | The **single shared pool per player**, filled by channeling bonded terrain (D8). Spent on spells, summons, and any actor's mana-abilities. Global. |
| **Action Points (AP)** | A creature's **private per-turn budget** (D10). Spent on its abilities — by default `1AP: Move` and `3!AP: Attack`. Refills each turn, no carryover. Local to each creature; the creature analog of the Champion's **Channel**. Subsumes the old Movement stat. |
| **`!` cost (`x!AP`)** | A cost flavor: **require x AP, then consume all remaining AP** (vs. plain `xAP` which spends exactly x and leaves the rest). Makes "no multi-attack" emergent — default attack is `3!AP` (D10). |
| **Default ability** | An ability every creature has unless replaced (`1AP: Move`, `3!AP: Attack`). Base rules expressed as replaceable abilities (pillar 5), not hardcoded logic. |
| **Effect** | A discrete, structured change to game state. **Base rules and card text are both expressed as effects** (pillar 5). |
| **Modifier** | A continuous effect that changes the answer to a query (e.g. "+1 Movement"). |
| **Keyword** | A named, reusable ability/property from the keyword library (e.g. Flying, **Ranged N** — range is a keyword, not a core stat, D10). |
| **Query** | A question the engine asks to get a current value (cost, movement, legality). Never read a raw stat — always query. |
| **Event** | An applied state change that flows through the pipeline where cards can intercept it. |
| **True State** | The single authoritative, deterministic game state held by the engine/server. |
| **View** | What one player perceives — a projection of True State through the perception layer. Two players' views may disagree. |
| **Perception** | The query axis answering, per observer: is this visible? what does it look like? what is known? Modified by cards (Mimic, Mist, Submerged). |
| **Mimic / Submerged / Mist** | Example asymmetric-info mechanics: a unit that looks like something else / a unit hidden until detected / a region only one player can see into. See `design-asymmetric-information.md`. |
| **Claim** | A card-authored, possibly-false projection into an observer's view (soft channel). Holds until a **hard fact** contradicts it; then it **collapses** (D18). |
| **Hard fact / Hard channel** | Information the engine always projects truthfully (mana network, hand size, AP, positions, deaths, combat outcomes) — what falsifies claims. Opposite: **soft channel** (identity/appearance/hidden-zone contents), which cards may fake. |
| **Collapse** | When a claim is falsified by a hard fact, the observer learns the **full truth** (default), incl. which of a masked pool it was (D18). |
| **Conservation law** | One of a small, finite set of engine-enforced checks (e.g. claimed mana ≤ public max) that auto-collapse mechanically-impossible claims. The engine does **no** other deduction — humans do (D18). |
| **Live mana ledger** | The hidden running record of mana actually spent — the one standing quantity hidden by default (network is public), enabling cost-deception (D18). |
| **Face** | The atomic deception primitive (D18): a **per-observer overlay** (claimed identity + stat modifiers) a **permanent** carries over its true state. A creature can *enter with* a face; a spell can *apply* one. Governed by the belief model (collapses on a hard fact). |
| **Mimic (effect) / ChooseOne** | Effect combinators (D18): `Mimic(face)` **applies a `face`** to a permanent (and shows it on the trace); `ChooseOne[…]` is a modal effect the caster picks privately (→ true state). |
| **Champion** | *(Provisional term; "Channeler" leans in.)* A player's single on-board avatar — the only entity that can draw mana from the land and channel it. It is the resource-network **root** (D8), the **win condition** (kill it to win, D9), and an attackable piece. Evolves between matches along paths. See `design-champions.md`. |
| **Channel** | The Champion's **one action per turn** (D9): spend it to **bond** a terrain, **act as a creature** (move/attack), or **activate a Champion ability**. Distinct from mana (which is spent on spells/units, many/turn). The central bond-vs-ability decision. |
| **Realm** | The contiguous set of terrains the Champion has bonded. The Champion's movement is effectively realm-bound: stepping off its own network reversibly pauses the disconnected mana (D9). |
| **Loadout** | A Champion's fully-resolved configuration for one match (level, chosen path, unlocked abilities), produced by the meta-layer and consumed by the match core. |
| **Path** | A branch of a Champion's between-match progression tree; committing to a path gives a Champion a distinct identity. |
| **Meta-layer** | Systems *outside* the deterministic match core that persist across matches (Champion progression, unlocks, collection). |
| **Band** | A level range (e.g. 1–5) that defines a match/tournament format. Champions only face others in the same band. Power is horizontal within a band, stepping up between bands. |
| **Ground slot** | One of the 3 creature positions on a hex's **ground** layer. Each of the three layers (ground/above/below, D12) holds up to 3 creatures. |
| **Combat** | An explicit fight object: `{attacking units, target hex, declared defenders}` that resolves. Scheduled sequentially by default; potentially phased later. |
| **Stack** | The LIFO region of the **Aether** in front of the `now` marker, where spells/abilities/responses wait and resolve top-down (D16). See `design-interaction-stack.md`. |
| **Priority** | The right to act at a defined window; how players respond during the opponent's turn. |
| **Instant-speed / Sorcery-speed** | A card playable in a priority window (reactive) vs. only on your own turn's main action (most default cards). |
| **Trap** | A pre-committed, hidden triggered ability (pillar 6) that goes on the stack when its condition fires. |
| **Terrain deck** | A separate deck (distinct from the main deck) of terrain/land cards, laid out in the home base at game start. Size varies by format. |
| **Terrain** | A land card occupying board space that produces mana when connected. Basic (8 colors, unrestricted) or non-basic (restricted, more powerful). |
| **Connection / Bond** | The act (up to once/turn) of adding one terrain to the network rooted at the Champion, via already-bonded terrain. Bonding is **permanent**; the ramp mechanism. |
| **Bonded** | A terrain the Champion has permanently added to its network. A bonded terrain produces mana only while an enemy-free path connects it back to the Champion. |
| **Color** | One of 8 mana types. Basic lands each produce one color. |
| **Home base** | Synonym for **Home zone** (D11) — kept for continuity with D8. Where the realm begins; not a special/destructible place. |
| **On hold** | A bonded terrain whose mana is **reversibly paused** because an enemy creature occupies a node on its only path back to the Champion (D8). Positional, non-permanent mana denial; resumes when the path clears. |
