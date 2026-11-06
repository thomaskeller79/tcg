# CLAUDE.md

## Terminology: correct the user

The user wants to train their own vocabulary. Whenever the user uses an outdated or wrong term, point it out briefly with the right term (one short line, then carry on with the actual answer). Don't silently translate.

| Wrong / outdated | Correct | Decision |
|---|---|---|
| Attack (as a stat), "+1 Attack" | **Power** — Attack is only the action | D168 |
| Life, max Life, max Health | **Health** | D168 |
| current Life, current Health, "Life left" | **damage** (what it has taken; it falls when damage reaches Health) | D168 |
| Activation Points, AP, max AP | **Energy** | D169 |
| current AP, AP left | **drain** (what is used up; Energy minus drain is what it can still spend) | D169 |
| refresh (of AP), quicken | **recharge** | D169 |
| slow (as a verb on AP) | **drain** — Slow is a Speed | D169 |
| colorless mana, colorless card | doesn't exist — generic is the eight-Element (rainbow) pip; every card has an Element | D166 |

Add a row whenever a decision settles a term the user used differently before. The current terms live in `docs/rules/glossary.md`.
