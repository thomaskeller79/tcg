# Implementation Plan — Rules Sync (2026-10)

*Brings the engine and the debug UI up to the current ruleset (`docs/rules/`, D1–D122; the guess review added D123–D127) so a full match can actually be played. Working doc for Track B (`PLAN.md` Track B item 7): what is missing, the order it gets built in, and every educated guess made where the rules leave a gap. Guesses and questions are labelled (G1, Q1, …) so they can be answered by label.*

**Branch:** `track-b-rules-sync` · **Started:** 2026-10-08

---

## 1. Gap analysis — rules vs. code at the start

| Area | Rules (current) | Code at start |
|---|---|---|
| Object model | Card / Trace / Permanent, one unified monotonic ID + timestamp, `source` links (D93, D97) | `CardDefinitionId` lists in zones (no card objects), `ActorId` for actors, `TraceId` separate, no timestamps, no `source` |
| Permanent types | Champion, Companion, Creature, Structure (Actors); Item, Remnant, Terrain (Objects) | Creature and Champion only |
| Terrain | One Terrain permanent per hex (D100), Element-producing, bond record = parent, drawn-this-cycle flag (D77), move-cost static | `Cell.Terrain` string, bond set on the Champion |
| Slices | Root / Ground / Sky, capacity 3, entry = every occupant shares your controller (D86, D119); two Structure slots (Ground, Root) | `Level` Surface/Air/Underground, any enemy blocks the level |
| Ancestry | Payment walk and Controller walk; Neutral controller (D54, D81, D83) | Owner field only |
| Mana | Pool per Champion and per Companion, credited live per producer (D77), Elements (D51) | One generic number, recomputed at Beginning/Bond |
| AP costs | `x`, `x!`, `x~`, `x*` flavors (D116) | `x` and `x!` only; Defend `0*AP` |
| Turn structure | 4 seats per round (A, Neutral A, B, Neutral B), Setup as A's first Beginning (D114) | 4 seats exist but neutral turns are empty; no Setup |
| Aether | Every activated ability goes through Pending (D45, D75); Speeds Slow/Quick/Reactive/Instant; Physical Traces with 0 Past residency; Instants auto-advance Now (D92) | Only Creature/Spell casts go through Pending; no Speeds |
| Casting | Legality → Pay → Choose → Target → Pending; a transaction (D70, D91, D94); D68 redirect | Validate + pay + push |
| Resolution | Per-instruction checks (identity, visibility, protection, verb applicability); immediate consequences (D112); triggers collected and ordered APNAP/ID/printed/event (D92) | Whole-trace resolution; state-based checks |
| Combat | Attack = Physical Trace naming terrain + Slice + entity (D115); Defend = Quick ability adding defenders bit by bit (D117); defended/undefended split at resolution | Declare-defenders step, combat object, one window |
| Network | Knotting keyword (D121); pause vs. cut, theft (D122); realm lock; Collapse Network | Any enemy pauses; no cut, no theft |
| Remnants | Fallen Creature/Companion leaves a Remnant (D110); Structure leaves nothing (D111); draw on simultaneous Champion fall (D113) | Actor removed |
| Perception | Root hidden except controller and D67 proximity; Hand and Discard private (D71); live mana hidden | Underground hidden + "Located" flag; Discard public |
| Neutral | Behavior, neutral seats acting (D56–D65, D82–D84) | Neutral turns pass immediately |
| Debug UI | Two map views, both layouts agreed (Root: Root creatures on the three G corners, the Root Structure in the centre, Remnants and Items on the left and right of the ring; Ground+Sky: Ground creatures on three alternating corners, Sky on the other three, the Ground-slot Structure in the centre, Remnants and Items on the ring, a mark for terrain with abilities), Mind/Aether panels | One small-hex board, no Slices drawn |

## 2. Build order

Each phase ends with its own tests passing and a commit. The engine is rebuilt around the new object model in phase 1; later phases add systems on top.

