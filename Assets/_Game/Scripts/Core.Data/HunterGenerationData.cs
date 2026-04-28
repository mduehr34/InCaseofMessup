namespace MnM.Core.Data
{
    [System.Serializable]
    public class HunterGenerationData
    {
        public string hunterName;  // Player-given name
        public string buildName;   // Aethel, Beorn, etc.
        public string sex;         // "M" or "F"
        public string spritePath;  // e.g. "Art/Generated/Characters/char_aethel_idle_s"
    }
}
