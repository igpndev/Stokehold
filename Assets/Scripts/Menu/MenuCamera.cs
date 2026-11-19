using UnityEngine;
using UnityEngine.InputSystem;

// Меню-поведение камеры: при старте перелетает к центру меню на столе,
// затем слегка поворачивается вслед за мышью. Саму механику перелёта
// выполняет CameraFlight (тот же компонент используется для перехода к столу).
public class MenuCamera : MonoBehaviour
{
    [Header("Куда смотреть")]
    [SerializeField] private RectTransform menuCanvas;
    [SerializeField] private float pitch = 55f;               // наклон к столу, градусы
    [SerializeField, Range(0f, 0.4f)] private float margin = 0.08f; // поля вокруг меню в кадре

    [Header("Перелёт")]
    [SerializeField] private float flyDuration = 1.5f;

    [Header("Покачивание за мышью")]
    [SerializeField] private float swayAngle = 1.5f;
    [SerializeField] private float swaySpeed = 3f;

    // Срабатывает автоматически после загрузки сцены: если в ней есть меню на столе,
    // а на камере нет наших компонентов — добавляем сами, руками ничего вешать не нужно.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AttachToMainCamera()
    {
        Camera main = Camera.main;
        if (main == null || main.GetComponent<MenuCamera>() != null) return;
        if (GameObject.Find("TableCanvas") == null) return;

        main.gameObject.AddComponent<CameraFlight>();
        main.gameObject.AddComponent<MenuCamera>();
    }

    private Camera cam;
    private CameraFlight flight;

    private void Start()
    {
        cam = GetComponent<Camera>();
        flight = GetComponent<CameraFlight>();

        // Если ссылку забыли назначить — ищем канвас меню по имени
        if (menuCanvas == null)
        {
            GameObject found = GameObject.Find("TableCanvas");
            if (found != null) menuCanvas = found.GetComponent<RectTransform>();
        }

        if (menuCanvas == null)
        {
            Debug.LogWarning("MenuCamera: не назначен Menu Canvas — камера останется на месте.");
            return;
        }

        if (flight == null) flight = gameObject.AddComponent<CameraFlight>();

        Vector3 targetPosition;
        Quaternion targetRotation;
        CalculateTarget(out targetPosition, out targetRotation);
        flight.FlyTo(targetPosition, targetRotation, flyDuration);
    }

    private void Update()
    {
        // Во время перелёта не мешаем — покачивание только на месте
        if (flight != null && flight.IsFlying) return;

        Sway(flight != null ? flight.TargetRotation : transform.rotation);
    }

    // Ставим камеру напротив центра меню и отодвигаем, пока все 4 угла канваса не влезут в кадр
    private void CalculateTarget(out Vector3 targetPosition, out Quaternion targetRotation)
    {
        // Запоминаем исходную позу — после подбора дистанции вернём камеру на место,
        // чтобы перелёт начался с неё (FlyTo фиксирует стартовую позицию сам).
        Vector3 originalPosition = transform.position;
        Quaternion originalRotation = transform.rotation;

        Vector3 forward = Vector3.ProjectOnPlane(menuCanvas.up, Vector3.up).normalized; // "верх" текста = от камеры
        Vector3 viewDir = Quaternion.AngleAxis(pitch, Vector3.Cross(Vector3.up, forward)) * forward;
        targetRotation = Quaternion.LookRotation(viewDir, Vector3.up);

        var corners = new Vector3[4];
        menuCanvas.GetWorldCorners(corners);
        Vector3 center = (corners[0] + corners[2]) * 0.5f;

        float distance = Vector3.Distance(corners[0], corners[2]) * 0.3f;
        transform.rotation = targetRotation;
        for (int i = 0; i < 300; i++)
        {
            transform.position = center - viewDir * distance;
            if (AllInView(corners)) break;
            distance *= 1.02f;
        }
        targetPosition = transform.position;

        transform.SetPositionAndRotation(originalPosition, originalRotation);
    }

    private bool AllInView(Vector3[] points)
    {
        foreach (Vector3 p in points)
        {
            Vector3 v = cam.WorldToViewportPoint(p);
            if (v.z <= 0f || v.x < margin || v.x > 1f - margin || v.y < margin || v.y > 1f - margin)
                return false;
        }
        return true;
    }

    private void Sway(Quaternion baseRotation)
    {
        if (Mouse.current == null) return;

        Vector2 mouse = Mouse.current.position.ReadValue();
        float x = Mathf.Clamp(mouse.x / Screen.width - 0.5f, -0.5f, 0.5f) * 2f;
        float y = Mathf.Clamp(mouse.y / Screen.height - 0.5f, -0.5f, 0.5f) * 2f;

        Quaternion target = baseRotation * Quaternion.Euler(-y * swayAngle, x * swayAngle, 0f);
        float t = 1f - Mathf.Exp(-swaySpeed * Time.unscaledDeltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, t);
    }
}
