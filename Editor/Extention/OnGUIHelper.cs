using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class OnGUIHelper
{
    public static void DrawButton(string text, bool enable,params GUILayoutOption[] options)
    {
        GUILayout.BeginHorizontal("box");
        GUILayout.Space(10);
        GUILayout.Label(text, options);
        GUILayout.FlexibleSpace();
        GUILayout.Space(10);
        GUILayout.EndHorizontal();

        if(enable)
        {
            GUI.Box(GUILayoutUtility.GetLastRect(),"", "SelectionRect");
        }
    }
}
