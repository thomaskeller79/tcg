# Playtest Variants — Alternatives Worth Trying

*`history/decisions.md` records the **current** rule for each decision — one answer, the one the game
actually runs on. This document is different: it's where we park **alternatives that lost (or
haven't been decided) but are worth keeping on hand** to actually playtest later, with the
reasoning for/against each, so a promising idea doesn't get lost the moment a default is picked.
Promote an entry into `history/decisions.md` the moment it's actually adopted; leave it here otherwise.*

**Status:** Living list · **Date:** 2026-08-09

---

## Champion realm-constraint movement (D9)

**Current rule:** while connected, the Champion may only move onto hexes it has itself bonded,
at `2AP`; `0AP` Collapse Network drops the whole bond set and frees movement. See `../champions.md`.

### Variant 1 — network follows the Champion at a AP premium · SUPERSEDED
The Champion is the network's root; a pricier `2AP` move keeps the network **active** as it
relocates (nothing pauses), while the cheap `1AP` move (same as any creature) lets the network
**collapse** instead — every bonded terrain no longer reachable is reversibly paused, restored
the moment a path reconnects (same mechanic as enemy blocking, just self-inflicted).

**Why replaced:** never actually restricted *where* a disconnecting move could go, so "keep the
network" was rarely worth the 2AP premium in practice.

### Variant 2 — "reachable through" bonded cells, not "is" a bonded cell · SUPERSEDED
The first lock-in draft required the destination to merely be *reachable through* bonded,
enemy-free cells — in practice this meant any hex adjacent to a bonded cell qualified, not just
the bonded cells themselves.

**Why replaced:** a Champion could still drift one hex outside its actual claimed territory while
"staying connected" — playtesting showed this reads as "I can still move outside of the network."
The current rule requires the destination to *be* a bonded cell, full stop.

## Attack and Defend cost (D15, D116)

**Current rule (D116):** Attack `3~AP`, Defend `1~AP` (option O4 below); the **Defender** keyword
gives `1AP: Defend`. See `history/decisions.md` D116 for the full rationale.

### The six options compared (D116)
`x~AP` = spend x, no more abilities this turn; `x!AP` = drain all remaining AP; `0*AP` = free,
once per own-turn cycle. Results per scenario: ✓ desired outcome, – a matter of preference, ✗ fails.

| | Attack | Defend | S1 | S2a | S2b | S2c | S3 |
|---|---|---|---|---|---|---|---|
| **O1** | `3!AP` | `1!AP` | ✗ | ✓ | – | ✗ | ✗ |
| **O2** | `3!AP` | `1~AP` | ✗ | ✓ | – | ✗ | ✓ |
| **O3** | `3~AP` | `1!AP` | ✓ | ✓ | – | ✓ | ✗ |
| **O4** (adopted) | `3~AP` | `1~AP` | ✓ | ✓ | – | ✓ | ✓ |
| **O5** (old rule, D15) | `3!AP` | `0*AP` | ✓ | ✓ | – | ✓ | ✗ |
| **O6** | `3~AP` | `0*AP` | ✓ | ✓ | – | ✓ | ✗ |

- **S1 — mirror trade.** Two creatures (4 AP, Attack 3, Life 5); a attacks b, b defends, then b
  attacks a in its own turn. Desired: a draw (both die) — attacking first must not be worse than
  being attacked. With `!` on Attack the attacker has 0 AP and can't defend: D15's original bug.
- **S2a — reach pays off.** a (5 AP, Attack 3, Life 4) moves 2 and attacks e (Life 3), killing it.
  Desired: the all-in attack is worth it. All pass.
- **S2b — all in, the target survives.** As S2a, but e has Life 5 and attacks back. Under O1–O4 a
  can't defend (it spent everything); under O5/O6 it defends for free. **Both are legitimate
  styles:** O1–O4 reward getting into position first, O5/O6 reward attacking.
- **S2c — step adjacent and wait.** a moves next to e without attacking; e attacks, a defends; then
  a attacks e. Under O1/O2, e has 0 AP after its own attack and dies undefended — whoever attacks
  first loses, so both sides wait (a standoff). The other four give the symmetric trade.
