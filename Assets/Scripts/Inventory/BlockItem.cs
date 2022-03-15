using System;
using UnityEngine;

namespace Inventory
{
    [CreateAssetMenu(fileName = "New Block Item", menuName = "Scriptable Objects/Inventory/Items/Block Item")]
    public class BlockItem : ItemObject
    {
        public byte blockId;

        private void Awake()
        {
            type = BlockType.Block;
        }
    }
}