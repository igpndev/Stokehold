using UnityEngine;
using UnityEngine.EventSystems;

// Наведение на карту: карта выдвигается вперёд (вглубь стола) и приподнимается.
// Двигается только визуальная часть карты; коллайдер остаётся на базовой позиции —
// иначе при выдвижении курсор у края карты "теряет" её (exit -> возврат -> enter)
// и карта дёргается.
public class CardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
                              IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float slideDistance = 0.10f; // выдвижение вперёд, вглубь стола, м
    [SerializeField] private float liftHeight = 0.03f;    // подъём над столом, м
    [SerializeField] private float speed = 10f;

    private Transform visual; // движущаяся часть карты (квад без коллайдера)
    private Vector3 baseVisualPosition;
    private Vector3 hoverOffset;
    private bool hovered;
    private bool pressed;
    private bool initialized;
    private bool suppressed; // истина во время перетаскивания — ховер не трогает карту

    // Базовая позиция известна только после раскладки — Init вызывает фабрика.
    public void Init(Vector3 basePos, Transform visualTransform)
    {
        visual = visualTransform;
        baseVisualPosition = basePos;
        hoverOffset = new Vector3(0f, liftHeight, slideDistance); // вперёд = вглубь стола, от игрока
        initialized = true;
    }

    // На время drag&drop карта целиком в ведении CardDragSession
    public void SetSuppressed(bool value)
    {
        suppressed = value;
    }

    private void OnDisable()
    {
        ResetPose();
    }

    private void Update()
    {
        if (suppressed || !initialized || visual == null) return;

        Vector3 target = baseVisualPosition + (pressed ? hoverOffset * 0.4f : hovered ? hoverOffset : Vector3.zero);
        float t = 1f - Mathf.Exp(-speed * Time.unscaledDeltaTime);
        visual.position = Vector3.Lerp(visual.position, target, t);
    }

    private void ResetPose()
    {
        hovered = pressed = false;
        if (!suppressed && initialized && visual != null) visual.position = baseVisualPosition;
    }

    public void OnPointerEnter(PointerEventData eventData) => hovered = true;

    public void OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
        pressed = false;
    }

    public void OnPointerDown(PointerEventData eventData) => pressed = true;

    public void OnPointerUp(PointerEventData eventData) => pressed = false;
}
