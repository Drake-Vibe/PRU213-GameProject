using UnityEngine;

/// <summary>
/// Persistent settings data. Saved/loaded via PlayerPrefs.
/// Contains audio volumes and key bindings.
/// </summary>
[System.Serializable]
public class SettingsData
{
    [Header("Audio")]
    public float masterVolume = 1f;
    public float musicVolume = 0.8f;
    public float sfxVolume = 1f;

    [Header("Key Bindings")]
    public KeyCode moveUp = KeyCode.W;
    public KeyCode moveDown = KeyCode.S;
    public KeyCode moveLeft = KeyCode.A;
    public KeyCode moveRight = KeyCode.D;
    public KeyCode attack = KeyCode.Mouse0;
    public KeyCode pickupWeapon = KeyCode.Q;
    public KeyCode openMenu = KeyCode.Tab;
    public KeyCode openMenuAlt = KeyCode.Escape;
    public KeyCode interact = KeyCode.E;

    /// <summary>
    /// Save settings to PlayerPrefs.
    /// </summary>
    public void Save()
    {
        PlayerPrefs.SetFloat("masterVolume", masterVolume);
        PlayerPrefs.SetFloat("musicVolume", musicVolume);
        PlayerPrefs.SetFloat("sfxVolume", sfxVolume);

        PlayerPrefs.SetInt("key_moveUp", (int)moveUp);
        PlayerPrefs.SetInt("key_moveDown", (int)moveDown);
        PlayerPrefs.SetInt("key_moveLeft", (int)moveLeft);
        PlayerPrefs.SetInt("key_moveRight", (int)moveRight);
        PlayerPrefs.SetInt("key_attack", (int)attack);
        PlayerPrefs.SetInt("key_pickupWeapon", (int)pickupWeapon);
        PlayerPrefs.SetInt("key_openMenu", (int)openMenu);
        PlayerPrefs.SetInt("key_openMenuAlt", (int)openMenuAlt);
        PlayerPrefs.SetInt("key_interact", (int)interact);

        PlayerPrefs.Save();
        Debug.Log("Settings saved!");
    }

    /// <summary>
    /// Load settings from PlayerPrefs. Uses defaults if not found.
    /// </summary>
    public void Load()
    {
        masterVolume = PlayerPrefs.GetFloat("masterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("musicVolume", 0.8f);
        sfxVolume = PlayerPrefs.GetFloat("sfxVolume", 1f);

        moveUp = (KeyCode)PlayerPrefs.GetInt("key_moveUp", (int)KeyCode.W);
        moveDown = (KeyCode)PlayerPrefs.GetInt("key_moveDown", (int)KeyCode.S);
        moveLeft = (KeyCode)PlayerPrefs.GetInt("key_moveLeft", (int)KeyCode.A);
        moveRight = (KeyCode)PlayerPrefs.GetInt("key_moveRight", (int)KeyCode.D);
        attack = (KeyCode)PlayerPrefs.GetInt("key_attack", (int)KeyCode.Mouse0);
        pickupWeapon = (KeyCode)PlayerPrefs.GetInt("key_pickupWeapon", (int)KeyCode.Q);
        openMenu = (KeyCode)PlayerPrefs.GetInt("key_openMenu", (int)KeyCode.Tab);
        openMenuAlt = (KeyCode)PlayerPrefs.GetInt("key_openMenuAlt", (int)KeyCode.Escape);
        interact = (KeyCode)PlayerPrefs.GetInt("key_interact", (int)KeyCode.E);
    }

    /// <summary>
    /// Reset all settings to defaults.
    /// </summary>
    public void ResetToDefaults()
    {
        masterVolume = 1f;
        musicVolume = 0.8f;
        sfxVolume = 1f;

        moveUp = KeyCode.W;
        moveDown = KeyCode.S;
        moveLeft = KeyCode.A;
        moveRight = KeyCode.D;
        attack = KeyCode.Mouse0;
        pickupWeapon = KeyCode.Q;
        openMenu = KeyCode.Tab;
        openMenuAlt = KeyCode.Escape;
        interact = KeyCode.E;
    }
}
