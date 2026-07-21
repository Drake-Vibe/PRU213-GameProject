using UnityEngine;
using UnityEngine.UI;

public class TabController : MonoBehaviour
{
    public Image[] tabImages;
    public GameObject[] pages;

    private void Awake()
    {
        CleanNullReferences();
    }

    private void OnValidate()
    {
        CleanNullReferences();
    }

    private void CleanNullReferences()
    {
        if (tabImages != null)
        {
            for (int i = 0; i < tabImages.Length; i++)
            {
                if (tabImages[i] == null)
                {
                    tabImages[i] = null;
                }
            }
        }
        if (pages != null)
        {
            for (int i = 0; i < pages.Length; i++)
            {
                if (pages[i] == null)
                {
                    pages[i] = null;
                }
            }
        }
    }

    void Start()
    {
        ActiveTab(0);
    }

    public void ActiveTab(int tabNo)
    {
        Debug.Log($"[TabController] ActiveTab({tabNo}) called.");
        if (pages != null)
        {
            for (int i = 0; i < pages.Length; i++)
            {
                if (pages[i] != null)
                {
                    pages[i].SetActive(i == tabNo);
                }
            }
        }

        if (tabImages != null)
        {
            for (int i = 0; i < tabImages.Length; i++)
            {
                if (tabImages[i] != null)
                {
                    tabImages[i].color = (i == tabNo) ? Color.white : Color.grey;
                }
            }
        }
    }
}
