# Pessimistic-Default Audit (review worklist)

*A systematic pass over D1–D21 to find rules that **violate the pessimistic-default principle** (D14): a default that **grants** a capability to everything (generous) should usually be flipped so the default is the **weak** case and the capability is a **positive keyword**. Negative card lines ("doesn't X", "can't Y") as the *fix* are the smell we're removing.*

**Status:** Open review · **Owner:** user-led (parallel task) · runs alongside implementation · **Date:** 2026-07-27

> Test for each default: *does it hand a capability to every creature/permanent for free?* If yes, and not every creature should have it, the default is probably generous — flip it and make the capability a positive keyword.

## Candidates found so far
| Current default | Concern | Recommendation |
|---|---|---|
| **D8 — any creature on a network node blocks/pauses mana** | **Generous.** Every body is a free mana-denier; a fast scout is accidentally oppressive and would need a *negative* line ("doesn't block mana") to fix. | **Flip:** default = does **not** block mana; **"Blockade"** = a **positive keyword** on units meant to deny. Also makes denial-density a per-format **tuning knob** (addresses D8's denial knife-edge). *(Strong change — recommend adopting.)* |
| **Zone of control / does a creature block enemy passage?** | Currently **undefined**. If we ever add "creatures stop enemy movement," that's a generous default. | If added, make **"blocks passage / ZoC" a positive keyword**, never a universal default. |

## Considered and intentionally KEPT (not violations)
| Default | Why it stays |
|---|---|
| **D19 — universal retaliation** | Explicit user choice; core combat symmetry. "No retaliation" is already the **Ranged** keyword (the exception). |
| **D10 — `1AP: Move` / `3!AP: Attack` defaults** | The *functional floor* (a creature that can't act is below "still-functional-and-fun"). "Rooted" / "can't attack" are occasional negative keywords. |
| **D19 — acting reveals a concealed unit** | Already pessimistic-correct (weak default; "stay hidden after acting" is the positive keyword). |
| **D4/D15 — defend once** | Already pessimistic-correct (weak default; multi-defend is the positive keyword). |

## TODO
- Sweep the remaining decisions/design notes for more generous defaults (permanents, Structures/Items, Champion abilities, perception).
- For each adopted flip, update the relevant decision (e.g. D8) + `rules-structure.md` + glossary, and add the new positive keyword to the (future) keyword library.
