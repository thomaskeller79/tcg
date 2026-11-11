# Idea Backlog

*Not rules, not decided. Card ideas and mid-level material from the user's early notes, left over after the Element identities were worked out (`element-identities.md`, `PLAN.md` Track C item 1) — translated to English and current terms (Energy, Health, Power, Root, fell, Island). An Element named next to an idea is what the notes suggested, not an assignment. Use it when designing the mid level and the first cards; delete entries as they're used or rejected.*

**Status:** Backlog · **Date:** 2026-11-10

---

## Mid-level mechanics

Core mechanics every Element needs in its own way (`PLAN.md` Track C item 1).

### Card draw
- **Plain draw** (Light, Water, Air in the notes): "Draw 2 cards."
- **Filter** (Water if you draw more than you discard, else Air; Ice also had a filtering variant): "Draw 3 cards, then discard 1." (could also be Metal)
- **Replace** (Air: improves the hand without changing its size): "Discard 2 cards, then draw 2 cards." / "Draw 2 cards, then discard 2 cards."
- **Selection** (Light: makes possible futures visible and lets you pick one): "Reveal the top 5 cards of your Library. Put one into your hand and the rest on the bottom." / "Look at the top 3 cards of your Library and rearrange them."
- **Draw paid by giving something up** (Darkness): "Discard a card: draw 2 cards." / "Pay 3 Health: draw 2 cards." / "Fell a creature: draw 2 cards." Also a variant where you give up cards while drawing. (The notes give fell costs in general to Darkness.)
- **Cantrips and tutors:** for Items (Metal: "When you cast this Item, draw a card." / "Search your Library for an Item card and put it into your hand.") and for Structures (Earth, the same with Structures).

### The opponent's hand
- **Discard:** Fire burning resources, Darkness stealing thoughts, Ice freezing thoughts.
- **Confuse:** make 2 or more cards look "empty", so the opponent can't be sure what they play.
- **Take a card** ("drain card"), theft and manipulation (Darkness). A Darkness twist: the theft is unobserved — the opponent notices something is missing, not what.
- **Nullify a future draw:** add a card to the opponent's Library that does nothing, or is even bad while in hand.

### Removal
- **Creatures**, split by the notes: direct damage Metal, area damage Fire (+ Earth?), exile Darkness, weaken (−x Power / −x Health), drain Energy, take control. "Think of this for each Element."
- **Structures:** direct damage, area damage (Earth: "Deal 4−x damage to each Structure, where x is its distance to target hex."), exile, cutting the network. "Think of this for each Element."
- **Erase:** "Remove target creature from the game." **Nullify:** "Remove all activated abilities from target permanent." (Darkness in the notes)
- **Open question — who does mass removal?** Earth fits spot removal with "destroy" (it returns to the earth), but not mass removal. Damage splits into Fire (mass: everything in an area or on one terrain) and Metal (one target); can destroy be split the same way? If it should be a neighbour of Earth, it would have to be Darkness.

### The control axis
Light's and Darkness's ways are settled (`PLAN.md` Track C item 1); these are the notes' card texts:
- **Dominate:** "Gain control of target opposing creature." **Convert:** the same, as Light's conviction (direct control is open).
- **Sever:** "Target creature becomes Neutral. It becomes Aggressive toward its previous controller."
- **Claim / Recruit:** "Gain control of target Neutral creature."
- **Liberate:** "Target creature becomes Neutral. It becomes Fleeing." (fleeing has no owner yet)
- **Taking control of terrain — and the Structures on it.**

### Energy
- **Resource denial** (Air in the notes): taking away the opponent's mana or Energy; close to drain and to Knotting.
- **Sleep** (Darkness in the notes): a creature has no Energy at all for a round.
- **Actions cost more** (Earth in the notes), with a penalty if there isn't enough Energy — Quicksand: "Mimic Waste. When a creature enters this, it gets trapped. A trapped creature loses all Move abilities and gains '8 Energy: lose trapped'. At end of turn, destroy all trapped creatures."

