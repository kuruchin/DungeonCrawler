using Inventory.Model;
using Inventory.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Inventory
{
    public class InventoryController : MonoBehaviour
    {
        [SerializeField]
        private UIInventoryPage inventoryUI;

        [SerializeField]
        private UIEquipedWeaponPage equipedWeaponUI;

        [SerializeField]
        private InventorySO inventoryData;

        public List<InventoryItem> initialItems = new List<InventoryItem>();

        [SerializeField]
        private AudioClip dropClip;

        [SerializeField]
        private AudioSource audioSource;

        private Player player;

        private void Start()
        {
            inventoryUI = FindObjectOfType<UIInventoryPage>();

            if(inventoryUI != null)
            {
                PrepareUI();
                PrepareInventoryData();
            }

            player = GetComponent<Player>();
        }

        public void Update()
        {

        }

        private void PrepareInventoryData()
        {
            inventoryData.Initialize();
            inventoryData.OnInventoryUpdated += HandleInventoryUpdate;
            foreach (InventoryItem item in initialItems)
            {
                if (item.IsEmpty)
                    continue;
                inventoryData.AddItem(StorageType.Inventory, item);
            }
        }

        private void OnDisable()
        {
            inventoryData.OnInventoryUpdated -= HandleInventoryUpdate;
        }

        private void HandleInventoryUpdate(bool isInventoryOnlyChanged, Dictionary<StorageType, Dictionary<int, InventoryItem>> inventoryState)
        {
            // Updating weapon data
            if (isInventoryOnlyChanged != false && player != null)
            {
                player.UpdateWeaponList(inventoryState);
            }

            UpdateInventoryUI(inventoryState);
        }

        public void ShowInventory()
        {
            inventoryUI.Show();

            UpdateInventoryUI(inventoryData.GetCurrentInventoryState());
        }

        public void HideInventory()
        {
            inventoryUI.Hide();
        }

        private void UpdateInventoryUI(Dictionary<StorageType, Dictionary<int, InventoryItem>> inventoryState)
        {
            inventoryUI.ResetAllItems();

            foreach (var storageType in inventoryState)
            {
                Dictionary<int, InventoryItem> storageDictionary = storageType.Value;

                foreach (var pair in storageDictionary)
                {
                    int itemIndex = pair.Key;
                    InventoryItem item = pair.Value;

                    inventoryUI.UpdateData( item.storageType,
                                            itemIndex,
                                            item.item.ItemImage,
                                            item.quantity);
                }
            }
        }

        private void PrepareUI()
        {
            inventoryUI.InitializeInventoryUI(inventoryData.InventorySize, inventoryData.EquipedSize);
            inventoryUI.OnDescriptionRequested += HandleDescriptionRequest;
            inventoryUI.OnSwapItems += HandleSwapItems;
            inventoryUI.OnStartDragging += HandleDragging;
            inventoryUI.OnItemActionRequested += HandleItemActionRequest;
        }

        private void HandleItemActionRequest(StorageType storageType, int itemIndex)
        {
            InventoryItem inventoryItem;
            bool isItemReturned = inventoryData.GetItemAt(storageType, itemIndex, out inventoryItem);

            if (!isItemReturned && inventoryItem.IsEmpty)
                return;

            IItemAction itemAction = inventoryItem.item as IItemAction;
            if(itemAction != null)
            {
                inventoryUI.ShowItemAction(storageType, itemIndex);
                inventoryUI.AddAction(itemAction.ActionName, () => PerformAction(storageType, itemIndex));
            }

            IDestroyableItem destroyableItem = inventoryItem.item as IDestroyableItem;
            if (destroyableItem != null)
            {
                inventoryUI.AddAction("Drop", () => DropItem(itemIndex, inventoryItem.quantity));
            }

        }

        private void DropItem(int itemIndex, int quantity)
        {
            inventoryData.RemoveItem(itemIndex, quantity);
            inventoryUI.ResetSelection();
            audioSource.PlayOneShot(dropClip);
        }

        public void PerformAction(StorageType storageType, int itemIndex)
        {
            InventoryItem inventoryItem;
            bool isItemReturned = inventoryData.GetItemAt(storageType, itemIndex, out inventoryItem);

            if (!isItemReturned && inventoryItem.IsEmpty)
                return;

            IDestroyableItem destroyableItem = inventoryItem.item as IDestroyableItem;
            if (destroyableItem != null)
            {
                inventoryData.RemoveItem(itemIndex, 1);
            }

            IItemAction itemAction = inventoryItem.item as IItemAction;
            if (itemAction != null)
            {
                itemAction.PerformAction(gameObject, inventoryItem.itemParameters);
                audioSource.PlayOneShot(itemAction.actionSFX);

                inventoryUI.ResetSelection();
                //if (inventoryData.GetItemAt(itemIndex).IsEmpty)
                //    inventoryUI.ResetSelection();
            }
        }

        private void HandleDragging(StorageType storageType, int itemIndex)
        {
            InventoryItem inventoryItem;
            bool isItemReturned = inventoryData.GetItemAt(storageType, itemIndex, out inventoryItem);

            if (!isItemReturned && inventoryItem.IsEmpty)
                return;

            inventoryUI.CreateDraggedItem(inventoryItem.item.ItemImage, inventoryItem.quantity);
        }

        private void HandleSwapItems(StorageType currentItemStorageType, int currentItemIndex, StorageType targetItemStorageType, int targetItemIndex)
        {
            inventoryData.SwapItems(currentItemStorageType, currentItemIndex, targetItemStorageType, targetItemIndex);
        }

        private void HandleDescriptionRequest(StorageType storageType, int itemIndex)
        {
            InventoryItem inventoryItem;
            bool isItemReturned = inventoryData.GetItemAt(storageType, itemIndex, out inventoryItem);

            if (!isItemReturned || inventoryItem.IsEmpty)
            {
                inventoryUI.ResetSelection();
                return;
            }

            ItemSO item = inventoryItem.item;
            string description = PrepareDescription(inventoryItem);
            inventoryUI.UpdateDescription(storageType, itemIndex, item.ItemImage,item.name, description);
        }

        private string PrepareDescription(InventoryItem inventoryItem)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(inventoryItem.item.Description);
            sb.AppendLine();
            for (int i = 0; i < inventoryItem.itemParameters.Count; i++)
            {
                sb.Append($"{inventoryItem.itemParameters[i].itemParameter.ParameterName} " +
                    $": {inventoryItem.itemParameters[i].value} / " +
                    $"{inventoryItem.item.DefaultParametersList[i].value}");
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public UIInventoryPage GetInventoryUI()
        {
            return inventoryUI;
        }
    }
}