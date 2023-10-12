using Inventory.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentWeapon : MonoBehaviour
{
    [SerializeField]
    private EquippableItemSO weapon;

    [SerializeField]
    private InventorySO inventoryData;

    [SerializeField]
    private List<ItemParameter> parametersToModify, itemCurrentState;

    public void SetWeapon(EquippableItemSO weaponItemSO, List<ItemParameter> itemState, WeaponDetailsSO weaponDetailsSO)
    {
        if (weapon != null)
        {
            // Added current weapon to the inventory
            inventoryData.AddItem(StorageType.Inventory, weapon, 1, itemCurrentState);
        }

        this.weapon = weaponItemSO;
        this.itemCurrentState = new List<ItemParameter>(itemState);
        EquipWeaponItem(weaponDetailsSO);
        ModifyParameters();
    }

    private void ModifyParameters()
    {
        foreach (var parameter in parametersToModify)
        {
            if (itemCurrentState.Contains(parameter))
            {
                int index = itemCurrentState.IndexOf(parameter);
                float newValue = itemCurrentState[index].value + parameter.value;
                itemCurrentState[index] = new ItemParameter
                {
                    itemParameter = parameter.itemParameter,
                    value = newValue
                };
            }
        }
    }

    private void EquipWeaponItem(WeaponDetailsSO weaponDetailsSO)
    {
        // Check item exists and has been materialized
        if (weaponDetailsSO == null) return;

        // If the player doesn't already have the weapon, then add to player
        if (!GameManager.Instance.GetPlayer().IsWeaponHeldByPlayer(weaponDetailsSO))
        {
            // Add weapon to player
            GameManager.Instance.GetPlayer().AddWeaponToPlayer(weaponDetailsSO);

            // Play pickup sound effect
            SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.weaponPickup);
        }
        else
        {
            // display message saying you already have the weapon
            Debug.Log("you already have the weapon");

        }
    }
}