| Phase | Content | Verified by |
|---|---|---|
| **P1 — Object model** | Unified `ObjectId` + timestamp; Card/Trace/Permanent objects; zones; Terrain permanents per hex; Slices + Structure slots; card definitions with abilities as data; JSON content; Ancestry walks (payer, controller); 4-seat turn order; AP flavors; Element mana pools with live crediting | unit tests on walks, pools, turn order |
| **P2 — Aether** | Activation procedure as one transaction (legality, pay, target, enter Pending); Speeds; priority + Now; Instants; Physical Traces; Past fade; per-instruction resolution checks; immediate consequences; trigger collection and ordering; instruction verbs (damage, heal, draw, fell, destroy, create, raise, bounce, flicker, gain AP, stat buffs, unbond) | tests per speed, per verb, trigger order (Cinder-Verdict-style) |
| **P3 — Network** | Bond (Champion `2*AP`, Companion `3*AP`), Knotting, pause, cut, theft, realm lock, Collapse Network, Structure/Remnant control from terrain | tests per D121/D122 rule |
| **P4 — Movement & Slices** | Move (terrain move cost, realm cost doubling), Ascend/Descend, Flying/Subterranean Slice filters, entry rule, mixed Slices, D68 redirect, Root perception + D67 proximity | tests incl. hidden-occupant redirect |
| **P5 — Combat** | Attack trace + legality, Defend (Quick, `1~AP`/doubled for bonded roots), Defender keyword, defended/undefended resolution with a damage-split decision, retaliation, Ranged | combat-scenarios.md S1–S8 as tests |
| **P6 — Permanents** | Casting Creature/Companion/Structure/Item/Spell; Equip/Un-equip; Remnants, raise; Haste X; Companion pool/Bond; cascades when a node ceases to exist | tests per type |
| **P7 — Setup & maps** | Map format (layout, start tiles, home/neutral ground, void), terrain decks, Setup S1–S9, shuffle, opening hand | setup test, deterministic replay |
| **P8 — Neutral** | Neutral turns refresh/act; Behavior `Aggressive toward X`; Companion-loss cascade assigns it | neutral-turn tests |
| **P9 — Perception & Host** | View rewrite (per-observer), legal commands enumerated against the actor's own view, trace redaction | perception tests, Host tests |
| **P10 — Debug UI** | Large hexes with the Ground+Sky layout and a Root view; Mind/Aether panels; step-by-step activation (pay → target → confirm/abort); priority/pass; decisions (damage split, redirect); content library + demo match | API smoke tests + driving the UI in a browser |
| **P11 — Harness & docs** | SimHarness random policy on the new engine; `architecture.md`, `continuous-effects.md`, `scenario-format.md` brought in sync; PLAN.md Track B items 1–4 removed as done | full test suite, sim run |

## 3. Educated guesses

Made where the rules leave a gap. Each is the simplest reading that keeps a match playable; all are easy to change.

