using UnityEngine;

public class MenuController : MonoBehaviour
{
    public GameObject menuCanvas;    
    void Start()
    {
        menuCanvas.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab    ))
        {
            if(!menuCanvas.activeSelf && PauseController.isGamePaused)
            {
                return;
            }
            menuCanvas.SetActive(!menuCanvas.activeSelf);
            PauseController.SetPause(menuCanvas.activeSelf);
        }
    }
}
