# Architecture Decision Log

*Resolved engineering decisions, newest first — the technical counterpart to `docs/rules/decisions.md` (D1–D21, gameplay/design decisions). Numbered independently (A1, A2, …) so the two logs never collide. Cross-references a rules decision where one exists.*

**Status:** Active · **Date:** 2026-07-28

---

### A5 — Card data must be engine-neutral (portability constraint)
Card/content data is stored as plain, engine-agnostic files (JSON), never in an engine-specific resource format (e.g. Godot `.tres`). This is what makes engine portability (A-below) real rather than accidental — an engine swap that also required migrating every card file would defeat the point. A Card Editor tool may exist as a convenience layer, but the plain-file format remains the source of truth, so the editor itself must not become the only way to produce a valid card. → `card-data-and-editor.md`.

### A4 — Meta-progression: no offline progression; online solo is replay-verified, not claim-synced
Supersedes an earlier same-session proposal ("client-local mirror + claim-based sync, validated against plausibility bounds"), which was correctly flagged as reopening the anti-cheat hole `docs/rules/decisions.md` D1 ("progression is server-authoritative") exists to close. Replaced by a mechanism that costs almost nothing extra, because the project already committed to determinism (pillar 4) and to recording/replaying command streams for balance analysis (`PLAN.md` §6):
- **Offline solo/hotseat:** no progression at all. No claims, no plausibility bounds, no reconciliation machinery.
- **Online solo vs AI:** the match runs entirely on a Local Host, identical to offline play (no live-hosting latency during play). At match end the client uploads its recorded command log; the server independently replays it on its own Rules Core instance and grants progression only if the replayed outcome matches the claim. The server computes the truth itself — no client-reported number is ever trusted.
- **Online 1v1:** unchanged from the default client-server design — live-hosted, inherently secure (the server is already the sole simulator).
- **Rejected fallback:** live-hosting every solo match (identical wiring to 1v1, with the human's connection authorized to submit commands for both the human and AI seats) — as secure as 1v1, but pays a live-connection cost for every solo session with no offsetting benefit once replay verification exists.
→ `design-architecture.md` §6, `docs/rules/decisions.md` D1, `docs/rules/design-champions.md` (progression section to be updated to reference this mechanism instead of a bare "server-authoritative").

### A3 — AI is a symmetric seat-controller, client-side only
AI is not a privileged, true-state-reading, potentially server-hosted component. It is architecturally the same *kind* of thing as a human client — a seat-controller that submits Commands into a Host and reads back only its own per-observer View — differing from a human only in what decides the next Command (search/heuristics vs. UI input). No server-hosted AI is in scope. A contained, explicitly temporary exception exists for early prototype AI reading True State directly for tractability; this must be structurally gated (a distinct code path, not a comment) so it can never end up wired to a Remote Host in online play. → `design-architecture.md` §2.4, §5.3.

### A2 — Online mode is thin-client, not predictive
During an actual online 1v1 match, the server is the sole simulator — the client only sends Commands and renders whatever View/Events it receives back. No client-side prediction/rollback component is in scope. The Rules Core's client-embeddability (A1) is only ever exercised when no server is present (offline modes, and the Local-Host leg of online-solo's replay verification, A4). Turn-based play with explicit priority windows doesn't need frame-perfect responsiveness, so the complexity of prediction/reconciliation isn't justified. → `design-architecture.md` §2.3, `docs/rules/design-interaction-stack.md` (priority windows).

### A1 — One Host interface, pluggable transport
A single abstract boundary — "commands in, this seat's View+Events out" — is satisfied by two interchangeable implementations: a **Local/Embedded Host** (in-process call; solo, hotseat, and online-solo's match leg) and a **Remote/Networked Host** (real network transport to a separate Server process; online 1v1). Client-facing code (UI, AI) is written once against this interface and must never know or care which implementation backs it. This is the mechanism that makes "everything the server does must be possible to run on the client" literally true: same Rules Core binary, same Host contract, different transport underneath. Supersedes the original `PLAN.md` §6 diagram, which had no such boundary and treated UI/AI/Netcode as flat siblings of the Rules Engine. → `design-architecture.md` §1, §2.3.
