# Scenario text format

A scenario file describes a Map and both players' components; loading it runs the real Setup
(`docs/rules/setup.md`, D114) and then applies optional **debug edits** on top. Plain text, one
declaration per line; blank lines and lines starting with `#` are ignored. Tokens are separated
by spaces; wrap a token in `"double quotes"` to include spaces. `card*3` means three copies.
Unknown keywords or malformed lines fail with a `line N: ...` message.

Cards are referenced by id from the card library in `content/cards/` (format:
`docs/tools/card-format.md`). Players are `A` and `B`. Coordinates are axial `q,r`.

## The match

```
name <text>                       # shown in the UI
seed <int>                        # default 0; drives every shuffle and random deal
map hexagon <radius>              # Layout: a hexagon around (0,0)
map rect <width> <height>         #   or a rectangle
void <q,r> ...                    # holes: Void terrain (can't be entered or bonded)
type <TerrainType> <q,r> ...      # flavour Terrain Type per hex (D52)
start <A|B> <q,r>                 # home tile
home <A|B> radius=<n>             # home ground = hexes within n of the start (Void excluded)
home <A|B> <q,r> ...              #   or an explicit list
neutral random <card> ...         # neutral ground: uniform random from this pool (with replacement)
neutral fixed <q,r>=<card> ...    #   or fixed per hex
neutral fill <card>               #   or one card everywhere not otherwise set
champion <A|B> <card>
terraindeck <A|B> <card> ...      # must be exactly as large as that home ground (D11)
deck <A|B> <card> ...             # main deck; top of the Library = first listed when unshuffled
noshuffle                         # keep deck order as written (tests)
openinghand <n>                   # default 5
startap <n>                       # Champions' AP at Setup: max(current, n), default 4
neutralpermanent <card> <q,r> behavior=Aggressive:<A|B> seat=<NeutralA|NeutralB> [slice=<Root|Ground|Sky>]
```

`neutralpermanent` places a Map-placed Neutral permanent at Setup S8; the scenario must state its
Behavior and neutral turn (D82). Mulligans are not implemented: every player keeps
(`docs/architecture/implementation-plan.md` G13).

## Debug edits (applied after Setup, not part of the rules)

```
place <A|B> <card> <q,r> [slice=<Root|Ground|Sky>] [ap=<n>] [life=<n>]
bond <A|B> <q,r> ...              # bond terrain directly to that Champion (cut rules still apply)
mana <A|B> <Element> <n>          # add mana to the Champion's pool (resets at its next Beginning)
handcard <A|B> <card> ...         # put cards straight into the hand
```

`place` creates a permanent controlled by that Champion with full Activation Points unless `ap=`
says otherwise; its "enters the Island" triggers resolve immediately. A `bond` that has no path
back to the Champion's tile is cut at once (D122), so list a connected chain.

## Example

```
name Combat lab
map hexagon 3
start A 0,3
start B 0,-3
home A radius=0
home B radius=0
neutral fill terrain.fire
champion A champion.pyra
champion B champion.thorn
terraindeck A terrain.fire
terraindeck B terrain.earth
openinghand 0
place A creature.fire-warrior 0,1
place B creature.stone-brute 0,0
handcard A spell.flame-dart
mana A Fire 2
```
