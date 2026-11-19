using UnityEngine;

// Настройки игры (громкость, полноэкранный режим): хранение в PlayerPrefs
// и применение к системе. UI-часть (слайдер, тумблер) живёт в MainMenu.
public static class SettingsService
{
    private const string VolumeKey = "settings.volume";
    private const string FullscreenKey = "settings.fullscreen";

    // Загрузка сразу применяет значение к системе
    public static float LoadVolume()
    {
        float volume = PlayerPrefs.GetFloat(VolumeKey, 1f);
        AudioListener.volume = volume;
        return volume;
    }

    public static bool LoadFullscreen()
    {
        bool fullscreen = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;
        Screen.fullScreen = fullscreen;
        return fullscreen;
    }

    // Сохранение сразу применяет значение к системе
    public static void SaveVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat(VolumeKey, volume);
    }

    public static void SaveFullscreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
        PlayerPrefs.SetInt(FullscreenKey, fullscreen ? 1 : 0);
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }
}
