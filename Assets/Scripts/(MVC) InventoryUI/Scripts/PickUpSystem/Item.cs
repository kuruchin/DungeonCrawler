using Inventory;
using Inventory.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour, IPickable
{
    [field: SerializeField]
    public ItemSO InventoryItem { get; private set; }

    [field: SerializeField]
    public int Quantity { get; set; } = 1;

    private bool canBePickedUp = true;

    [SerializeField]
    private AudioSource audioSource;

    public SoundEffectSO soundEffect;

    public float duration = 0.3f;


    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        GetComponent<SpriteRenderer>().sprite = InventoryItem.ItemImage;
    }

    public bool CanBePickedUp()
    {
        return canBePickedUp;
    }

    public void SetCanBePickedUp(bool value)
    {
        canBePickedUp = value;
    }

    public void PickUpItem()
    {
        //var player = GameManager.Instance.GetPlayer();

        // TODO: equip weapon first
        //int reminder = player.inventoryController.AddItem(StorageType.Inventory, item.InventoryItem, item.Quantity);
        //GameManager.Instance.GetPlayer().GetComponent<InventoryController>().PickUpItem(this);

        InventoryItem.ActionOnPickup(this);

        if (Quantity == 0)
        {
            DestroyItem();
        }
    }

    public void DestroyItem()
    {
        GetComponent<Collider2D>().enabled = false;
        StartCoroutine(AnimateItemPickup());
    }

    private IEnumerator AnimateItemPickup()
    {
        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.zero;
        float currentTime = 0;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            transform.localScale = 
                Vector3.Lerp(startScale, endScale, currentTime / duration);
            yield return null;
        }
        Destroy(gameObject);
    }
}
