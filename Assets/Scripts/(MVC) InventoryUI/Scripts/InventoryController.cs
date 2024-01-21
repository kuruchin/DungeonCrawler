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

        public event Action<InventoryUpdatedEventArgs> OnInventoryUpdated;

        private void Start()
        {

        }

        private void Awake()
        {
            inventoryUI = FindObjectOfType<UIInventoryPage>();

            player = GetComponent<Player>();

            if (inventoryUI != null)
            {
                PrepareUI();
                PrepareInventoryData();
            }
        }

        public void Update()
        {

        }

        private void PrepareInventoryData()
        {
            inventoryData.Initialize();
            inventoryData.OnInventoryModelUpdate += HandleInventoryModelUpdate;
            foreach (InventoryItem item in initialItems)
            {
                if (item.IsEmpty)
                    continue;
                inventoryData.AddItem(StorageType.Inventory, item);
            }

            // Subscribe to weapon fired event
            player.weaponFiredEvent.OnWeaponFired += WeaponFiredEvent_OnWeaponFired;

            // Subscribe to weapon reloaded event
            player.weaponReloadedEvent.OnWeaponReloaded += WeaponReloadedEvent_OnWeaponReloaded;
        }

        private void OnDisable()
        {
            inventoryData.OnInventoryModelUpdate -= HandleInventoryModelUpdate;

            player.weaponFiredEvent.OnWeaponFired -= WeaponFiredEvent_OnWeaponFired;

            // Unsubscribe from weapon reloaded event
            player.weaponReloadedEvent.OnWeaponReloaded -= WeaponReloadedEvent_OnWeaponReloaded;
        }

        /// <summary>
        /// Handle Weapon fired event by updating clip ammo capacity
        /// </summary>
        private void WeaponFiredEvent_OnWeaponFired(WeaponFiredEvent weaponFiredEvent, WeaponFiredEventArgs weaponFiredEventArgs)
        {
            inventoryData.UpdateClipAmmo(weaponFiredEventArgs.weapon);
        }

        /// <summary>
        /// Handle weapon reloaded event by updating clip ammo capacity
        /// </summary>
        private void WeaponReloadedEvent_OnWeaponReloaded(WeaponReloadedEvent weaponReloadedEvent, WeaponReloadedEventArgs weaponReloadedEventArgs)
        {
            inventoryData.UpdateClipAmmo(weaponReloadedEventArgs.weapon);
            AmmoType ammoType = weaponReloadedEventArgs.weapon.weaponDetails.weaponCurrentAmmo.ammoType;
            int ammoToRemove = inventoryData.GetAmmoCount(ammoType) - weaponReloadedEventArgs.totalRemainingAmmo;
            inventoryData.RemoveAmmoFromInventory(ammoType, ammoToRemove);
        }

        private void HandleInventoryModelUpdate(bool isInventoryOnlyChanged, Dictionary<StorageType, Dictionary<int, InventoryItem>> inventoryState)
        {
            // Updating weapon data
            if (isInventoryOnlyChanged != false && player != null)
            {
                Weapon currentWeapon = player.activeWeapon.GetCurrentWeapon();

                Weapon newWeapon = new Weapon();

                foreach (var item in inventoryState[StorageType.EquipedWeapon])
                {
                    if(currentWeapon != null)
                    {
                        if (item.Key == currentWeapon.weaponListPosition)
                        {
                            newWeapon = currentWeapon.GetWeapon(item.Value, item.Key);
                        }
                    }
                    else
                    {
                        var equipableItem = item.Value.item as EquippableItemSO;

                        if (equipableItem == null) continue;

                        newWeapon = newWeapon.GetWeapon(item.Value, item.Key);
                    }
                }

                player.setActiveWeaponEvent.CallSetActiveWeaponEvent(newWeapon);
            }

            UpdateInventoryUI(inventoryState);

            OnInventoryUpdated?.Invoke(new InventoryUpdatedEventArgs() { inventoryState = inventoryState });
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

        public int GetTotalAmmoByType(AmmoType ammotype)
        {
            return inventoryData.GetAmmoCount(ammotype);
        }

        public int GetCurrentWeaponTotalAmmo()
        {
            var currentWeapon = player.activeWeapon.GetCurrentWeapon();

            if(currentWeapon != null)
            {
                var currentWeaponAmmoType = currentWeapon.weaponDetails.weaponCurrentAmmo.ammoType;
                return GetTotalAmmoByType(currentWeaponAmmoType);
            }
            else
            {
                return 0;
            }
        }

        public Weapon GetWeaponByIndex(int index)
        {
            InventoryItem inventoryItem;
            bool isItemReturned = inventoryData.GetItemAt(StorageType.EquipedWeapon, index, out inventoryItem);

            if (isItemReturned)
            {
                Weapon weapon = new Weapon().GetWeapon(inventoryItem, index);
                return weapon;
            }

            return null;
        }

        public void PickUpItem(Item item)
        {
            // TODO: equip weapon first
            if (item != null)
            {
                // Play pickup sound effect
                SoundEffectManager.Instance?.PlaySoundEffect(item.soundEffect);

                // TODO: equip weapon first
                int reminder = inventoryData.AddItem(StorageType.Inventory, item.InventoryItem, item.Quantity);
                if (reminder == 0)
                    item.DestroyItem();
                else
                    item.Quantity = reminder;
            }
        }
    }

    public class InventoryUpdatedEventArgs : EventArgs
    {
        public Dictionary<StorageType, Dictionary<int, InventoryItem>> inventoryState;
    }
}