- **S3 — Neutral bait.** An idle creature (4 AP) is attacked by a Neutral monster in Neutral A's
  turn, then by the other Champion's creature. Desired (user's call): it can defend against each
  opponent once — keeping AP back buys one defence per opponent turn; otherwise monsters become a
  tax on defence against the real opponent. Only the `1~AP` options pass.

**More scenarios** (S4–S10, not played through) and the definitions above: `../examples/combat-scenarios.md`.

**Looked for and not found: a standoff under O4.** The closest case is a **fortress** — a hex full of
Defender-keyword creatures, which gang up (D13) on every attack while attacks come one at a time, so
attacking that hex is always bad. Without the keyword, `1~AP` breaks it: defenders are locked after
one defence per turn, so later attacks the same turn land undefended. The user accepts the fortress:
it is unlikely, and setting one up on a hex important enough to need attacking deserves the reward.
**Known O4 cost:** the `~` lock also stops the defender (or attacker) from using its own abilities
for the rest of that turn — e.g. a pump ability after defending.

### Variant 1 — `1!AP` (Exhaust) · REJECTED, concrete bug found
Defending costs `1!AP` (exhaust to 0), at most once per turn, gated on having AP left. **Why it
failed** (the scenario that killed it): two mirrored creatures (5 Life / 3 Power / full AP)
trade blows — attacker attacks (exhausts itself, default `3!AP`), defender defends (exhausts
itself too, `1!AP`), both take 3 damage. Both are now at 0 AP, but AP only refreshes on its own
controller's *own* Beginning phase — the defender's controller acts next, so it refreshes almost
immediately; the attacker's controller doesn't act again for a full round. In that window the
former defender attacks the still-exhausted former attacker for a free, undefended kill. Net
effect: **attacking first is strictly worse than being attacked** — an incentive against
attacking, the opposite of the intended design. Not a balance nitpick, a structural bug in how
the rule interacts with turn-based AP refresh.

### Variant 2 — the "recovery pulse" idea · proposed 2026-08-09, not evaluated or implemented
Instead of decoupling Defend from AP (what `0*AP` does), fix the *refresh timing* that actually
caused Variant 1's bug, and let Defend stay a normal AP-gated action:
- **(a)** Defend (and other actions) go back to being priced with an ordinary AP cost, e.g.
  `1!AP` again — no bespoke rule for Defend specifically.
- **(b)** New general rule: every actor regains **`1AP`** at the **beginning of the opponent's
  turn** too, on top of the existing full refresh on its own Beginning phase — a second, smaller
  refresh pulse every round instead of one big one. This is what closes Variant 1's gap: an
  exhausted attacker isn't stuck at 0 AP for the opponent's *entire* turn, just until that small
  trickle arrives.
- **(c)** New cost flavor, **`x!!AP`** ("double-exhaust"): like `x!AP`, but the actor also
  **skips the opponent's-turn recovery pulse from (b)** — its exhaustion genuinely lasts the
  full round, not just until the next small trickle. Flavor: the action was so costly it doesn't
  even recover for the quick pulse. `!` vs `!!` becomes a real per-card dial: "this drains you
  for a moment" vs. "this drains you for the whole cycle."

**Argued for (the idea's own appeal):** doesn't special-case Defend at all — a creature "can't
defend" as a *consequence* of being out of AP, exactly like every other AP-gated action, matching
MTG's tapped state (tapped means *both* "can't block" *and* "can't activate tap abilities," one
unified state, not two separate rules). Also opens design space elsewhere: any `1!AP`-costed
ability becomes meaningfully riskier once there's a recovery pulse to weigh against it (skip the
pulse via `!!` for a stronger effect, or take the plain `!` and stay defensible sooner).

