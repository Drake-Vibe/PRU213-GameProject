using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class CheckUIComponents : EditorWindow
{
    [MenuItem("Tools/PRU213 Setup/🔍 Debug UI Components", priority = 99)]
    public static void DebugUI()
    {
        GameObject settingTab = GameObject.Find("SettingTab");
        if (settingTab == null)
        {
            Debug.LogError("SettingTab not found in scene!");
            return;
        }

        Debug.Log($"=== Debugging components on {settingTab.name} ===");
        foreach (Component comp in settingTab.GetComponents<Component>())
        {
            if (comp == null)
            {
                Debug.Log("- Null component (missing script reference!)");
                continue;
            }

            string info = $"- {comp.GetType().Name} (enabled: {(comp is Behaviour ? ((Behaviour)comp).enabled.ToString() : "N/A")})";
            
            if (comp is Graphic g)
            {
                info += $", raycastTarget: {g.raycastTarget}";
            }
            if (comp is Button btn)
            {
                info += $", onClick listeners: {btn.onClick.GetPersistentEventCount()} persistent";
            }
            Debug.Log(info);
        }

        Debug.Log("=== Children ===");
        for (int i = 0; i < settingTab.transform.childCount; i++)
        {
            Transform child = settingTab.transform.GetChild(i);
            Debug.Log($"Child: {child.name}");
            foreach (Component childComp in child.GetComponents<Component>())
            {
                if (childComp == null) continue;
                string childInfo = $"  * {childComp.GetType().Name}";
                if (childComp is Graphic g)
                {
                    childInfo += $", raycastTarget: {g.raycastTarget}";
                }
                Debug.Log(childInfo);
            }
        }
    }
}
