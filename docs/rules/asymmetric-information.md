# Asymmetric Information (Pillar 6)

*Champions do not share one view of the board. What each Champion perceives is itself a manipulable, card-driven property.*

**Decisions:** D7, D12, D18, D41, D42, D67, D71, D102, D118, D131, D139, D140, D146, D148, D149, D150, D151, D152, D153, D154, D155, D161 (`history/decisions.md`)

---

## Why this exists
A digital TCG can do something a physical one fundamentally cannot: **give the two Champions genuinely different, actively-manipulated views of the same board.** This is largely unexploited. We treat it as a core design axis, not a garnish:
- **Mimic** — a unit appears to the opponent as a different unit (lies about its identity/stats).
- **Submerged** — units that move "below the surface," invisible to the opponent by default until detected.
- **Mist** — a summoned region where one Champion has vision and the other does not.
- **Bluffs, decoys, hidden deployment, fog** — all as first-class mechanics.

## The core model (D148, D149)
> **Every property of every game object exists three times: its true value, Champion A's belief and Champion B's belief.** A belief is a value or `?` (unknown). Whether a belief is true or false is derived — it equals the true value or not — never stored. The game record (turn, phase, active player, priority) has a true value only.

```
                 ┌──────────────────────────────┐
                 │ each property:               │
                 │   true value   (authoritative)│
                 │   A's belief   (value or ?)   │
                 │   B's belief   (value or ?)   │
                 └──────────────────────────────┘
```

**The state** is the game objects with their properties — relations included, as properties pointing at other objects — plus the game record. A Champion's **view**, what the GUI shows it, is its beliefs. True value and both beliefs are independently tracked and can **all three differ at once**; a belief is not a redacted copy of the truth.

**Worked example:** a creature with 2 Power. Champion A casts `Target creature gets −1 Power. Pretend: no effect.` Champion B then casts `Target creature gets +2 Power. Pretend: no effect.` True value: 2 − 1 + 2 = **3**. A believes **1**, B believes **4**. Three different values, live at once.

**The basic instruction sets every copy (D149).** Every change is **apply(object, f_true, f_A, f_B)**, one function per copy — "keep" and "set to `?`" included. "Target creature gets +1 Power" is (+1, +1, +1). In the prototype the object is resolved on the true state and used in every copy; a selection evaluated per copy ("the creature with the highest Power", as each player believes it) is outside the prototype scope. Beliefs then change in exactly three ways: by an instruction's own update (§Beliefs), by observation (§Observation) and by contradiction (§When a belief breaks).

## Observation: abilities of the terrain (D152)
There is no global observation rule. **Every terrain carries one observation ability per Slice**, a static ability checked after every instruction: where a Champion observes a permanent's location, its belief about that location becomes the true value.

- **Ground:** every Champion observes the location of every permanent on this terrain's Ground.
- **Sky:** every Champion observes the location of every permanent in this terrain's Sky.
- **Root:** a Champion observes the location of a permanent in this terrain's Root if it controls that permanent, or controls a permanent in the Root of this terrain or of a neighbouring terrain (D67).

**Observation corrects silently:** it sets a location belief to the true value (or `?`) and never triggers a contradiction or its large reveal — revealing the read values would show where a hidden creature went just because someone looked at an empty hex (D153).

**Sight lost forgets.** A Champion's belief about a permanent's location becomes `?` when it observed the permanent before an instruction and doesn't after it. Whether a Champion observes is computed from the true state, whatever its belief holds. A belief about a location nobody can see — including a false one set by Pretend — therefore stays until sight is gained or a contradiction breaks it. Other properties keep their beliefs when sight is lost.

**Changing observation.** An effect replaces a terrain's observation ability, found by its subtype (D129), with its own text for its duration. Effects changing the same ability apply in the order they were created; the later one counts. Example — **Darkness:** "Target terrain's Ground becomes dark, Duration 2": while it lasts, a Champion observes a permanent on that Ground only if it controls it or controls a permanent on the Ground of the same terrain. A creature that walks into a one-hex darkness becomes `?` to the opponent, though the opponent could know where it is; the player remembers that, the view doesn't.

**Moves.** A move keeps the opponent's belief by default; observation does the rest. A move's trace names the creature to everyone (D153); its destination follows §Traces.

## The Root Slice is structural hidden space (D12, D78)
A hex has three vertical Slices, bottom to top (Root / Ground / **Sky**); the **Root Slice is hidden by default** through its observation ability above. This makes concealment a *structural* part of the board, not only a card effect: Submerged movement simply *lives* in the Root Slice.

