# Match Formats

*Which sides participate in a match, and how many Champions each side runs — a different axis from `PLAN.md` §5's "game modes" (hotseat / solo-vs-AI / online, a delivery/platform question, not a format question). This doc collects format ideas as they surface; only 1v1 is actually designed and playable today.*

---

## Current invariant: 1v1, exactly two sides

The core assumes exactly two Champions, one per side (`overview.md`: "the two-Champion-seat framework is invariant for now"). Turn order, the win condition, mana pools, and perception are all written against this assumption. Any format below that changes the Champion/side count would need to revisit that invariant, not just add content on top of it.

## PvP (1v1) — designed, the default

Two players, one Champion each, alternating turns. This is the whole ruleset as currently written.

## PvE / scenario missions — committed direction, not yet designed

A player (or players) against Map-authored Neutral permanents (monster encounters, NPCs) rather than, or alongside, a second Champion. The underlying mechanism already exists — Neutral Permanents/Behavior (`neutral-permanents.md`) gives any uncontrolled permanent a deterministic policy to act under — but *placement* (how a scenario populates the Island with these encounters at Setup) and the actual mission/content layer are still open. See `PLAN.md` Track A's match-formats item.

## Two-headed giant (or other team variants) — floated, not designed

A hypothetical team format (à la MTG's Two-Headed Giant) where one side runs more than one Champion — e.g. two Champions sharing a life total but keeping separate mana pools, hands, and libraries (each teammate would still be its own network root, same shape as a Companion, `companions.md`/D8). Surfaced only as a reason to state "mana never crosses between pools" explicitly in `companions.md`, rather than leave it an accidental default. Genuinely undesigned.

## Open questions

1. **Shared vs. separate resources.** If a team format is ever pursued: one Life total or two? One Hand/Library per teammate (almost certainly, matching a Companion's own private-Hand-less shape) or something else? Mana pools already default to "never cross" per `companions.md` — does a team format ever want an opt-in exception?
2. **Turn order.** Does a team interleave turns (`Champion A1 → Champion B1 → Champion A2 → Champion B2`) or share one turn? No default chosen.
3. **Sides vs. Champions-per-side.** Does any variant ever need more than two *sides* (a true multiplayer free-for-all), or only more than one Champion *per side* (a 2v2 team)? These are different axes and haven't been distinguished.
4. **The "two-Champion-seat framework is invariant for now" line (`overview.md`).** Does a team format relax this to "two *sides*, each with 1+ Champions," or does it need a deeper rework of turn order/priority/Controller (`Champion A / Champion B / Neutral`, `ownership.md`)?
