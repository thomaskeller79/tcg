# Decision Archive

*Decisions, and parts of decisions, that later decisions replaced; newest first. Moved here verbatim from `decisions.md` so the active log holds only what is in force. Each entry names the decisions that replaced it. Read this only when reopening a replaced idea.*

---

### D164 — The observation model: what can be observed, what is certain, everything else deduced; a contradiction observes what the instruction read — replaced parts

*Replaced by D165.*

- **Q4, contradiction.** A certain observation overwrites only what it observes. When it contradicts what a copy carried out — a creature lives that the copy's death check removed, a creature falls that the copy expected to live, a creature spends more AP than the copy believes it has, a payment takes the copy's count of a pool below 0 (D157), a trace stays in the Aether that the copy believed faded (or the other way round), a permanent takes an action the copy believes illegal — that copy also **observes the true values of the properties the instruction read, together with their max values** (max-Life with Life, max-AP with AP). This is D146's rule with the max values added. An overspend observes the count and the production of the terrains that credit the pool; a trace in the Aether observes its Duration. A contradiction never fills in an unknown (D151): a `?` stays `?`.

- **A check reads static abilities like any other property,** so an action the copy believes illegal observes what its checks read. For now each check names them: a move (Move, Ascend, Descend) reads Flying and the terrain's Move surcharge; an Attack reads Ranged, so every combat contradiction observes it with Life and Attack; the timing of an activation reads Haste. Ability types may take this over (Track C item 10).

- **An ability the copy doesn't believe the permanent has:** using it shows the ability as the trace shows it (true, unless the ability carries its own Pretend), and the copy adds it to its belief of the permanent — nothing more.

- **Nothing else is revealed.** No cause: D151's large reveal is dropped, and with it D163's history rewrite. No name: a Mimic face survives a contradiction with its stats observed, and collapses by its own stated collapse (D131), a reveal, or a check that reads an identity property (name, card type, subtypes) and comes out differently than the copy expected — "Destroy target Goblin" fizzling on a Dragon with Mimic: Goblin. Then everything the Mimic hid is observed; a separate Pretend on the permanent stays (amends D150, D159). A Pretend that shows a different card type without Mimic is outside the prototype (Track D item 81). D131's fallback "collapse to the full truth" and D18's "collapse → full truth by default" go; D18's "no deduction engine" stays.

- **Only on a contradiction**, not in every combat. Observing more (every combat) stays open for playtests.

From **Why:**

- **The max values come along** because a higher current Life or AP can come from a higher max.

- **Start simple,** and add a cause reveal or more observations if it plays strangely or isn't fun.

From **Considered and rejected:**

- **The large reveal** (D151) and **the terrain observation abilities** (D152).

- **Item 72's other rejected variants:** a section as a set of possibilities ("4–5 cards", advertises a secret draw); face-down activation as the default; rule steps applied from the true state to every copy; on a contradiction, revealing the cause, or fitting with the death known.

---

### D163 — One history track per copy; any history may be read — replaced parts

*Replaced by D164.*

- **A large reveal also corrects the history** (D151). Every history entry produced by a trace or card the reveal makes true is rewritten in that observer's copy to its true output. Example: A's "+0 Power. *Pretend:* +1 Power" left "X got +1 Power" in B's history; when the Pretend is revealed, the entry becomes "X got +0 Power" in B's copy. Entries that are `?` stay `?` (a contradiction never fills in an unknown, D151), and entries B saw truly are already true.

- **A revealed lie must not stay in the history.** Otherwise a later condition reads the false entry again: the same card cast twice in a row would mispredict in B's copy a second time, though B already knows the truth.

- **History as a record of what was seen, never rewritten** (Claude): simpler, and the comparison would catch a stale entry — but by contradicting what the observer already knows.

---

### D162 — Each neutral seat holds its own belief copy — replaced parts

*Replaced by D164.*

- **Gaining a permanent makes its values true in the new observer's copy** — that permanent only. This is the default "your own permanents are true in your copy" applied at the moment of the gain, and it is what gives a monster true knowledge of itself.

---

### D157 — Overspending past the opponent's belief is a contradiction; an untested mana bluff ends at the reset; no "possible payers" marker — replaced parts

*Replaced by D164.*

- **A payment that takes the opponent's count below 0 contradicts it**, like a creature paying more AP than believed (D151): the large reveal as written — the count becomes true and the Pretends that caused the false count are revealed, traced on the true state. The real payment is always checked against the true pool, so a Pretend can only *show* spending the player can't afford. To playtest: the reveal names which of several spells was the cheap one, which a human could not deduce.

- **Count-only reveal** (the count becomes true, nothing else): an exception to the large reveal for one property. Kept for the playtest if the large reveal gives away too much.

---

### D155 — Your own permanents are true in your copy; traces show targets by observation; no hidden creation in the prototype — replaced parts

*Replaced by D164.*

- **A change to a permanent a Champion controls is true in its copy, unless a Pretend says otherwise.** An instruction default, not an observation checked after the instruction (that would undo the Pretend). It covers damage: the damaged creature's controller sees the true amount, whatever it believes about the source. A hidden curse on your creature is a Pretend.

- **The controller observes only damage dealt to its permanents** (Claude): narrower than needed; Pretend already makes the exceptions.

---

### D153 — Every instruction identifies the permanents it affects, never their location — replaced parts

*Replaced by D164.*

- **Every instruction identifies the permanents it affects to every Champion** — which instance, not where it is. "Target creature gets +1 Power" on a hidden creature tells the opponent "#12 gets +1 Power"; its belief of #12 updates correctly and its location stays hidden. A death names the creature that fell. So the update always lands on one object in every copy, and a permanent's controller is always known.

- **A death updates only observing copies, until the remnant is found** (briefly adopted): unneeded once a death names its creature.

---

### D152 — Observation lives on the terrain; sight lost forgets location; a move keeps the opponent's belief — replaced parts

*Replaced by D164.*

- **No global observation rule.** Observation is static abilities on every terrain, one per Slice, checked after every instruction; where a player observes a permanent's location, their belief becomes the true value. **Ground** and **Sky**: every player observes the location of every permanent there. **Root**: a player observes the location of a permanent in this terrain's Root if they control it, or control a permanent in the Root of this terrain or a neighbouring one (D67).

- **Changing observation:** an effect replaces the terrain's observation ability, found by its subtype (D129), with its own text for its duration. **Darkness:** "Target terrain's Ground becomes dark, Duration 2" — a player then observes a permanent on that Ground only if they control it or control a permanent on the Ground of the same terrain. Effects changing the same ability apply in creation order; the later one counts. This "instead" is a continuous effect on an ability, not an MTG replacement effect.

- **A global rule "you observe your own permanents' location"** (Claude): more general than needed; folded into the Root ability.

- **Observation as a terrain property** ("observation range" per Slice, Darkness sets it to 0): kept as a possible simplification — ability parameters — not decided.

---

### D151 — Contradiction by comparison, with the large reveal — replaced parts

*Replaced by D164.*

- **Large reveal:** a contradiction makes the read values true for the observer **and** reveals what caused the false value — the traces and their cards (modifiers remember their trace, D144) and the static abilities that contribute to it. The trace back runs on the true state. It also corrects later false beliefs from the same cause (Mirage 3's false end-of-turn drop).

- **Health and AP need no rule of their own:** a creature that lives although a belief put its Health at 0 or less, or pays more AP than a belief says it has, is a contradiction like any other; the large reveal makes the value true and shows its cause. AP spent is seen, since every action names its creature (D153).

**Why:** uncertainty should also shrink now and then, not only pile up. A static ability ("+1 Power while defending") can make a revealed Power look wrong again unless the ability is revealed with it. The comparison keeps untested bluffs alive.

**Considered:** the **small reveal** (read values only) and the **explicit reveal** (an instruction always reveals what it reads, no comparison — the old reveal points) — both kept as playtest variants (`playtest-variants.md`).

---

### D150 — Pretend is the one deception mechanism; players believe what they see — replaced parts

*Replaced by D164.*

it collapses by a contradiction (a hit too strong for a Goblin) — the large reveal traces back to the Pretend and reveals the card, so the whole face at once — or by its own stated collapse (D131)

---

### D149 — Three copies of every property; the basic instruction sets each copy — replaced parts

*Replaced by D164.*

the **death check** runs on the true state and applies to every copy; **damage** is applied with the true amount in the copy of the target's controller

---

### D146 — A belief breaks on contradiction; reveal points and private numbers are dropped *(amended by D151: the large reveal)* — replaced parts

*Replaced by D164.*

- **Numbers are not private.** A visible creature's Life, AP and Power are true by default (D140: fresh values from a true max, and a change to a visible object is seen); hidden or false numbers come only from keywords. The guarantees the user wanted then hold in the default case: a view showing 0 AP means 0, a card shown in a Hand is there.

- **A view is what the GUI shows**, not the player's belief. A Mimic face collapses when an outcome contradicts it, not at its first combat as such.

---

### D143 — Stat changes: Health and AP have a current-only pair and a ±X pair; Power is one value — replaced parts

*Replaced by D168.*

- **Health:** **damage / heal** change current only (heal capped at max); **+X / −X** change max and current together, as exact mirrors.

**Why:** Health and AP are consumed — damage uses up Life, actions use up AP — so a current tier tracks what is left; Power is never consumed. Other games agree: CCGs have damage/heal plus buffs for Health and only ±X for Power; AP games (Divinity: Original Sin 2, Pathfinder 2e, Dofus) change both tiers (Haste/Slowed per turn, Adrenaline or AP removal now). MTG's damage next to −X toughness confuses new players; here −X visibly lowers the max, which keeps the two explainable.

---

### D140 — A view state comes from the source; exceptions per instruction *(restated as one-time belief copies, D149; view states replaced by beliefs)*

*Replaced by D149, D154, D161.*

Restructures D139 (`PLAN.md` Track A item 51 (a)). Before: D139 listed transitions one by one, and treated activate like cast.

**Adopted (user):**
- **Default: a new object's property takes its view state from where the value comes from** — the predecessor's property for a copied or converted value, the input for a computed one (current Health from max, printed values from the name). A value with no predecessor (a target, a new card's or trace's section and position, current AP at 0) is true. **A changed property keeps its view state**: a change to an object the observer sees is seen, whatever caused it.
- **Exceptions:** **cast** — content hidden to an observer becomes true; **draw** — hidden becomes true for the owner; **entering Discard** — true for everyone, unless an effect states otherwise; **shuffle** and **concealment** — the observer loses track (what is kept is open, item 55).
- **Activate has no exception:** its trace inherits from the permanent, so a creature the opponent can't see (in the Root) activates with its content hidden to them; a trace's section and position stay true.
- Every instruction has a default view behavior; most need no exception (D141).

**Why:** a value's view state travels with the value; the few real changes sit where an observer gains or loses sight. D139 treated activate like cast, which would show where a hidden creature moves (D98 assumes a hidden move stays hidden).

→ `asymmetric-information.md` §Views.

---

### D139 — What the game itself does to a view: creation and section changes, in the default case *(restructured by D140; losing track reopened, item 55; view states replaced by beliefs, D149; losing sight of a permanent, D152)*

*Replaced by D140, D149, D152, D154, D161.*

Settles part (a) of `PLAN.md` Track A item 51 for the changes the game makes without a described effect, and without any keyword involved (a keyword states its own behavior). Before: D138 left each property's default in each view open; D71 hid Discard from the opponent and showed the owner their Library's remaining contents.

**Adopted (user):**
- **A view state per observer and property: true, hidden or false** — e.g. position (hidden = concealment, false = a decoy location), name and type (a Mimic face), a card's section and position. **Bonds** are always true. A creation or section change maps each observer's prior state to a new one; everything not listed below keeps its state (true → true, hidden → hidden, false → false).
- **Start of game.** Nothing is false. Library content is hidden from everyone, Hand content from the opponent, Root occupants from the opponent unless D67's proximity applies (D12). Everything else is visible: every card is visible as an object, with its section and position. The opening Hand is drawn before the game starts (mulligans need it).
- **Cast** (card → trace): for the caster nothing changes; for the opponent hidden content becomes true. Targets are true. A trace's section and position are always true, like bonds — a secret spell still shows as a trace and hides its content itself, as a trace and in Discard. Activate is the same from a permanent's ability.
- **Resolve** (permanent trace → permanent) and **Flicker** (permanent → trace): no view state changes. Fresh current Attack and Life take their max's view state; current AP enters at 0, true for everyone; location and target inherit each other's state.
- **Bounce** (permanent → card) and **Remand** (trace → card): the content keeps its view state, in its owner's Mind and in the opponent's. The new card's section and position are true for everyone.
- **Fall** (Creature/Companion → Remnant) and **Raise** (Remnant → permanent): only the name crosses, with its view state; a value raise reads from the card definition takes the name's view state. A false value (a Mimic face) ends at the fall: Mimic states that its face collapses then.
- **Draw:** hidden → true for the owner. **Entering Discard** (discard, mill, a cast card) makes the content true for everyone, unless an effect states otherwise.
- **Losing track forgets.** A view holds property values only for objects whose position the observer knows; losing track (a shuffle, concealment) forgets every value, true or false. A shuffle forgets for every observer, the owner included. The view never shows "one of these is X" — that is the player's memory.

**Why:** almost every value a new object gets is copied, converted or derived from its predecessor (D105), so its view state can travel with it; the few real changes sit at the edges of what a player sees — the opponent's Mind (cast, Discard), the owner's Library (draw) and losing track. Discard is public because cast already showed most of what enters it, a hidden Discard would make the view forget what everyone saw, and cards reading a Discard need it; D71's reason (a public Discard can't carry a secret cast) is met by the secret spell hiding itself, which it must anyway since its trace is visible.

