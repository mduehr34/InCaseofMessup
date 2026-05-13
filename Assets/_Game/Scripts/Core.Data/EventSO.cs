using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/Event", fileName = "New Event")]
    public class EventSO : ScriptableObject
    {
        public string eventId;              // e.g. "EVT-01"
        public string eventName;
        public int yearRangeMin;
        public int yearRangeMax;
        public bool isMandatory;
        public string campaignTag;
        public string monsterTag;           // Empty if not monster-specific
        public string seasonTag;
        public string difficultyTag;
        [Header("Travel")]
        public bool isTravel;           // If true, can fire during the Hunt Travel scene
        [TextArea] public string narrativeText;
        // Max 2 choices. 0 choices = mandatory outcome, no player decision.
        public EventChoice[] choices;
    }
}
