<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 9-G | Ivory Stampede Pack Monster
Status: Stage 9-F complete. Thornback behavior deck done.
Task: Build the PackMonsterSO class and implement the Ivory Stampede
— a herd of 3 ivory-tusked beasts that share one behavior deck
but activate as separate tokens. One token is the Alpha (has the
aggro targeting behaviour); the other two are Flankers (move
toward the closest non-aggro hunter). The pack behaviour must
work in the existing combat grid system.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_09/STAGE_09_G.md
- Assets/_Game/Scripts/Core.Data/MonsterSO.cs
- Assets/_Game/Scripts/Core.Systems/MonsterAI.cs
- Assets/_Game/Scripts/Core.Systems/CombatManager.cs

Then confirm:
- PackMonsterSO extends MonsterSO with tokenCount and per-token
  health tracking
- All pack tokens share one behavior deck draw per round
- Alpha and Flanker tokens have different movement rules
- If the Alpha is killed, the eldest Flanker becomes the new Alpha
- What you will NOT build (pack monster variant artwork — that
  is in the art sessions; only the data and AI logic here)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 9-G: Ivory Stampede Pack Monster

**Resuming from:** Stage 9-F complete — Thornback behavior deck done
**Done when:** PackMonsterSO class compiles; Ivory Stampede has 3 tokens on the grid; pack AI resolves correctly; killing the Alpha promotes a Flanker
**Commit:** `"9G: PackMonsterSO and Ivory Stampede — pack token system, alpha/flanker AI"`
**Next session:** STAGE_09_H.md

---

## The Ivory Stampede — Monster Design

**Name:** The Ivory Stampede
**Type:** Pack Beast
**Tier:** Standard (hunted Years 3–7)
**Difficulty:** Veteran recommended

**Lore:** Not one creature but three — a bonded trio of white-furred, ivory-tusked quadrupeds that have hunted together long enough to move as a single organism. Settlers debate whether they communicate or simply react. The outcome is the same: engage one and the others will have already moved.

**Combat Identity:**
- Three tokens on the grid simultaneously
- One is the Alpha (targeting the highest-Grit hunter), two are Flankers (targeting the closest non-Alpha-target)
- Cards draw once per round for the whole pack; Alpha resolves attack, Flankers resolve movement only
- When a Flanker reaches its target, it attacks once for 1 Flesh damage (automatic, not from the card)
- Pack is most dangerous when it has space to approach from multiple angles

---

## Part 1: PackMonsterSO.cs

**Path:** `Assets/_Game/Scripts/Core.Data/PackMonsterSO.cs`

```csharp
using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(fileName = "Pack_", menuName = "MnM/Pack Monster")]
    public class PackMonsterSO : MonsterSO
    {
        [Header("Pack Settings")]
        public int tokenCount = 3;

        [Header("Alpha Parts")]
        public MonsterPart[] alphaParts;       // The Alpha has unique parts/HP

        [Header("Flanker Parts")]
        public MonsterPart[] flankerParts;     // All Flankers share this part template

        [Header("Flanker Auto-Attack")]
        public int flankerAdjacentDamage = 1;  // Damage per flanker when adjacent
    }

    [System.Serializable]
    public class MonsterPart
    {
        public string partName;
        public int    shellHP;
        public int    fleshHP;
    }
}
```

---

## Part 2: PackTokenState.cs

Add to `CampaignState` or as a standalone runtime class:

```csharp
namespace MnM.Core.Data
{
    [System.Serializable]
    public class PackTokenState
    {
        public string tokenId;          // "Alpha" | "Flanker_1" | "Flanker_2"
        public bool   isAlpha;
        public bool   isDead;

        // Part health — uses MonsterPart definitions from PackMonsterSO
        public int[]  partShellHP;
        public int[]  partFleshHP;

        // Grid position
        public int    gridX;
        public int    gridY;
    }
}
```

---

## Part 3: PackMonsterAI.cs

**Path:** `Assets/_Game/Scripts/Core.Systems/PackMonsterAI.cs`

