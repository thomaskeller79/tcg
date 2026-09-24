# Project "Leyline" — High-Level Plan

*A digital deckbuilding + tactical hex-combat game. Working title: **Leyline**.*

**Status:** Initial plan · **Date:** 2026-07-23

---

## 1. Vision

A digital strategy game that fuses the **deckbuilding and variety of Magic: The Gathering** with the **positional, round-based tactics of a hex-grid tabletop wargame**. You bring a deck you built; you deploy and maneuver the units those cards summon across a hex battlefield; you win through superior deck construction *and* superior tactical play.

The two halves reinforce each other: your deck defines *what tools you have*, the board defines *how well you use them*. Neither pure luck (card draw) nor pure calculation (chess) should dominate — the target is **~70% skill, ~30% variance**, where variance creates fresh situations rather than deciding games.

### Design pillars
1. **Every card is also a board decision.** A card is never just "resolve and forget" — playing it means *where*, *when*, and *facing what*.
2. **Position is a resource.** High ground, chokepoints, flanking, and zone control matter as much as card advantage.
3. **Readable depth.** Simple to parse a board state at a glance; deep in the interactions. Complexity lives in card combinations, not in fiddly rules.
4. **Deterministic at heart.** The rules engine is a pure, deterministic simulation — enabling AI, replays, networked play, and rigorous testing (see §7).
5. **Cards rewrite the rules.** The game must grow via new cards indefinitely, and cards must be able to *change the rules themselves* — not just numbers. The base rules and card effects share one representation, so "a card changes a rule" is the *only* case, never a bolted-on special case. This is the hardest architectural constraint and is designed in from day one. See `docs/rules/history/knowledge-capture-plan.md` §Part A.
6. **Asymmetric information.** Players do not share one view of the board — what each player *perceives* is a manipulable, card-driven property (Mimic, Submerged units, Mist regions). This exploits a digital-only advantage most TCGs leave on the table. Architecturally it is the same modifier/query paradigm as pillar 5, applied to a *perception* axis: one authoritative true state, projected into per-observer views. See `docs/rules/asymmetric-information.md`.
7. **Champions.** *(Secondary pillar.)* Each player is embodied by a single **Champion** — the only entity that can draw mana from the land and *channel* it into magic. It is the resource-network **root** (D8), the **win condition** (kill the enemy Champion to win — no separate Base, D9), and an exposed board piece, all at once. It runs the **same two-resource shape as a creature** (D9, revised 2026-08-06): *mana* (many spells/units per turn) and its own **Action Points** spent on draw / bond / move / fight / activate an ability — mechanically a special king-like creature, not a third economy. That resource-allocation puzzle is the game's second core decision layer. The in-match Champion is a special card type inside the deterministic core; all persistent progression lives in a **separate meta-layer** that hands the core a resolved loadout. Progression uses **level bands** (D2): horizontal within a band, a discrete step up between bands, matched only within a band. See `docs/rules/champions.md`.

---

## 2. Core gameplay loop

A single **turn** for one player (D21), at a glance:

```
Beginning  ─────────────────►  Action  ──────────────────►  End
   │                              │                            │
 refresh mana/AP            play spells, spend AP to      end-of-turn
 (simultaneous);            move / attack / draw / bond   triggers
 APNAP begin triggers       / activate, any order,        (APNAP)
                            until you finish (opponent
                            may respond after EVERY
                            action — priority/stack)
```

Players alternate turns. A match is: **build deck → deploy from your realm → maneuver → fight → kill the enemy Champion.**

---

## 3. Key systems

