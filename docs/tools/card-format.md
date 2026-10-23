# Card data format

Card definitions live in `content/cards/*.json` (A5: plain, engine-neutral data). A file holds one
card object or an array of them; ids must be unique across all files (D99). Loaded by
`Leyline.Content.Json.CardJson`. Under the prototype scope (`docs/rules/effect-form.md`, D104)
rules text is a list of targets plus a sequence of instructions.

## Fields

| Field | Used by | Meaning |
|---|---|---|
| `id`, `name` | all | Unique id; display name. |
| `type` | all | `Creature`, `Companion`, `Spell`, `Structure`, `Item`, `Champion`, `Terrain`. |
| `subtypes` | all | Free tags. |
| `cost` | castable cards | Compact cost, below. |
| `speed` | castable cards | `Slow` (default), `Quick`, `Reactive`, `Instant` (D45). |
| `attack`, `life`, `ap` | Actors | Printed max stats. A Structure has no Attack. |
| `keywords` | permanents | `Flying`, `Subterranean`, `Knotting`, `Defender`, `Haste`, `Haste 2`, `Ranged 1-2` (distances 1 to 2, D127). |
| `abilities` | permanents | Printed activated and triggered abilities, below. |
| `statics` | permanents, Terrain | Printed static abilities, below. |
| `removedDefaults` | permanents | Default abilities this card lacks, e.g. `["equip"]`. |
| `targets`, `instructions`, `duration` | Spell | Rules text; Duration (ends of player turns in Past, D144) defaults to 10. |
| `produces` | Terrain | Mana per draw, one entry per mana: an Element (`"Fire"`), Elements joined by `/` for mana spendable as any of them (`"Fire/Metal"`), or `"Colorless"` (D125). |
| `carrierAttack`, `carrierLife` | Item | Static bonus to the carrier. |
| `entersSlice` | permanents | Overrides the Slice filter (default: Flying → Sky, else Ground; a Structure takes the Ground or Root slot). |
| `bondAp` | Companion | Its Bond cost, default 3 (`3*AP`). |
| `text` | all | Reminder text for the UI. |

Default abilities (Move, Attack, Defend, Equip, Un-equip, Ascend/Descend for Subterranean, Bond and
Collapse Network for the Champion and Companions, Draw for the Champion) are added by type and need
no entry. Each has the subtype of its own name (D129).

## Cost syntax

Tokens separated by spaces or `+`:
- **mana:** digits for generic, then Element letters — `L` Light, `F` Fire, `M` Metal, `E` Earth,
  `D` Darkness, `I` Ice, `W` Water, `A` Air. `2FF` = two generic and two Fire. Letters joined by
  `/` are one pip any of them pays (D125): `1F/AW` = one generic, one Fire-or-Air, one Water.
- **Activation Points:** `3AP`, `3~AP` (done for this turn), `2!AP` (exhaust), `5*AP` (once per cycle) — D116.
- **Life:** `2Life`.

Example: `"cost": "1F 3!AP"`.

## Targets

```json
{ "name": "T1", "kind": "Creature", "control": "NotYou", "withinOfSource": 2, "min": 1, "max": 1 }
{ "name": "T2", "kind": "Creature", "relativeTo": "T1", "withinOfTarget": 1, "min": 0 }
```

