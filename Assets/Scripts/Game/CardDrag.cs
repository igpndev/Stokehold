using UnityEngine;
using UnityEngine.EventSystems;

// Перетаскивание одной карты: старт по зажатию курсора на карте.
// Само ведение и сброс выполняет общая сессия CardDragSession
// (курсор может уйти с коллайдера карты, поэтому события точечные).
public class CardDrag : MonoBehaviour, IPointerDownHandler
{
    private const float DragLift = 0.04f; // на сколько карта висит над столом при переносе

    private CardDragSession session;
    private CardHover hover;
    private Transform visual;

    public void Init(CardDragSession session, CardHover hover, Transform visual)
    {
        this.session = session;
        this.hover = hover;
        this.visual = visual;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (session != null) session.Begin(this);
    }

    // Вызывается сессией каждый кадр перетаскивания
    public void SetDragPosition(Vector3 tablePoint)
    {
        if (visual != null) visual.position = tablePoint + Vector3.up * DragLift;
    }

    // Перетаскивание началось: ховер замирает, чтобы не тянул карту обратно
    public void BeginDrag()
    {
        if (hover != null) hover.SetSuppressed(true);
    }

    // Промах мимо клетки: ховер сам плавно вернёт карту на её место
    public void EndDrag()
    {
        if (hover != null) hover.SetSuppressed(false);
    }
}
