using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    public abstract class ItemSO : ScriptableObject
    {
        [field: SerializeField]
        public bool IsStackable { get; set; }

        public int ID => GetInstanceID();

        [field: SerializeField]
        public int MaxStackSize { get; set; } = 1;

        [field: SerializeField]
        public string Name { get; set; }

        [field: SerializeField]
        [field: TextArea]
        public string Description { get; set; }

        [field: SerializeField]
        public Sprite ItemImage { get; set; }

        [field: SerializeField]
        public List<ItemParameter> DefaultParametersList { get; set; }

        // Method for getting ItemParameter
        public ItemParameter GetParameter(ItemParameterType itemParameterType)
        {
            foreach (ItemParameter item in DefaultParametersList)
            {
                if (item.IsParameterTypeEquals(itemParameterType))
                {
                    return item;
                }
            }
            // if it is absent
            return new ItemParameter { itemParameter = null, value = 0 };
        }
    }

    [Serializable]
    public struct ItemParameter : IEquatable<ItemParameter>
    {
        public ItemParameterSO itemParameter;
        public float value;

        public bool Equals(ItemParameter other)
        {
            return other.itemParameter == itemParameter;
        }

        public bool IsParameterTypeEquals(ItemParameterType itemParameterType)
        {
            return itemParameterType == itemParameter.ParameterType;
        }

        public ItemParameter ChangeValue(float newValue)
        {
            return new ItemParameter
            {
                itemParameter = this.itemParameter,
                value = newValue
            };
        }
    }
}

