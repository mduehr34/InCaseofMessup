<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 2-A | Interface Definitions (Approval Gate)
Status: Stage 1 complete. All 15 SO classes compile. Mock
data assets verified in Editor.
Task: Create the three interface files ONLY. No implementations.
After creating them, present them clearly for approval before
this session ends. Do not write GridManager.cs, CombatManager.cs,
or MonsterAI.cs yet.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_02/STAGE_02_A.md
- Assets/_Game/Scripts/Core.Data/Enums.cs

Then confirm:
- The 3 files you will create
- That you will NOT write any implementations this session

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 2-A: Interface Definitions (Approval Gate)

**Resuming from:** Stage 1 complete  
**Done when:** All 3 interfaces compile, are presented for review, and explicitly approved before session ends  
**Commit:** `"2A: IGridManager, ICombatManager, IMonsterAI interfaces defined"`  
**Next session:** STAGE_02_B.md — only after interfaces are approved  

---

## Why This Session Exists

Per `.cursorrules` INTERFACE-FIRST rule: the public API for Grid, Combat, and Monster AI must be approved before any implementation is written. Changing an interface after an implementation exists causes cascading rework. This session costs 30 minutes and saves days.

---

## Supporting Types — Add to DataStructs.cs First

Before creating the interfaces, add these types to `Assets/_Game/Scripts/Core.Data/DataStructs.cs`:

```csharp
// Add inside namespace MnM.Core.Data

[System.Serializable]
public class GridOccupant
{
    public string occupantId;       // hunterId or "monster"
    public bool isHunter;
    public int gridX;
    public int gridY;
    public int footprintW;          // For monster: 2 or 3. For hunter: always 1
    public int footprintH;
}

public interface IJsonSerializable { }  // Marker interface for all runtime state classes
```

---

## Step 1: IGridManager

**Path:** `Assets/_Game/Scripts/Core.Systems/IGridManager.cs`

```csharp
using UnityEngine;
using MnM.Core.Data;

namespace MnM.Core.Systems
{
    public interface IGridManager
    {
        // Constants
        int GridWidth { get; }          // 22
        int GridHeight { get; }         // 16

        // Cell queries
        bool IsOccupied(Vector2Int cell);
        bool IsInBounds(Vector2Int cell);
        bool IsDenied(Vector2Int cell);         // Spear card movement denial
        bool IsMarrowSink(Vector2Int cell);     // Hazard tile
        GridOccupant GetOccupant(Vector2Int cell);

        // Placement
        void PlaceOccupant(GridOccupant occupant, Vector2Int cell);
        void RemoveOccupant(string occupantId);
        void MoveOccupant(string occupantId, Vector2Int destination);

        // Arc / facing
        // Returns which arc the attacker is in relative to the target's facing
        FacingArc GetArcFromAttackerToTarget(
            Vector2Int attackerCell,
            Vector2Int targetCell,
            Vector2Int targetFacing);

        // Range & sight
        bool HasLineOfSight(Vector2Int from, Vector2Int to);
        Vector2Int[] GetCellsInRange(Vector2Int origin, int range);
        int GetDistance(Vector2Int a, Vector2Int b); // Chebyshev: diagonal = 1

        // Denial (Spear zone control cards)
        void SetDenied(Vector2Int cell, bool denied, int durationRounds);
        void TickDeniedCells();         // Called once per round end
    }
}
```

---

## Step 2: ICombatManager

**Path:** `Assets/_Game/Scripts/Core.Systems/ICombatManager.cs`

```csharp
using UnityEngine;
using MnM.Core.Data;

namespace MnM.Core.Systems
{
    public interface ICombatManager
    {
        // State — full JSON-serializable snapshot at all times
        CombatState CurrentState { get; }
        CombatPhase CurrentPhase { get; }

        // Lifecycle
        void StartCombat(CombatState initialState);
        void AdvancePhase();
        bool IsCombatOver(out CombatResult result);

        // Hunter actions
        bool TryPlayCard(string hunterId, string cardName, Vector2Int targetCell);
        bool TryMoveHunter(string hunterId, Vector2Int destination);
        void EndHunterTurn(string hunterId);

        // Monster actions — called by IMonsterAI
        void ExecuteBehaviorCard(string behaviorCardName);

        // Events — UI and other systems subscribe to these
        event System.Action<CombatPhase> OnPhaseChanged;
        event System.Action<string, int, DamageType> OnDamageDealt;   // id, amount, type
        event System.Action<string> OnEntityCollapsed;                  // occupantId
        event System.Action<CombatResult> OnCombatEnded;
    }
}
```

---

## Step 3: IMonsterAI

**Path:** `Assets/_Game/Scripts/Core.Systems/IMonsterAI.cs`

```csharp
using MnM.Core.Data;

namespace MnM.Core.Systems
{
    public interface IMonsterAI
    {
        // Deck state
        BehaviorGroup CurrentGroup { get; }
        int RemainingRemovableCount { get; }
        bool HasRemovableCards();

        // Draw and execute
        BehaviorCardSO DrawNextCard();
        void ExecuteCard(BehaviorCardSO card, CombatState state);

        // Deck manipulation — called mid-turn on part break/wound
        void RemoveCard(string cardName);

        // Group progression — called during Behavior Refresh phase
        void AdvanceGroupIfExhausted();

        // Apex trigger — called by CombatManager on first part break
        void TriggerApex();

        // Initialization
        void InitializeDeck(MonsterSO monster, string difficulty);
    }
}
```

---

## Approval Checklist

After creating the 3 interface files, present this checklist for review:

```
INTERFACE REVIEW — Stage 2-A

IGridManager:
[ ] GetArcFromAttackerToTarget takes attackerCell, targetCell, targetFacing — correct?
[ ] GetDistance uses Chebyshev (diagonal = 1) — correct?
[ ] TickDeniedCells called once per round end — correct?
[ ] RemoveOccupant takes string occupantId (not Vector2Int) — correct?

ICombatManager:
[ ] TryPlayCard returns bool (false if invalid) — correct?
[ ] OnDamageDealt signature: (string id, int amount, DamageType type) — correct?
[ ] ExecuteBehaviorCard called by IMonsterAI, not by UI — correct?

IMonsterAI:
[ ] TriggerApex is a separate method (not automatic) — correct?
[ ] InitializeDeck takes MonsterSO + difficulty string — correct?
[ ] RemoveCard called mid-turn by CombatManager, not by IMonsterAI itself — correct?

Assembly check:
[ ] All 3 files are in MnM.Core.Systems assembly
[ ] They reference MnM.Core.Data types only — no circular dependencies
[ ] Zero compiler errors
```

**Do not proceed to Session 2-B until these are approved.**

---

## Verification Test

1. Confirm zero compiler errors
2. Confirm all 3 interface files exist in `Scripts/Core.Systems/`
3. Confirm `GridOccupant` and `IJsonSerializable` added to `DataStructs.cs`
4. Present the approval checklist above and wait for confirmation

---

## Next Session

**File:** `_Docs/Stage_02/STAGE_02_B.md`  
**Covers:** CombatState and all sub-state classes — JSON-serializable, no Unity types
