# Architecture — Card Data & Content Pipeline

*How cards (and terrain, maps, Champions) actually get from a designer's head onto disk and into the Rules Core — the component the original architecture pass was missing entirely.*

**Status:** First draft, structurally scoped only · **Date:** 2026-07-28 · Decision: A5

---

## Why this is its own component

Pillar 5 already commits the game to being fully data-driven (cards defined in data, not code). But "data-driven" only becomes real once something concrete: (1) defines the shape every card follows, (2) stores that data in files, (3) makes those files loadable by the Rules Core as queryable content, and (4) gives a human a sane way to author them. None of that existed as a named component before this pass.

## What this is NOT

- **Not the card schema itself.** The concrete shape a card takes (its properties, how effects reference the keyword/ability library) is a *rules-design* task, not an architecture one — see Open dependency below. This doc reserves the component and states its constraints; it doesn't specify the schema.
- **Not Meta-progression.** This component answers "which cards exist" (static content, ships with the game). Meta-progression answers "which cards this specific player currently owns/may use" (a per-player, server-authoritative concern, `design-architecture.md` §2.7). Keeping these visibly separate matters — a card can exist in the Content Repository long before any player has unlocked it.

## Pieces

### Card schema
The data shape every card, terrain, map, and Champion follows. **Not yet specified** — depends on finishing `docs/rules/knowledge-capture-plan.md` steps 5 (card anatomy/schema) and 6 (keyword/ability library), which are still open Track A work. Treat schema completion as a near-term Track A deliverable that feeds this component, not a blocker for the rest of the architecture.

### Card data files
Plain, engine-agnostic JSON (per decision A5) — one entry per card. Effects reference the keyword/ability library rather than embedding free-text logic or prose (`knowledge-capture-plan.md`: "effects are structured data, not prose"). This is also what keeps engine portability real (`design-architecture.md` §4) — an engine swap must never require migrating card data.

### Content Repository
Loaded by Rules Core at startup; indexes all card/terrain/map/Champion definitions. Pure data, no logic of its own — Rules Core queries it the same way it queries anything else (pillar 5: never a raw read, always a query, even for "what does this card do"). The base-rule defaults living in the query/modifier layer and the content in this repository are answered through the same mechanism, by design.

### Card Editor tool
A separate authoring tool, not part of the shipped runtime, that reads/writes the same JSON files. The JSON format is the source of truth; the editor is a convenience layer over it. It must not become the *only* way to produce a valid card file (e.g. by being built as an engine-editor-only plugin with its own serialization) — that would silently reintroduce an engine-specific dependency and undermine the portability constraint (A5) this component exists to protect.

## Open dependency

This component cannot be fully specified until Track A finishes:
- `docs/rules/knowledge-capture-plan.md` step 5 — card anatomy / schema
- `docs/rules/knowledge-capture-plan.md` step 6 — keyword / ability library

Both are listed as not-yet-done in the capture plan as of this writing. Revisit this doc once they land.