```csharp
using System.Collections.Generic;
using UnityEngine;
using MnM.Core.Data;
using MnM.Core.UI;

namespace MnM.Core.Systems
{
    public class PackMonsterAI : MonoBehaviour
    {
        [SerializeField] private PackMonsterSO _packData;

        private List<PackTokenState> _tokens = new();
        private BehaviorCardSO[]     _deckShuffled;
        private int                  _deckIndex;

        // References to token GameObjects (spawned by CombatScreenController)
        private Dictionary<string, GameObject> _tokenObjects = new();

        public void InitializePack(Vector2Int[] startPositions)
        {
            _tokens.Clear();

            for (int i = 0; i < _packData.tokenCount; i++)
            {
                bool isAlpha = i == 0;
                var token = new PackTokenState
                {
                    tokenId   = isAlpha ? "Alpha" : $"Flanker_{i}",
                    isAlpha   = isAlpha,
                    isDead    = false,
                    gridX     = startPositions[i].x,
                    gridY     = startPositions[i].y,
                    partShellHP = InitPartHP(isAlpha, shell: true),
                    partFleshHP = InitPartHP(isAlpha, shell: false),
                };
                _tokens.Add(token);
            }

            ShuffleDeck();
            Debug.Log($"[PackAI] {_packData.monsterName} pack initialized. {_tokens.Count} tokens.");
        }

        private int[] InitPartHP(bool isAlpha, bool shell)
        {
            var parts = isAlpha ? _packData.alphaParts : _packData.flankerParts;
            if (parts == null) return new int[0];
            var hp = new int[parts.Length];
            for (int i = 0; i < parts.Length; i++)
                hp[i] = shell ? parts[i].shellHP : parts[i].fleshHP;
            return hp;
        }

        /// <summary>Called each monster phase — resolves one behavior card for the whole pack.</summary>
        public void ResolvePack(List<HunterState> activeHunters,
                                 Dictionary<string, Vector2Int> hunterPositions)
        {
            if (_tokens.Count == 0) return;

            // Draw next card
            var card = DrawCard();
            Debug.Log($"[PackAI] Card: {card.cardName}");

            // Identify Alpha and Flankers
            PackTokenState alpha   = GetAlpha();
            var flankers           = GetFlankers();

            // Resolve Alpha: full card behavior
            if (alpha != null && !alpha.isDead)
            {
                HunterState alphaTarget = GetAlphaTarget(activeHunters);
                ResolveAlphaCard(card, alpha, alphaTarget, hunterPositions);
            }

            // Resolve Flankers: movement only (each toward closest non-alpha-target)
            foreach (var flanker in flankers)
            {
                if (flanker.isDead) continue;
                HunterState flankerTarget = GetFlankerTarget(activeHunters,
                    alpha != null ? GetAlphaTarget(activeHunters) : null);
                ResolveFlankerMovement(card, flanker, flankerTarget, hunterPositions);
                CheckFlankerAdjacentAttack(flanker, flankerTarget, hunterPositions);
            }
        }

        // ── Alpha Logic ─────────────────────────────────────────────────

        private void ResolveAlphaCard(BehaviorCardSO card, PackTokenState alpha,
                                       HunterState target,
                                       Dictionary<string, Vector2Int> hunterPos)
        {
            Debug.Log($"[PackAI] Alpha resolving: {card.cardName} → target: {target?.hunterName}");

            // Movement from card
            if (!string.IsNullOrEmpty(card.movementEffect))
                MoveTokenToward(alpha, target, hunterPos, steps: 1);

            // Attack from card
            if (target != null && !string.IsNullOrEmpty(card.attackEffect))
            {
                Debug.Log($"[PackAI] Alpha attacks {target.hunterName}: {card.attackEffect}");
                // CombatManager handles actual damage application
                CombatManager.Instance?.ApplyMonsterAttack(target.hunterId, card.attackEffect);
            }
        }

        // ── Flanker Logic ────────────────────────────────────────────────

        private void ResolveFlankerMovement(BehaviorCardSO card, PackTokenState flanker,
                                             HunterState target,
                                             Dictionary<string, Vector2Int> hunterPos)
        {
            if (target == null) return;
            MoveTokenToward(flanker, target, hunterPos, steps: 2);
            Debug.Log($"[PackAI] {flanker.tokenId} moves toward {target.hunterName}");
        }

        private void CheckFlankerAdjacentAttack(PackTokenState flanker,
                                                 HunterState target,
                                                 Dictionary<string, Vector2Int> hunterPos)
        {
            if (target == null || !hunterPos.ContainsKey(target.hunterId)) return;

            var flankerPos = new Vector2Int(flanker.gridX, flanker.gridY);
            var targetPos  = hunterPos[target.hunterId];

            if (IsAdjacent(flankerPos, targetPos))
            {
                Debug.Log($"[PackAI] {flanker.tokenId} adjacent — auto-attack {target.hunterName}" +
                          $" for {_packData.flankerAdjacentDamage} Flesh damage");
                CombatManager.Instance?.ApplyDirectDamage(target.hunterId,
                    _packData.flankerAdjacentDamage);
            }
        }

        // ── Targeting ────────────────────────────────────────────────────

        private HunterState GetAlphaTarget(List<HunterState> hunters)
        {
            // Alpha targets highest Grit
            HunterState best = null;
            foreach (var h in hunters)
            {
                if (h.isDead) continue;
                if (best == null || h.grit > best.grit) best = h;
            }
            return best;
        }

        private HunterState GetFlankerTarget(List<HunterState> hunters,
                                              HunterState alphaTarget)
        {
            // Flankers target any non-dead hunter that isn't the alpha's target
            foreach (var h in hunters)
            {
                if (h.isDead) continue;
                if (alphaTarget != null && h.hunterId == alphaTarget.hunterId) continue;
                return h;
            }
            return alphaTarget; // Fall back to alpha target if everyone else is dead
        }

        // ── Alpha Promotion ──────────────────────────────────────────────

        public void OnTokenKilled(string tokenId)
        {
            foreach (var t in _tokens)
            {
                if (t.tokenId != tokenId) continue;
                t.isDead = true;
                Debug.Log($"[PackAI] {tokenId} killed.");
                break;
            }

            // If the Alpha was killed, promote first living Flanker
            if (GetAlpha() == null || GetAlpha().isDead)
            {
                foreach (var t in _tokens)
                {
                    if (!t.isDead && !t.isAlpha)
                    {
                        t.isAlpha = true;
                        Debug.Log($"[PackAI] {t.tokenId} promoted to Alpha.");
                        break;
                    }
                }
            }

            // Check if all tokens dead
            bool allDead = true;
            foreach (var t in _tokens) if (!t.isDead) { allDead = false; break; }
            if (allDead)
            {
                Debug.Log($"[PackAI] All {_packData.monsterName} tokens dead — hunt victory.");
                CombatManager.Instance?.OnMonsterDefeated();
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────

        private PackTokenState GetAlpha()
        {
            foreach (var t in _tokens)
                if (t.isAlpha && !t.isDead) return t;
            return null;
        }

        private List<PackTokenState> GetFlankers()
        {
            var result = new List<PackTokenState>();
            foreach (var t in _tokens)
                if (!t.isAlpha && !t.isDead) result.Add(t);
            return result;
        }

        private void MoveTokenToward(PackTokenState token, HunterState target,
                                      Dictionary<string, Vector2Int> hunterPos, int steps)
        {
            if (target == null || !hunterPos.ContainsKey(target.hunterId)) return;
            var targetPos = hunterPos[target.hunterId];
            var current   = new Vector2Int(token.gridX, token.gridY);

            for (int i = 0; i < steps; i++)
            {
                Vector2Int delta = targetPos - current;
                if (delta == Vector2Int.zero) break;

                Vector2Int step = Mathf.Abs(delta.x) >= Mathf.Abs(delta.y)
                    ? new Vector2Int((int)Mathf.Sign(delta.x), 0)
                    : new Vector2Int(0, (int)Mathf.Sign(delta.y));

                current += step;
            }

            token.gridX = current.x;
            token.gridY = current.y;

            // Animate token movement
            if (_tokenObjects.TryGetValue(token.tokenId, out var tokenGO))
                CombatManager.Instance?.AnimateTokenMove(tokenGO,
                    new Vector2Int(token.gridX, token.gridY));
        }

        private bool IsAdjacent(Vector2Int a, Vector2Int b)
        {
            int dx = Mathf.Abs(a.x - b.x);
            int dy = Mathf.Abs(a.y - b.y);
            return (dx + dy) == 1;
        }

        // ── Deck ─────────────────────────────────────────────────────────

        private void ShuffleDeck()
        {
            _deckShuffled = new BehaviorCardSO[_packData.behaviorDeck.Length];
            _packData.behaviorDeck.CopyTo(_deckShuffled, 0);

            // Fisher-Yates shuffle
            for (int i = _deckShuffled.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (_deckShuffled[i], _deckShuffled[j]) = (_deckShuffled[j], _deckShuffled[i]);
            }
            _deckIndex = 0;
        }

        private BehaviorCardSO DrawCard()
        {
            if (_deckShuffled == null || _deckIndex >= _deckShuffled.Length)
                ShuffleDeck();
            return _deckShuffled[_deckIndex++];
        }

        public void RegisterTokenObject(string tokenId, GameObject go)
            => _tokenObjects[tokenId] = go;
    }
}
```

