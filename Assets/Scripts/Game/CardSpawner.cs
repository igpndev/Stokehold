using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Раскладывает стартовую руку карт-шаблонов у начала стола.
// Только оркестрация: поиск стола — TableSurface, создание карт — CardFactory,
// наведение — CardHover. Создаётся из кода (TableTransition), в сцену вешать не нужно.
public class CardSpawner : MonoBehaviour
{
    [Header("Стол")]
    [SerializeField] private string tableObjectName = "GameTable_PrefVarial";
    [SerializeField] private string surfaceObjectName = "table"; // меш столешницы (зелёное сукно)

    [Header("Точка обзора камеры")]
    [SerializeField] private float pitch = 50f;            // наклон камеры к столу, градусы
    [SerializeField] private float distanceScale = 0.85f;  // дистанция = ширина стола * этот множитель
    [SerializeField] private float lookAhead = 0.6f;       // насколько глубже в стол смотрит камера (карты уходят вниз кадра)

    [Header("Карты")]
    [SerializeField] private int cardCount = 4;
    [SerializeField] private float appearDuration = 0.3f;  // появление одной карты, сек
    [SerializeField] private float stagger = 0.08f;        // задержка между появлениями, сек

    [Header("Сетка стола")]
    [SerializeField] private int gridColumns = 8;          // клеток по ширине столешницы

    private TableSurface surface;
    private BoardGrid grid;
    private bool spawned;

    /// <summary>Стол найден — можно получать точку камеры и спавнить карты.</summary>
    public bool Ready { get { return surface != null; } }

    /// <summary>Центр ряда карт на столе.</summary>
    public Vector3 CardsCenter { get { return surface != null ? surface.CardsCenter : Vector3.zero; } }

    private void Awake()
    {
        surface = TableSurface.Find(tableObjectName, surfaceObjectName);
        if (surface == null)
            Debug.LogWarning($"CardSpawner: стол \"{tableObjectName}\" не найден — карты не появятся.");
    }

    // Куда встать камере, чтобы смотреть на начало стола.
    public void GetCameraTarget(out Vector3 position, out Quaternion rotation)
    {
        Camera cam = Camera.main;
        if (surface == null)
        {
            // Стола нет — камера остаётся где была
            position = cam != null ? cam.transform.position : Vector3.zero;
            rotation = cam != null ? cam.transform.rotation : Quaternion.identity;
            return;
        }

        surface.GetCameraTarget(pitch, distanceScale, lookAhead, out position, out rotation);
    }

    // Раскладываем руку карт. Вызывается после прилёта камеры (или сразу, если камеры нет).
    public void SpawnHand()
    {
        if (surface == null)
        {
            Debug.LogWarning("CardSpawner: стол не найден — карты не созданы.");
            return;
        }
        if (spawned) return;
        spawned = true;

        float cardWidth = Mathf.Clamp(surface.TopBounds.size.x / 16f, 0.1f, 1.5f); // вдвое мельче привычной
        float cardHeight = cardWidth * 1.4f;
        float spacing = cardWidth * 1.15f;
        float z = Mathf.Max(surface.TopBounds.min.z + cardHeight * 0.65f, CardsCenter.z);

        // Сетка стола + разметка + общая сессия перетаскивания
        grid = new BoardGrid(surface.TopBounds, gridColumns);
        var gridView = new GameObject("BoardGridVisual").AddComponent<BoardGridView>();
        gridView.transform.SetParent(transform, false);
        gridView.Build(grid);

        var session = CardDragSession.Ensure();
        session.Configure(grid, gridView, surface.TopBounds.max.y, transform);

        List<Transform> cards = new List<Transform>(cardCount);
        float startX = CardsCenter.x - (cardCount - 1) * spacing * 0.5f;
        for (int i = 0; i < cardCount; i++)
        {
            Vector3 position = new Vector3(startX + i * spacing, surface.TopBounds.max.y + 0.005f, z);
            GameObject card = CardFactory.CreateCard(position, new Vector2(cardWidth, cardHeight), transform);
            card.name = "CardTemplate" + (i + 1);

            // drag&drop: зажать карту — вести за курсором, отпустить на клетке — здание
            var drag = card.GetComponent<CardDrag>();
            drag.Init(session, card.GetComponent<CardHover>(), card.transform.Find("Visual"));

            cards.Add(card.transform);
        }

        if (appearDuration > 0f)
            StartCoroutine(AppearAnimation(cards));
    }

    // Карты "вырастают" из нулевого масштаба одна за другой.
    private IEnumerator AppearAnimation(List<Transform> cards)
    {
        Vector3[] fullScales = new Vector3[cards.Count];
        for (int i = 0; i < cards.Count; i++)
        {
            fullScales[i] = cards[i].localScale;
            cards[i].localScale = fullScales[i] * 0.001f;
        }

        for (int i = 0; i < cards.Count; i++)
        {
            yield return new WaitForSecondsRealtime(stagger);

            float t = 0f;
            while (t < appearDuration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / appearDuration));
                cards[i].localScale = Vector3.Lerp(fullScales[i] * 0.001f, fullScales[i], k);
                yield return null;
            }
            cards[i].localScale = fullScales[i];
        }
    }
}
