# Design — Interaction & the Stack

*Players can act on the opponent's turn (MTG-style instants). Rather than three separate features — instants, combat tricks, traps — we build **one primitive**: priority + a lightweight stack. Everything reactive is a use of it.*

**Status:** Design note · **Date:** 2026-07-23 · Decision: D5, D6 · relates to D16

---

## Why this exists
The user loves MTG's instants and the stack and wants reactive play. Instead of bolting on ad-hoc reaction rules, one mechanism covers all reactive gameplay and keeps it consistent.

## The primitive
> **Priority + a LIFO stack.** When something reactive happens, players get *priority* to respond; responses pile onto a stack and resolve top-down (last-in, first-out), including responses to responses.

**The stack is not a separate structure — it is the front of the Aether (D16).** The "stack" is the LIFO region *in front of the `now` marker*; as things resolve they cross `now` and become past **traces** (which then fade). So instants, the history log, and the spell-graveyard are one timeline, viewed per-observer. Nothing here changes; this just says *where* the stack lives.

## Kept tame (the simplicity levers)
The whole point is MTG-like depth *without* MTG-like overwhelm on turn one:
1. **Priority only at defined windows** — not MTG's fire-hose. Windows open when:
   - a spell/ability is put on the stack, and
   - a `Combat` is declared (pre-resolution window — see combat integration).
   Outside these windows, play is simple sequential action.
2. **Most default cards are sorcery-speed** (no stack interaction). The stack is *invisible* to a new player until they pick up a reactive ("instant-speed") card. Depth is opt-in.
3. Start the implementation with a constrained stack and widen it as cards demand.

## What one mechanism unifies
| Feature | How it's just "the stack" |
|---|---|
| **Instants / reactions** | Instant-speed cards played in a priority window. |
| **Combat tricks** | An instant played in a `Combat`'s pre-resolution window (see below). |
| **Traps** (D6) | A pre-committed, **hidden** triggered ability (pillar 6) that goes on the stack when its condition fires. |
| **Triggered abilities** | "When X happens, do Y" — put on the stack when X occurs. |

## Combat integration (recovers the "surprise pump")
Each `Combat` (see `rules-structure.md` §4) gets a **pre-resolution priority window**: attack declared → defender may respond → attacker may respond → resolve. This delivers the *"all creatures +1 attack"* blowout the user wanted — the defender commits, then you reveal — **per-combat**, without needing full board-wide simultaneous combat. Works identically whether combat is scheduled sequentially or (later) phased.

## Costs (accepted — this is the biggest complexity commitment so far)
- **AI** must evaluate responses (search over "respond vs. pass" at each window).
- **Netcode** must handle priority windows over the wire (whose priority, timeouts, holding priority).
- **State-based checks** (e.g. destroy 0-HP units) run between stack resolutions — needs a defined checkpoint.
- **Timing/UX**: players need clear, fast "respond or pass" prompts; auto-pass when a player has no legal response keeps it snappy.

## Invariant vs. mutable
- **Invariant:** a stack and a priority system always exist; resolution is LIFO; deterministic.
- **Mutable (card-driven):** what can be played at instant speed, what triggers exist, which windows a card cares about, extra priority a card grants.

## Open questions
1. **Window liberality:** exactly which events open a priority window? (Start minimal: stack additions + combat declaration.)
2. **Resource cost of instants:** do reactions cost the normal resource pool (spent on the opponent's turn), a separate reactive resource, or are they free-with-conditions?
3. **Timeouts:** in real-time online play, how long is a response window before auto-pass?
4. **Trap limits:** how many traps can be pre-set? Are they revealed on trigger only, or can they be scried/detected (pillar 6 interplay)?
5. **Full stack vs. capped depth:** allow unlimited responses-to-responses, or cap stack depth for simplicity/UX?
