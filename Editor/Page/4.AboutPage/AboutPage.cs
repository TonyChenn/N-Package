using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AboutPage : IPage
{
    private GUIStyle blueTextStyle;

    public const string TAG = "4.关于";
    public string GetPageName()
    {
        return TAG;
    }

    public void DrawWndUI(EditorWindow window, object data = null)
    {
        if(blueTextStyle== null)
        {
            blueTextStyle = new GUIStyle();
            blueTextStyle.normal.textColor = Color.blue;
        }

        EditorGUILayout.LabelField("关于");

        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("TonyChenn");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("https://blog.tonychenn.cn", blueTextStyle))
        {
            Application.OpenURL("https://blog.tonychenn.cn");
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
    }
}
