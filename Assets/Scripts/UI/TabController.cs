using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;

public class TabController : MonoBehaviour
{
    public UnityEngine.UI.Image[] tabImages;
    public GameObject[] pages;
    void Start()
    {
        ActiveTab(0);
    }
    public void ActiveTab(int tabNo)
    {
        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] != null)
                pages[i].SetActive(false);
            if (i < tabImages.Length && tabImages[i] != null)
                tabImages[i].color = Color.grey;
        }
        if (tabNo >= 0 && tabNo < pages.Length && pages[tabNo] != null)
            pages[tabNo].SetActive(true);
        if (tabNo >= 0 && tabNo < tabImages.Length && tabImages[tabNo] != null)
            tabImages[tabNo].color = Color.white;
    }
}
