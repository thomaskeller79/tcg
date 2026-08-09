# Docs index

Three folders, split by the kind of question a doc answers:

- **`rules/`** — *what the game is.* Mechanics, board/card/resource/combat rules, vocabulary, and the D1–D21 gameplay decision log (`rules/decisions.md`). Read this to understand how a match plays.
- **`architecture/`** — *how it's built.* Components, the client/server Host boundary, engine/platform choices, and the A1–A5 engineering decision log (`architecture/decisions-architecture.md`). Read this to understand the codebase's shape.
- **`tools/`** — *how to work with it.* File formats and workflows for the dev-facing tooling (e.g. the debug UI's scenario format), as opposed to the game engine itself.
- **`cards/`** — concrete card ideas surfaced while designing the systems above, not yet a rules layer of their own.

All documentation lives under `docs/` — nothing doc-shaped should sit next to implementation files in `src/`/`tools/`/`tests/`. Start with `PLAN.md` (project root) for the overall vision and roadmap; it links into these folders.
