<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 10-G | Combat Visual Polish — Animations, Part Damage States, Phase Transitions
Status: Stage 10-F complete. All scene backgrounds wired.
Task: Polish all combat visuals that currently feel static or
placeholder. Specifically: (1) hunter walk animation when moving
across the grid, (2) monster movement smoothing already lerps but
needs a walk cycle, (3) broken monster parts should visually
darken/crack, (4) overlord phase 2 transition should have a
dramatic visual flourish, (5) hunter death should play a proper
collapse animation not just a tint.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_10/STAGE_10_G.md
- Assets/_Game/Scripts/Core.Systems/CombatManager.cs
- Assets/_Game/Scripts/Core.Systems/CombatAnimationController.cs
- Assets/_Game/Scripts/Core.UI/CombatHUDUpdater.cs
- Assets/_Game/Scripts/Core.Systems/HunterTokenController.cs
- Assets/_Game/Scripts/Core.Systems/MonsterTokenController.cs
- _Docs/Stage_09/STAGE_09_C.md       ← animator controller reference

Then confirm:
- Hunter animator controllers have Idle, Walk, Attack, HitReact states (from 9-C)
- Monster tokens use SpriteRenderer with lerp movement (from Stage 8-Q)
- CombatAnimationController owns all visual effect coroutines
- Monster parts are tracked in MonsterPartDisplay objects tied to the HUD
- What you will NOT do (audio, settings menu — those are 10-H and 10-I)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 10-G: Combat Visual Polish — Animations, Part Damage States, Phase Transitions

**Resuming from:** Stage 10-F complete — all scene backgrounds generated and wired
**Done when:** Hunters animate while moving; monster parts visually degrade; overlord phase transitions have a screen-flash flourish; hunter death plays a full collapse animation
**Commit:** `"10G: Combat visual polish — walk animations, part damage states, phase transitions, death collapse"`
**Next session:** STAGE_10_H.md

---

## Part 1 — Hunter Walk Animation

The hunter animator controller has a `Walk` state defined in Stage 9-C but it may be an empty clip. This section generates the walk animation as a 4-frame sprite sheet and wires the animator parameter.

### 1a — Generate Walk Sprite Sheet for Each Build

For each hunter build, generate a 4-frame walk cycle sprite sheet (4 frames × 128×128 = 512×128 PNG).

**Generation prompt template for each build:**
> "Top-down view dark fantasy pixel art, 4-frame walk cycle sprite sheet, horizontal layout, [build description matching Stage 10-D], south-facing, dark background. Frame 1: left foot forward. Frame 2: neutral. Frame 3: right foot forward. Frame 4: neutral. Each frame 128×128, total image 512×128."

Save as: `Hunter_[Build]_Walk_S.png` in `Assets/_Game/Art/Hunters/Generated/`

Import as sprite sheet: **Texture Type = Sprite, Sprite Mode = Multiple**. Use Unity's Sprite Editor to slice into 4 × 128×128 sprites.

**For Aldric specifically**, check if a walk sheet already exists. If not, generate one now using the same template.

### 1b — Create Walk Animation Clips

For each build, create `Hunter_[Build]_Walk.anim` in `Assets/_Game/Art/Hunters/Animations/`:

```
Frames: Hunter_[Build]_Walk_S_0, _1, _2, _3
Sample Rate: 12 FPS
Loop: true
Wrap Mode: Loop
```

Use `mcp__coplay-mcp__create_animation_clip` and `mcp__coplay-mcp__set_sprite_animation_curve` to set the sprite curve programmatically, or use Unity's Animation window manually.

### 1c — Wire the Walk Animator Parameter

Confirm each build's AnimatorController has:
- Bool parameter: `isWalking`
- Transition: Idle → Walk when `isWalking == true` (no exit time, immediate)
- Transition: Walk → Idle when `isWalking == false` (no exit time, immediate)

In `HunterTokenController`:

