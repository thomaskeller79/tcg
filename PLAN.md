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
5. **Cards rewrite the rules.** The game must grow via new cards indefinitely, and cards must be able to *change the rules themselves* — not just numbers. The base rules and card effects share one representation, so "a card changes a rule" is the *only* case, never a bolted-on special case. This is the hardest architectural constraint and is designed in from day one. See `docs/rules/knowledge-capture-plan.md` §Part A.
6. **Asymmetric information.** Players do not share one view of the board — what each player *perceives* is a manipulable, card-driven property (Mimic, Submerged units, Mist regions). This exploits a digital-only advantage most TCGs leave on the table. Architecturally it is the same modifier/query paradigm as pillar 5, applied to a *perception* axis: one authoritative true state, projected into per-observer views. See `docs/rules/design-asymmetric-information.md`.
7. **Champions.** *(Secondary pillar.)* Each player is embodied by a single **Champion** — the only entity that can draw mana from the land and *channel* it into magic. It is the resource-network **root** (D8), the **win condition** (kill the enemy Champion to win — no separate Base, D9), and an exposed board piece, all at once. It runs the **same two-resource shape as a creature** (D9, revised 2026-08-06): *mana* (many spells/units per turn) and its own **Action Points** spent on draw / bond / move / fight / activate an ability — mechanically a special king-like creature, not a third economy. That resource-allocation puzzle is the game's second core decision layer. The in-match Champion is a special card type inside the deterministic core; all persistent progression lives in a **separate meta-layer** that hands the core a resolved loadout. Progression uses **level bands** (D2): horizontal within a band, a discrete step up between bands, matched only within a band. See `docs/rules/design-champions.md`.

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
- **The board comes from a *map card* (D11)** — a fourth match component (with card deck, terrain deck, Champion). Maps are a strategic choice, come in **sizes**, and **map size sets the terrain-deck size**. A map defines: form (which cells exist; "holes" = **void/aether** terrain), both start positions, each player's **home zone** (sized to the terrain deck, randomly filled at start), a **neutral area**, and optional **landmarks**. The engine consumes one resolved map as **data** (cells keyed by cube/axial coord).
- **Hex grid** for 6-directional movement and cleaner adjacency/flanking than squares. The engine exposes adjacency, distance, path, and **line-of-sight** as queries (LOS serves Ranged *and* Mist).
- **Three vertical layers per hex (D12):** ground / above (flying) / below (submerged), each capacity 3; the **below layer is hidden by default** (pillar 6).
- **Terrain** is a **cell property** (affects move cost, LOS, and mana via the D8 network — the mana graph is a subgraph of the board graph).
- Each player's **Champion starts on a map-defined home tile**; no separate destructible base — the Champion is the objective (§3.5, D9).

### 3.2 Cards & deckbuilding (the MTG half)
- **Card type is a data-driven tag (pillar 5), not a fixed class (D17).** **"Spell"** is the **umbrella** for anything cast through the Aether. Seed types:
  - **Creature** (Unit) — mobile Play-permanent; **three stats: Attack / Life / AP** (D10). AP is its private per-turn action budget; **mana is a single shared pool** (see `docs/rules/design-economy.md`).
  - **Structure** — stationary, **non-carryable** Play-permanent; subtypes (e.g. *Building*) fit the hex/location theme. Absorbs non-equippable "artifacts."
  - **Item** — **carryable** Play-permanent (pick up / equip).
  - **Rite** — the one-shot type; resolves to an Aether **trace**, leaves no permanent.
  - *Not "spells" — pre-game / separate economy:* **Map** (§3.1), **Champion** (pillar 7), **Terrain** (§3.3).
- **A legal match brings four components (D11):** card deck, **terrain deck** (§3.3), **Champion** (§pillar 7), and a **map** (§3.1) — all legality-constrained; map/terrain-deck sizes are linked.
- **Deckbuilding rules:** deck size, copy limits, and a **faction/color identity** system to give decks flavor and force meaningful choices. (Theme deferred, but the *structure* of factions is a mechanic to design now.)
- **Data-driven:** every card is defined in data (JSON/resource files), not code — so designers can add/balance cards without engineering. This is foundational to the architecture (§7).

### 3.3 Resource system — terrain connection network (DECIDED, D8)
A **separate terrain deck** is laid out randomly in each player's home base at start. Up to once per turn a player **connects** one more terrain reachable from their **Champion** through already-connected, unblocked terrain; connected terrain produces mana. This is a spatial, deterministic ramp that **eliminates draw-based mana/color screw** while keeping lands powerful and thought-after. 8 colors (8 unrestricted basics + restricted non-basics). The **Champion is the network root**, so advancing it endangers the economy — fusing resources to board and Champion (pillars 1, 2). Mana **denial is positional and reversible** (occupy a node to put a connection "on hold"), never random. This is the game's signature economy and a deliberately rich core system. Full design + open questions: `docs/rules/design-resources-terrain.md`.

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
This is the single most important technical decision. Full component breakdown, the client/server Host boundary, and the mode-by-mode "what runs where" matrix now live in `docs/architecture/design-architecture.md` (engineering decisions logged as A1–A5 in `docs/architecture/decisions-architecture.md`) — the shape below is the short version:

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

