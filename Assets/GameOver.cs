using UnityEngine;
using UnityEngine.SceneManagement;
public class GameOver : MonoBehaviour
{
    public void loadMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

}
