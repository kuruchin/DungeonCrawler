using Inventory.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DroppedItemsTable", menuName = "Inventory/DroppedItems")]
public class DroppedItemsTableSO : ScriptableObject
{
    public GameObject[] ammoItems;

    public GameObject GetRandomAmmo()
    {
        if (ammoItems == null || ammoItems.Length == 0)
        {
            Debug.LogError("Ammo items array is null or empty.");
            return null;
        }

        int randomIndex = Random.Range(0, ammoItems.Length);
        return ammoItems[randomIndex];
    }

    public GameObject GetAmmoItemByType(AmmoType ammoType)
    {
        foreach (var item in ammoItems)
        {
            var ammoItem = item.GetComponent<Item>();

            if(ammoItem != null)
            {
                var ammoItemType = (ammoItem.InventoryItem as AmmoItemSO).ammoType;

                if (ammoType == ammoItemType)
                {
                    return item;
                }
            }
        }
        return null;
    }
}
