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
| `keywords` | permanents | `Flying`, `Subterranean`, `Knotting`, `Defender`, `Haste`, `Haste 2`, `Ranged 2`. |
| `abilities` | permanents | Printed activated and triggered abilities, below. |
| `removedDefaults` | permanents | Default abilities this card lacks, e.g. `["equip"]`. |
| `targets`, `instructions`, `duration` | Spell | Rules text; Duration defaults to 5 (D50). |
| `produces`, `moveCost` | Terrain | Mana per draw, one entry per mana: an Element (`"Fire"`), Elements joined by `/` for mana spendable as any of them (`"Fire/Metal"`), or `"Colorless"` (D125); move cost, default 1. |
| `carrierAttack`, `carrierLife` | Item | Static bonus to the carrier. |
| `entersSlice` | permanents | Overrides the Slice filter (default: Flying → Sky, else Ground; a Structure takes the Ground or Root slot). |
| `bondAp` | Companion | Its Bond cost, default 3 (`3*AP`). |
| `text` | all | Reminder text for the UI. |

Default abilities (Move, Attack, Defend, Equip, Un-equip, Ascend/Descend for Subterranean, Bond,
Draw, Collapse Network for the Champion) are added by type and need no entry.

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
`Terrain`, `Remnant`, `Item`, `Structure`. `control`: `Any`, `You`, `NotYou`. `withinOfSource`
counts from the acting permanent. `relativeTo` + `withinOfTarget` names another target: a relation,
kept in the trace and rechecked at resolution (D83).

## Instructions

`{ "verb": ..., "target": ..., "amount": ..., "card": ..., "stat": ..., "untilEndOfTurn": ... }`.
`target` names a target, or `self` (the acting permanent), or `here` (the terrain it stands on).

| Verb | Effect |
|---|---|
| `damage`, `heal` | Change current Life by `amount` (heal stops at max-Life). |
| `draw` | You draw `amount` cards. |
| `gainAp` | Target gains `amount` Activation Points. |
| `buff` | `stat` `Attack` or `Life` by `amount`; permanent (max tier) unless `untilEndOfTurn`. |
| `fell` | Target falls (Remnant for a Creature or Companion). |
| `destroy` | Target ceases to exist, leaving nothing, no fall triggers. |
| `create` | Create `card` on target terrain (D88), parent and Behavior from the trace. |
| `raise` | Target Remnant becomes a permanent of its card again (D110). |
| `bounce` | Target returns to its controller's hand as a new card (D69). |
| `flicker` | Target becomes a new Permanent trace on its terrain (D69). |
| `unbond` | Target terrain loses its bond record; everything behind it is cut (D122). |

## Abilities

```json
{ "id": "tower-volley", "name": "Volley", "cost": "2AP", "speed": "Slow",
  "targets": [ ... ], "instructions": [ ... ] }
{ "id": "herald-draw", "name": "Dawn's Insight", "trigger": "EntersIsland",
  "instructions": [ { "verb": "draw", "amount": 1 } ] }
```

`trigger`: `EntersIsland`, `Falls`, `BeginningOfYourTurn`, `EndOfYourTurn` — the permanent's own
events. Triggered abilities take no targets yet. `physical: true` makes the trace physical
(Duration 0, D45).
