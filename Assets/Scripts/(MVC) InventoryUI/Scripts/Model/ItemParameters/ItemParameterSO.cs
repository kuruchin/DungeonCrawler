using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu(fileName = "ItemParameter_", menuName = "Inventory/ItemParameter")]
    public class ItemParameterSO : ScriptableObject
    {
        [field: SerializeField]
        public string ParameterName { get; private set; }

        [field: SerializeField]
        public ItemParameterType ParameterType { get; private set; }

    }
}