using System.Collections;
using UnityEngine;
using TMPro;
using Inventory.Model;
using Inventory;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(MaterializeEffect))]
public class Chest : MonoBehaviour, IPickable
{
    #region Tooltip
    [Tooltip("Set this to the colour to be used for the materialization effect")]
    #endregion Tooltip
    [ColorUsage(false, true)]
    [SerializeField] private Color materializeColor;
    #region Tooltip
    [Tooltip("Set this to the time is will take to materialize the chest")]
    #endregion Tooltip
    [SerializeField] private float materializeTime = 3f;
    #region Tooltip
    [Tooltip("Populate withItemSpawnPoint transform")]
    #endregion Tooltip
    [SerializeField] private Transform itemSpawnPoint;
    //private int healthPercent;

    private GameObject weaponItemGameobject;
    private Item weaponItem;

    private GameObject ammoItemGameobject;
    private Item ammoItem;

    private GameObject healthItemGameobject;
    private Item healthItem;

    //private int ammoPercent;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private MaterializeEffect materializeEffect;
    private bool isEnabled = false;
    private ChestState chestState = ChestState.closed;
    //private GameObject chestItemGameObject;

    //private ChestItem chestItem;

    private TextMeshPro messageTextTMP;


    private bool canBePickedUp = true;

    public bool CanBePickedUp()
    {
        return canBePickedUp;
    }

    public void SetCanBePickedUp(bool value)
    {
        canBePickedUp = value;
    }

    private void Awake()
    {
        //  Cache components
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        materializeEffect = GetComponent<MaterializeEffect>();
        messageTextTMP = GetComponentInChildren<TextMeshPro>();
    }

    /// <summary>
    /// Initialize Chest and either make it visible immediately or materialize it
    /// </summary>
    public void Initialize(bool shouldMaterialize, GameObject healthItemPrefab, GameObject weaponItemPrefab, GameObject ammoItemPrefab)
    {
        if (healthItemPrefab != null)
        {
            healthItemGameobject = Instantiate(healthItemPrefab, itemSpawnPoint.position, Quaternion.identity, this.transform);

            healthItem = healthItemGameobject.GetComponent<Item>();
            healthItem.SetCanBePickedUp(false);
            healthItemGameobject.SetActive(false);
        }

        if (ammoItemPrefab != null)
        {
            ammoItemGameobject = Instantiate(ammoItemPrefab, itemSpawnPoint.position, Quaternion.identity, this.transform);

            ammoItem = ammoItemGameobject.GetComponent<Item>();
            ammoItem.SetCanBePickedUp(false);
            ammoItemGameobject.SetActive(false);
        }

        if (weaponItemPrefab != null)
        {
            weaponItemGameobject = Instantiate(weaponItemPrefab, itemSpawnPoint.position,Quaternion.identity, this.transform);

            weaponItem = weaponItemGameobject.GetComponent<Item>();
            weaponItem.SetCanBePickedUp(false);
            weaponItemGameobject.SetActive(false);
        }


        if (shouldMaterialize)
        {
            StartCoroutine(MaterializeChest());
        }
        else
        {
            EnableChest();
        }
    }

    /// <summary>
    /// Materialise the chest
    /// </summary>
    private IEnumerator MaterializeChest()
    {
        SpriteRenderer[] spriteRendererArray = new SpriteRenderer[] { spriteRenderer };

        yield return StartCoroutine(materializeEffect.MaterializeRoutine(GameResources.Instance.materializeShader, materializeColor, materializeTime, spriteRendererArray, GameResources.Instance.litMaterial));

        EnableChest();
    }

    /// <summary>
    /// Enable the chest
    /// </summary>
    private void EnableChest()
    {
        // Set use to enabled
        isEnabled = true;
    }

    /// <summary>
    /// Use the chest - action will vary depending on the chest state
    /// </summary>
    public void PickUpItem()
    {
        if (!isEnabled) return;

        switch (chestState)
        {
            case ChestState.closed:
                OpenChest();
                break;

            case ChestState.healthItem:
                CollectHealthItem();
                break;

            case ChestState.ammoItem:
                CollectAmmoItem();
                break;

            case ChestState.weaponItem:
                CollectWeaponItem();
                break;

            case ChestState.empty:
                return;

            default:
                return;
        }
    }

    /// <summary>
    /// Open the chest on first use
    /// </summary>
    private void OpenChest()
    {
        animator.SetBool(Settings.use, true);

        // chest open sound effect
        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.chestOpen);

        UpdateChestState();
    }

    /// <summary>
    /// Create items based on what should be spawned and the chest state
    /// </summary>
    private void UpdateChestState()
    {
        if (healthItem != null)
        {
            chestState = ChestState.healthItem;
            healthItemGameobject.SetActive(true);
        }
        else if (ammoItem != null)
        {
            chestState = ChestState.ammoItem;
            ammoItemGameobject.SetActive(true);
        }
        else if (weaponItem != null)
        {
            chestState = ChestState.weaponItem;
            weaponItemGameobject.SetActive(true);
        }
        else
        {
            chestState = ChestState.empty;
        }
    }

    /// <summary>
    /// Collect the health item and add it to the players health
    /// </summary>
    private void CollectHealthItem()
    {
        if (healthItem != null)
        {
            healthItem.InventoryItem.ActionOnPickup(healthItem);
        }

        if (healthItem == null)
        {
            UpdateChestState();
        }

        //// Add health to player
        //GameManager.Instance.GetPlayer().health.AddHealth(healthPercent);

        //// Play pickup sound effect
        //SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.healthPickup);

        //healthPercent = 0;

        //Destroy(chestItemGameObject);

        //UpdateChestState();
    }

    /// <summary>
    /// Collect an ammo item and add it to the ammo in the players current weapon
    /// </summary>
    private void CollectAmmoItem()
    {
        if (ammoItem != null)
        {
            ammoItem.InventoryItem.ActionOnPickup(ammoItem);
        }

        if (ammoItem == null)
        {
            UpdateChestState();
        }
        //// Check item exists and has been materialized
        //if (chestItem == null || !chestItem.isItemMaterialized) return;

        //Player player = GameManager.Instance.GetPlayer();

        //// Update ammo for current weapon
        //player.reloadWeaponEvent.CallReloadWeaponEvent(player.activeWeapon.GetCurrentWeapon(), ammoPercent);

        //// Play pickup sound effect
        //SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.ammoPickup);

        //ammoPercent = 0;

        //Destroy(chestItemGameObject);

        //UpdateChestState();
    }

    /// <summary>
    /// Collect the weapon and add it to the players weapons list
    /// </summary>
    private void CollectWeaponItem()
    {
        if (weaponItem != null)
        {
            weaponItem.InventoryItem.ActionOnPickup(weaponItem);
        }

        if (weaponItem == null)
        {
            UpdateChestState();
        }
    }

    /// <summary>
    /// Display message above chest
    /// </summary>
    private IEnumerator DisplayMessage(string messageText, float messageDisplayTime)
    {
        messageTextTMP.text = messageText;

        yield return new WaitForSeconds(messageDisplayTime);

        messageTextTMP.text = "";
    }
}