# Docs index

Four folders, split by the kind of question a doc answers:

- **`rules/`** — *what the game is, right now.* Mechanics, board/card/resource/combat rules, vocabulary. Read this to understand how a match plays — no revision history or rejected alternatives mixed in.
- **`architecture/`** — *how it's built, right now.* Components, the client/server Host boundary, engine/platform choices. Read this to understand the codebase's shape.
- **`tools/`** — *how to work with it.* File formats and workflows for the dev-facing tooling (e.g. the debug UI's scenario format), as opposed to the game engine itself.
- **`cards/`** — concrete card ideas surfaced while designing the systems above, not yet a rules layer of their own.

**Current vs. history, inside `rules/` and `architecture/`:** each of those two folders holds only current-state docs at its top level, plus a `history/` subfolder — `rules/history/` and `architecture/history/` — that's the *only* place revision narrative, rejected alternatives, and the decision log live:
- `rules/history/decisions.md` — the gameplay decision log (D1–D35, newest first): the call, why, what it touches.
- `rules/history/playtest-variants.md` — rejected-for-now or not-yet-decided rule alternatives worth an actual playtest later, cross-referenced from the `decisions.md` entry each relates to.
- `rules/history/pessimistic-default-audit.md`, `rules/history/knowledge-capture-plan.md` — working process trackers, not part of the current ruleset itself.
- `architecture/history/decisions-architecture.md` — the engineering decision log (A1–A5), same shape as the gameplay one.

**Default rule:** read the current-state doc for "what does the game/codebase do right now." Only open the matching `history/` doc when actually reconsidering a decision, to see why the current answer won and what's already been tried and rejected.

All documentation lives under `docs/` — nothing doc-shaped should sit next to implementation files in `src/`/`tools/`/`tests/`. Start with `PLAN.md` (project root) for the overall vision and roadmap; it links into these folders.
