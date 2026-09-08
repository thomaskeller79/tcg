# The Map

*A map card is a fourth match component, alongside the card deck, terrain deck, and Champion. It resolves once at game start into board data and is not itself part of any zone.*

**Decisions:** D11, D51, D52, D53 (`history/decisions.md`)

---

## Layout
A map card specifies which hex cells exist and their connections (D11, D52). "Holes" are **void/aether** terrain, present as data with special functionality, not missing cells. Maps come in **sizes**, and map size sets the terrain-deck size — map legality is tied to size.

## Starting positions, home ground, neutral ground
A map specifies both players' **starting positions** and, around each, a **home ground** sized to exactly the terrain-deck size — this is where a player's own terrain deck ends up. Beyond both home grounds, **neutral ground** covers the rest of the map.

Populating these areas with actual terrain is itself a map-authored rule, with a default per area (D53):
- **Home ground (default: random).** At game start, a player's terrain deck is distributed randomly across their home-ground cells. A map may instead specify an alternative rule (e.g. each player first chooses one land to place as desired, then the rest fill uniformly at random).
- **Neutral ground (default: fixed assignment).** The map specifies exactly which terrain sits on each neutral hex. A map may instead specify a randomized/generated procedure.

## Terrain Type and Element (D52)
Every hex on the map — home ground or neutral ground alike — carries a **Terrain Type**: a purely worldbuilding classification with **no mechanical effect** (no move-cost, LOS, or capacity changes). It is fixed by the map card itself, independent of whichever Element ends up on that hex. Initial set, open to extending or shortening: **Forest, Open, Wetland, Shore, Elevated, Settlement, Urban, Edge.**

**Element** is the mana color of a terrain card (D51) — **Light, Fire, Metal, Earth, Darkness, Ice, Water, Air**. Every terrain card, basic or non-basic alike, carries only an Element, never its own Terrain Type — Type always comes from whichever hex it lands on.

A hex's full identity is its **Terrain Type combined with whichever Element lands there** — e.g. a Fire card landing on a Forest-type hex becomes a "Fire-adapted forest." This lets a map carry a consistent authored identity (a city map, a forest-choked map) regardless of which Elements the two players actually bring to a match. Specific Type+Element combination names are not locked.

## Landmarks
A map may define **landmarks**: specific hexes with special rules (e.g. "the first creature to enter this hex draws its Champion a card").

## Open questions
1. **Map selection (1v1):** both players bring a map — what's the selection/ban mechanism for picking the one actually played?
2. **Element identities:** Light/Fire/Metal/Earth/Darkness/Ice/Water/Air are named, but their individual identities and relationships to each other (a color-wheel pass) are not yet designed. See `PLAN.md`.
3. **Terrain Type list:** the initial 8 are a starting point, open to extending or shortening as content needs dictate.
4. **Population rules beyond the defaults:** the space of possible home-ground / neutral-ground population rules (beyond "random" and "fixed assignment") is unexplored — deliberately parked, not a structural gap.
