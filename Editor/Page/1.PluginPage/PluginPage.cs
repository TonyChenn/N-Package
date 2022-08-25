using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PluginPage : IPage
{
    public const string TAG = "1.插件管理";
    public string GetPageName()
    {
        return TAG;
    }

    private Vector2 pluginPos;
    private string[] pluginTypeItems = new string[] { "全部", "已安装" };
    private int selectedPluginTypeIndex;
    public void DrawWndUI(EditorWindow window, object data = null)
    {
        GUILayout.BeginVertical(GUILayout.Width(200), GUILayout.MinHeight(window.position.height - 10));
        DrawSearchGUI();

        GUILayout.BeginHorizontal();
        selectedPluginTypeIndex = GUILayout.SelectionGrid(selectedPluginTypeIndex, pluginTypeItems, 2);
        GUILayout.EndHorizontal();
        GUILayout.Space(5);
        GUILayout.Button("", GUILayout.Height(1));
        GUILayout.Space(10);

        pluginPos = GUILayout.BeginScrollView(pluginPos);
        {
            for (int i = 0; i < 50; i++)
            {
                GUILayout.Button("123");
            }

        }
        GUILayout.EndScrollView();

        GUILayout.EndVertical();
        GUILayout.Button("", GUILayout.Width(1), GUILayout.Height(window.position.height));
    }
    private string searchText;
    void DrawSearchGUI()
    {
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        searchText = EditorGUILayout.TextField("", searchText, "SearchTextField");
        GUILayout.EndHorizontal();
    }
}
