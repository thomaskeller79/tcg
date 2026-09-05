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
- **The board comes from a *map card* (D11)** — a fourth match component (with card deck, terrain deck, Champion). Maps are a strategic choice, come in **sizes**, and **map size sets the terrain-deck size**. A map defines: form (which cells exist; "holes" = **void/aether** terrain), both start positions, each player's **home zone** (sized to the terrain deck, randomly filled at start), a **neutral area**, and optional **landmarks**. The engine consumes one resolved map as **data** (cells keyed by cube/axial coord).
- **Hex grid** for 6-directional movement and cleaner adjacency/flanking than squares. The engine exposes adjacency, distance, path, and **line-of-sight** as queries (LOS serves Ranged *and* Mist).
- **Three vertical layers per hex (D12):** ground / above (flying) / below (submerged), each capacity 3; the **below layer is hidden by default** (pillar 6).
- **Terrain** is a **cell property** (affects move cost, LOS, and mana via the D8 network — the mana graph is a subgraph of the board graph).
- Each player's **Champion starts on a map-defined home tile**; no separate destructible base — the Champion is the objective (§3.5, D9).

### 3.2 Cards & deckbuilding (the MTG half)
- **Card type is a data-driven tag (pillar 5), not a fixed class (D17, updated D39).** Every card type shares one generic play-a-card procedure, so no umbrella term is needed anymore. Seed types:
  - **Creature** (Unit) — mobile Play-permanent; **three stats: Attack / Life / AP** (D10). AP is its private per-turn action budget; **mana is a single shared pool** (see `docs/rules/economy.md`).
  - **Companion** (D22) — a Champion's signature "friend," deckbuilding-gated to specific Champion(s); Creature-shaped (Attack/Life/AP) but can also **bond terrain** like a channeler — the mana it draws stays in a **private pool** it alone spends, on its own printed abilities. See `docs/rules/companions.md`.
  - **Structure** — stationary, **non-carryable** Play-permanent; subtypes (e.g. *Building*) fit the hex/location theme. Absorbs non-equippable "artifacts."
  - **Item** — **carryable** Play-permanent (pick up / equip).
  - **Spell** (D39) — the one-shot type; resolves to an Aether **trace**, leaves no permanent.
  - *Pre-game / separate economy, outside the card-type list:* **Map** (§3.1), **Champion** (pillar 7), **Terrain** (§3.3).
- **A legal match brings four components (D11):** card deck, **terrain deck** (§3.3), **Champion** (§pillar 7), and a **map** (§3.1) — all legality-constrained; map/terrain-deck sizes are linked.
- **Deckbuilding rules:** deck size, copy limits, and a **faction/color identity** system to give decks flavor and force meaningful choices. (Theme deferred, but the *structure* of factions is a mechanic to design now.)
- **Data-driven:** every card is defined in data (JSON/resource files), not code — so designers can add/balance cards without engineering. This is foundational to the architecture (§7).

### 3.3 Resource system — terrain connection network (DECIDED, D8)
A **separate terrain deck** is laid out randomly in each player's home base at start. Up to once per turn a player **connects** one more terrain reachable from their **Champion** through already-connected, unblocked terrain; connected terrain produces mana. This is a spatial, deterministic ramp that **eliminates draw-based mana/color screw** while keeping lands powerful and thought-after. 8 colors (8 unrestricted basics + restricted non-basics). The **Champion is the network root**, so advancing it endangers the economy — fusing resources to board and Champion (pillars 1, 2). A **Companion (D22) is a second, independent root** — it bonds terrain the same way, but the mana stays in a **private pool** only it can spend, never joining the shared pool. Mana **denial is positional and reversible** (occupy a node to put a connection "on hold"), never random. This is the game's signature economy and a deliberately rich core system. Full design + open questions: `docs/rules/resources-terrain.md`, `docs/rules/companions.md`.

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

## 8. Immediate next steps (updated — core design locked)

The design is captured across `docs/rules/` (current state) and `docs/rules/history/` (decision log **D1–D50** and rejected alternatives) — see `docs/README.md` for how the split works. The **structural keystones are done**; what remains is tuning/content/playtest (see open items in `docs/rules/history/decisions.md`). **The next session starts here** (see memory `session-handoff`), as **two parallel tracks**:

**Track A — design review (parallel, user-led; does not block implementation):**

