<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 8 Extension | Terrain Buff & Resource Grant System
Status: Stage 8-Q complete. Multi-cell terrain, movementCost, and
terrain entry log all working.
Task: Wire the terrain on-enter buff and resource grant systems.
Fields buffOnEnterTag, buffDurationRounds, resourceGrantTag, and
resourceGrantAmount already exist on TerrainCellSO and TerrainCellState.
The TODO stubs exist in CombatManager.TryMoveHunter. This session
resolves both stubs into working code.

Read these files before doing anything:
- .cursorrules
- CLAUDE.md
- _Docs/Stage_08/STAGE_08_TerrainFX.md
- Assets/_Game/Scripts/Core.Data/TerrainCellSO.cs
- Assets/_Game/Scripts/Core.Data/CombatState.cs      (TerrainCellState)
- Assets/_Game/Scripts/Core.Systems/CombatManager.cs (TryMoveHunter stubs)
- Assets/_Game/Scripts/Core.Logic/StatusEffectResolver.cs

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 8 Extension: Terrain Buff & Resource Grant System

**Prerequisite:** Stage 8-Q complete — terrain infrastructure, movementCost, and footprint expansion all working
**Done when:** Stepping onto a buff terrain cell applies the named StatusEffect for the specified duration; stepping onto a resource terrain cell grants the named resource (first entry per combat only); both confirmed via Debug.Log
**Commit:** `"8-TerrainFX: terrain on-enter buffs and resource grants"`

---

## What Already Exists

From Stage 8-Q, `TerrainCellSO` already has:

```csharp
public string buffOnEnterTag;       // e.g. "Energized", "Shielded"
public int    buffDurationRounds;
public string resourceGrantTag;     // e.g. "Bone", "Hide"
public int    resourceGrantAmount;
```

`TerrainCellState` mirrors these fields for JSON persistence.

`CombatManager.TryMoveHunter` has two commented-out TODO stubs immediately after the terrain entry log.

---

## Part 1: Buff On Enter

### 1-A: Parse the status effect tag

`StatusEffect` is an enum. The tag stored in `buffOnEnterTag` must map to a `StatusEffect` value.

```csharp
if (!string.IsNullOrEmpty(terrainAtDest.Value.buffOnEnterTag) &&
    System.Enum.TryParse<StatusEffect>(terrainAtDest.Value.buffOnEnterTag, out var effect))
{
    ApplyStatusEffect(hunterId, effect, terrainAtDest.Value.buffDurationRounds);
    Debug.Log($"[Terrain] {hunter.hunterName} gained {effect} " +
              $"for {terrainAtDest.Value.buffDurationRounds} rounds from {terrainAtDest.Value.terrainId}");
}
```

Replace the first TODO stub in `TryMoveHunter` with this block.

### 1-B: Confirm StatusEffectResolver ticks the buff

`TickAfterAction` in `StatusEffectResolver` decrements duration-based effects. Confirm it handles the terrain-applied buff identically to card-applied buffs — it should, since both use the same `activeStatusEffects` string array.

### 1-C: Mock asset — add a buff terrain cell

Create `Assets/_Game/Data/Terrain/Terrain_AncientAltar.asset`:
- `terrainType`: Bonus
- `buffOnEnterTag`: "Energized" (or whichever StatusEffect represents heightened readiness)
- `buffDurationRounds`: 2
- `cssClass`: "grid-cell--terrain-high" (reuse high-ground tint for now — add a dedicated one if needed)
- Add to `TerrainSetup_GauntArena` at a reachable position

---

## Part 2: Resource Grant On First Entry

Resource grants fire **once per combat per cell** — re-entering the same cell should not double-grant.

### 2-A: Track visited terrain cells

Add to `CombatManager`:
```csharp
private HashSet<string> _terrainCellsGrantedThisCombat = new(); // key: "x,y"
```

Reset this set in `StartCombat`.

### 2-B: Grant logic

```csharp
if (!string.IsNullOrEmpty(terrainAtDest.Value.resourceGrantTag))
{
    string key = $"{destination.x},{destination.y}";
    if (!_terrainCellsGrantedThisCombat.Contains(key))
    {
        _terrainCellsGrantedThisCombat.Add(key);
        // TODO: wire to ResourceManager when available (Stage 9 resource system)
        Debug.Log($"[Terrain] {hunter.hunterName} discovered {terrainAtDest.Value.resourceGrantTag} " +
                  $"×{terrainAtDest.Value.resourceGrantAmount} at ({destination.x},{destination.y}) " +
                  $"— resource grant queued (ResourceManager not yet wired)");
    }
}
```

Replace the second TODO stub in `TryMoveHunter` with this block. The actual `ResourceManager.Instance?.Grant(...)` call goes here once the resource system is available (Stage 9 resource stage).

### 2-C: Mock asset — add a resource terrain cell

Create `Assets/_Game/Data/Terrain/Terrain_BloodPool.asset`:
- `terrainType`: Bonus
- `resourceGrantTag`: "Bone"
- `resourceGrantAmount`: 2
- `cssClass`: "grid-cell--terrain-ash"
- Add to `TerrainSetup_GauntArena`

---

## Part 3: CSS — New Terrain Types (if needed)

If new terrain visual types are required (e.g., an altar glow), add a CSS class to `combat-screen.uss` and clear it in `RefreshGrid`'s reset block.

---

## Verification Test

- [ ] Hunter steps onto buff terrain → StatusEffect applied → `[Terrain] X gained Energized for 2 rounds` in console
- [ ] After 2 rounds, StatusEffect expires (confirmed via `TickAfterAction` log)
- [ ] Hunter steps onto resource terrain → `[Terrain] X discovered Bone ×2` in console
- [ ] Re-entering the same resource cell does NOT log a second grant
- [ ] All existing terrain tests from Stage 8-Q still pass

---

## Next Session

Return to the main stage sequence at wherever this extension is inserted relative to Stage 8-R.