```csharp
private Animator _animator;

public void StartMoving()
{
    _animator.SetBool("isWalking", true);
}

public void StopMoving()
{
    _animator.SetBool("isWalking", false);
}
```

Call `StartMoving()` when the movement coroutine begins and `StopMoving()` when it ends.

### 1d — Movement Coroutine Update

In `CombatAnimationController.MoveHunterToCell()`:

```csharp
public IEnumerator MoveHunterToCell(HunterCombatState hunter, Vector2Int targetCell)
{
    var token = _hunterTokens[hunter.hunterId];
    token.StartMoving();
    token.SetFacing(targetCell - new Vector2Int(hunter.gridX, hunter.gridY));

    Vector3 start  = token.transform.position;
    Vector3 target = GridCellToWorldPos(targetCell);
    float   t      = 0f;
    float   dur    = 0.30f;

    while (t < 1f)
    {
        t += Time.deltaTime / dur;
        token.transform.position = Vector3.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t));
        yield return null;
    }

    token.transform.position = target;
    token.StopMoving();
    token.SetFacing(Vector2Int.down); // Return to south-facing after move
}
```

---

## Part 2 — Monster Walk Cycle

Monsters don't have multi-frame sprite sheets — generating one per monster would be an entire art sprint. Instead, use a **wobble walk**: a simple tween that rocks the monster token slightly as it moves.

In `CombatAnimationController.MoveMonsterToCell()`:

```csharp
public IEnumerator MoveMonsterToCell(Vector2Int targetCell)
{
    Vector3 start  = _monsterToken.transform.position;
    Vector3 target = GridCellToWorldPos(targetCell);
    float   t      = 0f;
    float   dur    = 0.35f;

    while (t < 1f)
    {
        t += Time.deltaTime / dur;
        float smooth = Mathf.SmoothStep(0f, 1f, t);
        _monsterToken.transform.position = Vector3.Lerp(start, target, smooth);

        // Wobble: oscillate Z rotation as monster moves
        float wobble = Mathf.Sin(t * Mathf.PI * 4f) * 3f; // ±3 degrees
        _monsterToken.transform.localEulerAngles = new Vector3(0f, 0f, wobble);
        yield return null;
    }

    _monsterToken.transform.position            = target;
    _monsterToken.transform.localEulerAngles    = Vector3.zero;
}
```

This gives the impression of movement weight without requiring extra sprites.

---

## Part 3 — Monster Part Damage Visual States

Monster parts are currently tracked by HP numbers but the sprite doesn't change. Add a visual damage state: as a part takes damage, its HP bar colour shifts, and when broken, the token gets a crack overlay.

### 3a — HP Bar Colour Tiers

In `CombatHUDUpdater.RefreshPartBar(MonsterPart part)`:

```csharp
public void RefreshPartBar(MonsterPart part)
{
    var bar = GetPartBar(part.partName);
    if (bar == null) return;

    float pct = (float)part.currentFleshHP / part.maxFleshHP;

    // Update HP value labels
    bar.Q<Label>("part-hp-current").text = part.currentFleshHP.ToString();

    // Colour the bar based on health percentage
    var fill = bar.Q("part-hp-fill");
    if (fill == null) return;

    Color barColour;
    if (pct > 0.6f)
        barColour = new Color(0.60f, 0.15f, 0.15f);   // Healthy: dark red
    else if (pct > 0.3f)
        barColour = new Color(0.75f, 0.35f, 0.10f);   // Damaged: orange-red
    else if (pct > 0f)
        barColour = new Color(0.85f, 0.60f, 0.10f);   // Critical: amber
    else
        barColour = new Color(0.20f, 0.20f, 0.20f);   // Broken: grey

    fill.style.backgroundColor = barColour;

    // Label colour darkens when broken
    var label = bar.Q<Label>("part-name-label");
    if (label != null)
        label.style.color = pct <= 0f
            ? new Color(0.40f, 0.40f, 0.40f)
            : new Color(0.83f, 0.80f, 0.73f);
}
```