---

## Part 2: Ivory Stampede Behavior Cards

Create 12 cards in `Assets/_Game/Data/Monsters/IvoryStampede/BehaviorCards/`.

The pack's deck is smaller — the herd's numbers compensate for fewer card types.

| cardId | cardName | movementEffect | attackEffect | specialEffect |
|---|---|---|---|---|
| `IST-B01` | Herd Advance | Alpha moves 1 toward aggro. | — | — |
| `IST-B02` | Stampede | Alpha moves 2 toward aggro. | 2 Flesh to aggro target. | Flankers also move 1 toward their targets. |
| `IST-B03` | Gore Rush | Alpha moves 1 toward aggro. | 3 Flesh to aggro target. | — |
| `IST-B04` | Encircle | Alpha holds. | — | Each Flanker moves 2 toward their target. Flankers deal auto-attack if adjacent. |
| `IST-B05` | Ivory Crash | Alpha moves 1. | 2 Flesh to aggro target. Knockback 1 space. | — |
| `IST-B06` | Tusk Drive | Alpha holds. | 4 Flesh to aggro target. | Cannot be performed if aggro target is not adjacent. isShuffle: true |
| `IST-B07` | Thunder Run | Alpha moves 3 toward aggro, ignoring Knockback. | 1 Flesh to all hunters in movement path. | — |
| `IST-B08` | Trample Circle | Alpha pivots to lowest-Flesh target. | — | All three tokens deal 1 Flesh to any adjacent hunter. |
| `IST-B09` | Regroup | Alpha moves toward centre of grid. | — | Flankers also move 1 toward Alpha. |
| `IST-B10` | Berserk | Trigger: any token below 50% Flesh. Alpha moves 2 toward aggro. | 3 Flesh. | Flankers move 2 and auto-attack if adjacent. isShuffle: true |
| `IST-B11` | Survey | Alpha holds. | — | Aggro moves to hunter with highest Accuracy. |
| `IST-B12` | Mass Charge | All tokens move 2 toward nearest hunter. | Each deals 1 Flesh damage to nearest hunter. | isShuffle: true |

