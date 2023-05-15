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

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        GUILayout.Label("TonyChenn");
        if(GUILayout.Button("https://blog.tonychenn.cn", blueTextStyle))
        {
            Application.OpenURL("https://blog.tonychenn.cn");
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }
}
