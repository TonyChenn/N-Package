using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class APIPage : IPage
{
    public const string TAG = "3.API文档";

    public APIPage()
    {
        RefreshData();
        init();
    }

    private void init()
    {
        GUITextStyle.GetTextStyle(FontStyle.Bold, 23);
    }

    public string GetPageName()
    {
        return TAG;
    }

    private Vector2 scrollPos;
    public void DrawWndUI(EditorWindow window, object data = null)
    {
        float height = window.position.height;
        GUILayout.BeginVertical(GUILayout.Width(200), GUILayout.Height(height));
        DrawSearchGUI();
        scrollPos = GUILayout.BeginScrollView(scrollPos);

        TreeView.DrawUI(root, 1);

        GUILayout.EndScrollView();
        GUILayout.EndVertical();


        GUILayout.Button("", GUILayout.Width(1), GUILayout.Height(height));
        GUILayout.Space(10);


        DrawAPIDetail();
    }
    private void DrawAPIDetail()
    {
        if (TreeView.CurSelectNode != null)
        {
            GUILayout.BeginVertical();
            APIInfoAttribute info = (APIInfoAttribute)TreeView.CurSelectNode.data;
            GUILayout.Label(info.ClassName, EditorStyles.boldLabel);
            GUILayout.Space(5);
            GUILayout.Label(info.Description);
            GUILayout.Button("", GUILayout.Height(1));
            GUILayout.Space(5);

            GUILayout.EndVertical();
        }
    }


    private string searchText;
    private void DrawSearchGUI()
    {
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        searchText = EditorGUILayout.TextField("", searchText, "SearchTextField");
        GUILayout.EndHorizontal();
    }

    #region [数据相关]
    TreeNode root = new TreeNode("root");
    public void RefreshData()
    {
        root.children.Clear();

        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        for (int i = 0, iMax = assemblies.Length; i < iMax; i++)
        {
            Assembly assembly = assemblies[i];

            Type[] classTypes = assembly.GetTypes();
            for (int j = 0, jMax = classTypes.Length; j < jMax; j++)
            {
                APIInfoAttribute attribute = classTypes[j].GetCustomAttribute<APIInfoAttribute>(false);
                if (attribute == null) continue;

                string groupName = attribute.GroupName;

                // 先确定分组
                if (!root.HasChild(groupName))
                    root.AddChild(new TreeNode(groupName));

                TreeNode groupNode = root.GetChild(groupName);
                if (groupNode == null) return;
                groupNode.AddChild(new TreeNode(attribute.ClassName, attribute));
            }
        }
    }
    #endregion
}
