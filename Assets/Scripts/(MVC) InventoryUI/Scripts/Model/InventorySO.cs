using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu(fileName = "Inventory_", menuName = "Inventory/Inventory")]
    public class InventorySO : ScriptableObject
    {
        [SerializeField]
        private List<InventoryItem> equipedItems = new List<InventoryItem>();

        [SerializeField]
        private List<InventoryItem> inventoryItems = new List<InventoryItem>();

        public int[] StorageSize = { 10, 2 };

        [field: SerializeField]
        public int EquipedSize { get; private set; } = 2;

        [field: SerializeField]
        public int InventorySize { get; private set; } = 10;

        public event Action<bool, Dictionary<StorageType, Dictionary<int, InventoryItem>>> OnInventoryUpdated;

        public void Initialize()
        {
            foreach (StorageType storageType in Enum.GetValues(typeof(StorageType)))
            {
                if (storageType == StorageType.None)
                    continue;

                List<InventoryItem> itemList = GetItemListByStorageType(storageType);

                if (itemList == null)
                    continue;

                itemList.Clear();
                //itemList = new List<InventoryItem>();

                for (int i = 0; i < StorageSize[(int)storageType]; i++)
                {
                    itemList.Add(InventoryItem.GetEmptyItem(storageType));
                }
            }
        }

        public int AddItem(StorageType storageType, ItemSO item, int quantity, List<ItemParameter> itemState = null)
        {
            if(item.IsStackable == false)
            {
                // TODO: check other storages
                List<InventoryItem> itemList = GetItemListByStorageType(storageType);

                if (itemList != null)
                {
                    for (int i = 0; i < itemList.Count; i++)
                    {
                        while (quantity > 0 && IsInventoryFull() == false)
                        {
                            quantity -= AddItemToFirstFreeSlot(storageType, item, 1, itemState);
                        }
                        InformAboutChange();
                        return quantity;
                    }
                }
            }
            quantity = AddStackableItem(storageType, item, quantity);
            InformAboutChange();
            return quantity;
        }

        private int AddItemToFirstFreeSlot(StorageType storageType, ItemSO item, int quantity, List<ItemParameter> itemState = null)
        {
            InventoryItem newItem = new InventoryItem
            {
                item = item,
                quantity = quantity,
                itemParameters = new List<ItemParameter>(itemState == null ? item.DefaultParametersList : itemState),
                storageType = storageType
            };

            List<InventoryItem> itemList = GetItemListByStorageType(storageType);

            if (itemList == null)
                return 0;  // TODO: check

            for (int i = 0; i < itemList.Count; i++)
            {
                if (itemList[i].IsEmpty)
                {
                    itemList[i] = newItem;
                    return quantity;
                }
            }
            return 0;
        }

        private bool IsInventoryFull() => inventoryItems.Where(item => item.IsEmpty).Any() == false;

        private int AddStackableItem(StorageType storageType, ItemSO item, int quantity)
        {
            List<InventoryItem> itemList = GetItemListByStorageType(storageType);

            if (itemList == null)
                return 0;  // TODO: check

            for (int i = 0; i < itemList.Count; i++)
            {
                if (itemList[i].IsEmpty)
                    continue;
                if(itemList[i].item.ID == item.ID)
                {
                    int amountPossibleToTake = itemList[i].item.MaxStackSize - itemList[i].quantity;

                    if (quantity > amountPossibleToTake)
                    {
                        itemList[i] = itemList[i].ChangeQuantity(itemList[i].item.MaxStackSize);
                        quantity -= amountPossibleToTake;
                    }
                    else
                    {
                        itemList[i] = itemList[i].ChangeQuantity(itemList[i].quantity + quantity);
                        InformAboutChange();
                        return 0;
                    }
                }
            }
            while(quantity > 0 && IsInventoryFull() == false)
            {
                int newQuantity = Mathf.Clamp(quantity, 0, item.MaxStackSize);
                quantity -= newQuantity;
                AddItemToFirstFreeSlot(storageType, item, newQuantity);
            }
            return quantity;
        }

        public void RemoveItem(int itemIndex, int amount)
        {
            if (inventoryItems.Count > itemIndex)
            {
                if (inventoryItems[itemIndex].IsEmpty)
                    return;
                int reminder = inventoryItems[itemIndex].quantity - amount;
                if (reminder <= 0)
                    inventoryItems[itemIndex] = InventoryItem.GetEmptyItem(StorageType.Inventory); //!!!!!!!!!!!!!!!!!! REWORK
                else
                    inventoryItems[itemIndex] = inventoryItems[itemIndex].ChangeQuantity(reminder);

                InformAboutChange();
            }
        }

        public void AddItem(StorageType storageType, InventoryItem item)
        {
            AddItem(storageType, item.item, item.quantity);
        }

        public Dictionary<StorageType, Dictionary<int, InventoryItem>> GetCurrentInventoryState()
        {
            Dictionary<StorageType, Dictionary<int, InventoryItem>> returnValue = new Dictionary<StorageType, Dictionary<int, InventoryItem>>();

            foreach (StorageType storageType in Enum.GetValues(typeof(StorageType)))
            {
                if (storageType == StorageType.None)
                    continue;

                Dictionary<int, InventoryItem> itemDictionary = new Dictionary<int, InventoryItem>();

                List<InventoryItem> itemList = GetItemListByStorageType(storageType);

                if (itemList != null)
                {
                    for (int i = 0; i < itemList.Count; i++)
                    {
                        if (itemList[i].IsEmpty)
                            continue;

                        itemDictionary.Add(i, itemList[i]);
                    }
                }

                returnValue.Add(storageType, itemDictionary);
            }

            return returnValue;
        }

        public bool GetItemAt(StorageType storageType, int itemIndex, out InventoryItem item)
        {
            List<InventoryItem> itemList = GetItemListByStorageType(storageType);

            if (itemList == null)
            {
                item = InventoryItem.GetEmptyItem(storageType);
                return false;
            }

            item = itemList[itemIndex];
            return true;
        }

        public List<InventoryItem> GetItemListByStorageType(StorageType storageType)
        {
            switch (storageType)
            {
                case StorageType.Inventory:
                    return inventoryItems;

                case StorageType.EquipedWeapon:
                    return equipedItems;

                case StorageType.None:
                default:
                    return null;
            }
        }

        public void SwapItems(StorageType currentItemStorageType, int currentItemIndex, StorageType targetItemStorageType, int targetItemIndex)
        {
            List<InventoryItem> currentItemList = GetItemListByStorageType(currentItemStorageType);
            List<InventoryItem> targetItemList = GetItemListByStorageType(targetItemStorageType);

            if (currentItemList == null || targetItemList == null)
                return;

            InventoryItem currentItem = currentItemList[currentItemIndex];

            currentItemList[currentItemIndex] = currentItemList[currentItemIndex].SwapItemTo(targetItemList[targetItemIndex]);
            targetItemList[targetItemIndex]= targetItemList[targetItemIndex].SwapItemTo(currentItem);
            
            // Check if inventory changed only
            bool isInventoryOnlyChanged = false;
            if(currentItemStorageType != StorageType.Inventory || targetItemStorageType != StorageType.Inventory)
            {
                isInventoryOnlyChanged = true;
            }

            InformAboutChange(isInventoryOnlyChanged);
        }

        private void InformAboutChange(bool isInventoryOnlyChanged = false)
        {
            Debug.Log("Inventory update");
            OnInventoryUpdated?.Invoke(isInventoryOnlyChanged, GetCurrentInventoryState());
        }
    }

    [Serializable]
    public struct InventoryItem
    {
        public ItemSO item;
        public int quantity;
        public List<ItemParameter> itemParameters;
        public StorageType storageType;
        public bool IsEmpty => item == null;

        public InventoryItem ChangeQuantity(int newQuantity)
        {
            return new InventoryItem
            {
                item = this.item,
                quantity = newQuantity,
                itemParameters = new List<ItemParameter>(this.itemParameters),
                storageType = this.storageType
            };
        }

        public InventoryItem SwapItemTo(InventoryItem itemForSwap)
        {
            return new InventoryItem
            {
                item = itemForSwap.item,
                quantity = itemForSwap.quantity,
                itemParameters = itemForSwap.itemParameters,
                storageType = this.storageType
            };
        }

        public static InventoryItem GetEmptyItem(StorageType storageType) => new InventoryItem
        {
            item = null,
            quantity = 0,
            itemParameters = new List<ItemParameter>(),
            storageType = storageType
        };
    }

    [Serializable]
    public class InventoryItemList
    {
        public List<InventoryItem> items = new List<InventoryItem>();
    }
}



public enum StorageType
    {
        None = -1,
        Inventory = 0,
        EquipedWeapon = 1
    }