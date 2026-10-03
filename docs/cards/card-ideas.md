# Card Ideas — Scratch List

*Not rules, not finalized — a running catch-all for concrete card/design-space ideas surfaced in conversation, kept so they don't get lost before the actual card-authoring pass (`docs/rules/history/knowledge-capture-plan.md` step 7, "Prototype cards"). Freeform; promote an idea into the real rules docs once it's actually decided.*

**Status:** Living scratch doc · **Date:** 2026-08-07

**Before promoting an idea to a real card:** write its rules text out in the effect normal form (`../rules/effect-form.md`). If it doesn't fit, reword the card or file the gap as a Track A item in `PLAN.md`.

---

## Terrain

- **"Mimic Basic Mountain"** — a non-basic terrain that visually presents as a basic Mountain (D18 `face`) but is actually a trap: e.g. "When a creature enters this terrain for the first time, deal 3 damage to it." A direct application of the existing Mimic/face (D18) + trap (D6) primitives to a terrain card — no new mechanism, just a new domain for them.
- **Bond surcharge on a non-basic** — e.g. "Bonding this costs `4!AP` more," printed on a powerful non-basic terrain — the AP-cost analog of MTG's "enters tapped." A per-card modifier on the Bond action's cost (pillar 5), not a base-rule change.
- **Terrain with a static combat buff** — e.g. "Defending creatures on this terrain get +1 power."
- **Terrain with a movement surcharge** — e.g. "Moving into this terrain costs +1 AP."
- **Terrain with a paid triggered ability** — e.g. "Whenever a creature enters this, you may pay 1 AP. If you do, deal 1 damage to that creature." Terrain holds no AP, so the cost is paid by its bonder (Champion or Companion); a dormant (Neutral) terrain skips it, since it needs a choice (D87). See `resources-terrain.md`.
- **Terrain that brings a Structure (D87)** — e.g. **Fire Flat** (Terrain: produces 1 Fire) comes with a **Fire Barrack** (Structure: `3!AP` + 1 Fire: create a **Fire Warrior** on this hex — Ground, its card's default Slice; fizzles if that Slice has no space). Terrain has no activated abilities; the Structure carries the engine and can be destroyed, leaving a plain Fire land. Fire Warrior is a normal card (D88). How the Structure arrives at Setup is open (`resources-terrain.md` Open question 3).

## Remnants and Ruins (D110)

Prototype cards from the item 27 discussion — what a fallen permanent leaves, and the fell / destroy / raise verbs.

- **Raise the Fallen** (Spell) — "Raise target Remnant you control." The basic zombify. "You control" means a Remnant on terrain you bonded, so an enemy creature that fell on your ground can be raised; the raised creature is yours (parent copies from your Trace).
- **Watchtower** (Structure) — card static: "can only target a terrain you control with a Ruin." Casting it replaces the Ruin (D40). The way to express "build on a Ruin" without a cost that refers to the not-yet-chosen location.
- **Gravewarden** (Creature) — "`1AP`, destroy target Remnant within distance 1: this gains 2 Life." A Remnant as a consumed resource.
- **Unmake** (Spell) — "Destroy target creature." Removal that leaves no Remnant and fires no fall triggers; should cost more than an equivalent "fell."
- **Carrion Crow** (Creature, Flying) — "When a creature within distance 1 falls, draw a card." Tests the fall trigger: destroy, Flicker and Bounce don't fire it.
- **"Do something where it fell"** — target the hex: "Fell target creature T on target hex H. Create a Fire Warrior on H." After the fell, T no longer exists, so text can't read T's location.

---

*Add more here as they come up.*
