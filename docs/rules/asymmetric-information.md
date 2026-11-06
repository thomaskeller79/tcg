# Asymmetric Information (Pillar 6)

*Champions do not share one view of the board. What each Champion perceives is itself a manipulable, card-driven property.*

**Decisions:** D12, D18, D41, D42, D67, D71, D102, D118, D131, D146, D148, D149, D150, D151, D152, D153, D154, D155, D157, D161, D162, D163, D164, D165 (`history/decisions.md`)

---

## Why this exists
A digital TCG can do something a physical one fundamentally cannot: **give the two Champions genuinely different, actively-manipulated views of the same board.** This is largely unexploited. We treat it as a core design axis, not a garnish:
- **Mimic** — a unit appears to the opponent as a different unit (lies about its identity/stats).
- **Submerged** — units that move "below the surface," invisible to the opponent by default until detected.
- **Mist** — a summoned region where one Champion has vision and the other does not.
- **Bluffs, decoys, hidden deployment, fog** — all as first-class mechanics.

## The core model (D148, D149, D164)
> **Every property of every game object exists five times: its true value and one belief per observer — Champion A, Champion B, Neutral A and Neutral B.** A belief is a value or `?` (unknown). Whether a belief is true or false is derived — it equals the true value or not — never stored. The game record (turn, phase, active player, priority) has a true value only.

```
                 ┌─────────────────────────────────────┐
                 │ each property:                      │
                 │   true value         (authoritative)│
                 │   A's belief          (value or ?)  │
                 │   B's belief          (value or ?)  │
                 │   Neutral A's belief  (value or ?)  │
                 │   Neutral B's belief  (value or ?)  │
                 └─────────────────────────────────────┘
```

**The state** is the game objects with their properties — relations included, as properties pointing at other objects — plus the game record. A Champion's **view**, what the GUI shows it, is its beliefs. The true value and the beliefs are independently tracked and can **all differ at once**; a belief is not a redacted copy of the truth. A belief never changes the true state; the only indirect effect is a neutral seat's copy steering its permanents' Behavior.

**Observers (D162).** Each Champion and each neutral seat is an observer. **An observer's permanents** are, for a Champion, the permanents it controls, and for a neutral seat, the Neutral permanents assigned to it (`neutral-permanents.md`). A neutral seat's copy follows the same rules as a Champion's; a Neutral permanent reads it for its Behavior and its visibility checks. No player and no GUI sees a neutral seat's copy — a player learns what a seat knows only by watching its permanents act. A neutral seat has the same belief structure as a Champion, cards included: in this document, a rule about a Champion's belief applies to every observer. A neutral seat has no Mind of its own, so a Hand's content is `?` for it as for the opponent, and it sees a card change section like the opponent does.

**Worked example:** a creature with 2 Power. Champion A casts `Target creature gets −1 Power. Pretend: no effect.` Champion B then casts `Target creature gets +2 Power. Pretend: no effect.` True value: 2 − 1 + 2 = **3**. A believes **1**, B believes **4**. Three different values, live at once.

**The basic instruction sets every copy (D149).** Every change is **apply(object, f_true, f_A, f_B, f_NA, f_NB)**, one function per copy — "keep" and "set to `?`" included. "Target creature gets +1 Power" is (+1, +1, +1, +1, +1). A selection evaluated per copy ("the creature with the highest Power", as each player believes it) is outside the prototype scope.

**Every belief is one of three kinds (D164):**
- **Observed** (§Observation): which objects an observer can observe at all.
- **Certain** (§Certain properties): the properties that are true whenever the object is observed. No Pretend reaches them.
- **Deduced** (§Deduced properties): everything else, your own permanents included. A deduced value is false only through a lie (§Belief objects); it becomes true when a basic instruction observes it (investigate), or when its lie is destroyed (§Contradiction).

The view tells certain values from deduced ones, and shows where each deduced value came from (`PLAN.md` Track A item 60).

