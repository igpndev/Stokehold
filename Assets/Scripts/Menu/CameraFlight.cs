using System;
using UnityEngine;

// Механика плавного перелёта камеры между точками, без привязки к меню.
// Используется и для стартового перелёта к меню, и для перехода к столу.
public class CameraFlight : MonoBehaviour
{
    private Vector3 startPosition, targetPosition;
    private Quaternion startRotation, targetRotation;
    private float flyTime;
    private float flyDurationCurrent = 0.01f;
    private bool flying;
    private Action arrivalCallback;

    /// <summary>Идёт ли сейчас перелёт.</summary>
    public bool IsFlying { get { return flying; } }

    /// <summary>Куда камера летит или где должна остановиться (база для покачивания).</summary>
    public Vector3 TargetPosition { get; private set; }

    /// <summary>Целевой поворот камеры (база для покачивания за мышью).</summary>
    public Quaternion TargetRotation { get; private set; }

    private void Awake()
    {
        TargetPosition = transform.position;
        TargetRotation = transform.rotation;
    }

    private void Update()
    {
        if (!flying) return;

        flyTime += Time.unscaledDeltaTime;
        float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(flyTime / flyDurationCurrent));
        transform.position = Vector3.Lerp(startPosition, TargetPosition, t);
        transform.rotation = Quaternion.Slerp(startRotation, TargetRotation, t);

        if (flyTime >= flyDurationCurrent)
        {
            flying = false;
            Action onArrive = arrivalCallback;
            arrivalCallback = null;
            if (onArrive != null) onArrive();
        }
    }

    // Плавный перелёт к точке. onArrive вызывается один раз по достижении цели.
    public void FlyTo(Vector3 position, Quaternion rotation, float duration, Action onArrive = null)
    {
        TargetPosition = position;
        TargetRotation = rotation;
        flyDurationCurrent = Mathf.Max(duration, 0.01f);
        arrivalCallback = onArrive;

        startPosition = transform.position;
        startRotation = transform.rotation;
        flyTime = 0f;
        flying = true;
    }
}
