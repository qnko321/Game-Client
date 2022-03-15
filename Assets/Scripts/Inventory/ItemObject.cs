using UnityEngine;

namespace Inventory
{
    public abstract class ItemObject : ScriptableObject
    {
        [HideInInspector] public BlockType type;
        public int id;
        public string itemName;
        public string description;
        public Sprite sprite;
        public int maxStack;
    }
}