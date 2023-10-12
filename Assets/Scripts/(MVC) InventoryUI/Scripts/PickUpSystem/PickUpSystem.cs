using Inventory.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpSystem : MonoBehaviour
{
    [SerializeField]
    private InventorySO inventoryData;

    private Player player;

    private void Awake()
    {
        // Load components
        player = GetComponent<Player>();
    }

    private void Update()
    {
        // Process player use item input
        UseItemInput();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Item item = collision.GetComponent<Item>();
        //if (item != null)
        //{
        //    int reminder = inventoryData.AddItem(item.InventoryItem, item.Quantity);
        //    if (reminder == 0)
        //        item.DestroyItem();
        //    else
        //        item.Quantity = reminder;
        //}
    }

    /// <summary>
    /// Use the nearest item within 2 unity units from the player
    /// </summary>
    private void UseItemInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            float useItemRadius = 2f;

            // Get any 'Useable' item near the player
            Collider2D[] collider2DArray = Physics2D.OverlapCircleAll(player.GetPlayerPosition(), useItemRadius);

            // Loop through detected items to see if any are 'useable'
            foreach (Collider2D collider2D in collider2DArray)
            {
                Item item = collider2D.GetComponent<Item>();
                if (item != null)
                {
                    // TODO: equip weapon first
                    int reminder = inventoryData.AddItem(StorageType.Inventory, item.InventoryItem, item.Quantity);
                    if (reminder == 0)
                        item.DestroyItem();
                    else
                        item.Quantity = reminder;
                }
            }
        }
    }
}
