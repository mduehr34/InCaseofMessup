using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MnM.Core.Data;

namespace MnM.Core.Systems
{
    public class GameStateManager : MonoBehaviour
    {
        // ── Singleton ────────────────────────────────────────────
        public static GameStateManager Instance { get; private set; }

        // ── State ────────────────────────────────────────────────
        public CampaignState CampaignState { get; private set; }
        public CampaignSO    CampaignData  { get; private set; }
        public CombatState   CombatState   { get; private set; }
        public HuntResult    LastHuntResult { get; private set; }

        // Hunt selection — stored between Settlement and Travel scenes
        public MonsterSO              SelectedMonster    { get; private set; }
        public string                 SelectedDifficulty { get; private set; }
        public RuntimeCharacterState[] SelectedHunters   { get; private set; }

        // Gear Grid — characterId to open on GearGrid scene load
        public string PendingGearGridCharacterId { get; private set; }

        // ── Lifecycle ────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[GSM] GameStateManager initialized — persisting across scenes");
        }

        // Pending setup — populated by CampaignSelect, consumed by CharacterCreation
        public CampaignSO PendingCampaign   { get; private set; }
        public string     PendingDifficulty { get; private set; }
        public bool       PendingIronman    { get; private set; }

        // ── Campaign Lifecycle ────────────────────────────────────
        public void PrepareNewCampaign(CampaignSO campaign, string difficulty, bool ironman)
        {
            PendingCampaign   = campaign;
            PendingDifficulty = difficulty;
            PendingIronman    = ironman;
            Debug.Log($"[GSM] Pending campaign: {campaign.campaignName} / {difficulty} / Ironman={ironman}");
        }

        public void StartNewCampaign(CampaignSO campaignData)
        {
            CampaignData  = campaignData;
            CampaignState = CampaignInitializer.CreateNewCampaign(campaignData);
            Debug.Log($"[GSM] New campaign started: {campaignData.campaignName} " +
                      $"Year:{CampaignState.currentYear}");
            SceneManager.LoadScene("Settlement");
        }

        // Called by CharacterCreationController after player confirms hunter names.
        // Initialises CampaignState from PendingCampaign; character name override is a
        // TODO deferred to the full runtime-character system (later stage).
        public void StartNewCampaign(List<HunterGenerationData> hunters)
        {
            if (PendingCampaign != null)
            {
                CampaignData  = PendingCampaign;
                CampaignState = CampaignInitializer.CreateNewCampaign(PendingCampaign);
            }
            Debug.Log($"[GSM] Campaign started with {hunters.Count} hunters");
            // TODO: override CampaignState.characters with player-named hunter data
        }

        public void LoadCampaign(CampaignState state, CampaignSO campaignData)
        {
            CampaignData  = campaignData;
            CampaignState = state;
            Debug.Log($"[GSM] Campaign loaded. Year:{state.currentYear}");
            SceneManager.LoadScene("Settlement");
        }

        // ── Hunt Flow ────────────────────────────────────────────
        public void PrepareHunt(
            MonsterSO monster,
            string difficulty,
            RuntimeCharacterState[] hunters)
        {
            SelectedMonster    = monster;
            SelectedDifficulty = difficulty;
            SelectedHunters    = hunters;

            // Build CombatState from current campaign + selected hunt parameters
            CombatState = CampaignInitializer.BuildCombatState(
                CampaignState, monster, difficulty, hunters);

            Debug.Log($"[GSM] Hunt prepared: {monster.monsterName} ({difficulty}) " +
                      $"with {hunters.Length} hunters");
            SceneManager.LoadScene("Travel");
        }

        public void BeginCombat()
        {
            // Travel scene calls this after travel events are resolved
            Debug.Log("[GSM] Beginning combat — loading CombatScene");
            SceneManager.LoadScene("CombatScene");
        }

        public void ReturnFromHunt(HuntResult result)
        {
            LastHuntResult = result;
            CampaignState.pendingHuntResult = result;
            SaveManager.Save(CampaignState);
            Debug.Log($"[GSM] Returning from hunt. Victory:{result.isVictory}");
            SceneManager.LoadScene("Settlement");
        }

        // ── Navigation Helpers ───────────────────────────────────
        public void OpenGearGrid(string characterId)
        {
            PendingGearGridCharacterId = characterId;
            SceneManager.LoadScene("GearGrid");
        }

        public void GoToMainMenu()
        {
            // Save before leaving (if campaign active)
            if (CampaignState != null)
                SaveManager.Save(CampaignState);
            SceneManager.LoadScene("MainMenu");
        }

        public void OpenCodex()
        {
            // Codex can be opened from Settlement — loads additively
            SceneManager.LoadScene("Codex", LoadSceneMode.Additive);
        }

        // ── State Validation ─────────────────────────────────────
        public bool HasActiveCampaign => CampaignState != null;
        public bool HasSave           => SaveManager.HasSave();
    }
}
