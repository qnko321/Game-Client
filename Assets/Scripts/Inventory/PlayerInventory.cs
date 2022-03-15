using System;
using Managers;
using TMPro.Examples;
using UnityEngine;

namespace Inventory
{
    public class PlayerInventory : MonoBehaviour
    {
        [SerializeField] private Player player;
        private InventoryObject inventory;
        public ItemDatabase itemDB;
        public InventoryMenu inventoryMenu;

        private void Awake()
        {
            inventory = player.inventoryObject;
            itemDB = GameManager.instance.gameObject.GetComponent<ItemDatabase>();
            inventoryMenu = UIManager.instance.transform.GetChild(2).GetComponent<InventoryMenu>();
        }

        public void PickUpItem(int _id, int _amount)
        {
            ItemObject _item = itemDB.GetItem[_id];
            (int[] _updatedSlots, int _leftOverAmount) = inventory.AddItem(_item.id, _amount, _item.maxStack);
            inventoryMenu.UpdateSlots(_updatedSlots);
        }

        public void PickUpItemByBlockId(byte _blockId, int _amount)
        {
            ItemObject _item = itemDB.GetItemByBlockId[_blockId];
            (int[] _updatedSlots, int _leftOverAmount) = inventory.AddItem(_item.id, _amount, _item.maxStack);
            inventoryMenu.UpdateSlots(_updatedSlots);
        }

        public bool IsBlockSelected()
        {
            int _itemId = inventory.slots[inventoryMenu.selectedSlotId].id;
            if (_itemId > 0)
                return itemDB.GetItem[_itemId].type == BlockType.Block;
            return false;
        }

        public void PlaceBlock()
        {
            inventory.RemoveItemsFromSlot(inventoryMenu.selectedSlotId, 1);
            inventoryMenu.UpdateSelectedSlot();
        }

        public byte GetBlockId()
        {
            return ((BlockItem) itemDB.GetItem[inventory.slots[inventoryMenu.selectedSlotId].id]).blockId;
        }

        public void UndoPlaceBlock()
        {
            inventory.AddItemsFromSlot(inventoryMenu.selectedSlotId, 1, inventoryMenu.CheckMaxStack);
            inventoryMenu.UpdateSelectedSlot();
        }
    }
}