using System.Collections.Generic;

namespace MnM.Core.Data
{
    public static class CombatStateFactory
    {
        // Builds a minimal valid CombatState for testing
        // Uses the canonical mock scenario: Aldric vs Gaunt Standard
        public static CombatState BuildMockCombatState()
        {
            var aldric = new HunterCombatState
            {
                hunterId            = "hunter_aldric",
                hunterName          = "Aldric",
                gridX               = -1,
                gridY               = -1,
                isUnplaced          = true,
                facingX             = 1,    // Facing East
                facingY             = 0,
                accuracy            = 3,
                strength            = 3,
                luck                = 0,
                movement            = 5,
                currentGrit         = 3,
                maxGrit             = 3,
                apRemaining         = 2,
                hasActedThisPhase   = false,
                isCollapsed         = false,
                bodyZones           = BuildHunterBodyZones(),
                handCardNames       = new[] { "Brace", "Shove" },
                deckCardNames       = new string[0],
                discardCardNames    = new string[0],
                activeStatusEffects = new string[0],
            };

            var gaunt = new MonsterCombatState
            {
                monsterName         = "The Gaunt",
                difficulty          = "Standard",
                gridX               = 12,
                gridY               = 7,
                facingX             = -1,   // Facing West
                facingY             = 0,
                footprintW          = 2,
                footprintH          = 2,
                parts               = BuildGauntStandardParts(),
                activeDeckCardNames = new[] { "Creeping Advance", "Scent Lock", "Flank Sense" },
                removedCardNames    = new string[0],
                currentStanceTag    = "",
                activeStatusEffects = new string[0],
            };

            return new CombatState
            {
                campaignId    = "mock_campaign",
                campaignYear  = 1,
                currentRound  = 0,
                currentPhase  = "DeploymentPhase",
                aggroHolderId = "hunter_aldric",
                hunters       = new[] { aldric },
                monster       = gaunt,
                grid          = BuildEmptyGrid(),
                log           = new string[0],
            };
        }

        private static BodyZoneState[] BuildHunterBodyZones()
        {
            return new[]
            {
                new BodyZoneState { zone = "Head",     shellCurrent = 2, shellMax = 2, fleshCurrent = 3, fleshMax = 3 },
                new BodyZoneState { zone = "Torso",    shellCurrent = 2, shellMax = 2, fleshCurrent = 3, fleshMax = 3 },
                new BodyZoneState { zone = "LeftArm",  shellCurrent = 1, shellMax = 1, fleshCurrent = 2, fleshMax = 2 },
                new BodyZoneState { zone = "RightArm", shellCurrent = 1, shellMax = 1, fleshCurrent = 2, fleshMax = 2 },
                new BodyZoneState { zone = "LeftLeg",  shellCurrent = 1, shellMax = 1, fleshCurrent = 2, fleshMax = 2 },
                new BodyZoneState { zone = "RightLeg", shellCurrent = 1, shellMax = 1, fleshCurrent = 2, fleshMax = 2 },
            };
        }

        private static MonsterPartState[] BuildGauntStandardParts()
        {
            // Shell 2 / Flesh 3 per part — Standard difficulty
            string[] partNames = { "Head", "Throat", "Torso", "Left Flank", "Right Flank", "Hind Legs", "Tail" };
            var parts = new List<MonsterPartState>();
            foreach (var name in partNames)
            {
                parts.Add(new MonsterPartState
                {
                    partName     = name,
                    shellCurrent = 2, shellMax = 2,
                    fleshCurrent = 3, fleshMax = 3,
                    isBroken     = false,
                    isRevealed   = true,    // No trap zones on Standard Gaunt
                    isExposed    = false,
                    woundCount   = 0,
                });
            }
            return parts.ToArray();
        }

        private static GridState BuildEmptyGrid()
        {
            return new GridState
            {
                width           = 22,
                height          = 16,
                deniedCells     = new DeniedCell[0],
                marrowSinkCells = new string[0],
            };
        }
    }
}
