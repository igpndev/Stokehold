using UnityEngine;
using UnityEngine.EventSystems;

// При наведении табличка "приподнимается" над столом и чуть увеличивается.
public class MenuButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
                               IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float liftHeight = 15f;   // в единицах канваса
    [SerializeField] private float hoverScale = 1.06f;
    [SerializeField] private float speed = 12f;

    private Vector3 basePosition;
    private Vector3 baseScale;
    private bool hovered;
    private bool pressed;

    private void Awake()
    {
        basePosition = transform.localPosition;
        baseScale = transform.localScale;
    }

    private void OnDisable()
    {
        hovered = pressed = false;
        transform.localPosition = basePosition;
        transform.localScale = baseScale;
    }

    private void Update()
    {
        // Лицевая сторона канваса смотрит вверх, поэтому "над столом" = локальная -Z
        float lift = pressed ? liftHeight * 0.3f : hovered ? liftHeight : 0f;
        float scale = hovered ? hoverScale : 1f;

        float t = 1f - Mathf.Exp(-speed * Time.unscaledDeltaTime);
        transform.localPosition = Vector3.Lerp(transform.localPosition, basePosition + Vector3.back * lift, t);
        transform.localScale = Vector3.Lerp(transform.localScale, baseScale * scale, t);
    }

    public void OnPointerEnter(PointerEventData eventData) => hovered = true;
    public void OnPointerExit(PointerEventData eventData) => hovered = pressed = false;
    public void OnPointerDown(PointerEventData eventData) => pressed = true;
    public void OnPointerUp(PointerEventData eventData) => pressed = false;
}
