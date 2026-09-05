# Neutral Permanents — Draft Proposal (NOT YET DISCUSSED)

**Status: draft, parked.** This is the user's original proposal text, preserved verbatim. It has **not** been discussed or adopted — it was deliberately set aside during the 2026-09-04 rules-merge session so the rest of that session's much larger rules draft could be worked through first. **This is the explicit first topic for the next design session.** See `PLAN.md` §8 (Track A) and `../ownership.md`'s "Open question — what does an uncontrolled creature do?" (D29), which this proposal directly answers if adopted.

Do not treat anything below as a current rule. Terminology in it (e.g. "permanent," layer names) has not been reconciled with decisions made later in the same session (D37–D50) — that reconciliation is part of the work still to do when this gets picked up.

---

## 6. Neutral Permanents — Rules Draft

1. Control

Every permanent has exactly one of three control states:

* controlled by Player A;
* controlled by Player B;
* Neutral.

A Neutral permanent is controlled by neither player.

⸻

2. Neutral Phases

Each round contains two Neutral Phases:

Player A → Neutral Phase 1 → Player B → Neutral Phase 2 → Player A → …

Each Neutral permanent acts during one of these two phases.

When a permanent becomes Neutral, the player who caused it to become Neutral chooses Neutral Phase 1 or Neutral Phase 2 for that permanent.

This assignment remains unchanged until the permanent becomes controlled or another effect changes its assigned phase.

⸻

3. Behavior

Every permanent that becomes Neutral must receive a Behavior.

A Behavior is an algorithm that determines what the permanent does during its Neutral Phase.

A Behavior may:

* select something to pursue or interact with;
* remember that selection;
* change that selection;
* determine movement;
* determine attacks;
* activate abilities;
* make random choices;
* or simply do nothing.

The Behavior itself determines when and how these things happen.

A Neutral permanent follows its current Behavior until it becomes controlled or its Behavior is changed.

⸻

4. Belief State

Each Neutral permanent has a belief state, just as each player does.

Its belief state consists of everything it currently knows about the game.

A Neutral permanent makes decisions using only its current belief state. It does not have access to information that it has not observed.

Neutral permanents therefore use the same information and observation rules as players.

⸻

5. Behavior and changing information

A Behavior may cause a Neutral permanent to make a decision based on its current belief state.

If the permanent's belief state changes unexpectedly in a way relevant to its Behavior, the Behavior may require the permanent to reconsider its current decision.

The Behavior defines what happens in this situation.

For example, Aggressive toward X may require the permanent to select a new target if it discovers a previously hidden creature satisfying X that is a better target according to its targeting rules.

A change that is irrelevant to the Behavior does not cause it to reconsider its decision.

⸻

6. Behavior algorithms

Every Behavior must define a complete algorithm.

Whenever the algorithm requires the permanent to choose between multiple equally valid options, the Behavior must specify how the choice is resolved.

Possible methods include:

* a defined priority;
* a deterministic tie-breaker;
* random selection.

A Behavior must never require a player to make a decision on behalf of a Neutral permanent.

⸻

7. Example Behavior: Aggressive toward X

Aggressive toward X causes a Neutral permanent to seek out and attack creatures satisfying X.

While the permanent has AP remaining, perform the following:

1. If there is no currently selected target, select one:
    * consider all creatures satisfying X that the permanent currently knows about;
    * select the closest one;
    * if several are equally close, choose one at random.
    * If there is no creature satisfying X within the permanent's relevant perception range, move to a random neighboring hex and repeat.
2. If the selected target cannot currently be attacked, move one hex toward it.
    * If several neighboring hexes are equally suitable, choose one at random.
    * Then reassess the Behavior using the permanent's updated belief state.
3. If the selected target can be attacked but the permanent does not have enough remaining AP to attack it, terminate the Behavior.
4. If the selected target can be attacked and the permanent has enough AP, attack it.
5. After the action, update the permanent's belief state.
    * If the belief state has changed unexpectedly in a way relevant to this Behavior, discard the current target and return to step 1.
    * Otherwise, continue using the current target.

This is still a draft; particularly "relevant perception range", "equally suitable", and exactly when a belief-state change counts as unexpected need to be defined.

⸻

8. Other example Behaviors

These illustrate the breadth of the system rather than defining final rules.

Patrol

Follow a specified path. If the algorithm encounters a creature satisfying a specified condition, attack it according to the defined attack rules. Otherwise continue along the path.

Unlike Aggressive, the creature doesn't need to select an opponent as its target. Its movement is determined by the path.

Flee toward X

Move toward a specified destination using a shortest path. If another creature blocks the chosen path, follow the behavior's specified rule for dealing with the obstruction.

The behavior could, for example, attack an eligible creature blocking the path rather than simply stopping.

Produce

A Neutral structure can have a completely non-movement-based Behavior:

Activate the creature-generating ability.

If that ability creates Neutral creatures, each newly created permanent receives its own Behavior.

⸻

9. Creating Neutral permanents

Effects may create permanents directly as Neutral.

The effect must specify:

1. the permanent's Behavior; and
2. the Neutral Phase during which it acts, or who chooses that phase.

For example:

Spawn: Create two 2/3/6 Soldier creatures in target hex an opponent controls. They are Neutral, have Aggressive toward that opponent as their Behavior, and you choose their Neutral Phase.

⸻

10. Changing control

When a controlled permanent becomes Neutral, the effect causing the change must specify its Behavior.

For example:

Betrayed: Target creature becomes Neutral. It becomes Aggressive toward its previous controller.

Homesick: Target creature becomes Neutral. It becomes Fleeing toward its home.

When a Neutral permanent becomes controlled, it immediately stops following its Behavior.
