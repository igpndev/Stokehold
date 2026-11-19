using UnityEngine;

// Переход из меню к столу: прячет панели меню, летит камерой к началу стола,
// по прилёте раскладывает стартовую руку карт.
public static class TableTransition
{
    public static void Run(GameObject mainPanel, GameObject settingsPanel, float flyDuration)
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        Camera cam = Camera.main;
        MenuCamera menuCamera = cam != null ? cam.GetComponent<MenuCamera>() : null;
        CameraFlight flight = cam != null ? cam.GetComponent<CameraFlight>() : null;

        CardSpawner spawner = new GameObject("TableCards").AddComponent<CardSpawner>();

        if (flight == null || !spawner.Ready)
        {
            Debug.LogWarning("TableTransition: не найдены камера с CameraFlight или стол — карты появятся без перелёта.");
            spawner.SpawnHand();
            return;
        }

        Vector3 targetPos;
        Quaternion targetRot;
        spawner.GetCameraTarget(out targetPos, out targetRot);
        flight.FlyTo(targetPos, targetRot, flyDuration, spawner.SpawnHand);
    }
}