---

## Part 3: Ivory Stampede MonsterSO

Create `Assets/_Game/Data/Monsters/IvoryStampede/IvoryStampede_Standard.asset` using PackMonsterSO:

```
monsterName: The Ivory Stampede
monsterType: Pack Beast
difficulty: Standard
huntYearMin: 3
huntYearMax: 7
tokenCount: 3

Alpha Parts:
  Crown: shellHP=5, fleshHP=7
  Chest: shellHP=4, fleshHP=9
  Tusk: shellHP=3, fleshHP=5

Flanker Parts:
  Body: shellHP=3, fleshHP=6
  Tusk: shellHP=2, fleshHP=4

flankerAdjacentDamage: 1
behaviorDeck: [IST-B01 through IST-B12]
```

---

## Verification Test

- [ ] PackMonsterSO asset creates without error in Unity
- [ ] Ivory Stampede has 3 tokens placed on the combat grid at hunt start
- [ ] Each token has its own HP bars (Alpha has Crown + Chest + Tusk; Flankers have Body + Tusk)
- [ ] Each round: one card drawn, Alpha resolves full card, Flankers resolve movement only
- [ ] Flanker that ends its turn adjacent to its target deals 1 automatic Flesh damage
- [ ] Kill the Alpha → `[PackAI] Flanker_1 promoted to Alpha` in Console
- [ ] New Alpha now resolves full card behavior next round
- [ ] Kill all 3 tokens → hunt victory triggered
- [ ] IST-B10 (Berserk) only fires when a token is below 50% Flesh; otherwise no special
- [ ] IST-B12 (Mass Charge): all 3 tokens move and attack in the same round
- [ ] No NullReferenceException if a hunter dies mid-pack-resolution

---

## Next Session

**File:** `_Docs/Stage_09/STAGE_09_H.md`
**Covers:** Bog Caller — a full monster design including lore, parts, 16 behavior cards, and MonsterSO asset. The Bog Caller is a mid-tier ambush predator that uses terrain and poison.