**Per-observer views (asymmetric information — pillar 6).** The core holds one deterministic *true state* but exposes it only through a **perception layer** that projects a separate *view* per player. The engine emits per-observer (redacted/transformed) event streams, never one global stream. Consequences: networking **must** be server-authoritative and view-redacted (clients never receive true state); the UI renders a view, never truth; AI must eventually reason from its own view, not truth. See `docs/rules/design-asymmetric-information.md`.

**Priority + stack (interaction).** The core includes a real priority/LIFO-stack capability so players can act on the opponent's turn (instants), enabling combat tricks and traps through one mechanism. Kept tame via defined priority windows and mostly sorcery-speed default cards. This is the largest complexity commitment in the core and touches AI (evaluate responses) and netcode (priority windows). See `docs/rules/design-interaction-stack.md`.

### Data-driven content
- Cards, units, terrain, and factions defined in **data files** (JSON or Godot resources), loaded by the engine.
- Enables rapid iteration and eventually a card editor — critical for a content-driven genre.

---

## 7. Roadmap (milestones)

| # | Milestone | Goal | Exit criteria |
|---|---|---|---|
| **M0** | Design lock-in | Answer §4 tensions on paper; pick resource model; draft 20–30 starter cards + 1 board | A one-page ruleset you can play by hand |
| **M1** | Rules engine core | Headless deterministic engine: state, phases, legal moves, combat, win check, terrain/mana network, a *minimal* priority/stack, a *minimal* hidden-info layer — exposed via two first-class queries: legal-command enumeration and per-observer view projection | Full game playable via unit tests / console; 100% deterministic |
| **M1.5** | Minimal playable prototype (debug UI) | Bare Godot 4 client over M1's query surface: monocolored hex terrain (TileMap), text-labeled creatures, legal-actions-as-buttons, a pending-stack list with respond/pass, three windows (P1 view / P2 view / true state) across screens | A full match is actually playable, and perceived-vs-true state can be eyeballed for consistency |
| **M2** | Playable hotseat (Godot) | Real board rendering, hand UI, input, local 1v1, polish | Two humans finish a real match on one PC |
| **M3** | Content + balance pass | Data-driven cards, 2–3 factions, terrain; first balance iteration | ~60–80 cards; matches feel varied and fair |
| **M4** | Solo vs AI | Legal-move-generating AI (heuristic → search) | AI plays a competent full game |
| **M5** | Polish + mobile export | UX, animation, touch controls, mobile build | Runs and is playable on a phone |
| **M6** | Online 1v1 *(stretch)* | Server-authoritative or lockstep netcode, matchmaking | Two players play remotely |

**Critical path:** M0 → M1 → M1.5 gets you to a game you can actually playtest — sooner than the original M0 → M1 → M2 path, since M1.5 is a stripped debug UI rather than the polished M2 client. Everything valuable about the design gets validated there before heavier investment. M2 comes after, reusing M1.5's query surface but building it out with real presentation.

---

## 8. Immediate next steps (updated — core design locked, D1–D21)

The design is captured across `docs/` (glossary, rules-structure, decisions **D1–D21**, and per-topic design notes). The **structural keystones are done**; what remains is tuning/content/playtest (see open items in `docs/rules/decisions.md`). **The next session starts here** (see memory `session-handoff`), as **two parallel tracks**:

**Track A — design review (parallel, user-led; does not block implementation):**
0. **Pessimistic-default audit** — sweep D1–D21 for generous defaults that should become positive keywords (e.g. D8 "creature blocks mana flow" → a **"Blockade"** keyword). Worklist: `docs/rules/pessimistic-default-audit.md`.
1. **Other card types.** Design/confirm rules for the non-Creature card types: current state of **Terrain**, **Map**, and **Rite** is thin; **Structure/Item** (Building subtype) need fleshing out; and there's a **new card type, "Companion,"** not yet introduced into the docs at all.
2. **The 8 colors.** Define each color's identity and its relationships to the others (allies/enemies, mechanical identity, philosophy) — aiming from the start for Mark-Rosewater-color-wheel quality, not a placeholder pass to redo later.
3. **Initial deck ideas per color.** Sketch what an early deck in each color wants to do — this should double as a seed for drafting actual starter cards.
4. **Fill remaining structural gaps:** Zones (`Hand/Library/Play/Aether`, D16), the three board layers (ground/above/below, D12), and **Graves** — currently undecided whether a Grave is a Zone or a board-layer/marker concept; needs to be pinned down.
5. **Gap sweep.** Once 1–4 land, take stock of what's still missing from the ruleset overall.

