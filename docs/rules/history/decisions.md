# Decision Log

*Resolved design decisions, newest first. Each: the call, why, and what it touches. Open questions live in the individual docs, not here.*

Decisions, and parts of decisions, that later decisions replaced are in `decisions-archive.md`. Read it only when reopening a replaced idea.

**Status:** Active · **Date:** 2026-07-23

---

### D165 — Belief objects: a contradicted observation destroys a whole lie; damage taken is observed

`PLAN.md` Track B items 79 and 26, discussed 2026-11-05 to 11-06 (a mixed Track A/B session on how the engine represents false beliefs). Before: D164 — every copy carried out every trace on its own values, and a contradiction corrected only the values the instruction read, leaving the lie's cause and a Mimic face in place.

**Adopted (user):**
- **Six questions** organise the belief rules: how a false belief arises (Q-a: only Pretend, Mimic included), how it spreads (Q-b), when it is lifted (Q-c), how (Q-d), how a value becomes unknown (Q-e) and how it becomes known (Q-f).
- **A belief object** describes one lie for one observer. It is not a game object and can't be referenced. One Pretend creates one per fooled observer — a Mimic creates three (B, Neutral A, Neutral B), identical except for the observer. It is attached to every object that carries the lie (card, trace, permanent); beliefs follow the `source` chain, and once the trace ceases to exist the permanent links to the card (D97).
- **Believed values are stored,** not recomputed. A false value computed from a lie (the damage dealt by a creature with a false Power, the Life it leaves) is a **derived belief object** that rests on its sources.
- **A lie is only ever destroyed, never altered.** Destroying a belief object destroys everything that rests on it; a contradiction on a derived belief object destroys its roots. Afterwards each attachment is deduced again from the true source with what the observer can observe: true where observable, `?` otherwise — a decoy caught in darkness gets location `?`; a card in Discard follows its trace, or its permanent once the trace is gone.
- **History entries produced by a lie** rest on its belief object, displayed or not, and are deduced again with it, so a history query never reads a lie the observer has already seen through. A belief object ends when it is destroyed or every gap it left has closed, not when its traces fade.
- **Instructions change believed values.** An instruction that closes the gap (a heal to max in the truth and in the belief) destroys the derived belief object; one that only narrows it keeps it.
- **Two or more lies on one value:** destroying one replays that value's changes with the remaining lies (D's Life after two bluffing hits and a heal the cap cut short). Exact, never a search over possible causes; designed in `PLAN.md` Track B item 86, which may change the rules.
- **One Pretend is one belief object,** so what is revealed together is card design: a card that wants its Flying to surface without its face uses two Pretends.
- **When (Q-c): a contradicted observation.** Observed are the certain properties (D164) and **the damage a creature takes** — the Life it actually loses, so damage beyond lethal doesn't show — each only for objects the observer can observe; nothing about an object is observed if the object can't be observed. A contradiction destroys the belief objects behind the false values the contradicted instruction read, its checks included (D164: a move reads Flying and the Move surcharge, an Attack reads Ranged, an activation's timing reads Haste). A lie is lost only for the observer who observes the contradiction.
- **Using an ability a belief object hid** is a contradiction like any other: the trace shows the ability, and the belief object goes.
- **Rule steps never reveal** (AP refresh, Life cap, death check, fading, credit). They run on each observer's believed values, and what they compute rests on the same belief objects.
- **A trace's Duration is certain in the prototype** (`PLAN.md` Track D item 83): a pretended Duration is always caught at the first turn-end where the Durations differ.
- **The solutions considered differ only in the observation list,** which playtests adjust.

**Why (user):**
- Lies spread out of control under D164 because a partial correction left the lie in place and running: it kept producing false values (a curse fading in a copy that had already seen the creature's true Power).
- Destroying the whole belief object stops that; "only destroy" keeps the bookkeeping simple — a reveal never computes a new value.
- B sees its own creature hit harder than the face allows; damage beyond lethal shows nothing.
- An exact replay is cheap: we always know which lies remain, so it never searches, and more than a handful of belief objects on one value is not expected.

**Considered** (→ `playtest-variants.md` §What a contradiction reveals):
- **Destroy on read** (the base solution): Q-b never happens, but paying AP reads AP although "enough AP?" has the same answer both ways, and reads nobody sees would reveal; can't be judged without playing it.
- **"Changing":** current Life is also observed whenever it changes, so return damage on a Mimic's false Life reveals it.
- **Contradiction on the certain properties only** (D164's list), destroying the whole belief object.
- **D164's partial reveal** — possible now only as card design, one Pretend per property.
- **A value resting on a destroyed lie becomes true** even if another lie shaped it too; **merging the differences at a cap**. Both err toward the truth.
- **Representations:** offsets from the truth (break at every cap, can't hold a creature missing from one belief); five full copies of the state; a list of changes for every value, recomputed on every read.

**Replaces in D164** (→ `decisions-archive.md`): Q4's observing of the values read with their max values, and "nothing else is revealed" (a face surviving a contradiction, no cause); an unbelieved ability added without a contradiction; observing only on a contradiction, never in combat; and the history's "a contradiction never rewrites it". **Reopens:** what is observed of opposing cards in Hand and Discard (`PLAN.md` Track A item 82).

**Open:** the Aether as a view of the history (`PLAN.md` Track A item 85); the replay (Track B item 86).

→ `asymmetric-information.md` (§Deduced properties, §Contradiction, §Representing deception), `glossary.md`, `overview.md` §9, `interaction-stack.md` (§Resolution), `history/playtest-variants.md`, `PLAN.md` Track A items 82 and 85, Track B items 9, 26, 79 and 86, Track C item 10, Track D item 83. Engine: Track B items 79 and 86.

### D164 — The observation model: what can be observed, what is certain, everything else deduced; a contradiction observes what the instruction read *(replaced parts in `decisions-archive.md`)*

`PLAN.md` Track A item 72, discussed 2026-10-28 to 11-04. Rebuilds the belief rules around four questions. Before: observation was an ability on every terrain (D152), your own permanents were true in your copy (D155), every instruction named the permanents it affected (D153), the death check ran on the true state for every copy (D149), and a contradiction also revealed its cause (D151's large reveal).

**Adopted (user):**
- **Q1, which objects can be observed — one global default, overridable like any rule.** An observer always observes its own objects in Mind and Island and everything in the Aether, and every terrain; never the opposing cards in Hand, Library and Discard; opposing permanents on Ground and Sky; in the Root, only in hexes with one of its own permanents in the Root at distance ≤ 1 (D67). Effects override it for a terrain or a permanent; the later one counts. Darkness: "Creatures you control in this location are observed only by you"; an invisible scout: "This permanent is observed only by its controller". Setup is the instruction that shows the starting cards' backs. Replaces D152's observation abilities and D7's full visibility.
- **Q2, certain properties — true whenever the object is observed; no Pretend reaches them.** Own Hand: section, position, name, ID, timestamp, `source`, subtypes, cost, Speed. Own Library and Discard: section only. Own permanents: location, instance, name, subtypes, controller. Others' permanents: location, instance, controller (controller only together with location, Track D item 74). Terrain: the capacity of each Slice. Aether: section, position, controller, `source` as instance; a trace's name is certain for its activator, its content isn't. An observed location can't be pretended (Track D item 75; illusions use a decoy with Mimic). Terrain Mimic is Track D item 76.
- **Q3, everything else is deduced, your own permanents included.** A false belief comes only from a tampered trace: a trace's content, when it is created or changed, is the only pretendable observation (Mimic included). Each copy carries out every trace as it believes it, on its own values; an instruction has no separate output. Rule steps without a trace (death check, AP refresh, pool reset) also run in each copy on its own values; the game ends on the true state. A value outside the certain list becomes true only when a basic instruction that observes true values runs (investigate; a contradiction, below). Replaces D155's "a change to your own permanent is true in your copy" and D149's damage default: a pretended enemy Attack shows less damage on your own creature.
- **Pretend** remembers its caster on the object and reaches every copy but the true one and its caster's, so four casters give four different wrong versions. For a permanent's own Mimic the caster is its owner, not its current controller.
- **Targets are trace content** (amends D153). A target is pretendable like the rest of the content. A non-targeted effect names nothing: each copy applies it to the permanents it believes in. "A death names the creature that fell" and "a permanent's controller is always known" go: a death is deduced in each copy. Taking a Neutral permanent in an unobserved Root stays hidden only with a Pretend. AP spent stays public, since a move's trace names its creature as content.
- **Gaining a permanent or stealing a card** observes only what is certain for your own objects; the content beliefs carry over (replaces D162's "gaining a permanent makes its values true").
- **A card belief always has one section, possibly false** (amends D154's "knows its section"). A secret draw is "Draw a card. *Pretend:* no effect".
- **Q4, contradiction.** A certain observation overwrites only what it observes. It contradicts what a copy carried out when a creature lives that the copy's death check removed, a creature falls that the copy expected to live, a creature spends more AP than the copy believes it has, a payment takes the copy's count of a pool below 0 (D157), or a permanent takes an action the copy believes illegal; what a contradiction does is D165. A contradiction never fills in an unknown (D151): a `?` stays `?`.
- **A check reads static abilities like any other property,** so an action the copy believes illegal contradicts what its checks read. For now each check names them: a move (Move, Ascend, Descend) reads Flying and the terrain's Move surcharge; an Attack reads Ranged; the timing of an activation reads Haste. Ability types may take this over (Track C item 10).
- **A Pretend that shows a different card type without Mimic is outside the prototype** (Track D item 81). D131's fallback "collapse to the full truth" and D18's "collapse → full truth by default" go; D18's "no deduction engine" stays.
- **A ghost can't be targeted** — a permanent a copy believes in that isn't there fails the visibility check at binding. Capacity already fits (D132: a truly full Slice fizzles the move, the cost is spent).
- **A belief never changes the true state;** the only indirect effect is a neutral seat's copy steering its permanents' Behavior. Changing a belief by hand is outside the prototype (Track D item 80): it would let a player set an enemy's Life to 0, see the creature standing and observe its true Life.
- **The view tells certain values from deduced ones, and shows where each deduced value came from** (`PLAN.md` Track A item 60).

**Why (user's reasoning):**
- **"All properties apart from those listed as certain observations are deduced, except when a basic instruction that observes true values is executed."** One model for every object; a hidden curse on your own creature works.
- **Uncertainty must not only pile up.** Some of it is lifted when an object takes part in certain interactions. The smallest set of such states is the states where a contradiction arises; more stays possible.
- **A revealed name reveals everything about a Mimic**, not only what the interaction read.
- **One global observation default** instead of an ability on every terrain.
- **A legality check is one more read** (Claude, agreed): one rule, "observe what the checks read", instead of a table of ability groups per action.
- **A revealed identity means a revealed Mimic:** all things hidden by the Mimic are revealed.

**Considered and rejected** (→ `playtest-variants.md` §What a contradiction reveals, §Observation):
- **The terrain observation abilities** (D152).
- **Fit without observing anything** (item 72's first answer): a contradicted value with no observation needs a guess. The guesses tried: `?` for every value the instruction read (`?` spreads into every later instruction that reads it); the smallest change that makes the belief fit, with tie-breaks (enemy values before own; a fixed order of property and controller; values an opponent's trace touched, most recent first — but a Pretend can lie about the property and the target); a forced pick by the player at every contradiction, with an automatic rule for the neutral seats.
- **Reasoning over every legal trace** to find the cause: a deduction engine (D18), and it needs the opponent's hidden cards.
- **Life certain for your own permanents:** kills hidden curses.
- **D155's "your own permanents are true in your copy"**.
- **Static abilities attached to the trace** (user): an Attack trace carries Ranged, a move carries Flying. Kept as a variant: it needs the attached abilities true in every copy, so it shows them at every use, not only on a contradiction.
- **Item 72's other rejected variants:** a section as a set of possibilities ("4–5 cards", advertises a secret draw); face-down activation as the default; rule steps applied from the true state to every copy; on a contradiction, fitting with the death known.

**Deferred:** changing a belief by hand (Track D item 80); a Pretend on a card type (Track D item 81); the smallest change for a location instead of `?` ("keep where its last move or descend put it", Track D item 77); a card that changes what an observation shows (Track D item 78); ability types instead of the per-check lists (Track C item 10).

**Amends, in place:** D153 (targets are content), D154 (section possibly false), D157 (the count is observed, no cause), D159 (a differing check is an ordinary contradiction; a Mimic face collapses only when the check reads an identity property). **Replaced parts moved to `decisions-archive.md`:** D7, D18, D131, D146, D149, D150, D151, D152, D153, D155, D157, D162, D163.

→ `asymmetric-information.md` (rewritten), `glossary.md`, `interaction-stack.md`, `object-properties.md`, `effect-form.md`, `overview.md` §9, `history/playtest-variants.md`, `PLAN.md` Track A items 56 and 60, Track B items 9, 26, 27, 70 and 79, Track C item 10, Track D items 74, 77, 78, 80 and 81. Engine: Track B item 79.

### D163 — One history track per copy; any history may be read *(replaced parts in `decisions-archive.md`)*

`PLAN.md` Track A item 36. D98 left open whose view a history query reads, which facts the track records, and whether a longer memory is ever a card property; its window was one round, as a hard rule.

**Adopted (user):**
- **Every copy keeps its own history track** — the true one, both Champions' and both neutral seats' (D162). An instruction appends its output in each copy as that copy applied it. A hidden creature's move is in the opponent's track as the opponent saw it: the creature named, its destination `?`.
- **A history query is a value read in a copy,** like "damage equal to its Power": each copy evaluates it from its own track, and the comparison at resolution reveals a false belief when an outcome shows it (D151). No rule of its own.
- **Any history may be read — the whole game is the goal.** Replaces D98's one-round window. Card text may ask about anything that happened since the game began.
- **Prototype: the current round.** Reading further back is outside the prototype scope (D104).
- **What counts as an event:** everything recorded is an instruction output. A pass of priority is not an event; a fully prevented attack is ("if a creature attacked this turn" holds). Which outputs each basic instruction has is settled with `PLAN.md` Track A item 56.
- **How the track is stored** (snapshots, diffs, instruction sequences) is an implementation question.

**Why (user's reasoning):**
- **Each copy appears consistent with its player's belief.** Reading history per copy is what the belief model already does with every other value; a query on the true state would leak hidden information.
- **From a design perspective every history query should be possible.** A window is a storage limit, not a rule a player should feel; the prototype accepts it only as scope.

**Considered and rejected:**
- **D98's one-round window as a hard rule**, with longer memory written onto an object as a property ("whenever a creature dies, put a counter on this") (Claude): limits design for a storage reason.

**Checked, already decided:** a conditional instruction still has its targets bound at cast (D83) and its condition checked just before it runs (D95, D135), so a false belief only changes the predicted outcome, never whether a target is needed. Example: "If that creature's Power is 5 or more, target Champion discards a card" needs its target at cast whatever anyone believes.

→ `glossary.md` (History track), `object-properties.md` (§Short-term history), `asymmetric-information.md` (§When a belief breaks), `PLAN.md` Track A items 56 and 72, Track B item 9, Track D item 71. Engine: not built.

### D162 — Each neutral seat holds its own belief copy *(replaced parts in `decisions-archive.md`)*

`PLAN.md` Track A item 61. The three-copy model (D149) has one belief per Champion; a Neutral permanent has no controller, so which copy its Behavior reads, what it observes, and whose observation decides a Neutral trace's visibility check (D85) were unstated. Initial decision, to be confirmed by playtesting.

**Adopted (user):**
- **Five copies.** Every property has its true value and one belief per **observer**: Champion A, Champion B, **Neutral A** and **Neutral B**. A neutral seat has the same belief structure as a Champion, beliefs about cards included, and its copy is updated by the same rules.
- **An observer's permanents:** for a Champion, the permanents it controls; for a neutral seat, the Neutral permanents assigned to it. Every rule that said "controls" or "the opponent" now says this. Observation, "true in your own copy", naming affected permanents (D153), traces showing targets, sight lost, contradiction and the mana-pool counts all run per observer.
- **A Neutral permanent reads its seat's copy:** its Behavior's targets and values, and the visibility check at resolution of a trace from a Neutral source.
- **A Pretend reaches everyone else:** every copy except the true one and the caster's, both neutral seats included. No audience in the prototype.
- **Neither players nor the GUI see a neutral seat's copy.**
- **Belief does not move with a permanent.** A control change or a seat change transfers no knowledge; what the old holder knew stays in its copy.

**Why (user's reasoning):**
- **Neutral permanents were designed to be independent** (D82): not extensions of a Champion's army, but creatures following their own will. A seat with its own knowledge is that; reading a Champion's belief makes them half-allies.
- **Watching a monster is a source of information.** A Behavior is deterministic, so a player who watches a monster act deduces what its seat knows and the player doesn't. A pro, not a cost.
- **Two copies are affordable**, unlike one per permanent.
- **The rules cost is small:** one definition (observer and its permanents) rewords every rule, and the Pretend audience is the only real choice.

**Considered and rejected:**
- **A copy per Neutral permanent** (user): the cleanest independence, but an Element that creates many Neutral permanents would make the number of copies infeasible.
- **A neutral seat reads its Champion's belief, read-only or also contributing to it** (user's option 3) and **Neutral reads the true values** (Claude's first suggestion): kept as variants, `history/playtest-variants.md` §Neutral beliefs.
- **A Pretend names its audience** (Claude, as the alternative to the default): card-level complexity no card needs yet.

**Accepted cost:** no player can fully predict a monster, since neither sees the copy it reads.

→ `asymmetric-information.md` (§Core model, §Observation, §Beliefs, §Pretend), `neutral-permanents.md` (§Target selection), `interaction-stack.md` (visibility check), `effect-form.md`, `glossary.md` (View, Belief, Pretend, Observer), `history/playtest-variants.md`, `architecture/implementation-plan.md` (G39), `PLAN.md` Track B item 70. Engine: not built.

### D161 — Discard and mill keep content beliefs; Discard is not public by default

`PLAN.md` Track A item 58. In the belief model (D149) a cast card in Discard is believed as its trace, but no rule said what a milled or discarded card shows.

**Adopted (user):**
- **Discard and mill are moves** (D148): they change a card's section and position, and keep every content belief. A milled card stays `?` for everyone, its owner included. A discarded card stays known to its owner, and the opponent keeps whatever it believed before (usually `?`).
- **There is no Discard rule.** What a player knows about a card in Discard follows from how it got there. A card that wants a card seen says *reveal*.
- **Replaces D140's** "entering Discard (discard, mill, a cast card) makes the content true for everyone, unless an effect states otherwise."

**Why (user's reasoning):**
- **Most of Discard is public anyway, with no special rule.** Casting is a public action: it creates a trace that matches the true card, or shows its Pretend or Mimic mask. The card in Discard matches its trace, so it is believed as the trace showed it, true or false (D149, D155). Casting is by far the most common way a card reaches Discard, so most cards there are known.
- **Discard is an ability that sees play, and a hidden default opens design space.** A very cheap "discard target card" that doesn't show the card; it also raises the value of cards that reveal a Hand before it. A normal "reveal and discard target card". A more expensive "reveal a Hand, then choose a card". This is the pessimistic default (D14), and it fits well here.
- **The same goes for mill.** Mill is interesting when it can be abused, and abuse usually needs knowing what was milled. A plain mill is harder to abuse; "mill and reveal" is a separate, stronger effect. Reveal on its own gains value too.

**D140's reasons for a public Discard, rechecked:**
- **"Cast already showed most of what enters it":** the copy-once rule covers this without a Discard rule.
- **"A hidden Discard would make the view forget what everyone saw":** beliefs are copied, not reset, so nothing seen is forgotten.
- **"Cards reading a Discard need it":** such a card reveals first, or reads `?`. A target with an unknown content isn't new: `discard` already targets a card in the opponent's Hand.

**Considered and rejected:**
- **Entering Discard without a cast reveals the content to everyone** (item 58's candidate, D140's rule): an extra rule, and it removes the design space above.
- **Entering Discard makes the content true for its owner** (Claude): an extra rule for one case, mill. It would let the owner pick knowingly from their own Discard, which is exactly the self-mill abuse the user wants to cost a reveal.

**Accepted cost:** the owner's own Discard can show `?` cards (milled ones).

→ `asymmetric-information.md` (§Beliefs, defaults), `glossary.md` (Discard). Engine: already built (`Mind.Move` keeps beliefs).

### D160 — A permanent enters with 2 Activation Points, so a fresh creature can defend

`PLAN.md` Track A item 57. Under D89 a permanent entered with 0 AP: a creature cast in its controller's turn couldn't Defend (`1~AP`) in the opponent's next turn, so any attack on it was undefended and the attacker took no retaliation.

**Adopted (user):**
- **Every permanent enters the Island with 2 Activation Points** (placeholder), capped at its max, however it got there. The rest of D89 stands: it refreshes at its controller's next Beginning phase, and a control change leaves AP untouched.
- **Entry AP stays below the Attack cost** (`3~AP`), so summoning sickness means "can't attack yet". 2 covers every Defend cost, including a connected Champion's or Companion's `2~AP`.
- **No other restriction.** A fresh permanent may also move or use any ability its entry AP pays for. An ability costing 2 AP or less is therefore stronger, and should be rarer; powerful cheap abilities can carry `!` or `~`.
- **Haste X** sets the entry AP to X instead; X below 2 is a drawback.
- The value is set together with the default action costs and the Champion's starting AP (`PLAN.md` Track A item 67).

**Why:** summoning sickness should stop a creature from acting on its own initiative, not from defending. A creature played by surprise in the opponent's turn (a Quick creature) must be able to defend right away without a second keyword. Without a default, nearly every creature would print Haste 1 or Haste 2.

**Accepted cost:** this breaks the pessimistic default (D14). Cheap abilities work the turn a permanent enters, and Flicker refills a spent permanent to 2 AP. D89 had closed this for created permanents (D57's Fire Warrior); here it reopens on purpose, for costs up to 2.

**Considered and rejected:**
- **Full AP, locked as if it had paid a `~` cost** (Claude): it can defend in the opponent's next turn, but a creature flashed in during the opponent's turn can't defend in that turn.
- **Full AP, only Quick abilities until the controller's next Beginning:** satisfies both cases, but needs a "sick" status to track. Kept as a fallback in `playtest-variants.md`.
- **First refresh at the next Beginning of any player:** fails the flash case.
- **Refresh at the End of the controller's turn:** fails the flash case, and every creature would have full AP in the opponent's turn (breaks D116).
- **Defend costs no AP, once per turn:** removes D116's choice between spending AP and keeping it to defend.
- **1 entry AP:** covers a creature's Defend, but not a connected Champion's or Companion's.

→ `economy.md` (§Summoning sickness), `setup.md`, `neutral-permanents.md`, `object-properties.md`. Engine: Track B item 68.

### D159 — Resolution checks in the belief model: visibility is a belief matching the truth

`PLAN.md` Track A item 24. Restates D85's visibility condition, written for the live view, in D148–D155 terms.

**Adopted (user):**
- **Visibility, for a bound permanent only:** the trace's Champion's belief of its location equals its true location. A permanent the Champion doesn't observe fails — its belief is `?` (sight lost) or false (a decoy). A Reveal (D102) sets the belief true without sight, so a revealed creature can be hit until it moves (a move keeps the belief, which is then false). A terrain or a Slice as a target is always known and isn't checked, so Darkness cast in response doesn't make a Move fizzle.
- **The checks read the true state; each copy runs them with its own beliefs (D151).** A Mimic face never decides identity, protection or verb applicability; it only shapes the opponent's prediction, and a differing outcome is an ordinary contradiction. Hidden protection: the Bolt fizzles, the opponent expected damage, the protection is revealed. Bluffed protection: the Bolt hits, the opponent expected a fizzle — the same.
- **Test cases for verb applicability:** "Destroy target Structure" on a creature with "*Pretend:* a Wall" — legal to target by the caster's belief, fizzles at resolution, collapses the face. "Unbond target terrain" after it was unbonded in response — nothing left to unbond.
- **No further implicit condition found:** identity (the object), visibility (the caster's knowledge of it), protection (what it refuses), verb applicability (what the verb can act on); anything else is printed.

**Why:** a belief that matches the truth is the model's meaning of "knows where it is", and it gives Reveal a use as setup for an attack.

**Considered and rejected:**
- **The Champion observes it** (D152, Claude): equivalent except after a Reveal, which it makes useless as setup.
- **The belief isn't `?`:** a false belief (a decoy) would grant reach — a bluff would make the opponent better at hitting you.

**Open:** whose belief decides for a trace from a Neutral source (`PLAN.md` Track A item 61); resolution-time choices (Track D item 64).

→ `interaction-stack.md` (§Resolution). Engine: Track B item 25 (the checks per copy), item 26 (S9).

### D158 — An exhausting cost pays `x` at C1 and drains the rest at C5

`PLAN.md` Track A item 63, raised by the engine's D135 sync (Track B).

**Adopted (user):** `x!AP` splits into its two parts. **`x` is paid at C1** with the fixed costs; **the remaining AP is consumed at C5**, when the casting arrives — after every surcharge paid at C4. An exhausting Move onto a Sucking Mire ("Move abilities that target this terrain cost 1 more AP") needs `x`+1 AP and still ends at 0 AP. An abort before C5 drains nothing.

**Why:** with the whole drain at C1, nothing is left for a C4 surcharge, so an exhausting ability could never target a surcharged object — a hidden "can't" (against D133) that the Mire's "costs 1 more" never said, and against D134 (a cost modifier adds on top of whichever value applies). Draining at C5 matches D135: casting is one atomic event, and what it causes arrives at C5. Only surcharges become payable; AP beyond them is still drained.

**Considered and rejected:**
- **The literal rule** (drain at C1, as the engine first built it): exhausting abilities can never pay a surcharge.

→ `economy.md` (§AP cost flavors), `interaction-stack.md` (§Casting, C5), `glossary.md`. Engine: Track B item 28.

### D157 — Overspending past the opponent's belief is a contradiction; an untested mana bluff ends at the reset; no "possible payers" marker *(replaced parts in `decisions-archive.md`)*

`PLAN.md` Track A item 52 (mana pool), continued from D156.

**Adopted (user):**
- **The Beginning-phase reset sets the pool to 0 in every copy**, so a mana bluff never tested by an outcome is not revealed afterwards; it only has to last until the owner's next Beginning phase.
- **No GUI marker for "resources a hidden trace's possible payers could have spent".** Every instruction names its source (D153) and a cost is paid by the Payment walk from that source alone (`ancestry.md`), so each payment lands on exactly one believed pool. A Pretend showing a different source lowers the wrong pool's count and is caught by the overspend contradiction.

**Why:** the AP rule and D18's conservation law (claimed costs ≤ what the network produced) already make overspending a contradiction; a special rule for mana needs a reason no playtest has given yet. Without the check, a Pretend showing a higher cost would never be caught.

**Considered and rejected:**
- **No check on overspending** (user's first idea): a Pretend showing a higher cost is never caught.

→ `asymmetric-information.md` (§Beliefs, §Costs, §The consistency problem), `ancestry.md` (§Payment). Engine: Track B item 27.

### D156 — The mana pool is a property; the opponent believes it as one count

`PLAN.md` Track A item 52 (mana pool), restating D77's pool and D137's visibility in the belief model (D149).

**Adopted (user):**
- **The pool is a property** of the Champion and of each Companion: a count per mana kind (colorless, or a disjunction over Elements, D125), e.g. "3 Water, 1 Water/Air". A unit of mana is not an object — nothing targets, names or tracks one. The drawn-this-cycle flag is a terrain property (D77).
- **The opponent's belief of a pool is one count**, not a count per kind; the controller's own copy holds the counts per kind. The opponent's copy changes by what the opponent believes — pretended costs and pretended gains; true costs, true gains and which mana paid never reach it.
- **Default updates, the same function in every copy (D149):** a credit raises the count by the terrain's production and flips its drawn flag (bonds and production are true, so the opponent knows what was drawn — a hidden Knotting creature can be read off a terrain that wasn't drawn); add mana raises it by the believed amount; a cost lowers it by the believed pips (D155: values in a trace come from beliefs); the owner's Beginning phase sets it to 0.
- **Prototype: color is never checked.** A Pretend can claim an Element the player has no terrain for; spotting it is the human's deduction (D18). A belief with color is Track D item 62.

**Why:** bonds are true and production is printed, so the amount a player drew is known anyway; hiding it would only make players compute it. A count-only belief means a Pretend cost never forces the player to invent which mana paid it. As a property, the pool needs no visibility mechanism of its own.

**Considered and rejected:**
- **An unobservable pool:** the amount follows from true bonds, so hiding it is only a burden.
- **A conservation law for colored pips** ("a claimed colored pip needs a bonded terrain of that Element", Claude): any one such terrain anywhere silences it; parked in Track D item 62.

→ `asymmetric-information.md` (§Beliefs), `resources-terrain.md` (§Mana), `glossary.md` (Live mana ledger), `overview.md` §4. Engine: Track B item 27.

### D155 — Your own permanents are true in your copy; traces show targets by observation; no hidden creation in the prototype *(replaced parts in `decisions-archive.md`)*

`PLAN.md` Track A item 59 (observation of cards and traces). Generalizes D149's damage default; replaces D152's move-trace rule.

**Adopted (user):**
- **A trace shows each target only to Champions who observe it.** A permanent target is always named (D153), its location shown only to those who observe it. A location target — one Slice of one terrain — is shown only to those who observe that Slice; a Champion observes a Slice where it would observe a permanent it doesn't control. A move inside darkness stays hidden; a move out into the light shows its destination. A visible effect still hints where a hidden source is — the price of acting in the open.
- **An ability trace takes each Champion's belief of its source:** activating creates the trace from the permanent's ability, so each copy of the trace is copied once from that Champion's belief of the ability (D140) — automatically, with nothing written on the ability. A Mimic believed to be a Goblin that activates a Dragon ability contradicts that belief. A Pretend written on an ability is only for deceiving beyond that.
- **Values in a trace come from beliefs:** a value computed from a property ("damage equal to its Power") takes each Champion's belief of that property (D140), fixed at activation or computed at resolution alike. So B's copy reads 2 where the truth reads 3, and the comparison reveals the difference only when an outcome shows it.
- **Discard follows the trace:** each Champion's belief of a cast card in Discard is its belief of the trace (D140).
- **Prototype: creating a permanent is never hidden.** A Pretend on a creation may only show a different permanent — a Mimic face, or "create 2 Fire Warriors. *Pretend:* create 2 Water Elementals". So every permanent is in every belief, with values. Truly unseen creatures are Track D item 52.

**Why:** a trace's targets were the remaining way a hidden creature's location leaked. Hidden abilities for hidden permanents (Claude) didn't cover values the ability reads. Banning hidden creation removes the only case of a permanent named without any believed values.

**Considered and rejected:**
- **A hidden creature's ability trace keeps its content hidden** (D140, Claude): leaks through its targets anyway, and hides an ability the opponent can infer from the named creature.

→ `asymmetric-information.md` (§Observation, §Beliefs, §Representing deception), `effect-form.md` (§Prototype scope), `glossary.md` (Pretend). Engine: Track B item 25.

### D154 — A card belief always knows its section; a card you lose track of becomes anonymous

`PLAN.md` Track A item 59 (counts, a shuffled card). Answers item 55 for cards.

**Adopted (user):**
- **A belief is never disjunctive** ("either this card or that one"). On the Island D153 guarantees it; cards need their own rule, because showing which card a hidden draw took would reveal it.
- **A card belief always knows the card's section,** so the counts of Library, Hand and Discard follow from the objects in each belief. Its position may be `?`.
- **When an instruction moves a card a Champion can't identify** — a draw from a Library whose order it doesn't know — every card in that section whose position the Champion doesn't know becomes **anonymous** for it: every belief about it except its section is forgotten, and the section stays true.
- **Example:** A shuffles Bolt #7 from Hand into Library. For both, its section becomes Library and its position `?`. A draws: A sees the drawn card, so A's beliefs stay certain. B sees a card move from Library to Hand but not which, so every card B knew in that Library with an unknown position, the Bolt included, becomes anonymous. A card whose position B knows (put on top) stays identified.

**Why:** beliefs stay about one object each, as on the Island, and the counts can't drift from the cards. The shuffle itself doesn't forget: the owner still knows the Bolt is in their Library; only an unseen draw makes its section uncertain.

**Considered and rejected:**
- **Keep** (the unidentified Champion's copy stays as it was): a belief turns false without any Pretend (the Bolt still believed in the Library).
- **Counts as a separately observed property of a section** (Claude): needs a belief spread over several cards, which can drift out of sync with the counts.
- **A shuffle forgets at once.**

**Open:** how a player sees why a belief was forgotten and what it was before (`PLAN.md` Track A item 60).

→ `asymmetric-information.md` (§Beliefs), `glossary.md` (Belief). Engine: Track B item 25.

### D153 — Every instruction identifies the permanents it affects, never their location *(replaced parts in `decisions-archive.md`)*

`PLAN.md` Track A item 59 (objects created or dying unseen, a hidden target).

**Adopted (user):**
- **Pretend may name a different object:** "Target creature gets +1 Power. Pretend: another target creature gets +1 Power" — the opponent's copy applies it to the pretended one, a second target chosen at cast. Still one object per copy.
- **Observing a permanent identifies its instance,** not only its name; two identical creatures are never confused by default.
- **The objects in a belief need not match the true state:** a belief may hold an object that no longer exists or never existed (a Pretend decoy), and may lack one it has never observed nor seen affected.
- **Seeing a permanent missing makes its location `?`:** when a Champion observes a location where it believes a permanent is, and it isn't there, its location belief becomes `?` — the same whether it moved, died or never existed. Whether the view shows known permanents with an unknown location is a GUI detail, not decided.
- **Observation corrects silently:** it never triggers a contradiction or the large reveal (D151). Example: X moves honestly from h4 to h5 inside darkness; B looks at h4 and finds nothing — a large reveal would make the move's destination true for B.

**Why:** the Island is the core of the game and must stay simple; what makes a hidden creature hidden is its location, not its identity. With every affected permanent named, no Island belief can become disjunctive, so there are no candidates to reason over. A card that wants a truly secret target uses Pretend (pessimistic default).

**Considered and rejected:**
- **A hidden target stays hidden; the changed property becomes `?` on every creature it could have hit** (briefly adopted): sound, but one hidden pump blanks every unseen creature's Power — too complex for the Island.
- **Every object exists in every copy, with an unknown location if unseen** (Claude): leaks how many objects exist.
- **Identification by name only** (seeing a Goblin tells you a Goblin, not which one): needs reasoning over several candidates. Accepted cost: a shell game with identical creatures needs Pretend.
- **Seeing a permanent missing removes it from the belief:** asserts what the player can't know, and answering differently for a real creature and a decoy would tell them which it was.

→ `asymmetric-information.md` (§Beliefs), `glossary.md` (Belief). Engine: Track B item 25.

### D152 — Observation lives on the terrain; sight lost forgets location; a move keeps the opponent's belief *(replaced parts in `decisions-archive.md`)*

`PLAN.md` Track A item 59 (darkness walkthrough). Before: D139/D140 listed what loses track (shuffle, concealment); D67 stated Root proximity as a rule of its own.

**Adopted (user):**
- **Sight lost forgets:** a player's location belief becomes `?` when they observed the permanent before the instruction and don't after — computed from the true state, whatever the belief holds.
- **A move keeps the opponent's belief by default;** observation does the rest. A move's trace shows its destination only to players who see the creature (D140); it names the creature to everyone (D153).
- **A false location needs no extra object:** Pretend can move the opponent's belief of an unseen creature (out of a dark hex, say); observation only reveals permanents that are really there, so nothing contradicts it until a contradiction (D151).

**Why:** every observation rule sits on an object, as abilities do. Forgetting on every unobserved state wiped out any false location by construction; forgetting only when sight is lost keeps it. A move writing its destination into every copy leaked the true location of a creature moving inside darkness.

**Considered and rejected:**
- **Unobserved means keep the last belief:** right for a one-hex darkness, stale and false in a larger one. Accepted cost of the adopted rule: a creature in a one-hex darkness becomes `?` though B could know where it is.
- **A move's target always hidden** (user, briefly): would hide where a visible creature is going while B can still respond.

→ `asymmetric-information.md` (§Observation, §The Root Slice), `overview.md` §9, `glossary.md` (Observation ability). Engine: Track B item 25.

### D151 — Contradiction by comparison, with the large reveal *(replaced parts in `decisions-archive.md`)*

`PLAN.md` Track A item 59 (Runs 2 and 3, the curse). Amends D146 for the three-copy model (D149).

**Adopted (user):**
- **Comparison:** each copy runs the instruction; when an outcome the observer sees differs from their own copy's prediction, there is a contradiction (D146).
- **A consistent bluff may stay hidden forever** (curse dealt lethal damage both sides expected).
- **A combat is compared once:** its damage in both directions is one instruction (D13). First Strike would split it into two instructions, each compared on its own.
- **A contradiction never fills in an unknown** (Claude, adopted on trust; check in playtest, Track B item 26): the large reveal corrects false beliefs only; a belief that is `?` stays `?`. Example: a ranged creature in darkness with a Power bluff attacks; the contradiction corrects its Power, but its location, read by the range check, stays `?` — the darkness hid it honestly. A decoy's location is false, not `?`, so it is corrected when the creature acts from its real place.

→ `asymmetric-information.md` §When a belief breaks, `overview.md` §9, `glossary.md` (Collapse); variants in `playtest-variants.md`. Engine: Track B item 25.

### D150 — Pretend is the one deception mechanism; players believe what they see *(replaced parts in `decisions-archive.md`)*

`PLAN.md` Track A item 59.

**Adopted (user):**
- **Pretend:** card text states the true effect and, after *Pretend*, what the opponent sees; the opponent believes it. "Mirage Might — Target creature gets +0 Power. *Pretend:* target creature gets +1 Power." The true and controller's copies apply the true effect, the opponent's copy the pretend.
- **Secret and concealed are forms of Pretend:** secret is "*Pretend:* no effect" (the opponent doesn't see the target); concealed is "*Pretend:* does nothing to target creature" (the target is seen, nothing seems to happen). So both are in the prototype.
- **Players believe what they see:** an effect never turns a belief into `?`; `?` stays for what nobody has seen (Library, the opponent's Hand) or what a Champion lost track of (D152, D154).
- **Mimic is Pretend on the cast:** "5/5 Dragon. *Pretend:* 1/1 Goblin" — the opponent's copy of the trace reads Goblin, and the permanent and the card in Discard copy that belief (D140). Observation shows location and instance only, so the face holds. Choosing a plausible face is the designer's job (a Flying Dragon entering the Sky as a Goblin contradicts at once). An observable mana pool would kill it (item 52).
- Card keywords may name the forms differently; that is content (item 39's alias layer), one mechanism in the rules.

**Why:** a concealed effect that set "everything it may change" to `?` leaked which properties a hidden spell touches; setting every property of the target to `?` was harsh and gave no duration. Pretend can also play with durations (the true effect permanent, the pretend until end of turn).

**Considered and rejected:** **concealed as `?`** (above).

→ `asymmetric-information.md` §Representing deception, `overview.md` §9, `glossary.md` (Pretend, Claim, Face). Engine: Track B item 25.

### D149 — Three copies of every property; the basic instruction sets each copy *(replaced parts in `decisions-archive.md`)*

`PLAN.md` Track A item 59. Restates D139–D140's views. Before: a view state per observer and property (true, hidden, false).

**Adopted (user):**
- **Every property exists three times:** the true value, A's belief, B's belief; a belief may also be `?`. True and false are derived (belief equals the true value or not), not stored. Turn, phase and active player have a true value only. (Beliefs for Neutral permanents: open.)
- **The basic instruction is apply(object, f_true, f_A, f_B)**, one function per copy — keep and `?` included. Rule text on cards maps to it ("gains +1 Power" is (+1, +1, +1)). In the prototype the object is resolved on the true state and used in every copy; a selection evaluated per copy ("the creature with the highest Power" as each player believes) is outside the prototype scope.
- **Instructions set the beliefs themselves**, by default: a **cast** creates its trace true for everyone; a **draw** reveals the card to its owner; **creating an object from a predecessor** (cast, resolve, Bounce, Remand, Flicker, put, copy) copies each belief once (D140), so a cast card in Discard is as known as its trace.
- **Durations run per copy:** a trace's Duration may differ between copies (Mirage 3: permanent in truth, Duration 1 in the opponent's copy).

**Why:** a POMDP-style factored belief, sound where nobody deceives and incomplete by design (no deduction, D18). Per-copy functions replace a mask of apply/keep/forget and make deceiving a player about an ability's text carry into its effect.

**Considered and rejected:**
- **Global observation rules** "a controller observes all properties of their permanents" (wipes out pretends aimed at a creature's controller) and "traces are observable" (the cast's own update does it). Replaced by the damage default above and by D152.
- **A Discard rule revealing every card that enters it:** leaks a secret spell's card; the one-time copy keeps the card as secret as its trace. Mill and discard: item 58, decided by D161.
- **A mask (apply, keep, forget per copy) with one f** (Claude): a special case of per-copy functions.
- **An observable "appearance" factor in the true state next to sound beliefs** (Claude): four copies instead of three, same behaviour.

→ `asymmetric-information.md` (§The core model, §Beliefs), `effect-form.md` §Instructions, `object-properties.md` §5, `glossary.md` (View, Belief, Discard, Mind). Engine: Track B item 25.

### D148 — The state: objects, their properties, the game record; zone is not state

`PLAN.md` Track A item 59.

**Adopted (user):**
- **The state** is the game objects with their properties (relations included, as properties pointing at objects; the drawn-this-cycle flag is a terrain property; a Champion's Life is its Health) plus the game record (turn, phase, active player, priority).
- **Zone is not state:** it follows from the object's kind. "No object ever changes zone" becomes "a transition between kinds creates a new object".
- **A card's location is (whose Mind, section, position),** so a card can **move** between Minds and keeps its identity — reverses D141's "a card from another Champion's Library" as a put.
- **History, the next ID and the random generator are implementation details,** not state the rules change. The mana pool: item 52.

**Why:** "Mind is one zone with six sections" vs "a card changes zone" was a naming question; a card moving to another Mind feels like the same card.

**Considered and rejected:** **each Mind a zone of its own, crossing it a put** (Claude).

→ `overview.md` §1, `object-properties.md` §1–2, `effect-form.md` §Instructions, `glossary.md` (Zone). Engine: Track B item 25.

### D147 — Paying Life is damage to the payer

Closes part (d) of `PLAN.md` Track A item 51. Before: D137 kept "pay N life" as not damage, with damage the only way an instruction lowers Life.

**Adopted (user), to test:** paying N Life as a cost deals N damage to the payer, so Life goes down through damage or −X only (D143). "Whenever this is dealt damage" triggers fire on a cost payment; a card with such a trigger is priced with that in mind. Scenario: Track B item 24.

**Why:** one instruction instead of two, and the trigger interaction is printed on the card, not hidden in a rules distinction. The two old catches are gone: damage no longer reveals anything by itself (D146), and the prototype has no prevention instruction (D141) — if prevention is added, a Life cost must not be preventable.

**Considered and rejected:**
- **Paying Life is −X:** lowers the max for good — harsher than a cost should be.
- **A separate "lose Life"** (MTG's split, current only, not damage): avoids damage triggers on costs, at the price of the distinction D143 set out to avoid.

→ `economy.md`, `glossary.md` (Life).

### D146 — A belief breaks on contradiction; reveal points and private numbers are dropped *(replaced parts in `decisions-archive.md`)*

Closes parts (b) and (c) of `PLAN.md` Track A item 51. Supersedes D137. Before: D137 made a permanent's numbers and the mana pools private and guaranteed a list of reveal points; the user suspected the guarantees couldn't be had.

**Adopted (user):**
- **Contradiction:** an instruction runs on the true state and, for each observer, on that observer's view; the parts of the two results the observer can see are compared. If they match, the view keeps its values — a lie survives as long as it is consistent. If they differ, **every false or hidden value the instruction read becomes true** for that observer. A hidden value predicts nothing, so a seen outcome that reads it shows it. The observer always sees the true outcome. A keyword may state its own collapse instead (D131).
- **Reveal points are dropped** — they were examples of the contradiction rule (damage dealt reads Power; a creature acting with 0 AP in the view reads AP).
- The mana pool's visibility and the GUI marker on resources a hidden trace could have spent move to item 52.

**Why (user's example):** a 3/3/6 secretly made 1/3/6 deals 1 damage; the owner's view expected 3, so it learns the truth without any keyword. Comparing two runs of the same instruction is cheap and needs no deduction (D18). Collapsing every read value avoids a search; collapsing to the true value avoids an inferred value that would itself be false.

**Considered and rejected:**
- **Collapse by dependency** (any seen outcome reveals every value it read, consistent or not): ends bluffs that were never tested.
- **Minimal-subset search** (find the smallest set of false values whose correction explains the outcome): cheap enough — an instruction reads a handful of values — but only matters when two false values were read; kept as an alternative.
- **Collapse to an inferred value** (a 1-Power face kills a 2-Life creature → "2"): a deduction, and a new false belief.
- **Collapse to hidden** ("?"): reveals the least, at the price of a visible unknown state; kept as an alternative.
- **D137's numbers private to their owner, and its reveal points** (above).

→ `asymmetric-information.md` (§The consistency problem, §When a belief breaks, Open questions), `glossary.md` (View, Hard fact, Collapse, Live mana ledger, Reveal point), `overview.md` §9, `docs/architecture/implementation-plan.md` (Perception). Engine: Track B item 20.

### D145 — A trace's cost is the pips, not the mana spent

Closes part (a3) of `PLAN.md` Track A item 51. Before: "cost (narrowed: what was paid)" could mean either.

**Adopted (user):** a trace's cost property is magnitudes — the printed pips ({F}{F/M}{1}), X, the chosen mode, surcharges — never which mana, creatures or cards paid it (`object-properties.md` §4's magnitude rule, applied to mana). Cost is card content, so its view state follows the content's (D140). Which resources paid is a change to the pools, creatures or Hand, made by the paying instructions. A card that wants to know what was spent ("for each Element of mana spent") reads the history track (D98), where spending is an instruction output.

**Why:** with the mana spent as the cost, Remand would hand back a card demanding {F}{M}{W} instead of a hybrid and a generic pip — harder to cast because of how it was paid once.

**Considered and rejected:** **the mana spent as the cost** (above).

→ `object-properties.md` (Trace cost).

### D144 — Two kinds of duration; Duration counts the ends of player turns

Closes the duration question raised during `PLAN.md` Track A item 51. Before: D50's "an effect lives as long as its vessel does" next to "until end of turn" effects; Duration counted rounds.

**Adopted (user):**
- **An effect is permanent, or lives as long as its vessel:** a trace (its `Duration X`; destroying the trace ends it early) or a permanent with a static ability. "Until end of turn" is a short Duration, not a duration kind of its own.
- **Duration counts the ends of player turns** (Neutral turns don't count): Duration 1 lasts until the end of this turn, 2 until the end of the next player's turn, and so on. The default (5 rounds) becomes about 10, still a placeholder.
- **±X is a modifier**, counted while its vessel exists (`glossary.md` **Modifier**, pillar 5), which is why ±X must be the exact mirror of −X (D143).
- **The `~AP` lock (D116) stays** a cost flavor tied to the turn structure: it comes from a cost, not from an effect with a vessel.

**Why (user):** it gives the Aether, a concept unique to this game, more weight — interacting with traces matters more, and dispelling an effect is destroying its trace, with no layer or timestamp system. Rounds are too coarse: some effects must end with this turn.

**Considered and rejected:** **Duration in rounds** (too coarse).

→ `object-properties.md`, `glossary.md` (Aether, Modifier), `overview.md`, `economy.md`. Engine: Track B item 23.

### D143 — Stat changes: Health and AP have a current-only pair and a ±X pair; Power is one value

Closes the stat-change question raised during `PLAN.md` Track A item 51. Before: max- and current-tier for Attack, Life and AP (D70), with damage and heal on Life.

**Adopted (user):**
- **Health:** **damage / heal** change current only (heal capped at max); **+X / −X** change max and current together, as exact mirrors.
- **AP:** **slow / quicken** change current only; **+X / −X** change max and current together. Names to settle in item 39.
- **Power is one value**, changed by ±X: **current-Attack is dropped** from the property list. A permanent change is carried by Bounce (D70); a temporary one ends with its vessel (D144).

**Why:** Health and AP are consumed — damage uses up Life, actions use up AP — so a current tier tracks what is left; Power is never consumed. Other games agree: CCGs have damage/heal plus buffs for Health and only ±X for Power; AP games (Divinity: Original Sin 2, Pathfinder 2e, Dofus) change both tiers (Haste/Slowed per turn, Adrenaline or AP removal now). MTG's damage next to −X toughness confuses new players; here −X visibly lowers the max, which keeps the two explainable.

**Considered and rejected:** **+X raising the max only as far as current exceeds it** (user): not the reverse of −X — −2 then +2 turns 3/5 into 3/3, and a temporary +2 can't be undone cleanly.

→ `object-properties.md`, `overview.md`, `economy.md`, `glossary.md` (Life, Activation Points). Engine: Track B item 23.

### D142 — Cards are abilities: a spell card is an activated ability, a permanent card a definition plus a Cast ability

Closes the "player actions vs instructions" question raised during `PLAN.md` Track A item 51. A working route (user): the whole game as activated and triggered abilities, with the turn structure the one exception; backed out where it hurts more than it helps. Before: casting and activating were the same procedure (D75, D135) but distinct concepts; three trace groups (D109).

**Adopted (user):**
- **A spell card is an activated ability** usable from the Hand: cost, targets, instructions. **The card moving to Discard is part of its cost** (paid at C1, D135).
- **A permanent card is a definition plus an implicit Cast ability** whose one instruction creates the permanent defined there at the target location, from the card's current values (a bounced card's raised max Life included, D70). **The Permanent-trace group goes away:** it is an ability trace whose instruction carries the definition, which Flicker and Remand keep.
- The GUI still shows "cast"; nobody sees Cast as an ability.
- **Activate an ability** (with a cost override) is an instruction — "you may cast the top card of your Library without paying its cost" activates its Cast ability. Defined, outside the prototype.
- **"Put card X onto the Island"** is card wording for creating a permanent from card X; the card stays in the Mind and goes wherever the text says. Its effect's trace is the only one.
- Item 39 needs a name for a trace that comes from a card.

**Why:** casting already was activating, procedurally; one concept makes flashback (a second Cast ability), casting from elsewhere (where the ability works) and cost changes uniform. Abilities working from the Hand stay outside the prototype scope (D104) except Cast.

→ `object-properties.md` (§2, §5 Trace groups), `interaction-stack.md` §Casting, `effect-form.md`, `glossary.md` (Trace, Spell). Engine: Track B item 23.

### D141 — The basic instructions, and which are in the prototype *(a card moving to another Champion's Mind is a move, D148)*

Raised during `PLAN.md` Track A item 51 (each instruction needs a default view behavior, D140), checked against MTG (rule 701 and plain card text), Hearthstone, Legends of Runeterra, Gwent and our docs and content.

**Adopted (user):**
- **Two layers:** a slim set of basic instructions for the engine, and **aliases** for card wording, chosen for clarity (item 39) — "draw" is a move from Library to Hand, "return to Hand" a put, "counter" a destroy on a trace.
- **Move vs put:** **move** keeps the object and changes its place within its zone (section and position in the Mind or the Aether — a delay into Future is a move; terrain and Slice on the Island). **Put** makes a new object in another zone from a predecessor that ceases to exist (Bounce, Remand, Flicker, a card from another Champion's Library). What refers to the object survives a move but not a put: a buff, an equipped Item, a target on the stack.
- **Put, copy and create stay three instructions, marked essentially identical** (a new object from an object that ceases, from one that stays, from a card name or card object); **cease to exist and destroy** likewise. Merged once cards show whether they need to differ.
- **In the prototype:** draw, discard, mill, shuffle (any Mind section: Hand and Discard are ordered too), search, look at and reveal (any object with hidden or false information; one verb with an audience or two, open), move, put, create (permanents), copy, destroy (any object — MTG's exile and counter), fell, deal damage / heal and ±Health, slow / quicken and ±AP, ±Power (D143), gain / lose an ability, add mana, attach, detach, gain control, bond, unbond, raise, attack, defend.
- **Not in the prototype:** activate an ability (D142), create for cards and traces, choices at resolution (Discover comes back later as a composite of look at, choose, put).
- **Not instructions:** scry, surveil, fight, exchange, transform, regenerate; sacrifice (fell or destroy on a creature you control); Collapse Network (a composite of unbonds); un-summon (destroy the permanents whose `source` is a trace).

**Why:** card text should read clearly; the engine should not grow a verb per wording. Gain / lose stays apart from destroy: an ability is a property value, not an object, and gaining or losing it for a while needs a verb that comes back.

**Considered and rejected:**
- **Add mana excluded for good** (user, briefly): the network's passive mana fixes screw and flood, and an add-mana instruction doesn't bring them back — it is acceleration on top. In the prototype instead, to see the two play together.
- **One "return" verb across zones and sections** (Claude): what survives differs, so move and put are two.

→ `effect-form.md` §Instructions. Engine: Track B item 23.

### D138 — One zone per domain: a card's or trace's section and position are properties

Closes `PLAN.md` Track A item 5 (transitions within a domain). Before: seven zones (Hand, Library, Discard; Past, Pending, Future; Island) grouped into three domains, and the Mind counts were D137's private numbers.

**Adopted (user):**
- **Three zones: Mind (one per Champion), Aether, Island.** The term "domain" is dropped; it is the same thing as a zone now.
- **Principle: no object ever changes zone.** Cards exist only in the Mind, traces only in the Aether, every other object only on the Island (D59, now without exception). Every move across zones creates a new object.
- **Every card has a section — Hand, Library or Discard — and a position** (all three are ordered, D71). **Every trace has a section — Past, Pending or Future (D38's Sections) — and a position.** Both are ordinary properties: in a view each can be true, hidden or false, like any other property. Position applies in every section, so no property matters only for some values of another (an observation, not a principle; changed if a card ever needs it).
- **A move within a zone (draw, discard, mill, resolve, delay) changes the section and position of the same object.** Nothing else changes. A move between zones creates a new object (D69); an object that leaves every zone ceases to exist (D97).
- Card text keeps its words ("your hand", "the top card of your Library"); they name a section.

**Why:** the question "can the opponent know I drew two cards?" becomes the same true/hidden/false question as any property, instead of a special rule about zone counts. A hidden draw changes two cards' section; whether the opponent's view shows it is decided by the view defaults (`PLAN.md` Track A item 51). Counts are only a GUI summary. It also answers item 5 with one rule.

**Considered and rejected:**
- **M2 — Mind counts in the view** (D137 as written): a second kind of thing next to objects, needing its own marker logic when a hidden trace draws.
- **M1 — cards as visible objects, zones unchanged:** a card's zone could then never be hidden or false, so every hidden draw, discard or mill would show.

**Still open:** each property's default in each view (object type × controller), including section and position for the opponent — the user's lean is that every card is visible as an object with its content hidden (item 51). Library position stays invisible even to the owner (D71).

→ `overview.md` §1, `object-properties.md`, `interaction-stack.md`, `asymmetric-information.md`, `glossary.md` and others: rewrite tracked at `PLAN.md` Track A item 53; engine at Track B item 21. D137's Mind counts are superseded by this decision.

### D136 — Every Unit has a close-quarters ability, usable only against an enemy in its own hex and Slice

Closes `PLAN.md` Track A item 46. Before: a mixed Slice (D86, D132) had no rules of its own. Its enemies fought at distance 0 like any other attack and could simply move away, so the situation wasn't special at all. It arises only in special circumstances (entering a Slice with a hidden enemy, a control change, possibly more causes later).

**Adopted (user):**
- **Every Unit (Creature, Companion, Champion) carries one default close-quarters ability**, chosen per card from a small shared list. The card's class guides the choice; the ability fits the Unit's flavour. Like Move and Attack (D10), it is a default ability that a card can replace.
- **It targets an enemy in the same hex *and* the same Slice.** Another Slice of the same hex doesn't count. Distance is unchanged: D120's Melee still reaches the same hex in any Slice.
- **The active side gets the advantage.** Whoever uses the ability first gains from it. In the hidden-entry case that is usually the Unit that entered. Hidden creatures may carry "may enter a hex with an enemy" to start the situation on purpose.
- **Each ability ends the situation quickly**, not a back-and-forth between two creatures. This is a design target for every entry in the list; exceptions are possible.
- **The list is content** (`PLAN.md` Track C item 12). Starting ideas: an Attack that can't be Defended (assassins, after Dark Souls' backstab), Stun, Plunder (take its activated abilities), Pin (Hive's Beetle), Shove, Disarm, Turn (likely too strong for a default), Slip away, Devour, Spy. "Stops an entering enemy's movement" (Wesnoth's Ambush) is a keyword for some hidden creatures, not a list entry.

**Why:** a default ability adds one slot to every Unit instead of a special-case rule; the variety lives in card data. It gives classes a mechanical meaning (Fire Emblem's class commands: Steal, Talk, Shove). Most hidden creatures are revealed when another creature enters (D118 per effect), so the entrant usually learns what it walked into once it is there.

**Considered and rejected:**
- **One shared ability for every Unit:** less identity per card for the same rules cost.
- **An advantage for the hidden side by default** (Root's ambush cards): rewards hiding by default and doesn't fit the control-change case. Left to keywords (Track C item 5).
- **Making a mixed Slice a lasting lock** (zone of control, D14): the situation should be short.

**Moved to Track D:** what races mean mechanically (`PLAN.md` item 49).

→ `overview.md` §2, §4; `glossary.md`; `PLAN.md` Track A item 46 (closed), Track B item 19, Track C item 12, Track D item 49.

---

### D135 — Casting is choose → bind → pay, step by step; a cost sits on the step and the option it belongs to

Raised by re-running the Cinder Verdict walkthrough against D130 (`PLAN.md` item 47). Before (D130): every choice came first and one Pay step paid them all; a cost was a CNF of labelled branches paid in a fixed order of four kinds, and conditions could test which branch was paid. A payment choice therefore reserved an object without using it up: the creature chosen to be felled could still be chosen as a target, two fell costs could name the same creature, a card could be discarded twice, and the card being cast, still in the Hand, could target itself.

**Adopted (user):**
- **Steps:** C0 Legality (a check only) → C1 Commit → C2 Amounts → C3 Choose → C4 Target → C5 Enter Pending. **Each of C1–C4 is choose → bind → pay:** C1 — play the card; bind the source references; pay the card and every fixed cost. C2 — every `X`; bind it; pay the amounts. C3 — the options of each effect; bind them; pay the costs on the chosen options. C4 — the targets; bind them and the references that depend on them; pay the costs that depend on targets (caster's view, D130), a surcharge's own choices made here.
- **A payment choice is the payment.** What is paid leaves the decision state at once, so it can't be chosen, targeted or paid again.
- **Casting is one atomic event for the game**, a sequence of decisions only for the player. The steps work on a scratch state; everything paid **arrives** at C5 — the card and discarded cards in Discard, a felled creature's Remnant, the triggers. Abort throws the scratch state away. Not an exception to D112: the whole casting is one instruction-level event.
- **Binding comes before paying and replaces a reference by its values:** "sharing a type with the felled creature" becomes "being a Goblin or a Warrior" while the creature still exists — no last-known information needed.
- **Costs by step, each with its scope:** C1 fixed costs — read by nothing; C2 amounts — card-wide variables; C3 a cost on an option — visible only inside that option; C4 a cost on a target — belongs to that target. The CNF, branch labels, conditions on branches and the four-kind payment order are gone. Kicker is `choose 0..1 { ({2}) … }`; "+{1}: A / +{2}: B, choose one or more" is `choose 1..2 { ({1}) A } { ({2}) B }`; an option may have no instructions. Card text may be worded differently from the form.
- **One level of nesting, bounded by time.** An option of a choice made at casting may contain, where an instruction would stand, a **choice made at resolution** — with its own chooser and, like an instruction, an optional condition. An option of a choice made at resolution holds only instructions: one chooser at resolution, no third layer. **Why:** choices at casting are all the caster's and made at once, so nested ones flatten into one menu; flattening fails only across the casting/resolution boundary, which is exactly where nesting is allowed. It expresses Cinder Verdict's "(fell a creature you control) T1's controller chooses: they discard, or you draw" and Harrow's "if it falls this way, its controller chooses…" (`PLAN.md` item 41). The user's view: cards will rarely come close to this limit; card design will show whether they ever do.
- **A name introduced inside an option — its cost, its selections, its outputs — is visible only inside that option.** References flow inward, never outward. A card reads strangely if it refers to an option that may not have been chosen (user).
- **Semi-automatic mana payment (D125) works within each step.** A tie resolved in an earlier step can leave a later step unpayable; the player aborts. **Why (user):** one decision after another, each paid right away, feels more natural than one big payment at the end.
- **An option chosen at resolution may carry a cost.** Its chooser pays it when the instruction is reached, as an ordinary instruction — no transaction, no abort; the option can be chosen only if its cost is payable then; the trace's cost doesn't change (like a top-up). It makes sense only when the choice depends on something unknown at casting ("if it fell this way, you may pay {2} to draw a card"), which needs `PLAN.md` item 41.
- **Every choice made while casting is the caster's;** other choosers choose only at resolution. **Why:** casting is hidden from the opponent until C5 (D94); an opponent choosing during it would have to see it. MTG allows an opponent's mode choice at casting (CR 700.2e) on only three cards (Fatal Lore, Library of Lat-Nam, Misfortune), a template it no longer prints.
- **Accepted gap (user):** a surcharge another card adds may still pick an object that is a target of the same trace (target your own throwaway creature, fell it for the surcharge). If a card makes this hurt, it shows in testing and the card is adjusted. Noted at `PLAN.md` S3.

**Considered and rejected:**
- **Mark chosen objects as reserved** (Pay stays one step): a new object state, a "can't" rule, and a second way of being used up next to actually being used up.
- **Pay objects when chosen, add up amounts and pay them at the end:** keeps the most-constrained-first guarantee of D125, but splits the rule by kind; abort already covers a dead end.
- **Keep labelled cost branches read by conditions:** a condition then refers to a choice that need not have been made in that place.

Closes `PLAN.md` Track B item 16 (dynamic casting UI): the order is now the rules' own.

→ `interaction-stack.md` §Casting and activating, §Binding, §Resolution; `effect-form.md`; `object-properties.md` §4, §5; `resources-terrain.md`; `examples/cinder-verdict.md`.

---

### D134 — A connected Champion's or Companion's Move, Attack and Defend costs are values of their own; modifiers add on top

Raised by the rules-sync guess on Sucking Mire (`architecture/implementation-plan.md` G9): does a connected Champion entering its own Mire pay `(1+1)×2` or `2+1`? Before: the docs described the connected costs as the generic defaults "doubled while bonded" (D49, D116, D117), and the build multiplied the ability's cost by 2.

**Adopted (user):** a connected Champion or Companion pays `2AP` to Move, `2~AP` to Defend and `6~AP` to Attack — printed values, not a doubling rule; disconnected it pays the generic `1AP`, `1~AP`, `3~AP`. A cost modifier adds on top of whichever value applies, the same way as for any ability: entering its own Mire costs `2AP + 1`. "Twice the generic default" stays in the docs as the reason for the values. **Why (user):** a modifier must not be multiplied by a context rule; and if the base values change, the connected values may need adjusting too, so they are stated, not derived.

→ `champions.md`, `companions.md`, `economy.md`, `glossary.md`, `interaction-stack.md` §Combat integration, `overview.md`.

---

### D133 — No negative rules: whoever has an ability may use it; "a Structure never fights" removed

Reopens rules-sync guess G6 (confirmed earlier the same day as "Structures never defend, D107"). Before: D107 said "a Structure never fights" — the user's answer then was "a plain no".

**Adopted (user):** the rules describe what things *can* do — a positive list of default abilities per type — never what they can't. Whoever has a Defend ability may defend, if the ability's own conditions allow it; whoever has Attack may attack. A Structure has no Attack or Defend by default, but a card may grant them (a cannon tower that defends, a ghost rising from a Remnant). "A Structure never fights" is deleted, not reworded. **Removed with the same reasoning (user):** "only Actors and Traces can carry a Behavior" (the property lists already say it) and "a Companion's parent is a Champion or none, never another Companion" (nothing needs it — the Payment and Controller walks work at any depth; a created Companion's parent is its payer like a creature's). Also removed: "a Creature cannot bond terrain", "an Item is never a combat participant / never holds AP" and "a Creature's or Trace's parent is never another creature" — the positive definitions (default abilities; parent = the Champion or Companion that pays) already carry them. **Kept:** a Champion never becomes Neutral (it is its player's side); an Item's abilities work only while it's equipped (what an Item is). **Damage goes only to permanents with Life (user):** an undefended attacker splits its damage among the attacked entity's permanents that have Life in the attacked Slice, and an attack is legal only if one is visible there — only a terrain and Slice are attacked, so "a Remnant can't be attacked" is not a rule; a Remnant simply has no Life. **Why:** like Draw and Bond, combat is just abilities; a negative rule is words that add nothing beyond the positive list of defaults, and it blocks cards that would want the capability. **The one deliberate exception is Terrain**, which has no activated abilities and no AP (D87) — D87's reason (terrain can't be attacked or destroyed, so its engine could only be answered by position) plus the user's: players shouldn't have to watch every terrain for something it might activate. Terrain's other special rules were reviewed and kept (user): it never occupies a Mind zone, can never be bounced or destroyed, there is exactly one per hex, and its Terrain Type is flavor only.

→ `object-properties.md` §5, `interaction-stack.md` §Combat integration.

---

### D132 — A hidden occupant never blocks entry; D68's redirect loop is removed

Raised by D130 (deal with hidden information at resolution, where the owner can respond). Before: entry into a Slice needs every occupant to share the mover's controller and room under the 3-slot cap (D86, D119), checked against the mover's view at declaration; if a hidden occupant made the destination illegal against true state, D68's redirect loop ran at casting — cost committed, pick another destination or cancel.

**Adopted (user):**
- **Controller uniformity is checked only against the acting Champion's view, at declaration.** A hidden occupant of another controller never blocks entry: the mover enters and the Slice becomes **mixed**, as after a control change (D86). A visible enemy still blocks.
- **At resolution only the capacity is checked**, against true state: a Slice that is truly full makes the instruction fizzle, its cost spent. An enemy that becomes visible or enters in response doesn't stop the move — both end up sharing the Slice; only filling the Slice to capacity blocks it.
- **D68's redirect loop is gone**, and with it the one exception to "abort rolls the whole transaction back" (D94): nothing at casting is checked against true state any more.

**Why (user):** a hidden creature is already strong; it should not also make every move into its hex fizzle and be treated as invincible. Hiding should reward staying off the routes others travel — and under D118's recommended default, entering usually breaks the concealment, so a hidden creature sitting on a route gets exposed. Mixed Slices didn't exist when D68 was decided; now they do, so reuse them.

**Consequences:** mixed Slices become common, not rare — `PLAN.md` Track A item 46 (enemies sharing a Slice) gains weight. Responding to a move by occupying its destination ("block the path") only works by filling the Slice.

**Considered and rejected:** the move fizzles at resolution when its destination is illegal for any reason, hidden enemy included (Claude's first proposal) — makes hidden creatures roadblocks; checking uniformity again at resolution — revealing a hidden creature in response would become a free way to block.

→ `overview.md` §2, `ancestry.md` §Control changes and occupancy, `interaction-stack.md`, `structures-items.md`.

---

### D131 — Every hidden-information ability states when and how far it collapses *(replaced parts in `decisions-archive.md`)*

Raised while settling D130's top-up (it tells the caster their view was wrong). Before: a soft claim collapsed to the full truth when a hard fact tested it, detected by the engine (D18); only concealment effects had to state their own breaking (D118).

**Adopted (user):** a design rule for card text. Every ability that creates hidden or false information (a concealment, a Mimic face…) states when it collapses and what part of it collapses. What is revealed need not be the full truth, but it must fit what the observer saw. **Why:** the game must not become one of memorizing — a creature shown as 2 power that dealt 4 damage must not keep showing 2 power. The engine can't work out in general what an observation forces to be revealed (too many cases), so card text says it.

**Considered:** the engine deciding what must be revealed — too hard. A fixed base rule that being targeted or fighting reveals a permanent's true identity — a sensible default for many cards, but not right for all (a Mimic copying a creature with the same combat stats needn't collapse in combat).

→ `asymmetric-information.md` §The consistency problem & belief model.

---

### D130 — Casting order: amounts, choices, targets, then pay; a cost may depend on targets and is topped up at resolution *(replaced parts in `decisions-archive.md`)*

Closes the question raised by G9 (Sucking Mire: "moving onto it costs 2 AP"). Before: D70's order pay → choose → target, so a cost could not depend on a target — yet the rules already had two such costs (a terrain's bond surcharge, an Item's equip surcharge).

**Adopted (user):**
- **A cost may depend on targets.** It is computed against the **caster's view** and paid in full at casting.
- **A surcharge another card adds** (e.g. a creature's "targeting this costs a discard") is only known after targets, so its choices are made when it is paid — at casting or at a top-up. Harmless: nothing on the casting card can read them. Restricting such surcharges to amounts (mana, AP, life) would remove the choice — parked as `PLAN.md` possible simplification S3.
- **Top-up at resolution.** If the true cost of a target is higher than what was paid, the caster **may** pay the difference at the start of resolution, **per target**; a target not topped up becomes illegal for this trace. A lower true cost is not refunded (paid cost is never refunded). Optional, so an illegal target doesn't tell the opponent whether the caster couldn't or wouldn't pay — the mana balance is hidden.
- **Collapse.** A top-up demand is a hard fact against the caster's view, so it collapses as D131 says.
- **Remand.** A trace's cost is exactly what was paid at casting (`object-properties.md`), surcharges included; a top-up never changes it, and Remand always comes before resolution. A recast therefore pays the old surcharge again inside the locked cost, plus whatever the new target adds — accepted as simple (user), Remand being a niche case.

**Why:** target-dependent costs ("targeting this costs 1 more") can't always be rewritten as something else. Deferring the hidden part to resolution keeps hidden information protected by the response system: the owner can counter, Remand or move the target before anything is revealed, and targeting a suspected Mimic is no free scouting. Triggers already pay at resolution (D35), so the mechanism exists. A player who expects a possible surcharge plays around it by keeping mana open.

**Considered and rejected:**
- **Only amounts come before targets; other payment choices stay at payment.** A target can depend on which creature was felled, not only on a number; and "all choices first" needs no test of which choice is referenced later.
- **No target-dependent costs at all** (a cost may depend only on the card or ability and its source; D70 unchanged). Clean, but a steep sacrifice — "targeting this costs 1 more" has no general equivalent.
- **Compute the true cost at casting** (top-up before entering Pending, like D68's redirect). Simpler, but the hidden information is lifted at casting and the owner can do nothing against it.
- **A cost reads only the caster's view, never topped up.** Then a disguised Mire wouldn't slow anyone down — hidden information silently cancels the ability.

→ `interaction-stack.md` §Casting and activating, §Resolution; `object-properties.md` §4.

---

### D129 — Terrain has no move cost; abilities have subtypes

Closes rules-sync guess G9's open part (Track B item 7). Before: Terrain had a move-cost property (base 1, changed by static abilities); the build and Sucking Mire (`"moveCost": 2`) used it.

**Adopted (user):**
- **No move-cost property on Terrain.** Move is an ability like any other — every creature-type permanent just has it by default. A terrain that changes movement does it with a static ability on Move abilities, which D87 already allows: "Move abilities of permanents on this terrain cost 1 more AP" (leaving), or "Move abilities that target this terrain cost 1 more AP" (entering, a target-dependent cost, D130).
- **Abilities have subtypes**, the same mechanism as card subtypes (D99). The defaults carry theirs (Move, Attack, Defend…), so a card can modify abilities by subtype, and a creature's own "Leap" with subtype Move pays a Move surcharge too. An ability's subtype describes its kind of action; a card's subtypes (race, class) are **not** inherited — "an ability of a Goblin" is a query on its `source`.

**Considered:** inheriting the card's subtypes onto its abilities (so a bounced ability, Track D item 42, reads "Goblin") — dropped: `source` already answers it, and it would mix two axes. Deriving Physical from the subtype — parked as `PLAN.md` possible simplification S1. Dropping terrain capacity too — kept, parked as S2.

**Open:** whether every card and ability must have at least one subtype, groups like "Combat", and a symbol per ability subtype on cards.

→ `object-properties.md` §5.

---

### D128 — Priority runs whenever Pending is non-empty; triggers default to Reactive

Closes rules-sync guess G5 (`architecture/implementation-plan.md` §3). Before: the Beginning and End phases opened no priority at all, so beginning- and end-of-turn triggers resolved one after another with no response window (overview §3: "no priority this phase to start"; End "same as Beginning").

**Adopted (user):**
- **Priority runs whenever Pending is non-empty, in any phase** (D123's rules). Beginning and End end as soon as Pending is empty. Only the Action phase gives priority with Pending empty — that is where the active Champion acts — with its closing pass round (D123). Setup still opens no priority (D114).
- **A triggered ability is Reactive unless its card states another Speed.** In Pending a trigger's Speed only matters as Instant or not: an Instant trigger resolves with no response window (D92), any other is respondable by Reactive and Instant cards. A card that wants a trigger to resolve unanswered says Instant.

**Why:** Speed is already per ability, and the Instant row already says "nothing can respond, `Now` advances automatically" — a phase-level "no priority" rule duplicated what per-trigger Speed covers. And a turn still feels like one phase with some bookkeeping at the start and end: with no Beginning/End triggers, or only Instant ones, it really is one, and costs no extra clicks.

**Considered and rejected:**
- **P2 — the Action phase's rule in every phase** (each phase ends only when every Champion has passed in succession with Pending empty, so Quick/Reactive cards are playable in every Beginning and End). One rule with no special case, but two extra passes per Champion per turn — 12 instead of 4 per round, since neutral turns always occur (D56, D60) — and the turn no longer feels like one phase. Built and briefly adopted, then reverted before testing (simplicity first).
- **Remove the Action phase's pass round** (would make P1 fully uniform). Already rejected in D123.
- **Triggers default to Instant** (keeps the old no-window behavior as the default). A trigger stays respondable like every other trace unless its card says otherwise.
- **Skip a neutral turn when no Neutral permanent exists** (as a click-saver). Breaks D56: a neutral turn's occurrence must never reveal whether a hidden Neutral permanent exists.

→ `interaction-stack.md` §The primitive and §Triggered vs. activated abilities; `overview.md` §3.

---

### D127 — Ranged a-b, and a defender must reach its attacker

Closes item 45 and rules-sync guess G7 (`architecture/implementation-plan.md` Q6). Before: `Ranged N` attacked within distance N (0 included) and was the sole exception to universal retaliation (D19); whether it could be defended was open, and the build let anyone defend it without hitting back.

**Adopted (user):** **Ranged a-b** is a keyword with two values: the attack reaches distances a to b — Ranged 1-3 reaches 1, 2 or 3, not 0 or 4+. Melee is the default reach 0–1 (D120). **A creature may Defend only if the attacker is within its own attack reach** — distance only, no Slice, AP or "can attack" check. Retaliation then has no exception: every defender hits back, Ranged attackers included. Everything else stays: an attack still names a terrain, a Slice and an entity (D115), flyers still attack Sky or Ground, Sky creatures still help defend Ground, defenders still stand on the attacked terrain.

**Why:** the research (Duelyst, Fire Emblem, Wesnoth, Advance Wars, Gloomhaven, HoMM) mostly ties hitting back to reach; folding reach into who may defend makes it one structural rule instead of a Ranged exception. **Consequences accepted:** a melee creature can't defend against a Ranged attack from 2 away, which hits directly; a Ranged 2-3 creature can't defend against melee; a Ground archer damages a flyer only by defending. The user isn't sure this is intuitive — a playtest point.

**Considered and rejected** (`history/playtest-variants.md` §Ranged attacks): defendable without retaliation (G7); undefendable; "the defender could attack the attacker" with Slice and "can't attack" included (Ground could no longer defend against a diving flyer, and a "can't attack" creature could never defend); an attack target without a Slice (Claude — a flyer could no longer fight flyers only); deriving the Slice as "Sky first".

→ `overview.md` §4, `interaction-stack.md` §Combat integration, `glossary.md`, `economy.md`, `object-properties.md` §5, `tools/card-format.md`.

---

### D126 — A Companion moves and collapses exactly like a Champion

Closes item 38 and rules-sync guess G10 (`architecture/implementation-plan.md` Q4). Before: `companions.md` priced a Companion's move `1AP` "if its network collapses" and `2AP` "if it stays connected" — a collapse as a side effect of stepping off, with no Collapse Network ability and no realm lock; the build followed that (a move off its bonded terrain cut its network).

**Adopted (user):** "a Companion is exactly like a Champion, with the exception of the Mind zone." It gets **Collapse Network** (`0AP`) and the **realm lock**: while connected it moves only onto terrain it bonded itself (`2AP`); while disconnected it moves freely (`1AP`). **Connected is a state check (user):** the terrain it stands on is bonded by itself, knotted or not (D122) — not whether it ever used Collapse Network. Losing that bond any other way (a theft, an unbond) disconnects it just the same, and cuts the rest of its network (D122). Its default abilities are the Champion's minus Draw. **Why:** one mechanism for one idea, instead of two. **Not changed:** its pool never shares (D22); Collapse Network may still be removed for everyone (Track C item 4).

→ `companions.md`, `glossary.md`, `object-properties.md` §5, `tools/card-format.md`.

---

### D125 — Mana: disjunctive pips and pool mana, paid by domination

Closes rules-sync guess G1 (`architecture/implementation-plan.md` Q3) and the cost-model half of item 37. Before: the build kept an Element-keyed pool, and paid generic pips automatically from the largest pile — which could spend the Element a later card needed.

**Adopted (user):** a cost pip is **generic** or **colored**, a disjunction over Elements (one Element = colored, two = hybrid, …); nothing else exists. A pool mana is **colorless** or **colored**, likewise a disjunction. A mana **dominates** another if it pays every pip the other pays. Pips are paid **from the most constrained to the least** (one Element, two, …, generic last); each takes the mana every other candidate dominates, and when the candidates tie, the player chooses. **Why:** the automatic picks are exactly the safe ones — no player could do better by choosing — and most payments need no choice: colorless always pays generic, a one-Element pool never chooses, a two-Element pool only chooses which Element pays generic. **Considered and not adopted:** looking ahead across the whole cost to remove choices that lead to a dead end (Claude) — the user keeps the obvious algorithm first; a smarter one may come later. (The engine lists only complete payments, so a dead-end choice is never offered anyway.)

**Still open (item 37):** whether generic pips exist at all — dropping them would make nearly every payment automatic.

→ `resources-terrain.md` §Mana costs and payment, `object-properties.md` §5, `tools/card-format.md`.

---

### D124 — Quick is playable while Pending is empty

Closes rules-sync guess G3 (`architecture/implementation-plan.md` Q2). Before: Quick was playable when "only Physical Traces are in Pending" (D45), and it was open whether that holds with Pending empty.

**Adopted (user):** yes. "Every trace in Pending is physical" is a for-all over an empty set, so it is true. The speeds nest: Slow ⊂ Quick ⊂ Reactive ⊂ Instant, and Quick reads as "can't respond to spells". A Quick card can be played in the end-of-phase pass round (D123). **Rejected:** Quick needs a Physical Trace in Pending, i.e. combat-trick speed only — no case was found that it protects against.

→ `interaction-stack.md` §The primitive.

---

### D123 — Acting keeps priority; passing goes around in turn order

Closes rules-sync guesses G2 and G4 (`architecture/implementation-plan.md` Q1). Before: the docs said priority follows "the standard multiplayer rule — active-first, then each other participant in turn order" (`neutral-permanents.md`), which left open who gets priority right after a Champion puts something into Pending.

**Adopted (user):** a Champion with priority may act as often as they wish; acting never passes priority, only passing does. A pass hands priority to the next Champion in turn order (APNAP). After a resolution, the active Champion gets priority (in a neutral turn, the next Champion in turn order). This is MTG's CR 117.3b–d.

**The phase ends with a pass round (user, guess G4):** an Action phase ends only when every Champion has passed in succession with Pending empty. Ending one's own phase is a pass; every other Champion then gets one window with Pending empty — MTG's "at end of turn, I cast…". In a neutral turn the round runs after the Behaviors are done. **Cost accepted:** the non-active Champion passes once more per turn. **Rejected:** the phase ends when the active Champion passes, so on the opponent's turn a Champion can only respond to something already in Pending.

**Why:** some rule must say who acts after a cast; "you keep priority until you pass" needs no transfer at all, so it is the simplest one. It also lets a Champion stack a second response on their own trace before the opponent answers. **Considered and rejected:** priority returns to the active Champion whenever something enters Pending — an extra transfer rule, and it interrupts a responding Champion mid-sequence.

→ `interaction-stack.md` §The primitive.

---

### D122 — A knot pauses, a gap cuts; knotted terrain can be stolen

Closes item 28 (can the opponent bond a terrain whose path is merely blocked?). Before: a bond lasted until an explicit unbond; a terrain whose path was broken — by a knot or by a missing bonded hex — kept its record and sat paused; whether the opponent could bond it was undecided.

**Adopted (user, flavor first):** a knot still lets the connection through — it only stops the flow; a gap cuts it.
- **Paths:** a bonded terrain's path runs through terrain bonded by the same root, back to the root's current tile, which must itself be bonded.
- **Knot:** knotted terrain stays connected and keeps its bond record; it and everything behind it are paused.
- **Cut:** a bonded terrain with no such path at all, counting knotted terrain as a path, loses its bond record immediately. So an unbond in the middle of a network costs its owner everything behind it, not just one Bond to repair.
- **Theft:** terrain paused by an enemy knot — the knotted hex and everything behind it — can be bonded by the opponent with an ordinary Bond, if reachable from their network; the new bond replaces the record. Stealing a chokepoint cuts the rest of the branch. The user: the networks must already meet, so mana is usually plentiful and the hex matters mostly tactically; it can be stolen with no special card beyond a Knotting creature; and narrow branches reaching toward the enemy are fast but exposed — wanted.
- **Realm lock:** "connected" uses the cut check, so a fully knotted Champion stays confined; the only way out of the realm is Collapse Network. (Before: a fully knotted Champion counted as having no network and could walk off.)

**Root tile must be bonded — no special rule (user).** An unbond effect on the hex the Champion stands on therefore cuts its whole network, like Collapse Network. Claude proposed letting the Champion's own tile always count as part of a path; rejected — instead, cards that unbond must avoid targeting the root's tile, or be priced for it (Track C item 4).

**Considered:** a theft that takes only the stolen hex and leaves the rest paused (Claude) — not needed once a cut unbonds. D8's rejection of permanent severing "from mere positional blocking" still holds: a knot alone only pauses; a permanent loss needs the opponent's Bond and a network that reaches the branch.

→ `resources-terrain.md` §Severing behavior, `ancestry.md`, `champions.md` (realm constraint), `companions.md`, `glossary.md`, `overview.md` §5, `structures-items.md`, `PLAN.md` items 28 (closed), 38, Track B 4, Track C 4.

---

### D121 — Only permanents with Knotting block mana paths

Closes item 26 and the D8 row of the pessimistic-default audit. Before: any enemy creature on a path node paused the mana behind it (D8), in any Slice (D86). Two problems followed: a hidden creature in the Root Slice paused the opponent's mana and the red leylines gave away roughly where it stood, and flyers parked over your terrain paused your mana while your Ground creatures couldn't attack them (D115, D119).

**Adopted (user):** blocking mana paths is a keyword, **Knotting**. A terrain is knotted for a side while an enemy permanent with Knotting stands on it, in any Slice; a path is clear while none of its terrains is knotted. Nothing else blocks, whatever its type — Creature, Companion, Champion, Structure alike (a Structure with Knotting is a wall). A Neutral permanent with Knotting is an enemy to both sides.

**No special rule for concealment or Slices (user):** a hidden permanent with Knotting still knots, and the paused leylines reveal roughly where it is — the controller's own tradeoff. Flying plus Knotting is a strong pair, but no rule is needed: like Flying in MTG, both keywords are expected to be common, so the answers are common too.

**Considered and rejected:** keep blocking as a default for Ground creatures only, with keywords for Root and Sky (Claude) — argued that denial is the reason D8's economy is positional (pillar 2) and shouldn't depend on a keyword. The user: then print the keyword often, like Flying; its frequency per Element also gives Elements identity. And "Root creature" is no category — Subterranean creatures change Slices.

**Name:** Knotting — a knot in the leyline holds the flow back until it comes loose, matching the reversible pause. Rejected: Blockade (too technical), Dam (watery), Choke, Leybreaker ("break" suggests permanent). Provisional until item 39.

→ `resources-terrain.md` §Severing behavior, `ancestry.md`, `champions.md`, `companions.md`, `glossary.md`, `overview.md` §5, `structures-items.md`, `history/pessimistic-default-audit.md`, `PLAN.md` items 14, 26 (closed), 39, Track B 4.

---

### D120 — Melee is distance ≤ 1, so creatures on the same hex can fight

Closes item 25. Before: "Melee = adjacent", which left attacks within a hex unstated — though they are ordinary, since a flyer may enter a hex with enemies on the Ground (D119), and a Slice can become mixed through a control change (D86). **Adopted (user):** Melee means distance ≤ 1 — the same hex or an adjacent one. Same-hex attacks follow the Slice tables like any other (D115): Sky → Ground on the same hex, and co-located enemies in a mixed Slice (the user wants them able to fight, flavor). Ranged at distance 0 moved to item 45. Whether creatures sharing a hex *and* a Slice need any special rule (moving away, defending, capacity) is item 46, with the user's caveat to keep the default rules' complexity in mind for so rare a case.

→ `overview.md` §2, §4, `PLAN.md` items 25 (closed), 45, 46.

---

### D119 — Entering a hex depends only on creatures in your own Slice; enemy Structures don't block

**Question (user):** what is the general rule for when a creature can enter a hex — is "no enemy in the same Slice" sufficient, given flyers above enemy Ground creatures, and abilities like "defends as though it had flying" or "attacks as though it had flying"?

**Adopted (user):** the existing rule is sufficient — a creature may enter a Slice if every occupant shares its controller and there is a free place (D86; Neutral counts as another controller). Entering and fighting are separate questions: such abilities change who may attack or defend whom (the Slice tables, D115), never who may enter. **Occupants are creature-type permanents** (Creature, Companion, Champion): an enemy Structure in its own slot (D40) does not block entry (Claude raised the gap; the user: a creature may enter a hex with an enemy's Structure — blocking would be a keyword, by the pessimistic default), nor do Remnants or loose Items (capacity-exempt).

**Also corrected in discussion:** that flyers parked over your terrain block your mana holds only under today's default (every enemy blocks paths, D8/D86); item 26 proposes making mana-blocking a positive ability instead, which would make it a flyer's paid choice.

→ `overview.md` §2, `PLAN.md` item 26.

---

### D118 — The attacker stays put; no base rule reveals on contact — each concealment effect specifies what breaks it

**Advancing after combat.** The user proposed that an attacker that leaves no defender alive may or must enter the attacked hex. Claude's problems with it: "no defender survived" isn't "the Slice is empty" (non-defending creatures may remain, and D86's entry rule forbids entering then); Ranged attackers would jump N hexes; the attacker is locked by `3~AP`, so the advance would have to be part of the Attack's resolution; "must" can drag an attacker from a safe hex into the enemy's reach; and "may" is a free move — a generous default against D14. **Adopted (user):** the attacker stays; advancing is a keyword's job — **Pursue** parked as an idea in `docs/cards/card-ideas.md`.

**Revealing on contact.** The user proposed: attacking a hex in Root reveals hidden Root occupants there; attacking in Ground or Sky reveals hidden Ground and Sky occupants (Sky defends Ground, D115); the same for entering a hex. Claude: for default Root concealment this is mostly D67 already (a Melee attacker or mover is next to the hex); should "revealed" also strip a Mimic face (a full Reveal, D102)? Claude proposed lifting only the concealment, since a Mimic collapses by its own rules when it acts. The user agreed on Mimic and concluded that this can't be one general rule: **every concealment effect must specify it itself.** There can be further Root concealment beyond the default, so D67 is just the default's own specification.

**Adopted:** no base rule reveals on attack or entry. Default Root concealment keeps D67 as its specification. Every other concealment effect must state what happens when (C-a) its hex is attacked in its Slice group, (C-b) an enemy enters its hex in its Slice group, (C-c) the concealed creature acts itself — a Track C rule for every concealment card. The Slice groups (Root; Ground + Sky) and "the concealment breaks" are the recommended default for C-a/C-b; breaking concealment doesn't lift a Mimic face and is one-time, like Reveal (D102).

→ `interaction-stack.md` §Combat integration, `asymmetric-information.md` §Concealment effects, `docs/cards/card-ideas.md`, `PLAN.md` Track C.

---

### D117 — Defend is a Quick response to the Attack; defenders are added bit by bit; the Attack checks at resolution whether it is defended; Champion and Companion Defend doubled while bonded

Closes item 34. Before: "when a hex is attacked, the defending Champion declares defenders" (D4), with no rule for *when* relative to the Attack trace in Pending.

**Requirements (user):** a priority window after the attack and another after defenders are declared, as in MTG; no special rule for Attack traces — physical actions either use the Aether as intended or stay out of it; no new area to track beyond Matter, own Mind and Aether.

**Adopted:**
- **Defend is an ordinary Quick activated ability**, "Defend target attack" (`1~AP`, D116), played by an eligible creature (D115) in response to the Attack trace. It leaves a Physical Trace; when that resolves, it **adds its creature to the Attack trace as a defender** (user's precision: a change to the Attack trace's state). Not defending is passing. Both windows come from the Aether itself, with no special rule. D75's "at most one Physical Trace in Pending" becomes "at most one Slow physical action"; Defend is the one Quick physical action.
- **At resolution the Attack checks whether it still has a defender** (user's choice, deliberately unlike MTG). Defended: the attacker splits its Attack freely among the remaining defenders, each deals its Attack back (D13). Undefended (none added, or all gone — e.g. killed in response): free split among all legal targets — the attacked entity's permanents in the attacked Slice, in the attacking Champion's view — no retaliation.
- **Defending bit by bit** (user's observation): defenders are not declared once; they accumulate one Defend at a time, interleaved with tricks. Unlike other TCGs, where the block is a one-time choice. The user sees nothing bad in it: a valuable creature never has to commit until it's needed. Claude's notes, accepted as a playtest point: the defender can always answer last with another body, which weakens the attacker's tricks and removal aimed at defenders, and together with gang-up tilts combat toward the defender — braked by each Defend's AP cost and lock.
- **Champion and Companion Defend `2~AP` while network-bonded, `1~AP` otherwise** (user) — doubled like their Move and Attack (D49). This answers the open S9: a Champion that spent its AP on Draw and Bond can't defend until its next turn, like any creature that spent everything.
- Neutral permanents defend through their Behavior whenever they could respond to an Attack on them (D65's Behavior part unchanged).

**Considered** (full write-ups in `playtest-variants.md` §Combat procedure): a two-stage Attack trace (Claude's first attempt — a special rule, ruled out); physical actions outside the Aether (rejected by the user — another area to track); the user's "Defend counters Attack" (rule-made Instant trace, a trace removing another — three mechanisms X doesn't need); a **combat phase** (several attackers per hex, who defends whom, fewer interruptions — but loses interleaving, puts windows outside the Aether, and splits physical actions into trace and non-trace; never rejected in the records, only postponed, D3/D21; item 2 could bring its depth back). The user noted X has its own "who blocks whom": with limited defences across several attacks in one turn, choosing who defends what is not trivial.

**Split out:** whether a Ranged attack can be defended (Ranged takes no retaliation, D19) — `PLAN.md` Track A item 45.

→ `interaction-stack.md` §Speed, §Combat integration, `overview.md` §4, `economy.md`, `champions.md`, `companions.md`, `glossary.md`, `neutral-permanents.md`, `playtest-variants.md`, `PLAN.md` items 25, 34 (closed), 45, Track B.

---

### D116 — Attack costs `3~AP`, Defend `1~AP`; new cost flavor `~`; Defender keyword; `*` means once per own-turn cycle

Part of items 25 and 34. Before: Attack `3!AP` (drain all), Defend `0*AP` (free, once per turn by the notation, once per round by D65), and a card could opt in to "this creature can't defend" (D15).

**Adopted (user's call):**
- **New cost flavor `x~AP`:** spend exactly x; the permanent can't activate abilities until the end of **this turn**. Leftover AP stays, usable again from the next turn on — including other players' turns.
- **Attack `3~AP`:** no multi-attack and no hit-and-run by default (the creature is done for the turn), but leftover AP stays for defending.
- **Defend `1~AP`:** one defence per opponent turn (Neutral A, the other Champion, Neutral B are separate turns) while AP lasts. Every AP held back from your own turn buys one more defence. The limit comes entirely from AP and the lock: no flag, no history-track query (item 34's proposal), no `*`.
- **Defender keyword:** "this creature has `1AP: Defend`" — any number of defences per turn while AP lasts. The user: multi-defend is the pessimistic-default keyword, and Life is the real cost of defending (each defence takes the attacker's damage).
- **`!` stays, for powerful abilities:** a strong ability priced with `!` costs the permanent its defence until its next refresh — power balanced against defending (user). This replaces D15's opt-in "this creature can't defend" text: out of AP means can't defend, no separate mechanism.
- **`*` means once per own-turn cycle** (from the controller's turn start to its next), replacing "once per turn". The user: "once per turn" for Defend was a terminology mismatch, always meant per cycle (D65's "once per round" was the same thing, imprecisely). Draw and Bond, the only other `*` uses, are Slow, so nothing changes for them.
- Champion and Companion follow the generic default (D49): Attack `3~AP`, `6~AP` while network-bonded; Defend `1~AP`.

**How it was chosen:** six combinations — Attack `3!`/`3~` × Defend `1!`/`1~`/`0*` — played through scenarios with an agreed desired outcome each, rated ✓ / – (preference) / ✗. Full table and scenarios: `playtest-variants.md` §Attack and Defend cost. In short: **S1** (mirror trade, desired a draw) fails every `3!` option without `0*` — D15's original bug. **S2b** (all in, the target survives) splits by style: O1–O4 reward getting into position, O5/O6 (free Defend) reward attacking; both legitimate. **S2c** (step adjacent and wait) shows O1/O2 make whoever attacks first lose — a standoff. **S3** (Neutral bait: an idle creature attacked by a Neutral monster, then by the other Champion) — the user wants one defence per opponent so monsters aren't a tax on defence; only `1~` passes. O4 was the only option without a ✗. **The user then asked for a case where O4 is bad, especially a standoff.** None in the duel; the closest is a **fortress** of Defender-keyword creatures (gang-up, D13, against one-at-a-time attacks). Accepted: unlikely, and worth rewarding on a hex important enough to need attacking. Without the keyword, `1~` breaks fortresses — defenders are locked after one defence per turn. **Known cost:** the `~` lock also stops a defender or attacker from using its own abilities for the rest of that turn (e.g. a pump after defending).

**Considered on the way:** `3*AP` Attack (hit-and-run becomes the default); `3AP` Attack (Life doesn't limit undefended attacks — no retaliation — so multiple attacks per turn would be free); `1AP` Defend as the base (kept as the Defender keyword instead).

**Not settled here:** the combat procedure itself — Defend as a Quick response in the Aether vs. a combat phase (item 34); Ranged attackers and defending (item 34); S9, a Champion that spent all its AP on Draw and Bond can't defend (item 34).

→ `economy.md`, `overview.md` §4/§10, `champions.md`, `companions.md`, `glossary.md`, `playtest-variants.md`, `PLAN.md` items 25, 34, Track B.

---

### D115 — An attack names a terrain, a Slice and an entity; same-Slice attacks plus Sky → Ground; who defends; when an attack is legal

First part of item 25 (combat). Before: an attack targeted a terrain (D100), the defending Champion declared defenders "on that hex" (D4), and D19's table allowed Ground → Root and Sky → Root "if located".

**Adopted (user's call on every point):**
- **The target is a terrain, a Slice and an entity** (Champion A, Champion B, or Neutral). A terrain alone doesn't say whom you attack: a hex can hold creatures of three entities, split across Slices, or mixed within one after a control change (D86). **Neutral is one entity** — the Neutral A/B seats only name turns; splitting them would make one attack on a mixed group of Neutrals impossible for no gain. Slice and entity are selected automatically when only one is legal; in the base rules only a Sky attacker ever chooses a Slice.
- **Slice rule:** an attacker may only attack its own Slice, plus Sky → Ground. Ground → Root and Sky → Root ("if located") are dropped: Root is its own battlefield, and so is Sky unless a flyer chooses to attack Ground. The user: these are the base rules; keywords will add many more pairs (e.g. Ground → Root), just not as ground rules.
- **Who may defend:** creatures of the attacked entity on the attacked terrain, in the attacked Slice; when Ground is attacked, Sky creatures too. Ground never defends the Sky, Root only defends Root. Retaliation by defenders stays universal (D19). Hidden Root creatures therefore never defend against a Ground attack, so defending never reveals them.
- **Legality:** an attack can be declared only if the attacking Champion's view shows at least one permanent of the attacked entity in the attacked Slice of that terrain — the attack version of "no paying into a target-less cast" (`interaction-stack.md` step 0). It also means attacking can't probe for hidden creatures. If the only such permanent leaves in response and nobody defends, the attack does nothing and its AP stays spent — what the response bought. **Undefended:** the attacker's damage goes only to the attacked entity's permanents in the attacked Slice.

**Considered — no Slice in the target** (Claude's first proposal, briefly agreed): the Slice rules would decide who defends, so Sky defending a Ground attack needed no exception. The user found the flaw: without an attacked Slice, "Sky attacks Ground" (Ground may defend) and "Sky attacks Sky" (Ground may not) can't be told apart. **Considered — defending by connected Slices** (Claude: Ground and Sky defend each other, keeping D19's "air superiority = initiative, not immunity"): the user chose the per-Slice rule, under which Ground can't defend the Sky.

**Noted, left to item 26:** a Ground creature may enter a hex where the enemy has only flyers (entry checks one Slice, D86); flyers over your terrain then block your mana there, and your Ground creatures can't attack them.

**Still open in item 25:** damage assignment and Trample-like exceptions, range at distance 0 (Melee, Ranged, same-hex attacks, mixed Slices), and Defend (item 34).

→ `overview.md` §4, `glossary.md`, `object-properties.md`, `PLAN.md` item 25, Track B.

---

### D114 — Setup is Champion A's first Beginning phase; Setup-placed permanents enter the Island; terrain and hand mulligans; Haste X

Closes item 29. D89 exempted Setup-placed permanents by saying Setup isn't entering the Island — added at write-up time only to protect D76 (Champions start at 4 AP, Champion A's first refresh skipped). With it came `setup.md`'s workaround: a Setup-placed permanent couldn't use "enters the Island" and had to say "at the beginning of your first turn".

**Adopted (user's call on every point):**
- **Setup-placed permanents enter the Island** under the ordinary rules: 0 Activation Points unless Haste (D89, now with no exception), and their "enters the Island" triggers fire. The "at the beginning of your first turn" workaround is gone.
- **Setup replaces Champion A's turn-1 Beginning phase.** Champion A is the active seat, which gives Setup's triggers an APNAP order without a new rule. D76's "skip A's first AP refresh" is no longer a rule: A's first Beginning phase is Setup, which places instead of refreshing. D76 rejected "skip A's whole first Beginning phase" because it would lose the payoff mana-refresh and A's first-turn triggers; neither applies here, since mana credits live (D77) and Setup keeps the trigger step.
- **Setup opens no priority, but is observable.** "Invisible and non-interactive" is dropped: mulligans are decisions on what a player sees. What matters is that no one can act or respond, including to Setup's own triggers — the user's one firm requirement.
- **Procedure** (`setup.md`): S1 Island shape · S2 deal terrain face up, not yet on the Island (home ground for each player who hasn't kept; neutral ground on the first pass only) · S3 terrain mulligan, home ground only, back to S2 while anyone takes one · S4 all terrain enters · S5 each Champion enters on its home tile, bonds it, and its AP becomes max(current, 4) · S6 shuffle and draw an opening hand for each player who hasn't kept · S7 hand mulligan, back to S6 while anyone takes one · S8 Map-placed Neutral permanents enter · S9 trigger step: every trigger from S1–S8 enters Pending as one batch in the automatic order (the per-instruction ordering rule is extended to Setup as one batch) and resolves without priority. Then Champion A's Action phase, with the first priority window.
- **Order, user's choice:** the Mind after the Champions — seeing the hand before the Champion stands on the Island read as strange; mulliganing terrain before seeing the hand was accepted. Claude had proposed the Mind first, so the terrain mulligan could take the hand's colours into account. S8 after S7 may move after playtesting.
- **Dealing before entering** (S2/S4): terrain mulliganed away never entered, so it leaves no dead trigger in S9 — the user's point: in an observable Setup, fizzling triggers from terrain that isn't there are confusing. Considered first: terrain enters at S2 and its triggers fizzle on identity (D85) — no extra rule, but visible noise.
- **The home-tile bond is an instruction** ("each Champion bonds its home tile"), not the Champion's `2*AP` Bond ability — otherwise it would use Champion A's once-per-turn Bond for turn 1.
- **max(current, 4)**, not "gains 4" or "set to 4": a Champion with Haste keeps its higher entry value.
- **Mulligan costs** are required (the loops must end), possibly increasing, and settled by playtesting. Currencies named: starting mana (1), opening hand size (5), starting AP (4).
- **Haste X** (follows from "summoning sickness is 0 AP on entering"): the permanent enters with X Activation Points, capped at its max; plain Haste means its max. An entry-time keyword — granting it to a permanent already on the Island does nothing (such a card says "gains X Activation Points"); Flicker sets a Haste permanent back to X. Its name is item 39's.
- **Champion B keeps its Setup mana (1, credited at S5) through Champion A's turn 1**, and may hold AP for Instant-speed play — wanted, the same reason both Champions get 4 AP at Setup.

→ `setup.md` (rewritten procedure), `economy.md`, `champions.md`, `overview.md` §3/§10, `glossary.md`, `resources-terrain.md` Open question 3 (AP and trigger part settled; slot and creation triple still open), `PLAN.md` item 29 (closed), Track B.

---

### D113 — Both Champions falling from the same instruction is a draw; a loose Item stays unparented

**Simultaneous Champion fall → draw** (closes item 44, user's call). Falling's consequences are immediate (D112), so one instruction — a Combat with mutual retaliation (D13), "deal 2 damage to each Champion" — can fell both Champions in one step; the match is then a draw. Only one instruction can do this: once an instruction fells one Champion, the match ends immediately and nothing after it runs. The code needs a draw result (`PLAN.md` Track B).

**A loose Item is not terrain-parented** (closes item 43, user's call). The question (raised by D111's side fix): a loose Item and a Remnant now look alike — both lie on a terrain, capacity-exempt, same Slice rule — yet a Remnant belongs to whoever bonded its terrain (D110) while a loose Item has no parent (D81). The user's answer: an Item's control is never determined by its location; it comes from its carrier. Only a creature's Remnant takes control from where it lies. So D81 stands, now with that reason instead of the dropped "capacity-limited slot" contrast.

→ `interaction-stack.md` §Resolution, `champions.md`, `overview.md` §7, `ancestry.md`, `PLAN.md` items 43, 44 (closed).

---

### D112 — Falling's type-dependent consequences are immediate consequences, not triggers; this replaces state-based actions

**Adopted:** a falling permanent ceases to exist and, by type, a Remnant is created (Creature, Companion), nothing happens (Structure), or the match ends (Champion) — all in the same step, right after the instruction that caused the fall, as part of that instruction's output (D96). **Immediate consequence** names everything the rules make happen because of an instruction; there is no separate state-based check phase (MTG's state-based actions). It extends D92 ("falling is immediate"), and "every state change is an instruction" makes it catch e.g. an effect ending that leaves a creature at 0 Life. Card abilities reacting to a fall stay ordinary triggers through Pending.

**Considered — the consequences as Instant-speed triggers** (user's question): Instant speed removes the response window, but triggers are still added while an Instant is in Pending (`interaction-stack.md` Speed table), so three problems remain. (1) **Ordering:** a card's "when this falls, raise its Remnant" fires together with the Remnant-creation trigger; APNAP can put the card's trigger on top, so the raise finds no Remnant — fixing it needs "rule triggers resolve first," a state-based action in disguise. (2) **The rest of the trace runs first:** in "Fell target creature on H. Draw a card for each Remnant on H," the second instruction would run before the Remnant exists. (3) **Two Champions falling** would have their match-end triggers ordered by APNAP, so trigger order would pick the winner. Rejected; the user agreed. Who wins when both Champions fall at once is still open: `PLAN.md` Track A item 44.

→ `interaction-stack.md` §Resolution, `glossary.md`, `PLAN.md` item 44.

---

### D111 — Ruin dropped as a type; a fallen Structure leaves nothing; only Creature and Companion leave a Remnant

**Why Ruins were dropped** (user, reopening item 27 a day after D110): Remnants will matter often, since creatures fall all the time and zombify-style cards, Remnant-consuming abilities and Remnant control by terrain are all natural designs. Ruins were the niche half: the only use D88 ever named was "a Structure may be cheaper built on a Ruin" (a cost reduction, outside the prototype scope), and the only prototype card found in the D110 discussion, **Watchtower** ("can only target a terrain you control with a Ruin"), existed only because Ruins existed — no card was found that wanted a Ruin for its own sake. Meanwhile Ruins cost rules: a type of their own, a slot rule (D40: a Ruin occupies its slot, building onto it replaces it, an effect can clear it), and raise needing a Structure case. A type that every falling Structure produces by default, with no reason to stay around, also breaks the pessimistic-default rule (D14): power comes from cards opting in.

**Adopted:**
- **A fallen Structure leaves nothing;** its slot is free again. Ruin is no longer a type, and a Structure slot only ever holds a Structure.
- **A ruin, if a card wants one, is a Structure.** A Structure card can say "When this falls, create Ruined Tower on its slot," where Ruined Tower is an ordinary Structure card with its own abilities — its own reason to stay around (the user's proposal; a double-faced card is one way to print the pair). No rule supports this beyond the fall trigger and normal creation.
- **Only Creature and Companion leave a Remnant.** A falling Champion ends the match (D9) and leaves nothing.
- **Fell / destroy stay for everything with Life, Structures included.** The user first suggested the vocabulary was needed only for creature-types; Claude pushed back and the user agreed: the Remnant is creature-only, but the fell/destroy split is about **fall triggers**, which Structures need too — Ruined Tower relies on "when this falls," and destroying the tower (no fall trigger) is meaningfully different from felling it (leaves the ruined Structure). Without the vocabulary, a Structure at 0 Life would need its own word. "Fell a tower" reads fine.
- **Raise applies to Remnants only.** Rebuilding a Structure is a card (e.g. a Ruined Tower ability), not a rule.
- **D40's slot rule** reduces to: two Structure slots per hex (Ground, Root). The D110 scope extension (card statics restricting the card's own location) stays: the Slice filter still needs it. Watchtower is withdrawn.

**Remnant kept as the working name.** With Ruins gone, its one weakness ("a Ruin is arguably also a remnant") disappears. **Fallen** as a noun got stronger (no Ruin to confuse it with; fell / falls / a Fallen / raise is a full set), but still loses: a Structure still falls without leaving a Fallen, and the noun clashes with the adjective in card text ("each fallen creature" vs "each Fallen"). **Body** was considered newly (creature-only now): plain and universal, but flavourless and odd for elementals and spirits. Final naming stays in item 39.

**Side fix:** `ancestry.md` justified a loose Item having no terrain parent by contrast with "Ruin/Grave/Structure, which occupy a capacity-limited slot" — never true of Graves, which occupy no slot. Now stated per type: Structure occupies a slot; a Remnant is terrain-parented so it belongs to whoever bonded its terrain (D110). Whether a loose Item should be terrain-parented the same way is open (`PLAN.md` Track A item 43).

→ `object-properties.md` §5, `glossary.md`, `ancestry.md`, `structures-items.md`, `overview.md`, `economy.md`, `neutral-permanents.md`, `interaction-stack.md`, `effect-form.md`, `resources-terrain.md`, `cards/card-ideas.md`, `history/playtest-variants.md`, `PLAN.md` items 39, 43.

---

### D110 — Remnant and Ruin are new objects; fell / destroy / raise; no "sacrifice"; card location restrictions are card statics *(Ruin dropped by D111)*

**Reverses D88's status model.** A permanent that **falls** ceases to exist, and a new object is created on its terrain: a **Remnant** (Creature, Companion) or a **Ruin** (Structure). The user's motivation (item 27): the game shouldn't distinguish death from Flicker and Bounce. The Ancestry notices that a node ceased to exist and updates its subtree, so every cascade (D107) is now worded "ceases to exist", with death no longer a special case. Also bought: "target creature" excludes a Remnant for free (a different type); raising one reuses the creation rules instead of a "reverse the status" rule; `source` joins up through the fallen permanent (D97). **Cost accepted:** a trace that fells its target can't refer to it afterwards (D85's identity check), exactly as after a Flicker. Cards that act where something fell target the hex: "Fell target creature T on target hex H. Create a Fire Warrior on H." The user judged this no real cost. Rejected alternatives: last-known information (a new mechanism, and it would change Flicker too) and keeping the status model — `history/playtest-variants.md`.

**Removal vocabulary** (closes the removal-words part of item 39):
- **fell** / **falls**: the permanent ceases to exist and leaves a Remnant or Ruin. Applies to Creature, Companion, Structure (an Item has no Life and can't fall). Reaching 0 Life is falling. Replaces "dies"/"kill"/"destroy" in that sense. Chosen as the one common verb that fits both a creature and a building.
- **destroy** / **is destroyed**: the object ceases to exist and leaves nothing; no fall triggers. Applies to every object (permanent, Remnant, Ruin, trace, card). Strictly stronger than fell on a permanent, so priced higher. Accepted risk: MTG players expect "destroy" to leave a graveyard card.
- **raise**: a Remnant or Ruin ceases to exist, and a permanent of its card is created at its location — the opposite of fell. Rejected: "return … from grave" (the target is a Remnant, not a creature).
- **No "sacrifice".** "Fell a creature you control" / "destroy a creature you control" says the same; "sacrifice" would cover only one of the two verbs and need a twin. What it buys in MTG (bypassing indestructible) is decided when such a keyword is designed — e.g. "can't be felled except to pay a cost" as data on that card.
- Rejected for fell: kill (wrong on a Structure), defeat (collides with losing the match), slay (creature-only), break/raze (Structure-only). For destroy: erase, unmake, banish.

**Remnant** is a working name for what was "Grave"; finalized in item 39 (alternatives considered are listed there).

**Remnant and Ruin properties** (a transition within the Matter domain):
- **Name copies** from the fallen permanent — the reference to its card definition (D99), which is what raise reads. Nothing else of the permanent survives: no stats, Activation Points (D88), abilities, Behavior, counters. An Object (D103); no Life, so it can't be attacked.
- **ID, timestamp fresh; `source`** = the fallen permanent, joined up to that permanent's own `source` once it ceases to exist (D97).
- **Location:** the fallen permanent's terrain. A Remnant's Slice follows the loose-Item rule (D105): Root stays Root, Ground and Sky become Ground — so a Root Remnant is hidden like a Root creature, and killing a hidden creature reveals nothing new. Claude first preferred keeping the Sky Slice, the user preferred no Slice; this reuse of an existing rule settled it. A Remnant is capacity-exempt (D40). A Ruin stays in its Structure slot and occupies it (D40).
- **Parent, controller, payer derived** from the terrain (D81, D101): a Remnant belongs to whoever bonded the terrain, so a creature that falls on your terrain leaves *your* Remnant. Confirmed by the user, no change from before.

**Raise** = Remnant/Ruin → Permanent, by the creation rules (D106/D107): everything from the card definition copies; current stats fresh at max, current-AP fresh at 0 (D89); ID, timestamp fresh, `source` = the Remnant/Ruin; parent and Behavior copy from the raising Trace unless the instruction states a triple. Terrain copies from the Remnant/Ruin; the Slice is the Remnant's if the card's Slice filter allows it, otherwise the filter's (as Flicker, D105). An illegal location (full Slice, enemy present) is an ordinary illegal location, as for Flicker (D107). Prototype card: **Raise the Fallen** (Spell): Raise target Remnant you control.

**Card location restrictions are card statics — prototype scope extended (D104, D108).** "This card can only target a terrain you control with a Ruin" is a static ability of the *card*, not of the permanent it becomes (the user's call, by analogy to cycling). D32's Slice filter is the same kind of restriction, now named as one; Flying grants a card static (enter in Sky) alongside its permanent static. These are in scope — the one kind of card ability functioning off the Island. Prototype card: **Watchtower** (Structure): can only target a Structure slot holding a Ruin you control; casting it there replaces the Ruin (D40). Claude's first try, "destroy a Ruin on this card's hex" as a cost, fails: the cost is paid before the target exists (D46).

**Un-summon** (D16) is a route to ceasing to exist without falling: no Remnant, no fall triggers.

→ `object-properties.md` §5, `glossary.md`, `ancestry.md`, `interaction-stack.md`, `effect-form.md`, `structures-items.md`, `companions.md`, `overview.md`, `economy.md`, `asymmetric-information.md`, `neutral-permanents.md`, `examples/cinder-verdict.md`, `history/playtest-variants.md`, `PLAN.md` items 27 (closed), 39.

---

### D109 — Trace groups; Duration 5 for every trace but physical ones; Ability traces have no name

**Trace groups**, by what creates the trace: **Trace** (all: ID, timestamp, `source` fresh; parent and Behavior copied, D106; cost narrowed to what was paid; Duration; physical; derived controller, payer, Elements) → **Permanent trace** (+ everything on the permanent card + its target, which becomes the location), **Spell trace** (+ name, subtypes, Speed, rules text with bound targets), **Ability trace** (+ the ability's rules text with bound targets).

**Duration is 5 for every trace by default, 0 for a physical one** (the user's simplification). A Spell card or ability may print `Duration X` (D50); a permanent card has no Duration slot (D108), so a Permanent trace always has 5. Physical and Duration are copied from the ability for an Ability trace.

**An Ability trace's name is none.** Giving it the permanent's name would make it look like a card: a name is the reference to a card definition (D99). The user would like every trace to be able to become a card (Remand, "return target trace to your hand"); Permanent and Spell traces already can (D74). For an Ability trace this needs a definition identity per ability (which would then be the trace's name), self-references that survive leaving the permanent, and a cost for triggered abilities — `PLAN.md` Track D item 42. "None" keeps the property in place, so that extension only fills in a value.

**Item 4 closed.** Every type's property list and transition rules are in `object-properties.md` §5 (D97–D109); the creation triple's name joins item 39.

→ `object-properties.md` §5, `PLAN.md` items 39, 42.

---

### D108 — Card groups mirror the permanent groups; card abilities function only on the Island (prototype scope)

**Card groups.** Card (all: name, subtypes, cost, Speed, cast condition, ID, timestamp, `source`; derived parent and Elements) → **Permanent card** (+ static, triggered and activated abilities) → **Actor card** (+ max-AP, max-Life). Item card = Permanent card; Structure card = Actor card; Creature card = Actor card + max-Attack; Companion card = Creature card. **Spell card** = Card + rules text + Duration (D50). Read off the transition walks (D105–D107): a card holds exactly what its permanent copies, nothing the Island makes fresh. Activated abilities sit on Permanent card rather than Actor card, at the user's call: Item has them too, and no card-level type lacks them in a way that matters (Terrain, which has none, D87, never is a Card). **Timestamp** added to the Card list — the general rule already said every game object, and the walks treat it as fresh everywhere.

**Spell under the prototype scope** needs nothing more: its rules text is targets plus a sequence of instructions (D104). Item 33 keeps only what lies beyond the scope.

**Scope addition (D104):** a card's abilities function only on the Island (an activated one through its Trace). Abilities working from Hand, Library or Discard ("as long as this is in your hand…", cycling, channel) are out of scope — folded into Track D item 11 as the mirror of effects reaching into the Mind.

→ `object-properties.md` §5, `effect-form.md` §Prototype scope, `PLAN.md` item 11.

---

### D107 — Companion and Structure transition rules; leaving the Island cascades like death; Structures never fight *(replaced parts in `decisions-archive.md`)*

**Companion** = Creature's rules (D106) plus: mana pool fresh and empty, dropped going backward; bonded terrains derived from the terrains' bond records, so a new Companion — including a flickered one — starts with none; Bond is a default ability written on the card and copies.

**Leaving the Island cascades like death.** Every Ancestry cascade was worded "dies" (Companion: terrains unbond, funded creatures and Traces get a Behavior per D82; Creature: carried Items drop). Flicker and Bounce aren't deaths but the permanent ceases to exist (D97), leaving bond records and parent links pointing at nothing. "Dies" in these cascades now means dies or leaves the Island by any route. A dead creature's Grave is still the same object (D88), so death itself is unchanged.

**Structure** = Creature's rules without Attack: location converts as for Item (a flickered Structure returns to its own terrain's slot; a slot filled meanwhile is an ordinary illegal target), parent derived from its terrain, Behavior copies from the creating Trace. No cascade, since nothing has a Structure as parent.

→ `object-properties.md` §5, `ancestry.md`, `companions.md`.

---

### D106 — Creature transition rules; a created permanent copies parent and Behavior from its Trace unless the instruction states a triple

**Creature.** Name, subtypes, cost, Speed, max-tier stats and all abilities copy — the default abilities (Move, Attack, Defend, Equip, Un-equip) are written on the card, hidden from players; "this creature can't attack" is how a card without Attack reads. Current-Attack and current-Life are fresh at max, current-AP fresh at 0 (D89); all three drop going backward. Location as for Item (D105). ID, timestamp, `source` fresh; controller, payer, Elements, position derive.

**Parent.** A cast card is always paid from its Champion's pool (D20; a Companion's pool funds only its own subtree), so the payer of a cast is the Champion who held the card — no counterexample found. Parent is derived on the Card (the zone's Champion, D81), copied into the Trace, then into the Permanent and on Flicker; Bounce derives it again from the destination Hand.

**Creation triple placed (item 4 (c), D83).** A permanent copies parent and Behavior assignment from the Trace that creates it — a cast creature gets the Champion and no Behavior; a permanent created by a Neutral source's ability gets no parent and the source's Behavior, which the Trace copied at activation (D82). So a **Trace carries a Behavior assignment**; D83's decider already covered every object, the property just wasn't listed. Flicker copies it there and back (a flickered Neutral creature stays Neutral, same turn); Bounce drops it. **A creation instruction may state a triple as an argument, and it wins:** a stated Champion is the parent directly (the opponent paid nothing, so the payer can't be it); "Neutral" means no parent plus the stated Behavior and neutral turn, missing either makes the card invalid. Claude first put stated triples outside the prototype scope; withdrawn — the triple is an ordinary argument of a `verb arg*` instruction, so D104 includes it. The triple's name stays open.

→ `object-properties.md` §5, Open question 6, `ancestry.md`, `glossary.md`.

---

### D105 — Transition rules per property; Item walked through; position; loose Items never in Sky

**Transition rules.** The property inventory records, per property, how a created object gets its value from its predecessor: **copy**, **narrow** (open choices resolved — the CNF cost loses its unpaid disjuncts at Trace creation), **fresh** (ID, timestamp, `source`), **convert** (target ↔ location), **drop** (no slot on the new object), **derive** (a query). A table of which tier has which property says what exists where but not how one object becomes the next; the user asked for the process. Being tested type by type, starting with Item.

**Item.** Name, subtypes, abilities, cost and Speed copy (under the prototype scope, D104, narrowing has nothing to do); ID, timestamp, `source` fresh; location converts from the cast target and back to a Flicker target, and is dropped by Bounce; parent, controller, payer, Elements derive.

**Position.** Flickering an equipped Item needs a terrain target, but its location is its carrier. Locations already form a tree rooted at the Terrain, so every permanent gets a derived **position**: follow `location` until reaching a terrain. Ancestry was considered and rejected: it coincides with location for Items and Structures (D101) but not for a Creature, whose parent is its payer. A Flicker target takes the position as its terrain and the current Slice if the card's Slice filter (D32, default Ground) allows it, otherwise the filter's Slice — an equipped Item returns on its carrier's terrain, in Ground.

**Loose Items are never in Sky.** Un-equip and carrier death (D27, D30) drop the Item on the carrier's terrain, in Root if the carrier is in Root, in Ground otherwise — replacing "at the carrier's current (terrain, Slice)". For Equip, Ground and Sky count as one Slice, so a flyer can equip an Item lying in Ground; Sky creatures never descend. Matches the Structure slots, where Ground stands in for Sky (D40).

→ `object-properties.md` §4, §5, `structures-items.md`, `glossary.md`.

---

### D104 — Prototype scope: restricted card shapes until the game runs; out-of-scope rules go to Track D

**Process rule.** Until there is a running game, rules are built for a restricted set of card shapes; extensions come after playing. Three conditions: the restricted set is a **subset** of the full model, so extending it only adds (every legal card stays legal); the restriction applies to **card shapes, not engine structure** (domains, transitions, property tiers, the casting procedure stay fully designed — pillar 5 lives there); **nothing decided is deleted** — rules outside the scope stay documented, marked as such. Corner cases no in-scope card needs are parked in `PLAN.md` Track D, not discussed.

**The scope** (`effect-form.md` §Prototype scope): rules text is targets plus a sequence of instructions, run in card order; no modes, conditions, outputs, repeatable picks, resolution-time choices or choosers other than you; a permanent card targets only its location; abilities follow the same shape; cost is a fixed list (no `X`, branches or cast condition). Several targets per card stay in. Modes were dropped too: if they are easy to add, they are easy later; if not, the prototype avoided them.

**Out of scope but still decided:** D72 (modal permanents), D95's resolution-time choices, conditions and block scope, D94/D96 outputs and menu order, D91's cost branches and cast condition, D83's relations, D70's `X`. **Unaffected engine procedure:** pay → choose → target with a CNF cost (D70/D91), the casting transaction (D94), resolution checks (D85, item 24), the redirect loop (D68, item 30), resolution order (D92).

**First consequence:** choices on permanent cards and "as this enters, choose…" abilities (MTG's linked abilities) are out of scope; considered for later: make the choice at cast as an ordinary option, locked like a mode (Track D item 40).

→ `effect-form.md`, `PLAN.md` §8 Track D.

---

### D103 — Structure is an Actor; Permanent and Actor are the property groups; properties are never added

**Structure is an Actor.** D44 defined Actors as the types holding their own Activation Points; Structure has held them since D58/D88, so the classification just never caught up. Actors: Champion, Companion, Creature, Structure. Objects (Item, Terrain, Grave, Ruin) remain a label only. "Only permanents holding AP can carry a Behavior" (D100) is now simply "only Actors can".

**Two property groups** for the inventory: **Permanent** (name, subtypes, static and triggered abilities, ID and timestamp, location, parent, `source`, cost, Speed, cast condition; derived controller, payer, Elements) and **Actor** (Permanent plus activated abilities, Activation Points, Life, Behavior). Per type: Terrain = Permanent + its own block; Item = Permanent + activated abilities; Structure = Actor; Creature = Actor + Attack; Companion and Champion = Creature + mana pool — their property lists are identical, everything separating them lies in values and abilities. Considered and dropped: **Object** (its two property-bearing members share almost nothing) and **castable permanent** (Creature, Companion, Structure, Item — a useful word for card text, item 39, but no property group once "none" counts as a value).

**"None" is an ordinary value; properties are never added or removed during play.** A Champion's parent, a Map-placed permanent's `source`, Terrain's cost are simply none. A type lacks a property only where any value would change the rules (Terrain with Activation Points could act). Consequently a Structure has **no Attack** — Attack on Structure (default 0) was considered and declined; whether a Structure can ever fight is left open (`object-properties.md` Open question 7).

**Champion stats are a Creature's** — Attack, Life, Activation Points — closing the gap that `champions.md` never printed a damage value. A Champion's card definition is the **customised card from the player's loadout**, not the generic printed card, since progression changes it between matches; the loadout (Champion, Map, deck, terrain deck, a future sideboard) sits outside every zone, like the Map (`PLAN.md` Track A item 12). Hand, Library and Discard are zones the Champion owns, not its properties.

→ `object-properties.md` §5, `glossary.md`, `overview.md` §1, `structures-items.md`, `economy.md`, `ancestry.md`, `champions.md`.

---

### D102 — Reveal is a keyword; the surfacing rule is withdrawn

**Reveal** sets one player's belief about an object equal to its true state, lifting any Mimic face — D18's "collapse → full truth", triggered deliberately instead of by a contradicting hard fact. It is a one-time event, not a lasting state: an object that is hidden again later (e.g. a Root-Slice creature moving out of the observer's D67 vision) is not tracked.

**Withdrawn: "attacking from the Root Slice surfaces a creature" (D19, hardened to "permanently" by D80).** D19 had recorded it as an interpretation of a user note, marked for confirmation; D80 resolved that hedge by picking the stricter reading instead of confirming the rule itself. The user doesn't recognise the rule. Root-Slice visibility is D67 alone (an observer sees a hex's Root occupants while one of their permanents is in the Root Slice on that hex or a neighbour). A concealment keyword that is lost when an opponent's creature enters the same location and Slice is Track C content, not a base rule. Track C item 6 (re-concealment ability) is removed. D80's other half (Root cannot attack Ground) stands.

→ `asymmetric-information.md`, `overview.md` §4, `history/pessimistic-default-audit.md`.

---

### D101 — One fact, one home: occupants store their location; parents are read off it where possible

A permanent stores its own **location**; a terrain's occupants and the contents of its Structure slots are derived queries (an engine may keep a reverse index as a cache). Location is (terrain, Slice) for Creature and Structure alike — a Structure's Slice is Ground or Root, never Sky — and (terrain, Slice) or a carrying Actor for an Item. The position of a creature among a Slice's three places is display-only. Considered: storing occupants on the terrain, which would give the place within the Slice for free — rejected, the rules never ask for it, and if they ever do it becomes a third component of the location.

Consequence for the Ancestry: an edge that duplicates the location is read off it, not stored. Structure → terrain is the terrain in its location; Item → carrier is its location when that is an Actor (a loose Item has no parent). Stored edges remain Terrain → bonder (the bond record) and Creature/Companion/Trace → payer. No behaviour changes; it decides which properties the per-type inventory (`object-properties.md` §5) lists.

→ `object-properties.md` §5, `ancestry.md`.

---

### D100 — Terrain: exactly one per hex, location targets name a terrain, Type is flavour, Element is derived *(replaced parts in `decisions-archive.md`)*

**Every hex holds exactly one Terrain, always.** Terrain can't be destroyed; a future "destroy target terrain" would replace it with a void terrain, keeping the invariant. So a hex is only the terrain's position in the Map's Layout (adjacency, distance), not an object of its own, and card text targets **"target terrain"**, never "target hex". The terrain stores its grid position; nothing in the base rules changes it.

**Terrain Type is flavour only.** Copied from the Map at Setup, it names the terrain (Forest, Plain…) and nothing can query it — not a rules property.

**Element is derived**, as MTG derives colour identity: the set (0–8) of Elements in the mana the terrain produces and in its rules text. Mana production is an expression (amount; coloured, hybrid, generic…), not "1 of its Element".

**Bond and equip surcharges are static abilities** ("bonding this costs an additional `4!AP`"), part of the action's cost and paid before its trace exists (D70) — not triggered costs, which would fire after the bond/equip had already happened.

→ `object-properties.md` §5, `map.md`, `glossary.md`, `resources-terrain.md`, `structures-items.md`, `neutral-permanents.md`, `ancestry.md`, `interaction-stack.md`, `effect-form.md`, `examples/cinder-verdict.md`.

---

### D99 — Name is immutable and is the link to the card definition; subtypes on every type; copy restriction is deckbuilding-only

**Name is the one immutable property** of every game object, and **names are unique across card definitions** (reprints share one). The name therefore is the reference to the printed card definition — no separate field. The definition is outside the game and can't be removed, which `source` (D97) can't guarantee: a card can leave Discard, and an ability-created permanent has no card object at all (D88's "real card" is a definition). The definition supplies printed originals ("return to printed stats", "becomes a copy of") and same-card identity (uniqueness, item 1; "a trace with the same name"). Name-changing effects don't exist; if wanted, they come with a separate current-name property. Considered: deriving immutable values through `source` — rejected, it breaks on the two cases above and on Markovian creation (§3 of `object-properties.md`).

All other properties are copied along the chain (Speed and cast condition stay on a permanent though unused there, so a Bounce returns what the chain carried — same accepted "banking" as raised max-stats).

**Subtypes** are a property of every type with a card (including Terrain), and mutable. **Copy restriction** is irrelevant once the game starts: it lives on the card definition only, never on a game object.

→ `object-properties.md` §5, `glossary.md`.

---

### D98 — History is a track of facts with a one-round window; physical is a Trace property *(replaced parts in `decisions-archive.md`)*

**Aether holds objects, history holds facts.** Card text asking about the recent past ("did a creature attack this turn?") reads a **history track**: the sequence of instruction outputs (D96 state diffs), each with its timestamp. Nobody can select or target an entry. Considered: answering history from traces — rejected, it would fill the Aether with every Move and dilute its role as the visible record of magic; keeping Physical traces in Past longer — rejected for the same reason, and a card can already print a Duration on a physical ability.

**Physical / non-physical is a Trace property**, not a Duration value of 0 — so "non-physical trace" stays meaningful when a card changes a physical ability's Duration.

→ `object-properties.md`, `glossary.md`.

---

### D97 — Objects that leave every zone cease to exist; `source` is a backward link, joined up on removal *(replaced parts in `decisions-archive.md`)*

**No exile zone and no record state.** A trace fading from Past, a card removed from Discard, any object leaving every zone simply ceases to exist; nothing can bring it back. A digital game doesn't need MTG's exile as a place to put things. The engine may still keep a log (replays, determinism) — the rules never read it. Considered (kept in `history/playtest-variants.md`): the object stays as a record that only stored links can reach. **Adopted to try, explicitly revertible**: B is A with the links dropped, so switching only changes what the rules may read.

**`source`** (D75's open provenance question) is a link from every object to its **immediate predecessor** — permanent → trace → card, or → the activating permanent. Only backward links are stored; "what this trace created" is a query over `source`. **When an object ceases to exist, links through it are joined up**: whatever had it as `source` takes over its `source`. So Mind → Matter stays reachable (a card and the permanent it produced stay linked while both exist), while Aether → Matter lasts only while the trace does — permanents get harder to reach through their trace as they age, making Duration a design lever. Card text wanting "the card that created it" follows `source` until it reaches a card. A link to an object that no longer exists resolves to nothing (D85's identity check). Following a link into a hidden zone (an opponent's Discard) is a separate question; an opponent's cards are hidden, so targeting them is expected to cost a reveal.

→ `object-properties.md` §2, §5, `glossary.md`, `history/playtest-variants.md`.

---

### D96 — An output is its instruction's state diff; repeated picks run in menu order

**Output = state diff.** An instruction's output is everything it changed (the state after minus the state before), queried by conditions and arguments ("how many creatures died"); the deaths it causes are part of it, since they happen immediately after it (D92). Replaces D94/D95's "a named result, such as the objects it killed." Sums across repeated picks need no aggregate: "draw a card for each creature killed" repeated in each option gives the total. Thresholds across picks don't work that way — parked with the related gaps in `PLAN.md` item 33.

**Repeated picks run in menu order**, repeats back to back (as MTG's 700.2d), not in the order the player picked them — deterministic, independent of input order.

→ `effect-form.md`.

---

### D95 — Effect normal form: one "choose" primitive, block scope, three-state outputs, conditions on instructions

Settles the internal form of rules text (item 4's "rules text" property), kept compact for validators and AI; card text may use sugar. Grammar and details: `effect-form.md`.

**One primitive.** A mandatory chooser (default "you") chooses a `min..max` count among options (an effect) or objects (a selection), at cast or at resolution. A target is exactly an object selection at cast; "you may X" is `choose 0..1 at resolution { X }`; "a creature of your choice" is an object selection at resolution — so no separate non-target declaration is needed. Considered: an optional chooser field — rejected, mandatory-with-default is simpler. Kind ("creature," "hex") is an ordinary constraint, not a separate field (card text may present it differently).

**Conditions sit on the instruction only.** Considered: conditions on the effect, or in both places — two places add no expressiveness; the instruction is where just-in-time checks happen anyway. Accepted gap, later reopened as `PLAN.md` item 33: a condition can't gate a choice.

**Block scope.** A name is visible in the block where it's declared (card or option), only to what follows. Card level: variables (`X`, branch labels) and card-level selections, one instance each — needed for a target shared across effects (Cinder Verdict's T1). Option level: option selections and all outputs, instanced per pick. Considered: card-wide outputs — rejected, ambiguous when an option is picked twice; the user also floated option-only scope for everything, dropped because it can't express a target shared across effects without nesting. Cost: a fact needed in several options is repeated; aggregates across picks aren't expressible.

**Outputs have three states** — unbound (didn't run), empty (ran or fizzled, affected nothing), a value; an instruction reading an unbound output doesn't run and its own output is unbound. Refines D94's "an output of an instruction that didn't run is empty," which made "if it did not die this way" true when the damage never happened. Considered: two states with a static rule that a reader repeats its producer's condition — rejected by the user as worse than three states.

**Track C rule:** every card designed in Track C is written out in this form; a card that doesn't fit is reworded or surfaces a Track A item.

→ `effect-form.md`, `interaction-stack.md`, `PLAN.md` Track C, `docs/cards/card-ideas.md`.

---

### D94 — Casting is a transaction: abort refunds everything; the card pays at the end of Pay; instructions bind outputs *(replaced parts in `decisions-archive.md`)*

**The card is part of the cost** (D37, D68: "mana and the card alike"). It leaves the Hand as payment starts but arrives in Discard only when payment ends, so it can't pay for itself (an "exile a card from your discard" cost can't use the card being cast). Replaces D37/D68's unspecified "cast-commitment point."

**Casting is a transaction; abort rolls it back.** Nothing done in steps 0–3 is visible to the opponent before the trace enters Pending, and triggers it fires (a sacrifice cost's "whenever a creature is sacrificed") are collected and enter Pending together with the trace, on top of it. An abort returns every paid cost and drops the collected triggers — the state is exactly as before step 0. Replaces the previous reading that an abort, like D68's cancel, forfeits what was paid. Rationale: nothing has happened yet from the opponent's point of view, so a refund can't be exploited; digital clients (e.g. MTG Arena) behave the same way, and in practice most casts take seconds. **Exception, interim:** once a D68 redirect attempt has failed, the cost is committed — the failure revealed hidden information, and a refunding abort after it would be the free probe D68 rejected. The user wants to rediscuss the redirect loop itself (`PLAN.md` Track A item 30); this exception is to be resettled there.

→ `interaction-stack.md`.

---

### D93 — Every game object has an ID; IDs follow text order within one creation event

D61 gave IDs to permanents only. Trigger ordering (D92) needs a source ID for triggers from any object — a Trace, a Card ("when discarded"), a delayed trigger — so **every game object (Card, Trace, Permanent) gets an ID** from the same monotonic counter. Considered: a second ordering rule for non-permanent sources — rejected, one counter covers it. **Within one creation event, IDs follow the order in the text**: "create a Soldier and a Warrior" gives the Soldier the lower ID. "Create two Soldiers" assigns IDs in enumeration order, which is invisible to the player — accepted as harmless. A control change keeps the ID (D61, unchanged), so a stolen creature stays "old" among its new controller's triggers.

→ `neutral-permanents.md`, `object-properties.md`.

---

### D92 — Trace resolution: card order, just-in-time checks, immediate death, collected triggers; Instant blocks active play instead of resolving atomically

Surfaced in item 4's discussion; MTG's resolution rules (CR 608, 603.3, 704) were read as the reference point and mostly not adopted.

**Card order, checked just-in-time.** Instructions run in card order; each is checked (D33/D85) immediately before it runs, against the current state including what earlier instructions did. Considered: MTG's up-front target check (608.2b) — rejected. It exists in MTG only to feed the whole-spell fizzle ("all targets illegal → nothing happens"), which D33 already rejected; without it an up-front check has no consequence and would only act on stale state. Resolution-time choices are made when their instruction is reached.

**Death is immediate.** A permanent whose current Life is 0 or less dies immediately after the instruction that caused it — "destroy" and "0 Life" behave identically, and the next instruction already sees it dead. Considered: MTG's state-based actions, deferred until the spell finishes (704.3) — rejected as the source of "if it dies this way" needing workarounds. Per-damage-event granularity rejected: it would break simultaneous damage (D13 combat, "deal 1 damage to each creature"). Accepted consequence: text order matters — "deal 3 damage to T1; T1 gets +3 max-Life" kills a 3-Life T1 and the second instruction fizzles.

**Every state change is an instruction.** Cost payment and phase-boundary events (an "until end of turn" buff ending) are instructions too, so "immediately after the instruction" needs no list of exceptions (pillar 5). An instruction does not require a trace: a trace is one way instructions are executed (queued, respondable); the casting procedure and the turn structure execute instructions directly, without Pending, so paying a cost or a buff expiring is never respondable.

**Triggers are collected, then enter Pending.** Triggers fired while a trace resolves are collected; once the trace has finished and moved to Past, they enter Pending so that the earliest instruction's triggers are on top (resolve first). Considered: entering Pending as each trigger fires — rejected, LIFO would reverse text order. Within one instruction: APNAP over the four seats (D60) — the active seat's triggers enter first and resolve last — then by source ID (D61/D93, oldest enters first, resolves last), then by printed ability order, then by the order of events inside the instruction. Fully identical triggers are interchangeable and need no order. **Order is automatic, never chosen by a Champion** — MTG's player-ordered triggers (603.3b) rejected as a rarely relevant, annoying decision point; the UI shows the order in Pending before anyone must respond. D61's wording "oldest ID first onto the stack, so oldest resolves first" contradicted LIFO; corrected: oldest enters first, resolves last.

**The trace moves to Past as the final step of its resolution**, before its collected triggers enter Pending, so a trigger reading "traces in Past" sees it.

**Instant is not atomic; it blocks active play.** While an Instant is in Pending, nothing can be actively put into the Aether (no cast, no activation, including another Instant); `Now` advances automatically until the Instant has resolved. Triggers are still added and resolve normally — e.g. a trigger from the Instant's own sacrifice cost enters Pending above it and resolves first, with no special rule. Replaces D45's "enters Pending and `Now` is advanced past it as part of the same action."

**Compared against simpler games.** Hearthstone avoids these questions by having no player responses (Secrets are automatic) and hiding deferred death processing and order-of-play trigger resolution in the engine; Legends of Runeterra (Burst/Fast/Slow speeds, visible LIFO stack, immediate death) is the closest match. Conclusion: with responses (D45) kept, this procedure is already near the simple end; the remaining rule for automatic decisions is that they are visible before a player has to act.

→ `interaction-stack.md`, `economy.md`, `glossary.md`, `neutral-permanents.md`.

---

### D91 — Casting procedure refined: legality step, earliest-possible binding, optional and labelled cost branches, card-wide target scope *(replaced parts in `decisions-archive.md`)*

Surfaced in item 4's discussion while working out what a card's rules text is made of; checked against MTG's CR 115, 601.2b–c and 700.2.

**References bind as early as possible**, as a side effect of whichever step fixes what they depend on — D83's "bound at declaration" is kept, its timing refined. Source-relative references ("you", the payer, an inherited Behavior/neutral turn) bind right after step 0, before Pay — Pay can change the Ancestry (a "sacrifice this creature" cost removes the source). Target-relative references ("target creature's controller") bind right after step 3. Considered: binding everything at the end — rejected for the sacrificed-source case.

**Target scope.** A target declared at card level is one shared instance, referenceable by every instruction (including across modal effects); a target declared inside a modal option is instanced fresh for every pick of that option (D70's "each pick is its own instance"; MTG 700.2d). A target is required only if an instruction in a chosen option references it (MTG 700.2c) — settled before step 3, since all choices are made first. "That creature"/"it" is a reference to a target, not a new target.

**Target counts and choosers.** A target slot has a count `min..max` ("up to one target creature" allows zero); an instruction with zero bound targets does nothing (not a fizzle). A choice can name its chooser, which need not be the trace's controller (MTG 700.2e); the chooser binds like any reference.

→ `interaction-stack.md`.

---

### D90 — Subterranean grants Root access, not a Root starting Slice; a cast Subterranean creature enters on Ground by default

Corrects the D32 mapping "a burrower-type keyword → Below," which `structures-items.md` had carried as "Subterranean → Root (D42)" — D42 itself never said Subterranean creatures start in Root. **Subterranean** grants being able to exist in the Root Slice plus Ascend/Descend (D42); it does not choose the entering Slice, so a Subterranean creature enters on **Ground** like any creature without a Slice keyword. **Flying is different:** a flyer is always in the Sky unless it loses Flying, so Flying does set the entering Slice. A creature that enters directly into Root says so on its own card (the same card-printed Slice choice D32 already allows).

→ `structures-items.md`, `glossary.md`.

---

### D89 — Every permanent entering the Island enters with 0 Activation Points; replaces D57 *(Setup exception removed by D114; entry AP 2 by D160)* *(replaced parts in `decisions-archive.md`)*

**Adopted:** a permanent entering the Island — cast, created by an effect (D88), flickered, however it got there — enters with **0 Activation Points** and refreshes at its controller's next Beginning phase. That is all summoning sickness is: a permanent that wasn't there at the beginning of its controller's turn can't act yet. **Haste** removes it. D57's criterion ("did it enter via casting") is dropped — it wrongly let a created creature (e.g. D87's Fire Warrior) act immediately.

**A control change does not touch Activation Points.** A stolen creature keeps its current AP and can act against its previous controller right away. Considered and rejected: MTG-style "sick unless controlled continuously since your turn began" (or resetting AP to 0 on a control change) — an extra rule for a case that is self-limiting: a stolen creature usually stands among its former allies, now enemies, and is killed before it can act unless it can act itself; and in practice steal effects usually grant haste anyway. One rule, no exceptions, is preferred.

→ `economy.md`, `overview.md`, `structures-items.md`, `glossary.md`, `neutral-permanents.md`.

---

### D88 — Every created object has a card; there are no tokens; Grave and Ruin are a status of the dead object, not separate objects *(status model reversed by D110: Remnant is a new object; Ruin dropped by D111)* *(replaced parts in `decisions-archive.md`)*

**Adopted:** anything created in the game references a real card — card text never says "create a 2/2 creature with…", it names a card (with a link to it). Every permanent therefore carries all required properties (a real mana cost, not MTG's token "mana value 0"), and Bounce/Flicker need no special rules for created objects. D69's "a Bounce/Flicker-created object is not a token" generalizes: nothing is a token.

**Consequences, accepted on purpose:**
- **Created-object cards are normal cards.** E.g. Fire Warrior (created by Fire Barrack, D87's example) is legal in a deck like any other card. For power-level reasons it will mostly be played only via the cards that create it; no deckbuilding restriction is adopted for it.
- **Bouncing a created object creates a real, castable Card** in its Champion's Hand (D69) — unlike MTG, where a bounced token ceases to exist. Wanted, not a side effect to patch.
- **Map-placed permanents** (Neutral Structures/Items/Creatures/Companions, `PLAN.md` Track A item 7) reference cards too.

→ `object-properties.md`, `glossary.md`, `economy.md`, `ancestry.md`, `PLAN.md`.

---

### D87 — Terrain holds no Activation Points and has no activated abilities; activated power comes from a Structure the terrain brings; static and triggered terrain abilities stay

**The gap:** Terrain held its own printed Activation Points (D58) only to gate and fund its own activated abilities — the one AP-holder that can't be attacked, destroyed or cast. Tested against two prototype cards: **Fire Barrack Spot** (Terrain: produces 1 Fire; `3!AP` + 1 Fire: create a Fire Warrior) and **Fire Flat** (Terrain: produces 1 Fire; comes with a **Fire Barrack**, a Structure carrying that same ability). The first is an engine that can only be answered positionally, since no rule destroys terrain. The second gives the same engine a combat answer (Structure → Ruin, leaving a plain Fire land) and uses existing Structure machinery: parent is the terrain, mana climbs to the bonder, dormant when the terrain is Neutral (D83), self-acting under a Behavior (D84) — a natural PvE spawner.

**Adopted:**
- **Terrain holds no Activation Points and has no activated abilities** (reverses D58 for Terrain; D57's "Terrain enters with full AP" becomes moot).
- **Activated power a terrain card wants is carried by a Structure the terrain brings with it** — a real card (D88), able to use either of the hex's two Structure slots (D40; a Root Structure is hidden for free). **How it arrives at Setup is deliberately open:** `setup.md` forbids "enters the Island" for Setup-placed permanents, a technicality from Setup having no response window; a clean solution is to be found, not patched here.
- **Terrain keeps static and triggered abilities.** A triggered ability may print a cost ("whenever a creature enters this, you may pay 1 AP; if you do, deal 1 damage to it"): this falls out of Payment (D58/D74) with no new rule — Terrain holds no AP, so the walk climbs to its bonder (Champion or Companion), same as mana. A trigger with a choice needs a decider, so a dormant Neutral terrain skips it (D83). A once-per-round limit, which own-AP used to provide, is printed where wanted.
- **A dormant terrain's static abilities and choiceless triggers still apply** (D83, confirmed for Terrain): an unbonded swamp on neutral ground still slows everyone.
- **GUI requirement:** a terrain-ability badge shows only on hexes whose terrain has static/triggered abilities, so basics stay visually quiet; the badge follows the **observer's belief**, so a Mimic terrain presenting as a basic shows none. A static's effect must also be shown where it lands (movement preview, affected creature's stats). Hex-view sketch: `PLAN.md` Track B item 5.

**Rejected:** *(a) Terrain has no abilities at all beyond Element and mana*, everything else on a Structure — the most uniform option, every non-mana effect destructible, and far less for a player to monitor across ~50 hexes. Rejected because some effects are properties of the land itself (a swamp's movement cost), and moving them onto a Structure feels artificial; the monitoring argument is answered by the GUI requirement instead. The Bond surcharge (`card-ideas.md`) is unaffected — it is paid from the bonder's AP.

→ `resources-terrain.md`, `economy.md`, `ancestry.md`, `structures-items.md`, `setup.md`, `glossary.md`, `docs/cards/card-ideas.md`, `PLAN.md`.

---

### D86 — Slice controller uniformity is an entry rule, not a standing invariant; only a control change can mix a Slice; D66's fizzle removed *(replaced parts in `decisions-archive.md`)*

Resolves `PLAN.md` Track A item 23 (mixed-controller Slice after automatic control loss). **Reverses D66.**

**The gap:** D66 enforced "a Slice's occupants share one controller, no exceptions" only for control-change *effects*, by fizzling them. Automatic control changes can't fizzle, and there is an open-ended set of them: a Companion dies and its creature (now Neutral, D83/D84) shares a Slice with a creature its Champion cast directly; a temporary control effect ends while the stolen creature stands among the thief's creatures; and D66 applied to "target Companion becomes Neutral" (the whole subtree converts) would make such a card almost never resolve. Expected to be incomplete — so a general answer was needed, not per-case rulings.

**Candidates:** (1) *veto* — a creature whose change would mix its Slice instead takes the Slice's controller — rejected as exploitable (steal "until end of turn," park the creature among your own, keep it forever); (2) *repair by destruction* — every creature whose control change would mix a Slice is destroyed — total and machinery-free, but harsh and combo-prone (temporary steal + parking = a kill spell; a Companion's death destroying some of its creatures is the double punishment D82 avoided); (3) *entry-only invariant* — adopted. D66's own rejected alternatives (forced combat, a "Captured" state, forced relocation) were re-checked and still don't fit.

**Adopted: general relaxation (allowing mixed Slices outright) stays rejected; mixed Slices exist only as a special case.** Entering a Slice — Move, Summon, Relocate/push, Ascend/Descend — still requires every occupant to share the entrant's controller (D68's procedure unchanged), and the 3-slot cap stays a hard invariant. **A Slice may become mixed only through a control change** — any control change, automatic or by effect. D68's rejection of mixed occupancy is untouched: it was about entry (a creature walking into or through any non-full hex, gutting pillar 2), which stays blocked. The general relaxation was also rejected for complexity: it would make "who is being attacked" a question on every attack, one that matters almost never and confuses players when it does. The special case is accepted as a precedent for its opportunities. **D66's fizzle is removed:** control-change effects never check occupancy. Knowingly overridden: D66's pessimistic-default argument that stealing a creature out of a defended cluster shouldn't be easy — it now works, but the stolen creature stands among enemies.

**Falls out with no new rules:** nobody can enter a mixed Slice (its occupants don't share any entrant's controller), so it blocks both sides; any occupant leaves normally, and once one side is gone the Slice is uniform again; capacity is unaffected (a control change adds no occupant); an enemy on the hex blocks mana paths for both sides; D67 vision is per controller and unaffected; location-based effects ("each creature on target hex") are unaffected; Behaviors need nothing new.

`PLAN.md` Track A item 23 removed; item 25 (exact combat rules, including Slices) added; item 2's motivating example relied on D66's fizzle and is flagged there.

→ `overview.md` §2, `ancestry.md` (§Control changes and occupancy, Invariant), `PLAN.md`.

---

### D85 — What a trace checks at resolution: its remaining text plus four implicit conditions; perception is always a live view *(visibility restated in the belief model by D159)* *(replaced parts in `decisions-archive.md`)*

Resolves `PLAN.md` Track A item 22 (what "can no longer be targeted" means at resolution). **Decided for now, deliberately untested** — the list is not expected to be complete; a second pass is item 24.

**Framing:** casting instantiates the card's text into a trace (the card is unchanged): choices (X, modes) first, then every `target <type> [condition]` placeholder binds, with its condition consumed (D83). Other references bind automatically ("you," controller), and resolution-time choices bind only their chooser — kept, since effects over information revealed during resolution ("look at the top 3 cards, put one into your hand"), over an earlier instruction's outcome ("draw two, then discard a card"), or chosen by someone else (an edict) need them. So the explicit side is settled: consumed qualifiers are never rechecked ("target creature with power 3 or less at target location in your realm" → "deal 3 damage to Goblin 42 at Hex 23"; the power and realm conditions are gone), and a relation still in the text is rechecked (Goblin 42 no longer at Hex 23 → the instruction fizzles). The question is only **what text is implicitly on every trace.**

**Adopted — each bound object implicitly requires, at resolution:** (1) **identity** — the same instance still exists; a Flickered or Bounced object is gone, even if it has returned; (2) **visibility** — it is visible in the live view of the Champion bound to the trace, or, for a Neutral source, of that source permanent itself (dealing damage to a creature whose whereabouts aren't known would be strange); (3) **protection** — it doesn't have "can't be affected by [this kind of effect]"; (4) **verb applicability** — it is still something the instruction's verb can act on (a creature turned into a Structure in response may no longer take "deal 3 damage"). **"Can't be targeted" is not rechecked:** targeting is a step of casting, not of resolution, so gaining it in response doesn't affect a trace already in Pending; "can't be affected" is its resolution-time counterpart. The keyword states which step it acts on — a structural rule, not a case split. **No implicit location condition:** a creature-targeting effect still follows its target across ordinary moves; the relevant dodge (moving somewhere hidden — Descend, Mist) is already covered by visibility, and a card that wants move-dodgeability says "at target hex." Adding location implicitly would make every creature spell dodgeable.

`PLAN.md` Track A item 22 removed; item 24 appended (second look).

→ `interaction-stack.md` (§Resolution), `neutral-permanents.md` (Target selection), `glossary.md` (View), `PLAN.md`.

---

### D84 — A Behavior is never deleted, only replaced by a new assignment; a Companion made Neutral passes its Behavior and neutral turn to its creatures

Resolves `PLAN.md` Track A items 20 and 21.

**A Behavior is never deleted.** A permanent keeps its Behavior and neutral turn while controlled; the controller simply takes precedence as decider (D83), so the Behavior sits unused and decides again once the controller is gone. The former rule "gaining control of a Neutral permanent ends its Behavior immediately" is removed. **When a permanent becomes Neutral, a newly assigned Behavior replaces the old one; if nothing is assigned, the old one stays.** Behavior and neutral turn always move as a pair — a new assignment replaces both, no assignment keeps both. Three rules were compared for the Behavior a permanent has at the moment it becomes Neutral: (a) end on gaining control — the assigned one, or none (dormant) if nothing is assigned; (b) keep, a new assignment replaces — the assigned one, or the old one if nothing is assigned; (c) keep, a new assignment only fills an empty slot — the old one if any, else the assigned one. (a) and (b) differ only when nothing is assigned; (b) and (c) only when both an old and a new Behavior exist. **(b) adopted.** (a) rejected: a temporary steal ("gain control of target creature until end of turn") of a Neutral monster would leave it permanently dormant once the effect ends, since no rule assigns it a new Behavior — a cheap temporary effect becoming permanent neutralization; the same failure disarms a scenario Structure whose Bond edge is activated and then blocked again (item 21's test case: under (b) it resumes its `Produce`-style Behavior). Withdrawn as an argument against (a): that it needs an event on gaining control, contradicting D83's event-free edge activation — (a) can be applied lazily when the permanent becomes Neutral. (c) rejected: it silently ignores a Behavior a card explicitly states ("target creature becomes Neutral with `Aggressive toward A`" on a stolen monster still carrying `Aggressive toward B`). Also rejected: a hybrid — explicit card text replaces, automatic defaults only fill an empty slot — as a split by assignment source; its only motivating case (a stolen scenario monster with an authored `Patrol`, funded by the thief's Companion, taking the Companion-death default) reads acceptably under (b), since the monster had joined that side. Consequences accepted: a stolen-then-lost monster returns to its old Behavior, possibly `Aggressive toward` the Champion who just held it; a "tame" card wanting permanent effect must state "it loses its Behavior." Presentation cost: a controlled permanent's unused Behavior must be visible.

**A Companion made Neutral (D55) passes its Behavior instance and neutral turn to the creatures in its subtree** — the same instance, with X as already bound at declaration (D83), replacing any Behavior they carried (above). One Behavior per conversion, not two: a card naming separate Behaviors for the Companion and for its creatures was rejected because a card making a single creature Neutral would read the same way with one name, so every conversion effect names exactly one. A card wanting something else for the creatures states it explicitly. A Behavior keyword that needs to act differently as a Companion than as a creature does so inside its own definition (a mode's trigger condition may test the permanent's own type) — no separate Companion/creature variant rule. Accepted cost: Companion and creature policies can't be freely combined per conversion without explicit card text.

**Refines D83's item-19 consequence:** a Structure/Ruin/Grave/Terrain losing control through the Ancestry still never *receives* a Behavior, but one already carrying a Behavior (e.g. scenario-placed) is decided by it again rather than dormant.

`PLAN.md` Track A items 20 and 21 removed.

→ `neutral-permanents.md` (neutral turns, Modes, §Becoming Neutral), `ancestry.md` (active edges, Structure case, Neutral objects), `glossary.md` (Behavior, Dormant), `PLAN.md`.

---

### D83 — Ancestry is a present-tense payment tree; control is a query on it, Behavior an object property; blocked bonds are inactive edges; every trace reference binds at declaration *(replaced parts in `decisions-archive.md`)*

Resolves `PLAN.md` Track A item 18 (what can Ancestry answer, and what can't it) and item 19 (Structure/Ruin/Grave losing control via terrain).

**Ancestry answers present-tense questions only:** who pays now (Payment walk) and which Champion, if any, is at the root now (Controller query). It never answers anything about the past. **D81's claim that a Trace's flattened parent is D75's `source` back-reference is reversed as an error** — not only in the Neutral case D82 exposed, but in the controlled case too: a creature-activated Trace's parent is the creature's Companion, so a card like "destroy the permanent whose ability created target Trace" would hit the Companion. Flattening is correct for payment/control and exactly what destroys provenance. Provenance ("what produced this") is a separate property, fixed at creation, and belongs to item 4's per-type inventory.

**"Neutral" had been conflating two states: autonomous (a decider of its own) and dormant (no decider at all).** Adopted: every object's **decider** is its controller if the Controller query returns a Champion; otherwise its **Behavior** (a per-object property, including its neutral turn) if it has one; otherwise **nobody — the object is dormant**. A dormant object makes no choices: its activated abilities can't be used and triggered abilities requiring a choice (a target, a "may") don't happen, while mandatory choiceless triggers still fire (e.g. a Structure's "when a creature enters a terrain within distance 2, deal 2 damage to that creature" still hits whoever enters). Ancestry never decides between autonomous and dormant; a Behavior is only ever attached by a card, a scenario, or a rule. Behavior is per node, not per root. **Considered and reverted within the same discussion:** making control a per-object stored property instead of a tree query — rejected because Terrain's and Structure's control must be derived live (D54's path check), so a derived control everywhere is the uniform choice.

**Why a cut-off Structure goes dormant and an orphaned creature gets a Behavior:** a Structure/Terrain/Item has a basic action that restores control (re-bond, unblock, Equip); an orphaned creature has none and would otherwise be stranded forever. This is the design reason for which rules attach Behavior, not a rule itself. Consequence for item 19: Structure/Ruin/Grave losing control via its terrain never receives a Behavior — it is dormant.

**A blocked Bond is an inactive edge, not a removed node.** Every Ancestry edge carries an active condition, "always" for every edge but the Bond edge (Terrain → bonder), whose condition is an enemy-free path to the bonder. Payment and Controller only traverse active edges. Activation/deactivation is not an event: no rule fires, the Structure simply goes dormant and returns silently. Removing the node was rejected because it destroys the bond record D54 needs for automatic restoration.

**Ancestry changes update their subtree atomically.** Removing or re-parenting a node includes updating its subtree while the tree is still intact: every creature whose controller changes from a Champion to None is assigned a Behavior as part of that same change. No "before/after the event" sequencing and no last-known-information construct: the tree at the moment of the change distinguishes the cases that look identical afterward (Companion X was A's, was stolen by B earlier, or was already Neutral — the last case assigns nothing). Removal cuts the children's parent links; they become roots. Covers a Companion's death (D82's automatic default) and a Companion made Neutral (D55); the Behavior assigned in the second case is not yet decided.

**Every reference in a trace binds at declaration (extends D70/D33).** A target placeholder is replaced by the bound object together with every qualifier that belongs only to it ("target creature you control," "within distance 2 of this"); "you"/"your" binds by the walk it already uses (Controller for "creatures you control," Payment for "your mana pool"/"you draw"); resolution-time choices bind their chooser. A clause relating two targets ("target creature at target hex") is not a qualifier and stays in the trace text. At resolution only what the trace still states is checked: an instruction fizzles if its bound objects are gone, can no longer be targeted, the remaining text is false, or executing it would violate an invariant. Rejected along the way: splitting qualifiers by kind (controller-type dropped, physical-type rechecked) — replaced by the single scope-based rule. Consequences accepted: a targeted buff lands even if the target was stolen in response; a creature-targeting spell can't be dodged by moving, but a hex-targeting spell or one relating the creature to a target hex can — dodgeability is expressed by what the effect acts on. Rationale: a trace does what its text says (the digital display shows the bound text, unlike a physical stack). "You" fixed at declaration matches every TCG including MTG; MTG's full qualifier recheck is the intentional departure. **Closes D82's gap:** a creation/conversion instruction from a Neutral source binds the source's Behavior and neutral turn at declaration, so inheritance works even if the source dies in response. A Trace's own live controller now matters only for the Trace's own activated abilities.

`PLAN.md` Track A items 18 and 19 removed; items 20–23 appended (Behavior on a Companion made Neutral, leftover Behavior on re-control, "can no longer be targeted" at resolution, mixed-controller Slice after automatic control loss); item 4 gains the creation-triple and `source` pointers.

→ `ancestry.md` (Controller section, active edges, subtree update, Open Questions), `neutral-permanents.md` (decider, dormant, §Becoming Neutral), `interaction-stack.md` (binding and resolution), `object-properties.md` (Open Question 5 reopened as provenance), `glossary.md` (Controller, Neutral, Behavior, Dormant), `PLAN.md`.

### D82 — Neutral-permanent Behavior and neutral-turn assignment resolved for every way a permanent can become Neutral; a source/parent gap spun out to its own item *(replaced parts in `decisions-archive.md`)*

Resolves `PLAN.md` §8 item 16's practical question (how Neutral relates to Ancestry beyond being one of Controller's three outcomes) — the abstract "is Neutral a formal Ancestry node" framing turned out not to be needed; working through every concrete way a permanent becomes Neutral answered the practical question directly, without touching Ancestry's structure.

**Six ways a permanent becomes Neutral, cross-cut by two axes:** whether it's entering the Island fresh or an already-controlled permanent changing control, and what caused it (another Neutral permanent's own ability, a Champion's card effect, a Map/scenario at Setup, or — the one fully automatic case — a Companion's death orphaning what it funded, `ancestry.md` §Removing an inner node). Each needs a Behavior and a neutral-turn (A/B) assignment before it can act.

**Caused by another Neutral permanent (creation or conversion alike):** defaults to inheriting both wholesale from the causing permanent — same Behavior instance, same turn, overridable by explicit card text. No new mechanism: a straight copy of an already-resolved instance, chosen because it gives Neutral permanents a coherent "third and fourth player" identity (a Neutral force's own actions can recruit/convert into its own standing orders) and needs zero new default logic. Applies to any Neutral permanent type capable of acting (Creature, Companion, Structure, Terrain — not just Creature), and whether the causing ability was activated or a mandatory trigger — Behavior's existing Scope rule already doesn't distinguish these.

**Caused by a Champion's card effect, or by Map/scenario placement at Setup:** no default in either case — the effect or scenario must state both explicitly. Deliberately not defaulted: differentiating Behavior by card (and eventually by Element identity, `PLAN.md` Track C item 1) is exactly the kind of choice a default would flatten.

**Caused automatically by a still-Champion-controlled Companion's death (the clearest case with no author to ask):** defaults to the neutral-turn seat already paired with the permanent's former controller in turn order, and a Behavior of `Aggressive toward` the *other* Champion — not its former controller. (A Companion already Neutral when it dies changes nothing about its subtree's controller — it was Neutral already, since a Neutral Companion is itself a root, D55.) A second automatic pathway, Structure/Ruin/Grave losing payer and controller when its terrain's bond is severed or blocked (`ancestry.md` §Removing an inner node), is not covered by this default — it may need different treatment given it can be transient/reversible rather than permanent, and is spun out to its own item rather than assumed to inherit this rule. Considered and rejected: `Aggressive toward its previous controller` (the `Betrayed` precedent, `history/neutral-permanents-draft.md`) as the automatic default — appropriate for a deliberate, paid-for card effect, but as a *free* consequence of a Companion dying it would double-punish the Companion's owner (lost funding and command, plus an active counter-attack from their own former army) and risks making Companion-heavy decks unplayable. Also considered and rejected: attributing the cause to whoever/whatever killed the Companion and treating that as an effect-driven case — breaks down whenever the Companion's own controller is the killer (e.g. paying a sacrifice cost), where "the cause" and "the victim's own side" are the same Champion.

`PLAN.md` Track A item 16 marked resolved and removed; items 18 and 19 appended.

→ `neutral-permanents.md` (§Creating and converting Neutral permanents rewritten as §Becoming Neutral, Decisions line), `ancestry.md` (open-question cross-references to item 16 updated to point at item 18; §Removing an inner node point 1 clarified for an already-Neutral Companion), `PLAN.md` (item 16 removed, items 18–19 appended).

### D81 — Ownership renamed Ancestry; the structure extended to Trace and Card, resolving PLAN.md §8 item 9 *(replaced parts in `decisions-archive.md`)*

Resolves `PLAN.md` §8 item 9 ("is Ownership a top-level concept, or only meaningful on the Board?"). Answer: **one structure, uniformly, across all three domains** — not three rules that happen to agree.

**Terminology: "Ownership" → "Ancestry."** Surfaced from a real confusion (the word "ownership" implies an "owner," a concept D74 already explicitly rejected — no such field exists anywhere). "Ownership tree"/"parent chain" (the umbrella term for the whole multi-hop structure) is replaced by **Ancestry**. **Parent** (the single, one-hop link, D26/D27) is unchanged — it was never the confusing term. `ownership.md` renamed to `ancestry.md`; full terminology sweep of current-state docs (not `history/decisions.md`'s own prior entries, which keep their period vocabulary as historical record — only their file-path links were repointed).

**Trace joins the Ancestry, leaf-resolved exactly like Creature.** A Trace's parent is resolved **directly, once, at creation**, to whichever Champion or Companion recursively funds/activates the entity holding the ability that produced it (even if the cost is 0) — never another Trace, Creature, Structure, or Terrain, no matter how many hops of Trace-on-Trace or permanent-triggered creation produced it. This is the exact same flattening pattern D26/D69 already established for Creature ("never another creature... a leaf... no matter how many hops"), just applied to Aether objects. **Motivating case:** a creature (parent: a Companion) has an activated ability that creates a Trace, which itself has an activated ability requiring mana — if the Trace's parent were the creature (a naive "immediate producer" reading), and either the creature died or an earlier Trace-in-a-chain aged out of Past, the later Trace's ancestry would break, even though nothing about who's actually funding it changed. Flattening to the nearest Champion/Companion at creation avoids this, matching how Creature-ancestry already only breaks when the *Companion itself* dies, never an intermediate hop. **Resolves two previously-open items:** `object-properties.md` Open Question 5 / `PLAN.md` item 4's parked "does a Trace carry a `source` back-reference" (yes — it's this parent link, no separate field) and D74's explicitly-left-open "responsibility" worked example (a Companion-cast "pay 1 life at the beginning of turn" upkeep trace — the upkeep trace's parent is the Companion, Payment climbs there and stops immediately since Companion holds Life directly, D58).

**Controller extends to Trace, unchanged in mechanism.** Climb the Ancestry to the root; Champion at top = that Champion controls it; no live Champion reached = Neutral. This formally backs two places that already used "the controller" for a trace/ability informally, with no definition behind it: `interaction-stack.md`'s Slow-speed rule ("playable during the controller's main phase") and its triggered-ability resolution-cost rule ("only the controller decides whether to pay").

**Card (the Mind-domain game-object, not the deckbuilding-tier collection card, `PLAN.md` item 6) also joins the Ancestry.** Its parent is **live, re-derived at all times, never stored** — whichever Champion's Hand/Library/Discard currently contains it. Always resolves to exactly one Champion, never Neutral, never parentless (only a Champion has these zones — a Companion has none). This is *not* a resurrection of D74's rejected persistent Owner field: that was rejected specifically for being a fixed value surviving zone/control changes; this is the opposite — a live query, same shape as Terrain's already-existing "controller is a live, per-query result, never a stored flag." Consequence: a card moving between zones — including a hypothetical future "steal a card" effect moving it into a *different* Champion's zone — updates its parent automatically, with no D55-style explicit control-change rule needed; cross-Champion card movement is just an ordinary zone-transition (`PLAN.md` item 5's territory), not a new mechanism. A destroyed/exiled card (with nowhere to go) simply ceases to exist — the "transition to nowhere" case `PLAN.md` item 5 already flags, now with a concrete worked example.

**Full parent-type enumeration, confirmed against the existing table plus two rows that never existed before.** Unchanged: Companion→Champion/nothing; Creature→Companion/Champion/nothing; Terrain→Companion/Champion/nothing; Structure→Terrain (always); Item(equipped)→Creature/Companion/Champion; Item(loose)→nothing. **New:** Trace→Companion/Champion/nothing (above); Card→Champion (always, live, above); **Ruin→Terrain (always)** and **Grave→Terrain (always)**, both previously undocumented — inherit Structure's exact rule, since both occupy the same kind of terrain slot/location Structure does. **Deliberately not extended to match:** a loose Item stays parentless rather than picking up a Terrain parent the way Ruin/Grave/Structure do — those three occupy a real, capacity-limited slot on their terrain (D24/D40), a structural relationship a loose Item never has (it's merely located there).

→ `ancestry.md` (renamed from `ownership.md`; parent table extended, Controller/Payment sections updated, new Open Questions section), `glossary.md` (Parent/Payment/Controller entries updated, new Ancestry entry, Trace/Grave/Ruin/Companion entries updated), `companions.md`, `structures-items.md`, `overview.md`, `interaction-stack.md` (terminology sweep), `object-properties.md` (Open Question 5 resolved, Open Question 2 updated), `PLAN.md` (item 9 removed as resolved, item 4's resolved sub-clause trimmed, item 10's stale cross-reference fixed, items 16–17 appended).

### D79 — Mana-pool "shared/private" axis replaced by naming pools after their root; "Champion" replaces "player" wherever a concept is engine-tracked

Resolves `PLAN.md` §8's Companion mana-pool terminology bug, plus a broader vocabulary sweep it surfaced.

**The bug:** `economy.md` and `companions.md` described a Companion's mana pool as funding "only that Companion's own printed abilities" — narrower than `ancestry.md`'s actual funding-tree rule (a Companion funds its **entire subtree**: every creature/terrain/structure/item rooted to it, not just its own abilities). Diagnosing the wording gap surfaced a bigger one: **"shared pool" (Champion) vs. "private pool" (Companion) asserts a bigger distinction than actually exists.** The payment walk (D74) already treats both identically — climb the parent chain, stop at the first Champion or Companion node found. The Champion's pool isn't structurally different from a Companion's; it's just the pool sitting at the tree's root, so it happens to catch everything a Companion doesn't intercept first.

**Adopted: name pools by their root, not by an assumed shared/private axis.** "The Champion's pool" / "a Companion's own pool," with scope following D74's existing walk mechanism rather than an adjective. Fixed throughout `economy.md`, `companions.md`, `champions.md`, `ancestry.md`, `overview.md`, `structures-items.md`, `resources-terrain.md`, `neutral-permanents.md`, `glossary.md`.

**A second, related question surfaced from there: does "player" or "Champion" name a given in-game concept?** The core is deterministic and self-contained (pillar 4), and Champion is pillar 7's in-match representation of the player — progression/meta stuff lives outside the core entirely. So rules-doc prose should say **Champion** wherever the referenced thing is something the engine itself actually tracks (a zone, a pool, a turn, a controller value, a perception/view) — the payment/controller walk only ever climbs to a Champion or Companion node, never to an abstract "player" — and keep **player** only for genuine human/meta actions the core doesn't model (choosing a Champion pre-match, deckbuilding, Setup's "players cannot observe/interfere," and the still-separately-parked `target player` card-templating keyword — a distinct, later question about what cards themselves print, not resolved here).

**Applied consistently:**
- **Mind domain (Hand/Library/Discard) is Champion-owned**, not player-owned — matches `ancestry.md`'s existing rule that Hand/Library access is held only by a Champion, never a Companion.
- **Home ground / terrain deck / realm** are Champion-owned board possessions, not player possessions (`map.md`, `glossary.md`, `resources-terrain.md`, `setup.md`, `companions.md`).
- **Turn-seat/controller identity: "Player A / Player B" → "Champion A / Champion B"** throughout (Controller, turn order, "active Champion," "both Champions pass priority," first/second Champion's turn) — `ancestry.md`, `neutral-permanents.md`, `overview.md`, `glossary.md`, `interaction-stack.md`, `resources-terrain.md`, `setup.md`, `asymmetric-information.md`.
- **Perception/visibility ("per-observer view," belief state, "visibility belongs to...")** is Champion-scoped, not player-scoped — `asymmetric-information.md`, `overview.md`, `glossary.md`'s View entry, `neutral-permanents.md`, `interaction-stack.md`.

**Left alone, deliberately:** the `target player` card-templating keyword (parked — a separate question about card-facing wording, not internal rules vocabulary) and every genuinely human/meta usage (choosing a Champion, deckbuilding talk, Setup's non-interactivity, "the player redirects/cancels" at a targeting decision point).

→ `economy.md`, `companions.md`, `champions.md`, `ancestry.md`, `overview.md`, `structures-items.md`, `resources-terrain.md`, `neutral-permanents.md`, `glossary.md`, `map.md`, `setup.md`, `interaction-stack.md`, `asymmetric-information.md`, `object-properties.md`; `PLAN.md` §8's Companion-pool terminology item removed as resolved.

### D78 — Level → Slice; Surface/Air/Underground → Ground/Sky/Root (bottom-to-top), superseding D41
Two independent word collisions surfaced once Champion progression terminology (D2, `champions.md`) was checked against the hex vertical-space terms D41 introduced: **"Level"** already means a Champion's power tier ("level bands," "level-up," "a level 6–10 Champion"), and **"Air"** already names one of the 8 Element colors (D51) — both concepts appear in the same rules text as the hex-space terms, sometimes in the same sentence about a Champion, so the overlap isn't hypothetical. D41's "pure relabeling" never weighed either collision.

**Candidates considered for the collective term and rejected:** Zone (taken — the four Hand/Library/Play/Aether zones, D16/D36), Tier (taken — Card/Trace/Permanent property tiers, D69), Band (taken — Champion progression range, D2, and semantically wrong besides: Band already means *a range*, not a single discrete slot, so reusing it for one of three stacked spaces fights its own connotation), Realm (taken — a Champion's bonded network, D9), Reach and Stratum (both read as too large/grand a word for a world built on small floating islands — Reach implies scale, Stratum implies geological permanence, neither fits), Plane (viable once Air was renamed away, but flavorless/too geometric), Horizon (rejected — connotes something infinitely receding, the opposite of a bounded floating island).

**Adopted: Slice.** It's simultaneously a layer (a slice of cake is one stacked layer) and inherently finite (a slice has edges, unlike an expanse) — matching a floating island's actual scale. German sanity-check: *Scheibe* carries the same double meaning and reads as a plain, everyday word, not a technical or pompous one.

**Adopted individual names, bottom to top: Root / Ground / Sky.** Ground reverts to its own pre-D41 name (dropped by D41 in favor of "Surface," now restored) — chosen over keeping "Surface" for rhythm: Root/Ground/Sky are all one syllable, "Surface" broke that parallel. Sky replaces Air specifically to clear the Element-name collision. Root replaces Underground/submerged — a better fit than "underground" for a landmass that's airborne (it's the island's own hanging rock-and-root mass, not literally below ground), and its hidden-by-default rule (D12) reads naturally as "you can't see a thing's own roots." **Canonical listing order is Root, Ground, Sky** (bottom to top, matching physical adjacency) wherever the three are enumerated together, superseding whatever order individual docs previously used incidentally.

**"Submerged" (the asymmetric-info flavor term, `asymmetric-information.md`) is explicitly untouched by this decision** — a separate descriptive term for the concealment mechanic, not the Slice name itself, and revisiting it is out of scope here.

→ `overview.md`, `glossary.md`, `asymmetric-information.md`, `interaction-stack.md`, `ancestry.md`, `companions.md`, `structures-items.md` (full sweep done); resolves `PLAN.md` §8 Track A item 2. Not yet touched: implementation (`Leyline.RulesCore`, the scenario format, DebugUI labels still use the old terms) — a Track B follow-up, not done here.

### D77 — Mana-generation timing & accounting: abandon "enters tapped," adopt a live per-color pool with a per-permanent drawn flag (Option C); A and B considered and rejected

Resolves `PLAN.md` §8 item 14. The existing "mana is summoning-sick" rule (a per-Beginning-phase snapshot; a bond made mid-turn doesn't credit until the *next* Beginning phase) is abandoned outright, not patched. Three independent arguments converged: (1) **psychological** — a player who takes the Bond action and watches their pool not move experiences a direct action→no-reward pairing, the same felt tax MTG's "enters tapped" produces even among players who understand the math; delaying access by *not* acting (bonding a turn later, untapped) has no such sting, since no promise was made to violate — the two are only equivalent on a spreadsheet, not at the table. (2) **mechanical inconsistency** — D57 already settled that summoning sickness keys on *how* a permanent entered the Island (cast vs. Setup-placed), not on control changes, and already exempts Terrain's own Activation Points from any sickness delay for exactly this reason; a Bond is a control-change on an already-present object, never a casting event, so applying a casting-shaped delay to its mana was an unjustified analogy (the original note cites "same shape as D14" with no derivation from the actual criterion), not a consequence of any adopted rule. (3) `resources-terrain.md`'s own severing section already specifies that restoring a paused bond is "live, per-query... not a stored flag" — directly contradicting the blanket "mana is a per-turn snapshot" language stated elsewhere in the same doc. None of the three was contested.

**Three candidate replacement models were evaluated:**

**Option A ("exact," rejected).** Every mana-producing permanent holds its own (max, current) mana, refilled live on reachability (D69/D70's existing property-tier pattern), summed per query, needing a configurable draw-order policy to resolve which source(s) pay when several are eligible. Correctly modeled, A never claws back *credited* mana, but a later block on an already-reachable, uncredited source excludes it from the live sum. Rejected: real per-source state and its own allocation-policy problem (see B), for no benefit over C — and where it does differ from C, it claws back mana the player never had a chance to protect, exactly the punishing behavior the "enters tapped" rejection above exists to avoid.

**Option B ("tube network," rejected but not discarded).** Mana flows to the Champion only through cleared segments, closest-to-Champion first; a segment clears once drawn (received, whether spent or freely discarded — never forced). This changes A's allocation-policy problem rather than removing it — order across different distances is forced by topology, but sources tied at the same distance still need a tiebreak, just a narrower one than A's free choice among every reachable source. It does real work once color is in play: mono-color's "A≡B always" collapses the moment distinct colors compete for one path (order among sources of one fungible type never affects a total; confirmed via worked examples), and drawing through an off-color segment to reach a same-color one behind it permanently discards the off-color unit — unusable for a *different* part of the same combined cost. This can make a multicolor cost genuinely unpayable, not just delayed, even with sufficient total supply on the board (a chain interleaving A,B,A,B,A makes `AAA and BB` together impossible; satisfying either color exhausts the other). Tested against 10 random 3-neighbor home-corner boards for a representative depth-cost ask: 7/10 had both colors trivially available in excess; the 3/10 constrained cases were all fully resolved by a single Champion move (repositioning changes which terrain reads as "close" — an existing, cheap action). Given a real two-color deck's actual needs (a handful of pips per color, not the 6-7 needed to make the constrained cases bite), B's friction essentially never triggers in realistic play. **Rejected as specified — the complexity isn't paid for by a puzzle that mostly doesn't happen.** Not discarded as a direction: if a harder color-depth puzzle is wanted later, B would need deliberate tightening, not adoption as originally described. Kept live in `history/playtest-variants.md`.

**Option C ("accumulator," adopted).** For each color, a player (Champion) — or Companion, for its private pool — holds a single spendable pool. Every mana-producing permanent also carries its own boolean "drawn this cycle" flag (not a numeric current-mana — just a gate): the flag flips true, crediting the permanent's production into the shared pool exactly once, the instant it's both reachable and still flagged false; further reachability changes don't re-trigger a credit. This is necessary because a hex bonded, blocked, and reconnected within the same turn must only ever pay out once — the flag, not a live recomputation, is what guarantees that. Both the pool and every permanent's flag reset at the owner's own Beginning phase. **Displayed as a graphical state per permanent:** a drawn permanent's leyline connection renders green (flag true, contributing, safe regardless of what happens to it later this cycle); a permanent that's currently unreachable and still undrawn renders red (blocked). There's no third state — a reachable-and-undrawn permanent credits immediately and is green the same instant. Decisively simpler than both alternatives, and behaviorally identical to A in every case A can currently reach. Resolves the hard-unbond leftover-mana question for free: since mana never lives "in" the permanent (only the flag does, and the flag only ever gates a one-time credit), there's nothing left behind to ask about — a harder unbond only ever forfeits future credit, identically to a pause.

**Refill cadence:** every Beginning phase (matching AP), not once per round — `resources-terrain.md`'s open question #3 is resolved. The pool (and every flag) is explicitly emptied/reset *before* that phase's crediting recomputes it, and no priority window opens between the empty and the recredit — a player never gets a chance to spend soon-to-be-wiped mana at the boundary.

**The Champion's Setup-time pre-bond to its home tile is kept** — no longer needed to patch "enters tapped" (an ordinary mid-turn Bond now credits live under Option C, no special-casing required), but independently justified by two things surfaced this session: (1) it accelerates the opening turns, valuable given Leyline likely runs comparably long for a TCG; (2) combined with the existing realm-confinement rule (`champions.md`: a connected Champion may only move onto already-bonded ground, `2AP`/hex, until it pays `0AP` to Collapse Network and cuts loose), starting pre-bonded means any strategy that wants to rush the Champion far from home — toward the opponent's home base, or a specific piece of terrain — must pay the Collapse Network cost first, from turn 1. A deliberate tax on aggressive early positioning. **Tuning idea floated, not adopted:** raising Collapse Network's cost from `0AP` to a small positive value to sharpen this tax further — parked as a playtest question.

**Deliberately not resolved here:** the exact numeric pip cost of "deep" color requirements (a deckbuilding/content question) and the colored-pip-vs-generic cost model itself (`resources-terrain.md` open question #1) — now more urgent, since this session found only depth-costed pips make color matter at all under a visible, deterministic terrain system, but still undecided.

→ `resources-terrain.md`, `champions.md` (keep the pre-bond, rewrite its rationale), `economy.md`, `setup.md`.

### D76 — Match Setup is not a domain transition; Champion/Terrain placement, first-turn AP asymmetry, and neutral-ground population all revised *(Setup procedure, its non-observability, the enters-the-Island consequence and the skipped refresh revised by D114)* *(replaced parts in `decisions-archive.md`)*

Resolves `PLAN.md` §8 item 9(e), scoped to **Champion and Terrain only** — Neutral-permanent/scenario placement is explicitly split out as its own item (§8 item 15), not addressed here. New `setup.md`.

**Why this isn't a Cast, structurally.** Champion and Terrain are printed and collected like any other card, but never occupy Hand/Library/Discard (`object-properties.md` §1) — "card" here does double duty for two different kinds of game object (a real naming collision, tracked separately at `PLAN.md` §8 item 13, deliberately not resolved by this decision). Since they never pass through a Mind-domain zone, their arrival at Setup was never a Mind→Matter transition to begin with — there's no Card object to skip the Aether from, so D69/§2's "Mind→Matter forbidden" invariant isn't in tension with this at all. Setup reads directly from a player's collection and creates Permanents; no Card or Trace is ever created for Champion or Terrain specifically.

**Neutral-ground population loses its default, revising D53.** Home ground keeps D53's original default (uniform random from that player's own terrain deck) unchanged. Neutral ground previously defaulted to "fixed assignment" (the map lists exactly what's on each hex) — **now there is no default at all: every map must explicitly specify which population algorithm it uses, plus that algorithm's own parameters, chosen from a growing, open-ended library.** Fixed assignment becomes one option in that library, not a privileged fallback. Illustrative examples of the library's shape (not adopted as named mechanics, just to establish the pattern): a **gradient/blend** algorithm placing terrain similar to each player's own home base near that player, blending toward the middle for a natural feel; a **clustered-distribution** algorithm scattering specific special terrain randomly within a bounded sub-region rather than uniformly across the whole map.

→ `setup.md` (new — full procedure), `map.md` (neutral-ground default removed, D53 revised), `champions.md` (D48 superseded, pre-bond flagged provisional), `overview.md` §3 (mana/AP refresh split into independently-skippable effects), `PLAN.md` §8 item 9(e) marked resolved for Champion/Terrain and item 9 overall marked fully resolved, with items 13, 14, and 15 spun out as the threads this surfaced but deliberately didn't resolve here.

### D74 — Payment generalizes into one "resource walk" covering both costs and effects; Bounce/Remand need no Owner concept, since a card's own destination wording already resolves via the existing Controller walk *(replaced parts in `decisions-archive.md`)*

Resolves `PLAN.md` §8 item 9(d) (are Neutral, Map-placed permanents legal Bounce targets) — the investigation took a different route than the question implied, and surfaced a second, more general gap along the way.

**The natural first idea — give Bounce/Remand a persistent Owner property (separate from Controller, fixed at Cast, surviving control changes) so a backward-created Card knows which player's Hand it belongs in — was proposed and tested against three cases (a control-reassigned creature, a cast-then-Neutral creature, a Map-placed Neutral creature with no Cast at all) and held up structurally.** Rejected anyway, on design-philosophy grounds, not a structural flaw: Leyline is a digital-only design that already lets mana cross into contested territory rather than staying leashed to a fixed "home" network — the same instinct applies here. There's no need for a hidden, implicit "who does this belong to" default at all. **A Bounce/Remand-style effect simply states its own destination in its own printed text** ("return target creature to **its controller's** hand," "to **your** hand," etc.) — reusing the *existing* Controller walk (`ancestry.md`), no new field anywhere. A Neutral, Map-placed permanent then has no legal "controller's hand" to send anything to (Neutral has no hand at all) — the instruction simply has nothing to do, the same per-instruction-fizzle shape D33 already established for an illegal target. **Answers 9(d): yes, legal to target in general — whether a specific effect has anywhere to put the result is a property of that effect's own wording, not a base rule.** No Owner concept is adopted anywhere.

**That in turn exposed a real, separate gap: `ancestry.md`'s Payment section was written only for costs** ("who funds **a specific cost**") — but the identical climbing mechanism is exactly what an *effect's* implicit resource-holder pronoun needs too ("add 1 mana to **your** mana pool," "**you** draw a card"), and nothing said so. **Generalized: the same walk — starting at the entity itself, climb parent-links, stop at the first node that holds the needed resource — serves both roles**, cost and effect alike, differing only in what happens when the walk finds nothing:
- **As a cost:** finding no node holding the resource means the ability/spell **isn't a legal action at all** — already covered by D46 ("a card/ability is only ever offered as a legal play if a legal cost-and-target combination exists"); just confirmed to apply here too, not a new rule.
- **As an effect** (the instruction produces or reads a resource rather than spending one to become legal): finding no node **fizzles that one instruction**, the rest of the ability/card resolving normally — the same per-instruction-fizzle shape D33 already established for an illegal target. This can only happen when the walk runs off the top of the chain without reaching a live Champion, i.e. the acting permanent is currently Neutral with no live root at all.

**Two worked examples, both hypothetical (neither is a printed ability yet — used only to stress-test the rule):**
- *"Add 1 mana to your mana pool"* on a creature — "your" resolves via the identical mana-climb Payment already defines: stop at the nearest Companion in the chain, or the Champion if there's none. If the creature is currently Neutral (no live root reachable — its Companion died, or it's a Map-placed permanent never in anyone's chain to begin with), the instruction fizzles; there is no pool to add to.
- *"Draw a card"* on a creature — needs Hand+Library access, held only by a Champion (a Companion has no Hand, `companions.md`) — climbs past any Companion in the chain and stops at the Champion. Same Neutral-creature fizzle case applies.

**Activation Points and Life still never climb** (self-held by everything that carries them, D58) — the generalization only matters for a resource an acting permanent might *not* itself hold (mana; Hand/Library access), the exact same set the original Payment walk already had to climb for.

→ `ancestry.md` (Payment section generalized to cover effects, worked examples added, cost-vs-effect failure split stated), `glossary.md` (Payment (walk) entry broadened), `object-properties.md` (Open Question 1 resolved — Bounce/Remand destination is per-effect card text via Controller, not a rules-level Owner field; §2's Bounce/Remand table rows updated), `PLAN.md` §8 item 9(d) marked resolved.

### D73 — No non-location target ever persists as a Permanent-tier property; the D68 location/permanent split doesn't generalize into the object-creation model

Resolves `PLAN.md` §8 item 9(c) / `object-properties.md`'s open question on location- vs. permanent-typed targets — whether "target" needs splitting into sub-kinds for any purpose in this doc's own model, beyond the unrelated reason D68 already needed the split (its redirect-on-illegal-destination mechanism applies only to location-typed choices, never permanent-typed ones).

Tested via a systematic search for a non-Aura MTG permanent whose own casting (not a subsequent triggered ability) targets another permanent and has that relationship persist as an ongoing property of the resulting permanent — the pattern §4 already grants Location, uniquely, via "Location is target's Matter-side identity, not a separate property."

**Aura is the one real precedent, and Leyline deliberately has no Aura-shaped card type.** Bestow is the creature-flavored version of the same mechanism, not an independent case.

**Every apparent non-Aura candidate collapses under inspection:**
- "Enters the battlefield, exile target creature" (Fiend Hunter/Banisher Priest-style) resolves via an ETB **triggered ability**, not the card's own casting target — the same category already ruled out earlier in this thread by the Item/equip example ("When this enters the Island, you may equip it to target creature" has a persistent property too, but via a trigger, not a cast-time target).
- Doppelganger/Mocking Doppelganger-style "enter as a copy of a creature" **is not a target at all** in MTG's technical sense — rule 115.1 defines targeting purely by the literal word "target" appearing in the text, and this templating deliberately avoids it. The choice is a **replacement-effect choice** (CR 707, copying objects) made **as the permanent would enter the battlefield**, re-evaluated against board state at that moment — never locked in at cast, never subject to Hexproof/illegal-target fizzle. Confirmed via Mocking Doppelganger's own ruling ("You may choose only a creature that's already on the battlefield").

**Why the pattern doesn't exist outside Aura, structurally:** an ordinary target on an instant/sorcery has a remaining instruction to apply itself to at resolution (fizzles per-instruction if the target's gone, D33). A permanent spell's resolution *is* "become a permanent" — there's no separate instruction left to receive the target. Aura sidesteps this because the target relationship *is* the permanent's whole mode of existence (continuously rechecked, the Aura falls off if it disappears) rather than a value consumed once by an effect. Doppelganger's actual solution confirms MTG's own designers hit this same wall and routed around targeting entirely rather than target a permanent spell.

**Adopted: no generalization.** Location remains the sole permanent-specific instantiation-time field (§4, unchanged). The D68 location/permanent target split stays scoped to D68's own redirect mechanism only; `object-properties.md`'s target model needs no equivalent split, since "target never locks in" already covers every kind of target uniformly with no observed exception. Closed on a negative result (no counterexample found after a real search), not merely unpursued.

→ `object-properties.md` (open question on location/permanent-typed targets removed, resolved; remaining questions renumbered).

### D72 — A permanent card's modal choice is the same "choose" step machinery a Spell's mode already uses; closes `object-properties.md` Open Question 1

Resolves `PLAN.md` §8 item 9(b) — whether any permanent-card modal choice exists that doesn't collapse into an ordinary triggered ability, the working hypothesis Open Question 1 left unconfirmed.

**Worked example: a split-permanent card** (MTG's split-card tech doesn't exist for permanents there, but nothing structurally prevents it in Leyline) — one card castable as either Creature A or Creature B, each its own full stat/ability bundle. This does *not* reduce to a triggered ability: a triggered ability presupposes a base object already exists and only modifies it ("as this enters, choose one: +1 Attack or Flying"); choosing "Creature A" vs. "Creature B" instead picks which entire max-tier schema (name, Attack/Life/AP, ability list) the trace collapses into in the first place — there's no base object yet for a trigger to modify.

**Resolved anyway with zero new machinery — the same "choose" step (D70 step 2) already covers it.** Nothing in D69/D70 ever restricted a modal menu to varying only effect instructions on one fixed object; a menu selecting among entire stat/ability bundles is the identical mechanism, just a richer option shape — the same move D70 already made once, generalizing a single modal pick to a repeated-pick list. "Which face" is chosen and locked in at step 2 exactly like any other mode, gated as usual by what step 1's cost already permitted.

**Differing-cost split (Creature A: 3 mana / Creature B: 5 mana) initially looked pseudo-modal** — paying the 3-branch narrows step 2's legal menu to one option, so the choice looks absorbed into payment, needing no independent choose step at all. **The sharper test that actually confirms a real, free choice exists: a same-cost split** (both faces 3 mana). Here step 1 can't disambiguate at all — step 2 is a genuine, unconstrained pick between two stat/ability bundles, proving the choice is real and independent of cost, not merely an artifact of the differing-cost case.

**Consequence, confirmed deliberate, not a gap:** per D69 §3/§4, a resolved modal choice locks in permanently — a bounced/flickered split-permanent returns locked to whichever face was chosen, the other face gone for good, exactly like a bounced X-cost creature returns flatly-statted. Explicitly endorsed as a feature, not merely tolerated: this is the same "a bounce-created object only ever exists in the form the bounce produced" behavior that already makes ordinary Bounce/Flicker well-defined for card-less Neutral permanents (D69 §2) — not a special exception carved out for split cards, a natural instance of the same rule.

**Net effect on Open Question 1:** the literal hypothesis ("location is the only true instantiation-time choice a permanent card can have") is falsified by this example — but the actual design question it stood in for closes clean anyway: a permanent card needs no machinery separate from a Spell's modal "choose" step; location remains the only *permanent-specific* instantiation-time field (a Trace's own target/location, §4), everything else a permanent might choose at cast time is ordinary modal choice, already covered. **Deliberately not written into the rules docs as a special case** (the user's explicit call) — `object-properties.md` §4's existing "modal card's chosen mode" language already generalizes past Spells, which is the only current-state rules change; the split-card example itself lives here, in the decision log, as the worked test case, not as card-type content in the rules docs.

→ `object-properties.md` §4 (wording generalized from "modal spell" to "modal card"; Open Question 1 removed, resolved, remaining questions renumbered).

### D70 — Casting/activating is a three-step procedure (pay → choose → target); cost is CNF-structured and paid semi-automatically; a trace only exists in the Aether once all three steps complete — which is also why modal choices lock in on Remand/Bounce while target never can *(replaced parts in `decisions-archive.md`)*
Resolves `PLAN.md` §8 item 12 — the suspected link between D46's cost/instantiate boundary and `object-properties.md` §4's durable-vs-resets boundary. **Confirmed: they're the same boundary**, sharpened from D46's original two-stage split (pay, then everything else) into three. Reached through comparative research against Magic, Hearthstone, and Android: Netrunner, then validated against one fully worked test card end to end, not settled by discussion alone.

**A trace does not exist in the Aether — isn't visible, isn't respondable-to, isn't reachable by Remand or anything else — until all three steps are complete.** This resolves the actual mechanism behind D69 §4's already-adopted "target never locks in, cost/mode always do" split: it was never a property-by-property judgment call, it's a direct consequence of *when* a trace becomes a real object at all. Since nothing outside the casting player's own private declaration can observe or act on a trace until cost, mode, *and* target are all fixed, there is no reachable state where a Remand-style effect could catch an "in-progress" trace with modes still open — the state doesn't exist to be caught.

**A real alternative was weighed here and rejected, not merely passed over: keeping modal choices open through Remand/Bounce**, so a bounced card would read exactly as printed except for its now-fixed cost — explicitly attractive for the added versatility it would hand back to the returning player. Rejected for two independent reasons, either one sufficient on its own: (a) it requires exactly the reachable-half-resolved-trace state the paragraph above rules out; (b) it would carve out an exception to the already-adopted "a modal spell's chosen mode locks in like an X-cost" rule (D69 §4) for no principled reason found — a repeated-pick modal choice isn't a different *kind* of thing than a simple one, just a richer shape.

**A trace's displayed/current text is generated by iteratively collapsing the card's own template text as each step resolves, not by revealing a pre-written hidden block.** An unpaid cost alternative disappears once its sibling branch is chosen; the effect menu is replaced by whichever instructions were actually picked once step 2 completes, and a clause conditioned on a now-permanently-settled fact (e.g. "if a creature was sacrificed to pay for this, also...") collapses to a flat, unconditional clause; each `target X` placeholder becomes an annotated binding (`target creature (Grunt #145)`) once step 3 resolves. **The annotation is live game data, not decoration** — resolution reads it directly to determine what the trace actually affects; the bare template text is kept alongside it specifically so Remand's job stays mechanical (delete the annotation, the unchanged template is still there) rather than needing to regenerate placeholder phrasing from a flattened value.

**Confirms and closes `PLAN.md` §8 item 9(a):** Remand for a Spell, previously untested against a concrete card, now has been — behaves exactly as `object-properties.md` §4 already predicted.

**The broader principle this surfaces, stated explicitly for the first time though implicit in D69 §3's property-tier model all along: forward instantiation and backward creation (Bounce/Flicker/Remand) are not two independently-designed rules that happen to agree — they're one per-field tier-membership table, read in opposite directions.** Forward, a transition fills in whichever fields the destination tier's schema adds that the source didn't have. Backward, a transition keeps whichever fields the destination tier's schema still has room for and drops the rest — the identical lookup, reversed, not a second rule requiring independent upkeep as new field types get added later. The full table (one row per property, across every object type) is `PLAN.md` §8 item 10's actual deliverable — not yet built; this decision only establishes that building it once, correctly, settles both directions at once.

→ `interaction-stack.md` (casting/activating procedure rewritten), `structures-items.md` (casting procedure corrected to match), `object-properties.md` (§3 gains the tier-table principle, §4 extended to repeated-pick modes, Open Question 1 closed).

### D69 — Object creation is symmetric across domains (Bounce/Flicker join casting) and Markovian (a created object depends only on its immediate predecessor's current state); target is the one property that never locks in *(replaced parts in `decisions-archive.md`)*
Resolves the "properties of game objects" question flagged 2026-09-15 (`PLAN.md` §8 item 8, surfaced by a real overheal bug — `HealEvent`, `src/Leyline.RulesCore/Events/Event.cs`, was left deliberately uncapped for exactly this reason). Full current-state rules: `object-properties.md` (new). This took a long, corrective discussion to land, and the corrections are worth keeping as much as the conclusion.

**First correction: casting a permanent-producing card doesn't move a Card object — it *creates* a linked Trace, and the Trace's resolution *creates* a linked Permanent (D16, already existing).** The initial instinct was that this made a *backward* transition — returning a permanent to hand ("Bounce") — impossible without either violating "one object, one zone" or needing tokens. Both are wrong: D16 was being read as if it forbade creating a *second*, separately-existing card while the original discarded one stays put, when it only ever said an object can't be in two zones *itself*. Bounce (Matter→Mind) and its Aether counterpart **Flicker** (Matter→Aether) are symmetric with the forward direction: they **create** a new, linked object rather than relocating the old one. The old discarded card is untouched and stays exactly where it was; the created object still carries the same mandatory Card-Definition reference (so it's a real instance of a real card, never a token), it's just a *second* physical object. This also means Bounce/Flicker never need to find the permanent's original card or trace — resolved entirely by reading the *permanent itself*, which is what makes them work identically whether the original card is still in Discard, has been exiled from Discard entirely, or (a Neutral, Map-placed permanent) never existed at all. **Mind→Matter is adopted as the one forbidden direction** — "magic always leaves a trace," even an Instant resolving atomically still passes through the Aether (D45) rather than skipping it. **Aether→Mind ("Remand")** for a permanent-producing card needs no special rule beyond the general one below; for a Spell it's flagged open (`object-properties.md` Open questions).

**Second, and the actual mechanism: what does a Bounce/Flicker-created object's properties come from?** Three candidates were weighed — (a) always reset to the card's blueprint/printed values; (b) derive from whatever the specific card instance looked like at the moment it was cast; (c) derive from the permanent's *current* state at the moment of Bounce/Flicker. **(c) was adopted.** It resolves (a)'s and (b)'s shared weakness for free: neither needs to locate an ancestor object at all, which is exactly what makes them work for permanents with no card origin or a since-removed one. Generalized, this became **Markovian object creation**: a created object's properties are a pure function of its immediate predecessor's *current* state only, never anything further back — no lineage reference, ever, in either direction.

**Property tiers fall out of this directly.** Card and Trace hold only the **max**-tier of every stat plus cost and the ability list (what "printed" means); Permanent adds the **current**-tier (current-Attack/Life/AP alongside max). A backward creation reads the predecessor's current state, restricted to whichever fields the *new* object's own type has room for — current-tier values are dropped on Bounce/Flicker not as a special exclusion but because Card/Trace never had a current-tier slot at all, the same reason Card has no location field. **Consequence, deliberately accepted:** a genuinely permanent max-stat increase survives a Bounce/Flicker into the new card/trace; anything that only ever touched a *current* value (ordinary damage, an "until end of turn" combat trick) does not. This lets a temporary buff be "banked" into a permanent upgrade via Bounce — accepted as intentional, differently-costed design space (a lasting +2/+2 is categorically more expensive than a temporary one) rather than something the engine should prevent. Abilities (static/triggered/activated) are stats under this same model, inheriting and extending the same way numeric stats do. AP fits the identical (max, current) shape, with one independent axis worth naming: **the recovery policy for *current* is per-stat, not universal** — current-Life never self-recovers (D14, persists until healed), current-AP fully refills every Beginning phase (existing rule) — same shape, opposite default behavior, and a future stat may need a third policy.

**Third: what locks in vs. what always resets, on any backward creation.** A resolved variable value (an X-cost, an X/X/X stat line, a modal spell's chosen mode) becomes an ordinary fixed property from that point on — flows forward and backward like any printed stat — because cost/mode are properties of the object's *own* nature; the Card's schema always had a slot for them, "X" or not. A cost that consumes specific objects to pay it (sacrifice X creatures, discard N cards) locks in only the count, never which specific objects were consumed. **Target never locks in — a single flat rule, not derived from a broader taxonomy, since no second property needing this treatment has been found despite looking.** An earlier attempt to justify this via a general "self-referential vs. other-referential" property taxonomy was retracted as unnecessary machinery for a rule with exactly one known member; an even earlier attempt ("Card only has *assigned values*, target is only ever an abstract unresolved requirement") was shown to fail, since an unresolved X-cost is exactly as "unassigned at the Card level" as an unresolved target, yet the two must behave oppositely — the assigned/abstract framing doesn't actually distinguish them. **The distinction that survives:** a target — of any kind, a location, a creature, a player — never has a value-holding slot in the Card's own schema, resolved or not; the count/type of targets a card needs is metadata *about the casting action's legality*, not a property the card itself possesses, in contrast to cost which the Card's schema does hold a slot for (even variable). **Location is target's Matter-side identity, not a separate property:** introduced first at Trace (the resolved value of the one mandatory, implicit target every permanent-producing card has), inherited into Permanent under a different name suited to a live, continuously-mutable fact rather than a frozen one-time choice — flowing in *both* directions between Trace and Permanent specifically (forward at an ordinary cast, backward at Flicker, freshly re-read each time) but never reaching Card, which never had the field. A flickered permanent's new Trace target is a forced, single-candidate read of the permanent's *current* location — not a player choice — unless a card states otherwise.

→ `object-properties.md`, `overview.md` §1, `glossary.md` (**Zone**, **Bounce**, **Flicker**, **Remand**).

### D67 — Underground visibility belongs to the observer, granted by any permanent they control on the hex itself or a neighboring hex *(stated as the Root observation ability, D152)*
Underground is hidden by default (D12) to everyone but a permanent's own controller — but that default already had an unwritten exception, confirmed here rather than newly invented: **an observer sees a hex's Underground occupants if *any* permanent they control has Underground presence on that hex or on any neighboring hex.** Two things worth being precise about, since both were gotten wrong in an earlier pass at this same entry: (1) this is narrower than "any presence reveals it" — a Surface or Air unit standing on a hex gains *no* Underground vision of that same hex; only matching-layer (Underground) presence, on the hex itself or adjacent to it, does; (2) it's a per-**observer** query like every other perception query (`asymmetric-information.md`), not a per-*creature* one — the presence can come from *any* permanent the observer controls, not specifically the one about to act.

**The general rule this produces: entering any level is blind exactly when the acting player currently has no qualifying Underground presence (via any permanent they control) already covering that destination — nothing else about the specific ability matters.** This is not fundamentally an "Ascend vs. Descend" split, and it is not "always" anything for either — it was previously mis-stated both ways:
- A creature Ascending is *usually* safe only because its controller is usually already standing right there (its own Underground presence, at minimum) — not because Ascend as an ability is exempt. Ascending into a **Mist**-concealed Surface hex is exactly as blind as any Descend.
- A creature Descending is *usually* blind only because a controller usually doesn't already have independent Underground presence at that hex — but if they do (a second Subterranean scout already stationed there or on a neighboring hex), the Descend is perfectly safe, because visibility belongs to the *player*, not to the specific creature performing the action.

The same logic applies uniformly to any effect placing a creature into a level it lacks qualifying presence in — a card-effect exception forcing a non-Subterranean unit Underground (D42), summoning a Subterranean creature directly onto Underground, or any Move/Relocate/Ascend/Descend into a concealed destination the player has no independent eyes on yet.

Deliberately bounded, not a free scouting tool: range is exactly one hex (a permanent's own hex plus its six neighbors); it requires actually committing a real creature to Underground (a real card, a real cost, a body that can be found and killed once spotted); and visibility here is a **live, continuously-recomputed query**, never a one-time reveal event — nothing is permanently unlocked, and it can be lost again the instant the qualifying presence leaves or dies, exactly like every other perception query (`asymmetric-information.md`). → `asymmetric-information.md`, `overview.md` §4.

### D65 — Declare Defenders runs through the same Behavior mechanism as any other choice; Defend's cap moves from once per turn to once per round *(Defend cap replaced by D116)* *(replaced parts in `decisions-archive.md`)*
An early candidate design — a blanket "a Neutral creature always auto-defends its own hex when attacked" rule, handled entirely outside the Behavior system — was considered and rejected. The reason is itself one of the session's more important calls, worth preserving in the user's own terms: **a Neutral creature should feel exactly like an ordinary creature whose *only* difference is that a deterministic policy stands in for a player, not a creature with its own bespoke combat exception.** A real player's creature doesn't get a free, hardcoded "always defend" rule — the player decides every time, through the normal declare-defenders choice. Giving Neutral creatures a shortcut there would be exactly the kind of special-cased "Neutral is a different kind of thing" rule the whole design has been avoiding since D56. The fix, once the mode/policy mechanism (D62) existed to support it: Defend is simply another activated ability a mode can select, evaluated at the declare-defenders decision point using the identical live-query-and-score machinery as any proactive turn action — no bespoke combat rule required.

### D64 — Behavior scoring is a strict priority list over named criteria, deliberately not a general weighted value function
A late, explicit fork, decided by weighing simplicity against expressiveness rather than defaulting to the more general option. A value function (a weighted sum over criteria, e.g. `value = a·damage + b·life_gained`) can express genuine trade-offs a strict priority order cannot — "2 damage + 2 life gained" beating "3 damage alone" requires a numeric exchange rate between damage and life, which no ordering of discrete tiers can encode. The question was whether that expressiveness is worth its cost for what this game's Neutral permanents actually need.

**Why a strict priority list was judged simpler in the ways that matter, not merely shorter to write:**
- **No cross-criterion weight-tuning or unit-conversion burden.** A priority list never compares magnitudes across different criteria at all, only within one criterion at a time when higher tiers are tied — it never has to answer "how many damage points is one life point worth." A value function must answer that for every pair of criteria it combines, as a real number, and that number is scale-sensitive: a ratio tuned for small magnitudes can misbehave the moment a new effect introduces a much larger one (a big heal card suddenly making "life gained" span 1–20 instead of 1–2).
- **Safe, purely additive extension.** Inserting a new tier can never retroactively change how a *higher* tier already compares two candidates, since higher tiers are decided first and lower ones are never consulted unless everything above ties. Adding a term to a weighted sum, even a small-weight one, mathematically participates in *every* future comparison and can perturb a previously-tuned trade-off — which matters here specifically because keywords get extended after the fact by card effects (grants, removals), and that extension needs to be safe by construction.
- **Explainability, to designers and players alike.** "This creature values dealing damage over its own safety" is a priority order, one true sentence. A weight ratio ("damage is weighed 3.2× life") is not natural card text and not something a player can intuit mid-game, even when technically precise.
- **Determinism/precision for free.** Tier comparison is exact equality on discrete state, no floating-point weighted sums and no epsilon-comparison discipline — a real, if secondary, concern given how much the engine already leans on exact reproducibility (pillar 4).
- **Direct alignment with the stated design goal for Neutral permanents:** not an optimal AI, a system that feels fun and intuitively challenging while accepting real imperfection. The one thing a strict priority order produces that a smoothed value function wouldn't — "extreme," non-graduated choices at the margin (a damage-maximizing creature takes one extra point of damage over its own certain death, if damage is its top tier, with zero smoothing as the gap narrows) — was judged a *feature* under that goal: a simple, legible, learnable personality trait a player can read directly off the stated priority order and exploit, not an opaque numeric quirk.
- **Empirical validation, not just an a-priori argument:** every one of the five traced prototypes (Warrior, Sharp Shooter, Assassin, Item-Seeker, Vanisher), including both edit scenarios pressure-tested against Assassin and Sharp Shooter, was fully expressible with pure priority lists — nothing forced reaching for a weighted function. Reordering the *same* two criteria (damage-then-safety vs. safety-then-damage) is what already produces Warrior's and Sharp Shooter's genuinely different personalities from one shared mechanism — direct evidence the ordering axis alone carries real expressive range.

**The accepted cost, stated precisely:** no smooth trade-off between different criteria is ever possible — a higher tier wins by any margin, full stop. **The partial mitigation already in use, not invented for this purpose:** a tier doesn't have to be a raw magnitude — coarsening it into a boolean or threshold (Assassin's "did this deal any damage at all" gate, checked before its safety tier) recovers some of what a value function buys, cheaply, while staying strictly lexicographic. **Deliberately not built:** a general weighted-value scoring mode. If a concrete future card genuinely needs a real numeric trade-off, the right response is to let *that one keyword's* relevant tier declare a weighted term for itself, not to generalize the whole system on speculation before a card asks for it. → new `neutral-permanents.md`.

### D63 — A Behavior's candidates are whole-turn action sequences bounded by remaining Activation Points, not single next actions; execution stays receding-horizon
A direct correction to an earlier mistake in this session's own reasoning, worth recording precisely because the mistake was subtle and is likely to resurface. "No optimal AI" — this is a partially observable MDP, solving it exactly is neither feasible nor wanted, since the *AI opponent* (a separate, future system) is what's meant to approach that, not Neutral permanents — was initially taken to also mean "evaluate only the single next action, never look further ahead." Those are not the same limitation. Tracing Assassin against a granted ranged attack exposed the gap concretely: comparing only the immediate next step, a weak ranged shot available *right now* always out-scores "move one hex closer, then land a much stronger melee hit next step" — not because the stronger option was weighed and rejected, but because the model was *blind* to it entirely (a movement candidate always scores zero damage *this instant*, regardless of what it sets up one step later). That's a foresight failure within the creature's own single turn, not an instance of the accepted "doesn't model other agents" limitation the game is actually designed to tolerate.

The fix: a candidate is a bounded *sequence* of this permanent's own actions, filling as much of its remaining Activation Points this turn as the sequence needs, still generated from the same declared eligible action shapes (D62) and still hypothetically resolved (direct-effect-only, no recursion, no modeling of how another agent might respond) — just chained forward through the whole sequence instead of stopping after one step. Still cheap, still bounded: search depth is capped by the same AP budget that already caps how long any turn can run, and it still stops exactly at the boundary the game already agreed to accept (this permanent's own turn) — it never models what an opponent might do in response, and never plans across this permanent's own *future* turns. That boundary — one permanent's own remaining AP this turn, nothing about any other agent, nothing about future turns — is the precise, corrected statement of what "myopic" means here.

**Scoring criteria must each declare whether they're cumulative or terminal**, a distinction that didn't need stating before sequences existed: "damage dealt" sums across every hit in the sequence (two attacks in one turn should outscore one), while "resulting distance to nearest opponent," "resulting Hidden status," and similar state-facts are evaluated only at the sequence's *end* — summing a distance across intermediate steps would be meaningless.

**Execution remains receding-horizon, not a committed plan.** The sequence search only ever decides the *next single action*; after it executes for real (opening whatever priority windows it normally would, respondable by both real players per D56), the whole search re-runs from scratch against live state before the following action. Deliberate, not an oversight — it's exactly what already made ability grants/removals/thefts mid-plan "just work" without any change-detection machinery (D62), and extending to sequences doesn't cost that property: nothing about a multi-step plan is ever cached past the single action actually taken. → new `neutral-permanents.md`.

### D62 — Behavior is a deterministic policy: modes (trigger → target → scored action selection), not a fixed procedural script
Redesigns the internal shape of a Behavior (D56's original "complete, deterministic decision algorithm" framing) after tracing it against five prototype creatures (a plain damage-maximizing Warrior, a range-and-safety-conscious Sharp Shooter, a stealth-strike Assassin, a fixed-destination Vanisher, and a multi-stage Item-Seeker) and three card-effect edits a real match needs to support cleanly (stripping an ability mid-plan, granting a new damage ability, granting a battlefield-control ability). The prototypes are the actual argument for everything below — each requirement traces to a specific one of them, not to abstract taste.

**Scope: only choice-bearing abilities need Behavior coverage.** A static ability never needs one (it's not a decision). A triggered ability with no target choice and no "you may" clause fires automatically, no Behavior involvement — confirmed concretely by Assassin's on-hit Poison, which needed zero rules of its own. Only activated abilities (including the basic Move/Attack/Defend every permanent already has) and choice-bearing triggered abilities (a target to pick, an optional "you may") need a Behavior's involvement at all. Closes D56's original open question about *what* a Behavior even has to describe.

**A Behavior selects abilities by live-queried effect shape, never by fixed identity — this is what makes granted/removed/modified abilities "just work."** The original worry (how does a Neutral permanent notice its options changed, when a card grants it a new ability or strips one it was relying on?) dissolves once the policy queries *"the currently-legal abilities matching a declared effect shape (e.g. 'deals damage to a creature satisfying X')"* fresh, every single decision point, off the exact same live legal-action enumeration the engine already has to maintain for the UI and will need for AI (M4) — rather than ever naming a specific ability like `core.attack` or remembering what abilities a permanent used to have. A grant simply appears in the next query's results; a removal simply stops appearing; a cost increase that makes something unpayable is filtered out by the same legality check that already governs everything else. There is no "detect a change" step to design, because nothing is ever cached to go stale.

**A Behavior is an ordered/mutually-exclusive set of *modes*.** Each mode is: a trigger condition over live state; a target derivation (which permanent or hex the mode is "about," plus its own tiebreak if the derivation could be ambiguous — e.g. "closest creature satisfying X, ties broken by ID"); a declared, *bounded* set of eligible action shapes (deliberate scoping, not full permissiveness — see below); and a priority-ordered list of scoring criteria ranking whatever's currently legal among those shapes (D64 covers why this list is strictly ordered rather than a general weighted function). Item-Seeker is the cleanest illustration: three modes (Fetch/Hunt/Deliver), each with one target and one short criteria list, collapse what an earlier six-rule draft needed two rules per mode to express — the "in range, act" vs. "not yet in range, approach" split falls *out* of the criteria list for free (an ability that achieves the mode's goal, like Equip or Deliver, scores at least as well on the first criterion as any partial-progress candidate, so it's chosen without a separate priority rule ever being authored for it).

**The eligible-action-shapes declaration exists specifically to bound the candidate pool a mode's scoring considers — an unscoped "every legal action" pool is a real problem, not a hypothetical one.** Grant a Hunt-mode creature an unrelated "1AP: gain 1 Life" ability, and if "own resulting health" is anywhere in that mode's criteria list, the healing candidate can out-rank continuing to close distance on the actual target, purely because it scores well on a *later* criterion the keyword author never meant it to compete on. (That specific example turned out to be self-limiting, bounded by max Life — but the general risk, an off-agenda granted ability winning a step by exploiting an irrelevant criterion, doesn't require self-limitation to occur, and its cost only grows as more effects accumulate on a long-lived Neutral permanent — exactly the "massive amounts of neutral stuff" content this session is explicitly building toward.) Scoping isn't a contradiction of the single-ranked-list simplification — the ranked list decides *among* a mode's eligible shapes; declaring those shapes is a separate, narrower decision each mode makes once.

**Modes are live-derived (a state → mode mapping), not a stored state machine, by default.** A state-machine framing was seriously considered — it diagrams beautifully, and nothing about presentability actually forced the choice either way, since a diagram's nodes/edges are exactly the same modes/trigger-predicates regardless — but a *stored*, sticky state machine has to define an exhaustive set of outgoing edges from every state, including "my precondition just stopped holding" edges, or a permanent can get stuck (concretely: something steals Item-Seeker's Relic mid-Deliver; without an explicit escape edge back to Hunt or Fetch, it's stuck walking toward the delivery hex with nothing to deliver, forever). Re-deriving the active mode fresh from live state at every decision point is mathematically the special case of a state machine whose transition graph is complete — every state has an implicit edge to every other, taken instantly whenever that state's membership predicate becomes true — so there is no stored state to get stuck in, by construction. A keyword *may* still opt into genuinely sticky, stored state (the same pattern D56 already uses for "current target" memory, generalized one level up to "current mode") when it deliberately wants hysteresis rather than instantaneous reactivity — but that's an opt-in per keyword, not the default, and it inherits the exhaustive-edges authoring obligation when chosen.

→ supersedes the "complete algorithm, every case enumerated" procedural framing in the original draft's §6–§7 with this mode/scoring structure. New `neutral-permanents.md`. See D63 (what a "candidate" is scored over) and D64 (how candidates are ranked).

### D61 — Permanent creation identifiers: a shared timestamp per creation event, plus a unique, monotonic ID per permanent
Surfaced while designing how multiple Neutral permanents assigned to the same neutral turn should be ordered relative to each other (their Behaviors are triggered abilities per D56, and several can fire simultaneously at a neutral turn's start). Two related but distinct needs turned out to require two different identifiers, not one:

- **Timestamp** — assigned per creation *event* (one effect resolving, or one setup placement); every permanent produced by that one event shares it. This is the *card-text-facing* concept: "target creature that entered the Island before another target creature" compares timestamps, and two creatures from one spell (e.g. "put two 2/2/7 Soldiers into play") correctly have *equal* timestamps — neither entered before the other, so they can't be picked as an ordered pair. Getting this right mattered specifically because the alternative (a single always-unique counter, with no way to express "these truly happened at the same instant") would make simultaneous creation silently look sequential to card text that checks entry order — a real, avoidable modeling gap.
- **ID** — a separate, always-unique, monotonically incrementing counter, one per permanent, with no ties ever. This is the *engine-internal* tiebreak: it decides the order in which several Neutral permanents' simultaneous Behavior triggers go on the stack (oldest ID first onto the stack, so oldest resolves first — deterministic, and stable under board movement, unlike a board-scan-by-position tiebreak that was considered and rejected for exactly that instability). Since both counters are monotonic and effects resolve one at a time, sorting by ID alone is equivalent to sorting by (timestamp, then ID) — so the engine only ever needs to track and compare one number, while card text only ever needs the timestamp concept.

**IDs are true-state only, never player-visible**, and this isn't incidental — because permanents can be hidden (a concealed Below-level unit, a hidden Neutral permanent), a raw, globally-sequential visible ID would leak the *count* of things created between two visible IDs (a jump from ID 12 to ID 19 reveals six things exist that aren't visible). Per-observer views expose only *relative* order among permanents that observer already knows about; card text speaks only in terms of "entered before / earliest / latest," never a number.

**Left explicitly for implementation, with a stated interim default rather than a fully worked rule** (the detail wasn't worth blocking on): the ID order *within* one creation event when a card doesn't specify one ("put two Soldiers into play," "create a 1/1 for each card in hand" — both treated identically, one event → one timestamp, still-distinct IDs in whatever order the effect naturally enumerates its output); the setup-time default (provisionally: everything placed at game start shares one timestamp, so nothing on the starting board "entered before" anything else on it); and rapid re-entry/"flicker" (provisionally MtG's answer — leaving and re-entering the Island makes a new permanent, new ID, new timestamp — flagged as worth reconsidering later for flavor, since a more identity-preserving answer was floated but not designed).

→ new global concept; touches `neutral-permanents.md` (Behavior ordering) and, later, `architecture/continuous-effects.md` (also the natural timestamp for continuous-effect layering once that's built — no separate concept needed there either).

### D60 — Neutral turns are freestanding, full turns; two neutral seats are genuine turn-order participants
Resolves the D56-left-open phase-placement question. Two live options were weighed: nesting a Neutral Phase inside each player's own turn (before End), vs. keeping it freestanding between the two players' turns. Nesting looked attractive at first because it inherits turn scaffolding for free — an obvious active player, end-of-turn cleanup for any turn-scoped modifier a Behavior or a response creates, end-of-turn triggers that see the results. But it has one unrecoverable cost: it would make "during your turn" card text include your Neutral turn's actions by default, since the phase would structurally BE inside your turn, with no way to carve it back out without ugly boilerplate ("during your turn (but not your Neutral Phase)"). Freestanding keeps "your turn" meaning exactly your own Beginning/Action/End, full stop; a card that wants to react to what a Neutral permanent does must say "during a neutral turn" explicitly. That's the deciding factor: it keeps Neutral interaction strictly opt-in, matching D14's pessimistic-default philosophy — a new interaction surface shouldn't feed into existing triggers for free.

Freestanding then needs its own answers for the things nesting got for free, and this decision supplies them:
- **A neutral turn is a full turn, not a phase or an AP-refresh-only mini-turn** — Beginning (Activation Points refresh for permanents assigned to that seat, and a private mana pool refresh for any Neutral Companion assigned to it, same cadence a player's own Beginning already uses), Action (Behaviors run), End (end-of-turn triggers, and — the piece D56 left as a real gap — expiry of any "until end of turn" effect created during it). A deliberate design bet, made generous rather than lean on the user's explicit call ("I plan a massive amount of neutral stuff in some PvE games, and I want some colors to actively play with the creation of neutral permanents that annoy the opponent") — Neutral content is meant to be a first-class part of the game, not a lightweight afterthought.
- **Two neutral turns per round, not one, and they are NOT interchangeable slots.** With a single neutral turn, whichever player's turn immediately precedes it is structurally disadvantaged — a hostile Neutral permanent always reacts to that player's freshest board state, while the other player gets to react to the Neutral permanent's move before their own turn. Two turns, one on each side of the round (immediately after A's turn, immediately after B's turn), let the player who makes something Neutral choose which slot: the one that reacts to their opponent's fresh moves and resolves right before their own turn (generally the preferred slot for an "annoy the opponent" deck), or the other. This also directly motivates D56's requirement that both turns always occur unconditionally regardless of content — the mere existence/timing of a neutral turn must never leak whether a hidden Neutral permanent is present.
- **The two neutral turns are genuine turn-order participants (a "Neutral A seat" and a "Neutral B seat"), not a borrowed/persisted active-player hack.** An earlier proposal tried to avoid inventing new turn-order concepts by having a real player's "active player" status persist through the neutral turn that immediately follows their own turn (so APNAP/priority during that neutral turn just reused whichever player was already active). That works, but it forces every subsequent question (who orders simultaneous triggers when several Neutral permanents fire at once? does "active player" now mean something different from "whose turn it is," needing its own careful redefinition?) to be answered as a bespoke exception. The better move: treat the two neutral turns as two more seats in the game's turn order — round order `A → NA → B → NB` — and let every ordering rule the game needs (whose priority window is first, in what order do simultaneous triggers resolve) be just "the standard multiplayer rule, walking the turn order starting from whoever's turn it currently is," verified directly against actual Magic: the Gathering multiplayer rules (101.4, 603.3b: the active player's simultaneous triggers go on the stack first — and so resolve last — then each other player in turn order). This required zero special-casing once adopted, which is the signal it, not the persisted-active-player patch, was the right level of abstraction. `NA`/`NB` are turn-order participants only, never "players" for any other purpose — not targetable by "target player," not counted by "each player," no life/hand/library. Card text only ever refers to control as Player A / Player B / Neutral (D54); `NA`/`NB` name *when* a Neutral permanent acts, never who controls it.

→ `overview.md` §3, `neutral-permanents.md`.

### D59 — Zone/domain re-leveled to two tiers; "section" retired; board areas renamed "home ground" / "neutral ground" *(replaced parts in `decisions-archive.md`)*
Separately, a naming cleanup from the same pass: the map's **"home zone" → "home ground"** and **"neutral area" → "neutral ground"**. "Zone" now formally means an object container, so a board sub-area needed a different word; "area" also collided with **Area** (D43, the targeting primitive). The legacy alias **"home base" (D8) is retired** — no current-rules doc uses it after this pass. **Realm** (the terrain a Champion has actually bonded) is untouched.

### D58 — Activation Points: one resource, one payment rule, for every permanent but Item
Unifies "Action Points" (Actor) and "Activation Capacity" (Object, D47) into a single concept, **Activation Points**, held by **every permanent except Item**, each explicitly printed on its own card data — D47's silent "default 1" placeholder is removed; a card's printed value must be large enough to afford its own priciest self-funded ability, since capacity and funding are now the same pool. Values are deliberately kept comparable across types so a generic effect ("target permanent gains 2 Activation Points") reads the same regardless of what it targets.

**Payment is now one rule, not two:** to pay a cost of any resource, climb from the entity itself and stop at the **first** node (inclusive of the entity) that holds that resource. This single rule reproduces every previous special case as a natural instance rather than a bespoke exception: Activation Points and Life "never climb" simply because the entity itself is the first node checked and already holds them (generalizing D10's Actor-only statement to every permanent that carries these); mana climbs because only a Champion or Companion ever holds it, so the walk keeps going until it reaches one, exactly as before. An explicit card-level override ("this creature's controller pays 2 life") still works exactly as before — a deliberate instruction to skip past a resource-holding node, not something this general rule does automatically.

**Consequence:** an Object's own Activation-Points-costed ability (e.g. a Structure's printed activated ability) is now funded by the Object itself, not by whoever bonds/controls it — a genuine balance change, decoupling such abilities from the controlling player's own action economy. This is also what makes D56's Behavior scope work for Objects: an Activation-Points-only ability is always self-payable by a Neutral Object holding its own Activation Points, with no bonder required; a mana-inclusive ability still needs a live root for that portion. → `economy.md`, `ancestry.md`, `resources-terrain.md`, `structures-items.md`.

### D56 — Behavior: mechanism, scope, and Neutral Phases *(replaced parts in `decisions-archive.md`)*
Resolves the mechanical design of Neutral Permanents' Behavior system (`history/neutral-permanents-draft.md` §3–§10), correcting several points in the original draft text:
- **Behavior is card-text vocabulary, not a new mechanism** — a keyword (like Flying), internally a triggered ability keyed to "this permanent's assigned Neutral Phase begins" (D35's existing triggered-ability pipeline), never exposed as such on a card (same split as D50's Properties/Abilities).
- **Behavior scope is determined by capability, not by Actor/Object type**: any permanent that (a) holds Activation Points (D58 — i.e., anything but Item) and (b) has at least one legal action available to it may be assigned a Behavior by whatever effect makes it Neutral or spawns it directly as Neutral. This supersedes an earlier, narrower framing considered mid-session (Behavior limited to Creature/Companion only) — a Structure or Terrain is equally eligible once D58's self-funding rule is in place, provided the specific ability it would need is actually payable (an Activation-Points-only ability is always self-payable once Neutral; a mana-inclusive one still needs a live root supplying mana).
- **Target selection and mandatory reconsideration both reuse the standard legal-target query** (the same one any attack/ability already uses, D19/D46) — no separate "perception" or "belief state" system. A Behavior re-checks "is my current target still a legal target" after every action; this single check already subsumes the target dying, no longer satisfying its condition, or leaving visibility — replacing the original draft's vaguer "if the belief state changed unexpectedly" language, which is dropped as ambiguous (it would fail to trigger reconsideration on the fully-deterministic case of a Behavior's own attack killing its target). Since legal-target checks are inherently evaluated against currently observed/claimed properties (D18), a target's disguised true stats (Mimic) already produce natural, correct deception — the Behavior acts on the claim until an interaction (e.g. resolving an attack against it) tests it, exactly as a player would. No generalized "perception range" either — a keyword wanting a numeric range (e.g. "within distance 3") states it directly in its own text as a plain distance check layered on the standard legal-target query, rather than via a bespoke vision mechanic.
- When a permanent becomes Neutral via a card effect, that effect's controller chooses its assigned Neutral Phase. A **mission/scenario** makes this assignment when a Neutral permanent originates from PvE/mission content instead of a card effect (PLAN §9, not designed further here).
- **Creating/converting a Neutral permanent** requires the causing effect to name a Behavior only for a permanent capable of holding one (see scope above); one created or left Neutral with no Behavior specified simply sits inert, per the pre-existing "no one may activate an uncontrolled Object's ability" rule. Gaining control of a Neutral permanent ends its Behavior immediately. → new `neutral-permanents.md`.

### D55 — Neutral Companion; parent reassignment via card effects
Two amendments to the ownership tree (`ancestry.md`):
1. **A Companion is a root exactly when it is Neutral** — amending D26's "a Companion is never itself a root" invariant, which now holds only for a Companion controlled by a player. A card may make a controlled Companion Neutral (never automatic — see D54); everything already in its subtree (funded creatures, bonded terrain, its private mana pool) is structurally unaffected — only the controller-walk's resolution changes, from "climb past the Companion to its Champion" to "stop at the Companion itself, Neutral." This is a distinct root concept from the terrain-network root a Companion already has (`companions.md`) — the two graphs (ownership vs. terrain-network) are separate, and this amendment only touches the ownership one. A Neutral Companion still funds and acts within its own subtree exactly as before, just requires its own Behavior (D56) to decide what to do with its own AP/mana each Neutral Phase — a clean way to get a self-sufficient "band of monsters with their own economy" with no new machinery.
2. **A Creature's or Companion's parent may be directly reassigned by an explicit card effect** ("Gain control of target creature/companion") — amending the "resolved once, at creation, forever" language in `ancestry.md`, which remains the *default* but is no longer absolute. Companion's deck-legality gating (restricted to specific Champions, `companions.md`) is a deckbuilding-time constraint only and is not re-checked when control changes this way. → `ancestry.md`, `companions.md`.

### D54 — Neutral Permanents adopted; control unified to three states (A/B/Neutral) for every permanent type
Adopts the Neutral Permanents draft (`history/neutral-permanents-draft.md`), with the amendments in D55–D58, resolving D29's parked "what does an uncontrolled creature do" question. Every permanent's controller is exactly one of three values — **Player A, Player B, or Neutral** — derived by climbing its parent chain to the root, for every permanent type without exception. Previously this three-way result was only ever produced for Actors and Item; **Terrain, and Structure by inheritance, now fold into the same three values too, superseding D27's three-named-bond-states model** — "bonded and interrupted" is no longer a distinct controller value. Instead, the **bond record (parent) persists independently of the derived controller**: a terrain's parent link to its bonder is untouched by a blocked path, so the controller-walk simply fails to reach that bonder while blocked (resolving to Neutral) and succeeds again the instant the path clears, with no new Bond action needed. A **truly unbonded terrain has no parent at all**, also resolving to Neutral, but with nothing to auto-revert to. The distinction that mattered under the old "interrupted vs. unbonded" framing — auto-reverts vs. needs a fresh Bond — is preserved exactly, just carried by the parent/bond-record structure rather than by an expanded controller enum. A Structure inherits this for free (its parent is the terrain cell, unaffected) — no separate Structure-level state needed. Terrain's underlying pause/block *mechanic* (an enemy on the path denies mana) is completely unchanged; only its relationship to "controller" terminology is. → `ancestry.md`, `resources-terrain.md`, `structures-items.md`, new `neutral-permanents.md`.

### D53 — Home zone / neutral area population are map-level rules, each with a default
Populating a map's home zone and neutral area with actual terrain is itself a map-authored rule, not a hardcoded procedure — each region has a default, which a specific map card may override. **Home zone default: random** (the player's terrain deck is distributed randomly across their home-zone cells, unchanged from D8/D11). **Neutral area default: fixed assignment** (the map specifies exactly which terrain sits on each neutral hex). A map may instead specify an alternative population rule for either region (e.g. a home zone where each player first chooses one land to place as desired, then the rest fill uniformly at random). Deliberately not designed further at this depth — the space of possible population rules is a future content question, not a structural one now. → `../map.md` (new).

### D52 — Terrain Type: a new per-hex worldbuilding classification, decoupled from Element; map "form" renamed to Layout
Every hex on a map (home zone and neutral area alike) carries a **Terrain Type** — a purely worldbuilding/flavor classification with **no mechanical effect** (no move-cost, LOS, or capacity changes), authored by the map card itself, independent of whichever Element ends up there. Initial set of 8, open to extending or shortening: **Forest, Open, Wetland, Shore, Elevated, Settlement, Urban, Edge.** A hex's full name is its map-fixed Terrain Type combined with whichever Element lands on it (e.g. Forest+Fire → "Fire-adapted forest"); specific combination names are not locked. **Every terrain card carries only an Element, never its own Type** — Type always comes from the map slot, with no exception for non-basic ("fancy") cards, keeping the model uniform. This is what lets a map carry a consistent authored identity (a city map, a forest-choked map) regardless of which Elements the two players bring. **Naming collision resolved:** D11's map-card property "form" (which cells exist + connections) is renamed **Layout**, distinct from Terrain Type. → `../map.md` (new), `../overview.md` §2, `../resources-terrain.md`, `../glossary.md`.

### D51 — Elements named: the 8 colors are Light, Fire, Metal, Earth, Darkness, Ice, Water, Air
Fills in D8's previously-unnamed "8 colors": mana colors are named **Elements** — **Light, Fire, Metal, Earth, Darkness, Ice, Water, Air**. A terrain card carries exactly one Element; nothing else about a card's identity comes from this. Element identities and relationships to each other (a color-wheel pass) remain undesigned — see `PLAN.md`. → `../resources-terrain.md`, `../glossary.md`.

### D50 — Trace Duration default; Properties/Abilities is card-text vocabulary only
Default Trace Duration: 5 rounds in the Past before a trace vanishes (placeholder, real number deferred to tuning), with `Duration X` as a card-level override — fills D16's previously-unspecified "configurable window" with a concrete shape. Separately: **Properties vs. Abilities** (Properties = plain stat changes like Power/Life/AP; Abilities = named granted capabilities) is adopted as **card-text/readability vocabulary only** — it does not change removability mechanics. The general principle stays "an effect lives as long as its vessel does" (a Trace, or a Creature with a static ability granting it) — the same source-tied Modifier pattern already in `../../architecture/continuous-effects.md`; nothing is ever targeted as an independent "buff object," removal always goes through removing the source. Whether a plain stat buff bakes directly into the stat vs. becomes a full removable ability-object is left an open implementation choice, for playtesting to settle. → `../overview.md` §1, `../economy.md`.

### D45 — Speed system (Slow/Quick/Reactive/Instant) replaces D5, D23, and the enumerated priority-window list
**Supersedes D5** (card-level instant/sorcery) **and D23** (`^` per-ability marker); retires `../interaction-stack.md`'s enumerated "priority opens at these events" list. Every card/ability has one of four speeds, gated by the live contents of the Aether's Pending section:
- **Slow** — controller's main phase only, and only while Pending is empty. (≈ today's sorcery-speed.)
- **Quick** — whenever only **Physical Traces** are in Pending. A Physical Trace is the trace left by Move, Attack, Ascend, or Descend (confirmed set; extensible). Physical Traces still enter Pending (respondable — a real priority window exists) but on resolution are **removed from the Aether entirely, zero Past residency** — bypasses Trace Duration (D50) rather than using a short number, keeping the Past meaningful despite how often these fire.
- **Reactive** — whenever no **Instant Trace** is in Pending. Kept even though currently vestigial (see Instant) — a robustness safeguard for a possible future card that makes an Instant linger.
- **Instant** — playable any time, and **resolves fully atomically**: enters Pending and Now immediately advances past it as part of the same action — zero duration, no response window for anyone, not even another Instant. Deliberate: keeps the top speed tier free of MTG-style unbounded stack/priority-passing complexity (the single largest AI/netcode cost `../interaction-stack.md` already flags), while Quick/Reactive still provide real interaction against everything below Instant. The real Reactive-vs-Instant difference is what happens *after* playing, not *when*: Instant resolves with no response window; Reactive goes through the normal Pending/response process.
→ `../economy.md` (remove the `^` marker section), `../interaction-stack.md` (major rewrite), `../overview.md` §1/§3, `../glossary.md`.

### D43 — Areas: distance/line/listing hex-set primitive
A formal primitive for an effect's target hex-set: **distance** (all hexes within N), **line** (hexes along a line through specified hexes), **listing** (an explicit hex list). Closed set for now — more shapes added later only as specific cards need them. Unless stated otherwise, an area describes hexes, not occupants. Fills a real gap (AoE targeting was previously unformalized). → `../overview.md` §2.

### D42 — Subterranean bundles Underground access and concealment; adds Ascend/Descend
A Unit needs the **Subterranean** keyword to exist Underground at all — access is gated (pessimistic default, D14), closing a real gap (nothing previously specified how a unit gets Underground/Below). Subterranean and concealment are one package: the keyword buys both Underground access *and* the existing structural hidden-by-default treatment Below already had (D12). Subterranean is the **default** gate, not an absolute wall — a specific card effect can still force a non-Subterranean unit Underground as an explicit exception. New abilities: **Ascend** (`3AP`, Underground → Surface) and **Descend** (`3AP`, Surface → Underground). → `../overview.md` §4, `../asymmetric-information.md`, `../glossary.md`.

### D39 — Supersedes D17: "Spell" no longer needs to be an umbrella; "Rite" no longer needed as a separate name
D17's "Spell = umbrella for anything cast through the Aether" is dropped, and with it the reason "Rite" existed as a separate name for the one-shot type (it only needed a distinct name to avoid colliding with "Spell" as umbrella). Once the domain split (D37) made the Card Zone and the Aether into explicit, separate zones, **each already has its own natural umbrella term, so "Spell" isn't needed for that job anymore:** in the Card Zone, the umbrella for any card, of any type, is simply **"card."** In the Aether, the umbrella for anything resolving there is simply **"trace"** (or, if the card that produced it needs stating, "trace from a card"). With no umbrella left for "Spell" to mean, it's free to directly name the one-shot type — **Rite is retired, "Spell" is that type's name**, full stop. Card types: Creature, Companion, Spell, Structure, Item. → `../glossary.md`, `../structures-items.md` + `../companions.md` (reword "cast through the Aether like any Spell," which no longer parses — say "cast like any card" instead), code (`CardType.Rite`/`CastRiteCommand`/`RiteEffectIds` rename — implementation follow-up, not yet done).

### D38 — Aether restructured: three Sections (Past / Pending / Future) plus Now, a moving point, not a Section
The Aether gains a third Section: **Past** (resolved traces), **Pending** (the "immediate future" — next to resolve; functionally today's stack), and **Future** (traces already instantiated but scheduled later than "next" — genuinely new: enables delayed effects and a re-cast pushed further out than the very next slot, not redundant with Pending). **`Now` is explicitly not a Section** — it's the moving point dividing Past from Pending. **Clarified 2026-09-05:** `Now` advances past whatever's next in Pending when both players pass priority in succession (the standard "everyone's done, resolve the top thing" trigger); an Instant is the sole exception — it advances `Now` past itself immediately and atomically as part of being played (D45), never waiting for both players to pass. → `../overview.md` §1, `../interaction-stack.md`, `../glossary.md`.

### D35 — Triggered abilities are a fundamental ability kind, universal and independently costable
Triggered abilities ("when X happens, do Y", `../interaction-stack.md`) are not a Structure-specific pattern — they sit alongside **activated** abilities as one of the two fundamental ability shapes, available to any permanent or actor regardless of Type: a Creature, Structure, Champion, or Companion can carry one directly, and an Item can grant one to its carrier via equip (same funding rules as any granted ability, D26). The two shapes differ in when a cost, if any, is paid: an **activated** ability's cost is paid upfront, by choice, to put it on the stack at all; a **triggered** ability goes on the stack automatically the instant its condition is met, with no activation choice — but it can still carry its own cost, paid **at resolution**, typically as an optional "you may pay" clause (e.g. "whenever a creature enters within distance 2, you may pay 2 mana; if you do, draw a card") rather than an upfront requirement. Funding for that resolution-time cost follows the ordinary ownership-tree payment walk (D26) exactly like any other cost; only the controller (D28) decides whether to pay. → `../interaction-stack.md`, `../structures-items.md`.

### D34 — Item has no Life; loose Items are exempt from layer capacity
Unlike Structure (D24, attackable, has Life), an **Item never has Life** — it's never a combat participant (no Attack, no AP, never a legal attack/ranged target the way a Structure or creature is), so a Life total would only matter for hypothetical generic-damage/AoE destruction, which nothing requires. An Item is removed from play only via an explicit printed effect (e.g. "destroy target Item", a sacrifice cost) — never by combat damage. Separately: a **loose Item does not consume any of a layer's 3-creature capacity** (D12) — layer capacity is a creature-only cap; any number of loose Items may share a (terrain, layer) address (exact board visualization TBD, not a rules concern). → `../structures-items.md`.

### D33 — Casting/materialization via the Aether: target locked before resolution; illegal target fizzles per-instruction, not a whole-spell counter
Casting a Creature, Structure, or Item follows one procedure: choose the spell → choose target terrain + layer → pay cost → the spell goes into the **Aether** (a trace ahead of the stack's `now` marker, `../interaction-stack.md`, D16) → when `now` reaches it, it resolves: the Aether trace stays put and its resolution creates a separate, linked permanent at the chosen (terrain, layer) — not the trace itself relocating (D16, refined 2026-08-11). Because the board can change while a spell sits in the Aether (opponent responses, contested terrain, a layer filling up), resolution re-checks legality. **If a chosen target is no longer legal, that specific instruction fizzles — the spell does not resolve that part — rather than the whole spell being countered MTG-style.** For a single-target permanent cast (today's only case) this looks identical to a counter in outcome, but the principle is general: a future multi-effect spell with several targets only loses the parts that went illegal, not the whole effect. Paid cost is never refunded either way. This is a general Aether-resolution rule (relates to D16), not specific to Structures/Items. → `../structures-items.md`, `../interaction-stack.md`.

### D32 — Location model unified across Creature/Structure/Item; layer is card-derived, not a free choice *(burrower mapping corrected by D90)*
A permanent's board location is uniformly **(terrain cell, layer)** — Creature (already true, D12), Structure (D31), and now a **loose** Item (closes the open "where does a loose item live" question from D25): a dropped item lands exactly where it was dropped, layer included (an underground creature's dropped sword ends up at (that cell, Below), hidden by the same rule that already hides Below creatures). An **equipped** Item's location is instead its carrying actor (D26) — not spatial at all, since it travels with that actor. **Legal layer(s) at cast time come from the card's own data/keywords, never a free choice by default**: in the overwhelming majority of cases exactly one layer is legal (no keyword → Ground; Flying → Above; a burrower-type keyword → Below), matching the pessimistic-default posture (D14) — a card may explicitly print a choice among multiple legal layers as opt-in flexibility, but there's no generic "pick any layer" capability. Applies identically to Creature and Structure casts. → `../structures-items.md`.

### D30 — Un-equip joins the default-action set, free and uncapped; Zone of Control rejected as a default
**Un-equip** is a new default ability every eligible actor (Champion, Companion, Creature) carries alongside Move/Attack/Defend/Equip — `0AP`, completely free, no once-per-turn cap (unlike Defend, D15, there's no known balance reason to restrict it). Drops a carried Item back to loose on the actor's current hex, removing the actor as its parent (D26). Closes the open item flagged in the Structures/Items design pass. **Separately, Zone of Control** (does a creature block/threaten enemy movement through its adjacent hexes) — surfaced in the same discussion, considered, and **rejected as a default**: handing every creature a passage-blocking capability for free is exactly the generous-default pattern D14 warns against (already flagged as a candidate in `history/pessimistic-default-audit.md` before this session). If added at all, it's a **positive keyword** on specific creatures, never a base rule. → `../ancestry.md`, `../structures-items.md`, `history/pessimistic-default-audit.md`.

### D29 — Companion-death controller loss is derived, not a choice; real open question is uncontrolled-creature behavior
Corrects D27's framing. Whether a creature loses its **controller** when the Companion that summoned it dies was never actually an open design choice — it falls directly out of D28's definition: if climbing the parent chain doesn't reach a live Champion, there is no controller, full stop, no special case needed. **The genuinely open question is what an uncontrolled-but-still-living creature actually does.** Terrain and Structure are inert, so "uncontrolled" just means nobody may activate them — but a creature still holds and refills its own AP (D10, always self-paid, never climbed regardless of controller), so it isn't obviously inert the same way. Not designed here: whether it can still act at all, and if so, who/what directs it. **This opens a larger, deliberately-parked design space:** mission/PvE content with NPC-like creatures running an AI-driven routine, independent of either player — an uncontrolled creature (e.g. one orphaned by a Companion's death) as one possible source of such an NPC, with killing one as a potential bonus objective. Parked in `PLAN.md` §9, not designed now. → `../ancestry.md`.

### D28 — Terminology correction: "controller" is a strict, Champion-only walk; "payment" is a separate, shorter one
D26/D27 used "controller" loosely for two different things: (a) whoever actually pays a cost (which can be a Companion), and (b) whose side a permanent is on. These are genuinely different walks over the same parent chain and need different names. **Payment:** climb from the entity itself and stop at the **first** node holding the needed resource — AP/Life stop immediately (self, D10); mana stops at a Companion if one is found before reaching the Champion, since a Companion is the one non-Champion node able to hold a resource at all. **Controller:** climb **all the way to the top**, regardless of what's held along the way. **A Companion is never itself a root** — it always has a live Champion parent for as long as it's in play — so this walk, if it resolves, always resolves to a Champion. **"If the root is a Champion, that Champion is the permanent's controller"** — stated conditionally, deliberately not asserting the climb always succeeds (that's what D27's open ToDo is about: whether it still resolves through a dead intermediate). Consequence worth naming: a permanent's payer and controller can differ — anything rooted through a Companion is *paid for* by that Companion but *controlled* by the Champion who cast it. A card costing "this creature's controller pays 2 life" therefore always hits the Champion's Life, even for a Companion-summoned creature — intentional, not an edge case. Also corrects D24's/`../resources-terrain.md`'s looser "terrain/structure is controlled by whoever bonds it (Champion or Companion)" language: that's the **payment** relationship, not the strict controller — a Companion-bonded terrain/structure is *funded* by that Companion but *controlled* by that Companion's own Champion. → `../ancestry.md`.

### D27 — Ownership tree refined: no creature-parents-creature; terrain gets three bond states; inner-node removal uncontrols successors *(the three-bond-states model is superseded by D54 — see that entry)*
Amends D26 after further discussion. **Creature is always a leaf for creation purposes:** when a creature's own ability produces another creature or a structure, that new permanent's parent resolves **directly, once, at creation** to whichever Champion/Companion recursively pays for it — never to the creating creature. A creature remains a valid parent for **Items** it carries (a distinct "carrying" relationship, not creation/funding), so a creature's death still orphans its equipped items (**confirmed**: they become ownerless, reverting to loose). **Terrain now has three states, not two:** bonded-**active**; bonded-**interrupted** (D8's existing enemy-on-path "pause," now formally named — behaves as "no parent" for every downstream purpose including Structure control, reverts to active automatically once the path clears, no re-Bond needed); and **unbonded** (the link is actually gone, needs a fresh Bond). **New ways to reach unbonded**, beyond the existing Companion-death cascade (D22): the Champion's own voluntary choice to drop one specific bond (a granular sibling to the existing all-at-once Collapse Network, D9); "unbond from target land" as an additional cost printed on a card (new cost primitive); and a hostile targeted effect ("target Champion/Companion unbonds from target land") as a new attack-the-economy effect archetype, distinct from D8's positional pause. **General rule:** removing (or interrupting) an inner node uncontrols all its successors. Confirmed for Terrain→Structure (deliberate — opens area-control fights over contested terrain, interrupting or severing a bond either way cuts a Structure loose). **Open ToDo, explicitly not decided:** whether the same applies to Companion→Creature — i.e. does a creature lose its *controller* (not just its funding root) when the Companion that summoned it dies, or does control survive via a static link even though funding breaks. → `../ancestry.md`.

### D26 — General ownership tree: every permanent has one parent; costs climb to a controller
**Supersedes the funding specifics of D24/D25** with one general mechanism. Every entity other than a Champion has exactly **one parent** — Companion → the Champion that cast it; Creature → whoever/whatever summoned it (Champion, Companion, **or another creature**, transitively); Terrain → whoever currently bonds it (dynamic, unchanged from D8/D22); Structure → the terrain it occupies (one hop removed, inherits Terrain's dynamism for free); Item while equipped → its equipping actor; Item while unequipped → **no parent, and no usable ability at all** (see below). Walking parents upward always terminates at a **controller** (a Champion or Companion). **Paying a cost:** AP and Life are held directly by every actor/creature (D10) and are always **self-paid**, never climbed — this is why AP needed no bespoke exception in the original framing, and why Life resolves the same way (a creature's "pay 2 life" spends its own Life, not its controller's, unless a card explicitly names the controller as payer). Every other cost type (mana, discard a card, sacrifice a permanent) **climbs the parent chain live** until it reaches a controller, who pays from its own pool (shared for the Champion, private for a Companion). **Live walk, not a cached snapshot:** a break anywhere in the chain (not just the topmost root) orphans everything below it from non-AP/non-Life costs. **Only Champions cast spells** (no Hand on a Companion) — this alone explains why a discard cost printed on a Companion's ability would climb past it to the Champion, no special case needed. **Amends D22:** "a Companion can't share mana" is narrowed, not repealed — a Companion funds its *entire subtree* (creatures/terrain/structures/items rooted to it) from its private pool; it still can't fund a *different* controller's own abilities directly. **Item's funding resolved via a generic ability, not a DAG:** every eligible actor gets `2AP: Equip target Item sharing this location` (tuning baseline); combined with unequipped items having zero usable abilities, this keeps the whole model a tree (an earlier multi-parent/DAG draft for loose items was considered and rejected as unnecessary complexity) — equip is removable/re-priceable per creature (pillar 5), stealing is a costed action via the same ability, and exclusivity (one holder at a time) falls out for free. **Open:** what happens to an equipped Item when its carrier dies (presumed: reverts to loose, unconfirmed); whether chain-breaking should be a live walk or a cached root captured at creation time (flagged as an extension beyond what was explicitly stated). → `../ancestry.md`, amends `../companions.md`, `../structures-items.md`.

### D25 — Item: equip is an explicit AP action; funding follows the carrier
Resolves the "Item" open in D17/`../economy.md`. **Equip is an explicit AP-costed action**, not automatic-on-move. **Superseded/refined by D26:** the equip mechanism turned out to be a **generic ability** every eligible actor holds (`2AP: Equip target Item sharing this location`, tuning baseline), and an equipped Item's funding is just the ownership-tree default (D26) applied to its parent — the equipping actor's own pool. This also resolved exclusivity (one parent = one holder) and avoided a multi-parent/DAG model for loose items. Still open: where a loose Item sits spatially, un-equip/drop mechanics, and what happens to an equipped Item when its carrier dies. → `../structures-items.md`, `../ancestry.md`.

### D24 — Structure: dedicated per-hex slot; control mirrors Terrain's current bonder, not the builder
Resolves the "Structure" opens in D17/`../economy.md`. **Occupancy:** a Structure gets its own dedicated **per-terrain-cell slot, capacity 1** — separate from the Ground/Above/Below layers (D12), so it never competes with creatures for room (visually centered on the hex). **Cast like a Creature** (D20 pattern): onto a bonded cell in the caster's realm with a free Structure slot, paid in shared-pool mana. **Attackable, has Life**, 0 Life → grave/ruins in Play (D14/D16 pattern); "indestructible" is a keyword, not default. **No default effect on movement/LOS** — a specific card (e.g. a Building subtype) may print that it blocks either, as cell-property data. **Control is dynamic, not fixed at build time:** a Structure's controller is whoever **currently bonds** the terrain cell it sits on — the exact same rule as Terrain control (D8/D22), not a separate ownership state. Deliberate consequence: contested/newly-unbonded terrain can be re-bonded by the opposing root, **capturing the Structure positionally** without attacking it — fits D8's "denial is positional and reversible" shape. **Funding:** the controlling root's own pool (shared/private per D22), same split as Terrain abilities. **Default cost is mana-only** (mirrors Terrain — a Structure has no AP of its own, activated "like casting a spell"); a specific printed ability **may** additionally cost the controller's own AP as card-level data (pillar 5) — this same flexibility now also applies to Terrain's own abilities (amends the D8 terrain-abilities note, which read as strictly mana-only). **Generalized by D26:** "control follows the current bonder" turned out to be one instance of a general ownership-tree rule (a Structure's parent is the terrain it occupies; the terrain's own parent is whoever bonds it) rather than a Structure-specific mechanism. → `../structures-items.md`, `../resources-terrain.md`, `../ancestry.md`.

---

### D22 — Companion: a new card type between Creature and Champion; second network root
**New card type**, deckbuilding-gated to specific Champion(s). Makes the game's channeling hierarchy explicit: **Creature** spends mana but can't draw it; **Companion** draws mana (bonds terrain) but can't share it — it keeps a **private mana pool**, spendable only on its own printed abilities; **Champion** draws *and* shares (D8/D9 unchanged). Shape: Creature's three numbers (Attack/Life/AP, D10) **plus** a private mana pool **plus** an AP-costed **Bond** ability (`x*AP`, same cost flavor as the Champion's `2*AP` Bond, D9 — priced more restrictively against the Companion's smaller AP total, so bonding crowds out most of its turn). Cast like a Creature (D20: drawn, paid in mana, summoned onto a bonded terrain). **Extends D8 rather than replacing it:** a Companion in play is a **second network root**, bonding terrain reachable from itself via already-bonded terrain, same reachability/pause mechanics as the Champion's root. **Ownership follows the bonder** — whichever root (Champion or a specific Companion) performs a bond determines which pool that terrain feeds (shared vs. that Companion's private pool); **a Companion's death un-bonds its own terrain outright** (reverts to unbonded, re-bondable later by any surviving root — same "undo, don't pause" vocabulary as un-summon, D16), since a Companion's bond is only as permanent as the Companion itself, unlike the Champion's (D8). Mana never crosses the shared/private boundary in either direction — this also governs **terrain control**: a terrain is controlled by whichever root bonded it (or by no one, if unbonded), and any of that terrain's own printed activated abilities can only be funded from the pool that node feeds (shared for Champion-bonded, that Companion's private pool for Companion-bonded). See `../resources-terrain.md`. Move/Attack are also **network-dependent costs**, mirroring the Champion's realm-constraint shape (D9): cheaper once the Companion's own connection collapses, pricier while it stays connected. **Invariant preserved:** still exactly two resources, mana + AP (economy.md) — a Companion adds a second *pool instance* of mana, not a third resource. **Open:** deck/board count limits, whether Companions gate faction identity, meta-progression shape (parked to the meta-layer, post-M4); also surfaced a pre-existing table/prose contradiction in D9's Champion Move-cost description that needs a fix. → `../companions.md`.

---

### D21 — Turn structure: Beginning / Action / End; per-action response windows
**Beginning of turn:** mana/AP **refresh** (covers creatures and the Champion alike, D9) — **simultaneously, no state checks**; then the active player puts their beginning-of-turn triggers into the Aether, then the opponent theirs (**APNAP** order → opponent's resolve first, LIFO). **No priority in this phase to start** (simple); active-player *choices* (upkeep costs) still work — only *opponent-reactive* upkeep needs a window, added later as a **data change** (phases are data, D3). *(Card draw is no longer an automatic Beginning-phase step — as of the D9 revision it's the Champion's `5*AP: Draw` action, taken in the Action phase.)* **Action phase:** active player plays spells, spends AP to move/attack/draw/bond/activate, in any order, until they choose to finish; **after every active-player action — including a move and including a deliberate pass — the inactive player gets a response window** (refines D5: windows now open per-action, not only on stack/Combat events; enables movement-triggered traps; kept fast by auto-pass + sorcery-speed defaults). **End of turn:** end-of-turn triggers, APNAP, same as beginning. **Refresh (not bank) resolves D8 open #4.** Remaining tuning: starting hand size, hand cap, mulligan. Deck **legality = format** concern (1v1), not core. → `../overview.md` §3, PLAN §2.

### D20 — Summoning: onto a bonded terrain in your realm; mana cost on card
A creature is summoned onto a **bonded terrain cell within the player's realm/mana network** (so the **realm is the deployment zone as well as the economy** — the summoning frontier creeps with the realm). Cost = the card's **mana** (a plain mana-economy play, unrelated to the Champion's own AP, D9), payable multiple times/turn as mana allows. Constraints: a **free slot on the creature's layer** (D4/D12). **Open (rec pessimistic, D14):** **summoning sickness** — a summoned creature **cannot act the turn it enters** by default; "acts immediately" is a positive (haste-like) keyword. → `../overview.md` §4.

### D18 — Belief-consistency model for asymmetric information (the hard core of pillar 6) *(faces and claims recast as Pretend, D150)* *(replaced parts in `decisions-archive.md`)*
**Principle:** *a soft claim is cheap and holds until a hard fact tests it, then the truth is applied and the lie collapses.* The engine **never fabricates a consistent lie** (rejected: regress/unbalanceable/unauthorable); **cards author claims**, the engine projects them. **No deduction engine** — the engine guarantees hard facts are truthful and enforces a **small, finite set of conservation laws** (e.g. *claimed mana costs this turn ≤ public network max*, card-count conservation) that auto-collapse mechanically-impossible claims; **all other deduction is the human's** (UI surfaces hard facts).
**Resource border (a principle, not a list):** *standing state public; the private-resource ledger hidden; any cost with a board footprint self-revealing* ⇒ **a spell is mimickable iff its whole cost is payable from the hidden mana ledger.** Defaults: mana **network public** (needed for denial, D8), mana **spend/live-balance hidden** (engine of cost-deception + "mana for a trick?" bluffs), **hand size public** (count; contents private), **AP public** (max & current — surprises come from instant-speed *timing*, D5, not hidden state), footprint costs (sacrifice/unbond/discard) **self-revealing**. Only standing quantity hidden = **live mana balance** (minimal carve-out from D7). Interacts with D8 #4 (bank vs refresh).
**Representation:** the atomic primitive is a **`face`** — a per-observer overlay (claimed identity + stat modifiers) a **permanent** carries; this **unifies** creature-mimicry (enter *with* a face) and spell-mimicry (an effect *applies* a face), with the card's trace showing the same face. Composable combinators — **`ChooseOne[…]`** (modal, private caster choice → true state) and **`Mimic(face)`** (applies a face over an object's whole Aether life — stack + trace, hiding true identity). Key capability: **events/effects carry a per-observer projected form, not just objects** — and since `face` is itself an effect, projecting it **mutates the opponent's view state, board objects included** (a buffed creature carries a *true* modifier + a per-observer *claimed* modifier: **modifiers can be per-observer**). Trace and board stay consistent (one authored `face`); the divergence rides along until a hard fact collapses it. Honest and bluff casts of the same Mimic card are **indistinguishable** to the opponent. **Open:** default spell-resolution visibility (lean Model 2: true by default, Mimic overrides). → `../asymmetric-information.md`, `../overview.md` §9.

### D14 — Design principle: pessimistic (additive) defaults
**A rule's default is the weakest *still-functional-and-fun* case; cards improve on it via *positive* lines.** Keywords read as granted abilities, not restrictions — negative lines ("may only defend once," "cannot X") are dispreferred as defaults (they feel worse), though allowed occasionally for flavor/balance. **Guardrail:** "worst case" = weakest-but-complete, never literally crippled — the base game must stand on its own as a satisfying game. Companion to "simplest rule that still exposes a hook." **Corollaries decided now:** (a) **Damage persists** across turns; **no automatic end-of-turn healing** — healing is a special ability/effect (fixes the MtG behavior the user dislikes); Life is persistent state, 0 Life → destroyed. (b) **Defend-once is the pessimistic default**; *multi-defend* is a **positive keyword** (Bulwark/Guardian-type), while `cannot defend` remains an occasional negative keyword (D10). → `../overview.md` header + §4; memory `pessimistic-defaults`.

### D13 — Combat damage: mutual, simultaneous, attacker-assigns, gang-up
Accepted package. Scope: sequential → **one attacker vs a hex's declared defenders** now; many-to-many rides the deferred phased scheduler (D3). **Invariant:** a Combat resolves into **directed damage events** (`attacker → target(s)`, `target → attacker`), each independently preventable/modifiable/redirectable (pillar 5) — so Ranged/First Strike/Trample/retaliation-immunity are modifiers, never new rules. **Defaults:** (1) **mutual** — attacker and defenders damage each other (Ranged = "takes no retaliation" keyword); (2) **simultaneous** — both sides' damage computed from the pre-combat state, so "both die" is possible (First Strike = opt-in sequencing keyword); (3) **attacker assigns** its Attack across the declared defenders (engine exposes a damage-assignment decision point); (4) **gang-up** — every declared defender deals its Attack to the attacker. **Undefended** (zero defenders): attacker assigns its Attack among targets of its choice on the hex (creatures, or the Champion → win path), no retaliation. Deferred as keywords/tuning: Ranged/First Strike/Trample/numbers, and **cross-layer targeting** (D12) = the next combat sub-question. → `../overview.md` §4, PLAN §3.4.

### D12 — A hex has three vertical layers (ground / air / sub), each capacity 3
Completing D4's "reserved corners." A hex holds **three occupancy layers** — **ground**, **above (flying)**, **below (submerged/subterranean)** — each with capacity **3** (the tactical-vs-crowding compromise: enough to guard key creatures, not a scrum). The engine represents a cell as three distinct occupancy spaces; adjacency/LOS/combat are potentially per-layer (details deferred). **The below layer is hidden by default** (pillar 6): submerged occupancy appears only in its owner's view; the opponent sees nothing there unless Detection/reveal applies. **Reveal / view-merge is an open perception question** (how a "reveal" spell merges the hidden sub-view into the opponent's view — scope, duration, granularity). Fits the existing per-observer perception layer cleanly (reveal = a visibility-modifier), so it is *structurally* supported already; the mechanic details are content. → `../overview.md` §2, `../asymmetric-information.md`.

### D11 — The map is a fourth match component: a card, sized to the terrain deck
A legal match loadout is **four** things: card deck, **terrain deck**, Champion, **and a map** (all legality-constrained). **Maps are a strategic choice and come in sizes; map size determines terrain-deck size** (map legality is tied to size). A **map card** specifies: (1) **form** — which hex cells exist and their connections; "holes" are **void/aether** terrain (present as data with special functionality, not absent cells); (2) both players' **starting positions**; (3) each player's **home zone** around their start, sized to **exactly the terrain-deck size**, filled **randomly** with that player's terrain deck at game start (this *is* the D8 "home base"); (4) a **neutral area** with a predefined terrain-generation procedure (usually fixed basics, possibly randomized); (5) optional **landmarks** with special rules (e.g., first creature to enter → its Champion draws). The engine consumes **one resolved map** as data (consistent with cell-as-data). **Open (pre-game, deferrable):** in 1v1 both players bring a map — the selection/ban mechanism to pick the one played. → `../overview.md` §2, `../resources-terrain.md`; resolves D8's home-base + terrain↔hex + terrain-deck-size questions.

### D10 — Creatures run on Action Points; mana is one shared pool
**Mana is a single shared pool per player** (channeled by the Champion, D8/D9) drawn on by the Champion's spells/abilities **and every creature's mana-abilities** — this is what makes the Champion a "channeler." **Creatures have a second, private economy: Action Points (AP).** A creature spends its own AP to **move, fight, and activate abilities**; abilities may cost a mix of `mana + AP` (Champion abilities cost the same shape, `mana + AP` — see D9). **A creature is three numbers: Attack / Life / Action Points.** AP **subsumes** the old Movement stat (moving costs AP, rec 1/hex) and the "move-once/attack-once" rules (both are AP purchases → multi-attack/hit-and-run are emergent); **Range demotes to a keyword.** The repeating pattern across the game: *every actor = shared mana + a private AP budget.* **Champion↔AP (confirmed, D9 revised 2026-08-06):** the Champion has its own AP budget and spends it directly, exactly like a creature — one combat/action model for all entities, no gating resource. **Move/attack are default *abilities*, not rules:** every creature has replaceable `1AP: Move` and `3!AP: Attack`. **The `!` cost:** `xAP` spends exactly x; **`x!AP`** requires x then consumes all remaining AP — so `3!` attack = no multi-attack emergently (a non-`!` attack would allow it). **Defending is free**; `cannot defend` is a common keyword, not a default (avoids disincentivizing attacks / board clog). Remaining tuning levers: AP refresh/carryover, Champion base AP. → `../economy.md`; rewrites `../overview.md` §4.

### D9 — Champion runs on mana + Action Points, like a creature; killing it wins *(replaced parts in `decisions-archive.md`)*
**Revised 2026-08-06** — supersedes the original one-per-turn "Channel" model (kept below for history). The Champion is **not a bespoke third resource model**; it runs the **same two-resource shape as a creature** (D10): **mana** (spent on spells/units, many per turn, shared pool) and its own **Action Points** (private, refills each turn), spent on **draw a card, bond a terrain, move, fight, and activate abilities**. Mechanically the Champion is a special king-like creature — exactly one per side, its death ends the game — not a second combat model (pillar 5). Baseline example costs (tuning, not locked; full ability list still undesigned): `5*AP` draw (replaces the automatic per-turn draw), `2*AP` bond (the D8 once/turn bond limit, now expressed via the new `*` once-per-turn cost flavor — see `../economy.md`), `2AP`/`1AP` move (network-active/collapsed, revised below), `5!AP`/`3!AP` attack (network-active/collapsed — drain-all like a Creature's default attack), `2–5AP` abilities. **Tension** is an open resource-allocation puzzle (`move+fight`, `draw+bond`, `bond+move×2`, …) rather than a hardcoded ternary choice; the classic ramp-vs-spend tension still falls out of the numbers (`Draw+Bond` = the 7 AP baseline). **Win condition unchanged:** killing the enemy Champion wins (Duelyst-style); no separate Base; the Champion is attackable, protected only by units and by staying deep in its realm. **Open:** the Champion's actual signature-ability list, exact AP costs, and how Champion defense/retaliation works (ties to D15) are undesigned — this decision fixes the *shape* of the economy, not its content. → `../champions.md`; resolves win-condition + Base-merge keystones and overview.md §7/§10 opens.

**Movement/realm rule — revised again, tightened same day, under test (2026-08-08).** The "cheap move collapses the network" framing above is superseded. The Champion is now a **hard lock-in** while connected: `2AP` move, restricted to hexes that are **themselves already bonded** (not merely reachable through the network — an earlier same-day draft allowed that and playtesting immediately caught it: "I can still move outside of the network"); to move elsewhere it must first spend `0AP` on the new **Collapse Network** ability, which drops every bond outright (not a reversible pause — they're gone, re-bond from scratch), after which it moves freely at `1AP` like any creature. A Champion that never bonded, or is currently fully blockaded (D8's reversible pause, untouched by this change), already counts as disconnected and so is never rooted. Flagged "under test" pending playtest — may be revisited if the lock proves too punishing or Collapse too free. → `../champions.md` §"The Champion action economy," realm constraint.

### D8 — Resource system: terrain deck connected outward from the Champion *(replaced parts in `decisions-archive.md`)*
Separate terrain deck (size varies by format), laid out randomly in the home base at start. Up to once/turn, connect one terrain reachable from the Champion via already-connected, unblocked terrains — a spatial, deterministic ramp that eliminates draw-based mana/color screw. 8 colors (8 unrestricted basics + restricted non-basics). The **Champion is the network root** (answers "is the Champion the resource engine?" → yes). Denial is positional ("put the connection on hold"), reversible, and counterable. **Severing rule RESOLVED to (a) pause:** a Champion draws mana from a bonded terrain only while a path of enemy-free terrain connects it back to the Champion; an enemy on any path node **pauses** downstream mana reversibly (kill/reroute to restore). Bonding itself is permanent — only the *draw* is conditional. → `../resources-terrain.md`. Supersedes the placeholder "Model C." **Complexity note:** this is a deliberately rich (non-simple) core system; keep the rest simple to compensate.

### D6 — Traps are supported, as a special case of the stack
A trap = a pre-committed, **hidden triggered ability** (pillar 6 hides the pre-commitment) that goes on the stack when it fires. No separate subsystem. → `../interaction-stack.md`.

### D4 — Board is a container: a hex holds up to 3 ground creatures
A hex is larger than a creature. Up to **3 ground creatures** occupy it (alternating corners; the other 3 corners reserved for a later use — TBD). Attacks target a **hex**; the defender **declares defenders** from creatures on that hex. Enables guarding important creatures. → `../overview.md` §2, §4.

### D3 — Combat runs sequentially now, but stays revertible to a phased model
Default: sequential resolution (declare/resolve one attack at a time), with **defend-once-per-turn** as a tactical primitive. Minor goal: model every fight as an explicit `Combat` object (`{attackers, target hex, declared defenders} → resolve`) so sequential vs. phased is only a **scheduling policy** — a later revert is a scheduler swap, not a rewrite. Simultaneous multi-attacker-on-one-hex is the feature the phased scheduler would unlock. → `../overview.md` §3–4.

### D2 — Champion progression uses level bands (formats by power ceiling)
Power is a **step function**: horizontal (sidegrades) *within* a band, a discrete jump *between* bands. Matches/tournaments are defined per band ("bring a level 1–5 champion"), so players never face a champion outside their band — vertical growth never creates an unfair match. Enables phased launch (ship band 1 first) and three distinct metagames. **Open:** down-leveling (may a higher champion play a lower bracket at a capped config?). → `../champions.md`.

### D1 — Meta/core firewall confirmed
Champion/card progression is **never** match-internal. Leveling happens outside matches, at the player's pace, as a deliberate build decision. The match core consumes a resolved loadout only. → `../champions.md`, PLAN §6.