## Observation: one global default (D164)
Which objects an observer observes is one global rule, overridable like any rule:
- **Always:** its own objects in Mind and Island, everything in the Aether, and every terrain.
- **Never:** the opposing cards in Hand, Library and Discard.
- **Ground and Sky:** every opposing permanent there.
- **Root:** an opposing permanent there only in a hex where one of the observer's own permanents in the Root is at distance ≤ 1.

**Effects override the default** for a terrain or a permanent; when several do, the later one counts. **Darkness:** "Creatures you control in this location are observed only by you". **An invisible scout:** "This permanent is observed only by its controller". **Setup** is the instruction that shows the starting cards' backs.

**Sight lost forgets.** A Champion's belief about a permanent's location becomes `?` when it observed the permanent before an instruction and doesn't after it. Whether a Champion observes is computed from the true state, whatever its belief holds. A belief about a location nobody can see — including a false one set by Pretend — therefore stays until sight is gained. Other properties keep their beliefs when sight is lost. A creature that walks into a one-hex darkness becomes `?` to the opponent, though the opponent could know where it is; the player remembers that, the view doesn't.

**Seeing a permanent missing** — observing a location where you believe a permanent is, and it isn't there — makes its location belief `?`, whether it moved, died or never existed. This correction is silent: it observes nothing else, so looking at an empty hex never shows where a hidden creature went.

**Moves.** A move keeps the opponent's location belief by default; observation does the rest. A move's trace names its creature as content; its destination follows §Traces.

## Certain properties (D164)
A certain property is true in an observer's copy whenever the observer observes the object:

| Object | Certain properties |
|---|---|
| Own card in Hand | section, position, name, ID, timestamp, `source`, subtypes, cost, Speed |
| Own card in Library or Discard | section |
| Own permanent | location, instance, name, subtypes, controller |
| Another observer's permanent | location, instance, controller (controller only together with location, `PLAN.md` Track D item 74) |
| Terrain | the capacity of each Slice |
| Trace | section, position, controller, `source` as instance, Duration; its name for its activator |

**Observing a permanent identifies its instance,** not only its name; two identical creatures are never confused. **An observed location can't be pretended** — location decides whether a permanent is observed at all; an illusion uses a decoy, a real permanent with a Mimic face (`PLAN.md` Track D item 75). Terrain Mimic is Track D item 76.

## The Root Slice is structural hidden space (D12, D78)
A hex has three vertical Slices, bottom to top (Root / Ground / **Sky**); the **Root Slice is hidden by default** through the observation default above. This makes concealment a *structural* part of the board, not only a card effect: Submerged movement simply *lives* in the Root Slice.

**The Subterranean keyword (D42)** bundles this structural concealment together with the ability to be in the Root Slice at all — a Unit without Subterranean can't go there by default, so the concealment and the access gate are one package. A card effect can still put a non-Subterranean unit into the Root Slice, and the same concealment applies — concealment belongs to the *Slice*, not to the keyword.

**Proximity (D67)** is per *Champion*, not per creature: any of its own permanents in the Root at distance ≤ 1 gives it Root vision there, recomputed after every instruction and lost again the moment that presence leaves. It costs a real, killable creature, so it's a bounded, costed way to "bring light into the dark." **This is why entering any Slice is blind exactly when the acting Champion has no presence already covering the destination** (`overview.md` §4) — not an "Ascend is safe, Descend isn't" rule: Ascending into a darkened Ground hex is as blind as any Descend, and a Descend next to one's own Root scout is safe.

## Concealment effects specify their own breaking (D118)

There is **no base rule** that attacking or entering a hex reveals what is hidden there. The default Root concealment breaks only through the observation default above — which in practice breaks it on contact, since a Melee attacker or a mover is in or next to the hex. **Every other concealment effect** (Mist, Submerged, further Root effects, Mimic) must state on its card or keyword what happens when:

- **C-a.** its hex is attacked in its Slice group,
- **C-b.** an enemy enters its hex in its Slice group,
- **C-c.** the concealed creature acts itself (attacks, defends, uses an ability).

