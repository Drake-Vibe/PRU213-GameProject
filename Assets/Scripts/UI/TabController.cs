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
        Debug.Log($"[TabController] ActiveTab({tabNo}) called.");
        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] != null)
            {
                pages[i].SetActive(i == tabNo);
            }
            if (i < tabImages.Length && tabImages[i] != null)
            {
                tabImages[i].color = (i == tabNo) ? Color.white : Color.grey;
            }
        }
    }
}