`kind`: `Creature` (Creature, Companion or Champion), `Actor` (those plus Structure), `Permanent`,
`Terrain`, `Remnant`, `Item`, `Structure`, `Card` (in any Mind), `Trace`. `section` narrows a card
or trace to one section (`"Hand"`, `"Discard"`, `"Pending"`…). `control`: `Any`, `You`, `NotYou`
(a card's controller is its owner). `withinOfSource` counts from the acting permanent. `relativeTo` + `withinOfTarget` names another target: a relation,
kept in the trace and rechecked at resolution (D83).

## Instructions

`{ "verb": ..., "target": ..., "amount": ..., "card": ..., "to": ..., "section": ..., "ability": ..., "mana": ... }`.
`target` names a target, or `self` (the acting permanent), or `here` (the terrain it stands on);
`to` names a second target. Verbs are working names (`PLAN.md` Track A item 39). A ±X lasts as
long as its trace (D144): `"duration": 1` ends it at the end of this player turn.

| Verb | Effect |
|---|---|
| `damage`, `heal` | Change current Life by `amount` (heal stops at max-Life). |
| `draw` | You draw `amount` cards. |
| `quicken`, `slow` | Target gains or loses `amount` current AP (not below 0). |
| `power`, `health`, `ap` | Target gets ±`amount` Power, Health or AP. Health and AP change max and current together (D143). |
| `gain`, `lose` | Target gains or loses `ability`: a keyword (`"Flying"`, `"Ranged 1-3"`) or a card's ability (`"creature.tidecaller/tide-heal"`); a lost one may be named by its id alone (`"move"`). |
| `fell` | Target falls (Remnant for a Creature or Companion). |
| `destroy` | Target ceases to exist, leaving nothing, no fall triggers. A trace: countered. |
| `create` | Create `card` on target terrain (D88), parent and Behavior from the trace. |
| `raise` | Target Remnant becomes a permanent of its card again (D110). |
| `put` | Target becomes a new object: `"section": "Hand"` returns it to its controller's hand as a card, `"Pending"` makes it a trace creating its card's permanent on its terrain (D69). |
| `move` | The same object, a new place: a card to `section` of its owner's Mind (last), a creature onto the `to` terrain (entry check, no path needed). |
| `discard` | Target card in a Hand goes to its owner's Discard. |
| `mill` | The top `amount` cards of a Library go to its Discard: yours, or target Champion's. |
| `shuffle` | Shuffle `section` of your Mind. |
| `copy` | A new object, the original stays: a creature onto the `to` terrain, a card into your Hand, a trace on top of Pending (yours). |
| `add mana` | Add `amount` mana of kind `mana` (`"Darkness"`) to your Champion's pool. |
| `attach`, `detach` | Target Item is carried by the `to` creature, wherever they are; or drops on its carrier's terrain. |
| `gain control` | Target Creature or Companion: your Champion becomes its parent. |
| `unbond` | Target terrain loses its bond record; everything behind it is cut (D122). |
| `darken` | `section` (a Slice: `"Ground"`, `"Sky"`, `"Root"`) of target terrain becomes dark while the trace exists: a permanent there is observed only by its controller and by whoever controls a permanent in that Slice of that terrain (D152). |

## Pretend (D150)

What the opponent of the trace's Champion sees instead, on a Spell or an ability:

```json
"pretend": { "instructions": [ { "verb": "power", "target": "T1", "amount": 1 } ], "duration": 1 }
"pretend": { "instructions": [] }
"pretend": { "card": "spell.might" }
```

`instructions`: what it seems to do; `[]` is "no effect" (secret). Pretended instructions name the
card's own targets; a target none of them names is unseen by the opponent (so "another target
creature" is a second target the true instructions leave alone). `card`: the card it seems to be
(its name, and its instructions when `instructions` is left out). `duration`: how long it seems to
last. On a permanent card, `card` is a face (a Mimic: "5/5 Dragon, pretend 1/1 Goblin"): the
opponent believes the permanent and the card in Discard are that card. The engine runs
pretended stat changes, damage and heal, gain/lose and move; other pretended verbs change nothing.

## Abilities

```json
{ "id": "tower-volley", "name": "Volley", "cost": "2AP", "speed": "Slow",
  "targets": [ ... ], "instructions": [ ... ] }
{ "id": "herald-draw", "name": "Dawn's Insight", "trigger": "EntersIsland",
  "instructions": [ { "verb": "draw", "amount": 1 } ] }
```

`trigger`: `EntersIsland`, `Falls`, `BeginningOfYourTurn`, `EndOfYourTurn` — the permanent's own
events. Triggered abilities take no targets yet. `physical: true` makes the trace physical
(Duration 0, D45). `subtypes` names the ability's kind of action (`Move`, `Attack`…), so static
abilities on that subtype reach it (D129).

## Static abilities

```json
{ "subtype": "Move", "scope": "TargetingThis", "ap": 1 }
```

The one shape built so far (D129): abilities with `subtype` cost `ap` more AP. `scope`: `OnThis` —
abilities of permanents standing on this terrain (leaving it); `TargetingThis` — abilities that
target this permanent or terrain (entering it). Added on top of a connected Champion's or Companion's
own Move, Attack and Defend costs (D134).
