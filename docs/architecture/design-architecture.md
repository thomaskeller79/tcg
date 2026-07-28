# Architecture — Components & the Host Boundary

*How the game is built, as opposed to what it is (see `docs/rules/` for that). This doc is the standing reference for the component breakdown and "what runs where" — the answer to the central Track B question: given a client-server architecture for online 1v1, what must also run fully offline for solo/hotseat?*

**Status:** First draft · **Date:** 2026-07-28 · Decisions: A1–A4 (`decisions-architecture.md`)

---

## 0. Verdict on the original PLAN.md diagram

`PLAN.md` originally sketched:
```
Rules Engine → { Godot UI, AI, Netcode, Test suite } (four symmetric siblings)
```
This is superseded. Three problems it has: (1) UI and AI are not peers of Netcode — both are *seat-controllers* sitting on one side of a boundary (the Host) the old diagram doesn't have at all; Netcode is wiring **inside** one specific Host implementation, not a fourth sibling consumer. (2) There is no Host, so nothing marks the seam between "same process" and "network process" — without it, offline-capable + online-capable from the same core isn't actually representable. (3) Perception (pillar 6) and Meta-progression (D1) are both load-bearing and both missing.

## 1. Corrected top-level shape

```
Test/Sim harness ──(direct calls, dev/CI only, never shipped)──▶ RULES CORE
                                                                       │ True State + true Events
                                                                       ▼
                                                              PERCEPTION LAYER
                                                        (same assembly; per-observer
                                                         projection; faces/claims/D18)
                                                                       │ per-observer View + Events
                                                                       ▼
                                                              HOST  (interface)
                                                     ┌─────────────────┴─────────────────┐
                                            Local/Embedded                      Remote/Networked
                                          (in-process call)              (proxy ↔ Netcode ↔ Server process,
                                                                          which wires its own Rules Core +
                                                                          Perception the same way, no seat
                                                                          controller of its own)
                                                     │                                   │
                                       ┌─────────────┴─────────────┐                     │
                                       ▼                           ▼                     ▼
                              Seat-controller:             Seat-controller:     (remote seat-controllers,
                             Human/UI (engine of choice)         AI              same two kinds, over the wire)

Card Data & Content Pipeline (static content Rules Core reads at startup — see card-data-and-editor.md)
Platform Services (per-platform account/achievements adapter — sits beside Netcode/Meta-progression, §7)
Meta-progression (outside the match core; touches it only via a resolved Loadout at match start):
  Offline solo/hotseat: no progression.  Online solo: replay-verified.  Online 1v1: live-hosted (inherently secure).
```

## 2. Components — responsibility, boundary, interface

### 2.1 Rules Core
The sole authoritative simulator: state, event pipeline (intent → pipeline → event), query/modifier layer, replacement effects, legal-move generation, combat, win-check, zones. Base rules are themselves low-priority effects in this pipeline (pillar 5). Reads the Card Data Content Repository as static input; never touches Meta-progression (consumes a resolved Loadout only, D1); never references an engine, sockets, or a serialization format.
**In:** validated Commands from a Host. **Out:** True State + true Events, to Perception only (or to the Test/Sim harness).

