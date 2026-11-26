using UnityEngine;

// Поиск стола в сцене и его игровой поверхности (столешницы).
// Отдельный класс: определение стола не должно мешаться со спавном карт.
public class TableSurface
{
    private Bounds topBounds;
    private bool ready;

    private const float BoardEdgeMargin = 0.06f; // отступ доски от дальнего края столешницы, м
    private const float BoardHandGap = 0.05f;    // зазор между доской и дальним краем руки, м

    /// <summary>Границы столешницы (по ним считаются размер и раскладка карт).</summary>
    public Bounds TopBounds { get { return topBounds; } }

    /// <summary>Центр ряда карт: на поверхности стола, у ближнего края.</summary>
    public Vector3 CardsCenter { get; private set; }

    /// <summary>Стол найден и поверхность посчитана.</summary>
    public bool Ready { get { return ready; } }

    private TableSurface() { }

    // tableObjectName — корневой объект стола, surfaceObjectName — меш столешницы.
    // Возвращает null, если стол не найден.
    public static TableSurface Find(string tableObjectName, string surfaceObjectName)
    {
        var surface = new TableSurface();
        surface.Compute(tableObjectName, surfaceObjectName);
        return surface.Ready ? surface : null;
    }

    private void Compute(string tableObjectName, string surfaceObjectName)
    {
        GameObject table = GameObject.Find(tableObjectName);
        if (table == null)
        {
            Debug.LogWarning($"TableSurface: объект \"{tableObjectName}\" не найден в сцене.");
            return;
        }

        Renderer[] renderers = table.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            Debug.LogWarning($"TableSurface: у \"{tableObjectName}\" нет рендереров.");
            return;
        }

        // Поверхность: сначала меш, указанный по имени (у нашей модели столешница
        // называется "table"), иначе — самый крупный тонкий горизонтальный рендерер.
        // Общие bounds использовать нельзя: в префабе стола есть висячая лампа
        // и высокие детали, они завышают max.y.
        Transform surfaceTf = table.transform.Find(surfaceObjectName);
        Renderer surfaceRenderer = surfaceTf != null ? surfaceTf.GetComponentInChildren<Renderer>() : null;
        if (surfaceRenderer == null)
        {
            float bestArea = -1f;
            foreach (Renderer r in renderers)
            {
                if (r.bounds.size.y > 0.6f) continue; // высокие детали — не столешница
                float area = r.bounds.size.x * r.bounds.size.z;
                if (surfaceRenderer == null || area > bestArea)
                {
                    bestArea = area;
                    surfaceRenderer = r;
                }
            }
        }

        topBounds = surfaceRenderer != null ? surfaceRenderer.bounds : CombineAllBounds(renderers);

        CardsCenter = new Vector3(
            topBounds.center.x,
            topBounds.max.y,
            topBounds.min.z + topBounds.size.z * 0.12f); // чуть отступаем от кромки вглубь стола

        ready = true;
    }

    private static Bounds CombineAllBounds(Renderer[] renderers)
    {
        Bounds all = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            all.Encapsulate(renderers[i].bounds);
        return all;
    }

    // Сколько глубины за рукой остаётся под доску: от дальнего края руки
    // до отступа от дальнего края столешницы.
    public float GetBoardAvailableDepth(float handBackZ)
    {
        return topBounds.max.z - BoardEdgeMargin - handBackZ - BoardHandGap;
    }

    // Доска — область столешницы за рукой карт, куда ставят постройки.
    // По ширине — по центру стола, по глубине прижата к дальнему краю (с отступом).
    public Bounds GetBoardBounds(float width, float depth)
    {
        Vector3 center = new Vector3(
            topBounds.center.x,
            topBounds.max.y,
            topBounds.max.z - BoardEdgeMargin - depth * 0.5f);

        return new Bounds(center, new Vector3(width, 0f, depth)); // плоская область на поверхности стола
    }

    // Куда встать камере: карты — внизу кадра, взгляд уходит глубже в стол.
    public void GetCameraTarget(float pitch, float distanceScale, float lookAhead,
                                out Vector3 position, out Quaternion rotation)
    {
        Camera cam = Camera.main;
        if (!ready || cam == null)
        {
            // Нечего показать — камера остаётся где была
            position = cam != null ? cam.transform.position : Vector3.zero;
            rotation = cam != null ? cam.transform.rotation : Quaternion.identity;
            return;
        }

        // Смотрим со стороны игрока (от кромки вглубь стола), наклоняя камеру вниз
        Vector3 toTable = Vector3.ProjectOnPlane(topBounds.center - CardsCenter, Vector3.up).normalized;
        if (toTable.sqrMagnitude < 0.0001f) toTable = Vector3.forward;

        Vector3 viewDir = Quaternion.AngleAxis(pitch, Vector3.Cross(Vector3.up, toTable)) * toTable;
        rotation = Quaternion.LookRotation(viewDir, Vector3.up);

        // Смотрим не на сами карты, а глубже в стол — карты остаются внизу кадра,
        // камера при этом смещается вперёд, к кромке.
        Vector3 lookPoint = CardsCenter + toTable * lookAhead;

        float distance = Mathf.Clamp(topBounds.size.x * distanceScale, 1.2f, 4f);
        position = lookPoint - viewDir * distance;
    }
}
