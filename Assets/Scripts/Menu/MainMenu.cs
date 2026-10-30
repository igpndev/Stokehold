using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Логика главного меню: кнопки "Играть", "Настройки", "Выход" и панель настроек.
// Сами кнопки лежат на столе (World Space Canvas) в сцене MainScene.
public class MainMenu : MonoBehaviour
{
    [Header("Сцена игры (должна быть в File > Build Profiles)")]
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("Панели")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Элементы настроек")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle fullscreenToggle;

    private const string VolumeKey = "settings.volume";
    private const string FullscreenKey = "settings.fullscreen";

    private void Start()
    {
        float volume = PlayerPrefs.GetFloat(VolumeKey, 1f);
        bool fullscreen = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;

        AudioListener.volume = volume;
        Screen.fullScreen = fullscreen;

        if (volumeSlider != null)
        {
            volumeSlider.SetValueWithoutNotify(volume);
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
        if (fullscreenToggle != null)
        {
            fullscreenToggle.SetIsOnWithoutNotify(fullscreen);
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
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
        if (Application.CanStreamedLevelBeLoaded(gameSceneName))
            SceneManager.LoadScene(gameSceneName);
        else
            Debug.LogWarning($"Сцена \"{gameSceneName}\" не найдена в Build Profiles. " +
                             "Создайте её и добавьте в список сцен (или поменяйте имя в компоненте MainMenu).");
    }

    public void OpenSettings()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        PlayerPrefs.Save();
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

    private void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat(VolumeKey, value);
    }

    private void OnFullscreenChanged(bool value)
    {
        Screen.fullScreen = value;
        PlayerPrefs.SetInt(FullscreenKey, value ? 1 : 0);
    }
}
