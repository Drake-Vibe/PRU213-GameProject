# Báo Cáo Chi Tiết: Dự Án The Abyssal Dungeon

Tài liệu này tổng hợp toàn bộ các chức năng, yêu cầu kỹ thuật (Requirements), đoạn mã nguồn quan trọng (Critical Code) và vị trí của các tài nguyên hình ảnh (Sprites) trong dự án game **The Abyssal Dungeon** để phục vụ việc viết báo cáo cuối kỳ.

---

## 1. Danh Sách Thành Viên & Phân Chia Công Việc (Contribution)
* **Trịnh Gia Bảo (DE180403 - 30%):** Thiết kế bản đồ (Map Making), Tương tác nhân vật (Player Interact), Menu trong game (In-game Menu), Hệ thống Lưu/Tải game (Save-Load System), Giao diện bản đồ thu nhỏ (Mini Map UI), Hệ thống hòm đồ (Inventory System).
* **Nguyễn Võ Hoàng Huy (DE180397 - 23.33%):** Menu chính (Main Menu), Màn hình thua cuộc (Game Over), Hệ thống chiến đấu (Battle System), Di chuyển của quái (Monster Movement).
* **Trương Kiều Ngọc Diễm (DE170116 - 23.33%):** Giới thiệu cốt truyện (Intro Cutscenes), Di chuyển nhân vật (Player Movement Animations), Các hoạt ảnh khác (Animations).
* **Nguyễn Mạnh Duy Hưng (DE170356 - 23.33%):** Hệ thống máu (Health System), Hệ thống tạm dừng game (Pause Game System), Tiếp tục chơi (Resume Game System).

---

## 2. Danh Sách Các Cảnh Chơi (Scenes) trong Game
Dự án bao gồm **7 cảnh chơi** chính (đã được cập nhật và đổi tên theo chuẩn demo):
1. **GameMainMenu.unity:** Giao diện bắt đầu game.
2. **UI-Default.unity:** Khu vực sảnh chính (Hub Town) để chuẩn bị vào phụ bản.
3. **Level 1.unity:** Tầng ngục tối thứ nhất (Dungeon Floor 1).
4. **Level 2.unity:** Tầng ngục tối thứ hai (Dungeon Floor 2).
5. **Boss Fight.unity:** Phòng Boss đấu Crystal Knight (Dungeon Floor 3).
6. **DemoEnding.unity:** Màn hình chiến thắng và cảm ơn người chơi đã hoàn thành bản Demo.
7. **GameOver.unity:** Màn hình thua cuộc khi người chơi hết máu.

*Đường dẫn lưu trữ trong Project:* `Assets/Scenes/`

---

## 3. Các Yêu Cầu Kỹ Thuật (Requirements) & Đoạn Mã Nguồn Quan Trọng

### Requirement 1: Player Movement (Di chuyển nhân vật bằng bàn phím)
* **Cơ chế:** Sử dụng hệ thống Input mới của Unity (`UnityEngine.InputSystem`) thu nhận chuyển động và áp dụng vào Rigidbody2D của nhân vật thông qua phương thức `rb.MovePosition`.
* **Đường dẫn Sprite nhân vật:** 
  * Ảnh gốc Idle/Run: `Assets/Art/Sprites/GameAssets/KnightAnimation/`
  * Tên File đại diện: `knight_m_idle_anim_f0.png`, `knight_m_run_anim_f0.png`
