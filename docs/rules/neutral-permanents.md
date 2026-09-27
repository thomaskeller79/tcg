# Neutral Permanents

*A third control state — **Neutral** — alongside Champion A/Champion B, giving every permanent type a defined answer for "what happens when nobody controls this." Every object's **decider** is its controller if it has one; otherwise its **Behavior** — a deterministic policy standing in for a player's own decisions, carried as a property of the object — if it has one; otherwise nobody, and the object is **dormant** (D83).*

**Decisions:** D54–D58, D60–D65, D82, D83 (`history/decisions.md`)

---

## Control: A / B / Neutral, for every permanent type

Fully covered in `ancestry.md` — every permanent's controller is derived by climbing its parent chain to the root, resolving to Champion A, Champion B, or Neutral. This doc covers what happens once a permanent is Neutral, and the neutral turns it acts in.

## Decider: controller, Behavior, or dormant (D83)

| Controller | Behavior property | Decider |
|---|---|---|
| a Champion | any | that Champion |
| Neutral | set (includes its neutral turn) | the Behavior — the object acts **autonomously** |
| Neutral | none | nobody — the object is **dormant** |

A **dormant** object makes no choices: its activated abilities can't be used, and a triggered ability that needs a choice (a target, a "may") doesn't happen. Mandatory, choiceless triggered abilities and static abilities still apply — e.g. a dormant Structure with "when a creature enters a terrain within distance 2 and this has at least 1 Activation Point, this deals 2 damage to that creature and its Activation Points become 0" still hits whichever creature enters.

The controller comes from the Ancestry; the Behavior never does — it is attached only by a card, a scenario, or a rule (§Becoming Neutral).

## Permanent identity: timestamp and ID

Every permanent on the Island carries two identifiers, assigned when it enters:

- **Timestamp** — shared by every permanent produced by the same creation event (one effect resolving, or one setup placement). Card text compares timestamps: "a creature that entered the Island before another target creature" is true, false, or — for two permanents from the same event — neither, since they share a timestamp and neither entered before the other.
- **ID** — always unique, assigned as a monotonically increasing counter, no ties ever. This is the engine's own tiebreak, used to order several Neutral permanents' simultaneous Behavior triggers (oldest ID first) and as the final tiebreak inside a Behavior's own scoring.

A permanent that re-enters the Island (recast after dying, replayed after being bounced) gets a new timestamp and a new ID — it's a new permanent, not a continuation. A permanent that only changes control keeps both.

Both identifiers are **true-state only** — never exposed as raw numbers to a Champion, since a visible, globally-sequential value would leak how many hidden permanents exist between two visible ones. Card text and Champion-facing views only ever speak in terms of relative order ("entered before," "earliest," "latest") among permanents that observer already knows about.

## Neutral turns

Each round is four turns: `Champion A → Neutral A → Champion B → Neutral B → …`. **Neutral A** and **Neutral B** are genuine participants in the turn order — not phases inside a player's turn, not a borrowed player identity — each a full turn with its own Beginning (Activation Points refresh for every permanent assigned to that seat, and its own mana-pool refresh for any Neutral Companion assigned to it), Action (Behaviors run), and End (end-of-turn triggers, and expiry of any "until end of turn" effect created during it).

**Both neutral turns always occur, every round, unconditionally** — regardless of whether anything is currently assigned to either — so a neutral turn's mere occurrence or timing never reveals whether a hidden Neutral permanent exists; anything that happens during it is redacted per-observer exactly like everything else hidden in the game (`asymmetric-information.md`).

**Turn order, priority, and simultaneous-trigger ordering are the standard multiplayer rule** — active-first, then each other participant in turn order (`Champion A, Neutral A, Champion B, Neutral B`, wrapping) — applied uniformly to all four seats. A neutral turn's Behavior triggers, and any real Champion's response windows during it, follow this same order; Neutral A/B never hold priority to cast anything of their own, they simply take their place in the order.

**Neutral A and Neutral B are turn-order participants only — never "Champions" for any other purpose.** They have no life, hand, or library; they are never a legal `target player`; they are never counted by "each player." Control is still, and only ever, Champion A / Champion B / Neutral (`ancestry.md`) — the two seats name *when* a Neutral permanent acts, never *who* controls it.

A permanent's assigned neutral turn is chosen by whatever effect made it Neutral (or spawned it directly as one), persists until reassigned together with its Behavior (§Becoming Neutral) — while the permanent is controlled it is simply unused — and, for a permanent with no card effect behind it, is assigned by the mission/scenario instead (`PLAN.md` §9).

## Behavior

A Neutral permanent carrying a Behavior, and capable of acting — it holds its own Activation Points (`economy.md`) and has at least one legal action available to it — follows that **Behavior**: a named keyword (e.g. `Aggressive toward X`) that expands to a fixed, deterministic decision policy, the same way any other keyword (Flying, Ranged N) expands to fixed rules text. This is card-text vocabulary only, never exposed as such.

