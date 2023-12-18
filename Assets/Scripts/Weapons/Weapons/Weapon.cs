using Inventory.Model;
using UnityEngine;

public class Weapon
{
    public WeaponDetailsSO weaponDetails;
    public int weaponListPosition;
    public float weaponReloadTimer;
    public int weaponClipRemainingAmmo;
    public int weaponRemainingAmmo;
    public bool isWeaponReloading;

    public Weapon GetWeapon(InventoryItem inventoryItem, int listPosition, float reloadTimer = 0f, bool isReloading = false)
    {
        EquippableItemSO equippedWeapon = inventoryItem.item as EquippableItemSO;

        if (equippedWeapon != null)
        {
            // Get clip Remaining Ammo 
            int clipAmmoRemaining = 0;
            foreach (ItemParameter itemParameter in inventoryItem.itemParameters)
            {
                if (itemParameter.IsParameterTypeEquals(ItemParameterType.ClipAmmoRemaining))
                {
                    // converting float to int
                    clipAmmoRemaining = (int)itemParameter.value;
                }
            }

            return new Weapon
            {
                weaponDetails = equippedWeapon.weaponDetails,
                weaponListPosition = listPosition,
                weaponReloadTimer = reloadTimer,
                weaponClipRemainingAmmo = clipAmmoRemaining,
                isWeaponReloading = isReloading
            };
        }

        return new Weapon();
    }
}
