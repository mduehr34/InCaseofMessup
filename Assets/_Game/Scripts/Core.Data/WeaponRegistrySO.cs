using System.Collections.Generic;
using UnityEngine;

namespace MnM.Core.Data
{
    /// <summary>
    /// Maps WeaponType enum to WeaponSO assets.
    /// Place one instance at Assets/_Game/Data/Resources/WeaponRegistry.asset.
    /// </summary>
    [CreateAssetMenu(menuName = "MnM/WeaponRegistry", fileName = "WeaponRegistry")]
    public class WeaponRegistrySO : ScriptableObject
    {
        public WeaponSO[] weapons;

        private Dictionary<WeaponType, WeaponSO> _lookup;

        public WeaponSO Get(WeaponType type)
        {
            if (_lookup == null)
                BuildLookup();

            _lookup.TryGetValue(type, out var result);
            return result;
        }

        private void BuildLookup()
        {
            _lookup = new Dictionary<WeaponType, WeaponSO>();
            if (weapons == null) return;
            foreach (var w in weapons)
            {
                if (w == null) continue;
                if (!_lookup.ContainsKey(w.weaponType))
                    _lookup[w.weaponType] = w;
                else
                    Debug.LogWarning($"[WeaponRegistry] Duplicate WeaponType: '{w.weaponType}' — second entry ignored");
            }
        }

        private void OnValidate() => _lookup = null;
    }
}