### Terrain
- **Changed terrain**, to consider for each Element: iced, flooded, raised (by Earth: "flyers can be attacked here"), sunken (by Earth: "Ground and Root merge here").
- **Scorch Land** (Fire): "Target terrain becomes Scorched and produces Fire mana instead of its normal mana."
- **Stone hex** (Earth): "Create a Stone hex; Earth creatures standing on Stone gain +1 Armor, and the Stone can later be raised into a Hill that blocks movement."
- **Ramp:** gain one-shot mana; Energy Pool (Water): "Pay 1 Water mana: put an Energy counter here, only once each turn. Remove X Energy counters: add X Water mana."
- **Cutting bond connections** (Metal in the notes).

### Traces
- Copy a trace; destroy what a trace created; bounce a trace?

### Method
- List all state values in the implementation, then think about who can change them, and how.

---

## Items, Structures and creatures without an Element

- **Masks:** an idea for Water — a mask is a false form (a Mimic as an Item).
- **Amulets / rings:** permanent passive equipment that grants a lasting ability or bonus while worn.
- **Totems:** an Item or a Structure, depending on the form.
- **Statues:** a construct.
- **Growth:** a creature that itself gets larger or stronger over time — unlike Metal, which grows through Items and modular enhancements. Could be Earth or another Element.

---

## Card ideas by Element

- **Light:** Blind — "Target creature can't attack or defend until end of turn."
- **Fire:** Wildfire — "Deal 1 damage to all creatures in target hex. If at least 2 creatures are affected, the fire spreads to all hexes at distance 1. If at least 12 are affected, it spreads to distance 2, and so on."
- **Earth:**
  - Root mastery: Earth creatures observe 2 hexes in the Root instead of 1, and can create a Sinkhole that exposes the Root beneath a Ground hex.
  - Deck highlight: fungify.
- **Darkness:**
  - Shadow Assassin — "Invisible: opponents can't observe this creature unless one of their creatures enters its hex. Assassinate: when attacking while hidden, this creature takes no damage."
  - Darkvision — "This creature observes 2 hexes in the Root instead of 1."
- **Water:**
  - Lure — "At the beginning of your opponent's turn, move target creature as far as possible toward target hex, paying its normal Energy cost."
  - Current — "Move all creatures in target region one hex in the same direction."
  - Pressure — "Remove 3 Energy counters: push all creatures adjacent to this hex 2 hexes away."
  - Hidden Water creatures: a creature only becomes observable when it attacks (or when an enemy happens to enter its hex). This makes "suicide" creatures, weak once found; entering a water hex hides it again, so it's stronger near lots of water. Better, more efficient ones are hidden only on water hexes and become observable after 1–2 rounds on other terrain: they kill faster and more reliably, but are found quickly.
  - Flyers are Water's problem: maybe water fountains that bring them down from the Sky.
- **Air:**
  - Gust — "Push target creature 4 hexes away from this hex."
  - Blast — "Push all opposing creatures in this region 2 hexes away from its center."
  - Evasive — "When this creature defends, you may pay 1 Air mana. If you do, remove it from combat and move it 1 hex."
  - Airbend — "Remove target creature from the Island. Return it at the beginning of its controller's next turn."
  - Transmission (Air is part of every hex, so energy, force or effects travel far): Transmit — "Move X Power and Health from this to another creature in a straight line."; "Target creature gains +2 Power until end of turn."
  - Information: Air carries sound and information — no simple mechanic found that doesn't clash with Light's observation.
  - Equalize (fun, probably too narrow): "Set each opposing creature's Power to the average Power of all opposing creatures, rounded up."

---

## Decks, evergreen mechanics and loose cards

- **Companion deck:** the Hero summons many Companions that do the work, leaving the Hero with little mana.
- **Zones by Element** (old table): Sky — Air, Fire, Light (creatures); Root — Earth, Darkness, Metal (creatures); buildings — Metal, Darkness, Light; cemetery — Darkness, Earth (re-use); inventory — Metal (weapons, armor), Earth (potions); Aether — Water, Ice, Air, Light (?); Companions.
- **Evergreen candidates:** always deals damage when it defends; invisible (scout).
- **Doppelgänger:** spawns mimicking another creature on the map (fits Water's Mimics).
- **A hidden −2 Power:** a creature gets −2 Power, and this is hidden (fits Darkness's hidden marks).
