using System.IO;
using UnityEditor;
using UnityEngine;

public class PluginPage : IPage
{
    public const string TAG = "1.插件管理";
	public string GetPageName() => TAG;


	private Vector2 pluginPos;
    private string[] pluginTypeItems = new string[] { "全部", "已安装" };
    private int selectedPluginTypeIndex;
    public void DrawWndUI(EditorWindow window, object data = null)
    {
        GUILayout.BeginVertical(GUILayout.Width(200), GUILayout.MinHeight(window.position.height - 10));
        drawSearchGUI();

        GUILayout.BeginHorizontal();
        selectedPluginTypeIndex = GUILayout.SelectionGrid(selectedPluginTypeIndex, pluginTypeItems, 2);
        GUILayout.EndHorizontal();
        GUILayout.Space(5);
        GUILayout.Button("", GUILayout.Height(1));
        GUILayout.Space(10);

        pluginPos = GUILayout.BeginScrollView(pluginPos);
        {
            if(selectedPluginTypeIndex == 0)
            {
                drawAllPackagesView();
            }
            else
            {
                drawInstalledPackagesView();
            }
        }

        GUILayout.EndScrollView();

        GUILayout.EndVertical();
        GUILayout.Button("", GUILayout.Width(1), GUILayout.Height(window.position.height));
    }
    private string searchText;
    private void drawSearchGUI()
    {
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        searchText = EditorGUILayout.TextField("", searchText, "SearchTextField");
        GUILayout.EndHorizontal();
    }


    private string selectedName = null;
    private void drawInstalledPackagesView()
    {
        string folder = Path_Package.InstallFolder;
        if(!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        DirectoryInfo[] folders = new DirectoryInfo(folder).GetDirectories();

        if(folders.Length == 0)
        {
            drawEmptyTip();
            return;
        }

        if(selectedName == null)
            selectedName = folders[0].Name;

        for (int i = 0;i < folders.Length;i++)
        {
            GUILayout.BeginHorizontal("box");
            GUIContent content = EditorGUIUtility.IconContent("Assembly Icon");
            GUILayout.Label(content, GUILayout.Width(30), GUILayout.Height(20));
            GUILayout.Label(folders[i].Name);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            var rect = GUILayoutUtility.GetLastRect();
            if (selectedName == folders[i].Name)
            {
                GUI.Box(rect, "", "SelectionRect");
            }
            if (rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseUp)
            {
                selectedName = folders[i].Name;
                Event.current.Use();
            }
        }
    }

    private void drawAllPackagesView()
    {
        drawEmptyTip();
    }

    private void drawEmptyTip()
    {
        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("这里空空如也");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
    }
}