### Scope

Only **choice-bearing abilities** need a Behavior's involvement:

- **Activated abilities** — including the basic Move/Attack/Defend every eligible permanent already carries, plus any printed activated ability — always need policy coverage, since activating one and choosing its targets is always a decision.
- **Triggered abilities with a choice** (a target to pick, an optional "you may" clause) need policy coverage for that choice.
- **Static abilities and mandatory, choiceless triggered abilities never need any Behavior coverage at all** — they simply happen, automatically, exactly as they would for a Champion's own permanent.

Any permanent holding Activation Points (i.e., anything but Item) is eligible for a Behavior, provided the ability it needs is self-payable — an Activation-Points-only ability is always self-payable by a Neutral permanent; a mana-inclusive one still needs a live root supplying mana (e.g. a Neutral Companion's own pool).

### Modes

A Behavior is an ordered, mutually-exclusive set of **modes**. Each mode is:

1. **A trigger condition**, evaluated against live state.
2. **A target derivation** — which permanent or hex the mode concerns, plus its own tiebreak if the derivation could be ambiguous (e.g. "the closest creature satisfying X; ties broken by ID").
3. **A declared, bounded set of eligible action shapes** — which kinds of ability (by their effect, e.g. "deals damage to a creature satisfying the target condition," "repositions a creature") the mode is allowed to consider. This bounds the candidate pool a mode ever looks at — it is never "every legal action this permanent could take." An unscoped pool lets an unrelated granted ability win a step purely by scoring well on some later criterion the keyword's author never meant it to compete on, and its cost only grows as more effects accumulate on a long-lived permanent.
4. **A priority-ordered list of scoring criteria**, used to rank the candidates the eligible action shapes currently produce, plus a final tiebreak for an exact tie.

Modes are **live-derived**, not stored: the active mode is whichever trigger currently holds, recomputed fresh at every decision point, never cached. A mode *may* declare a memory field it stores and re-validates each step (the same pattern used for a "current target," below) when it deliberately wants stability against flip-flopping between equally-good options — but the mode selection itself is never a stored state machine by default, since a stored machine requires an exhaustive, hand-authored escape edge out of every state for every way its trigger could stop holding, and a missed edge leaves a permanent stuck pursuing a goal that's no longer live.

If no mode's trigger holds, or every currently-triggered mode's candidate set is empty, the permanent does nothing this decision point.

A mode's trigger condition may test the permanent's own type, so one Behavior keyword can act differently when carried by a Companion than by a creature — written into the keyword's own definition, not a separate variant.

### Selecting abilities by effect shape, not by identity

A mode's candidate generator queries the permanent's **currently legal activated abilities matching its declared effect shape** — the same live legal-action enumeration the engine maintains for any actor — never a specific named ability. This is re-run fresh at every decision point, off current state including every active modifier, so:

- an ability granted by another card's effect is simply present in the next query's results, usable by any mode whose declared shape it matches, with no change to the keyword;
- an ability removed is simply absent, and any mode that had nothing else in its candidate set falls through to the next mode;
- a cost or range change that makes something unaffordable or unreachable is filtered out by the same legality check that already governs everything else.

There is no "notice a change" step, because nothing about a permanent's own abilities is ever cached between decisions.

### Target selection and reconsideration

Target derivation reuses the standard legal-target query (existence, type, any stated condition, visibility/concealment, D19), with visibility measured in the permanent's own live view (D85) — the same check any attack or ability already uses. A mode's target is re-validated by this same query at every decision point; if it's no longer legal, the mode's trigger condition (which depends on a live target existing) simply stops holding, and the next decision point re-derives from scratch. This alone produces correct deception for free: if a target's true stats are disguised (Mimic), the policy acts on the claimed value like anyone else would, discovering the truth only when an actual interaction tests the claim (D18) — the same belief-consistency model that already governs Champions.

No generalized "perception" or "belief state" system exists. A range restriction is a plain distance check stated directly in a mode's trigger or target derivation. Anything a keyword genuinely needs to remember beyond what's live-derivable (a Patrol's progress along its route, a Flee's remembered destination) is explicit, named, stored data scoped to that keyword — never a generic mirror of the full observer-view system.

### Candidates are whole-turn sequences, scored, and executed one step at a time

A candidate a mode's scoring ranks is not a single action — it's a bounded **sequence** of this permanent's own actions, filling as much of its remaining Activation Points this turn as the sequence uses, generated from the mode's declared eligible action shapes and hypothetically resolved step by step (each step's direct, known mechanical effect only — no recursion, no modeling of how any other agent might respond, no chaining beyond this one permanent's own current turn).

Each scoring criterion states whether it is **cumulative** (summed across every step of the sequence — e.g. total damage dealt) or **terminal** (evaluated only on the state after the sequence's last step — e.g. resulting distance to the nearest opponent, resulting concealment status).

**Execution is receding-horizon:** only the best sequence's *first* action is actually taken. It resolves exactly as a Champion's own action would — the same traces, the same priority windows, fully respondable by both real Champions. Before the next action, the whole search re-runs from scratch against live state. A multi-step plan is never committed to blindly; nothing about it survives past the one action actually taken, which is what keeps the permanent fully reactive to a response, a theft, a death, or any other change mid-plan.

### Scoring is a strict priority list

Scoring criteria are ranked lexicographically: the first-listed criterion decides the winner outright unless every remaining candidate ties on it, in which case the second criterion decides among the survivors, and so on, down to the stated final tiebreak (default: lowest ID). There is no general weighted trade-off between criteria — a higher-priority criterion wins by any margin, however small, and lower criteria are never consulted unless everything above them is exactly tied.

A criterion may be a raw magnitude (damage dealt) or a coarser boolean/threshold gate (does this candidate deal any damage at all) — coarsening a criterion this way is the tool available to a keyword that wants something closer to a trade-off without a full weighted function.

Reordering or recomposing the same handful of criteria — "prefer damage, then safety" vs. "prefer to deal any damage at all, then safety, then magnitude" — is enough on its own to produce genuinely different creature personalities from one shared mechanism; no new machinery is needed per personality.

### Declare Defenders

Defend is not a special case: it is simply another activated ability a mode can select, evaluated at the declare-defenders decision point (D4) through the same live-query-and-score machinery as any proactive action. A Neutral creature has no bespoke "always defends" rule — whether it defends, and against what, falls out of whatever mode is currently active and whatever that mode's scoring prefers, exactly as it would for any other permanent whose controller happens to be a deterministic policy rather than a person.

## Becoming Neutral: Behavior and neutral-turn assignment

A permanent capable of acting (see Scope, above) that becomes Neutral — whether entering the Island directly as Neutral, or converting from an already-controlled permanent — needs both a Behavior and a neutral-turn assignment (A or B) before it can act. Which applies depends on how it happened (D82, D83):

- **Caused by another Neutral permanent's own ability** (activated or a mandatory trigger; creation or conversion alike): defaults to inheriting both the Behavior and the neutral-turn assignment of the causing permanent — the same Behavior instance, the same turn. Both are bound into the trace when it is declared (`interaction-stack.md`, D83), so the inheritance holds even if the causing permanent is gone by resolution. Overridable by explicit card text, same as any other default below.
- **Caused by a Champion's card effect:** the effect must state both explicitly. No default.
- **Caused by a Map or scenario placing a permanent at Setup with no card effect behind it:** the mission/scenario must state both explicitly. No default.
- **Caused automatically by an Ancestry change** — the subtree update that accompanies removing or re-parenting a node (`ancestry.md` §Removing an inner node, D83). It assigns a Behavior to creatures only, and only to those whose controller changes from a Champion to Neutral:
  - **A still-Champion-controlled Companion dies:** each creature it funded defaults to the neutral-turn seat already paired with its controller at that moment in turn order (Neutral A if it was Champion A's, Neutral B if Champion B's), and a Behavior of `Aggressive toward` the *other* Champion — not its former controller. A Companion already Neutral when it dies changes nothing: its subtree was Neutral already.
  - **A Companion is made Neutral (D55):** its creatures become Neutral with it and are assigned the Companion's own Behavior instance and neutral turn — the one the converting effect gave the Companion. A conversion effect names exactly one Behavior; card text wanting something different for the creatures states it explicitly (D84).

Terrain, Structure, Ruin, Grave, and Item never receive a Behavior this way — losing control through the Ancestry (including a terrain whose Bond edge is inactive or removed) leaves them **dormant** until something restores control, unless they already carry a Behavior (e.g. placed by a scenario), which then decides for them (D83, D84).

**A Behavior is never removed by gaining a controller.** While controlled, a permanent keeps its Behavior and neutral turn unused — the controller decides. When it becomes Neutral, a newly assigned Behavior and neutral turn replace the old pair; if nothing is assigned (e.g. a temporary control effect ends), the old pair decides again (D84). A card that wants a permanent to lose its Behavior says so.

A Neutral permanent entering the Island enters with 0 Activation Points like any other (D89, `economy.md`) and can't act until its first Activation Points refresh.

## Invariant vs. mutable

- **Invariant:** every permanent's controller is exactly one of Champion A / Champion B / Neutral. A permanent capable of acting while Neutral always follows a complete, deterministic Behavior — never a player's ad hoc choice on its behalf. Both neutral turns always occur every round, unconditionally. A Behavior only ever reasons about its own current turn's own remaining Activation Points — never another agent's response, never its own future turns.
- **Mutable (card-driven):** which permanents can become Neutral and how; which Behavior a specific instance runs; which neutral turn it's assigned to.

*(Resolved questions are cut once closed — the rule lives above and, for decision-grade calls, in `history/decisions.md`. Only genuinely open items stay listed above.)*