**The Subterranean keyword (D42)** bundles this structural concealment together with the ability to be in the Root Slice at all — a Unit without Subterranean can't go there by default, so the concealment and the access gate are one package. A card effect can still put a non-Subterranean unit into the Root Slice, and the same concealment applies — concealment belongs to the *Slice*, not to the keyword.

**Proximity (D67)** is per *Champion*, not per creature: any of its own permanents in the Root of that hex or a neighbouring one gives it Root vision there, recomputed after every instruction and lost again the moment that presence leaves. It costs a real, killable creature, so it's a bounded, costed way to "bring light into the dark." **This is why entering any Slice is blind exactly when the acting Champion has no presence already covering the destination** (`overview.md` §4) — not an "Ascend is safe, Descend isn't" rule: Ascending into a darkened Ground hex is as blind as any Descend, and a Descend next to one's own Root scout is safe.

## Concealment effects specify their own breaking (D118)

There is **no base rule** that attacking or entering a hex reveals what is hidden there. The default Root concealment breaks only through the Root observation ability above — which in practice breaks it on contact, since a Melee attacker or a mover is in or next to the hex. **Every other concealment effect** (Mist, Submerged, further Root effects, Mimic) must state on its card or keyword what happens when:

- **C-a.** its hex is attacked in its Slice group,
- **C-b.** an enemy enters its hex in its Slice group,
- **C-c.** the concealed creature acts itself (attacks, defends, uses an ability).

**Slice groups:** Root alone; Ground and Sky together (Sky defends Ground, D115). Recommended default for C-a/C-b, which a card follows unless it says otherwise: the concealment breaks — attacking or entering Root reveals Root occupants, attacking or entering Ground or Sky reveals Ground and Sky occupants. Breaking concealment lifts only the concealment, not a Mimic face (a Mimic collapses by its own rules, e.g. when it acts), and like Reveal (D102) it is a one-time event.

## Example mechanics (seed — flesh out later)
| Mechanic | Truth | What the opponent perceives |
|---|---|---|
| **Mimic** | A 5/5 dragon | Appears as a common 1/1, until it acts/is revealed |
| **Submerged** | A unit on hex X moving under the surface | No unit visible unless opponent has Detection nearby |
| **Mist / Darkness (region)** | Real units maneuvering inside | Units inside are `?` to the opponent (a replaced observation ability) |
| **Decoy** | A creature still in the dark | Seen walking out on the other side (Pretend on its location) |
| **Scry/Spy** | Opponent's hidden info | *You* gain knowledge the opponent doesn't know you have |

## The consistency problem & belief model (D18)
The hard core of pillar 6 is not *hiding* a fact but keeping a *false view internally consistent* — and defining what happens when reality contradicts it.

**Governing principle:**
> **Players believe what they see. A lie holds until an outcome they see contradicts it; then the truth is applied, and the lie collapses.**