**Considered and rejected:**
- **A creation-type axis** in the defaults table (object type × controller × how it was created): the transition rules already say where each value comes from.
- **Bounce into the opponent's Mind hides the content** (as in a physical card game): hides nothing the player doesn't know, only makes the view forget.
- **Discard keeps the content's view state** (Claude): kept discards and mills hidden from the opponent; the user preferred one public rule and effects that hide.
- **D71's owner list of the remaining Library contents:** a "one of these is X" region; a deck tracker is a GUI matter outside the view.
- **Raise keeps the fallen permanent's true changes** (user's preference): parked for playtest, `history/playtest-variants.md` §What a raised Remnant brings back.

**Still open (item 51):** whether regaining track restores the old view state or gives the default of the object's current place; what kind of property the paid cost is; how a hidden trace's changes reach the opponent's view.

→ `asymmetric-information.md` §Views, `overview.md` §1 and §9, `glossary.md` (Mind, Library, Discard), `object-properties.md` Open question 5. Engine: `PLAN.md` Track B item 22. D71's Discard and Library rules are superseded.

---

### D137 — Every view is a belief: the numbers and counts are private, a few reveal points are guaranteed true *(superseded by D146 and D147; Mind counts by D138)*

*Replaced by D138, D146, D147.*

Closes `PLAN.md` Track A item 35 (hide the Mind from the opponent). Before: D18 made standing state public (hand size, AP, Life, Power) and hid only the live mana balance; D71 made the three Mind counts public; D7 made the board fully visible; D18 called fell, unbond and discard costs self-revealing.

**Adopted (user):**
- **Every view is a belief.** An observer's view is what they observed, with claims taken as true. By default a player's view of what they own is true; a card may change that too (Track D item 50).
- **The true numbers are private to their owner:** a permanent's stats and resources (Power, Life, AP) and every pool and count (mana, Hand, Library, Discard sizes). Positions, identities and bonds stay visible by default (D7 narrowed to these); the network stays public, since denial needs it (D8).
- **A change is seen by every player whose view contains the object.** A fell is seen by those who see the permanent, so felling a hidden Root creature shows nothing (as with its Remnant, D110); an unbond is seen by everyone. D18's "self-revealing costs" are this rule, not a category of their own; a discard is a change in a private Hand.
- **Reveal points** are always true, no card can fake them:

  | Reveal point | Shows | To whom |
  |---|---|---|
  | A permanent is dealt damage | its true Life | every player who sees it |
  | A permanent deals damage | its true Power | every player who sees it |
  | A permanent's AP reaches 0 | that it is exhausted | every player who sees it |
  | A Hand becomes empty | that it is empty | everyone |
  | A mana pool refreshes | its true amount | everyone |
  | A cost is paid | the true value of the paid resource | the payer's controller |

  Each depends on the owner's choices: keep 1 AP or one card back, avoid combat. No reveal points for Library and Discard sizes: a player who needs them plays a card that reveals them.
