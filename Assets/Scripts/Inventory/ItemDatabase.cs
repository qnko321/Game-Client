using System.Collections.Generic;
using UnityEngine;

namespace Inventory
{
    public class ItemDatabase : MonoBehaviour
    {
        public readonly Dictionary<int, ItemObject> GetItem = new Dictionary<int, ItemObject>();
        public readonly Dictionary<int, ItemObject> GetItemByBlockId = new Dictionary<int, ItemObject>();
        [SerializeField] private List<ItemObject> items = new List<ItemObject>();
        
        private void Awake()
        {
            foreach (var _item in items)
            {
                GetItem.Add(_item.id, _item);
                if (_item.type == BlockType.Block)
                {
                    GetItemByBlockId.Add(((BlockItem)_item).blockId, _item);
                }
            }
        }
    }
}