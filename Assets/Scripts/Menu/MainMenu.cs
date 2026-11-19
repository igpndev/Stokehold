using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Логика главного меню: кнопки "Играть", "Настройки", "Выход" и панель настроек.
// Сами кнопки лежат на столе (World Space Canvas) в сцене MainScene.
// Хранение настроек — SettingsService, переход к столу — TableTransition.
public class MainMenu : MonoBehaviour
{
    [Header("Сцена игры (должна быть в File > Build Profiles)")]
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("Переход к столу (пока сцены игры нет)")]
    [SerializeField] private float tableFlyDuration = 1.6f;

    [Header("Панели")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Элементы настроек")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle fullscreenToggle;

    private void Start()
    {
        // Загрузка настроек применяет их к системе и возвращает значения для UI
        float volume = SettingsService.LoadVolume();
        bool fullscreen = SettingsService.LoadFullscreen();

        if (volumeSlider != null)
        {
            volumeSlider.SetValueWithoutNotify(volume);
            volumeSlider.onValueChanged.AddListener(SettingsService.SaveVolume);
        }
        if (fullscreenToggle != null)
        {
            fullscreenToggle.SetIsOnWithoutNotify(fullscreen);
            fullscreenToggle.onValueChanged.AddListener(SettingsService.SaveFullscreen);
        }

        CloseSettings();
    }

    private void Update()
    {
        // Esc в настройках — вернуться назад
        if (settingsPanel != null && settingsPanel.activeSelf &&
            Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseSettings();
        }
    }

    public void Play()
    {
        // Когда сцена игры появится — играем её как раньше.
        if (Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
            return;
        }

        // Пока сцены игры нет: переход внутри этой сцены —
        // прячем меню, камера летит к началу стола, там появляются карты.
        TableTransition.Run(mainPanel, settingsPanel, tableFlyDuration);
    }

    public void OpenSettings()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        SettingsService.Save();
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
