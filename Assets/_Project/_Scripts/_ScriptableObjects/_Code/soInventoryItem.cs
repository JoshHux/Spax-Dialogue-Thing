using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Jackalope.Dialogue.Inventory
{
    [CreateAssetMenu(fileName = "InventoryItem", menuName = "DialogueStuff/Inventory/Item", order = 1)]
    public class soInventoryItem : ScriptableObject
    {
        [SerializeField] private string _itemName;
        [TextArea(15, 20)]
        [SerializeField] private string _description;
        [SerializeField] private Sprite _icon;
        [SerializeField] private Sprite _picture;

        public string ItemName { get { return this._itemName; } }
        public string Description { get { return this._description; } }
        public Sprite Icon { get { return this._icon; } }
        public Sprite Picture { get { return this._picture; } }
    }

}
