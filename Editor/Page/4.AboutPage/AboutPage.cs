using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class AboutPage : IPage
{
    public const string TAG = "4.关于";
    public string GetPageName()
    {
        return TAG;
    }

    public void DrawWndUI(EditorWindow window, object data = null)
    {
        EditorGUILayout.LabelField("关于");
    }
}