**Slice groups:** Root alone; Ground and Sky together (Sky defends Ground, D115). Recommended default for C-a/C-b, which a card follows unless it says otherwise: the concealment breaks — attacking or entering Root reveals Root occupants, attacking or entering Ground or Sky reveals Ground and Sky occupants. Breaking concealment lifts only the concealment, not a Mimic face (a Mimic collapses by its own rules, e.g. when it acts), and like Reveal (D102) it is a one-time event.

## Example mechanics (seed — flesh out later)
| Mechanic | Truth | What the opponent perceives |
|---|---|---|
| **Mimic** | A 5/5 dragon | Appears as a common 1/1, until it is revealed or its own collapse applies |
| **Submerged** | A unit on hex X moving under the surface | No unit visible unless opponent has Detection nearby |
| **Mist / Darkness (region)** | Real units maneuvering inside | Units inside are `?` to the opponent (an override of the observation default) |
| **Decoy** | A creature still in the dark | Seen walking out on the other side (Pretend on its location) |
| **Scry/Spy** | Opponent's hidden info | *You* gain knowledge the opponent doesn't know you have |

## The consistency problem & belief model (D18)
The hard core of pillar 6 is not *hiding* a fact but keeping a *false view internally consistent* — and defining what happens when reality contradicts it.

**Governing principle:**
> **Players believe what they see. A lie holds until an observation contradicts it; then the whole lie is gone for that observer.**

