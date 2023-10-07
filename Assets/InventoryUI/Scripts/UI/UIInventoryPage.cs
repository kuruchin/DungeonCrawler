using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.UI
{
    public class UIInventoryPage : MonoBehaviour
    {
        [SerializeField]
        private UIInventoryItem itemPrefab;

        [SerializeField]
        private RectTransform inventoryContentPanel;

        [SerializeField]
        private RectTransform equipedWeaponContentPanel;

        [SerializeField]
        private UIInventoryDescription itemDescription;

        [SerializeField]
        private MouseFollower mouseFollower;

        List<UIInventoryItem> listOfUIInventoryItems = new List<UIInventoryItem>();
        List<UIInventoryItem> listOfUIEquipedWeaponsItems = new List<UIInventoryItem>();
        //Dictionary<StorageType, List<UIInventoryItem>> UIInventoryItems = new Dictionary<StorageType, List<UIInventoryItem>>();

        Dictionary<StorageType, List<UIInventoryItem>> UIInventoryItems = new Dictionary<StorageType, List<UIInventoryItem>>
        {
            { StorageType.Inventory,     new List<UIInventoryItem>() },
            { StorageType.EquipedWeapon, new List<UIInventoryItem>() },
        };

        private int currentlyDraggedItemIndex = -1;
        private StorageType currentlyDraggedItemStorageType = StorageType.None;

        public event Action<StorageType, int> 
                OnDescriptionRequested,
                OnItemActionRequested,
                OnStartDragging;

        public event Action<StorageType, int, StorageType, int> OnSwapItems;

        [SerializeField]
        private ItemActionPanel actionPanel;

        private void Awake()
        {

        }

        public void InitializeInventoryUI(int inventorySize, int equipedWeaponSize)
        {
            InitializationRoutine(inventorySize, inventoryContentPanel, StorageType.Inventory);
            InitializationRoutine(equipedWeaponSize, equipedWeaponContentPanel, StorageType.EquipedWeapon);

            Hide();
            mouseFollower.Toggle(false);
            itemDescription.ResetDescription();
        }

        private void InitializationRoutine(int size, RectTransform transformParent, StorageType storageType)
        {
            for (int i = 0; i < size; i++)
            {
                UIInventoryItem uiItem = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity);
                uiItem.transform.SetParent(transformParent);
                UIInventoryItems[storageType].Add(uiItem);
                uiItem.OnItemClicked += HandleItemSelection;
                uiItem.OnItemBeginDrag += HandleBeginDrag;
                uiItem.OnItemDroppedOn += HandleSwap;
                uiItem.OnItemEndDrag += HandleEndDrag;
                uiItem.OnRightMouseBtnClick += HandleShowItemActions;
            }
        }

        internal void ResetAllItems()
        {
            foreach (var storageType in UIInventoryItems.Keys)
            {
                foreach (var item in UIInventoryItems[storageType])
                {
                    item.ResetData();
                    item.Deselect();
                }
            }
        }

        internal void UpdateDescription(StorageType storageType, int itemIndex, Sprite itemImage, string name, string description)
        {
            itemDescription.SetDescription(itemImage, name, description);
            DeselectAllItems();

            if (storageType == StorageType.None)
                return;

            UIInventoryItems[storageType][itemIndex].Select();
        }

        public void UpdateData(StorageType storageType, int itemIndex, Sprite itemImage, int itemQuantity)
        {
            if (storageType == StorageType.None)
                return;
            if (UIInventoryItems[storageType].Count > itemIndex)
            {
                UIInventoryItems[storageType][itemIndex].SetData(itemImage, itemQuantity);
            }
        }

        private void HandleShowItemActions(UIInventoryItem inventoryItemUI)
        {
            //int index = listOfUIInventoryItems.IndexOf(inventoryItemUI);
            StorageType storagetype;
            int index;
            bool isItemReturned = GetInventoryItemStorageAndIndex(inventoryItemUI, out storagetype, out index);
            if (isItemReturned)
            {
                OnItemActionRequested?.Invoke(storagetype, index);
            }
        }

        private void HandleEndDrag(UIInventoryItem inventoryItemUI)
        {
            ResetDraggedItem();
        }

        private void HandleSwap(UIInventoryItem inventoryItemUI)
        {
            //Debug.Log("Swapped: " + inventoryItemUI.name);
            //int index = listOfUIInventoryItems.IndexOf(inventoryItemUI);
            StorageType storageType;
            int index;
            bool isItemReturned = GetInventoryItemStorageAndIndex(inventoryItemUI, out storageType, out index);

            if (isItemReturned)
            {
                OnSwapItems?.Invoke(currentlyDraggedItemStorageType, currentlyDraggedItemIndex, storageType, index);
                HandleItemSelection(inventoryItemUI);
            }
        }

        private void ResetDraggedItem()
        {
            mouseFollower.Toggle(false);
            currentlyDraggedItemIndex = -1;
            currentlyDraggedItemStorageType = StorageType.None;
        }

        private void HandleBeginDrag(UIInventoryItem inventoryItemUI)
        {
            StorageType storagetype;
            int index;
            bool isItemReturned = GetInventoryItemStorageAndIndex(inventoryItemUI, out storagetype, out index);
            if (isItemReturned)
            {
                currentlyDraggedItemIndex = index;
                currentlyDraggedItemStorageType = storagetype;
                HandleItemSelection(inventoryItemUI);
                OnStartDragging?.Invoke(storagetype, index);
            }
        }

        public void CreateDraggedItem(Sprite sprite, int quantity)
        {
            mouseFollower.Toggle(true);
            mouseFollower.SetData(sprite, quantity);
        }

        private void HandleItemSelection(UIInventoryItem inventoryItemUI)
        {
            StorageType storagetype;
            int index;
            bool isItemReturned = GetInventoryItemStorageAndIndex(inventoryItemUI, out storagetype, out index);
            if (isItemReturned)
            {
                OnDescriptionRequested?.Invoke(storagetype, index);
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
            ResetSelection();
        }

        public void ResetSelection()
        {
            itemDescription.ResetDescription();
            DeselectAllItems();
        }

        public void AddAction(string actionName, Action performAction)
        {
            actionPanel.AddButon(actionName, performAction);
        }

        public void ShowItemAction(StorageType storageType, int itemIndex)
        {
            actionPanel.Toggle(true);
            actionPanel.transform.position = UIInventoryItems[storageType][itemIndex].transform.position;
        }

        private void DeselectAllItems()
        {
            foreach (var inventoryList in UIInventoryItems)
            {
                foreach (UIInventoryItem item in inventoryList.Value)
                {
                    item.Deselect();
                }
            }

            actionPanel.Toggle(false);
        }

        public void Hide()
        {
            actionPanel.Toggle(false);
            gameObject.SetActive(false);
            ResetDraggedItem();
        }

        public StorageType GetStorageType(UIInventoryItem inventoryItemUI)
        {
            StorageType returnValue = StorageType.None;
            foreach (var inventoryList in UIInventoryItems)
            {
                foreach (var item in inventoryList.Value)
                {
                    if(item == inventoryItemUI)
                    {
                        returnValue = inventoryList.Key;
                        break;
                    }
                }
            }
            return returnValue;
        }

        private bool GetInventoryItemStorageAndIndex(UIInventoryItem inventoryItemUI, out StorageType storageType, out int index)
        {
            bool returnValuie = true;
            storageType = GetStorageType(inventoryItemUI);
            if (storageType == StorageType.None)
            {
                index = -1;
                returnValuie = false;
            }
            else
            {
                index = UIInventoryItems[storageType].IndexOf(inventoryItemUI);
                if (index == -1)
                {
                    returnValuie = false;
                }
            }
            return returnValuie;
        }
    }
}