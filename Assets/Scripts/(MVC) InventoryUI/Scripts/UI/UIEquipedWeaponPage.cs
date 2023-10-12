using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.UI
{
    public class UIEquipedWeaponPage : MonoBehaviour
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

        private int currentlyDraggedItemIndex = -1;

        public event Action<int>
                OnDescriptionRequested,
                OnItemActionRequested,
                OnStartDragging;

        public event Action<int, int> OnSwapItems;

        [SerializeField]
        private ItemActionPanel actionPanel;

        private void Awake()
        {
            Hide();
            mouseFollower.Toggle(false);
            itemDescription.ResetDescription();
        }

        public void InitializeEquipedUI(int inventorySize, int equipedWeaponSize)
        {
            InitializationRoutine(inventorySize, inventoryContentPanel, ref listOfUIInventoryItems);
            //InitializationRoutine(equipedWeaponSize, equipedWeaponContentPanel, ref listOfUIEquipedWeaponsItems);
        }

        private void InitializationRoutine(int size, RectTransform transformParent, ref List<UIInventoryItem> listOfUIItems)
        {
            for (int i = 0; i < size; i++)
            {
                UIInventoryItem uiItem = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity);
                uiItem.transform.SetParent(transformParent);
                listOfUIItems.Add(uiItem);
                uiItem.OnItemClicked += HandleItemSelection;
                uiItem.OnItemBeginDrag += HandleBeginDrag;
                uiItem.OnItemDroppedOn += HandleSwap;
                uiItem.OnItemEndDrag += HandleEndDrag;
                uiItem.OnRightMouseBtnClick += HandleShowItemActions;
            }
        }

        internal void ResetAllItems()
        {
            foreach (var item in listOfUIInventoryItems)
            {
                item.ResetData();
                item.Deselect();
            }
        }

        internal void UpdateDescription(int itemIndex, Sprite itemImage, string name, string description)
        {
            itemDescription.SetDescription(itemImage, name, description);
            DeselectAllItems();
            listOfUIInventoryItems[itemIndex].Select();
        }

        public void UpdateData(int itemIndex, Sprite itemImage, int itemQuantity)
        {
            if (listOfUIInventoryItems.Count > itemIndex)
            {
                listOfUIInventoryItems[itemIndex].SetData(itemImage, itemQuantity);
            }
        }

        private void HandleShowItemActions(UIInventoryItem inventoryItemUI)
        {
            int index = listOfUIInventoryItems.IndexOf(inventoryItemUI);
            if (index == -1)
            {
                return;
            }
            OnItemActionRequested?.Invoke(index);
        }

        private void HandleEndDrag(UIInventoryItem inventoryItemUI)
        {
            ResetDraggedItem();
        }

        private void HandleSwap(UIInventoryItem inventoryItemUI)
        {
            Debug.Log("Swapped: " + inventoryItemUI.name);
            int index = listOfUIInventoryItems.IndexOf(inventoryItemUI);
            if (index == -1)
            {
                return;
            }
            OnSwapItems?.Invoke(currentlyDraggedItemIndex, index);
            HandleItemSelection(inventoryItemUI);
        }

        private void ResetDraggedItem()
        {
            mouseFollower.Toggle(false);
            currentlyDraggedItemIndex = -1;
        }

        private void HandleBeginDrag(UIInventoryItem inventoryItemUI)
        {
            int index = listOfUIInventoryItems.IndexOf(inventoryItemUI);
            if (index == -1)
                return;
            currentlyDraggedItemIndex = index;
            HandleItemSelection(inventoryItemUI);
            OnStartDragging?.Invoke(index);
        }

        public void CreateDraggedItem(Sprite sprite, int quantity)
        {
            mouseFollower.Toggle(true);
            mouseFollower.SetData(sprite, quantity);
        }

        private void HandleItemSelection(UIInventoryItem inventoryItemUI)
        {
            int index = listOfUIInventoryItems.IndexOf(inventoryItemUI);
            if (index == -1)
                return;
            OnDescriptionRequested?.Invoke(index);
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
            //actionPanel.AddButon(actionName, performAction);
        }

        public void ShowItemAction(int itemIndex)
        {
            //actionPanel.Toggle(true);
            //actionPanel.transform.position = listOfUIInventoryItems[itemIndex].transform.position;
        }

        private void DeselectAllItems()
        {
            foreach (UIInventoryItem item in listOfUIInventoryItems)
            {
                item.Deselect();
            }
            //actionPanel.Toggle(false);
        }

        public void Hide()
        {
            //actionPanel.Toggle(false);
            gameObject.SetActive(false);
            ResetDraggedItem();
        }
    }

}