### 3b — Broken Part Crack Overlay on Monster Token

When a monster part breaks, add a crack-texture overlay on the monster sprite renderer. Generate a single reusable crack overlay sprite:

**File:** `FX_PartBroken_Crack.png` in `Assets/_Game/Art/FX/`

**Generation prompt:**
> "Transparent PNG, 128×128. Abstract crack pattern — several jagged lines radiating from near-center, dark charcoal-grey on transparent background. Suitable for layering over a top-down sprite to indicate damage. No fill, just crack lines."

Add a second SpriteRenderer as a child of the monster token (`_crackOverlay`) with sorting order just above the monster sprite:

```csharp
// In MonsterTokenController:
[SerializeField] private SpriteRenderer _crackOverlay;
[SerializeField] private Sprite         _crackSprite;

public void ShowPartBroken(string partName, int brokenCount)
{
    if (_crackOverlay == null || _crackSprite == null) return;
    _crackOverlay.sprite = _crackSprite;
    // Increase opacity with each broken part
    float alpha = Mathf.Min(0.2f + brokenCount * 0.15f, 0.7f);
    var c = _crackOverlay.color;
    _crackOverlay.color = new Color(c.r, c.g, c.b, alpha);
}
```

Call `ShowPartBroken()` from `CombatAnimationController` when a part breaks, passing the running count of broken parts.

### 3c — Flash-Red on Part Hit

In `CombatAnimationController.PlayPartHitFlash(string partName)`:

```csharp
public IEnumerator PlayPartHitFlash(string partName)
{
    var bar = _hud.GetPartBar(partName);
    if (bar == null) yield break;

    var fill = bar.Q("part-hp-fill");
    if (fill == null) yield break;

    var origColour = fill.style.backgroundColor;
    fill.style.backgroundColor = new Color(1f, 0.2f, 0.2f); // Bright red flash
    yield return new WaitForSeconds(0.08f);
    fill.style.backgroundColor = origColour;
}
```

---

## Part 4 — Overlord Phase 2 Transition Flourish

The current phase 2 transition shows a banner. Add a full-screen flash effect and a brief camera shake to make it feel significant.

In `CombatAnimationController.PlayPhase2Transition(string bannerText)`:

```csharp
public IEnumerator PlayPhase2Transition(string bannerText)
{
    // Step 1: Full-screen white flash
    yield return StartCoroutine(FlashScreenWhite(0.3f));

    // Step 2: Shake the camera (or the combat root transform)
    yield return StartCoroutine(ShakeCombatRoot(0.25f, 0.12f));

    // Step 3: Show the phase banner (existing logic from Stage 9)
    _hud.ShowPhaseBanner(bannerText);
    yield return new WaitForSeconds(1.8f);
    _hud.HidePhaseBanner();
}

private IEnumerator FlashScreenWhite(float duration)
{
    var flash = _uiDocument.rootVisualElement.Q("screen-flash");
    if (flash == null) yield break;

    flash.style.display         = DisplayStyle.Flex;
    flash.style.backgroundColor = Color.white;

    float t = 0f;
    while (t < 1f)
    {
        t += Time.deltaTime / duration;
        float alpha = Mathf.Lerp(0.8f, 0f, t);
        flash.style.backgroundColor = new Color(1f, 1f, 1f, alpha);
        yield return null;
    }

    flash.style.display = DisplayStyle.None;
}

private IEnumerator ShakeCombatRoot(float duration, float magnitude)
{
    var combatRoot = GameObject.Find("CombatRoot")?.transform;
    if (combatRoot == null) yield break;

    Vector3 origin = combatRoot.localPosition;
    float   t      = 0f;

    while (t < 1f)
    {
        t += Time.deltaTime / duration;
        combatRoot.localPosition = origin + (Vector3)Random.insideUnitCircle * magnitude * (1f - t);
        yield return null;
    }

    combatRoot.localPosition = origin;
}
```

