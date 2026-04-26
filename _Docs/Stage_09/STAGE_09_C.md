<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 9-C | Character Animator Controllers
Status: Stage 9-B complete. 4-way directional sprites done.
Task: Build Unity Animator state machines for all 8 hunter builds.
Each build needs 4 animation states: Idle (looping), Walk (looping),
Attack (one-shot), HitReact (one-shot). Pixel art sprites are
frame-by-frame. Generate the animation frames for each state
for each build, create AnimationClip assets, wire them into
AnimatorController assets, and trigger animations from code.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_09/STAGE_09_C.md
- Assets/_Game/Scripts/Core.UI/HunterFacingController.cs

Then confirm:
- Animator is used for frame-based sprite animation (NOT DOTween
  or manual frame switching)
- Each build gets its own AnimatorController asset
- Triggers used: "Attack", "HitReact" — boolean: "IsWalking"
- Idle and Walk states loop; Attack and HitReact return to Idle
- All animation frames are generated as sprite sheets (not
  individual PNGs per frame)
- What you will NOT build (death animation, unique overlord
  animation — post-MVP)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 9-C: Character Animator Controllers

**Resuming from:** Stage 9-B complete — directional sprites and facing logic working
**Done when:** All 8 hunter builds have Idle/Walk/Attack/HitReact animations playing correctly in combat; triggers fire from code without Inspector changes
**Commit:** `"9C: Character animator controllers — idle, walk, attack, hit-react for all 8 builds"`
**Next session:** STAGE_09_D.md

---

## New Developer Note: How Unity Animation Works

In Unity, animation is built in three layers:

1. **Sprite Sheet** — a single image file with all animation frames arranged in a grid (e.g., 4 frames of an idle bob in a 4×1 strip)
2. **AnimationClip** — an asset that tells Unity which frames to show and when
3. **AnimatorController** — a state machine that decides which AnimationClip to play based on triggers from your code

Your code doesn't say "play frame 3". It says "trigger Attack" and the Animator handles the rest.

---

## Part 1: Generate Sprite Sheets

**New developer note:** A sprite sheet is one wide image containing all animation frames left-to-right. For a 4-frame idle animation at 32×48 pixels per frame, the sheet is 128×48 total.

Use CoPlay `generate_or_edit_images` for each build × animation state.

**Save path:** `Assets/_Game/Art/Generated/Hunters/{BuildName}/Anim/`

### Animation Frame Counts

| State | Frames | Loop |
|---|---|---|
| Idle | 4 | Yes — gentle breathing bob |
| Walk | 6 | Yes — step cycle |
| Attack | 4 | No — one-shot, snap back |
| HitReact | 3 | No — flinch and recover |

### Sprite Sheet Dimensions (32px × 48px per frame)

| State | Total Width | Height |
|---|---|---|
| Idle (4 frames) | 128px | 48px |
| Walk (6 frames) | 192px | 48px |
| Attack (4 frames) | 128px | 48px |
| HitReact (3 frames) | 96px | 48px |

### Prompt Template for Each Sheet

```
Pixel art sprite sheet. {ANIMATION_DESCRIPTION}.
{BUILD_DESCRIPTION}. South-facing (toward camera).
{FRAME_COUNT} frames arranged left-to-right in a single row.
Each frame is exactly 32×48 pixels. No gaps between frames.
Transparent background. Point art, no anti-aliasing.
Dark palette: ash grey, bone white, dried blood weapon.
Consistent silhouette and proportions across all frames.
```

### Per-Build Animation Descriptions

**Aethel (lean male with hood and short blade):**
- Idle: "Gentle breathing — shoulders rise and fall, cloak shifts slightly. 4 frames."
- Walk: "Steady stride — arms at sides, blade at hip, hood stable. 6 frames."
- Attack: "Quick horizontal slash — blade sweeps right, step forward, recoil back. 4 frames."
- HitReact: "Flinch backward — head turns, body rocks back, straightens. 3 frames."

**Beorn (stocky male with fur coat and axe):**
- Idle: "Heavy idle — weight shifts foot to foot, axe rests on shoulder. 4 frames."
- Walk: "Lumbering stride — axe swings slightly, fur collar bounces. 6 frames."
- Attack: "Overhead axe chop — raise, bring down, embed and pull back. 4 frames."
- HitReact: "Staggers — weight shifts hard left, recovers slowly. 3 frames."

