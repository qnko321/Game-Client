using UnityEngine;
using UnityEngine.InputSystem;

namespace Inventory
{
    public class InventoryMenu : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform hotBarHolder;
        [SerializeField] private Transform pocketsHolder;
        [SerializeField] private Transform packHolder;
        [SerializeField] private GameObject nameHolder;

        [SerializeField] private InventoryObject inventoryObject;
        [SerializeField] private ItemDatabase itemDB;
        private int SlotCount => hotBarHolder.childCount + pocketsHolder.childCount + packHolder.childCount;
        public int selectedSlotId = 0;
        
        private Slot[] slots;
        private Slot selectedSlot;

        private bool isInventoryOpen = true;

        private void Awake()
        {
            LoadSlots();
            SelectSlot(selectedSlotId);
        }

        private void LoadSlots()
        {
            slots = new Slot[SlotCount];
            int _slotIndex = 0;
            for (int _i = 0; _i < hotBarHolder.childCount; _i++)
            {
                slots[_slotIndex] = hotBarHolder.GetChild(_i).GetComponent<Slot>();
                _slotIndex++;
            }
            for (int _i = 0; _i < pocketsHolder.childCount; _i++)
            {
                slots[_slotIndex] = pocketsHolder.GetChild(_i).GetComponent<Slot>();
                _slotIndex++;
            }
            for (int _i = 0; _i < hotBarHolder.childCount; _i++)
            {
                slots[_slotIndex] = packHolder.GetChild(_i).GetComponent<Slot>();
                _slotIndex++;
            }
        }

        private void SelectSlot(int _i)
        {
            if (selectedSlot != null) selectedSlot.Deselect();
            slots[_i].Select();
            selectedSlot = slots[_i];
            selectedSlotId = _i;
        }
        
        public void UpdateSelectedSlot()
        {
            InventorySlot _invSlot = inventoryObject.slots[selectedSlotId];
            if (_invSlot.id < 1)
                slots[selectedSlotId].Clear();
            else
                slots[selectedSlotId].SetItem(itemDB.GetItem[_invSlot.id].sprite, _invSlot.amount);
        }

        public void UpdateSlot(int _updatedSlot)
        {
            InventorySlot _invSlot = inventoryObject.slots[_updatedSlot];
            if (_invSlot.id < 1)
                slots[_updatedSlot].Clear();
            else
                slots[_updatedSlot].SetItem(itemDB.GetItem[_invSlot.id].sprite, _invSlot.amount);
        }
        
        public void UpdateSlots(int[] _updatedSlots)
        {
            foreach (int _updatedSlot in _updatedSlots)
            {
                InventorySlot _invSlot = inventoryObject.slots[_updatedSlot];
                slots[_updatedSlot].SetItem(itemDB.GetItem[_invSlot.id].sprite, _invSlot.amount);
            }
        }

        public void SwapSlots(int _fromSlot, int _toSlot)
        {
            inventoryObject.SwapSlots(_fromSlot, _toSlot, CheckMaxStack);
            UpdateSlot(_fromSlot);
            UpdateSlot(_toSlot);
        }

        public int CheckMaxStack(int _itemId)
        {
            if (_itemId < 1)
                return 0;
            return itemDB.GetItem[_itemId].maxStack;
        }

        public void ToggleInventory(InputAction.CallbackContext _ctx)
        {
            if (!_ctx.started) return;
            
            if (isInventoryOpen)
                Close();
            else
                Open();
        }

        private void Open()
        {
            nameHolder.SetActive(true);
            pocketsHolder.gameObject.SetActive(true);
            packHolder.gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            GameManager.instance.localPlayer.cameraController.allowBlockModification = false;
            isInventoryOpen = true;
        }

        private void Close()
        {
            nameHolder.SetActive(false);
            pocketsHolder.gameObject.SetActive(false);
            packHolder.gameObject.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            GameManager.instance.localPlayer.cameraController.allowBlockModification = true;
            isInventoryOpen = false;
        }
    }
}