- **Damage is the only way an instruction lowers Life.** No separate "lose life" (MTG's damage vs. life loss is not adopted). Paying Life as a cost is not damage, so a hidden trace's Life cost stays hidden.
- **A cost is paid by the payment walk only** (`ancestry.md`). A cost can no longer name the controller as payer ("this creature's controller pays 2 life"); a card that wants the Champion to pay puts it in its effect. This keeps a hidden trace's possible payers known from its source.
- **GUI support, not a rule:** each observer sees an expected value per number (their view) and a marker on resources that a hidden trace's possible payers could have spent. Effects of hidden traces are not marked: after one resolves, anything may differ, and Mimic shows that a display may be false with no marker at all. Players are taught that hidden and false information exist. → `PLAN.md` Track B item 20.

**Why:** with public resources, a claimed cost collapses the moment it is paid (a spell claiming 3 while 6 mana leaves the pool), so Mimic can't work. Anything public and derivable should be shown, because bookkeeping that the system could do is no fun; so the choice was all public or private with reveal points, not public-but-undisplayed. Combat as the reveal for Life and Power keeps D18's hard fact. Removes the card-count / library-size conservation law: no public count is left to check against.

**Considered and rejected:**
- **A hidden Champion Draw** (so the hand count is unknown): with public AP, the 5-AP spend gives it away; a cheaper Draw or other hidden 5-AP actions would be needed. Dropped by the user as solving a broader problem at the wrong level.
- **Hiding AP only for hidden abilities:** a special case of the general rule.
- **Marking everything after a hidden trace resolves:** no information gain, and Mimic already makes unmarked numbers uncertain.
- **"A resource at 0 is public" for all resources:** kept for AP and Hand only. An empty mana pool would remove the "tapped out, or not?" doubt; mana gets the refresh reveal instead. "Empty" is too weak for Library and Discard.
- **Reveal when Life goes down for any reason:** would reveal a hidden trace's Life cost.

**Constraint on Mimic:** a face whose Life or Power differs from the truth collapses at the first combat; a face copying a creature with the same stats survives it (D131's per-card collapse still decides the rest). A card faking leftover mana mid-turn collapses at the next refresh at the latest.

→ `asymmetric-information.md`, `overview.md` §1, §9, `glossary.md`, `ancestry.md` §Paying a cost; `PLAN.md` Track A item 35 (closed), Track B item 20, Track D item 50.

---

### D131 — Every hidden-information ability states when and how far it collapses — replaced parts

*Replaced by D164.*

D18's collapse to the full truth stays as the fallback for anything a card doesn't cover.

---

### D130 — Casting order: amounts, choices, targets, then pay; a cost may depend on targets and is topped up at resolution — replaced parts

*Replaced by D135.*

- **Order:** Legality → **Amounts** (every `X`) → **Choose** (modes, other choices, and every choice of which resources pay, e.g. which creature to fell) → **Target** → **Pay** → Enter Pending. Every choice comes before paying; **paying itself makes no choices.** So a target may depend on a payment choice ("destroy target creature sharing a type with the felled creature").

---

### D125 — Mana: disjunctive pips and pool mana, paid by domination — replaced parts

*Replaced by D166.*

**Adopted (user):** a cost pip is **generic** or **colored**, a disjunction over Elements (one Element = colored, two = hybrid, …); nothing else exists. A pool mana is **colorless** or **colored**, likewise a disjunction. A mana **dominates** another if it pays every pip the other pays. Pips are paid **from the most constrained to the least** (one Element, two, …, generic last); each takes the mana every other candidate dominates, and when the candidates tie, the player chooses. **Why:** the automatic picks are exactly the safe ones — no player could do better by choosing — and most payments need no choice: colorless always pays generic, a one-Element pool never chooses, a two-Element pool only chooses which Element pays generic. **Considered and not adopted:** looking ahead across the whole cost to remove choices that lead to a dead end (Claude) — the user keeps the obvious algorithm first; a smarter one may come later. (The engine lists only complete payments, so a dead-end choice is never offered anyway.)

**Still open (item 37):** whether generic pips exist at all — dropping them would make nearly every payment automatic.

---

### D107 — Companion and Structure transition rules; leaving the Island cascades like death; Structures never fight — replaced parts

*Replaced by D133.*

**A Structure never fights** — closes item 4 (d). It has no Attack (D103) and can still be attacked (it has Life). Moving the question to Track D was proposed and declined: the user's answer is a plain no, not a deferral.

---

### D100 — Terrain: exactly one per hex, location targets name a terrain, Type is flavour, Element is derived — replaced parts

*Replaced by D133.*

**Only permanents that hold Activation Points can carry a Behavior** (`neutral-permanents.md`): Actors and Structure. Terrain and Item never do; corrects passages that said a dormant Terrain or Item could "already carry one".

*Replaced by D166.*

**Element is derived**, as MTG derives colour identity: the set (0–8) of Elements in the mana the terrain produces and in its rules text. Mana production is an expression (amount; coloured, hybrid, generic…), not "1 of its Element".

---

### D98 — History is a track of facts with a one-round window; physical is a Trace property — replaced parts

*Replaced by D116, D163.*

**The window is one round, a hard rule**: older history can't be accessed at all. One round is the shortest window containing every seat's turn (D60), so "since your last turn" stays expressible. Whether the engine keeps snapshots or diffs is a Track B question. Open: per-observer filtering — a hidden creature's move is an output too, so a query must read the asking player's view.

Lasting changes remain properties of the object; only short-term memory moves to the history track (first candidate: Defend's once-per-turn limit, `PLAN.md` Track A item 34).

---

### D97 — Objects that leave every zone cease to exist; `source` is a backward link, joined up on removal — replaced parts

*Replaced by D110.*

Removal vocabulary (proposed: "destroy", "sacrifice") and how a creature ceases to exist outright, given that destroying it leaves a Grave (D88), are decided in `PLAN.md` Track A item 27.

---

### D94 — Casting is a transaction: abort refunds everything; the card pays at the end of Pay; instructions bind outputs — replaced parts

*Replaced by D95, D96.*

**Instruction outputs** (confirmed from item 4's effect-normal-form discussion): an instruction can bind a named result (`killed := damage(T1, 3)`) that later instructions reference (`[if killed ≠ ∅] draw(1)`) — the meaning of "if it dies this way," distinct from "if T1 is dead." Outputs are the third kind of name beside variables (Pay) and targets (Target), bound at resolution, card-wide in scope and instanced per pick like targets (D91). An output of an instruction that didn't run is empty.

---

### D91 — Casting procedure refined: legality step, earliest-possible binding, optional and labelled cost branches, card-wide target scope — replaced parts

*Replaced by D130, D135.*

**Procedure** (refines D46/D70/D83): **0. Legality** — Speed fits Pending, the card's **cast condition** holds ("cast only if you control a Rebel"), and at least one legal cost-and-target combination exists. **1. Pay. 2. Choose. 3. Target** (with D68's redirect loop). **4.** The trace enters Pending. The card itself is part of the cost (refined in D94); abort remains possible until step 4. **The cast condition is checked only at step 0** — sacrificing the only Rebel as part of the cost is legal (MTG behaves the same).

**Pay stays before Choose** (D70 kept). MTG announces modes before paying (601.2b) so a mode can carry its own cost (Spree, 700.2h) and kicker can add targets. The same expressiveness comes from the cost itself: a disjunct may have an **empty branch (ε)**, making it optional; branches are **labelled** (`c2 = (b1: 1 ∨ b2: 2 ∨ b3: 3)`), and an instruction condition may test which branch was paid (`c2 = b1`). Kicker = an optional conjunct plus a condition; Spree = one disjunct enumerating the mode combinations (2ⁿ−1 branches, accepted — printed text can show the MTG-style sugar); Escalate = a variable cost plus `choose X+1`. Conditions test the **label**, not the amount, since two branches can cost the same; two same-cost plain-mana branches are therefore both prompted (D70's auto-pay applies only when exactly one branch is legal). Paying for no mode at all (all optional branches ε) is legal and simply wasteful — no constraint added.

---

### D89 — Every permanent entering the Island enters with 0 Activation Points; replaces D57 *(Setup exception removed by D114; entry AP 2 by D160)* — replaced parts

*Replaced by D114.*

**Setup is not entering:** Setup-placed permanents (Champion, Terrain, anything the Map places) are created by Setup, not by entering the Island (`setup.md`, D76), so this rule doesn't apply to them — the Champions' starting AP and Champion A's skipped first refresh (D76) are unchanged.

---

### D88 — Every created object has a card; there are no tokens; Grave and Ruin are a status of the dead object, not separate objects *(status model reversed by D110: Remnant is a new object; Ruin dropped by D111)* — replaced parts

*Replaced by D110, D111.*

**Grave and Ruin are not separate objects and not separate cards.** A Grave is the dead creature's own object, and a Ruin the destroyed Structure's, in a different **status** — keeping its own card, the same way a creature carrying a permanent counter is still that creature. They are resources: cards require or consume them (a zombify-style card needs a Grave; a Structure may be cheaper built on a Ruin). **They hold no Activation Points** (reverses D58 for them). Defining the status itself — which properties survive, how "target creature" excludes it, how D40's slot rule and D81's terrain parent read under it, what returning it does — is open: `PLAN.md` Track A item 27.

---

### D86 — Slice controller uniformity is an entry rule, not a standing invariant; only a control change can mix a Slice; D66's fizzle removed — replaced parts

*Replaced by D115, D120.*

**Adopted in principle, exact rule deferred to item 25:** co-located enemies in a mixed Slice can fight each other (it makes no sense flavor-wise that they can't). Surfaced along the way: uniformity is per Slice, not per hex, so enemies at distance 0 already exist today (Ground held by A under Sky held by B); a Melee change to "distance ≤ 1" would therefore open same-hex cross-Slice melee generally, not just in mixed Slices. The user's starting point for the attack model — an attack targets a hex plus an attacked entity (Champion A, Champion B, or Neutral, leaning to Neutral as one entity), selected by default when only one is present, every creature of that entity may defend — is recorded in item 25, not decided here.

---

### D85 — What a trace checks at resolution: its remaining text plus four implicit conditions; perception is always a live view *(visibility restated in the belief model by D159)* — replaced parts

*Replaced by D149, D159.*

**Perception is always a live view, never a belief state.** Every perception-dependent rules check uses the relevant observer's live view — what it perceives now, from current state plus perception rules, including D18's per-observer claims (so it can be deceived), without memory or inference. A Neutral permanent is an observer with its own live view; this does not reverse D56's "no persistent, generalized belief state" — it is the same statement. Consistent with D18's "no deduction engine": belief tracking belongs to the player (or AI), never to the rules. Persistent per-observer board facts (a collapsed claim, a surfaced creature) are stored state, not inference, and fit the principle. Floated and clarified along the way: a "belief state" for each Neutral permanent — the distinction that settled it is memory (a belief state would keep a probability distribution over where a Descended creature went; a live view simply no longer has it).

---

### D83 — Ancestry is a present-tense payment tree; control is a query on it, Behavior an object property; blocked bonds are inactive edges; every trace reference binds at declaration — replaced parts

*Replaced by D106.*

**Agreed, deferred to item 4 (property inventory):** every creation instruction carries a (controller, Behavior, neutral turn) triple bound at declaration — card source → the casting Champion; controlled permanent's ability → its controller; uncontrolled source → None plus the source's Behavior and turn; card text saying "Neutral" must state Behavior and turn or the card is invalid. Controller = Champion → the new permanent's parent is the resolved payer; controller = None → created as a parentless root. Name for the field (to avoid clashing with the Trace's own controller) open.

---

### D82 — Neutral-permanent Behavior and neutral-turn assignment resolved for every way a permanent can become Neutral; a source/parent gap spun out to its own item — replaced parts

*Replaced by D83.*

**A real gap surfaced, not fixed here:** the first case above (inherit from the causing Neutral permanent) needs to identify *which specific permanent* caused it — but a Trace's Ancestry `parent` (D81) is deliberately flattened to the nearest live Champion or Companion, which resolves to nothing when the causing permanent is itself Neutral. D81 assumed this flattened link also satisfied D75's separately-parked `source` question; it doesn't, once the producer has no live Champion/Companion above it. Not patched piecemeal — spun out to `PLAN.md` Track A item 18 as a more fundamental audit of what Ancestry can and can't answer.

---

### D81 — Ownership renamed Ancestry; the structure extended to Trace and Card, resolving PLAN.md §8 item 9 — replaced parts

*Replaced by D82, D83, D103.*

**Left explicitly open, not decided here:**
- A noun for "the entity that pays" (Payment's terminal node, distinct from Controller) — concept confirmed needed (e.g. "deal damage to the entity that pays for target creature"), "payer" proposed and rejected as weak, no replacement chosen. `PLAN.md` item 17.
- Whether other parent-relationships (Item's "equipped," in particular) deserve their own dedicated verb the way Terrain has "bonded" — enumerated but not named. `PLAN.md` item 17.
- How Neutral relates to the Ancestry more deeply than being one of Controller's three outcomes — deliberately spun out to its own item rather than folded in here. `PLAN.md` item 16.

---

### D80 — Two combat defaults tightened to match the pessimistic-default principle (D14): Root cannot attack Ground; surfacing is permanent *(surfacing part withdrawn by D102)*

*Replaced by D102, D115.*

Surfaced while scanning `docs/rules/` for loose, unscoped asides — two combat defaults were still generous, each hiding behind its own unresolved hedge instead of an actual decision.

**Root-Slice creatures can no longer initiate an attack on a Ground-Slice creature by default.** Previously allowed, flagged "tentative, pending balance." A hidden creature getting a free ambush attack upward is exactly the kind of generous default D14 exists to catch. Flipped to illegal by default; a specific positive keyword (an "ambush"-style ability) would need to grant it — not designed yet, `PLAN.md` Track C item 5.

**Surfacing (the reveal a Root-Slice creature suffers by attacking) is now permanent by default**, not automatically undone by moving off the hex it attacked from. Previously "re-conceals when it moves away" was stated as the default, flagged `(confirm)` — again a generous default (free re-hiding) sitting behind an unresolved hedge rather than a real decision. A specific ability could still grant conditional re-concealment (e.g. once far enough from every opponent) — not designed yet, `PLAN.md` Track C item 6.

Both logged in `history/pessimistic-default-audit.md`'s new "Flipped/adopted" section, alongside its existing D19 entries.

**Also cleaned up in the same pass, no rule change:** an inline "open question" in `champions.md` asking whether a Champion's identity is known to both sides by default was removed outright rather than answered — it already follows from the existing default-visibility rule (D7) plus `asymmetric-information.md`'s resource-observability table (Champion identity fits neither of the two narrow hidden-by-default categories: the live mana ledger, or hidden-zone contents), so there was nothing to add to the rules text. Three speculative, never-designed terrain-unbonding mechanisms (a granular single-bond Collapse Network variant, a new "unbond as an additional cost" primitive, a new "force-opponent-to-unbond" hostile effect) were also cut from `resources-terrain.md`/`ancestry.md`, where they'd been sitting as throwaway card-text examples with no design behind them — moved to `PLAN.md` Track C item 4 for future card prototyping instead. Separately, a batch of stale `PLAN.md` item-number cross-references (broken by a past renumbering) were fixed in `setup.md`, `object-properties.md`, and `structures-items.md`, and `PLAN.md` gained an explicit numbering rule: item numbers are never reused or renumbered going forward, only appended or deleted-with-a-gap.

→ `overview.md` §4, `history/pessimistic-default-audit.md`, `champions.md`, `resources-terrain.md`, `ancestry.md`, `setup.md`, `object-properties.md`, `structures-items.md`, `asymmetric-information.md`, `PLAN.md` (Track C items 4–6, numbering rule).

---

### D76 — Match Setup is not a domain transition; Champion/Terrain placement, first-turn AP asymmetry, and neutral-ground population all revised *(Setup procedure, its non-observability, the enters-the-Island consequence and the skipped refresh revised by D114)* — replaced parts

*Replaced by D77, D114.*

**Setup is fully non-observable and non-interactive** — no priority, no Pending, no response window exists while it runs, and neither player can perceive or influence its internal ordering. This is a structural property of Setup itself, not a targeted ruling — it's what settles the Terrain-before-Champion placement-order question as genuinely inconsequential (kept as originally proposed, revisit only if some future mechanism needs to distinguish it via D61 timestamp).

**Consequence: "enters the Island" is the wrong trigger for a Setup-placed permanent.** That trigger presupposes a real Aether trace and a real response window, neither of which exist during Setup. Instead, such an ability is written as **"at the beginning of your first turn"** — the *same* existing beginning-of-turn trigger mechanism (not a separate Action-phase-anchored trigger; considered and rejected, since "beginning of your turn" is already established vocabulary, `overview.md` §3, and reusing it beats introducing a parallel one), just restricted to firing once, after Setup has finished and normal turn structure is running. Mid-game entries (Cast, Flicker) are unaffected.

**First-player AP asymmetry moves from Setup to Round 1's own structure, superseding D48.** Both Champions now enter identically, with **4 Activation Points** each (no more first-player-reduced/second-player-full split). The asymmetry instead comes from **Player A's first Beginning phase skipping Activation-Points refresh specifically** — mana-refresh and beginning-of-turn trigger-firing still run normally. This requires treating "mana/AP refresh" (previously described as one bundled, simultaneous step, `overview.md` §3) as two independently-skippable effects — already separate in the implementation (`RefreshManaEffect`/`RefreshApEffect`), so this is a documentation sharpening, not an engine change. In practice this only ever affects the Champion today: Terrain already gets full Activation Points immediately on creation regardless of Beginning-phase timing (`resources-terrain.md`), and nothing else with Activation Points exists at Round 1. Player B's own first turn, later in the round, runs its Beginning phase completely normally — same net effect as D48's asymmetry, reached a different way. **Considered and rejected: skipping Player A's entire first Beginning phase.** This was the first idea, but it would also suppress the new "at the beginning of your first turn" trigger for Player A specifically (Player B's would still fire, an unwanted asymmetry) and would silently defeat the Champion's pre-bond (below) by skipping its payoff mana-refresh too.

**Champion pre-bonds to its home tile at Setup — kept as the current working default, explicitly provisional.** This was already written into `champions.md` (not merely an implementation note, as first assumed mid-discussion) as the fix for a real bad-feel bug: without it, a Champion's first Bond action pays AP but doesn't credit mana until the *next* Beginning phase, a full wasted cycle for a mandatory, no-real-choice action. Whether "produces mana starting next Beginning phase" is the right universal default at all — and whether pre-bonding is really the right fix, versus every other mana-generating permanent/ability having the same "enters tapped" problem — is a **separate, higher-priority open question** (`PLAN.md` §8 item 14, not resolved here). This decision only keeps today's known-working behavior in place so Setup's procedure has a complete answer now; item 14 may revise it.

---

### D75 — What Activate is: the identical pay→choose→target procedure, ordinary per-ability Duration, and a corrected doc bug that makes "at most one Physical Trace in Pending" an actual rule; the `source` property stays deliberately parked *(one Physical Trace → one Slow physical action, D117)*

*Replaced by D97, D117, D135, D142.*

Resolves `PLAN.md` §8 item 9(f) (what "Activate" — an existing Permanent using its own ability — actually is) except for one deliberately deferred piece.

**Trace properties: no new machinery.** An Activate-Trace is created by the identical pay → choose → target procedure D70 already established for Cast — it just reads cost/`X`/modal-shape off the ability currently sitting on the Permanent (already modeled as a stat the Permanent carries, §3) instead of off a Card. Nothing about Activate needed its own rule here; it was already covered by generalizing "casting or activating" together, which `interaction-stack.md` already did.

**Duration: also already covered, no Activate-wide special case.** §3 already states Duration is ordinary per-ability data, not something tied to being an Activate-transition specifically — the four base defaults (Move/Attack/Ascend/Descend) happen to be printed with Duration 0 (D45's "Physical Trace"); any other activated ability defaults to ordinary Trace Duration (5 rounds, card-overridable) exactly like a Cast-Trace, with nothing further to decide.

**A real documentation bug surfaced and got fixed along the way, not merely discussed:** `overview.md` stated Move/Attack "are Quick-speed Physical actions" — contradicted D45's actual intent. Quick describes what *other* cards can play while a Physical Trace already sits in Pending (combat tricks, `interaction-stack.md`), not the physical action's own speed. The base physical actions are **Slow** (playable only when Pending is empty) — this is also what makes "at most one Physical Trace ever sits in Pending" an actual enforced consequence of the Speed system, not merely an informal assumption: a second physical action simply can't be declared (Slow requires Pending empty) until the first one has fully resolved or been responded to. Fixed in `overview.md` §4; noted explicitly in `interaction-stack.md` so it isn't just an emergent fact nobody wrote down.

**Left deliberately parked: the `source` property.** Whether every Trace (not just an Activate-Trace) carries a `source` back-reference to whatever created it — a Card, a Permanent, or something else — is real, useful design space (it's what would let a card "counter the source of target ability" or similar), but it's explicitly deferred to `PLAN.md` §8 item 10, the full per-type property inventory, rather than decided piecemeal here for Activate alone.

→ `object-properties.md` §2 (Activate paragraph resolved, Open Questions updated — Activate question removed, `source` added as a new parked item pointing at item 10), `overview.md` §4 (Slow/Quick bug fixed), `interaction-stack.md` (Physical-Trace-Slow-implies-at-most-one made explicit), `PLAN.md` §8 item 9(f) marked resolved.

---

### D74 — Payment generalizes into one "resource walk" covering both costs and effects; Bounce/Remand need no Owner concept, since a card's own destination wording already resolves via the existing Controller walk — replaced parts

*Replaced by D81.*

**Left explicitly unresolved, a narrower and structurally different case:** `PLAN.md` §9's parked "responsibility" question (a Trace's own upkeep cost, e.g. "pay 1 life at the beginning of each turn" attached to a spell cast by a Companion) is **not** closed by this — a Trace isn't a Permanent and was never part of this parent-chain to begin with, so the walk described here doesn't reach it. That question stays open.

---

### D71 — Mind-domain zones (Hand, Library, Discard) are hidden from the opponent by default; each zone is ordered

*Replaced by D138, D139, D161.*

Resolves `PLAN.md` §8 item 6 (Track A/Rules), surfaced 2026-09-13 when the M1 implementation was found to have assumed Discard is fully public with no rule ever stating so.

**The rule, one line for all three zones:** nothing in the Mind domain is visible to the opponent by default — flavor ("why would an opponent see into a player's mind?") and mechanically load-bearing, since a domain that's public by default can't carry a genuinely secret cast. Each zone is also **ordered** — order is real, trackable state, not just a multiset, so a future card can reference position (e.g. "the top card of your Discard") without needing new machinery.

Per-zone shape — confirms D18's existing Hand rule, changes Discard, and writes Library's already-implemented behavior down as a real rule for the first time:
- **Hand** — unchanged (D18): owner sees full contents in true order; opponent sees a count only, never contents.
- **Discard** — **changed, this decision's actual effect.** Was fully public in the implementation (`ViewProjector.cs`, no observer filtering) with no rule backing it. Now matches Hand's shape exactly: owner sees full contents in true order; opponent sees a count only, contents hidden. A card that wants to see an opponent's Discard (or Hand) needs an explicit granted ability, same as any other hidden-zone peek.
- **Library** — order hidden even from the owner (draw order is never known, shuffled or not), but the owner *does* know their Library's remaining contents (deducible: own decklist minus what's been drawn/discarded/seen), so the owner's own view shows the multiset in a canonical, non-order-revealing sort. Opponent sees a count only. This was already the implementation's behavior (`ViewProjector.cs`'s existing comment); no code change needed, only documenting it as a deliberate rule rather than an unstated implementation choice. Explicitly deferred, not designed now: whether/how a card could ever grant a player visibility into their *own* draw order — not needed until AI or a UX feature wants it.

**Distinct from D7.** D7 ("default visibility is full") governs the Matter/board domain only — this decision doesn't reopen or narrow it; the two domains have independent, unrelated default-visibility rules.

→ `overview.md` §1, `glossary.md` (Library, Discard), `asymmetric-information.md` (resource-observability table). Implementation follow-up tracked at `PLAN.md` §8 Track B item 19 — `DiscardView`/`ViewProjector.cs`/the DebugUi Mind panel all currently treat Discard as public and need the same observer-gated shape `HandView` already has.

---

### D70 — Casting/activating is a three-step procedure (pay → choose → target); cost is CNF-structured and paid semi-automatically; a trace only exists in the Aether once all three steps complete — which is also why modal choices lock in on Remand/Bounce while target never can — replaced parts

*Replaced by D130, D135.*

**Comparative research, briefly — what it ruled in and out.** Magic's own 601.2 order (announce modes/X → target → divide → total cost locked → pay) turned out *not* to be "pay before target": Magic pays **last**, strictly after targets are chosen, which is the opposite of what Leyline wants — the actually load-bearing structural fact borrowed from Magic is narrower, that mode must precede target only because a mode can change what targets are even legal (a card with "choose one — target creature; or target artifact" can't ask for a target before knowing which). Whether other games even have non-mana additional costs at all turned out to be a real, live fork, not a solved question — Hearthstone has none (drawback effects like "discard a card" are written into resolution text, never a payable cost, so none of this machinery exists for it at all), while Yu-Gi-Oh (no resource system beyond tribute/discard/LP), Flesh and Blood (its entire economy *is* discarding cards for value, no separate mana pool), and Android: Netrunner all do. Netrunner (Null Signal Games' Comprehensive Rules, read directly) supplied the actual model adopted below: install/play/paid-ability procedures that pay cost fully before any targeting at all (targets are chosen inline, during resolution, not as a separate up-front step), and — the most directly useful find — **additional costs are paid atomically together with the base cost as one bundle**: *"A player must pay all additional costs simultaneously with the cost that is being added to... A player cannot pay the original cost or any of the additional costs individually."* No separate announce-then-pay split, and no Magic-Offering-style special case for choosing *which* object to sacrifice early, because nothing in a plain additional cost feeds back into calculating a different part of the same cost.

**The Offering question, resolved directly against a test card ("sacrifice a creature with power X; deal X damage to target creature"):** initially treated with the same caution as Magic's Offering (which needs an early, out-of-band choice because the sacrificed permanent's mana value *reduces a different part of the total cost still being calculated*). On inspection this test card doesn't have that shape at all — nothing about the sacrifice branch changes what the mana branch would have owed; the sacrifice is a fully self-contained payment action that outputs a number (the creature's power) *forward* into the effect, never *backward* into the rest of the cost. **The actual governing rule, sharper than "alt-cost payment must never determine a number": a payment choice may determine any downstream value it wants, as long as nothing else in the same cost still needs computing after it.** Feed-forward (into the effect) is free; feed-back (into a sibling part of the same cost) is the one thing that forces early, out-of-band resolution — and Leyline's design (§4, `object-properties.md`: a consumed cost's identity is never tracked as a property of anything, only its magnitude) structurally avoids the feed-back case by construction, so this exception essentially never triggers here.

**Adopted procedure — three steps, strictly ordered, none revisited:**
1. **Pay.** `X` (if any) is chosen here, since it's payment-determining. Cost is a **CNF** (conjunction of disjunctions), evaluated in a fixed disjunct order: **colored mana** → **mana with choices** (including a disjunct mixing a mana branch with a non-mana alternative, e.g. "3 fire mana or sacrifice a creature") → **neutral mana** → **additional costs with no mana alternative**. Payment is **semi-automatic**, with one deliberate carve-out: a disjunct with only one currently-legal branch resolves without a prompt *only when every branch in that disjunct is plain mana* — a disjunct mixing mana with any non-mana branch always demands explicit confirmation even when it's the only legal option, because silently auto-sacrificing/auto-discarding on a forced branch is not an acceptable default the way auto-spending fungible mana is. **An abort option is available at any point up until the trace actually enters the Aether** (generalizes D68's existing cancel-is-always-available language beyond just the redirect case).
2. **Choose.** Modal selections are made, including a "choose from this menu `N` times, repeats allowed" structure (`N` already fixed in step 1). Everything resolved across steps 1–2 — cost, `X`, every modal pick — becomes an ordinary, fixed, durable property of the trace from here on, extending `object-properties.md` §4's existing X-cost/single-modal-choice lock-in rule to the repeated-pick case explicitly (the repeated structure is a richer *shape*, a list instead of a scalar, but the same test applies: the Card's own schema has a slot for it, resolved or not, the same as it does for cost).
3. **Target.** Chosen last, against current game state. The same object may be chosen by two separate picks within one repeated-choice resolution (each pick is its own instance of a target requirement — same reasoning as Magic 601.2c's "the same object can be chosen once per separate instance of the word target, never twice within one instance"). Target remains the one property that never locks in, unchanged from D69/§4.

---

### D69 — Object creation is symmetric across domains (Bounce/Flicker join casting) and Markovian (a created object depends only on its immediate predecessor's current state); target is the one property that never locks in — replaced parts

*Replaced by D72–D75, D97, D105.*

**Left open, explicitly** (full list in `object-properties.md`): Remand for Spells specifically, untested against a concrete card; no permanent-card modal choice has yet been found that doesn't reduce to an ordinary triggered ability, which would make location the *only* true instantiation-choice a permanent card has if the pattern holds — logged as a working hypothesis, not proven; whether "target" needs sub-kinds here the way D68 already splits location- from permanent-typed targets for a different mechanism; whether a Neutral, Map-placed permanent is actually legal to target for Bounce, as opposed to merely not structurally blocked; card-level continuous effects depend on the still-undesigned continuous-effects system (`PLAN.md` §9 item 9); and, surfaced but untouched this session, Champion's own Attack stat is still unprinted anywhere in `champions.md` (only an Attack *ability cost* exists, unlike Creature/Companion's plain Attack/Life/AP triple); and this entire session only covers the six *cross-domain* transitions — within-domain zone transitions (draw, discard, mill, delay, resolve) and the "to nowhere" case (an object destroyed/exiled with no destination zone at all) are untouched. **Two more surfaced immediately on trying to write the model down, and one earlier claim had to be retracted** (the paragraphs above stand as adopted; the retraction below was a mistake made in the write-up afterward, not in D69's own reasoning): Champion/Terrain/Map-Neutral creation is **not** a plain Card→Permanent read (that would contradict Mind→Matter being forbidden) — it plausibly involves the Map as a participant, genuinely undecided; and an existing Permanent activating its own ability (**Activate**, including the base Move/Attack/Ascend/Descend defaults) is a distinct, common, everyday Matter→Aether case this session never actually modeled — closer in shape to Cast than to Flicker (the source Permanent isn't consumed, unlike Flicker) but not identical to Cast either, and its own rules are unresolved. Also clarified in the write-up: "**Physical trace**" (D45) isn't a separate kind of Trace at all — it's just Trace's existing **Duration** property (D50, default 5 rounds) at value 0.

---

### D68 — Hidden information never creates an illegal occupancy state: the instantiate step becomes iterative (redirect-until-legal-or-cancel); multiple controllers per hex-level was pursued at length and rejected

*Replaced by D86, D132.*

Resolves the "hidden-occupant-on-entry" question left open alongside D66 (`PLAN.md` §8/§9): what should happen when a Move, Summon, or Relocate/push discovers — only at the moment of resolution — that its chosen destination is illegal, either because a hidden enemy already occupies it (the "expected the hex to be empty" flavor of D66's own controller-uniformity invariant) or because a hidden creature already fills the level's last slot (a distinct "expected the hex to have space" capacity problem D66 never touched). Both flavors share one root cause: the acting player's belief state said the destination was fine, and true state disagreed.

**A whole alternative direction — relaxing the invariant itself so mixed controllers could simply share a level — was pursued in real depth before being rejected, and is worth recording precisely because so much of it actually worked:**
- **First step:** narrow the controller-uniformity check to bind only *visible* occupants — a hidden creature wouldn't count toward "must share one controller" at all, only toward capacity. This would have made every hidden-collision entry simply legal, no fizzle needed.
- **Extended further:** drop the controller invariant for *visible* occupants too, always allowing mixed-controller occupancy outright. This needed, and got, real answers: Attack would need to let the attacker declare *which* opposing controller they're engaging when a level holds more than one (a small, bounded addition, not a rework of Combat's resolution mechanics); this in turn surfaced that Melee's "adjacent" targeting had never needed to cover distance 0 (two creatures truly co-located) because it was previously impossible, floating "Melee could even become same-hex-only" as a further simplification once distance 0 was accepted; separately, the resulting **space** problem (a mover discovering only at resolution that the 3-cap was already secretly full) was given a real, bounded answer too — a capped-at-1, *publicly known* overflow slot that a newcomer falls into when the 3 normal slots are full, restricted to only leaving-the-level (and, if Melee moved to same-hex-only, attacking) until a normal slot frees up.
- **Rejected anyway, decisively:** allowing mixed-controller occupancy — even only for the passively-surfaced case, and certainly as a standing rule — means a creature can simply move into or through any hex that isn't already at full capacity, regardless of who is standing there, since "an enemy is here" stops being a legal obstacle at all. That guts pillar 2 ("position is a resource") by removing real spatial blocking, which the original 2026-08-08 invariant was quietly doing load-bearing work for, not merely restricting by accident. Everything built to support this direction (the overflow slot, the Attack-declares-controller addition, the Melee-distance-0 question) is abandoned along with it — none of it is needed under the adopted answer below, and none of it should be revived to solve some other problem without re-deriving why it existed.

**Adopted instead: the invariant is untouched (controller-uniformity and the 3-slot cap both stay exactly as originally decided, no exceptions) — what changes is the *procedure* for choosing a destination, not what states are legal.** True state can never actually go illegal, because a bad guess is never allowed to land:
- **Legality is still checked at declare time, against the acting player's own belief state** — an ordinary targeting rule (you may not declare a target/destination your own view already shows as illegal, e.g. a hex you can see is already at capacity with your own creatures). The mechanism below only ever engages when belief and truth *disagree* — which, since capacity math can never be secretly wrong on a level with no concealment, only ever happens on a concealed level (Underground, or wherever a card conceals a region).
- **The standard cast/activate/move procedure's *instantiate* step (D46: pay cost, then instantiate the remaining choices) becomes iterative for this case specifically.** This is a different check from D33's (D33 covers a target going illegal *later*, while sitting in Pending, from a visible board change — unchanged). Here, the very first attempt to instantiate a chosen destination can discover it's illegal against true state, at that exact moment, before ever reaching Pending. When that happens, the acting player is told the attempt failed and may either **choose a different destination** from the action's remaining legal-per-belief candidates, or **explicitly cancel** — repeating until a destination succeeds or the player stops. **A cancel option is always offered, distinct from "every candidate exhausted"** — a player is never forced to take a worse-than-doing-nothing legal option just because one technically exists.
- **Cost already paid is never returned, in any outcome — mana and the card alike.** The mage-with-loaded-power framing: once the cost is committed, the player can redirect where it goes, never get the investment back. This was checked explicitly against one tempting softening — discharging the card only once a trace is actually created, so a total failure would at least leave the card in hand — and that was **rejected**: since discard timing is otherwise unconditional (D37/D46, unchanged), deferring it would let any narrowly-targeted spell (one with essentially one sensible candidate, e.g. most "target creature" removal/control-change effects) become a repeatable, low-cost "does something hide here" probe — pay mana, learn whether the target is safe, keep the card regardless when it isn't. The exploit concentrates exactly on single-candidate spells, which are both common and exactly the category most likely to be used as a surgical probe, so there is no safe way to grant the leniency only where it can't be abused without adding a real conditional to the base cast procedure for a benefit that only pays out where it's already safe. Card discard stays exactly where D37/D46 already put it.
- **This splits actions into two groups, not one uniform behavior.** Actions with more than one legal-per-belief candidate destination (Move: other reachable hexes; Summon: other eligible bonded/connected hexes; Relocate/push: other hexes in the effect's Area) genuinely benefit from the redirect loop. Actions with exactly one fixed destination have nothing to redirect to and reduce to a single attempt — legal, or a clean hard-fail with cost sunk: **Descend** (always "the Underground level of the hex I'm already on," no alternate hex exists — and per D67, this specific destination is blind unless the controller already has independent Underground presence there via some other permanent, which is the common but not universal case) and **control-change** (D66, unchanged — see the location-vs-permanent boundary below). Descend being a real, usually-un-mitigated gamble is accepted as thematically correct — diving into unscouted Underground territory should carry real risk — not treated as a gap needing its own mitigation. **This list is not exhaustive** — any action whose targeting rules happen to produce exactly one legal-per-belief candidate reduces the same way, Descend and control-change are just the two identified so far.

**A necessary boundary, found by pressure-testing control-change specifically: the redirect loop only ever applies to a *location*-typed choice (a hex, or a (hex, level) address) — never to a *permanent*-typed choice (a specific creature/permanent named by the effect) — regardless of what the card's own text calls either one.** "Gain control of target creature" targets a creature, not a location — redirecting to a *different* creature would silently change what the spell does (you wanted the dragon, you'd end up stealing whatever else was available), which is not the graceful "same intent, different spot" pivot the mechanism is meant to provide. This holds regardless of how many creatures are eligible at declare time — even "gain control of target creature within range 3," which does have multiple initial candidates, gets no redirect, because the *thing being chosen* is creature-shaped, not location-shaped. **Card text is not a reliable guide to which is which:** a two-part card like "Move target creature to target location" uses the word "target" for both, but only the *location* half ever redirects — the creature-target stays exactly whatever was first chosen even when the location fails and gets redirected, and if the *creature*-target were ever what failed instead, that's a clean hard-fail for that instruction (D33-style), never a fallback to trying a different creature. Move/Summon/Relocate's destination choice is always location-shaped by definition, so this doesn't change anything for them; this boundary just makes the already-implicit split explicit and unambiguous, keyed to what kind of thing is being chosen rather than to the card's own vocabulary or a per-card judgment call about "does this feel like it changes the spell's intent."

→ `interaction-stack.md`, `structures-items.md`, `ancestry.md`, `overview.md` §2/§4, `asymmetric-information.md`.

---

### D66 — A control-change that would create mixed-controller occupancy on one hex-level fails to complete; forced combat and a bespoke "Captured" state were both considered and rejected

*Replaced by D86.*

Resolves the "mixed-owner levels produced by a control change" open question flagged 2026-09-09 (`PLAN.md` §9), queued behind the Behavior-as-policy work (D60–D65) until now. The underlying collision: a hex-level's occupants must all share one controller (`Query.CanOccupyLayer`, added 2026-08-08 to stop opposing creatures sharing a hex), but a control-change effect — "gain control of target creature," "target creature becomes Neutral," any Betrayed-style effect — can move exactly one occupant to a new controller while its old level-mates stay put, transiently violating that.

Three candidates were weighed and rejected before landing on the adopted rule:

- **Forced combat** (throw the affected creature into an automatic fight with its new level-mates) — doesn't reliably fix anything, since combat is rarely lethal (damage persists, no auto-heal, D13); the invalid state can simply survive combat unchanged, so this still needs a second, real fallback regardless — it adds a new, ill-fitting mechanism (what pays its AP cost? who's attacker/defender, outside the normal declare-attack flow?) without guaranteeing a fix.
- **A "Captured" state** (two different-controller creatures may share a level only as an explicit captor/captive relationship) — rejected as a *general* mechanism: this is structurally a second location/containment system sitting next to the existing uniform (terrain, layer) model, exactly the shape of the vehicle/containment idea D32–34 already considered and rejected once, specifically to avoid a parallel location system. It would also need its own new rules for capacity counting, targeting, and whether/how a captured creature can act. Not ruled out forever as a narrow, explicitly-printed keyword that opts one specific card *out* of the rule below — just not the general answer.
- **Forced relocation** (move the new controller's creature to the nearest legal level instead) — a creature's legal layer(s) are fixed by its own card data (D32), so relocation is almost never "shift to a different layer on the same hex" but an actual teleport to a different hex, requiring a new "nearest legal hex" search (with its own tiebreak) that nothing else in the ruleset needs or reuses. It's also not total — a packed map region, or a creature whose one legal layer is full everywhere, still needs a fallback, which is fizzle anyway, so relocation only narrows when fizzle applies rather than replacing it. Worse, the destination is nobody's decision — not the caster's, not the original controller's, not printed on the card — an opaque side effect a search algorithm picks, cutting against pillar 1 (every card is a board decision) and pillar 3 (readable depth).

**Adopted: any control-change effect — targeted or not — rechecks at resolution whether its result is a legal occupancy state (every hex-level's occupants share one controller); if not, that effect fails to complete (fizzles) instead, and paid cost is not refunded.** This generalizes the existing illegal-target-fizzles-at-resolution pattern (D33) to also cover control-change effects with no target at all (e.g. "at the beginning of your end step, this creature becomes Neutral"), rather than reusing D33 literally. Judged advantages, not just an acceptable cost: zero new machinery (no search, no new stored state, no new combat trigger); total by construction (works unchanged even on a packed level or a full map, no special-casing); matches the pessimistic-default posture (D14) — the base rule shouldn't make it easy to steal or exile a creature out of a defended cluster, and a specific card can still print an explicit override (a future Capture-style keyword) if it wants to guarantee the effect regardless; and it hands every color the same uniform, learnable, in-the-moment counterplay against a "mind control"-style effect (stack your creatures together) without inventing a bespoke defensive mechanic to provide it.

**A card may still choose to print a targeting hint** ("target creature that is alone on its hex-level") as an optional courtesy to the caster — never a requirement, since the general rule above already guarantees legality on its own regardless of what the card says — and the hint can never be a full guarantee anyway: a hidden (e.g. concealed Underground) creature sharing that level is invisible to the caster's own view, so a hinted target can still turn out to violate the rule, discovered only when the effect fizzles. → `ancestry.md`, `overview.md` §2.

---

### D65 — Declare Defenders runs through the same Behavior mechanism as any other choice; Defend's cap moves from once per turn to once per round *(Defend cap replaced by D116)* — replaced parts

*Replaced by D116.*

**Separately, D15's Defend cap moves from once per turn to once per round**, a genuine rule change forced by D60: a round now contains up to four turns (a player's own, two neutral turns, the opponent's), so "once per turn" would let a single creature defend as many as four times in one round — well beyond what the original cap was ever calibrated for. Once per round keeps a creature's total defensive capacity roughly stable regardless of how many turns fall in a round, and rewards concentrating attacks on one target within a round rather than diluting the cap's effect. The reset point stays tied to the controller's own turn-start (the same cadence AP already refreshes on), not a new separate "round boundary" concept — a real player's is their own single turn per round as before; a Neutral permanent's is its assigned neutral turn. Not a special case for Neutral — the same rule, applied uniformly, just now noticeably relevant because Neutral turns exist at all. → `economy.md` (D15), `neutral-permanents.md`, `glossary.md` (Defend).

---

### D59 — Zone/domain re-leveled to two tiers; "section" retired; board areas renamed "home ground" / "neutral ground" — replaced parts

*Replaced by D138.*

Collapses the three-tier **domain → zone → section** vocabulary (an artifact of D37/D38, never a deliberate design) into two tiers:
- **Zone** is the container an object actually sits in — the seven previously called "sections": **Hand, Library, Discard, Past, Pending, Future, Island**. D16's "every object is in exactly one zone at a time" is now literally true as written, rather than true only of the pre-D37 four-item list (Hand/Library/Play/Aether).
- **Domain** groups the zones into three families — **Mind, Aether, Matter** (D37's narrative frame, kept). Still never queried by a card or effect, but rules text now uses it for one structural invariant: a **card** exists only in a **Mind** zone, a **trace** only in an **Aether** zone, a **permanent** only in the **Matter** zone. The Idea→Action→Reality / past→present→future story is unchanged.

**"Section" is dropped** as a term. **"Card Zone" is dropped** — the Mind domain's zones are just Hand/Library/Discard, and in practice you name the specific zone or say "card". The D37 phrasing "the Island is simultaneously the Zone and its own single Section" goes away: Matter is a domain with exactly one zone, the Island. "The Aether" survives as informal shorthand for "whichever Aether zone."

Does **not** reopen D16 or D36: Hand, Library, and the three Aether zones stay distinct zones — they are simply no longer bundled under a "Card Zone" / "temporal" wrapper. D36's spatial-vs-temporal doc grouping is subsumed by the domain layer.

Purely terminological — no object moves, no timing changes, no card-interaction changes. No engine impact today: the code has no zone model yet (`PlayerState` holds only `Hand`/`Library`; the full Aether/Discard/Island model is unbuilt) — when it is built it should follow this seven-zone / three-domain shape. → `../overview.md` §1/§5/§7, `../glossary.md`, `../economy.md`, `../interaction-stack.md`, `../map.md`, `../resources-terrain.md`, `PLAN.md`.

---

### D57 — Summoning sickness (and Haste) is determined by casting, not by Actor/Object type *(superseded by D89)*

*Replaced by D89, D160.*

Supersedes D47's "Objects enter play with full Activation Capacity immediately, unlike an Actor." The actual determining factor was never Actor-vs-Object — it's whether the permanent entered the Island through the **casting/Aether procedure** (D33/D46) or was placed as match-setup data. **Creature, Companion, and Structure are all cast**, so all three now enter with **0 Activation Points** (summoning sickness, D14), refreshing at their first eligible refresh; **Haste** (D14's existing positive keyword) now applies uniformly to any of them, not just Creature. **Terrain** (placed via a map's home zone/neutral area) **and the Champion** (placed at match setup) never go through the casting procedure at all, so they were never subject to summoning sickness in the first place — not a type exception, the rule about casting simply never applied to them; the Champion's own separate D48 starting-AP asymmetry is an unrelated, additional balancing lever. A Ruin/Grave, being a transformation of an already-in-play permanent rather than something newly cast, is likewise unaffected. → `economy.md`, `structures-items.md`, `resources-terrain.md`.

---

### D56 — Behavior: mechanism, scope, and Neutral Phases — replaced parts

*Replaced by D60, D62, D162.*

- **No persistent, generalized Belief State object.** Anything a specific Behavior needs to remember beyond what's currently live-derivable (a current target, a Patrol's progress, a Flee's remembered destination) is explicit, per-keyword-scoped stored data declared by that keyword's own definition — not a generic mirror of the observer-view system. A Neutral permanent has no memory of its own the way a player's own reasoning backs `asymmetric-information.md`'s belief model — whatever it needs to remember has to be named and stored explicitly by its Behavior.
- **Every Behavior keyword must define a complete algorithm**, covering every reachable case explicitly (draft §6, kept) — there is no engine-level default resolution for an un-enumerated case (e.g. "target exists but no hex makes progress toward it"). If the same boilerplate branch recurs across many keywords later, that's the signal to extract a shared clause — not something pre-built now. Behavior algorithms should stay computationally cheap per decision step, both to avoid visibly slow phases and to keep the next point's information-parity requirement actually enforceable.
- **Neutral Phases: two per round**, one attributed to each player, always processed unconditionally every round regardless of whether anything is assigned to either — required specifically to avoid leaking "does an uncontrolled thing exist" through the mere occurrence or timing of the phase (redaction happens at the normal per-observer event layer, same as everything else hidden in the game; a residual wall-clock timing side-channel from expensive Behaviors is an implementation/architecture concern, not a rules one). Exact placement in the phase list (a freestanding phase between the two players' turns, vs. nested inside each player's own turn before End) is **left open** — it has a real consequence for whether a Neutral action counts as happening "during" a player's turn for other card text, to be settled against a concrete card rather than decided abstractly.
- A permanent's assigned Neutral Phase is a full mini-turn for it: an Activation Points refresh (mirroring Beginning), then its Behavior runs — reusing the same refresh mechanism player turns already use. Its actions go through the exact same pipeline a player's own action would (Move/Attack still create real traces, open the same priority windows) — no bespoke non-interactive resolution path.

---

### D49 — Champion/Companion Attack unifies with the generic Actor default

*Replaced by D116, D134.*

Champion and Companion Attack drops its bespoke figures (previously `5!AP` connected / `3!AP` disconnected for both) and instead **is** the generic Actor default: `3!AP` base, doubled to `6!AP` while network-bonded — the same formula as a plain Creature's Attack (D10) plus the existing bonded-cost-doubling pattern (D9/D22). A real simplification, not renumbering. → `../champions.md`, `../companions.md`.

---

### D48 — First-player Champion enters with reduced starting AP

*Replaced by D76, D114.*

New first-move-advantage balancing lever: the Champion of the player who goes first enters play with **reduced** starting AP (placeholder: 4) instead of its full Activation Capacity; the second player's Champion enters with full Activation Capacity as normal. Exact number deferred to tuning. → `../champions.md`, `../economy.md`.

---

### D47 — Activation Capacity extends to non-Actor Objects, additive to the ownership tree *(superseded in part by D57–D58 — see those entries)*

*Replaced by D57, D58, D87.*

**"Activation Capacity"** is the formal name for the existing implicit AP-max concept (D10/`../economy.md`) — a pure rename for Actors. **New:** non-Actor Objects (Terrain, Structure, Ruins, Graves) now also carry their own Activation Capacity (default 1) and AP, purely to self-gate "has this printed ability already fired this round" — reusing the existing AP-refresh-at-Beginning-phase mechanism instead of a bespoke once-per-turn clause or a tap/untap subsystem. **Additive** to `../ancestry.md`'s funding/payment-walk model, not a replacement: the bonder still pays the mana cost as before; the Object's own AP only answers "already used this round," independent of who funds it. Closes a real gap — nothing previously capped how many times a Terrain/Structure ability could fire per round. Default is once/round (pessimistic default, D14); a specific card may print a higher Activation Capacity for a deliberately repeatable ability. **Objects enter play with full Activation Capacity immediately** (no summoning-sickness delay), unlike an Actor, which still enters at 0 AP (D10/D20) — a pragmatic type-based split, accepted for now; a more general rule (keyed on *what the AP can be spent on* rather than object type) was considered and parked, not adopted. **Graves may carry their own printed activated abilities** under this mechanism — a new, intentional "graveyard matters" design space. → `../economy.md`, `../ancestry.md`, `../resources-terrain.md`, `../structures-items.md`.

---

### D46 — Casting/activating: pay cost before instantiating; legal-target precondition required

*Replaced by D130, D135.*

**Supersedes D33's ordering.** Playing a card or activating an ability now pays its complete cost **before** choosing targets/modes — reversing D33's target-then-pay order. Necessary for X-costs and cost-scaled modal effects, where available choices depend on what was actually paid. Formalized as two stages: **cost instantiation** (paid cost fixes available values/modes) then **effect instantiation** (remaining choices, incl. targets). **Precondition:** a card/ability is only offered as a legal play if at least one legal cost-and-target combination exists — no paying into a dead cast; matches how legal-command enumeration already has to work. D33's fizzle-on-illegal-target-at-resolution rule is unchanged; only the pay/target order at cast time moves. → `../structures-items.md` (rewrite casting procedure), `../interaction-stack.md`.

---

### D44 — Terminology: retire "Unit," adopt Actor/Object, confirm "permanent" spans both

*Replaced by D103, D136.*

The glossary's "Unit = synonym for Creature" is retired. **Actor** = {Champion, Companion, Creature} — the three types holding their own AP, acting under their own power (already used informally in `../structures-items.md`'s "unlike the three actors"). **Object** = {Structure, Item, Grave, Ruin, Terrain} — everything else, needing funding/direction from something else. **Permanent** = Actor or Object, i.e. everything on the Island — confirmed to match `../ancestry.md`'s existing usage, no change there. **"Actor" is tentative** — adopted because it's already load-bearing in the docs, not because it's fully liked; revisit naming later. Note: D47's Activation Capacity for Objects blurs the old "Actor has AP, Object doesn't" boundary somewhat — flagged, not a blocker. → `../glossary.md`, `../ancestry.md` (terminology only), `../structures-items.md`.

---

### D41 — Levels renamed: Ground/Above/Below → Surface/Air/Underground

*Replaced by D78.*

Pure relabeling, no mechanical content — same three levels (D12), same capacities. → `../overview.md` §2, `../glossary.md`, all cross-references.

---

### D40 — Structure/Ruin capacity: two slots per hex, superseding D31; Ruins occupy their slot *(Ruins dropped by D111)*

*Replaced by D111.*

**Supersedes D31.** A hex now has **two** Structure/Ruin slots, not one-per-layer: a shared **Surface** slot (Ground+Above combined — Air isn't expected to be structure-dense enough to need its own) and a separate **Underground** slot (concealment kept as its own dedicated axis). Legal cast layer(s) are still card-data-driven (D32, unchanged). Accepted consequence: a Ground and an Above Structure can no longer coexist on one hex either, since they now share a slot. **A destroyed Structure leaves a Ruin in whichever slot it occupied, and — unlike a Grave — a Ruin occupies its slot.** Building a new Structure directly onto a ruined slot replaces the ruin in one action (no separate clear step); a ruin can also be cleared directly by an explicit effect, independent of building. Graves stay completely outside this system: free, unlimited, never competing for a slot. → `../structures-items.md` (rewrite Structure occupancy), `../glossary.md` (Ruin as distinct from Grave).

---

### D37 — Mind/Aether/Matter domains adopted; Card Zone gains a real Discard, does not reopen D16

*Replaced by D138.*

Adopts a narrative/organizational frame — **Mind** (Card Zone), **Aether**, **Matter** (Island) — each mapping to one of the three Zones (D16). Confirmed **narrative/organizational, not a new mechanical query layer** — no card asks "which domain is X in." Card Zone = {Library, Hand, **Discard**} (D37); Aether = {Past, Pending, Future} plus `Now`, a moving point, not a Section (D38, clarified 2026-09-05); **Island deliberately has no further Section division of its own** — unlike the other two Zones, it *is* simultaneously the Zone and its own single Section, since splitting physical space by a past/present distinction would be artificial (clarified 2026-09-05, corrects this entry's original "Ruins&Graves/'the rest'" framing). Card Zone and the Aether-as-domain-header are mostly documentation scaffolding — "Island" is the term actually used in practice. **A real Discard pile is added.** Does **not** reopen D16: D16 rejected a discard/graveyard specifically to avoid the MTG pattern where a card only leaves play when its permanent dies. This Discard is structurally different — the **card** discharges into Discard the instant it's cast, fully decoupled from the resulting **permanent**'s later lifecycle; a dying creature still only ever produces a Grave (an Island marker, D16 unchanged). Closes a real implementation gap: current code (`SpellPipeline.cs`, `HandCardRemovedIntent`) deletes a cast card with no destination at all. → `../overview.md` §1 (rewrite), `../glossary.md`, `../ancestry.md` (no change — reconfirmed consistent with D16 as refined 2026-08-11).

---

### D36 — Zone count reaffirmed at four; Hand/Library/Aether-merge considered and rejected; spatial vs. temporal is a doc/implementation grouping, not a zone

*Replaced by D138.*

Surfaced while restructuring `docs/rules/` around Zones as a top-level concept (post-D16): a hand-drawn timeline visualization (Traces → `now` → Hand → Stack → Library) prompted considering whether Hand, Library, and Aether should collapse into one unified zone (two zones total, alongside Play). **Considered and rejected — D16's four zones stand: Hand, Library, Play, Aether.**

Two initial objections to merging (private-per-player Hand/Library vs. the shared Stack/Trace; a Library hidden even from its own owner) turned out not to be structural blockers — both are just instances of the existing perception-layer default-visibility mechanism (already precedented by Below-layer concealment, D12), not properties that require separate zones. (Library-hidden-from-its-own-owner is a genuinely new *value* on the visibility query — "hidden even from P where P = owner" hadn't shown up before — but not a new mechanism.) The "named regions within one zone" shape proposed for the merge is also a real, valid pattern — Play already works exactly this way (Ground/Above/Below are named regions of one zone, each with its own light internal shape). Unlike Play's hex coordinates, though, nothing today needs a literal shared coordinate space across the temporal regions — Library's order and a trace's fade window are each locally self-contained — so a merge would buy structural tidiness, not new capability.

**Decisive test:** would card text or rules text ever reference the merged zone by its own umbrella name, rather than always naming a specific region ("hand," "library," "stack," "trace")? No — so the merge doesn't earn its keep; it's a rename, not a simplification. **This same test does not extend to un-merging Stack/Trace** (already unified as Aether, D16): that merger was never justified by wanting a shared name — it's justified by object identity persisting when a resolved trace is pushed back across `now` to re-cast it (flashback-style). Aether stays one zone with two named positions, exactly as Play stays one zone with three named layers.

**Adopted instead of a zone merger:** a **documentation/implementation-level grouping** — a "spatial" view (Play) and a "temporal" view (Hand, Library, Aether) — for organizing the rules docs and engine state, without claiming there are fewer zones. → `../glossary.md`, `../overview.md` §1, `PLAN.md` §9.

---

### D31 — Structure gains a Layer; capacity becomes 1-per-layer, not 1-per-hex

*Replaced by D40, D111.*

Amends D24. A Structure's location is no longer just "the terrain cell" — it now includes a **Layer** (Ground/Above/Below, D12), and its dedicated slot's capacity is **1 per layer**, not 1 per hex: a single terrain cell can hold up to three Structures simultaneously, one per layer (e.g. a surface keep, a sky platform, and a sunken vault, all on the same cell). Mechanism is unchanged — still a dedicated slot, capacity 1, independent of creature layer capacity (D12's 3-per-layer creature cap) — just applied per layer instead of once per hex. Bonus: a Structure occupying Below is hidden by default, for free, via the same rule that already hides Below creatures (D12/pillar 6) — solves "a structure hidden inside a building/underwater" without inventing a containment mechanism. → `../structures-items.md`.

---

### D23 — Instant-speed marker (`^`) for activated abilities: default sorcery-speed

*Replaced by D45.*

Generalizes D5's card-level instant/sorcery-speed split down to the **ability** level, and removes the need for a Champion-specific "can abilities be reactive?" special case (supersedes the former Open question 3 in `../champions.md`). **Default: sorcery-speed** — any AP-costed ability (creature, Champion, or Companion alike) is playable only during its controller's own Action phase, and only while the stack (the Aether's pending region, D16) is empty — not as a response. **`^` marks an ability instant-speed:** usable in any priority window (D5), on either player's turn, same as a card-level instant. **Composes freely with the existing AP-cost flavors** — `^2*AP`, `^3!AP`, `^1AP` are all valid: `^` governs *when*, `!`/`*` govern *how the AP is consumed*, independent axes. **No new resource:** an instant-speed ability still spends the actor's own AP from its normal (refills-on-your-turn-only) budget, so acting reactively means that AP had to be held in reserve since the actor's own last turn — reactive plays cost you offense earlier, which is presumably the intended tension, not a gap (flag for playtest). → `../economy.md`, `../champions.md`.

---

### D19 — Cross-layer combat: initiation is layer-restricted, retaliation is universal *(table revised and defenders defined by D115)*

*Replaced by D115, D127.*

Default attack **initiation** legality (attacker → target): **Ground→**Ground ✓, Flyer ✗, Sub only-if-located. **Flyer→**Ground ✓, Flyer ✓, Sub only-if-located. **Sub→**Ground ✓ *(tentative, balance)*, Flyer ✗, Sub ✓ (if located). Subterrain is **hidden by default** (D12) → cannot be targeted unless located. All defaults; cards give exceptions (reach/anti-air/true-sight). **Retaliation is UNIVERSAL (corrects an earlier "gated by matrix" reading):** a defender **always** deals its damage back to the attacker regardless of layer — a ground creature retaliates against a diving flyer even though it can't *initiate* on flyers. Combat is mutual across layers; air superiority = an **initiative / target-selection** advantage, not damage-immunity. **Sole exception: Ranged** — a keyword making the attack **one-way** (no retaliation): "deals damage equal to its power to target creature." (Ranged combat otherwise untackled; covered by keyword later.) **Acting reveals a concealed sub:** attacking **surfaces** it → it stays **located while it remains on that hex**, and **re-conceals only when it moves away** *(interpreted from user's note — confirm next session)*. "Strike and stay hidden" is a rare positive keyword. This *is* D18 (a concealed thing risks revealing via the hard fact of acting). → `../overview.md` §4.

---

### D18 — Belief-consistency model for asymmetric information (the hard core of pillar 6) *(faces and claims recast as Pretend, D150)* — replaced parts

*Replaced by D164.*

**Collapse → full truth** by default (incl. which of a masked pool it was).

---

### D17 — Card-type taxonomy: "Spell" as umbrella; type is a queryable tag

*Replaced by D39.*

*(Superseded by D39 — the "Spell as umbrella" framing described here is no longer current; kept verbatim for history.)*
**Card type is a data-driven queryable tag (pillar 5), not a fixed class** — the engine has a type system with default behaviors per tag; cards can bend type; the type *list* is content that grows. **"Spell" = the umbrella** for anything **cast through the Aether** (matches MtG's real usage). Structural backbone = *what resolution leaves behind*:
- **Play-permanents:** **Creature** (mobile; Attack/Life/AP, D10), **Structure** (stationary, **non-carryable**, location-based; **subtypes** e.g. *Building* + others — absorbs non-equippable "artifacts"), **Item** (**carryable** — pick up / equip). Dead permanents → **graves/ruins in Play** (D16).
- **Trace-only:** **Rite** (the one-shot type; resolves to an Aether trace, no permanent).
**Outside the Spell umbrella** (pre-game / separate economy, never cast through the Aether): **Map, Champion, Terrain**. **Type-as-tag with subtypes** is adopted (not a fixed enum). ("Artifact" dissolves: equippable → Item, stationary → Structure.)
**Opens — RESOLVED 2026-08-09, see D24/D25:** Item equip/carry and Structure slot/control/funding are now designed in `../structures-items.md`. Still open there: loose-Item board footprint, un-equip mechanics, carry exclusivity. → PLAN §3.2; card-taxonomy step in `history/knowledge-capture-plan.md`.

---

### D16 — Unified zone model: Hand / Library / Play / Aether; no graveyard; objects are single-zone, linked across zones (refined 2026-08-11)

*Replaced by D37, D38, D97, D138.*

Zones: **Hand, Library** (deck), **Play** (permanents on the Board), and **Aether** — a per-observer **timeline**. The Aether unifies the **stack** and the spell-**graveyard**: a spell is one object that slides `Hand → front of the `now` marker (pending, LIFO = the stack) → across `now` → a **trace** in the past`. So stack and graveyard are the **same zone at different positions relative to `now`**, not two zones (one object moving, never copied). Abilities also leave traces. **No graveyard zone:** dead **creatures become graves in Play**, keeping a board location (spatial reanimation, pillar 2); spells live as Aether traces. Aether knowledge is **per-observer** (pillar 6) — your view of history is itself hideable. ~~The engine also allows a card in multiple zones at once (digital-only) — kept as a capability, not a default/mechanic until a concrete use earns it.~~ *(superseded below.)*
**Resolved:** (1) **Traces fade** on a **configurable** window — this is what makes the model a net *simplification* (recent past interactable, old past drops away) and a color-specialization axis. (2) **Graves take no slot** — hex-level markers (view-toggleable UI). (3) **Timeline distance is mechanical** — a spell can push a trace **into the future** to **re-cast** it; such time-manipulation cards are **deferred** (start simple) but the axis is real, keep it open. (4) ~~Multi-zone, concretely: casting a Play-permanent leaves both a permanent in Play and an anchored summon/build trace in the Aether.~~ *(mechanism superseded below; the kill-vs-un-summon outcome it describes is unchanged.)* (5) **Terminology confirmed:** zones are **Hand / Library / Play / Aether**; **Board** stays the spatial grid; **Discard/Graveyard deleted**; card text says *traces*/*graves*, never "graveyard." → glossary, `../overview.md` §1, `../interaction-stack.md`.

**Refined 2026-08-11 — object identity across zones, supersedes "multi-zone" above (kept struck-through for history).** Surfaced while restructuring `docs/rules/` around Zones as a top-level concept: "a card in multiple zones at once" was the wrong mechanism for an outcome that's still correct. Corrected model: **every object lives in exactly one zone at a time.** An object in one zone can *create* new objects in other zones, linked to it — but it is never itself present in two zones, and identity does not survive a zone crossing (an object doesn't relocate between zones; the source object is consumed and a new, linked object is created in the destination zone). Concretely: casting a Creature/Structure/Item consumes the Hand object and creates a new Aether trace object; when that trace resolves (crosses `now`), it does **not** relocate into Play — it stays in the Aether as a past trace, and its resolution *spawns* a separate, linked Play-permanent object. This reproduces D16's original kill-vs-un-summon design exactly, just as two objects instead of one: **kill** targets the Play object directly (Life → 0 → grave — an in-zone state change, not a zone crossing, so the same object stays put); **un-summon** deletes the linked Aether trace, which cascades to delete the linked Play object with no death trigger, only while the trace hasn't faded. A token-creating effect (e.g. a Companion's built-in summon ability resolving) creates a Play object directly with no card-lineage at all — zero-origin creation, not an exception to the model. → `../glossary.md`, `../overview.md` §1, `../structures-items.md` D33.

---

### D15 — Defend rule: RESOLVED to `0*AP` — free, but once per turn *(superseded by D116: Defend `1~AP`, Attack `3~AP`)*

*Replaced by D116.*

**Resolved 2026-08-09**, superseding the two-variant playtest below (kept for history). Defending costs **`0*AP`**: free (no AP check at all, in either direction), but gated to **at most once per turn per actor** via the same `*` once-per-turn flavor Bond/Draw already use (`Query.CanUseOncePerTurnAction`, keyed on `CoreAbilities.Defend`) — collapses the redundant `defended`-flag + AP into the existing once-per-turn mechanism instead of a bespoke flag.

**Why not V1 (Exhaust) — a concrete bug, not just a feel-bad:** V1 gated defending on remaining AP. Since default Attack already exhausts (`3!AP`, drains the attacker to 0), the very first combat between two mirror creatures (both attack + defend once, mutual damage) leaves *both* sides at 0 AP. But AP only refreshes on its own controller's Beginning phase — the defender (whose controller's turn is next) refreshes almost immediately, while the attacker (whose controller's turn is a full round away) stays at 0 AP through the *entire* following opponent turn. In that window the former defender can attack the still-exhausted former attacker for a free, undefended kill — so attacking first was strictly worse than being attacked, an incentive *against* attacking that directly fights the intended combat-forward design. `0*AP` removes this because Defend never reads AP: an actor that spent all its AP attacking (or on anything else) can still defend later.

**Why not flat free-and-unlimited (V2) either:** V2 also fixes the bug (no AP check), but drops the per-turn cap entirely — a single creature could defend against every attack that lands on its hex in one turn, with only Life as a brake. `0*AP` keeps V1's "defend once, then done" flavor (the once-per-turn gate), just decoupled from the thing that was actually broken (the AP check), rather than throwing away the cap along with the bug.

**Escape valve for the "tap cost" flavor cards may still want:** decoupling Defend from AP means a strong ability can no longer *incidentally* deny a creature's block by draining its AP (the way MTG's tap-to-activate denies blocking). That's now an explicit, card-level, opt-in effect instead of a base rule: a printed ability's own effect can additionally consume the actor's Defend for the turn (emit the same `OncePerTurnActionUsedIntent(actor, CoreAbilities.Defend)` Defend itself uses) — e.g. "5!AP, 3 mana: draw a card **and this creature can't defend this turn**." Pure card data, no new engine mechanism, and — critically — it never applies to the base Attack ability, so the original punish-the-attacker bug can't come back through the front door.

**Cross-reference, updated 2026-08-09:** the Champion-retaliation question in `../champions.md` (Open questions, "Champion retaliation") is effectively answered by this: the Champion defends through the exact same `Query.CanDefend`/`ResolveDefendCost` as any creature, no bespoke Option A/B needed. Note this **reverses** that question's originally-stated "desired shape" ("a Champion that spent its whole turn's resources elsewhere should no longer be able to retaliate") — under `0*AP` it *can* retaliate regardless of what else it spent AP on, once per turn, unless a specific card explicitly consumes its Defend as described above. → `../champions.md`.

**Superseded playtest packages (kept for history):**
- **V1 — Exhaust (`1!AP`):** defending costs `1!AP`. Rejected — see the bug above.
- **V2 — Delete defend-once:** defending free and unlimited, no per-turn cap; Life the only limiter. Rejected in favor of `0*AP`, which keeps V2's fix but restores V1's per-turn cap.

**Alternative not adopted, kept for a future playtest:** a "recovery pulse" idea — Defend stays AP-gated (back to `1!AP`), paired with a new general rule (every actor regains `1AP` at the *opponent's* Beginning phase too, not just its own) and a new `x!!AP` cost flavor ("double-exhaust" — skips that pulse). Fixes the same V1 bug from a different angle (closes the refresh-timing gap instead of decoupling Defend from AP) and avoids ever special-casing Defend, but touches the core AP-refresh rule and adds a third cost-flavor axis. Full writeup: `history/playtest-variants.md`.
→ `../overview.md` §4, `../economy.md`.

---

### D9 — Champion runs on mana + Action Points, like a creature; killing it wins — replaced parts

*Replaced by D77; the Channel model was already replaced within D9.*

**Mana snapshot + pre-bonded start — under test (2026-08-08).** Mana is refreshed **once per Beginning phase only** (`RefreshManaEffect`) — a bond made mid-turn is permanent but its mana isn't credited until the *following* Beginning phase, the same pessimistic "no retroactive unlock this turn" shape D14 already uses for summoning sickness. Paired with this: **both Champions now start match setup pre-bonded to their own home tile**, so 1 mana is live from turn 1 instead of needing a live Bond + a full turn cycle first — and, combined with the tightened movement rule above, a fresh Champion is *already rooted* from turn 1. → `../champions.md` §"The Champion action economy."

**Superseded model (kept for history):** originally the Champion had **two independent economies** — mana, and a single **Channel** action per turn spent on exactly one of {bond a terrain, act as a creature (move/attack), activate a Champion ability}, with bond-vs-activate as the central tension. Replaced above because a Champion-only "Channel" resource duplicated the AP model creatures already use (pillar 5) — the Champion is now simply a special creature, not a third combat model.

---

### D8 — Resource system: terrain deck connected outward from the Champion — replaced parts

*Replaced by an outdated implementation note.*

**M1 debug-UI implementation note (2026-08-08, scoped to the current placeholder board, not a reversal of the terrain-deck/8-color vision above):** M1's board has no terrain deck yet — every scenario just marks a few hexes as "terrain" so `CanBondTo` had something to gate on, which read as an arbitrary, unexplained restriction once rendered (green hexes with no in-game meaning). Two simplifications for the current build only: (1) **any** hex is bondable — the terrain-type gate is dropped until a real terrain deck exists to make that distinction meaningful; (2) **before a Champion has bonded anything, the only legal first bond is its own current tile** — the network must root at home before spreading outward (previously the first bond could be any adjacent cell). Consequence worth flagging: `Query.ResolveConnectedProducingTerrain`'s BFS only ever tested a root's *neighbors*, so a Champion standing on its own bonded tile wouldn't have counted as connected to it — fixed alongside this change (`Query.IsBondedAndReachable`) so the home-tile bond produces mana immediately. → `src/Leyline.RulesCore/Queries/Query.cs`.

---

### D7 — Default visibility is full

*Replaced by D164.*

Board is fully visible by default; all concealment (Mist/Submerged/Mimic) is opt-in via cards. Simplest default that still exposes the perception hook. → `../asymmetric-information.md`, `../overview.md` §9.

---

### D5 — Interaction model: priority + lightweight stack (players can act on the opponent's turn)

*Replaced by D45.*

The engine has a real priority/stack capability (LIFO, responses-to-responses). Kept tame by: (1) priority only at **defined windows** (something goes on the stack; a Combat is declared); (2) **most default cards are sorcery-speed**, so the stack is invisible to new players. Unifies instants, combat tricks, and traps into one mechanism. **Biggest complexity commitment so far**; accepted because the user wants MTG-style interaction and it feeds the deception pillar. → `../interaction-stack.md`.