**Track B — implementation (M1 COMPLETE as of 2026-08-05):**
1. **Toolchain:** ✅ .NET SDK 10.0 (LTS) installed; Windows-native, VS Code + C# Dev Kit. Project renamed **Hex → Leyline** (see PLAN.md title).
2. **Agent / work breakdown:** solution scaffolded as `Leyline.RulesCore` (headless rules core + Perception, same assembly per architecture decision A1), `Leyline.Host` (Host abstraction, `LocalHost` for M1), `Leyline.Content.Json` (JSON card loading), `Leyline.SimHarness` (batch/interactive test harness), plus xUnit test projects. See `docs/architecture/decisions-architecture.md` A1–A5 for the boundaries this follows.
3. **Build M1 — ✅ done, all 6 slices, 64 passing xUnit tests:**
   - **Slice 1** (combat sandbox): board, creatures, Move/Attack incl. `!` AP cost, full event pipeline + query/modifier layer, full D3/D4/D13/D19 combat resolution with the one Combat-declare priority window, D15 defend-rule config toggle, turn/phase machine, JSON content loader.
   - **Slice 2**: Champion as an attackable win-check target (D9) — Combat needed **zero changes**, confirming the ActorState-sharing design.
   - **Slice 3**: terrain/mana network (D8) — permanent bonding, conditional/path-blocked mana draw, `RefreshManaEffect` added to Beginning phase as proof "phases are data" holds.
   - **Slice 4**: Champion-as-actor via `ChannelActCommand`, sharing one Channel-used flag with Bond. Move/Attack needed **zero changes**.
   - **Slice 5**: hidden Below layer (D12/D19) — concealment, reveal-on-attack, re-conceal-on-move; one `Query.IsVisibleTo` rule shared by Perception and Combat targeting.
   - **Slice 6**: `LocalHost` hardening (5 negative-assertion tests in a project that never references RulesCore internals), `DeterminismReplayTests` (same log twice, and direct-vs-Host), a golden-path integration test, and an interactive Host-mediated REPL (`dotnet run --project tools\Leyline.SimHarness -- interactive`) alongside the existing batch V1-vs-V2 sim (`dotnet run --project tools\Leyline.SimHarness`).
   - Two real bugs were caught by tests during implementation (a state-based-check ordering bug that would've missed Champion deaths, and a missing Champion-AP-reset-to-zero) — both fixed, both now regression-tested.
4. **✅ Done (2026-08-06):** the modifier add/remove mechanism designed in `docs/architecture/design-continuous-effects.md` is now implemented — `IModifier`/`ModifierId`/`ModifierDuration`, `AddModifierEvent`/`RemoveModifierEvent`, `Modifiers/ModifierPipeline.cs`, and `Turns/ExpireModifiersEffect.cs` (End-phase cleanup). `IQueryModifier.Priority` was dropped in favor of append order, as the doc leaned toward. 7 new tests (71 total passing). See the doc's "Resume here next session" section for the updated status — the Rite/Spell casting pipeline is now the next open gap it flags.
5. **✅ Done (2026-08-07):** the Champion-economy doc/code drift flagged below is resolved. `ChannelActCommand`/`ChannelUsedThisTurn`/`ChampionPipeline` are gone; the Champion now refreshes AP every Beginning phase exactly like a creature (`RefreshApEffect`, no type-check) and acts directly via the normal `MoveCommand`/`DeclareCombatCommand` — no special "become able to act" command. Bond is now a real ability (`champion.bond`, gated through `AbilityIds` like `core.move`/`core.attack`, not a hardcoded type-check) costing `2*AP` — D9's `*` once-per-turn cost flavor, newly modeled as `ActorState.OncePerTurnActionsUsed` + `Query.CanUseOncePerTurnAction`, reset each Beginning phase by `ResetOncePerTurnActionsEffect`. Also added a shared `IHasCardDefinition` interface (`CreatureState`/`ChampionState`) to collapse `Query.cs`'s duplicate per-type arms — anticipates Structure needing the same trait per `design-continuous-effects.md`, without literal `ChampionState : CreatureState` inheritance (Champion is a distinct top-level card type per §3.2, not a Creature subtype). Left as explicit follow-up, not invented here: the network-active/collapsed Move/Attack cost differential and the `5*AP` Draw action (needs Hand/Library, not built yet) — both still just use the shared creature defaults (`1AP`/`3!AP`). 71 tests passing (5 Host + 66 RulesCore).
6. **⚠️ First thing next session:** continue Track B by designing the Rite/Spell casting pipeline (the next dependency `design-continuous-effects.md` flags — no way to actually *cast* anything yet), or start **Build M1.5** (not started): minimal 3-window Godot 4 + C# debug UI over M1's `LegalCommands`/`Apply`/`ViewProjector` surface (see §7 milestone table).

---

## 9. Open questions parked for later
- **Theme/setting** — deferred by choice; design mechanics first, skin later. Faction *structure* (not flavor) is designed now.
- **Monetization / business model** — relevant to "serious indie" but not to the prototype.
- **Art pipeline & style** — after core loop is fun.
- **Progression / metagame** (unlocks, ranked, collection) — post-M4.
