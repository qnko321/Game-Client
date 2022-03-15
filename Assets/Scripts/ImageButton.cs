using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImageButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public bool Interactable
    {
        get => _interactable;
        set 
        { 
            _interactable = value;
            UpdateColor();
        }
    }
    
    [SerializeField] private Color normalColor;
    [SerializeField] private Color hoverColor;
    [SerializeField] private Color clickColor;
    [SerializeField] private Color disableColor;

    [Header("Events")] 
    [SerializeField] private UnityEvent onClick;
    
    private bool _interactable = true;
    private bool _hovered;
    private bool _clicked;

    private Image _image;

    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    public void OnPointerEnter(PointerEventData _eventData)
    {
        if (!_interactable) return;
        
        _hovered = true;
        UpdateColor();
    }

    public void OnPointerExit(PointerEventData _eventData)
    {
        if (!_interactable) return;

        _hovered = false;
        UpdateColor();
    }
    
    public void OnPointerDown(PointerEventData _eventData)
    {
        if (!_interactable) return;

        _clicked = true;
        UpdateColor();
        onClick.Invoke();
    }

    public void OnPointerUp(PointerEventData _eventData)
    {
        if (!_interactable) return;

        _clicked = false;
        UpdateColor();
    }

    private void UpdateColor()
    {
        if (_interactable)
        {
            if (_hovered)
            {
                if (_clicked)
                {
                    _image.color = clickColor;
                }
                else
                {
                    _image.color = hoverColor;
                }
            }
            else
            {
                _image.color = normalColor;
            }
        }
        else
        {
            _image.color = disableColor;
        }
    }
}
