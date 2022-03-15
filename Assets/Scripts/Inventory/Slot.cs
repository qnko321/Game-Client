using System.Text.RegularExpressions;
using Helpers;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot : MonoBehaviour, IDragHandler, IDropHandler
{
    [SerializeField] private Image slotImage;
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text itemAmountText;

    public void SetItem(Sprite _sprite, int _itemAmount)
    {
        itemImage.color = new Color(1, 1, 1, 1);
        itemImage.sprite = _sprite;
        itemAmountText.text = _itemAmount.ToString();
    }

    public void Clear()
    {
        itemImage.sprite = null;
        itemImage.color = new Color(1, 1, 1, 0);
        itemAmountText.text = "";
    }

    public void Select()
    {
        slotImage.color = new Color(.8f, .8f, .8f, 1f);
    }

    public void Deselect()
    {
        slotImage.color = Color.white;
    }

    public void OnDrop(PointerEventData _eventData)
    {
        //The item was dropped on this one
        int _fromSlot = _eventData.pointerDrag.name.GetDigits();
        int _toSlot = name.GetDigits();
        UIManager.instance.inventoryMenu.SwapSlots(_fromSlot, _toSlot);
    }

    public void OnDrag(PointerEventData _eventData) { }
}
