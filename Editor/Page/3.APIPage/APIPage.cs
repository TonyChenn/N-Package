using System;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

public class APIPage : IPage
{
    public const string TAG = "3.API文档";
    private TreeView treeView = new TreeView();

    private MarkdownViewer markdownViewer;

    public APIPage()
    {
        markdownViewer = new MarkdownViewer(null, "");

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

        treeView.DrawUI(root, 1);

        GUILayout.EndScrollView();
        GUILayout.EndVertical();


        GUILayout.Button("", GUILayout.Width(1), GUILayout.Height(height));
        GUILayout.Space(10);


        DrawAPIDetail(window.position);
    }
    private void DrawAPIDetail(Rect windowRect)
    {
        if (treeView.CurSelectNode != null)
        {
            APIInfoAttribute info = (APIInfoAttribute)treeView.CurSelectNode.data;
            // debug use
            //GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(windowRect.width-440), GUILayout.MinHeight(10));
            //GUILayout.Label("111");
            //GUILayout.EndVertical();

            var lastRect = GUILayoutUtility.GetLastRect();
            var rect = new Rect(lastRect.x, lastRect.y + lastRect.height,
                            windowRect.width - 440, windowRect.height - 10);
            markdownViewer.DrawWithRect(rect);

            StringBuilder builder = new StringBuilder();
            builder.Append("# ");
            builder.Append(info.ClassName);
            builder.AppendLine();
            builder.Append("## ").Append(info.Description);
            markdownViewer.UpdateContent(builder.ToString());
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