- The engine **never fabricates a consistent alternate reality** (rejected: infinite regress, unbalanceable, not authorable). **Cards author what the opponent sees** (Pretend, below); the engine applies it and does nothing to back it up.
- **No deduction engine.** Deduced beliefs change only by carrying out traces, by observation and by contradiction — a sound but incomplete picture by design. The engine enforces a **small, finite, documented set of conservation laws** (e.g. a payment can't take the opponent's count of a mana pool below 0, D157). **All other deduction is the human's.** The mind-game lives in the player, not the CPU.
- **A lie is cheap until reality tests it.** A pretended stat holds until an outcome diverges (unexpected damage, or a fall the belief said wouldn't happen); a claimed cost holds until conservation can't reconcile it. A consistent lie may never be tested at all.
- **Every hidden-information ability may state its own collapse (D131).** A concealment, a Mimic face or any other ability that creates hidden or false information may state when it collapses and what part of it collapses; what is revealed must fit what the observer saw.
- **Reveal (keyword, D102)** sets one player's belief about an object equal to its true value, on purpose. It is a one-time event, not a lasting state.

## Deduced properties: carrying out traces (D149, D164)
**A false belief comes only from a tampered trace.** A trace's content, when it is created or changed, is the only pretendable observation (Mimic included). **Each copy carries out every trace as it believes it, on its own values;** an instruction has no separate output. **Rule steps without a trace** — the death check, the AP refresh, the pool reset — also run in each copy on its own values. The game ends on the true state.

The defaults, which card text may override:

- **Start of game.** Every belief equals the true value, except Library content (`?` for everyone) and Hand content (`?` for the opponent). Permanents in the Root at the start of the game are outside the prototype (`PLAN.md` Track D item 53).
- **Draw** moves the card into the Hand in every copy and reveals its content to its owner; a Champion who can't tell which card moved follows §Cards you lose track of. A secret draw is "Draw a card. *Pretend:* no effect".
- **Discard and mill** move the card into Discard in every copy and keep its content beliefs (D161): a milled card stays `?` for everyone, a discarded card stays known to its owner. A card that wants a card seen says *reveal*. Most cards in Discard are known anyway, because they got there by a cast (below).
- **Cast** creates its trace with every copy true (the opponent sees what is cast), unless the card's Pretend says otherwise.
- **Creating an object from a predecessor** — cast, resolve, Bounce, Remand, Flicker, put, copy — **copies each belief once** from the predecessor: a computed value takes its input's belief, a value with no predecessor (a target, current AP at its entry value, D160) is true, except that a trace shows its targets by observation (§Traces). The cast card in Discard is therefore exactly as known as its trace; after the creation the two are independent.
- **An ordinary change** (+1 Power, damage, a refresh) applies the same function in every copy, so where no Pretend is involved the copies stay true. This holds for your own permanents too: damage from a creature whose Power is pretended shows the pretended amount on your own creature.
- **Targets are trace content.** A trace names its targets, and a Pretend may name others ("Pretend: another target creature gets +1 Power", a second target chosen at cast). **A non-targeted effect names nothing:** each copy applies it to the permanents it believes in.
- **The death check** runs in each copy on its own values: a creature falls in a copy when that copy's damage on it reaches its Health.
- **Gaining a permanent or stealing a card** observes what is certain for your own objects (§Certain properties); the content beliefs carry over. Nothing else moves with it: what the old holder knew stays in its copy.
- **Durations are certain:** a trace's Duration is the same in every copy, so an effect ends everywhere at once. A pretended Duration is outside the prototype (`PLAN.md` Track D item 83).
- **A move** keeps the opponent's location belief (§Observation).
- **The mana pool (D156).** Every other observer's belief of a pool is one count; the holder's copy holds the counts per kind. A credit raises the count by the terrain's production and flips its drawn flag in every copy; add mana raises it by the believed amount; a cost lowers it by the believed pips; the owner's Beginning phase sets it to 0. Pretended costs and gains reach the other observers' copies, true ones don't; color is never checked.

A belief is never disjunctive ("either this one or that one"): every update lands on one object per copy.

**Objects in a belief.** A belief may hold an object that no longer exists or never existed (a Pretend decoy, a creature the copy believes alive), and may lack one it has never observed nor seen affected. **A ghost** — a permanent a copy believes in that isn't there — can't be targeted: it fails the visibility check at binding (`interaction-stack.md`).

**Cards you lose track of (D154).** A card belief always has one section, possibly false, so the counts of Library, Hand and Discard follow from the objects in each belief; its position may be `?`. When an instruction moves a card a Champion can't identify — a draw from a Library whose order it doesn't know — every card in that section whose position the Champion doesn't know becomes **anonymous** for it: every belief about it except its section is forgotten. Example: A shuffles a known Bolt into its Library — section Library, position `?` for both. A draws and sees the card, so A's beliefs stay certain; B sees only a card leaving the Library, so every card B knew there with an unknown position, the Bolt included, becomes anonymous. A card whose position B knows (put on top) stays identified.

**Traces (D155).** A cast trace's content is true for everyone unless the card's Pretend says otherwise, and the cast card in Discard is believed as its trace is. An ability trace is created from its permanent's ability, so each copy is copied once from that Champion's belief of the ability (D140) — automatically. **An ability a copy doesn't believe the permanent has** shows in its trace as it truly is, unless the ability carries its own Pretend; using it is a contradiction (§Contradiction), so a Mimic believed to be a Goblin that uses a Dragon ability loses its face. **A trace shows each target only to Champions who observe it:** a permanent target is named, its location shown only to those who observe it; a location target — one Slice of one terrain — only to those who observe that Slice, which a Champion does where it would observe a permanent it doesn't control. A move inside darkness stays hidden; a move out into the light shows its destination. A visible effect still hints where its hidden source is. **Values in a trace come from beliefs:** "damage equal to its Power" takes each Champion's belief of that Power (D140), whether fixed at activation or computed at resolution.

**Bonds** are true in every copy.

## Belief objects (D165)
**A lie is a belief object:** one lie for one observer. It is not a game object and can't be referenced. One Pretend creates one belief object per observer it fools — a Mimic creates three (B, Neutral A, Neutral B), identical except for the observer. A belief object is attached to every object that carries the lie: the card, the trace, the permanent. Beliefs follow the `source` chain; once the trace has ceased to exist, the permanent links to the card (D97), so the card follows the permanent.

**Derived belief objects.** A false value computed from a lie — the damage a creature with a false Power deals to its target — is a derived belief object, resting on the belief objects it was computed from. Rule steps (AP refresh, death check, fading, credit) also run on each observer's believed values, and what they compute rests on the same belief objects.

**A lie is only ever destroyed, never altered.** Destroying a belief object destroys everything that rests on it. Afterwards each attachment is deduced again from the true source, with what the observer can observe: a value it can observe becomes true, one it can't becomes `?`. A decoy caught while in darkness gets location `?`, not its true location.

**Instructions change believed values.** An instruction that closes the gap — the true and the believed value become equal, such as a heal to max in both — destroys the derived belief object; one that only narrows the gap keeps it.

**Two or more lies on one value.** When one of them is destroyed, the value's changes are replayed with the remaining lies. Example: D (Health 9, no damage) is hit by X and by Z, each truly 3 Power and believed 2; true damage 6, believed 4. A heal of 5 makes the true damage 1 and the believed 0 (a heal never goes below 0). When X's lie is destroyed, the replay gives damage 3 + 2 = 5, healed to 0: B believes 0, the part Z's lie still explains.

**One Pretend is one belief object,** so what is revealed together is card design: a card that wants its Flying to surface without its face uses two Pretends.

## Contradiction (D146, D164, D165)
**What is observed:** the certain properties (§Certain properties) and **the damage a creature takes** — up to what makes it fall, so damage beyond lethal doesn't show — each only for objects the observer observes. Nothing about an object is observed if the object can't be observed.

**When an observation contradicts what an observer's copy carried out,** the belief objects behind the false values the contradicted instruction read are destroyed for that observer, its checks included. A contradiction on a derived belief object destroys its roots. Other observers keep their lies until they observe a contradiction themselves. A lie that fits every observation stays. Rule steps never reveal: a contradiction needs an observation.

Typical contradictions:
- **A creature takes more or less damage than the copy expected.** B believes X has Power 2, truly 3; X hits B's creature K (Health 4). K takes 3 damage, B expected 2: X's belief object goes, and B sees X's true values and its true card.
- **A creature lives that the copy's death check removed, or falls though the copy expected it to live.** It is observed standing or missing.
- **A creature spends more AP than the copy believes it has** (AP spent is public: its trace names the creature).
- **A payment takes the copy's count of a pool below 0** (D157).
- **A permanent takes an action the copy believes illegal.** A check reads static abilities like any other property, so the belief objects behind what the checks read go. Each check names them: a move (Move, Ascend, Descend) reads Flying and the terrain's Move surcharge; an Attack reads Ranged; the timing of an activation reads Haste. Example: a Dragon with Mimic: Goblin, believed to have no Flying, enters the Sky — the Mimic's belief object goes.
- **A permanent uses an ability the copy doesn't believe it has** (§Traces).
- **A check that reads an identity property** (name, card type, subtypes) comes out differently than the copy expected: "Destroy target Goblin" fizzles on a Dragon with Mimic: Goblin, and the Mimic's belief object goes. A separate Pretend on the permanent stays.

**A contradiction never fills in an unknown (D151):** a belief that is `?` because nobody observed it stays `?`. A ranged creature in darkness whose Power bluff is caught keeps its location hidden, though the range check read it.

**A combat is compared once.** Its damage in both directions is one instruction (D13), so the comparison runs after all damage and falls together; a bluff can't surface between attack and return damage. A keyword like First Strike splits the damage into two instructions, each compared on its own.

**Costs.** A cost that changes something an observer sees (felling a visible creature, an unbond, Health paid as damage, D147) shows that change. The reset at the owner's Beginning phase sets the pool to 0 in every copy, so an untested mana bluff is never revealed afterwards.

## Representing deception: Pretend, Mimic, ChooseOne (D18, D150)
**Pretend is the one deception mechanism.** A card states its true effect and, after *Pretend*, what everyone else sees; they believe it. A Pretend remembers its caster on the object: the true copy and the caster's copy apply the true effect; every other copy — the opponent's and both neutral seats' (D162) — applies the pretend, so four casters give four different wrong versions. For a permanent's own Mimic, the caster is its owner, not its current controller. Each fooled observer gets its own belief object (§Belief objects).

```
Mirage Might    Target creature gets +0 Power.  Pretend: target creature gets +1 Power.
```

**Secret and concealed are forms of Pretend:** *secret* is "Pretend: no effect" — the opponent doesn't even see the target; *concealed* is "Pretend: does nothing to target creature" — the target is seen, nothing seems to happen. An effect never turns a belief into `?`; `?` is for what nobody has seen, or what a Champion lost track of (§Observation, §Cards you lose track of). Cards may name these forms with different keywords; that is card wording, one mechanism in the rules. Pretend works on any deduced property — on the location of a creature the opponent can't see too, moving its belief.

**Every copy applies the effect as it believes the trace reads.** So deceiving a player about an ability's text carries into everything the ability does, with no extra rule.
**Pretend on a creation (prototype, D155).** Creating a permanent is never hidden: a Pretend may only show a different permanent — a Mimic face, or "create 2 Fire Warriors. *Pretend:* create 2 Water Elementals". Every permanent is therefore in every belief, with values. An ability may carry its own Pretend, for deception beyond what the beliefs about its permanent already give.

**`ChooseOne[…]`** — a modal effect; the caster picks **privately**, and the choice reaches the true copy and the caster's copy. **Mimic** is Pretend on the cast: "5/5 Dragon. *Pretend:* 1/1 Goblin" — every copy but the true one and the caster's reads Goblin, and the permanent and the card in Discard copy that belief (D140). A permanent's name isn't certain for other observers, so the face holds until a contradiction destroys its belief object (§Contradiction), its own stated collapse applies (D131), or a reveal; then everything it hid is deduced again. The face must be plausible: a Flying Dragon entering the Sky as a Goblin is caught at once. A Pretend that shows a different card type without Mimic is outside the prototype (`PLAN.md` Track D item 81).

**Worked card:**
```
Deceptive Boon   cost {mana 2}
  ChooseOne[ target creature gets +2 Power , target creature gets +3 Health ]
  Pretend: target creature gets +3 Health
```
The honest cast and the +2-Power bluff are **indistinguishable** to the opponent. Plausibility of the pretend is the **designer's** job (pick something a real card could do); the engine only enforces conservation.

## Architectural consequences (real costs, accepted now)
1. **Networking is server-authoritative and view-redacted.** Clients receive only their own view — never true state. A modified client must be unable to reveal hidden info. (Reinforces the deterministic-core + server-authority design; kills any trust-the-client shortcut.)
2. **The engine emits per-observer event streams.** One true event → up to N transformed/redacted events, one per observer. "Unit moved into mist" may be a full event to its owner and absent (or a vague signal) to the opponent.
3. **AI must handle imperfect information.** Proper play needs belief states over hidden facts — genuinely hard. Prototype AIs may see true state to function, but that is *unfair by construction* for a deception game; the AI's perception must eventually be gated to its own view.
4. **UI renders a view, never true state.** Two clients, two renderings. Replays are also per-observer (or an omniscient "director" view for post-game review).
5. **Hotseat caveat.** Shared-screen hotseat conflicts with hidden info (both players see everything). Options: a "pass-the-device / hide screen" step, or degrade hidden-info fidelity in hotseat only. Affects mode ordering (see PLAN §5).

## Invariants vs. mutable
- **Invariant (never removed):** a true state exists; a per-observer view layer always exists; every Champion always has *some* view; the server is the sole authority on true state.
- **Mutable (card-driven):** what any observer sees, what any object appears as, what any region reveals/conceals, what knowledge a Champion holds.

## Open questions
2. **Bluff economy:** can a player deploy face-down/unknown units generally, or only via specific cards?
3. **Fairness of information:** do players get told *that* they lack information (e.g., "there is fog here") or can information be hidden so completely they don't know to look?
4. **Reveal / view-merge (D12):** when a "reveal" effect exposes the hidden **below** layer, how does the owner's hidden sub-view merge into the opponent's view — *scope* (one hex / region / all), *duration* (instant snapshot / lasting), *granularity* (existence only / full identity)? Structurally already supported (reveal = a visibility-modifier on the perception layer); the mechanic shape is the open part.
5. **Which static abilities each check reads** — named per check for now (§Contradiction); ability types may take this over (`PLAN.md` Track C item 10).
