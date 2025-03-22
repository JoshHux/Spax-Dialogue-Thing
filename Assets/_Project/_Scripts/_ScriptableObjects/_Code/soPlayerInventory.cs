using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jackalope.Dialogue.Inventory
{
    [CreateAssetMenu(fileName = "Backpack", menuName = "DialogueStuff/Inventory/Backpack", order = 1)]

    public class soPlayerInventory : ScriptableObject
    {
        [SerializeField] private List<soInventoryItem> _items;

        public List<soInventoryItem> Items { get { return this._items; } }
    }
}