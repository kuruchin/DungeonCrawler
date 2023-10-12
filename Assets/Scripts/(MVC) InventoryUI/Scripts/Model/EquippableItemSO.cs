using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu(fileName = "EquippableItem_", menuName = "Inventory/EquippableItem")]
    public class EquippableItemSO : ItemSO, IDestroyableItem, IItemAction
    {
        public string ActionName => "Equip";

        [field: SerializeField]
        public WeaponDetailsSO weaponDetails;

        [field: SerializeField]
        public AudioClip actionSFX { get; private set; }

        public bool PerformAction(GameObject character, List<ItemParameter> itemState = null)
        {
            AgentWeapon weaponSystem = character.GetComponent<AgentWeapon>();
            if (weaponSystem != null && weaponDetails != null)
            {
                weaponSystem.SetWeapon(this, itemState == null ? DefaultParametersList : itemState, weaponDetails);
                return true;
            }
            return false;
        }
    }
}