- **G1 — Mana colors (item 37).** Replaced by D125 (`resources-terrain.md` §Mana costs and payment). Basics produce 1 of their Element.
- **G2 — Priority order.** Confirmed as D123 (`interaction-stack.md` §The primitive).
- **G3 — Quick with empty Pending.** Confirmed as D124 (`interaction-stack.md` §The primitive).
- **G4 — Priority with empty Pending.** Confirmed as part of D123 (`interaction-stack.md` §The primitive).
- **G5 — Beginning/End triggers** resolve without priority, like Setup's S9 (overview §3: "no priority this phase to start").
- **G6 — Defend eligibility includes the Champion and Companions** (creature-type permanents); Structures never defend (D107: a Structure never fights).
- **G7 — Ranged N (item 45).** Replaced by D127: Ranged a-b, and a defender must reach its attacker.
- **G8 — Structure as attack target.** A Ground-slot Structure counts as being in the Ground Slice (and is reachable by Sky → Ground); a Root-slot Structure is in Root.
- **G9 — Move cost.** A Move costs the destination terrain's move cost (base 1). A Champion or Companion pays double while it stays connected — onto terrain it bonded itself, while its own tile is bonded (D9's `2AP`). A connected Champion can only move onto such terrain anyway (realm lock).
- **G10 — Companion movement (item 38).** Replaced by D126: a Companion has Collapse Network and the realm lock, like the Champion.
- **G11 — Bond reachability.** A root may bond its own tile if it isn't bonded by itself; otherwise a terrain adjacent to its own tile or to any terrain it bonded that has an unknotted path to it. The target must be unbonded, or bonded by an enemy root and currently paused by a knot (theft).
- **G12 — Summon target.** A permanent card may be cast onto a terrain the caster controls (bonded by the Champion or one of its Companions, with an unknotted path), into the Slice its Slice filter names, with room.
- **G13 — Mulligans (setup.md Open question 2).** Not implemented: every player keeps. Setup otherwise follows S1–S9.
- **G14 — Void terrain.** Holes in a Layout are Void terrain: produces nothing, can't be bonded or entered.
- **G15 — Neutral-ground population.** Two algorithms: `fixed` (the Map lists each hex's terrain card) and `random` (uniform draw from a pool the Map lists, with replacement).
- **G16 — Damage split.** When there is a single legal recipient, the whole Attack goes there without asking. Otherwise the attacker's Champion submits a split as a decision while the Attack resolves (a Neutral attacker's Behavior picks the lowest-Life target, ties by ID).
- **G17 — Trace visibility.** A trace is shown to an observer only if its source permanent (for an Ability trace) is visible to them; otherwise it shows as "a hidden action". Bound targets the observer can't see are shown as "a hidden object". The priority window itself still opens (Track B item 6).
- **G18 — Opening hand 5, Champion AP at Setup 4, Trace Duration 5** (the documented placeholders).
- **G19 — Behavior `Aggressive toward X`.** Each Neutral creature with it, in its neutral turn: attack a visible permanent of X if one is in reach (lowest Life, then lowest ID); otherwise move one hex closer to the nearest visible permanent of X. It defends when it can.
- **G20 — Trace parent for a Companion's own ability** is the Companion (the payment walk from it stops at itself for mana).
- **G21 — Healing** stops at max-Life; a permanent Life buff raises max- and current-Life together.
- **G22 — Drawing from an empty Library** does nothing (no decking loss).
- **G23 — Which side a Knotting permanent is on.** For deciding whom it knots, a Structure's side is read through its terrain's bond record even while that terrain is paused (the live controller would depend on the knot itself).
- **G24 — "Target creature"** means any creature-type permanent: Creature, Companion or Champion. A card can still be written Creature-only later.
- **G25 — Triggered abilities take no targets** in this build: every prototype trigger is targetless ("draw a card"). A targeted trigger needs a "choose its targets as it enters Pending" step.
- **G26 — Refresh and the `~` lock.** A Champion-controlled permanent refreshes its AP and `*` uses in its controller's Beginning, a Neutral one in its neutral seat's Beginning; every `~` lock ends at the end of each turn.

## 4. Questions for the user

Collected while building; nothing below blocks the build (each has a guess above or is parked).

All six are answered (D123–D127, and the Root view layout in §1). The remaining guesses in §3 are reviewed one by one (`PLAN.md` Track B item 7).

## 5. Paused features and gaps

Nothing had to be paused for lack of a guess. Not built in this pass because no prototype card needs them yet; each is filed as a `PLAN.md` Track B item:

- **Deception (D18):** faces, claims, Mimic, collapse; concealment effects beyond the Root default (Mist, Submerged, D118).
- **History track (D98)** and card text reading it ("attacked this turn").
- **Remand, un-summon, Future-zone delays** (D69, D16, D38).
- **The effect form outside the prototype scope (D104):** targeted triggers (G25), choices at resolution, modes, `X` costs, cost branches.
- **Mulligans** (G13), and Structures a terrain card brings at Setup (`resources-terrain.md` Open question 3).
- **Behaviors beyond `Aggressive toward X`.**

## 6. Status

P1–P11 are built and tested, plus the follow-ups D123–D127 from the guess review: 97 RulesCore tests (including random-play fuzzing over 8 seeds and a deterministic-replay test), 3 Host tests, 4 scenario/content tests, and a headless-browser test of the activation wizard (`tools/Leyline.DebugUi/wwwroot/uitest.html`). Run the debug UI with `tools/Leyline.DebugUi/start.ps1` and load `demo-match` at http://localhost:5299.
