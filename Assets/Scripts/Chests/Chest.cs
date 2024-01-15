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
    [Tooltip("Color for the materialization effect")]
    [ColorUsage(false, true)]
    [SerializeField] private Color materializeColor;

    [Tooltip("Time it takes to materialize the chest")]
    [SerializeField] private float materializeTime = 3f;

    [Tooltip("Transform to populate with item spawn point")]
    [SerializeField] private Transform itemSpawnPoint;

    private GameObject weaponItemGameObject, ammoItemGameObject, healthItemGameObject;
    private Item weaponItem, ammoItem, healthItem;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private MaterializeEffect materializeEffect;
    private bool isEnabled = false;
    private ChestState chestState = ChestState.closed;
    private TextMeshPro messageTextTMP;

    private bool canBePickedUp = true;

    public bool CanBePickedUp() => canBePickedUp;

    public void SetCanBePickedUp(bool value) => canBePickedUp = value;

    private void Awake()
    {
        // Cache components
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        materializeEffect = GetComponent<MaterializeEffect>();
        messageTextTMP = GetComponentInChildren<TextMeshPro>();
    }

    public void Initialize(bool shouldMaterialize, GameObject healthItemPrefab, GameObject weaponItemPrefab, GameObject ammoItemPrefab)
    {
        InitializeItem(healthItemPrefab, ref healthItem, ref healthItemGameObject);
        InitializeItem(ammoItemPrefab, ref ammoItem, ref ammoItemGameObject);
        InitializeItem(weaponItemPrefab, ref weaponItem, ref weaponItemGameObject);

        if (shouldMaterialize)
            StartCoroutine(MaterializeChest());
        else
            EnableChest();
    }

    private void InitializeItem(GameObject prefab, ref Item item, ref GameObject itemGameObject)
    {
        if (prefab != null)
        {
            itemGameObject = Instantiate(prefab, itemSpawnPoint.position, Quaternion.identity, transform);
            item = itemGameObject.GetComponent<Item>();
            item.SetCanBePickedUp(false);
            itemGameObject.SetActive(false);
        }
    }

    private IEnumerator MaterializeChest()
    {
        SpriteRenderer[] spriteRendererArray = { spriteRenderer };
        yield return StartCoroutine(materializeEffect.MaterializeRoutine(GameResources.Instance.materializeShader, materializeColor, materializeTime, spriteRendererArray, GameResources.Instance.litMaterial));
        EnableChest();
    }

    private void EnableChest() => isEnabled = true;

    public void PickUpItem()
    {
        if (!isEnabled) return;

        switch (chestState)
        {
            case ChestState.closed:
                OpenChest();
                break;

            case ChestState.healthItem:
                CollectItem(healthItem);
                break;

            case ChestState.ammoItem:
                CollectItem(ammoItem);
                break;

            case ChestState.weaponItem:
                CollectItem(weaponItem);
                break;

            case ChestState.empty:
                return;

            default:
                return;
        }
    }

    private void OpenChest()
    {
        animator.SetBool(Settings.use, true);
        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.chestOpen);

        // Added delay 0.5s after opening the chest
        StartCoroutine(DelayCoroutine(0.5f, () =>
        {
            UpdateChestState();
        }));
    }

    private void UpdateChestState()
    {
        if (healthItem != null)
        {
            chestState = ChestState.healthItem;
            healthItemGameObject.SetActive(true);
        }
        else if (ammoItem != null)
        {
            chestState = ChestState.ammoItem;
            ammoItemGameObject.SetActive(true);
        }
        else if (weaponItem != null)
        {
            chestState = ChestState.weaponItem;
            weaponItemGameObject.SetActive(true);
        }
        else
        {
            chestState = ChestState.empty;
        }
    }

    private void CollectItem(Item item)
    {
        if (item != null)
        {
            float delay = item.duration + 0.1f;

            item.InventoryItem.ActionOnPickup(item);

            // Waiting for demoterialize effect
            StartCoroutine(DelayCoroutine(delay, () =>
            {
                if (item == null) UpdateChestState();
            }));
        }
    }


    private IEnumerator DelayCoroutine(float delayTime, System.Action onComplete)
    {
        yield return new WaitForSeconds(delayTime);

        // Call the passed action after the delay ends
        if (onComplete != null) onComplete.Invoke();
    }

    private IEnumerator DisplayMessage(string messageText, float messageDisplayTime)
    {
        messageTextTMP.text = messageText;
        yield return new WaitForSeconds(messageDisplayTime);
        messageTextTMP.text = "";
    }
}