### 3.1 The board (hex grid)
- **The board comes from a *map card* (D11)** — a fourth match component (with card deck, terrain deck, Champion). Maps are a strategic choice, come in **sizes**, and **map size sets the terrain-deck size**. A map defines: Layout (which cells exist; "holes" = **void/aether** terrain), both start positions, each player's **home ground** and **neutral ground** (each populated by a map-level rule, default random/fixed respectively), each hex's **Terrain Type** (a worldbuilding classification, decoupled from a card's Element, D52), and optional **landmarks**. The engine consumes one resolved map as **data** (cells keyed by cube/axial coord). See `docs/rules/map.md`.
- **Hex grid** for 6-directional movement and cleaner adjacency/flanking than squares. The engine exposes adjacency, distance, path, and **line-of-sight** as queries (LOS serves Ranged *and* Mist).
- **Three vertical levels per hex (D12, D41):** Surface / Air (flying) / Underground (submerged), each capacity 3; the **Underground level is hidden by default** (pillar 6).
- **Terrain** is a **cell property** (affects move cost, LOS, and mana via the D8 network — the mana graph is a subgraph of the board graph).
- Each player's **Champion starts on a map-defined home tile**; no separate destructible base — the Champion is the objective (§3.5, D9).

### 3.2 Cards & deckbuilding (the MTG half)
- **Card type is a data-driven tag (pillar 5), not a fixed class (D17, updated D39).** Every card type shares one generic play-a-card procedure, so no umbrella term is needed anymore. Seed types:
  - **Creature** (Unit) — mobile permanent; **three stats: Attack / Life / AP** (D10). AP is its private per-turn action budget; **mana is a single shared pool** (see `docs/rules/economy.md`).
  - **Companion** (D22) — a Champion's signature "friend," deckbuilding-gated to specific Champion(s); Creature-shaped (Attack/Life/AP) but can also **bond terrain** like a channeler — the mana it draws stays in a **private pool** it alone spends, on its own printed abilities. See `docs/rules/companions.md`.
  - **Structure** — stationary, **non-carryable** permanent; subtypes (e.g. *Building*) fit the hex/location theme. Absorbs non-equippable "artifacts."
  - **Item** — **carryable** permanent (pick up / equip).
  - **Spell** (D39) — the one-shot type; resolves to an Aether **trace**, leaves no permanent.
  - *Pre-game / separate economy, outside the card-type list:* **Map** (§3.1), **Champion** (pillar 7), **Terrain** (§3.3).
- **A legal match brings four components (D11):** card deck, **terrain deck** (§3.3), **Champion** (§pillar 7), and a **map** (§3.1) — all legality-constrained; map/terrain-deck sizes are linked.
- **Deckbuilding rules:** deck size, copy limits, and a **faction/color identity** system to give decks flavor and force meaningful choices. (Theme deferred, but the *structure* of factions is a mechanic to design now.)
- **Data-driven:** every card is defined in data (JSON/resource files), not code — so designers can add/balance cards without engineering. This is foundational to the architecture (§7).

### 3.3 Resource system — terrain connection network (DECIDED, D8)
A **separate terrain deck** is laid out randomly in each player's home ground at start. Up to once per turn a player **connects** one more terrain reachable from their **Champion** through already-connected, unblocked terrain; connected terrain produces mana. This is a spatial, deterministic ramp that **eliminates draw-based mana/color screw** while keeping lands powerful and thought-after. 8 colors (8 unrestricted basics + restricted non-basics). The **Champion is the network root**, so advancing it endangers the economy — fusing resources to board and Champion (pillars 1, 2). A **Companion (D22) is a second, independent root** — it bonds terrain the same way, but the mana stays in a **private pool** only it can spend, never joining the shared pool. Mana **denial is positional and reversible** (occupy a node to put a connection "on hold"), never random. This is the game's signature economy and a deliberately rich core system. Full design + open questions: `docs/rules/resources-terrain.md`, `docs/rules/companions.md`.

### 3.4 Combat (DECIDED: D3, D4, D13)
- **Attacks target a hex; the defender declares defenders** (guard mechanic, D4). Every fight is an explicit `Combat` object, resolved **sequentially** now, revertible to phased (D3).
- **Damage (D13): mutual, simultaneous, attacker-assigns across gang-up defenders.** Resolution is a set of **directed damage events**, each independently modifiable — so Ranged/First Strike/Trample are keywords, not rules. **No dice, no facing/flanking/high-ground**: position matters via adjacency, Range, occupancy, and guarding, not combat modifiers.
- **Damage persists; no auto-heal** — healing is a special ability (D14, pessimistic defaults). Deterministic — variance comes from the deck, not dice.

### 3.5 Win conditions (DECIDED, D9)
- **Default: kill the enemy Champion** (Duelyst-style). There is **no separate Base** — the Champion *is* the objective, and also the economic root (D8) and most-exposed piece, so every Champion decision is loaded. You must fight *through* the map to reach it, marrying both halves.
- Secondary/alternate win conditions per faction add deckbuilding depth (control, objectives, mill-analog) — layered on as cards (the win check is an evaluated effect, not hardcoded).

---

## 4. The hard design tensions (resolve via playtesting)

These are the interesting problems at the MTG × tactics intersection. The plan is to prototype fast and answer them empirically:

1. **Variance vs. skill.** Where does randomness live? (Recommendation: in deck draw only; keep board/combat deterministic.)
2. **Turn structure.** Strict I-go-you-go (clean, can feel slow) vs. simultaneous/phased orders (dynamic, harder to build). *Start with alternating rounds; it's simplest to implement and reason about.*
3. **Snowballing.** Tactics games punish early losses harshly. Need catch-up valves (comeback mechanics, board resets, resource floors).
4. **Match length.** Must be satisfying yet short enough for mobile sessions. Target: **10–20 minutes.**
5. **Tempo of card play vs. movement.** How many cards/turn? How much movement/turn? This ratio *is* the game's texture.
6. **Complexity budget.** Every rule competes for the player's attention with the board. Favor fewer, deeper systems.

---

## 5. Game modes

Multiple modes were requested; recommended build order:

1. **Hotseat / local 1v1** — *first*, because it needs only the rules engine + UI, no AI or netcode. Fastest path to a playable game and rule validation. *Caveat: shared-screen hotseat conflicts with hidden info (pillar 6) — either add a "hide screen / pass device" step or accept degraded hidden-info fidelity in this mode only.*
2. **Solo vs AI** — the deterministic engine makes AI tractable (search over legal moves; start with heuristics, later MCTS/minimax).
3. **Online 1v1** — the deterministic engine enables lockstep or command-based netcode and server-authoritative validation. Highest infra cost; do last.

> Modes are ordered to reuse the same core. Because the engine is deterministic and headless, all three are *views* over one simulation.

---

## 6. Recommended technology

**Constraints:** PC-first, mobile portability later, experienced developer, serious-indie ambition with a learning goal.

### Engine: **Godot 4 (with C#)** — primary recommendation
- Free and open-source, no royalties — good for indie.
- First-class **PC and mobile** export from one codebase (satisfies the portability requirement).
- Excellent for **2D, turn-based, UI-heavy** card/tactics games (this game is not physics/3D-heavy).
- Lightweight and readable — strong for **learning game implementation** without Unity's overhead.
- C# gives you a real typed language for the rules engine; GDScript remains available for glue.

**Alternative: Unity (C#)** — choose if you want the largest asset/tutorial ecosystem and the most battle-tested mobile pipeline, and don't mind more weight and licensing considerations. Everything in this plan's architecture is engine-agnostic.

### Architecture: **deterministic rules core, separated from presentation**
This is the single most important technical decision. Full component breakdown, the client/server Host boundary, and the mode-by-mode "what runs where" matrix now live in `docs/architecture/architecture.md` (engineering decisions logged as A1–A5 in `docs/architecture/history/decisions-architecture.md`) — the shape below is the short version:

```
Rules Core (pure C#, headless, deterministic) → Perception layer (per-observer views, pillar 6)
                                                            │
                                                          HOST  (commands in / this seat's view+events out)
                                              ┌─────────────┴─────────────┐
                                     Local/Embedded                Remote/Networked
                                    (in-process, solo/hotseat)     (network → Server process, online 1v1)
                                              │                             │
                                   Human/UI seat-controller         AI seat-controller
```

Same Rules Core binary, same Host contract, either transport underneath — this is what makes "everything the server does in 1v1 must also run on the client" literally true, not just aspirational. `docs/` itself now splits the same way: `docs/rules/` = what the game is, `docs/architecture/` = how it's built (see `docs/README.md`).

Why it's worth the discipline:
- **Testable:** rules verified without rendering.
- **AI-ready:** the AI plays by generating and evaluating legal commands against the same engine.
- **Net-ready:** deterministic simulation enables server-authoritative or lockstep multiplayer with anti-cheat essentially for free.
- **Replays & balancing:** record command streams; replay and batch-simulate for balance analysis.

**Per-observer views (asymmetric information — pillar 6).** The core holds one deterministic *true state* but exposes it only through a **perception layer** that projects a separate *view* per player. The engine emits per-observer (redacted/transformed) event streams, never one global stream. Consequences: networking **must** be server-authoritative and view-redacted (clients never receive true state); the UI renders a view, never truth; AI must eventually reason from its own view, not truth. See `docs/rules/asymmetric-information.md`.

**Priority + stack (interaction).** The core includes a real priority/LIFO-stack capability so players can act on the opponent's turn (instants), enabling combat tricks and traps through one mechanism. Kept tame via defined priority windows and mostly sorcery-speed default cards. This is the largest complexity commitment in the core and touches AI (evaluate responses) and netcode (priority windows). See `docs/rules/interaction-stack.md`.

### Data-driven content
- Cards, units, terrain, and factions defined in **data files** (JSON or Godot resources), loaded by the engine.
- Enables rapid iteration and eventually a card editor — critical for a content-driven genre.

---

## 7. Roadmap (milestones)

| # | Milestone | Goal | Exit criteria |
|---|---|---|---|
| **M0** | Design lock-in | Answer §4 tensions on paper; pick resource model; draft 20–30 starter cards + 1 board | A one-page ruleset you can play by hand |
| **M1** | Rules engine core | Headless deterministic engine: state, phases, legal moves, combat, win check, terrain/mana network, a *minimal* priority/stack, a *minimal* hidden-info layer — exposed via two first-class queries: legal-command enumeration and per-observer view projection | Full game playable via unit tests / console; 100% deterministic |
| **M1.5** | Minimal playable prototype (debug UI) | **✅ Built (2026-08-07 → 08-09).** Tech pivot: a local web UI instead of Godot for this throwaway debug stage — Godot is still the real M2 client. Local API server (`Leyline.DebugUi`) over `IHost`, hex-board SVG (drag-and-drop move/attack, legal-actions-as-buttons, auto-resolved priority windows since M1 has no instant-speed content yet, P1/P2/true-state views incl. Hand/Library), plus a hand-editable **text scenario format** (`Leyline.Scenarios`, see `docs/tools/scenario-format.md`) so test positions are authored as data | ✅ A full match is actually playable, perceived-vs-true state can be eyeballed for consistency, and a new test scenario can be set up by editing a text file, not writing code |
| **M2** | Playable hotseat (Godot) | Real board rendering, hand UI, input, local 1v1, polish | Two humans finish a real match on one PC |
| **M3** | Content + balance pass | Data-driven cards, 2–3 factions, terrain; first balance iteration | ~60–80 cards; matches feel varied and fair |
| **M4** | Solo vs AI | Legal-move-generating AI (heuristic → search) | AI plays a competent full game |
| **M5** | Polish + mobile export | UX, animation, touch controls, mobile build | Runs and is playable on a phone |
| **M6** | Online 1v1 *(stretch)* | Server-authoritative or lockstep netcode, matchmaking | Two players play remotely |

**Critical path:** M0 → M1 → M1.5 gets you to a game you can actually playtest — sooner than the original M0 → M1 → M2 path, since M1.5 is a stripped debug UI rather than the polished M2 client. Everything valuable about the design gets validated there before heavier investment. M2 comes after, reusing M1.5's query surface but building it out with real presentation.

---

## 8. Immediate next steps

Design work runs across three parallel tracks — **Rules** (mechanics/engine-level design), **Implementation** (code), and **Content** (cards, Element identities, abilities/keywords) — split 2026-09-14 so a mechanics question and a "just write the card" question don't get tangled together. Current-state rules live in `docs/rules/`; full decision history and reasoning (**D1–D77**) lives in `docs/rules/history/decisions.md`. **This section tracks only what's still open** — what's already decided is not repeated here; see `docs/README.md` for how the split works.

**Numbering rule (all three tracks): item numbers are never reused or renumbered.** A resolved/removed item is deleted outright, leaving a gap — never causes the remaining items to shift. New items are only ever appended at the end. This is deliberate: other docs cross-reference these items by number, and a past renumbering silently broke several of those references (fixed 2026-09-22) with no way to detect it short of a manual audit.

**Track A — Rules (design review, parallel, user-led; does not block implementation):**

1. **Uniqueness / copy-limit rule (general).** Surfaced by Companions (expected to be one-ofs) but deliberately **not** a Companion-specific rule — design a general singleton/"legendary-type" mechanic (max copies in deck, max copies on the Island) that Companions, and any future card that wants the same restriction, just consume. Ties into the existing open "deck size / max copies" item in `overview.md` §6.
2. **Simultaneous movement / a richer priority-window model.** The current strict-alternating-turns model (D21/D45) can force a bad trade purely from action sequencing: e.g. two creatures share a hex specifically so an instant-speed "mind control"-style effect fizzles (D66) against either one — but the moment the controller needs to move *one* of them away, that single action opens a priority window with the pair now split, letting the opponent's instant resolve against the newly-isolated creature before the second move ever happens. The player never gets to treat "move both creatures" as one atomic decision. Candidate direction, not designed: the opponent doesn't gain priority until a *repeated* action on the same creature, or a spell/ability activation — letting a single creature's first move-of-the-turn (or a "move a whole group" action) resolve without opening a window. Not scoped, not evaluated against pillars 1/2/4, no prototype cards yet. See `docs/rules/interaction-stack.md`, `docs/rules/overview.md` §3.
3. **Compact the rules docs and decision log.** Much of `docs/rules/` reads as chatty/verbose (long sentences, heavy parenthetical justification), and the ruleset is likely nowhere near half designed — this density won't scale as more systems land. **Real tension to resolve first, not just an editing pass:** `decisions.md` entries are written deliberately dense at the user's own explicit request, specifically to preserve reasoning worth revisiting later — so "compact" can't just mean "make everything terser" without first deciding what's allowed to stay dense (the reasoning/history) versus what should be terse by default (the current-state rules in `docs/rules/*.md`, which don't carry the same "preserve for later" justification). Needs its own discussion before any editing.
4. **Full per-type property inventory** (`object-properties.md`, D69's Card/Trace/Permanent tiers). A first draft (Creature/Champion/Companion/Structure/Item/Terrain, plus the tiers themselves) found real gaps: Champion's Attack stat is unprinted anywhere in `champions.md`; Grave/Ruin are confirmed as stat-bearing objects, but their actual property list was never derived. **Explicitly not assumed complete** — go through every object type deliberately rather than trusting the draft table. Worth doing carefully, not as a checklist: this one table decides property survival in *both* directions at once — forward instantiation and backward Bounce/Flicker/Remand read the identical per-property tier lookup in opposite directions, so building it once, per property, makes every transition's behavior (known or future) fall out automatically instead of needing a separate ruling each time.
5. **Within-domain zone transitions, broader and not yet scoped.** D69's object-creation model only covers the six *cross-domain* transitions. Untouched: transitions *within* a domain (Library→Hand/draw, Hand→Discard/discard, Pending→Past/resolve, Pending↔Future/delay, mill) — some are ordinary game progression, others (discard, mill) are card-grantable actions that may need their own version of the "what survives" question. Also untouched: a transition **to nowhere** — an object destroyed/exiled outright, no destination zone at all.
6. **"Card" name collision — deliberately parked.** "Card" is used for two different things: a deckbuilding-tier **collection card** (anything assembled into a deck, including Champion and Terrain, which never touch a Mind-domain zone) and the specific Mind-domain **game-object** type that occupies Hand/Library/Discard. Needs distinct terminology eventually. A broader idea floated alongside it, also not adopted and also not scheduled: a Mind-domain flavor-renaming pass leaning into an Idea/Action/Reality framing (e.g. Library→Subconscious, Hand→Conscious, Discard→Memory, Card→Thought/Idea — illustrative only, not proposed as-is) — rejected for now on cost-vs-payoff grounds (these are the highest-frequency terms in the ruleset, and genre-standard vocabulary is free onboarding), but worth keeping alive if the collision ever gets tackled for real.
7. **Scenario/Neutral-permanent placement + the mission/PvE content layer.** How does a Map-driven mechanism place Neutral Structures/Items/Creatures/Companions onto the Island at Setup, alongside the already-resolved Champion/Terrain placement (`docs/rules/setup.md`)? A larger topic than Champion/Terrain since it's Map-driven, not a fixed rule. Built on top of this, also open: the actual mission/PvE content layer — scenario-authored Neutral permanents as monster encounters, an orphaned uncontrolled creature as one narrative NPC source, killing one as a bonus objective. One interface point already flagged: a mission/scenario (not a card effect) is what assigns a Neutral Phase to a permanent that didn't come from a card, since D56 only specifies who assigns it for the card-effect case. (The underlying "what does an uncontrolled creature do" question is already resolved — Neutral Permanents/Behavior, `docs/rules/neutral-permanents.md`; what's open here is specifically placement and content.)
10. **Where does Resources (mana/AP as abstract quantities) belong among the top-level concepts?** Unclear whether Resources deserves to be its own top-level concept (card games like MTG treat Mana/Life as general concepts independent of zones/card-types) or whether it folds into Ancestry/Payment (`docs/rules/ancestry.md`, D81) — the terrain-network/bonding mechanics are Board-zone content either way, but the abstract "what mana and AP *are*" piece still needs a home. See `docs/rules/economy.md`.
11. **Card-level continuous effects.** Does — and how does — a continuous effect (modifier) apply to a Card sitting in Hand or Library, before it's ever a Permanent (e.g. a hypothetical "target creature card in your hand gains +1/+1/+1")? `object-properties.md` only describes how such a modifier would *propagate* if one existed, not whether/how one attaches to a Card in the first place. Distinct from item 4 (which inventories what properties Card/Trace/Permanent already have) — this is whether the modifier system reaches into Hand/Library at all. See `object-properties.md` Open Question 1.
12. **How do Map, the Champions' terrain decks, and the Champions themselves actually combine to produce the initial board state?** Champion and Terrain plausibly reach the Island through a mechanism that also involves the Map, not a plain Card→Permanent read (D69) — genuinely open, not just unstated. `docs/rules/setup.md`'s current procedure gives a complete working answer for Champion/Terrain placement without settling this underlying question. Related to but distinct from item 7 (which is about placing *Neutral* permanents, not Champion/Terrain). See `object-properties.md` Open Question 4.
13. **Gap sweep once the rest of this list lands.** Take stock of what's still missing from the ruleset overall.
14. **Pessimistic-default audit** — sweep D1–D35 *and* everything decided elsewhere on this list for generous defaults that should become positive keywords (e.g. D8 "creature blocks mana flow" → a **"Blockade"** keyword). Deliberately last: the rest of this list will introduce their own defaults, and auditing once at the end catches those too instead of doing the pass twice. Worklist: `docs/rules/history/pessimistic-default-audit.md`.
15. **Work out match-format variants beyond 1v1.** PvP 1v1 is the only fully designed format; the core's turn/win/mana/perception rules all assume exactly two Champions, one per side (`overview.md`'s "two-Champion-seat framework is invariant for now"). Two directions floated, neither designed: PvE/scenario missions (ties to item 7's Neutral-permanent placement question) and team formats with more than one Champion per side (e.g. a two-headed-giant-style variant, floated in `companions.md` only as a reason to state the mana-pool-crossing rule explicitly). See `docs/rules/match-formats.md` for the collected ideas and open questions.
17. **Terminology for the Ancestry's parts.** Two related naming gaps surfaced by D81, neither resolved: (a) a noun for the node the **Payment** walk lands on (the first Companion/Champion holding a resource) — distinct from **Controller** — needed for card text like "deal damage to the entity that pays for target creature"; "payer" was proposed and rejected as weak, no replacement chosen. (b) Whether each distinct parent-child relationship in the Ancestry deserves its own verb, the way Terrain already has "bonded" and Item has "equipped" — candidates floated for Item (bearer/holder/carrier), none adopted; Creature/Trace/Card/Structure/Ruin/Grave's parent-relationships are suspected not to need their own word, since they either collapse onto the payer or reuse "location," but this wasn't confirmed type-by-type. See `docs/rules/ancestry.md` Open Questions.
18. **Ancestry adequacy audit — what questions can it actually answer, and which can't it?** Surfaced while resolving former item 16 (`history/decisions.md` D82, `docs/rules/neutral-permanents.md` §Becoming Neutral): correctly assigning Behavior/turn to a Neutral permanent created or converted by *another* Neutral permanent's own ability requires knowing which specific permanent caused it — but a Trace's Ancestry parent (D81) is deliberately flattened to the nearest live Champion or Companion for payment robustness, which resolves to nothing when the causing permanent is itself Neutral. D81 had assumed this flattened `parent` link also answered D75's separately-parked `source` question ("does a Trace carry a back-reference to whatever created it") without re-testing that assumption against the Neutral case — it holds whenever the producer's own chain reaches a live Champion/Companion, and silently fails otherwise. Not patched piecemeal (a `source` field tracked separately from `parent`, never re-climbed for funding, was floated but not adopted) — treat as one instance of a more fundamental question: what is Ancestry actually for, which questions is it the right mechanism to answer, and which questions does it structurally not retain by design? Connects to item 4's per-type property inventory (a `source` field, if adopted, is exactly the kind of property that inventory should place) but is framed more fundamentally than a property list.
19. **Does Structure/Ruin/Grave losing payer and controller (when its terrain's bond is severed or blocked, `ancestry.md` §Removing an inner node) need its own Behavior/neutral-turn default, or does D82's case-6 default extend to it?** Surfaced while documenting D82: this is a second "changed control, caused automatically by game rules" pathway alongside a Companion's death, not covered by D82's rule as written. Differs in character: reversible (the terrain's controller is a live, per-query result, never a stored flag, `ancestry.md`) rather than permanent, and a Structure's controller has no stored "previous controller" the way Creature/Trace's fixed creation-time parent gives them one — "previous controller" would have to mean something else here (e.g. whichever Champion it most recently resolved to). Not scoped further here.

**Track C — Content (cards, Element identities, abilities/keywords):**

1. **Element identities.** The 8 colors are named (D51: Light, Fire, Metal, Earth, Darkness, Ice, Water, Air) — still open: each Element's own identity and its relationships to the others (allies/enemies, mechanical identity, philosophy), aiming for Mark-Rosewater-color-wheel quality from the start, not a placeholder pass to redo later. Also part of this: a **Terrain Type** worldbuilding layer, decoupled from Element, giving a map its own authored identity — see `docs/rules/map.md` (D52, D53).
2. **Initial deck ideas per Element.** Sketch what an early deck in each Element wants to do — doubles as a seed for drafting actual starter cards.
3. **Write the Behavior keywords as real card text.** Blocked behind Track B validating the Behavior mechanism in an actual implementation first. **Aggressive toward X** (≈Warrior) and **Sharp Shooter** are pre-validated by their traces in D62–D64, ready to write up. **Vanisher** has two variants conceptually validated but not yet written into `docs/rules/neutral-permanents.md` (this item is their only record): an **evasive** variant (Move-only; a "safety" criterion — no enemy can legally attack the resulting hex — ranked above resulting-distance-to-home) and a **combat-clearing** variant (adds Attack, scoped to the enemy blocking the shortest remaining path, cumulative damage dealt as tiebreak). The safety criterion likely needs refining from a flat "can't be attacked at all" boolean toward "can't be attacked *for lethal*" — floated, not specified. Two separately-named keywords, not composed — no compositional modifier system exists yet. **Assassin, Patrol, Flee toward X, and Produce** still need real design, not just write-up: Patrol (follow a fixed path, react to what's encountered) and Produce (a non-Actor Structure Behavior activating a spawn ability) remain one-line sketches with no target-derivation or stopping-condition design. Flee toward X's pre-D62 sketch (`history/neutral-permanents-draft.md` §8) shares Vanisher's shape (move toward a destination, fight through if blocked) — likely the same design exercise as Vanisher's two variants, not a separate one.
4. **Design cards around three terrain-severing ideas removed from `docs/rules/resources-terrain.md`/`ancestry.md`.** They'd been sitting there as throwaway card-text examples with no real design behind them; the rules now state only the one actually-designed cause of a hard unbond (a Companion's death un-bonds its own terrain). The three cut ideas, worth prototyping as real cards before treating any as an adopted mechanism: (a) a granular, single-bond version of the Champion's Collapse Network — drop one specific bond instead of the whole network at once, cost/mechanism undesigned; (b) a new **cost primitive** — "as an additional cost, unbond target land you control" — alongside the existing mana/AP/life/discard/sacrifice cost types; (c) a new **hostile effect archetype** — "target Champion or Companion unbonds from target land" — economic disruption that lands without needing ongoing board presence, unlike the existing positional pause.
5. **Design a keyword that lets a Root-Slice (hidden) creature attack a Ground-Slice creature.** `overview.md`'s cross-Slice targeting table used to allow this by default, flagged tentative/pending balance; pessimistic default (D14) now makes it illegal by default instead, same as every other capability a hidden creature doesn't automatically get. Needs an actual positive keyword (an "ambush" style ability) that grants it, not yet designed.
6. **Design a card/ability that lets a surfaced creature re-conceal itself.** Attacking from the Root Slice permanently surfaces (reveals) a creature by default now (pessimistic default, D14) — it no longer automatically re-hides just by moving off that hex. Needs an actual ability, not yet designed, that re-conceals it under some condition (floated: once it's far enough away from every opponent).
7. **Prototype cards to stress-test D82's Neutral Behavior/neutral-turn rule (`docs/rules/neutral-permanents.md` §Becoming Neutral)** — settled entirely through abstract case reasoning, never run against an actual card. Three worth designing: (a) a **case-1 example**: a Neutral Structure (not a Creature) with a spawn/creation ability, to check the "inherits creator's Behavior and neutral turn" default still reads sensibly from a non-Creature source. (b) A **case-4 example**: a Neutral creature with a "target creature becomes Neutral" style ability, to check that inheriting the converter's *exact* Behavior instance (not just the keyword name — its already-resolved `X`, e.g. a specific enemy Champion) produces something coherent rather than something silly. (c) A **worked case-6 example**: a specific Companion with 2–3 named creatures, walked through dying mid-game, to actually feel out whether the "Aggressive toward the other Champion" default reads as fair in play, not just in the abstract argument that led to it. Once item 19 (Structure/Ruin/Grave's own default) is resolved, it likely wants its own worked example too.

**Track B — Implementation (M1/M1.5 complete; current build is fully playable end to end — move, attack, bond, draw, cast, defend):**
1. **Route every activated ability through Pending, not just Creature/Spell casting.** Per D45/`interaction-stack.md`, "pay a cost upfront to put it in Pending" is meant to apply generally, to any actor's activated ability — casting is just one instance, already routed through `Aether/AetherPipeline.cs` (`SpellPipeline`). Still bypassing it: the Champion's other activated abilities (Draw, Bond, Collapse Network) and Move/Attack (which keep Combat's own separate, pre-existing priority window). One item, not one-off patches per ability: give each a real Trace through the same mechanism — Move/Attack/Ascend/Descend specifically as "Physical Traces" (D45, zero Past residency) rather than a normal fading trace.
2. **Implement D70's cast/activate timing procedure** (pay → choose → target, CNF-structured cost, an abort option, live-collapsing trace text — see `docs/rules/interaction-stack.md`) through the same Pending/`AetherPipeline` mechanism item 1 is already building out. Rules-side design is done; implementation not started.
3. **Make Discard private, matching Hand (D71).** `DiscardView` (`src/Leyline.RulesCore/Perception/View.cs:37`) is populated unconditionally for both players in `ViewProjector.cs:47` — needs the same observer-gated shape `HandView` already has (`View.cs:25`, `ViewProjector.cs:36-38`): a public `Count` plus a `Cards` list that's `null` for any non-owner observer. Also touches `tools/Leyline.DebugUi/wwwroot/app.js` (Mind-zone rendering, ~lines 510-644, currently treats Discard as fully public by design per a comment at line 621) — needs the same own-cards-face-up/opponent-redacted split the Hand panel already does. Add a regression test alongside `tests/Leyline.RulesCore.Tests/Perception/ManaVisibilityTests.cs`/`HiddenLevelTests.cs`.
4. **Candidates, not committed, none blocking:** generalize the Rite/Spell effect placeholder (`RiteEffectIds`, currently just damage/heal) into a real card-effect system — `docs/architecture/continuous-effects.md` still calls this undesigned; extend casting to Structure/Item once Track A finishes designing those types; playtest the two rule variants parked in `docs/rules/history/playtest-variants.md`.

---

## 9. Explicitly deferred (out of scope for now)
- **Theme/setting** — deferred by choice; design mechanics first, skin later. Faction *structure* (not flavor) is designed now.
- **Monetization / business model** — relevant to "serious indie" but not to the prototype.
- **Art pipeline & style** — after core loop is fun.
- **Progression / metagame** (unlocks, ranked, collection) — post-M4.
