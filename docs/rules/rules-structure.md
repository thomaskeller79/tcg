# Rules — Match Structure (Strawman Defaults)

*The skeleton everything hangs on. Two companion principles govern every default: **(1) the simplest rule that still exposes a hook** (see `knowledge-capture-plan.md` §Part A); **(2) pessimistic defaults (D14)** — the baseline is the weakest *still-functional-and-fun* case, and cards improve it with **positive** lines, never default restrictions (e.g. damage persists by default; healing is an ability). Strawman by Claude — replace any part with your version.*

**Status:** Strawman for review · **Date:** 2026-07-23

Legend: 🪝 = the mutation hook this default preserves so cards can bend it.

---

## 1. The board
- **The board comes from a *map card* (D11).** A legal match brings **four** components: card deck, terrain deck, Champion, **and a map**. Maps are a strategic choice and come in **sizes**; **map size sets the terrain-deck size** (map legality is tied to size). The engine consumes **one resolved map** as data — a set of hex **cells** keyed by cube/axial coordinate; bounds emerge from which cells exist. *(A strawman sample map exists for testing; the model is map-as-content.)*
- **A map card specifies (D11):** (1) **form** — which cells exist + connections; "holes" are **void/aether** terrain (present as data with special functionality, not missing cells); (2) both **starting positions**; (3) each player's **home zone** around their start, sized to **exactly the terrain-deck size**, filled **randomly** with that player's terrain deck at game start (= the D8 home base); (4) a **neutral area** with a predefined terrain-generation procedure; (5) optional **landmarks** with special rules.
- **A hex has three vertical layers (D12):** **ground / above (flying) / below (submerged)**, each holding up to **3** creatures. The engine represents a cell as three distinct occupancy spaces. The **below layer is hidden by default** (pillar 6) — visible only in its owner's view. *(3-per-layer = the guard-your-key-creature vs. not-crowded compromise.)*
- **No separate Base (D9):** each player's objective *is* their **Champion**, which starts on its map-defined **home tile**. Kill the enemy Champion to win (§7).
- 🪝 Each cell has *queryable* properties (move-cost, terrain tags, per-layer capacity/occupants). Defaults: move-cost = 1, capacity = 3/layer. → terrain, high-ground, void, impassable, capacity changes, landmarks are all **data**, not new rules.
- 🪝 The engine exposes **adjacency, distance, reachability/path, and line-of-sight** as spatial queries (path serves both movement and the D8 terrain network; LOS serves Ranged *and* Mist — named now so perception isn't a bolt-on).

## 2. Zones
- **Default (D16): Hand, Library, Play, Aether.** No Discard/graveyard: one-shots resolve to **traces** in the Aether; dead permanents become **graves in Play** (hex markers, no slot). The terrain deck is a separate deck (§5). A card may occupy **multiple zones at once** — e.g. a cast creature = a permanent in Play **+** an anchored summon-trace in the Aether (enabling *un-summon* vs *kill*, D16).
- 🪝 Zones are a named set; the **Aether** is a per-observer timeline whose traces **fade** on a configurable window. → cards reference/move between zones; new zones can be added later.

## 3. Turn structure
- **Default (D21):** strict alternating turns (I-go, you-go), three phases:

  | Phase | What happens (default) |
  |---|---|
  | **Beginning** | mana / AP **refresh** (mana refreshes, no banking — D8 #4; AP refresh covers creatures and the Champion alike, D9) — **simultaneous, no state checks**. *(Card draw is no longer an automatic Beginning-phase step — as of the D9 revision it's the Champion's `5*AP: Draw` action, spent in the Action phase below.)* Then beginning-of-turn triggers go into the Aether **APNAP** (active player's first, then opponent's → opponent's resolve first). **No priority this phase to start** (simple); active-player *choices* like upkeep costs still work — only *opponent-reactive* upkeep needs a window, added later (phases are data). |
  | **Action** | active player plays spells, spends AP to move/attack/draw/bond/activate, in **any order**, until they choose to finish. **After every active-player action — including a move and a deliberate pass — the inactive player gets a response window** (D5, refined to per-action → enables movement-triggered traps; auto-pass keeps it fast). |
  | **End** | end-of-turn triggers, APNAP, same as Beginning. |

- 🪝 The phase list is **data the engine walks**, not hardcoded control flow. → "skip your draw," "extra Action phase," "add a priority window to Beginning" are all card/config effects.
- *Rationale:* one free-order Action phase = simpler to learn, more flexible than separate move/combat phases; forced sequencing can be imposed by cards.

## 4. Units: acting, movement, combat
**A creature is three numbers: Attack / Life / Action Points (AP) (D10).** AP is the creature's **private per-turn action budget** (refills to max each turn, no carryover by default); mana is the **shared** pool (§5, D8). See `design-economy.md` for the full three-resource model.
- **Summoning (D20):** a creature is summoned onto a **bonded terrain cell in your realm** (the realm is the deployment zone as well as the economy), paying the card's **mana** (a plain mana play, unrelated to the Champion's own AP, repeatable as mana allows). Needs a free slot on the creature's layer (§1, D12). 🪝 **Summoning sickness (pessimistic default, D14):** a summoned creature **can't act the turn it enters**; "acts immediately" is a positive (haste) keyword.
- **Move and attack are default *abilities*, not rules (D10).** Every creature carries two replaceable defaults: **`1AP: Move`** (one hex per AP; a creature may enter a hex only with a free **ground slot**, capacity 3, §1) and **`3!AP: Attack`** (targets a **hex**; **Melee** = adjacent, **Ranged N** keyword = within N). There is no hardcoded move/attack logic — just abilities with AP costs.
  - **The `!` cost:** `xAP` spends exactly x (leftover AP stays usable); **`x!AP`** requires x then **consumes all remaining AP**. So default `3!AP: Attack` = no multi-attack (attacking drains you); a printed `3AP: Attack` would allow it. See `design-economy.md`.
  - **Abilities** may cost a mix of `mana + AP` (and either AP flavor).
  - 🪝 AP total, ability set, per-action AP costs, capacity, and legal-target set are all **queries/effects**. → +AP buffs, cheaper/pricier or replaced move/attack, flying, rooted, reach, "can't attack fliers," etc.
- **Declaring defenders (D4):** when a hex is attacked, the defending player **declares which of their creatures on that hex defend**. Default: **each creature may defend only once per turn** — the **pessimistic default (D14)**: "once" is the weak case, so *multi-defend* is a **positive keyword** (Bulwark/Guardian-type), not a negative "defends once" line. **Defend-cost rule is under playtest (D15):** two candidates — **V1** defending costs `1!AP` (exhaust → once/turn, competes with acting; enables the +1-AP ambush-blocker), or **V2** defending is free + unlimited with **persistent damage (D14)** as the natural limiter (Life = defensive stamina; strip a guard by killing it or bypassing via layers/flank). Resolve empirically. `cannot defend` is an occasional negative keyword under either.
- **Combat as an explicit `Combat` object (D3):** every fight is `{attacking units, target hex, declared defenders} → resolution`, with a **pre-resolution priority window** (see `design-interaction-stack.md`) enabling combat tricks. **Default scheduling is sequential** — a `Combat` is created and resolved the instant an attack is declared. *Minor goal: keep scheduling swappable so a phased (declare-all-then-resolve, simultaneous multi-attacker) model is a later scheduler change, not a rewrite.*
- **Combat resolution (D13):** a Combat resolves into **directed damage events** — `attacker → target(s)` and `target → attacker`. Defaults: **mutual** (both sides damage each other), **simultaneous** (both computed from the pre-combat state → "both die" possible), the **attacker assigns** its Attack across the declared defenders, and **all defenders gang up** (each deals its Attack to the attacker). **Undefended** hex: attacker assigns among targets of its choice on the hex (creatures, or the Champion → win path), no retaliation. **No** dice, **no** facing/flanking/high-ground. A creature at **0 Life is destroyed** (→ becomes a **grave in Play**, D16, no slot consumed); **damage persists between turns — no auto-heal** (D14).
  - 🪝 Each damage instance is an **event** through the pipeline → Ranged (no retaliation), First Strike (sequenced timing), Trample (excess carries), prevention/redirection are all modifiers, never new rules.
- **Cross-layer targeting (D19).** Default attack **initiation** legality (attacker → target): **Ground→** ground ✓ / flyer ✗ / sub only-if-located. **Flyer→** ground ✓ / flyer ✓ / sub only-if-located. **Sub→** ground ✓ *(tentative, balance)* / flyer ✗ / sub ✓ (if located). Sub is **hidden by default** (D12) → untargetable unless located; cards give exceptions (reach/anti-air/true-sight). **Retaliation is universal:** a defender **always** deals its damage back regardless of layer (ground retaliates against a diving flyer) — combat is mutual across layers; air superiority = initiative, not immunity. **Sole exception: Ranged** (keyword) — a **one-way** attack ("deals damage equal to power to target creature"; ranged combat covered by keyword later). **Acting reveals a concealed sub:** attacking **surfaces** it; it stays **located while on that hex** and **re-conceals when it moves away** *(confirm)*; "strike and stay hidden" is a rare positive keyword.
- *Note:* position already matters via adjacency, Range, hex occupancy, and guarding — no combat modifiers needed to make placement meaningful.

## 5. Resources — terrain connection network (D8)
- **A separate terrain deck** (size varies by format) is laid out **randomly in the home base** at start. **Up to once/turn**, **bond** one terrain reachable from the **Champion** via already-bonded terrain. Bonding is **permanent**; bonding +1/turn is a smooth, deterministic ramp — no draw-based mana/color screw.
- **Mana draw is conditional:** a bonded terrain produces mana **only while a path of enemy-free terrain connects it back to the Champion**. An enemy creature on any path node **pauses** (reversibly) all mana downstream of it — kill it or reroute to restore.
- **8 colors:** 8 unrestricted basic lands + restricted non-basic "fancy" terrains. The **Champion is the network root** — moving it endangers the mana base.
- Denial is **positional** ("put the connection on hold" — reversible, counterable), never random.
- 🪝 Bond count, reachability, path-blocking, and what a terrain produces are all **effects/queries**. → extra bonds, bond-at-range, ignore-blocker cards, etc.
- *Note:* this is a deliberately **rich** core system (not a simple default) — its complexity is the price of a signature economy; keep other systems simple to compensate.

## 6. Deck & hand
- **Default:** deck size and max copies TBD (strawman: 30-card deck, max 3 copies). Draw 1/turn (§3). Opening hand size TBD (strawman: 5).
- 🪝 Draw count, hand size, deck legality are all queries/effects. → "draw 2," "max hand size 10," faction deckbuilding restrictions.
- Faction/color *structure* exists as a hook; flavor deferred.

## 7. Win condition
- **Default (D9):** **kill the enemy Champion.** There is **no separate Base** — the "home base" is just the Champion's start tile (usually a landmark terrain), not a destructible objective. The Champion is the objective, the economic root (D8), and the most-exposed piece all at once.
- 🪝 The win check is an **evaluated effect**, not a hardcoded `if`. → alternate win conditions ("control 5 hexes," "mill the enemy deck") are cards.

## 8. Invariants (deliberately NOT mutable)
The stable substrate cards rely on. Kept fixed on purpose:
- The game state is **deterministic**; randomness only via defined shuffles/draws with a seed.
- An **event pipeline** and a **query layer** always exist (cards hook them; they can't be removed).
- A single authoritative **True State** exists, and a **per-observer view layer** always exists — every player always has *some* view (asymmetric info, pillar 6). *What* a player sees is mutable; that they get a projected view is not.
- Turns alternate between players **unless an effect explicitly says otherwise** (the alternation itself is default, but the two-player-seat framework is invariant for now).
- A match **must be able to end** (win checks are always evaluated).

---

## 9. Visibility / perception (default)
- **Default:** the board is **fully visible** to both players. Every unit shows its true identity and stats. Concealment is **opt-in via cards** (Mist, Submerged, Mimic).
- 🪝 Visibility, appearance, and knowledge are **perception queries** per observer (see `design-asymmetric-information.md`). Default answers = "visible / true / known." → Mist, Submerged, Mimic, Detection are modifiers on these queries.
- *Rationale:* full visibility is the simplest default that still exposes the perception hook; hidden info is layered on by cards, exactly per the "simplest rule that still exposes a hook" principle.
- **Belief consistency (D18):** deceptions are card-authored **claims** that hold until a **hard fact** tests them, then **collapse to full truth**. The engine enforces only a small set of **conservation laws** (no deduction engine — humans reason). Border: *standing state public, private-resource ledger hidden, footprint costs self-revealing* → only the **live mana balance** is hidden by default. See `design-asymmetric-information.md`.

## 10. Champions (in-match default)
- **Default:** each player has exactly one **Champion** on the board, placed at start on its **home tile**. It is a special card type (HP, position) simulated by the core; **it is attackable and its death loses the game** (§7, D9).
- **Mana + Action Points (D9, revised 2026-08-06):** the Champion runs the **same two-resource shape as a creature** — *mana* (spent on spells/units, many/turn) and its own **AP** (private, refills each turn), spent on **draw a card, bond a terrain, move, fight, activate abilities** — not a bespoke third model. Baseline example (tuning): 7 AP; `5*AP` draw (replaces the automatic per-turn draw), `2*AP` bond (the D8 once/turn limit), `2AP`/`1AP` move, `5AP`/`3AP` attack (network-active/collapsed). No hardcoded "pick one" — see `design-champions.md` for the full breakdown, the tension, and open items (ability list, defend/retaliation).
- **Realm-bound movement (D9, D8):** the Champion is the network root; the pricier move that steps off its bonded network **reversibly pauses** the disconnected mana. The cheap move along the network is safe → the "walking channeler."
- The core consumes a **resolved Champion loadout** (level/path/abilities) as input; **no progression logic runs inside a match** (that's the meta-layer). See `design-champions.md`.
- 🪝 Champion AP is a per-turn resource query (default = 7, tuning); abilities/stats are the same effects/queries/modifiers as any card. → extra AP, free/reactive abilities, paths, level-ups, in-match buffs all hook the same machinery.

---

## Open questions for you
1. ~~**"Turn" vs "Round"**~~ — **RESOLVED:** "Turn" = one player's sequence of phases; players alternate.
2. **Board:** fixed single map to start, or multiple maps? Symmetric? Rough size feel — skirmish (~5 wide) or battle (~9+)?
3. ~~**Turn structure**~~ — **RESOLVED (D3, D5):** single free-order Action phase, **sequential** combat now, with priority windows for interaction; keep phased scheduling revertible.
4. **Combat damage rules (partially open):** direction resolved to sequential + declare-defenders (D3, D4). Still OPEN: is damage **mutual** (defenders hit back) or one-way? How does an attacker's damage split across multiple declared defenders?
5. **Resources:** confirm Model C auto-escalation as default, or do you already have a resource system in mind? (This one shapes everything — happy to capture yours instead.)
6. ~~**Win condition**~~ — **RESOLVED (D9):** kill the enemy Champion; no separate Base.
7. ~~**Champion (§10)**~~ — **RESOLVED (D9, revised 2026-08-06):** kill it = win; attackable; it *is* the resource/channeling engine (root, D8); runs mana + AP like a creature (draw/bond/move/fight/ability), not a separate Channel. *(Remaining opens in `design-champions.md`: term, exact AP costs/ability list, retaliation mechanism, reactive abilities, deck identity, down-leveling, level-up conditions.)*
8. Anything in your existing notes that **contradicts** a default above — those are the most valuable things to surface now.