* **Mã nguồn quan trọng ([PlayerMovement.cs](file:///d:/PRU213-GameProject/Assets/Scripts/Player/PlayerMovement.cs#L30-L51)):**
```csharp
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private int speed = 5;
    private Vector2 movement;
    private Rigidbody2D rb;

    private void OnMovement(InputValue value)
    {
        movement = value.Get<Vector2>();
    }

    private void FixedUpdate()
    {   
        if(PauseController.isGamePaused)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
    }
}
```

---

### Requirement 2: Health Bar System & Damage Hit (Hệ thống thanh máu)
* **Cơ chế:** Khi nhân vật nhận sát thương, Máu (Health) giảm dần và cập nhật lên Slider UI thông qua Gradient màu sắc (chuyển đỏ khi thấp máu).
* **Đường dẫn Sprite UI:**
  * Khung Máu: `Assets/Art/Sprites/GameAssets/HealthBar/Heart.png`
  * Thanh Máu: `Assets/Art/Sprites/GameAssets/HealthBar/Bar.png`
  * Thanh Giáp/Năng lượng: `Assets/Art/Sprites/GameAssets/HealthBar/Shield.png` / `Mana.png`
* **Mã nguồn quan trọng ([HealthBar.cs](file:///d:/PRU213-GameProject/Assets/Scripts/UI/HealthBar.cs#L13-L28)):**
```csharp
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider healthSlider;
    public Gradient gradient;
    public Image fill;

    public void SetMaxHealth(int health)
    {
        healthSlider.maxValue = health;
        healthSlider.value = health;
        fill.color = gradient.Evaluate(1f);
    }

    public void SetHealth(int health)
    {
        healthSlider.value = health;
        fill.color = gradient.Evaluate(healthSlider.normalizedValue);
    }
}
```

---

### Requirement 3: Main Menu (Giao diện chính)
* **Cơ chế:** Gồm các chức năng chính: Bắt đầu game mới (New Game - tải Hub `UI-Default`), Tải game cũ (Load Game - đọc file save JSON) và Thoát game (Quit).
* **Mã nguồn quan trọng ([MainMenu.cs](file:///d:/PRU213-GameProject/Assets/Scripts/MainMenu/MainMenu.cs#L22-L46)):**
```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        // Reset save flag and load the default Hub level
        PlayerPrefs.SetInt("ShouldLoadSave", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene("UI-Default");
    }

    public void LoadGame()
    {
        // Mark that the game should load save on start
        PlayerPrefs.SetInt("ShouldLoadSave", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene("UI-Default");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
```

---

### Requirement 4: Game Over Screen (Màn hình thua cuộc)
* **Cơ chế:** Khi nhân vật hết máu, giao diện Game Over hiện lên hiển thị nút chơi lại màn hiện tại hoặc quay về Main Menu.
* **Mã nguồn quan trọng ([GameOver.cs](file:///d:/PRU213-GameProject/Assets/Scripts/UI/GameOver.cs#L10-L28)):**
```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public void RestartGame()
    {
        // Restart game by reloading Main Hub and resetting stats
        SceneManager.LoadScene("UI-Default");
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("GameMainMenu");
    }
}
```

---

### Requirement 5: Player Attack & Weapons (Hệ thống Tấn công & Vũ khí)
* **Cơ chế:** Người chơi có thể sử dụng vũ khí cận chiến (Melee) hoặc súng (Ranged Gun) tiêu hao Năng lượng (Energy). Phím bắn/tấn công có thể rebind tự do.
* **Đường dẫn Sprite:**
  * Vũ khí và Đạn: `Assets/Art/Sprites/GameAssets/Ninja Adventure - Asset Pack/Items/Weapon/`
* **Mã nguồn quan trọng ([Gun.cs](file:///d:/PRU213-GameProject/Assets/Weapons/Gun.cs#L45-L65) - Rút gọn):**
```csharp
using UnityEngine;

public class Gun : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.5f;
    private float nextFireTime = 0f;

    void Update()
    {
        KeyCode attackKey = SettingsManager.CurrentSettings != null ? SettingsManager.CurrentSettings.attack : KeyCode.Mouse0;
        
        if (Input.GetKey(attackKey) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }
}
```

---

### Requirement 6: Cutscenes & Story Line (Dẫn truyện mở đầu)
* **Cơ chế:** Phát các hình ảnh cốt truyện tuần tự, cập nhật văn bản, hỗ trợ phát âm thanh dẫn truyện (voiceovers/bgm) cho từng trang và cho phép bỏ qua (Skip).
* **Đường dẫn Sprite Cutscenes:** `Assets/Art/Cutscenes/`
* **Mã nguồn quan trọng ([CutsceneManager.cs](file:///d:/PRU213-GameProject/Assets/Scripts/UI/CutsceneManager.cs#L120-L140) - Trình diễn từng trang):**
```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CutsceneManager : MonoBehaviour
{
    public List<CutscenePage> pages;
    private int currentPageIndex = 0;
    public Image displayImage;
    public TextMeshProUGUI displayText;

    public void ShowPage(int index)
    {
        if(index < 0 || index >= pages.Count) return;
        displayImage.sprite = pages[index].pageSprite;
        displayText.text = pages[index].dialogueText;
        
        // Play page specific audio
        if (pages[index].pageAudio != null) {
            audioSource.clip = pages[index].pageAudio;
            audioSource.Play();
        }
    }
}
```

---

### Requirement 7, 8, 11: In-game Menu, Pause & Settings System (Hệ thống Cài đặt & Tạm dừng)
* **Cơ chế:** Cho phép người chơi điều chỉnh Master/Music/SFX Volume, thay đổi Keybindings (Gán phím mới) và tạm dừng Game (`Time.timeScale = 0f`).
* **Mã nguồn quan trọng ([PauseController.cs](file:///d:/PRU213-GameProject/Assets/Scripts/Controllers/PauseController.cs)):**
```csharp
using UnityEngine;

public class PauseController : MonoBehaviour
{
    public static bool isGamePaused = false;

    public static void PauseGame()
    {
        isGamePaused = true;
        Time.timeScale = 0f;
    }

    public static void ResumeGame()
    {
        isGamePaused = false;
        Time.timeScale = 1f;
    }
}
```

---

### Requirement 9: Save & Load Game (Lưu & Tải game)
* **Cơ chế:** Lưu trữ toàn bộ dữ liệu trạng thái bao gồm: Vị trí người chơi, Tên cảnh hiện tại, Tầng hầm ngục, Điểm số, Vũ khí đang trang bị, Chỉ số Máu/Năng lượng và các potion hiện có vào file JSON cục bộ (`saveData.json`).
* **Mã nguồn quan trọng ([SaveController.cs](file:///d:/PRU213-GameProject/Assets/Scripts/Data/SaveController.cs#L58-L100)):**
```csharp
using UnityEngine;
using System.IO;

public class SaveController : MonoBehaviour
{
    private string saveLocation = @"D:\PRU213-GameProject\Save File\saveData.json";

    public void SaveGame()
    {
        Player player = FindAnyObjectByType<Player>();
        GameManager gm = GameManager.Instance;

        SaveData saveData = new SaveData
        {
            savedSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
            playerPosition = player != null ? player.transform.position : Vector3.zero,
            currentLevel = gm != null ? gm.currentLevel : 1,
            score = gm != null ? gm.score : 0,
            playerHealth = player != null ? player.currentHealth : 100
        };

        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));
        Debug.Log("Game saved to JSON file successfully!");
    }
}
```

---

### Requirement 12: Inventory System (Hệ thống túi đồ)
* **Cơ chế:** Cho phép người chơi nhặt, mang theo các loại vũ khí khác nhau và chuyển đổi nhanh giữa chúng để chiến đấu.
* **Mã nguồn quan trọng ([InventoryController.cs](file:///d:/PRU213-GameProject/Assets/Scripts/Data/InventoryController.cs)):**
```csharp
using UnityEngine;
using System.Collections.Generic;

public class InventoryController : MonoBehaviour
{
    public List<GameObject> weapons = new List<GameObject>();
    public int currentWeaponIndex = 0;

    public void EquipWeapon(int index)
    {
        if (index < 0 || index >= weapons.Count) return;
        weapons[currentWeaponIndex].SetActive(false);
        currentWeaponIndex = index;
        weapons[currentWeaponIndex].SetActive(true);
    }
}
```

---

## 4. Chức Năng Bổ Sung Đặc Biệt (Extra / New Functions)

### Màn hình Kết thúc Demo (Demo Victory Ending)
* **Cơ chế:** Tự động kích hoạt khi người chơi hạ gục Boss ở màn chơi thứ ba (`Boss Fight`). Nó dọn dẹp sạch sẽ các Singleton còn thừa từ DontDestroyOnLoad (như Player, HUD Canvas, Menu In-Game) để đảm bảo không bị rác bộ nhớ, phát bài nhạc chủ đề kết thúc `8 - End Theme.ogg`, và hiển thị tổng điểm số cuối cùng của người chơi mà không cần hiển thị số màn chơi hoàn thành theo yêu cầu tinh giản.
* **Mã nguồn quan trọng ([DemoEnding.cs](file:///d:/PRU213-GameProject/Assets/Scripts/UI/DemoEnding.cs)):**
```csharp
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class DemoEnding : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    private void Start()
    {
        // 1. Display Final Score from GameManager
        GameManager gm = GameManager.Instance;
        if (gm != null && scoreText != null)
        {
            scoreText.text = $"FINAL SCORE: {gm.score:0000}";
        }

        // 2. Clear all persistent gameplay singletons
        Player.DestroyInstance();
        PlayerHUD.DestroyInstance();
        MenuController.DestroyInstance();
        
        // Unlock cursor for UI interaction
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void LoadMenu()
    {
        LevelManager lm = LevelManager.Instance;
        if (lm != null)
            lm.ReturnToMenu();
        else
            SceneManager.LoadScene("GameMainMenu");
    }
}
```

---

## 5. Danh Sách Vị Trí Các Sprite Cần Thiết Cho Báo Cáo
Khi chèn ảnh minh họa cho báo cáo, bạn hãy sử dụng hoặc chụp màn hình dựa trên các tài nguyên ảnh tại các thư mục này:

1. **Sprite Nhân Vật Chính:** `Assets/Art/Sprites/GameAssets/KnightAnimation/`
   * Đại diện: `knight_m_idle_anim_f0.png` (Ảnh đứng yên), `knight_m_run_anim_f0.png` (Ảnh chạy).
2. **Sprite Gạch Nền (Tilemap):** `Assets/Art/Sprites/Map/`
   * Đại diện: `dgtile.png`, `dgtile1.png` (Sử dụng để vẽ bản đồ ngục tối).
3. **Sprite Vật Phẩm & UI Trang Trí:** `Assets/Art/Sprites/GameAssets/Ninja Adventure - Asset Pack/`
   * Trang trí (Decor): `Backgrounds/` & `Palettes/`
4. **Sprite Trái Tim & UI Thanh Máu:** `Assets/Art/Sprites/GameAssets/HealthBar/`
   * Đại diện: `Heart.png` (Tim), `Bar.png` (Thanh trượt máu), `Shield.png` (Giáp), `Mana.png` (Năng lượng).
5. **Sprite Cảnh Cốt Truyện (Cutscenes):** `Assets/Art/Cutscenes/`
   * Các ảnh minh họa cutscene phân trang dẫn truyện mở đầu game.
