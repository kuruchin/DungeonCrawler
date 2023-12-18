using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropOnDestroy : MonoBehaviour
{
    public DroppedItemsTableSO itemsTable;

    [SerializeField, Range(0, 100)]
    private int chanceToDropAmmo;

    IDestroyable destroyable;

    private void Start()
    {
        destroyable = GetComponent<IDestroyable>();

        if (destroyable != null)
        {
            destroyable.onDestroy += OnObjectDestroy;
        }
    }

    private void OnDisable()
    {
        if (destroyable != null)
        {
            destroyable.onDestroy -= OnObjectDestroy;
        }
    }


    private void OnObjectDestroy(OnDestroyArgs eventArgs)
    {
        if (chanceToDropAmmo == 0) return;

        // Random percent from 0 to 100
        int randomChance = Random.Range(0, 101); 

        if (randomChance <= chanceToDropAmmo)
        {
            var randomAmmoItem = itemsTable.GetRandomAmmo();

            if (randomAmmoItem != null)
            {
                Instantiate(randomAmmoItem, eventArgs.gameObject.transform.position, Quaternion.identity);
            }
        }
    }
}