**Argued against (the idea's own cost):** touches a foundational rule — AP refresh was, until
now, a single well-understood event (your own Beginning phase, full refresh, no partial states).
Adding a second, smaller refresh point makes "how much AP does an actor have right now" depend on
*two* timing rules instead of one, for every actor, every turn — a permanent complexity increase
across the whole engine, not a one-off card effect. Plus a third cost-flavor axis (`!!`, next to
`!`/`*`/`^`) for players and card designers to track.

**Rough sketch only (2026-10-23, from Track A item 57).** Revisit only if the current combat system
shows problems in playtesting. The user's version: Attack `3!AP`, Defend `1!AP`, every creature gets
1 AP in the opponent's turn, and the `~` flavor is dropped. Open concerns:
- **Acting no longer costs defence.** Every creature can defend once per opponent turn, whatever it
  did in its own turn. A creature that attacked (`3!AP`) still defends next turn. Under D116 a
  5-AP creature that moves 2 and attacks can't. In MTG attacking costs blocking, because the attacker
  taps (unless it has vigilance). `!!` restores the cost only on cards that print it.
- **Connected Champion and Companion defence** costs `2~AP` (D134), so 1 AP doesn't cover it.
  Needs its own answer.
- **"+1 AP" tricks lose their surprise.** A creature at 0 AP no longer looks spent in the
  opponent's turn, because it always gets the pulse.
- **Summoning sickness (item 57):** a fresh creature defends through the pulse. A creature flashed
  in during the opponent's turn still needs Haste 1 to defend right away.

**Not adopted.** D116's `3~AP` / `1~AP` fixes the same bug differently: the attacker keeps its leftover
AP for defending, so no second refresh point is needed — at the price of one new cost flavor (`~`).
Variant 2 stays here as a real alternative worth an actual playtest.

---

## Summoning sickness (D89, D160)

**Current rule (D160):** a permanent enters with 2 Activation Points (placeholder, below the Attack
cost), so it can defend and act cheaply but can't attack until its first refresh. See `../economy.md`.

### Variant — full AP, Quick abilities only until the next Beginning · NOT ADOPTED, fallback
A permanent enters with its max AP but can use only Quick abilities until its controller's next
Beginning phase. Defend is Quick, so it can defend, including right away when flashed in during the
opponent's turn. Move and Attack are Slow (D75), so it can't take them. Closest to MTG: a creature
with summoning sickness can block but can't act on its own initiative. **Why not adopted:** it
needs a "sick" status to track and show, and every Quick ability works at full AP right away. Haste
would mean "may use Slow abilities right away". Keep it in case the combat system shows problems.

---

## Structure control (D24)

**Current rule:** dynamic — a Structure is funded/controlled by whoever *currently* bonds the
terrain cell it sits on, tracking bond changes live, exactly mirroring Terrain's own rule (funding
can be a Companion; the strict controller is that Companion's own Champion, D28). See
`history/decisions.md` D24.

### Variant — fixed at build time · REJECTED in favor of dynamic
Whoever bonded the terrain when the Structure was cast keeps control permanently, even if that
bond later breaks or the cell gets re-bonded by someone else — Structures behave like Creatures,
owned for life once built.

**Argued for:** avoids a real board consequence the dynamic rule introduces — re-bonding
contested or newly-unbonded terrain (e.g. after the Companion that bonded it dies) lets the
opposing root **capture an enemy Structure positionally, without ever attacking it**. A player who
built a Structure might reasonably expect to keep it regardless of what happens to the ground
under it later.

**Argued against (why dynamic won):** fixed-at-build-time is a special case — every other
non-actor permanent (Terrain) already has control track the current bonder live; carving out an
exception for Structure just because it happens to sit on top of one breaks that symmetry for no
new capability. The "capture via re-bond" consequence isn't a bug either — it fits D8's existing
"denial is positional and reversible" shape and gives Structures a distinct vulnerability from
creatures (starve the land under it, don't fight it), which reads as a feature once framed that
way.

**Worth revisiting if:** playtesting finds Structure-capture-via-re-bond feels too strong (a
"free" removal with no combat, no cost) or too situational to matter (contested terrain rarely
gets fought over enough for this to come up) — either direction could argue for reverting to
fixed control.

---

## Creature/permanent location model (D31–D34)

**Current rule:** every spatial permanent's location is uniformly (terrain, layer); "vehicle" and
"transport" flavor (e.g. a Helicopter Item) is achieved through ordinary ability-granting
(Flying, cost changes) on the carrier, never by moving a creature off the board into a container.
See `history/decisions.md` D32.

### Variant — true containment ("inside" a Vehicle/Structure) · REJECTED, not built
A permanent could have a third kind of location beyond (terrain, layer): "inside" another
permanent (a Vehicle, or a Structure hiding a creature within it), removing it from the hex grid
entirely — with its own embark/disembark actions, capacity limits, visibility rules, and rules
for what happens to the contents if the container dies.

**Argued for:** more realistic for genuine transport/ambush flavor — a creature loaded into a
Vehicle would be fully off-grid and untargetable until it disembarks, and a container's death
could threaten its cargo, which ability-granting alone can't express.

**Argued against (why rejected):** doubles the location model for a flavor payoff most cards
don't need — a full containment system needs its own capacity, visibility, and
embark/disembark cost rules, all new surface area. Everything asked for so far (a Cannon
granting Ranged Attack, a Helicopter granting Flying + speed, "hidden in a building") is already
covered by the existing Item-grants-ability pattern plus Below-layer concealment (D31) — no
card yet needs a creature to actually leave the grid.

**Worth revisiting if:** a future card wants a creature to be genuinely untargetable/off-grid
while "loaded," or wants container-death-destroys-cargo — that's the point where ability-granting
stops being enough and true containment earns its complexity.

---

## Item Life (D34)

**Current rule:** an Item has no Life at all — never a combat participant, removed from play only
via an explicit printed effect (destroy/sacrifice), never by combat or splash damage. See
`history/decisions.md` D34.

### Variant — Item has Life, destroyable by damage · REJECTED
Give Item a Life total like Structure, so generic damage/AoE effects could destroy a loose item
lying on a hex (e.g. a Spell that burns everything on a cell).

**Argued for:** more uniform with Structure (which does have Life); opens a design space where
area-damage effects also threaten loose equipment, not just creatures.

**Argued against (why rejected):** Structure's Life exists specifically because it's a combat
participant (attackable, D24) — an Item never is, so a Life total would only ever matter for a
niche AoE-destroys-loose-items interaction nobody has asked for. Simpler and more legible to keep
Item removal strictly card-driven ("destroy target Item" as a printed effect) than to give every
Item a hidden Life stat on the chance a future card wants to burn the ground.

**Worth revisiting if:** a real card idea wants area-damage to sweep loose items off a contested
hex as a genuine tactical option.

---

## Objects leaving every zone (D97)

**Current rule (adopted to try):** an object that leaves every zone ceases to exist; links through
it are joined up, so a permanent can be reached through its trace only while the trace lasts. See
`../object-properties.md` §2.

### Variant — record state · NOT ADOPTED, kept as the fallback
An object that leaves every zone stays as a record: no player can select it, but stored links
(`source`) still reach it, so a permanent stays reachable through its long-faded trace.

**Argued for:** nothing is ever lost; any chain can always be walked back to its card.

**Why not (for now):** it is an exile zone under another name, and it removes the age-hardening
D97 wants to try — permanents becoming harder to reach through their trace as it fades, which makes
trace Duration a design lever.

**Worth revisiting if:** age-hardening isn't fun in play, or cards keep wanting to reach a faded
trace. Switching back only changes what the rules may read; the engine can keep the records anyway.

## What a falling permanent leaves (D110, D111)

**Current rule:** a permanent that falls ceases to exist. A fallen Creature or Companion leaves a new
Remnant on its terrain; a fallen Structure leaves nothing. See `../object-properties.md` §5.

### Variant — Grave/Ruin as a status of the same object (D88) · REVERSED by D110
A dead creature stays the same object, with its own card, in a Grave status.

**Argued for:** references survive death. A trace that kills its target can still read it
afterwards: "destroy T, then create a Warrior within distance 1 of T" works, since the Grave keeps
T's location.

**Why replaced:** death becomes a special case next to Flicker and Bounce. Every Ancestry cascade
had to say "dies or leaves the Island", "target creature" needed a rule to exclude Graves, and
returning a Grave needed its own "reverse the status" rule. As new objects, all three fall out of
existing rules. The lost references are handled by targeting the hex.

**Worth revisiting if:** "do something where it fell" cards turn out frequent and the hex-target
wording reads badly in practice. The other way out is last-known information (MTG 608.2h) — also
rejected for now, since it is a new mechanism and would change Flicker too.

### Variant — every fallen Structure leaves a Ruin (D40, D88, D110) · DROPPED by D111
A fallen Structure leaves a Ruin in its slot: an Object that occupies the slot until a Structure is
cast onto it (replacing it in one action) or an effect clears it. Cards can require or consume
Ruins, and raise rebuilds the Structure.

**Argued for:** symmetry with the Remnant; a battlefield that shows its history; a resource for
"build on a Ruin" cards.

**Why dropped:** no card wanted a Ruin for its own sake. The only named use was "cheaper built on a
Ruin" (a cost reduction, outside the prototype scope); the only prototype card, Watchtower ("can
only target a terrain you control with a Ruin"), existed only because Ruins existed. Yet Ruins cost
a type, a slot rule, and a Structure case for raise — and a leftover every Structure produces by
default, with no reason to stay around, is a generous default (D14). Now a Structure that should
leave a ruin creates one itself with a fall trigger ("When this falls, create Ruined Tower on its
slot"), and the ruin is an ordinary Structure with its own abilities.

**Worth revisiting if:** many Structure cards end up printing the same "leave a ruin" trigger, or
"build on a ruin" turns out to be a strong design space — then a default may pay for itself.

---

## Combat procedure (D117)

**Current rule:** Defend is a Quick activated ability played in response to the Attack trace; its
trace adds the creature to the Attack as a defender; defenders accumulate bit by bit; the Attack
checks at resolution whether it is still defended. See `../interaction-stack.md` §Combat integration.
**Playtest point:** defending bit by bit lets the defender always answer last with another body —
watch whether combat tilts too far toward the defender (with gang-up, D13).

### Variant — combat phase · NOT ADOPTED, worth a playtest
Action → Combat → Action, the combat phase a sequence of steps with priority windows (declare
attackers → window → declare defenders → window → damage), like the Beginning phase. **For:**
several attackers on one hex and "who defends whom", decisions made with every attack visible,
fewer interruptions (two windows and one defender step per combat), a clear combat moment for card
text and the GUI. Never rejected in the records — only postponed as "simpler for now" (D3, D21).
**Against:** loses interleaving (attack, free a hex, move through it), puts priority windows outside
the Aether, and makes Attack not a trace while Move still is. Item 2 (simultaneous actions) could
bring back interleaving and give the multi-attacker depth to moves too.

### Variant — Defend counters Attack · NOT ADOPTED
The user's option: the Attack creates a rule-made, Instant "declare defenders" trace; declaring
defenders creates a Defend trace, whose resolution runs the fight and removes the Attack trace, like
a counterspell; an Attack that still resolves is undefended. Uses the Aether as intended, but needs
rule-made traces, a trace removing another trace, and several Physical Traces in Pending — three
mechanisms the adopted rule doesn't need.

### Variant — physical actions outside the Aether · REJECTED
Each physical action a fixed step sequence with priority windows (Attack: declare → window →
defenders → window → damage), nothing physical in Pending, Quick redefined as "during a physical
action's window". Keeps interleaving, but the defender is prompted once per attack. Rejected by the
user: one more area to keep track of, next to Matter, own Mind and Aether — and it should be
solvable with the Aether.

### Rejected — a two-stage Attack trace
The Attack trace survives one round of passing priority, defenders are declared, then it resolves
after the second. Gives both windows, but is a special rule for Attack traces — exactly what the
user ruled out ("use the Aether as intended or not at all").

## Ranged attacks (D127)

**Current rule:** `Ranged a-b` reaches distances a to b (Melee: 0–1); a creature may Defend only if
the attacker is within its own attack reach (distance only); every defender hits back. The attack
target keeps its Slice. **Playtest point:** the user isn't sure this is intuitive — in particular a
melee defender that can't step in against an archer 2 hexes away, and a Ground archer that hurts a
flyer only by defending.

### Variant — defendable, no retaliation (guess G7) · NOT ADOPTED
Anyone eligible may defend a Ranged attack and absorb it, but nobody hits back. Kept one combat
procedure, but needed Ranged as an exception to universal retaliation.

### Variant — undefendable · NOT ADOPTED
A Ranged attack hits its target directly. A sniper keyword: picks off pieces behind a wall of
defenders, and would want a specific target creature instead of terrain + Slice + entity.

### Variant — "the defender could attack the attacker" · REJECTED
The user's first formulation. Read literally it also checks Slices and "can't attack": Ground
creatures could no longer defend against a diving flyer (Ground can't attack Sky), and a creature
with "can't attack" could never defend — both unwanted. Narrowed to distance only.

### Variant — no Slice in the attack target · REJECTED
Claude: an attack names terrain + entity; a flyer's attack reaches Ground and Sky, the damage split
among both. Removes the one Slice choice, but a flyer could no longer fight enemy flyers only —
Ground creatures could always step in.

### Variant — derive the Slice, "Sky first" · NOT ADOPTED
A flyer attacks the Sky while the enemy has a flyer on that hex, and dives only once the air is
clear. No choice at all, but an extra rule, and no diving past enemy flyers. Kept the rare Slice
choice instead.

## What a raised Remnant brings back (D110)

**Current rule:** a Remnant keeps only the fallen permanent's name; raise builds the permanent from
the printed card, so a permanent change (e.g. a raised max-Life) is lost at the fall. False values
(a Mimic face) end at the fall too — Mimic should state that its face collapses then.

### Variant — the Remnant keeps the true changes · NOT ADOPTED (playtest)
The user's preference: the Remnant carries the fallen permanent's last true values as properties
irrelevant to the Remnant itself, read only by raise, so a raised creature keeps its permanent
changes. Claude against: death should feel like a loss, and killing a buffed creature would be a
weaker answer whenever a raise is around. Kept the printed card as the playtest version; the
scenarios that test it are `PLAN.md` Track C item 13.

## What a contradiction reveals (D146, D164, D165)

**Current rule (D165):** an observation — the certain properties and the damage a creature takes —
that contradicts what an observer's copy carried out destroys, for that observer, the belief
objects behind the false values the instruction read (its checks included), with everything
resting on them; each attachment is then deduced again. One Pretend is one belief object. A belief
that is `?` stays `?`. The first three variants below differ from it only in what is observed.

### Variant — destroy on read (the base solution) · NOT ADOPTED (playtest)
A lie goes as soon as an instruction reads a false value, whatever is observed; rule steps never
reveal. False values then never spread: no derived belief objects, no replay. Weak spots a
playtest must show: paying AP reads AP although "enough AP?" has the same answer both ways, so a
face with a different max AP goes at the creature's first action; a read the player doesn't
observe (a fight in darkness) would reveal to it as well. It still needs a contradicted certain
observation to reveal too (a trace still in the Aether that its version has already ended).

### Variant — "changing": current Life observed whenever it changes · NOT ADOPTED (playtest)
Adds current Life to the observation list whenever it changes. Return damage on a Mimic whose
Life is false then reveals it, even when it takes the same damage both ways.

### Variant — certain properties only · NOT ADOPTED (playtest)
D164's observation list, with the whole belief object destroyed: damage taken isn't observed, so a
hit both sides expect the target to survive reveals nothing, and the lie spreads into the target's
Life as a derived belief object.

### Variant — partial reveal (D164) · REPLACED by D165
A contradiction made only the values the instruction read true, with their max values; the cause
and a Mimic's face stayed. Under D165 it exists only as card design, one Pretend per property.
Dropped as a rule because the lie that stays keeps running: a curse fading in a copy that has
already seen the creature's true Power makes the belief false again, and false values pile up
with nothing to clean them.

### Variant — large reveal (D151) · REPLACED by D165
The contradiction also reveals what caused the false value — the traces and their cards (a
modifier remembers its trace) and the static abilities that contribute to it, traced back on the
true state — and corrects later false beliefs from the same cause. D165 brings it back in another
form: the lie is a belief object, destroyed whole with everything resting on it, without tracing
causes on the true state. D151's history rewrite comes back the same way (§History after a contradiction).

### Variant — observe in every combat · NOT ADOPTED (playtest)
Every combat observes the read values of both creatures, whether or not anything contradicts.
Simpler to explain; one fight ends every hidden combat effect on both creatures. The user keeps
it open if contradiction-only plays strangely.

### Variant — static abilities attached to the trace · NOT ADOPTED (idea, user)
A trace carries the static abilities relevant to its action: an Attack trace carries the
attacker's Ranged, a move carries Flying. One general mechanism — the relevant abilities are part
of the trace — instead of a question of what a contradiction observes; which abilities go on which
trace is the same question as which abilities a check reads. Needs tweaking: a trace's content is
copied from each copy's own belief of the ability (D140), so an attached ability would teach
nothing unless it is true in every copy — and then every use shows it, before anyone responds, not
only a contradiction (stronger than observing in every combat). The user finds it interesting;
D165 keeps "a contradiction destroys the lies behind what the checks read" for now.

### Variant — explicit reveal · FALLBACK
No comparison: an instruction always reveals the values it reads (combat reveals the Attack of
each creature that dealt damage), consistent or not — fully explicit, no check after the
instruction, but every instruction must state what it reveals (the old reveal points, D146).
Ends bluffs that were never tested (a curse hit by damage both sides expected to be lethal).
Fall back to it only if the comparison proves unworkable.

### Variant — a contradiction also fills in unknowns · NOT ADOPTED (playtest)
Every value the instruction read becomes true, `?` included. A ranged creature in darkness whose
Attack bluff is caught then also shows its location, because the range check read it — the bluff
on one property exposes another that terrain hid honestly. Try this variant if caught bluffs feel
too toothless.

### Rejected — fit without observing anything (D164)
A certain observation overwrites only what it observes, and every other belief must still be
made to fit. Each way of making it fit was tried and dropped:
- **`?` for every value the instruction read:** the `?` spreads — damage dealt with a `?` Attack
  makes the target's Life `?`, a death check can't run on `?` Life — and the view fills with
  unknowns.
- **The smallest change that makes the belief fit** (a creature that lives gets Life 1): needs a
  choice when several values could be the cause (the attacker's Attack or the defender's Life).
  Tie-breaks tried: enemy values before your own, then a fixed order of (property, controller)
  pairs by how easy each is to pretend; values an opponent's trace touched, most recent first —
  but a Pretend can lie about the property and the target it names, so the trace's claim is only
  a guess. For a location the smallest change is unclear; "keep where its last move put it" is
  `PLAN.md` Track D item 77.
- **The player picks the new values at every contradiction**, with an automatic rule for the
  neutral seats: every contradiction stops the game, and the neutral seats still need one of the
  rules above.
- **Reasoning over every legal trace** to find the cause: a deduction engine (D18), and it needs
  the opponent's hidden cards.

### Rejected — two lies on one value: the value becomes true (D165)
When one of two lies on one value is destroyed, the value becomes true, though the other lie still
explains part of the gap. Simple, and it errs toward the truth; the user wants the other lie to
stay active (D hit by two bluffing creatures: B believes 4 after one is revealed, not the true 3).

### Rejected — two lies on one value: merge at a cap (D165)
Each lie keeps its own difference on the value; a cap that bites partly (a heal on D that the max
cuts short) merges the differences into one resting on both lies, and destroying either makes the
value true. No replay, but one too low in that case.

### Rejected — how the engine holds beliefs (D165)
- **Offsets from the truth** (the engine before D165): break at every cap, so a heal to max needs
  a hand-written patch, and can't hold a creature that fell in one belief only.
- **Five full copies of the state:** one code path, but every trace and rule step runs five times,
  with a death check, ghosts and all in every copy.
- **A list of changes for every value, recomputed on every read:** exact, but too much computation
  over a long game; D165 keeps such a list only where two or more lies meet on one value.

## Observation (D152, D164)

**Current rule:** one global observation default, overridden by effects for a terrain or a
permanent; a fixed list of certain properties; everything else deduced (D164).

### Variant — observation as abilities of the terrain (D152) · NOT ADOPTED
Every terrain carries one observation ability per Slice (Ground and Sky: everyone; Root: the
permanent's controller, or anyone with a permanent in the Root of this terrain or a neighbouring
one), checked after every instruction. Darkness replaces the ability, found by its subtype. Every
observation rule then sits on an object, as abilities do — but every terrain carries the same
three abilities, and a global default overridable like any rule does the same job. A simplification
was parked with it: an observation range per Slice as an ability parameter, Darkness setting it
to 0.

### Rejected — your own permanents are true in your copy (D155)
A change to a permanent you control was true in your copy unless a Pretend said otherwise; the
controller saw the true damage, whatever it believed about the source. Replaced by "everything
not certain is deduced" (D164): one model for every object, and a pretended enemy Attack shows
its pretended damage on your own creature.

### Rejected — Life certain for your own permanents (D164)
Your creature's Life would be certain in your copy, so you always see the damage it really took.
Kills every hidden curse on your own creatures: the first hit shows the true Life.

### Rejected — the other variants of `PLAN.md` Track A item 72 (D164)
- **A section as a set of possibilities** ("4–5 cards"): advertises a secret draw.
- **Face-down activation as the default:** the opponent observes a trace's existence, section,
  position and controller, its content `?`. Deception for free without a Pretend, but every
  activation a mystery by default.
- **Rule steps applied from the true state to every copy** (the death check, the AP refresh, the
  pool reset): adopted instead, each copy runs them on its own values, so every false belief comes
  from a tampered trace and everything not certain is deduced the same way.
- **On a contradiction, fitting with the death known:** the fall is shown, nothing else changes;
  superseded by observing the read values.

## Neutral beliefs (D162)

**Current rule:** each neutral seat (Neutral A, Neutral B) holds its own belief copy, updated by
the same rules as a Champion's; a Neutral permanent reads its seat's copy. A Pretend reaches both
seats. Nobody sees a seat's copy.

### Variant — a seat reads its Champion's belief · NOT ADOPTED (playtest)
Neutral A's permanents read Champion A's copy, Neutral B's read B's; no extra copies. A predicts
its seat's monsters exactly, B learns about A's belief by watching them, and B's Pretends fool A's
monsters along with A. The seat a monster is put on becomes a strong choice. Needs a monster to
read its own values as true, since A doesn't observe a Neutral-A monster in the Root. Rejected:
it makes Neutral permanents half-allies of a Champion, not independent (D82).

### Variant — a seat reads and contributes to its Champion's belief · NOT ADOPTED (playtest)
As above, and Champion A also observes through its seat's permanents (Root observation counts
them), so Neutral A's monsters are scouts for A. Rejected for the same reason, more strongly.

### Variant — Neutral reads the true values · NOT ADOPTED (playtest)
No copy: a Neutral permanent observes through the terrain abilities, computed on the true state,
and reads every value as true. Monsters can't be bluffed. Each monster then reveals the truth to
both players by how it acts (a "flee from Power ≥5" monster that ignores a Mimic dragon). The
baseline to compare D162 against.

### Rejected — a copy per Neutral permanent
Every Neutral permanent holds its own copy: the cleanest independence, but an Element that creates
many Neutral permanents makes the number of copies infeasible.

## History after a contradiction (D163, D164, D165)

**Current rule (D165):** each copy's history holds the instructions as that copy carried them out.
An entry produced by a lie ("X got +1 Attack" from a Pretend) rests on the lie's belief object,
displayed or not, and is deduced again when it is destroyed ("+0"); `?` entries stay `?`. So a
later history condition never reads a lie the observer has already seen through.

### Variant — the history is never rewritten (D163, D164) · REPLACED by D165
A contradiction never rewrote the history, since D164 revealed no cause. A later history condition
could then mispredict in a copy whose observer already knew the truth (the same card cast twice in
a row contradicts twice). D151 had rewritten it as part of the large reveal; D165 brings that back
as part of destroying a belief object.