Add a `screen-flash` VisualElement to the CombatScene UXML: full-size, `position: absolute`, `display: none` by default.

---

## Part 5 — Hunter Death Collapse Animation

The current death handling applies a dark tint. Replace with a proper collapse: the hunter slides down and fades out.

In `CombatAnimationController.PlayHunterCollapse(HunterCombatState hunter)`:

```csharp
public IEnumerator PlayHunterCollapse(HunterCombatState hunter)
{
    var token = _hunterTokens[hunter.hunterId];
    var sr    = token.GetComponent<SpriteRenderer>();

    // Trigger HitReact then transition to collapse
    token.GetComponent<Animator>()?.SetTrigger("hitReact");
    yield return new WaitForSeconds(0.25f);

    Vector3 startPos   = token.transform.position;
    Vector3 targetPos  = startPos + new Vector3(0f, -0.15f, 0f);
    Color   startColor = sr.color;
    Color   targetColor = new Color(0.2f, 0.1f, 0.1f, 0.4f); // Dark tinted, semi-transparent

    float t   = 0f;
    float dur = 0.6f;

    while (t < 1f)
    {
        t += Time.deltaTime / dur;
        float smooth = Mathf.SmoothStep(0f, 1f, t);
        token.transform.position = Vector3.Lerp(startPos, targetPos, smooth);
        sr.color                 = Color.Lerp(startColor, targetColor, smooth);
        yield return null;
    }

    // Final state: collapsed position, dark tint
    token.transform.position = targetPos;
    sr.color                  = targetColor;

    Debug.Log($"[Anim] {hunter.hunterName} collapse animation complete.");
}
```

The collapsed hunter remains on the grid, visually downed. Other hunters can see their position.

---

## Part 6 — Hunt Victory Visual

When all monster HP reaches 0 (or all nodes broken for overlords), before the screen transitions, show a brief victory flourish:

```csharp
public IEnumerator PlayHuntVictory()
{
    // Screen flash — warm amber this time
    var flash = _uiDocument.rootVisualElement.Q("screen-flash");
    if (flash != null)
    {
        flash.style.display         = DisplayStyle.Flex;
        flash.style.backgroundColor = new Color(0.9f, 0.6f, 0.1f, 0.6f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.6f;
            float alpha = Mathf.Lerp(0.6f, 0f, t);
            flash.style.backgroundColor = new Color(0.9f, 0.6f, 0.1f, alpha);
            yield return null;
        }
        flash.style.display = DisplayStyle.None;
    }

    // Hold 1 second, then trigger scene transition
    yield return new WaitForSeconds(1.0f);
    GameStateManager.Instance.ResolveCombatVictory();
}
```

---

## Verification Checklist

- [ ] Hunter moves 2 cells → walk animation plays for duration of movement; returns to idle after
- [ ] Hunter faces east when moving right, west (mirrored) when moving left
- [ ] Monster moves → wobble rotation oscillates during lerp; stops at destination
- [ ] Thornback takes damage on its Left Flank → bar turns orange-red; at critical → amber; at 0 → grey
- [ ] Thornback Left Flank breaks → crack overlay appears on monster token, partially transparent
- [ ] Second part breaks → crack overlay becomes more opaque
- [ ] Phase 2 transition: white screen flash → camera shake → phase banner appears → fades
- [ ] Hunter drops to 0 → hitReact trigger fires → slides down + dark tint over 0.6s
- [ ] Pale Stag Ascendant phase 2: monster token swaps to Phase 2 sprite AND white flash plays
- [ ] Hunt victory: warm amber screen flash → 1 second pause → post-combat resolution

---

## Next Session

**File:** `_Docs/Stage_10/STAGE_10_H.md`
**Covers:** Audio completion — generating monster-specific SFX (roars, movement), additional UI sounds (hover, error, notification), ambient settlement audio layers, and wiring all new audio into the AudioManager
