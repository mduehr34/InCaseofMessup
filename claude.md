# Marrow \& Myth - Developer Workflow

## Git \& Version Control

* Before starting a new feature or logic block, ensure the current state is stable.
* Suggest a Git commit point after every successful sub-feature implementation (e.g., "Movement logic working").
* Generate the exact terminal command needed to enter for the git commit point.

## Testing \& Mock Data

* When building a new system, prioritize creating "Mock Data" (placeholder ScriptableObjects) to test the logic before final assets are ready.
* If a system involves d10 math, always ask to implement a simple Debug Log output so we can verify the calculations.
* The canonical mock scenario for combat is: **Aldric vs The Gaunt Standard, Round 1**. Always use this as the first verification pass before building new combat features.
* For settlement testing: simulate a Year 1 hunt victory → inject a mock HuntResult → run the full settlement phase → advance to Year 2 → verify save/load round-trips cleanly.
* When adding a new BehaviorCardSO trigger condition, always add a Debug.Log inside EvaluateTrigger() confirming which condition branch fired.

## Editor Tools

* When creating complex ScriptableObject structures, suggest a custom Inspector or Editor Window if the default Unity view becomes difficult to manage.
* Art generation (Anthropic image API) is an **Editor tool only** — not runtime. Build a custom Editor Window to trigger generation, preview results, and save approved sprites to the correct asset folder.

## Stage Gates

* Each stage file (STAGE\_0N\_\*.md) defines a "Definition of Done" checklist.
* Do not begin a new stage until the previous stage's checklist is fully checked off and confirmed.
* State which stage you are working on at the start of each session.
* If rework is needed in a prior stage, fix it, re-run that stage's verification test, and explicitly confirm it passes before resuming the current stage.

## Clarification Protocol

Before implementing any of the following, stop and ask for explicit confirmation:

* Any new BehaviorCardSO trigger condition not already defined in a STAGE\_\*.md file
* Any change to a public interface (IGridManager, ICombatManager, IMonsterAI)
* Any deviation from the Assembly Definition structure (no new assemblies without approval)
* Any interpretation of a Chronicle Event's "mechanicalEffect" string that isn't obviously a stat or resource change
* Any art generation prompt not derived from Appendix B of the GDD
* Any monster created using PackMonsterSO (only The Pack uses this)

## Session Opening Checklist

At the start of every session, confirm:

* Which stage am I on?
* Did the previous stage's Definition of Done pass?
* Do all SO classes from Stage 1 still compile without errors?
* Is the mock data (Aldric, Gaunt Standard SO) still valid and inspectable in the Editor?
* Am I ≥95% confident in my next implementation step?

