using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Inventory.Model
{
    [CreateAssetMenu(fileName = "AmmoItem_", menuName = "Inventory/AmmoItem")]
    public class AmmoItemSO : ItemSO, IDestroyableItem
    {
        [field: SerializeField]
        public AmmoType ammoType;
    }
}
