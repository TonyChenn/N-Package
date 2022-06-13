using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class AboutPage : IPage
{
    public string GetPageName()
    {
        return "4.关于";
    }

    public void DrawWndUI(EditorWindow window, object data = null)
    {
        EditorGUILayout.LabelField("关于");
    }
}