- The engine **never fabricates a consistent alternate reality** (rejected: infinite regress, unbalanceable, not authorable). **Cards author what the opponent sees** (Pretend, below); the engine applies it and does nothing to back it up.
- **No deduction engine.** Beliefs change only by an instruction's update, by observation and by contradiction — a sound but incomplete picture by design. The engine enforces a **small, finite, documented set of conservation laws** (e.g. a payment can't take the opponent's count of a mana pool below 0, D157). **All other deduction is the human's.** The mind-game lives in the player, not the CPU.
- **A lie is cheap until reality tests it.** A pretended stat holds until an outcome diverges (unexpected damage, or a fall the belief said wouldn't happen); a claimed cost holds until conservation can't reconcile it. A consistent lie may never be tested at all.
- **Every hidden-information ability may state its own collapse (D131).** A concealment, a Mimic face or any other ability that creates hidden or false information may state when it collapses and what part of it collapses; otherwise the contradiction rule (§When a belief breaks) applies. What is revealed must fit what the observer saw — the game must never make a player remember that a creature shown as 2 power really dealt 4.
- **Reveal (keyword, D102)** sets one player's belief about an object equal to its true value, on purpose. It is a one-time event, not a lasting state.

## Beliefs: what instructions do by default (D149, D140)
Each instruction sets every copy itself. The defaults, which card text may override:

- **Start of game.** Every belief equals the true value, except Library content (`?` for everyone) and Hand content (`?` for the opponent). Permanents in the Root at the start of the game are outside the prototype (`PLAN.md` Track D item 53).
- **Draw** moves the card into the Hand in every copy and reveals its content to its owner; a Champion who can't tell which card moved follows §Cards you lose track of.
- **Discard and mill** move the card into Discard in every copy and keep its content beliefs (D161): a milled card stays `?` for everyone, a discarded card stays known to its owner. A card that wants a card seen says *reveal*. Most cards in Discard are known anyway, because they got there by a cast (below).
- **Cast** creates its trace with every copy true (the opponent sees what is cast), unless the card's Pretend says otherwise.
- **Creating an object from a predecessor** — cast, resolve, Bounce, Remand, Flicker, put, copy — **copies each belief once** from the predecessor: a computed value takes its input's belief (current Health from max), a value with no predecessor (a target, current AP at its entry value, D160) is true, except that a trace shows its targets by observation (§Traces). The cast card in Discard is therefore exactly as known as its trace; after the creation the two are independent.
- **An ordinary change** (+1 Power, damage, a refresh) applies the same function in every copy, so a visible creature's numbers stay true. Every instruction names the permanents it affects to every Champion, never their location (§Objects in a belief).
- **Your own permanents:** a change to a permanent a Champion controls is true in its copy, unless a Pretend says otherwise (D155) — so a damaged creature's controller sees the true amount, whatever it believes about the source.
- **The death check** runs on the true state and applies the fall to every copy: a death names the creature that fell, not where.
- **Durations run per copy:** a trace's Duration may differ between copies, so an effect can end in one copy and not in another.
- **A move** keeps the opponent's location belief (§Observation).
- **The mana pool (D156).** The opponent's belief of a pool is one count; the controller's copy holds the counts per kind. A credit raises the count by the terrain's production and flips its drawn flag in every copy; add mana raises it by the believed amount; a cost lowers it by the believed pips; the owner's Beginning phase sets it to 0. Pretended costs and gains reach the opponent's copy, true ones don't; color is never checked.

A belief is never disjunctive ("either this one or that one"): every update lands on one object per copy.

**Objects in a belief (D153).** **Every instruction identifies the permanents it affects to every Champion** — which instance, never where. A hidden creature's +1 Power tells the opponent "#12 gets +1 Power"; its belief of #12 updates and the location stays hidden. A permanent's controller is therefore always known. Pretend may name a different object ("Pretend: another target creature gets +1 Power", a second target chosen at cast); a card that wants a truly secret target uses Pretend: no effect. **Observing a permanent identifies its instance,** not only its name; two identical creatures are never confused by default. A belief may hold an object that no longer exists or never existed (a Pretend decoy), and may lack one it has never observed nor seen affected. **Seeing a permanent missing** — observing a location where you believe a permanent is, and it isn't there — makes its location belief `?`, whether it moved, died or never existed.

**Cards you lose track of (D154).** A card belief always knows the card's section, so the counts of Library, Hand and Discard follow from the objects in each belief; its position may be `?`. When an instruction moves a card a Champion can't identify — a draw from a Library whose order it doesn't know — every card in that section whose position the Champion doesn't know becomes **anonymous** for it: every belief about it except its section is forgotten. Example: A shuffles a known Bolt into its Library — section Library, position `?` for both. A draws and sees the card, so A's beliefs stay certain; B sees only a card leaving the Library, so every card B knew there with an unknown position, the Bolt included, becomes anonymous. A card whose position B knows (put on top) stays identified.

**Traces (D155).** A trace's section and position are always true. A cast trace's content is true for everyone unless the card's Pretend says otherwise, and the cast card in Discard is believed as its trace is. An ability trace is created from its permanent's ability, so each copy is copied once from that Champion's belief of the ability (D140) — automatically; a Mimic believed to be a Goblin that activates a Dragon ability contradicts that belief. **A trace shows each target only to Champions who observe it:** a permanent target is always named (D153), its location only to those who observe it; a location target — one Slice of one terrain — only to those who observe that Slice, which a Champion does where it would observe a permanent it doesn't control. A move inside darkness stays hidden; a move out into the light shows its destination. A visible effect still hints where its hidden source is. **Values in a trace come from beliefs:** "damage equal to its Power" takes each Champion's belief of that Power (D140), whether fixed at activation or computed at resolution, and the comparison reveals the difference only when an outcome shows it.

**Bonds,** and a trace's section and position, are true in every copy. For permanents found again, beliefs other than location are kept (§Observation).

## When a belief breaks (D146, D151)
**Contradiction by comparison.** Every copy runs the instruction with its own values; for each observer, the outcomes it sees are compared with what its own copy predicted. If they match, its beliefs stand: a lie survives as long as it is consistent. If they differ, the **large reveal** applies: every value the instruction read becomes true for that observer, **and** so does everything that caused the false value — the traces and their cards (a modifier remembers its trace, D144) and the static abilities that contribute to it. The trace back runs on the true state. A keyword may state its own collapse instead (D131).

Example: B believes X has Power 3, truly 2, because of a Pretend. X hits B's creature K (4 Health): B's copy predicted 3 damage, B sees 2 (damage is applied with the true amount in the controller's copy). The outcomes differ, so B's belief of X's Power becomes 2, and the Pretend card and its trace are revealed to B. Had the hit been lethal in both copies, nothing would differ and the lie would survive: what is compared is the state the observer sees afterwards — Health left, or fallen — not the damage number. A Mimic face collapses the same way — when an outcome contradicts it, not at a combat as such.
**A contradiction never fills in an unknown:** the large reveal corrects false beliefs only; a belief that is `?` stays `?`. A ranged creature in darkness whose Power bluff is caught keeps its location hidden, though the range check read it; a decoy's location is false, not `?`, so it is corrected when the creature acts from its real place.
**Health and AP** follow the same rule: a creature that lives although a Champion's belief put its Health at 0 or less, or pays more AP than the belief says it has, contradicts that belief; the value becomes true and its cause is revealed. AP spent is seen, since every action names its creature (D153).
**A combat is compared once.** Its damage in both directions is one instruction (D13), so the comparison runs after all damage and falls together; a bluff can't surface between attack and return damage. A keyword like First Strike splits the damage into two instructions, each compared on its own.

The small reveal (read values only) and the explicit reveal (every instruction reveals what it reads, no comparison) are kept as alternatives (`history/playtest-variants.md`).

**Costs.** A cost that changes something an observer sees (felling a visible creature, an unbond, Life paid as damage, D147) shows that change. A payment that takes the opponent's count of a pool below 0 contradicts it, with the large reveal (D157). The reset at the owner's Beginning phase makes the count true in every copy, so an untested mana bluff is never revealed afterwards.

## Representing deception: Pretend, Mimic, ChooseOne (D18, D150)
**Pretend is the one deception mechanism.** A card states its true effect and, after *Pretend*, what the opponent sees; the opponent believes it. The true copy and the controller's copy apply the true effect, the opponent's copy the pretend:

```
Mirage Might    Target creature gets +0 Power.  Pretend: target creature gets +1 Power.
Mirage 3        Target creature gets +1 Power.  Pretend: target creature gets +1 Power until end of turn.
```

**Secret and concealed are forms of Pretend:** *secret* is "Pretend: no effect" — the opponent doesn't even see the target; *concealed* is "Pretend: does nothing to target creature" — the target is seen, nothing seems to happen. An effect never turns a belief into `?`; `?` is for what nobody has seen, or what a Champion lost track of (§Observation, §Cards you lose track of). Cards may name these forms with different keywords; that is card wording, one mechanism in the rules. Pretend works on any property — on a location too, moving the opponent's belief of a creature it can't see.

**Every copy applies the effect as it believes the trace reads.** So deceiving a player about an ability's text carries into everything the ability does, with no extra rule.
**Pretend on a creation (prototype, D155).** Creating a permanent is never hidden: a Pretend may only show a different permanent — a Mimic face, or "create 2 Fire Warriors. *Pretend:* create 2 Water Elementals". Every permanent is therefore in every belief, with values. An ability may carry its own Pretend, for deception beyond what the beliefs about its permanent already give.

**`ChooseOne[…]`** — a modal effect; the caster picks **privately**, and the choice reaches the true copy and the caster's copy. **Mimic** is Pretend on the cast: "5/5 Dragon. *Pretend:* 1/1 Goblin" — the opponent's copy of the trace reads Goblin, and the permanent and the card in Discard copy that belief (D140). Observation shows only location and instance, so the face holds until a contradiction (a hit too strong for a Goblin) reveals the card and with it the whole face, or until its own stated collapse (D131). The face must be plausible: a Flying Dragon entering the Sky as a Goblin contradicts at once.

**Worked card:**
```
Deceptive Boon   cost {mana 2}
  ChooseOne[ target creature gets +2 Attack , target creature gets +3 Life ]
  Pretend: target creature gets +3 Life
```
The honest cast and the +2-Attack bluff are **indistinguishable** to the opponent. Plausibility of the pretend is the **designer's** job (pick something a real card could do); the engine only enforces conservation.

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
