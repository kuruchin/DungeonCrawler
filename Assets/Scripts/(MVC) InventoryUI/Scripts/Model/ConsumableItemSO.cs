using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu(fileName = "ConsumableItem_", menuName = "Inventory/ConsumableItem")]
    public class ConsumableItemSO : ItemSO
    {
        public override void ActionOnPickup(Item item)
        {
            foreach (ItemParameter parameter in item.InventoryItem.DefaultParametersList)
            {
                if (parameter.IsParameterTypeEquals(ItemParameterType.HealthPointsRecovery))
                {
                    Health health = GameManager.Instance.GetPlayer().GetComponent<Health>();

                    if (health != null)
                    {
                        SoundEffectManager.Instance.PlaySoundEffect(item.soundEffect);

                        health.AddHealth((int)parameter.value);

                        item.DestroyItem();
                    }
                }

                if (parameter.IsParameterTypeEquals(ItemParameterType.CurrentWeaponAmmoRecovery))
                {
                    ActiveWeapon activeWeapon = GameManager.Instance.GetPlayer().GetComponent<ActiveWeapon>();

                    if (activeWeapon != null)
                    {
                        var currentAmmoSO = activeWeapon.GetCurrentAmmo();

                        if(currentAmmoSO != null)
                        {
                            AmmoType currentAmmo = currentAmmoSO.ammoType;

                            var ammoItemPrefab = GameResources.Instance.droppedItemsTableSO.GetAmmoItemByType(currentAmmo);

                            // Creating new item
                            var ammoItemGameobject = Instantiate(ammoItemPrefab);

                            var ammoItem = ammoItemGameobject.GetComponent<Item>();
                            
                            // Set quantity
                            ammoItem.Quantity = item.Quantity;

                            GameManager.Instance.GetPlayer().GetComponent<InventoryController>().PickUpItem(ammoItem);

                            SoundEffectManager.Instance.PlaySoundEffect(item.soundEffect);

                            ammoItem.DestroyItem();
                            item.DestroyItem();
                        }
                    }
                }
            }
        }
    }
}