**Cyne (tall male with cloth wraps and dual blades):**
- Idle: "Still, watchful — cloth wraps drift, hands near hilts. 4 frames."
- Walk: "Light, precise steps — dual blades steady at hips. 6 frames."
- Attack: "Cross-draw slash — right blade sweeps up, left sweeps across. 4 frames."
- HitReact: "Sharp backward step — arms up defensively, drops back into stance. 3 frames."

**Duna (broad male with layered armour and greatspear):**
- Idle: "Planted idle — spear grounded, weight on back foot. 4 frames."
- Walk: "Heavy armoured march — spear carried vertically. 6 frames."
- Attack: "Spear thrust — lunge forward, arm extended, pull back. 4 frames."
- HitReact: "Hit absorbed — armour takes it, rocks back, stabilises. 3 frames."

**Eira (slight female with longbow and hunter's cloak):**
- Idle: "Alert rest — cloak drifts, hand near bow. 4 frames."
- Walk: "Quiet tread — cloak billows, bow bounces at back. 6 frames."
- Attack: "Draw and loose — draw bow, aim, release, lower. 4 frames."
- HitReact: "Dodge-stumble — twists away, recovers into crouch. 3 frames."

**Freya (medium female with twin daggers and braids):**
- Idle: "Restless idle — weight rocks, braids swing slightly. 4 frames."
- Walk: "Quick light walk — daggers at belt, braids behind. 6 frames."
- Attack: "Dual stab — both daggers forward, twist, pull back. 4 frames."
- HitReact: "Spins with the hit — rotates half turn, comes back ready. 3 frames."

**Gerd (heavy female with shield and shortsword):**
- Idle: "Shield-raised idle — sword at side, shield arm up. 4 frames."
- Walk: "Steady march — shield bounces, sword at hip. 6 frames."
- Attack: "Shield-bash then stab — push forward with shield, follow with sword. 4 frames."
- HitReact: "Shield absorbs — shield arm pulls back from impact, resets. 3 frames."

**Hild (wiry female with longspear and leather wraps):**
- Idle: "Coiled stillness — spear angled forward, weight forward. 4 frames."
- Walk: "Flowing stride — spear carried at angle. 6 frames."
- Attack: "Spinning spear strike — rotate spear around body, thrust. 4 frames."
- HitReact: "Sways back — spear tip drops, regains stance. 3 frames."

---

## Part 2: Import Settings

For each sprite sheet in Unity:

1. Select the PNG in the Project window
2. Inspector settings:
   - **Texture Type:** Sprite (2D and UI)
   - **Sprite Mode:** Multiple
   - **Filter Mode:** Point (No Filter)
   - **Pixels Per Unit:** 16
   - **Compression:** None
3. Click **Sprite Editor**
4. Use **Slice → Grid by Cell Size → 32 × 48**
5. Click Slice, then Apply
6. Name the resulting sprites: `{Build}_{State}_0`, `{Build}_{State}_1`, etc.

---

## Part 3: AnimationClip Creation via Script

**New developer note:** Instead of clicking through Unity's UI to create 32 AnimationClips (8 builds × 4 states), use a CoPlay script to do it all at once.

Create an Editor script at `Assets/_Game/Editor/AnimationClipBuilder.cs`:

```csharp
using UnityEngine;
using UnityEditor;
using System.IO;

namespace MnM.Editor
{
    public static class AnimationClipBuilder
    {
        private static readonly string[] Builds =
            { "Aethel", "Beorn", "Cyne", "Duna", "Eira", "Freya", "Gerd", "Hild" };

        // (state, frameCount, fps, loop)
        private static readonly (string, int, float, bool)[] States =
        {
            ("Idle",      4, 6f,  true),
            ("Walk",      6, 10f, true),
            ("Attack",    4, 12f, false),
            ("HitReact",  3, 12f, false),
        };

        [MenuItem("MnM/Build Animation Clips")]
        public static void BuildAll()
        {
            foreach (var build in Builds)
                foreach (var (state, frameCount, fps, loop) in States)
                    BuildClip(build, state, frameCount, fps, loop);

            AssetDatabase.Refresh();
            Debug.Log("[AnimBuilder] All clips built.");
        }

        private static void BuildClip(string build, string state,
                                       int frameCount, float fps, bool loop)
        {
            string sheetPath = $"Assets/_Game/Art/Generated/Hunters/{build}/Anim/" +
                               $"hunter_{build.ToLower()}_{state.ToLower()}.png";
            var sprites = AssetDatabase.LoadAllAssetsAtPath(sheetPath);

            if (sprites.Length == 0)
            {
                Debug.LogWarning($"[AnimBuilder] No sprites found at {sheetPath}");
                return;
            }

            var clip       = new AnimationClip();
            clip.frameRate = fps;

            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            // Build sprite keyframes
            var spriteKeyframes = new ObjectReferenceKeyframe[frameCount];
            float frameDuration = 1f / fps;

            int spriteIndex = 0;
            foreach (var obj in sprites)
            {
                if (obj is Sprite s && spriteIndex < frameCount)
                {
                    spriteKeyframes[spriteIndex] = new ObjectReferenceKeyframe
                    {
                        time  = spriteIndex * frameDuration,
                        value = s
                    };
                    spriteIndex++;
                }
            }

            var binding = new EditorCurveBinding
            {
                type         = typeof(SpriteRenderer),
                path         = "",
                propertyName = "m_Sprite"
            };
            AnimationUtility.SetObjectReferenceCurve(clip, binding, spriteKeyframes);

            string outDir  = $"Assets/_Game/Animations/Hunters/{build}";
            string outPath = $"{outDir}/{build}_{state}.anim";
            Directory.CreateDirectory(outDir);
            AssetDatabase.CreateAsset(clip, outPath);
            Debug.Log($"[AnimBuilder] Created: {outPath}");
        }
    }
}
```

**To run:** In Unity, click the top menu bar → **MnM → Build Animation Clips**. This creates all 32 `.anim` files automatically.

---

## Part 4: AnimatorController Creation

After clips are built, create one AnimatorController per build via the same editor approach.

Create `Assets/_Game/Editor/AnimatorControllerBuilder.cs`:

```csharp
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

namespace MnM.Editor
{
    public static class AnimatorControllerBuilder
    {
        private static readonly string[] Builds =
            { "Aethel", "Beorn", "Cyne", "Duna", "Eira", "Freya", "Gerd", "Hild" };

        [MenuItem("MnM/Build Animator Controllers")]
        public static void BuildAll()
        {
            foreach (var build in Builds)
                BuildController(build);
            AssetDatabase.Refresh();
            Debug.Log("[AnimCtrlBuilder] All controllers built.");
        }

        private static void BuildController(string build)
        {
            string outDir  = "Assets/_Game/Animations/Controllers";
            string outPath = $"{outDir}/{build}_AnimController.controller";
            Directory.CreateDirectory(outDir);

            var controller = AnimatorController.CreateAnimatorControllerAtPath(outPath);

            // Parameters
            controller.AddParameter("IsWalking", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Attack",    AnimatorControllerParameterType.Trigger);
            controller.AddParameter("HitReact",  AnimatorControllerParameterType.Trigger);

            var layer     = controller.layers[0];
            var stateMachine = layer.stateMachine;

            // Load clips
            AnimationClip idle     = LoadClip(build, "Idle");
            AnimationClip walk     = LoadClip(build, "Walk");
            AnimationClip attack   = LoadClip(build, "Attack");
            AnimationClip hitReact = LoadClip(build, "HitReact");

            // Create states
            var idleState  = stateMachine.AddState("Idle");
            idleState.motion = idle;

            var walkState  = stateMachine.AddState("Walk");
            walkState.motion = walk;

            var attackState = stateMachine.AddState("Attack");
            attackState.motion = attack;

            var hitState   = stateMachine.AddState("HitReact");
            hitState.motion = hitReact;

            stateMachine.defaultState = idleState;

            // Idle ↔ Walk
            var idleToWalk = idleState.AddTransition(walkState);
            idleToWalk.AddCondition(AnimatorConditionMode.If, 0, "IsWalking");
            idleToWalk.hasExitTime = false;

            var walkToIdle = walkState.AddTransition(idleState);
            walkToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsWalking");
            walkToIdle.hasExitTime = false;

            // Any → Attack
            var anyToAttack = stateMachine.AddAnyStateTransition(attackState);
            anyToAttack.AddCondition(AnimatorConditionMode.If, 0, "Attack");
            anyToAttack.hasExitTime = false;
            anyToAttack.duration    = 0.05f;

            // Attack → Idle
            var attackToIdle = attackState.AddTransition(idleState);
            attackToIdle.hasExitTime  = true;
            attackToIdle.exitTime     = 1f;
            attackToIdle.duration     = 0.05f;

            // Any → HitReact
            var anyToHit = stateMachine.AddAnyStateTransition(hitState);
            anyToHit.AddCondition(AnimatorConditionMode.If, 0, "HitReact");
            anyToHit.hasExitTime = false;
            anyToHit.duration    = 0.05f;

            // HitReact → Idle
            var hitToIdle = hitState.AddTransition(idleState);
            hitToIdle.hasExitTime = true;
            hitToIdle.exitTime    = 1f;
            hitToIdle.duration    = 0.05f;

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            Debug.Log($"[AnimCtrlBuilder] Built: {outPath}");
        }

        private static AnimationClip LoadClip(string build, string state)
        {
            return AssetDatabase.LoadAssetAtPath<AnimationClip>(
                $"Assets/_Game/Animations/Hunters/{build}/{build}_{state}.anim");
        }
    }
}
```

**To run:** In Unity → **MnM → Build Animator Controllers**. This creates 8 `.controller` files.

---

## Part 5: Wiring AnimatorControllers to Tokens

In `CombatScreenController`, add:

```csharp
[SerializeField] private RuntimeAnimatorController[] _hunterAnimControllers;
// Assign all 8 controllers in Inspector, in the same order as build names

private readonly string[] _buildOrder =
    { "Aethel", "Beorn", "Cyne", "Duna", "Eira", "Freya", "Gerd", "Hild" };
```

When spawning a hunter token, add an Animator component:

```csharp
private GameObject SpawnHunterToken(HunterState hunter, Vector2Int gridPos)
{
    var tokenGO   = new GameObject($"Token_{hunter.hunterName}");
    var renderer  = tokenGO.AddComponent<SpriteRenderer>();
    var facing    = tokenGO.AddComponent<HunterFacingController>();
    var animator  = tokenGO.AddComponent<Animator>();

    // Assign controller
    for (int i = 0; i < _buildOrder.Length; i++)
    {
        if (_buildOrder[i] == hunter.buildName && i < _hunterAnimControllers.Length)
        {
            animator.runtimeAnimatorController = _hunterAnimControllers[i];
            break;
        }
    }

    // ... rest of spawn logic
    return tokenGO;
}
```

---

## Part 6: Triggering Animations from Code

Create a thin wrapper `HunterAnimationTrigger.cs`:

**Path:** `Assets/_Game/Scripts/Core.UI/HunterAnimationTrigger.cs`

```csharp
using UnityEngine;

namespace MnM.Core.UI
{
    [RequireComponent(typeof(Animator))]
    public class HunterAnimationTrigger : MonoBehaviour
    {
        private Animator _animator;

        private static readonly int IsWalking = Animator.StringToHash("IsWalking");
        private static readonly int Attack    = Animator.StringToHash("Attack");
        private static readonly int HitReact  = Animator.StringToHash("HitReact");

        private void Awake() => _animator = GetComponent<Animator>();

        public void SetWalking(bool walking)
            => _animator.SetBool(IsWalking, walking);

        public void TriggerAttack()
            => _animator.SetTrigger(Attack);

        public void TriggerHitReact()
            => _animator.SetTrigger(HitReact);
    }
}
```

Add `HunterAnimationTrigger` to the token spawn in `CombatScreenController` alongside `HunterFacingController`.

Call from combat resolution:

```csharp
// When a hunter attacks:
hunterAnimTrigger.TriggerAttack();

// When a hunter takes a hit:
hunterAnimTrigger.TriggerHitReact();

// When a hunter starts/stops moving:
hunterAnimTrigger.SetWalking(true);
// ... after move completes:
hunterAnimTrigger.SetWalking(false);
```

---

## Verification Test

- [ ] Run **MnM → Build Animation Clips** → 32 `.anim` files appear in `Assets/_Game/Animations/Hunters/`
- [ ] Run **MnM → Build Animator Controllers** → 8 `.controller` files appear in `Assets/_Game/Animations/Controllers/`
- [ ] Place Aethel token in combat → Idle animation plays (frames cycling)
- [ ] Trigger `SetWalking(true)` → Walk animation plays
- [ ] Trigger `SetWalking(false)` → returns to Idle
- [ ] Trigger `TriggerAttack()` → Attack animation plays once, returns to Idle
- [ ] Trigger `TriggerHitReact()` → HitReact animation plays once, returns to Idle
- [ ] Both Attack and HitReact can interrupt each other (AnyState transitions)
- [ ] Test with Beorn and Eira to confirm all builds work, not just Aethel
- [ ] Facing direction does not reset when an animation plays
- [ ] No "Animator is not playing an AnimatorController" warnings in Console

---

## Next Session

**File:** `_Docs/Stage_09/STAGE_09_D.md`
**Covers:** Gear Overlay Sprite System — generating gear overlay sprites for each equipment slot (head, chest, arms, weapon, off-hand) that draw on top of the base hunter sprite, and wiring the overlay system so equipped gear is visually reflected on the combat token