**→ NEXT SESSION OPENS HERE: Neutral Permanents.** A full draft proposal — three-way control state (Player A/Player B/Neutral), two Neutral Phases per round, mandatory-deterministic Behavior algorithms, belief-state reuse of the existing View/perception model, example Behaviors (Aggressive toward X, Patrol, Flee toward X, Produce), and rules for creating/converting Neutral permanents — exists but was deliberately **not** discussed in the 2026-09-04 session (it was the one item held back so the rest of that session's large rules draft could be worked through first). See `docs/rules/history/neutral-permanents-draft.md` (verbatim draft text) — directly answers the parked D29 question below (item 4).

**2026-09-04 rules-merge session, done:** compared a second large rules draft (Mind/Aether/Matter domains, restructured Card Zone/Aether, a four-tier Speed system replacing D5/D23, Activation Capacity extended to non-Actor Objects, a Basic Abilities/Subterranean pass, and Structure/Ruin capacity revised) against the existing ruleset, item by item — nothing batch-adopted. Landed as **D37–D50**: Discard pile added (doesn't reopen D16); Aether restructured to Past/Now/Pending/Future; Rite renamed to Spell (D17's umbrella retired); Structure/Ruin capacity revised to two slots/hex, superseding D31; levels renamed Surface/Air/Underground; Subterranean keyword gates Underground access+concealment together, with Ascend/Descend; Areas primitive (distance/line/listing) adopted; "Unit" retired for **Actor**/**Object** terminology (tentative naming, revisit later); Speed system (Slow/Quick/Reactive/Instant) replaces the old priority-window model, with Instant fully atomic; cast/activate procedure now pays cost before choosing targets; Activation Capacity extends to Terrain/Structure/Ruins/Graves, additive to the existing ownership-tree funding model; first-player Champion starts with reduced AP; Champion/Companion Attack cost unified with the generic Actor default. Full detail: `docs/rules/history/decisions.md` D37–D50.

0. **Other card types — done, incl. a location model unified across Creature/Structure/Item (2026-08-09 → 08-10).** Map (D11) was already fully specced, not actually thin. Rite is fine as a card-type concept; what's unbuilt is the general card-effect system behind it (Track B concern, not a design gap). **Structure and Item are now designed** (D24, D25 — dedicated Structure slot, mana-usually/AP-sometimes funding; Item's generic `2AP: Equip`/`0AP: Un-equip` abilities, D30), and their funding **generalized into a single ownership-tree rule (D26, refined D27, terminology corrected D28)**: every permanent has at most one parent (a creature never parents another creature/structure — it always resolves directly to whichever Champion/Companion paid for it), and terrain now has three bond states (active/interrupted/unbonded) whose loss cascades to uncontrol any Structure built on it. Also narrows D22's Companion mana-sharing restriction. **2026-08-10 pass:** stress-tested the model against four concrete cards (a triggered-damage tower, an ability-granting/cost-modifying item, a scoped-uniqueness helmet, a flying "vehicle" item) and closed the remaining gaps — **every spatial permanent's location is now uniformly (terrain, layer)**, including a loose Item (D32, closing the old "where does a loose item live" question) and Structure, whose dedicated slot is now **1-per-layer instead of 1-per-hex** (D31, up to 3 Structures per cell, and a Below-layer Structure is hidden for free via the existing Below-concealment rule — no containment mechanism needed for "hidden in a building"). Legal cast-layer(s) are card-derived, essentially never a free choice (D32). Also decided: casting any permanent (Creature/Structure/Item alike) locks its target before going into the Aether, and an illegal target at resolution fizzles *that instruction only*, not the whole spell MTG-style (D33, relates to D16/`interaction-stack.md`) — the first piece of the still-unbuilt full Aether/trace model to get a concrete rule. Item has **no Life** (never a combat participant, removed only via an explicit effect) and loose Items are **exempt from layer capacity** (unbounded per (terrain, layer), D34). Explicitly considered and **rejected**: a general "containment" location (creature inside a Vehicle/Structure) — vehicle flavor (the Helicopter card) is achieved via ordinary Item ability-granting (ranged attack, flying, cost changes) instead, deliberately avoiding a second location system. **Also formalized: triggered abilities are a fundamental ability kind alongside activated ones (D35)** — universal across every Type (not a Structure-only pattern), available directly on Creature/Structure/Champion/Companion or grantable by an Item, and independently costable at *resolution* time via an optional "you may pay" clause rather than only ever being free or only ever costed upfront. See `docs/rules/ownership.md`, `docs/rules/structures-items.md`, `docs/rules/interaction-stack.md`. **Companion** (D22) and **Terrain** (D8) were already fully resolved. **Remaining open item, not Item/Structure-specific:** whether a creature loses its *controller*, not just its funding, when the Companion that summoned it dies, and what an uncontrolled-but-living creature does (D29, parked in §9). Card ideas surfaced along the way live in `docs/cards/card-ideas.md`.
1. **Uniqueness / copy-limit rule (general).** Surfaced by Companions (expected to be one-ofs) but deliberately **not** a Companion-specific rule — design a general singleton/"legendary-type" mechanic (max copies in deck, max copies in play) that Companions, and any future card that wants the same restriction, just consume. Ties into the existing open "deck size / max copies" item in `overview.md` §6.
2. **The 8 colors.** Define each color's identity and its relationships to the others (allies/enemies, mechanical identity, philosophy) — aiming from the start for Mark-Rosewater-color-wheel quality, not a placeholder pass to redo later.
3. **Initial deck ideas per color.** Sketch what an early deck in each color wants to do — this should double as a seed for drafting actual starter cards.
4. **Fill remaining structural gaps:** Zones (`Hand/Library/Play/Aether`, D16), the three board layers (ground/above/below, D12), and **Graves** — currently undecided whether a Grave is a Zone or a board-layer/marker concept; needs to be pinned down.
5. **Gap sweep.** Once 0–4 land, take stock of what's still missing from the ruleset overall.
6. **Pessimistic-default audit** — sweep D1–D35 *and* everything decided in steps 0–5 for generous defaults that should become positive keywords (e.g. D8 "creature blocks mana flow" → a **"Blockade"** keyword). Deliberately last: 0–5 will introduce their own defaults, and auditing once at the end catches those too instead of doing the pass twice. Worklist: `docs/rules/history/pessimistic-default-audit.md`.

**Track B — implementation (M1 COMPLETE as of 2026-08-05):**
1. **Toolchain:** ✅ .NET SDK 10.0 (LTS) installed; Windows-native, VS Code + C# Dev Kit. Project renamed **Hex → Leyline** (see PLAN.md title).
2. **Agent / work breakdown:** solution scaffolded as `Leyline.RulesCore` (headless rules core + Perception, same assembly per architecture decision A1), `Leyline.Host` (Host abstraction, `LocalHost` for M1), `Leyline.Content.Json` (JSON card loading), `Leyline.SimHarness` (batch/interactive test harness), plus xUnit test projects. See `docs/architecture/history/decisions-architecture.md` A1–A5 for the boundaries this follows.
3. **Build M1 — ✅ done, all 6 slices, 64 passing xUnit tests:**
   - **Slice 1** (combat sandbox): board, creatures, Move/Attack incl. `!` AP cost, full event pipeline + query/modifier layer, full D3/D4/D13/D19 combat resolution with the one Combat-declare priority window, D15 defend-rule config toggle *(superseded 2026-08-09 — D15 resolved to a single `0*AP` rule, the toggle/`MatchConfig` removed entirely, see item 9 below)*, turn/phase machine, JSON content loader.
   - **Slice 2**: Champion as an attackable win-check target (D9) — Combat needed **zero changes**, confirming the ActorState-sharing design.
   - **Slice 3**: terrain/mana network (D8) — permanent bonding, conditional/path-blocked mana draw, `RefreshManaEffect` added to Beginning phase as proof "phases are data" holds.
   - **Slice 4**: Champion-as-actor via `ChannelActCommand`, sharing one Channel-used flag with Bond. Move/Attack needed **zero changes**.
   - **Slice 5**: hidden Below layer (D12/D19) — concealment, reveal-on-attack, re-conceal-on-move; one `Query.IsVisibleTo` rule shared by Perception and Combat targeting.
   - **Slice 6**: `LocalHost` hardening (5 negative-assertion tests in a project that never references RulesCore internals), `DeterminismReplayTests` (same log twice, and direct-vs-Host), a golden-path integration test, and an interactive Host-mediated REPL (`dotnet run --project tools\Leyline.SimHarness -- interactive`) alongside the existing batch V1-vs-V2 sim (`dotnet run --project tools\Leyline.SimHarness`).
   - Two real bugs were caught by tests during implementation (a state-based-check ordering bug that would've missed Champion deaths, and a missing Champion-AP-reset-to-zero) — both fixed, both now regression-tested.
4. **✅ Done (2026-08-06):** the modifier add/remove mechanism designed in `docs/architecture/continuous-effects.md` is now implemented — `IModifier`/`ModifierId`/`ModifierDuration`, `AddModifierEvent`/`RemoveModifierEvent`, `Modifiers/ModifierPipeline.cs`, and `Turns/ExpireModifiersEffect.cs` (End-phase cleanup). `IQueryModifier.Priority` was dropped in favor of append order, as the doc leaned toward. 7 new tests (71 total passing). See the doc's "Resume here next session" section for the updated status — the Rite/Spell casting pipeline is now the next open gap it flags.
5. **✅ Done (2026-08-07):** the Champion-economy doc/code drift flagged below is resolved. `ChannelActCommand`/`ChannelUsedThisTurn`/`ChampionPipeline` are gone; the Champion now refreshes AP every Beginning phase exactly like a creature (`RefreshApEffect`, no type-check) and acts directly via the normal `MoveCommand`/`DeclareCombatCommand` — no special "become able to act" command. Bond is now a real ability (`champion.bond`, gated through `AbilityIds` like `core.move`/`core.attack`, not a hardcoded type-check) costing `2*AP` — D9's `*` once-per-turn cost flavor, newly modeled as `ActorState.OncePerTurnActionsUsed` + `Query.CanUseOncePerTurnAction`, reset each Beginning phase by `ResetOncePerTurnActionsEffect`. Also added a shared `IHasCardDefinition` interface (`CreatureState`/`ChampionState`) to collapse `Query.cs`'s duplicate per-type arms — anticipates Structure needing the same trait per `continuous-effects.md`, without literal `ChampionState : CreatureState` inheritance (Champion is a distinct top-level card type per §3.2, not a Creature subtype). Left as explicit follow-up, not invented here: the network-active/collapsed Move/Attack cost differential and the `5*AP` Draw action (needs Hand/Library, not built yet) — both still just use the shared creature defaults (`1AP`/`3!AP`). 71 tests passing (5 Host + 66 RulesCore).
6. **✅ Done (2026-08-07):** M1.5 built — new `Leyline.Scenarios` (scenario loader) and `Leyline.DebugUi` (ASP.NET Core minimal API + a plain-JS SVG hex-board UI) projects; `View`/`ActorView` extended with `Name`/`Kind`/`Attack`/`MaxLife`/`MaxAp`/`Mana`/`Winner` since presentation is exactly what Perception exists to feed. Usability pass same day: drag-and-drop move (client-side hex pathfinding, walks one real `MoveCommand` per hop) and drag-onto-enemy-to-attack, plus server-side auto-resolution of priority windows (`GameSession.AutoResolvePriorityWindows`) since M1 ships zero instant-speed content — the only ever-legal response is Pass, so auto-passing removes pure clicking friction without changing engine rules, and self-corrects the moment real instant-speed cards exist.
7. **✅ Done (2026-08-08):**
   - **Real bug fixed:** opposing creatures could share a hex (D12's capacity-3 room was never ownership-scoped). `Query.CanOccupyLayer` now requires a layer's occupants to all share one owner; used by Move and by the new summoning legality below. Regression-tested.
   - **Scenario format redone as hand-editable text** (`docs/tools/scenario-format.md`), replacing the original JSON shape — line-oriented, `key=value` tuning fields, `# comments`, `line N: ...` error messages. Added a `bond <owner> <q>,<r>` declaration (pre-bonds terrain before the first Beginning phase runs) so a scenario can start with live mana instead of requiring a real Bond + a full turn cycle first.
   - **D16 Zones — a deliberately partial slice, not the full model:** `PlayerState.Library`/`Hand` (`List<CardDefinitionId>`, unshuffled — deterministic "draw exactly this card next" beats realism for a testing tool), the Champion's `5*AP` **Draw** action (`Champions/ChampionPipeline.cs`, same `*`-flavor pattern as Bond), and **casting** (`Spells/SpellPipeline.cs`): `CastCreatureCommand` summons onto bonded **and connected/producing** terrain (D20 — reuses the same set mana itself draws from) with room on the Ground layer, `CurrentAp` starting at 0 (summoning sickness, D14's pessimistic default, free from the same zero-until-refresh pattern the Champion itself already used); `CastRiteCommand` resolves one of two hardcoded effects (`Spells/RiteEffectIds.cs`: damage, heal — a new `HealEvent`) and leaves no permanent. **Explicitly not built:** the full Aether (unified stack/graveyard, traces, fade windows, un-summon vs. kill) — a separate, much larger design surface nothing in the engine implements yet; a cast Rite just resolves and vanishes rather than leaving a tracked trace.
   - `CardDefinition` gained `Type` (`CardType`: Creature/Champion/Rite), `ManaCost`, `EffectId`/`EffectAmount` as trailing-optional fields (every existing call site kept compiling unchanged); `Leyline.Content.Json`'s DTO now actually maps the `Type` field it always had but ignored.
   - `Leyline.DebugUi` updated to match: Hand shown per-panel (own contents; D7's "hand size public, contents private" — the opponent's `HandView.Cards` is `null`, only `Count`), Library+Hand for both players in the true-state panel, `Draw`/`Cast*` buttons via the existing generic legal-command list (no new interaction pattern needed).
   - 96 tests passing (5 Host + 82 RulesCore + 9 Scenarios), all new engine behavior covered: occupancy, Draw (cost/once-per-turn/empty-library), Cast Creature (mana/target legality/summoning sickness/occupancy), Cast Rite (damage/heal/lethal-triggers-win-with-no-special-casing/visibility-gated targeting), plus scenario-format parsing including the new `bond` declaration.
   - Verified live in-browser (`casting-demo.scenario`, a new example scenario with a pre-bonded Champion and creature+rite cards already in hand): cast a Firebolt Rite for exact expected mana spend and damage, hand/library updated correctly across the P1/P2/true-state panels.
8. **✅ Done (2026-08-08 → 08-09), a live-playtesting pass on M1.5 driven by actually using it:**
   - **D9 Champion movement tightened twice, same window.** First pass: connected-to-network Move became `2AP`/restricted vs `1AP`/free once disconnected, plus a new `0AP` **Collapse Network** ability (drops every bond outright, not a pause). Playtesting the same day caught that the first "restricted" definition was still too loose (a Champion could drift one hex past its actual bonded tiles while nominally "staying connected") — tightened again same-day to: a connected Champion may only step onto hexes that are **themselves already bonded**. `Query.IsBondedAndReachable` is the shared predicate both the connectivity check and the move-legality check now use.
   - **Mana is a proper snapshot, and Champions start pre-bonded.** `RefreshManaEffect` already only fired once per Beginning phase (confirmed, regression-tested — a mid-turn bond doesn't credit mana until the *next* Beginning phase, same pessimistic shape as summoning sickness). Added: both Champions now start match setup already bonded to their own home tile (`bonds` param / a scenario's `bond` line), so 1 mana is live turn 1 instead of costing a full Bond-then-wait cycle.
   - **Real information leak fixed (D18):** `ViewProjector.Project` was reporting *every* player's live mana balance to *every* observer, unconditionally — direct contradiction of D18's "live mana balance is the one hidden standing quantity" rule. `PlayerManaView.Mana` is now `int?`, populated only for the observer's own entry. Regression-tested (`ManaVisibilityTests.cs`).
   - **Docs reorganized and audited against the running code** (the user's explicit ask, not just incidental cleanup): the one stray doc file living in `src/` (`ScenarioFormat.md`) moved to `docs/tools/`; a full pass over `docs/rules/`+`docs/architecture/` turned up several real doc/code contradictions — `overview.md`/`history/decisions.md` D21 claimed a priority window opens after *every* action when the engine only ever opens one at Combat-declare; `champions.md`/`glossary.md`'s Champion-movement description was two revisions stale; `continuous-effects.md` still described the Rite/Spell pipeline as unbuilt after it shipped; `history/knowledge-capture-plan.md` pointed at output-doc filenames that were never created; `architecture.md` didn't flag the debug UI's `/api/truestate` endpoint as a second True-State exception that must never reach a shipping build. All fixed except the priority-window gap, which the user confirmed is intentional (sorcery-speed-only test environment for now) and left alone.
   - **Debug UI polish:** hovering a legal-action button (e.g. "Bond terrain at (1,0)", "Target: Champion (P2)") now highlights the hex/actor it resolves to on that panel's board, before you commit to it.
   - **D15 (defend cost) resolved to `0*AP`** — free, but at most once per turn per actor, completely decoupled from remaining AP. Replaces the Exhaust/DeleteDefendOnce config toggle: Exhaust had a real bug, not just a feel-bad — since default Attack already exhausts the attacker, the *first* combat between two creatures left both sides at 0 AP, but AP only refreshes on its own controller's Beginning phase, so the attacker stayed defenseless through the *entire* following opponent turn while the defender refreshed almost immediately — attacking first was strictly worse than being attacked. `0*AP` fixes this (Defend never reads AP) while keeping V1's per-turn cap (unlike flat free-and-unlimited, which also fixed the bug but dropped the cap). A card can still deliberately consume an actor's Defend as part of a strong ability's own effect (the MTG "tap cost" flavor) — opt-in card data, not a base rule, and it never applies to Attack, so the original bug can't come back through the front door. Since D15 is no longer a toggle, `DefendRuleVariant`/`MatchConfig` were removed entirely (had exactly one field, used nowhere else) — rippled mechanically through ~15 files (every test fixture, the scenario format's `defendRule` keyword, `Leyline.SimHarness`'s defend-variant-comparison tool, repurposed into a plain batch runner). Verified live in-browser: an attacker at 0 AP successfully defends the very next enemy turn instead of dying to a free undefended hit.
   - **New `docs/rules/history/playtest-variants.md`** — a registry (distinct from `history/decisions.md`, which only records the *current* rule) for rejected-or-not-yet-decided rule alternatives worth an actual playtest later. Seeded with D15's rejected `1!AP` variant (with the failing scenario as the documented reason) and a new not-yet-implemented alternative the user proposed (Defend goes back to AP-gated, paired with a second small AP-refresh pulse at the *opponent's* Beginning phase and a new `x!!AP` "double-exhaust" cost flavor that opts out of it) — deliberately parked, not adopted, since it touches the core AP-refresh timing rule.
   - 105 tests passing (5 Host + 91 RulesCore + 9 Scenarios).
   - **Commit status:** the M1.5 build through the D15 defend-rule change and its docs is pushed to `origin/main` (4 commits: "Build M1.5 debug UI, scenario system, and Champion Draw/Cast/Collapse pipeline", "Reorganize docs/ and reconcile rules docs with the M1 implementation", "Resolve D15: Defend costs 0*AP, free but once per turn", "Document D15's resolution and add a playtest-variants registry"). Nothing outstanding uncommitted.
9. **No pressing gap.** Candidates for whoever picks this up next: generalize the Rite-effect placeholder (`RiteEffectIds`) into the real card-effect system `continuous-effects.md` still calls undesigned; extend casting to Structure/Item once Track A fleshes those types out; start on the actual Aether/trace model (D16) now that Hand/Library prove the zones pattern works; or actually playtest the two rule variants now parked in `docs/rules/history/playtest-variants.md`. None of these block M1.5 playtesting, which is fully usable end to end (move, attack, bond, draw, cast, defend).

---

## 9. Open questions parked for later
- **Theme/setting** — deferred by choice; design mechanics first, skin later. Faction *structure* (not flavor) is designed now.
- **Monetization / business model** — relevant to "serious indie" but not to the prototype.
- **Art pipeline & style** — after core loop is fun.
- **Progression / metagame** (unlocks, ranked, collection) — post-M4.
- **NPC/mission content & uncontrolled-creature behavior** — surfaced 2026-08-09 while designing the ownership tree (D26–D29): a creature necessarily loses its controller if the Companion that summoned it dies mid-chain, but unlike inert Terrain/Structure, a creature still holds its own AP (self-paid, D10) — what does an uncontrolled-but-alive creature actually *do*? Ties into a bigger, deliberately-deferred idea: mission/PvE content with NPC-like creatures running an AI-driven routine independent of either player, where an orphaned uncontrolled creature is one possible source of such an NPC, and killing one could be a bonus objective. **A full draft answer now exists (Neutral Permanents — control states, Neutral Phases, Behavior algorithms) but is not yet discussed or adopted** — see §8 Track A's top item and `docs/rules/history/neutral-permanents-draft.md`, the next session's opening topic. See `docs/rules/ownership.md`.
- **Is Ownership a top-level concept in its own right, or only meaningful on the Board?** Surfaced 2026-08-11 while sketching top-level concepts for `docs/rules/` (Zones, Card types, Temporal structure, Ownership, Resources). The current ownership tree (`ownership.md`) is defined entirely over Play-zone permanents (Creature/Structure/Item/Terrain parent chains) — it's unclear whether "ownership" means the same thing, something different, or nothing at all in Hand/Library/Aether. An Aether trace's "owner" is presumably just whoever cast the spell/activated the ability, not a parent-chain climb. **Control** may turn out to be a Board-only concept. **Payment** (today's "climb to the nearest node holding the resource" walk) may need to split off from a broader, zone-general **responsibility** concept — worked example: an ability reads "gain 10 life; as long as this can be traced in the Aether, pay 1 life at the beginning of turn" cast by a Companion — whose life pays that upkeep? Keeping Ownership as its own top-level concept for now rather than forcing an answer; revisit once more of the ruleset (especially the Aether/trace model) is fleshed out. See `docs/rules/ownership.md`.
- **Where does Resources (mana/AP as abstract quantities) belong among the top-level concepts?** Same 2026-08-11 pass. Unclear whether Resources deserves to be its own top-level concept (cards games like MTG treat Mana/Life as general concepts independent of zones/card-types) or whether it folds into Ownership/Payment once that question above is resolved — the terrain-network/bonding mechanics are Board-zone content either way (a separate observation from that same pass), but the abstract "what mana and AP *are*" piece still needs a home. Keeping Resources as its own top-level concept for now, structure TBD once a larger portion of the ruleset is in place. See `docs/rules/economy.md`.
- **Zone count reaffirmed at four (Hand/Library/Play/Aether); two-view implementation split recommended.** Surfaced 2026-08-12, continuing the 2026-08-11 top-level-concepts pass: seriously considered collapsing Hand, Library, and Aether into one unified "temporal" zone (a hand-drawn timeline visualization prompted it) — decided against it (`docs/rules/history/decisions.md` D36). The decisive test: would card text ever reference the merged zone by its own name, rather than always naming Hand/Library/Stack/Trace specifically? No — so merging would rename rather than simplify. **Recommended for the implementation regardless of zone count:** organize the engine/docs around two views over the same zone list — a **spatial** view (Play) and a **temporal** view (Hand, Library, Aether) — as an organizational grouping, not a claim that there are fewer zones.
- **Card/permanent property inventory — the prerequisite for Q2/Q3 (which objects live in each zone; how they transform across zone boundaries), agreed 2026-08-12, not yet started.** A first attempt at Q2/Q3 (a full 12-cell zone-transition matrix) moved too fast — it skipped a more foundational question the user raised: card/permanent properties split into **(a) type-independent** (e.g. mana cost — present on every card) vs. **type-dependent** (e.g. Attack/Life/AP on a Creature, absent on a Rite) — a transition model has to work identically across types, so this split must be explicit; **(b) properties fixed at cast time**, when a generic card template becomes a specific cast instance (targets chosen, modal choices like "choose one: target creature loses 2 Life / loses 2 AP", flexible/X-cost mana paid) — not necessarily fully designed yet, but real enough to matter for the transition model; **(c) properties dynamic *by rule design*, scoped to a zone** (a Play permanent's board location and current Life; a Champion/Companion's bonded terrain) — distinct from properties that vary only via a specific card's special effect. **Concrete hard case flagged:** token bounce — MTG's answer ("a bounced token is destroyed, since a token has no other zone to exist in") was explicitly called out as not necessarily the right answer here. **Agreed method:** for each card/permanent type × each zone, write down which properties exist there (working through hard cases — tokens, counters — explicitly); only *then* determine what's necessary for an object to legally cross a zone boundary. **Explicitly deferred beyond that, not scheduled:** a pass on Play's internal "substructure" boundaries — living creature ↔ grave, structure ↔ ruin — flagged as a specific worry (whether these are actually the clean in-zone state changes D16 assumes). **Do not resume from the 12-transition matrix** — start from the property inventory instead.
