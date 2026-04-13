using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/Character", fileName = "New Character")]
    public class CharacterSO : ScriptableObject
    {
        [Header("Identity")]
        public string characterName;
        public CharacterBuild bodyBuild;
        public CharacterSex sex;

        [Header("Stats — modified by events/innovations only, never by leveling")]
        public int accuracy;
        public int evasion;
        public int strength;
        public int toughness;
        public int luck;
        public int movement;

        [Header("Deck")]
        public ActionCardSO[] currentDeck;
        public InjuryCardSO[] injuryCards;
        public ActionCardSO[] fightingArts;
        public ActionCardSO[] disorders;

        [Header("Proficiency")]
        public WeaponProficiency[] weaponProficiencies;

        [Header("State")]
        public int huntCount;
        public bool isRetired;

        [Header("Gear")]
        public ItemSO[] equippedItems;
        public WeaponSO equippedWeapon;
    }
}
