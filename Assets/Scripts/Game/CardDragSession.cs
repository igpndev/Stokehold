using UnityEngine;

// Сессия перетаскивания карты: ведёт карту за курсором по плоскости стола,
// показывает разметку сетки, по отпусканию кнопки ставит здание-заглушку
// в клетку (или возвращает карту на место, если клетка занята / промах).
// Единственный экземпляр создаётся лениво при первом перетаскивании.
public class CardDragSession : MonoBehaviour
{
    public static CardDragSession Instance { get; private set; }

    public static CardDragSession Ensure()
    {
        if (Instance == null)
            Instance = new GameObject("CardDragSession").AddComponent<CardDragSession>();
        return Instance;
    }

    private CardDrag card;            // карта в руке (null — ничего не тащим)
    private BoardGrid grid;
    private BoardGridView gridView;
    private Transform buildingParent;
    private Plane tablePlane;
    private Camera cam;
    private Vector3 lastPoint;        // последняя точка курсора на плоскости стола
    private bool dragging;

    // Настройка один раз при раскладке карт
    public void Configure(BoardGrid boardGrid, BoardGridView boardView, float feltTopY, Transform buildingsParent)
    {
        grid = boardGrid;
        gridView = boardView;
        buildingParent = buildingsParent;
        tablePlane = new Plane(Vector3.up, new Vector3(0f, feltTopY, 0f));
    }

    public void Begin(CardDrag draggedCard)
    {
        if (dragging && card != null) card.EndDrag(); // прервать предыдущую карту

        card = draggedCard;
        card.BeginDrag();

        cam = Camera.main;
        dragging = true;
        lastPoint = card.transform.position; // до первого кадра ведения — не прыгать по столу
        if (gridView != null) gridView.SetVisible(true);
    }

    private void Update()
    {
        if (!dragging || card == null) return;

        var mouse = UnityEngine.InputSystem.Mouse.current;
        if (mouse != null && cam != null)
        {
            Ray ray = cam.ScreenPointToRay(mouse.position.ReadValue());
            if (tablePlane.Raycast(ray, out float enter))
                lastPoint = ray.GetPoint(enter);

            card.SetDragPosition(lastPoint);

            // Отпустил кнопку — сброс карты
            if (mouse.leftButton.wasReleasedThisFrame)
            {
                FinishDrag();
                return;
            }
        }
    }

    private void FinishDrag()
    {
        if (grid != null && grid.TryGetCell(lastPoint, out Vector2Int index) && !grid.IsOccupied(index))
        {
            grid.Occupy(index);
            grid.SpawnBuilding(index, buildingParent);
            Destroy(card.gameObject); // карта "сыграна"
        }
        else
        {
            card.EndDrag(); // промах — карта вернётся на своё место
        }

        if (gridView != null) gridView.SetVisible(false);
        dragging = false;
        card = null;
    }
}
