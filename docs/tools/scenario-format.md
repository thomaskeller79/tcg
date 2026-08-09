# Scenario text format

A scenario file is plain text, one declaration per line. Blank lines and lines starting with
`#` are ignored. Each line's first word is a keyword; the rest are space-separated tokens.
Wrap a token in `"double quotes"` to include spaces (e.g. a card name). Tuning fields use
`key=value` pairs (no spaces around `=`); a comma-separated `key=a,b,c` list has no spaces
either. Unknown keywords or malformed lines fail to load with a `line N: ...` message —
authoring mistakes are meant to be caught immediately, not silently ignored.

Two players only, always named `P1`/`P2` (a codebase-wide assumption — see `PlayerId`).

## Declarations

```
seed <int>                                  # default 0
defendRule <Exhaust|DeleteDefendOnce>       # default Exhaust
board <width>x<height>                      # default 4x4, a plain rectangle
terrain <q>,<r> <name> [moveCost=<int>]     # tags one cell; default moveCost=1 (see note below)
card <id> <Creature|Champion|Rite> "<name>" [attack=N] [life=N] [ap=N] [mana=N]
     [abilities=a,b,c] [effect=damage|heal] [amount=N]
champion <owner> <cardId> <q>,<r>
creature <owner> <cardId> <q>,<r> [layer=Ground|Below|Above]   # default Ground
bond <owner> <q>,<r>                        # terrain already bonded at match start
library <owner> <cardId>[,<cardId>...]      # repeatable per player; appends
hand <owner> <cardId>[,<cardId>...]         # repeatable per player; appends
```

`attack`/`life`/`ap`/`mana`/`amount` default to 0 when omitted; `abilities` defaults to none.
A `Rite` card's `attack`/`life`/`ap` are meaningless (it's never a board actor) — leave them
out. `effect`/`amount` are meaningless for `Creature`/`Champion` — leave them out.

`library` order is the draw order (first-listed = top = next card drawn) and is never
shuffled — for a testing tool, "draw exactly this card next" beats realism.

**`terrain` no longer restricts bonding (D8, revised 2026-08-08).** A Champion may bond *any*
hex, tagged or not — `terrain` is now purely cosmetic/flavor data (a placeholder for a future
terrain-type/color system) and carries no legality effect today. It's still useful for
`moveCost` overrides on a specific cell, just not for marking "bondable" cells anymore.

`bond` pre-bonds a terrain node **at match setup**, before any legality check runs — the same
"setup, not gameplay" footing as initial placement, so unlike a live in-match Bond command it is
**not** restricted to the Champion's own tile. It only affects the *bond*, not the mana that
flows from it — a bonded-but-disconnected node (e.g. no path to the Champion) still produces
nothing, same as an ordinary in-match bond. In actual play, a Champion's **first live Bond is
always its own current tile** (D8) — every current scenario pre-bonds each Champion to its home
hex with `bond` for exactly this reason (1 mana live from turn 1), except `terrain-bonding.scenario`,
which is deliberately left unbonded to exercise that first live Bond directly.

## Example

```
seed 6
defendRule Exhaust
board 4x4

card test.champion Champion "Champion" attack=2 life=15 ap=7 abilities=core.move,core.attack,champion.bond,champion.draw,champion.collapse
card test.grunt Creature "Grunt" attack=3 life=5 ap=3 mana=2 abilities=core.move,core.attack
card test.firebolt Rite "Firebolt" mana=1 effect=damage amount=3

champion P1 test.champion 0,0
champion P2 test.champion 3,3
creature P1 test.grunt 1,1

bond P1 0,0
bond P2 3,3

library P1 test.grunt,test.grunt,test.firebolt
hand P1 test.firebolt
```
