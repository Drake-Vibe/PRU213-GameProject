using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{   
    Transform originalParent;
    CanvasGroup canvasGroup;
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent; // Set OG parent
        transform.SetParent(transform.root); // Above other canvas
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f; // semi-transparent while dragging
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position; // follow mouse position
        Debug.Log("Pointer Enter = " + eventData.pointerEnter?.name);

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        Slot originalSlot = originalParent.GetComponent<Slot>();

        Slot dropSlot = null;

        if (eventData.pointerEnter != null)
        {
            dropSlot = eventData.pointerEnter.GetComponentInParent<Slot>();
        }

        // Không thả vào slot nào
        if (dropSlot == null)
        {
            transform.SetParent(originalParent, false);
            GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            return;
        }

        // Thả vào chính slot hiện tại
        if (dropSlot == originalSlot)
        {
            transform.SetParent(originalParent, false);
            GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            return;
        }

        // Nếu slot đích có item thì swap
        if (dropSlot.currentItem != null)
        {
            GameObject targetItem = dropSlot.currentItem;

            targetItem.transform.SetParent(originalSlot.transform, false);
            targetItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            originalSlot.currentItem = targetItem;
        }
        else
        {
            originalSlot.currentItem = null;
        }

        // Chuyển item đang kéo sang slot mới
        transform.SetParent(dropSlot.transform, false);

        RectTransform rect = GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;

        dropSlot.currentItem = gameObject;
    }
}

