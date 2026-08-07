# Design — Companions

*A Champion's signature "friend": a card that sits mechanically **between Creature and Champion**. Introduces the game's channeling hierarchy explicitly: **Creature spends mana it can't draw; Companion draws mana it can't share; Champion draws and shares.** New card type — not yet in D1–D21, captured here first per the knowledge-capture-plan's card-taxonomy step.*

**Status:** Design note (second pass — incorporates working-session edits; open items flagged below) · **Date:** 2026-08-07

---

## Concept

- **The channeling hierarchy (fiction → mechanics).** Mana is abundant and many things can *spend* it, but drawing it from the land is rare, and sharing what you draw is rarer still:
  - **Creature** — can spend mana (mana-costed abilities) but cannot bond terrain. Draws on the **shared** pool only.
  - **Companion** — can bond terrain like a channeler, but the mana it draws stays **private**: usable only by the Companion itself, never shared.
  - **Champion** — can bond terrain **and** channel what it draws into the **shared** pool everyone spends from (D8/D9).
- **Deckbuilding-gated.** A Companion is a signature card tied to specific Champion(s) — legal only in the deck of the Champion(s) it names. Not a general card pool.
- **Shape:** Creature's three numbers (Attack / Life / AP) **plus** its own private mana pool **plus** a Bond ability. **"In between a Champion and a Creature":** it moves, fights, and bonds using the same **network-dependent cost shape** as a Champion (cheaper once its own connection collapses, pricier while it stays connected — see the action economy below) — but it can **never share** what it draws, and its own abilities are typically **priced higher** than a Champion's equivalent. A narrower, weaker channeler, not a second Champion.
- **Because a Companion has no hand of spells to spend its private mana on** (it's a single card in play, not a pipeline of draws), it is printed with **its own built-in mana-abilities** — e.g. `4 mana, 4!AP: summon a 2/2/6 soldier`. The private pool needs *something* on the card itself to spend on, or it's dead resource.
- **Meta-progression:** evolves between games like the Champion, but along a **more linear track** (fewer branch points) rather than the Champion's build-defining branching paths (D2). This is meta-layer scope (PLAN §9, post-M4) — parked here as a forward-note, not designed now.

## In-match: what a Companion is

- A real card in the maindeck: drawn to Hand, **summoned like a Creature** (D20 — paid in mana from the shared pool, onto a bonded terrain cell in the player's realm, subject to layer-capacity and summoning sickness).
- **Stats: Attack / Life / AP**, the same three numbers as a Creature (D10) — it occupies a board slot and is attackable. It moves, fights, and bonds using the network-dependent cost shape below (shared logic with the Champion, D9), not a Creature's flat defaults.
- Losing a Companion is a real, permanent loss (Life → 0 → destroyed → grave, D14/D16) — it doesn't end the match, but it **un-bonds the terrain it personally bonded** (see Terrain network, below) and its printed abilities go with it.

## The Companion action economy — mana + AP, same shape, more restrictive

No new resource. Mana + AP are the only two things that keep the world ticking — a Companion doesn't get a third currency, it gets **a second pool instance of mana** (private, not shared) plus the standard private AP budget (D10):

| Resource | Topology | Refills | Spent on |
|---|---|---|---|
| **Mana (private)** | one pool **per Companion**, separate from the shared pool | as its own bonded terrain produces (same conditional/pause rule as D8) | **only this Companion's own printed abilities** — never shared, never spent by other actors |
| **Action Points (AP)** | private, same shape as any Creature (D10) | to max each turn | move · fight · Bond · its own abilities |

**Mana never crosses the shared/private boundary, in either direction:** a Companion cannot draw on the shared pool, and the shared pool never receives what a Companion draws. This is already true of the Champion by omission (it's presently the only sharer), but stating it explicitly here is what would keep a future two-sharer format (e.g. a two-headed-giant variant) well-defined.

**Default AP actions mirror the Champion's realm-constraint shape (D9):** staying connected to its own network costs more than letting the connection collapse.

| Action | Cost | Notes |
|---|---|---|
| **Move** | `1AP` if its network collapses · `2AP` if it stays connected | Same differential as the Champion (D9) — a Companion is its own root, so its own connection is what's at stake when it moves. |
| **Attack** | `3!AP` if its network collapses · `5!AP` if it stays connected | Drain-all (`!`) in both cases, same as the Champion's own Attack (D9, fixed 2026-08-07 to match) and a Creature's default. |
| **Bond** | `3*AP` *(tuning example)* | Once/turn (the `*` flavor, D9 / `design-economy.md`); priced high against a small total AP pool so it crowds out most of the rest of the turn. |
| **Its own printed abilities** | card-specific `mana + AP` | Typically pricier than an equivalent Champion ability — a Companion is a narrower, less efficient channeler, not a second Champion. |

Example total AP: **4–5** *(tuning)*, vs. the Champion's 7 — so `Bond` alone already eats 60–75% of a turn, matching "if the pool is extended, it won't be able to do much else." All numbers here are illustrative, not locked; exact totals are a tuning pass alongside the rest of AP costs.

## Terrain network — Companion as a second root (extends D8)

D8 currently has exactly one network root (the Champion). A Companion in play becomes **its own root**, using the identical rule:

- A Companion may **Bond** (its own AP-costed ability, above) a terrain **reachable from itself** through already-bonded terrain, unblocked by enemies — same reachability/pause mechanics as the Champion's bonding (D8), just evaluated from the Companion's own tile instead of the Champion's.
- **Ownership follows the bonder.** Whoever *performs* the bond (Champion or a specific Companion) determines which pool that terrain feeds going forward: Champion-bonded → shared pool; Companion-bonded → that Companion's private pool. Direct mechanical expression of the fiction (only the bonder's own channeling capacity determines where the mana goes); reuses D8's existing bond/pause machinery, just tagged per-root.
- **Severing/pause is per-root, unchanged mechanically.** An enemy on a bonded terrain's only path back to *its owning root* pauses that terrain's draw — exactly D8's existing rule, just checked against whichever root owns the node.
- **Control follows ownership too.** If a terrain carries its own printed activated ability (`design-resources-terrain.md`), only the controlling root's owner may activate it, funded **only** from that root's own pool — a Companion-bonded terrain's ability spends from that Companion's private pool alone, never the shared pool.
- **A Companion's death un-bonds its terrain.** Unlike the Champion's network — where bonding is permanent, and only the *draw* is conditional (D8) — a Companion's bond is only as permanent as the Companion itself. When it dies, the terrain it personally bonded reverts to **unbonded** (same "undo, don't just pause" vocabulary as **un-summon**, D16) rather than sitting paused forever. It can later be bonded again, by any surviving root, under whatever reachability holds at that time. This makes a Companion a real, killable economic target, not just a body on the board.

## Interactions with other pillars / systems

- **Deckbuilding:** Companion cards name the Champion(s) that may run them — a hard legality restriction, like the Champion gating faction identity. Count-per-deck / count-in-play limits are open (below).
- **D17 (card taxonomy):** Companion is a new **Play-permanent** seed type, alongside Creature/Structure/Item, cast through the Aether like any Spell — but deckbuilding-gated the way Map/Champion/Terrain are pre-game-constrained (a hybrid: cast like a Spell, gated like the non-Spell components).
- **Asymmetric info:** no special treatment proposed — visible by default (D7), like a Creature, unless a future card says otherwise.

## Invariant vs. mutable

- **Invariant:** exactly the two resources (mana, AP) — a Companion adds a second **pool instance** of mana, not a new resource; a Companion is a **second network root**, using the same bond/pause mechanism as the Champion, not a new one; mana never crosses the shared/private boundary by default.
- **Mutable (card-driven):** a Companion's stats, AP costs (including its Bond cost), its printed mana-abilities, and how many may be run/fielded.

*(Resolved questions are cut once closed — the rule lives in the sections above and, for decision-grade calls, in `decisions.md`. Only genuinely open/deferred items stay here.)*

## Open questions

1. **Count limits — deferred to PLAN.md Track A** (step 1, "Uniqueness / copy-limit rule (general)"): Companions are expected to end up singleton (1 in deck / 1 in play), but that should fall out of a **general** uniqueness rule other cards can also use, not a Companion-specific carve-out. Not designed here.
2. **Meta-progression shape:** "more linear than the Champion" — needs its own pass once the meta-layer is actually being designed (post-M4, PLAN §9). Parked, not blocking the in-match rules above.