### 2.2 Perception layer
Lives *inside the same library as Rules Core*, not as a separate service or inside Host/Netcode. It's the same query/modifier engine already used for cost/legality, applied to a perception axis (D18's own framing); it must produce identical projections regardless of Local vs Remote Host, which only holds with exactly one implementation both call; it needs full read access to True State and every active face/claim, arguing against putting any boundary in front of it.
Pure `(True State, faces/claims, observer) → View`, plus per-observer Event projection, plus the D18 claim/face/collapse machinery. Never decides legality (Rules Core's job) — only decides what a seat is told.

### 2.3 Host abstraction
The seam answering "what runs where." A seat-controller submits a Command tagged with its seat id; Host validates ownership, forwards to Rules Core, asks Perception to project for that seat, returns that seat's View + Events. Carries priority-window signaling (whose priority, deadline) as part of its contract, not as UI-only plumbing — every seat-controller must implement respond-or-pass as a first-class interaction (see Risks §5.1). Never returns True State or another seat's View; never accepts a Command for an unauthorized seat.
- **Local/Embedded Host** — same process, in-process call. Used by hotseat and solo-vs-AI (and by online-solo's replay-verification path, which runs the actual match on a Local Host — see §6.3).
- **Remote/Networked Host** — client-side proxy implementing the same interface, forwarding over Netcode/Transport to a Server process. Used by online 1v1.

### 2.4 Seat-controller abstraction
Given a Host + seat id, decides the next Command (including "pass priority"), consuming only its own View+Events. Never reads another seat's data or True State (except a flagged, contained AI shortcut, §5.3); never branches on Local vs Remote Host.
- **Human/UI seat-controller** — glue between the presentation engine's render/input and Host. The only component that should know which rendering engine is in use.
- **AI seat-controller** — same shape, driven by search/heuristics (later MCTS/minimax); must eventually reason from its own View, not True State.
- A future networked human is *not* a third component — just a Human/UI seat-controller plugged into a Remote Host instead of a Local one.

### 2.5 Netcode/Transport
Boundary and contract only in this pass; wire protocol deferred to M6. Carries serialized Commands client→server and View+Events server→client, plus priority-window turn-taking metadata and connect/reconnect/seat-binding. Carries no True State, no gameplay logic; sits entirely underneath the Remote Host and the Server's connection handling.

### 2.6 Server process
Hosts one live Rules Core + Perception instance per match, fronted by Netcode, forwarding Commands in and each seat's own View+Events out. No seat-controller runs server-side (no server-hosted AI, decision A3). "Server" really names two different-lifecycle services worth keeping logically separate even if co-deployed early: an *ephemeral per-match* Rules-Core host, and a *persistent* account/meta-progression store.

### 2.7 Meta-progression layer
Server-authoritative store (canonical XP/unlocks/level-bands/paths). No client-side mirror or offline claims exist (see §6 for why). Touches Rules Core only one-way, via a resolved Loadout at match start — D1's firewall is unchanged. See decision A4 (`decisions-architecture.md`) for the replay-verification mechanism that lets solo play earn progression without a live connection.

### 2.8 Presentation/UI
Renders the Human seat-controller's current View, captures input, runs no shadow simulation of its own (decision A2 — no client-side prediction). Never reads True State; never branches on Local vs Remote Host; never reveals what the View marks hidden. Logically distinct from the Human seat-controller (which talks to Host) so that contract stays testable without a rendering engine in the loop. This is also the only component pinned to a specific engine (Godot or otherwise) — see `decisions-architecture.md` A5 (portability).

### 2.9 Test/Simulation harness
The one sanctioned bypass of Host, for unit tests, batch simulation/fuzzing, replay-and-verify, balance analysis. The only place licensed to read True State freely. Must never ship in player-facing binaries. Should also gain a mode that drives Rules Core through a real Local Host + Perception, since Perception is otherwise the least-tested path in the whole design.

### 2.10 Card Data & Content Pipeline
See `card-data-and-editor.md` for the full writeup. Summary: a schema + plain JSON data files + a Content Repository Rules Core queries at startup, plus a separate Card Editor authoring tool. Kept engine-agnostic on purpose (ties to portability, §6/A5) and kept distinct from Meta-progression (this component answers "which cards exist"; Meta-progression answers "which cards this player may currently use").

### 2.11 Platform Services adapter
One implementation per target platform (Steam, Xbox Live, PSN, Nintendo Online, etc.), providing account/identity, achievements, and entitlements. Sits beside Netcode and Meta-progression; Rules Core/Host/Perception stay fully unaware it exists. Not yet designed in detail — flagged here so a future console/storefront port doesn't require reopening the core architecture, only adding an adapter.

## 3. What runs where — mode matrix

| Component | Hotseat | Solo vs AI | Online 1v1 |
|---|---|---|---|
| Rules Core + Perception | 1 instance, embedded in the client process | 1 instance, embedded in the client process | 1 instance, in the **Server** process only; each client ships the library but it sits dormant |
| Host | Local/Embedded | Local/Embedded | Remote/Networked on both clients; server-side wiring is functionally a "Local Host" internal to the Server process |
| Seat-controller #1 / #2 | Human/UI + Human/UI, same process, alternating whose View is rendered | Human/UI + AI, same process | Human/UI (or AI) per client, each through its own Remote Host |
| Netcode/Transport | not instantiated | not instantiated (unless replay-verifying progression post-match, §6.3) | active on both clients + Server |
| Server process | not instantiated | not instantiated (except the async replay-verification endpoint, §6.3) | one process per match |
| Meta-progression | none (offline = no progression) | none while offline; replay-verified if online (§6) | server-authoritative, live |

**Hotseat recommendation:** one Host instance, two Human/UI seat-controllers, gated by a mandatory pass-device/hide-screen confirmation before swapping which seat's View is rendered — not a degraded-fidelity special case. This keeps hotseat on the *exact same* Host contract as every other mode; the pass-device step is a Presentation-layer concern only, never a fork of Rules Core/Perception/Host. Residual risk (not architecturally solvable): a co-located pass-device step can still be defeated by two humans agreeing to peek — a product/UX limitation, not a bug to fix here.

## 4. Card data and engine portability (why this matters here)

Rules Core, Perception, Host (both implementations), Netcode, the AI seat-controller, and Meta-progression are all engine-agnostic C# with no rendering-engine dependency by design. An engine swap (e.g. Godot → Unity) should be contained to the Human/UI seat-controller and Presentation/UI — **provided** Card Data (§2.10) stays in plain, engine-neutral files (JSON) rather than engine-specific resource formats. This single constraint is what keeps the portability property real rather than accidental.

## 5. Risks / tensions carried forward

1. **Priority/stack respond-or-pass is a Host + seat-controller contract requirement**, not a UI nicety — build it into the View+Events shape and the seat-controller interface now (well before M6), or Netcode will need to reopen the "one uniform interface" the Host exists to protect.
2. **The Host's View+Events shape must reserve room for priority-window turn-taking now** even though the wire protocol is deferred — designing it late means redesigning the interface everything already depends on.
3. **AI-reads-True-State is a contained but real anti-cheat-shaped hole** in solo vs AI. Gate it structurally (a distinct code path/assembly), not by comment, so it can never accidentally get wired to a Remote Host in online play.
4. **Perception-bypass risk exists even without a network.** Nothing but discipline stops a Local Host from handing a seat-controller a live True State reference instead of a projected View — worst case is hotseat, where it would silently defeat the pass-device UX. The Local Host must always construct real per-observer View objects via Perception, never skip that step "because we're already in-process."
5. **"Server" is really two services wearing one name** (ephemeral per-match host vs. persistent meta/account store) — keep them logically separate from day one even if co-deployed early.
6. **Keep Commands/Views/Events as plain, serializable-shaped data even in the Local Host**, so a "works locally" mistake doesn't surface only once Remote Host/Netcode is finally built.
7. **The Test/Sim harness's normal True-State-reading mode never exercises Perception** — give it a second mode that goes through a real Local Host + Perception, since Perception is the most novel, least-tested part of the whole design.
8. **Card schema is not yet specified.** `docs/rules/knowledge-capture-plan.md` steps 5 (card anatomy/schema) and 6 (keyword/ability library) are still open — the Content Repository's concrete shape depends on finishing that Track A work.

## 6. Meta-progression and offline play

See `decisions-architecture.md` A4 for the full decision. Summary: rather than a client-local progression mirror reconciled via trust-but-verify claims (rejected — reopens the exact anti-cheat hole "progression is server-authoritative" exists to close), the game leans on its own determinism (pillar 4) and already-planned replay/command-log capability (`PLAN.md` §6):
- **Offline solo/hotseat:** no progression, period.
- **Online solo vs AI:** the match still runs on a Local Host exactly like offline play; at match end the client uploads its recorded command log; the server independently replays it on its own Rules Core instance and only grants progression if the replayed outcome matches the claim. No trust extended to the client at any point.
- **Online 1v1:** unchanged — live-hosted, inherently secure.
