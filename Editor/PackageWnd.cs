using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class PackageWnd : EditorWindow
{
    private static bool m_needRefresh = false;

    private static string m_tag = null;
    private static string m_data = null;

    [MenuItem("Tools/Perference...")]
    public static void ShowWnd()
    {
        GetWindow<PackageWnd>(false, "PackageMgr", true);
    }

    public static void ShowWnd(string tag, string data = null)
    {
        m_tag = tag;
        m_data = data;
        m_needRefresh = true;

        ShowWnd();
    }

    #region 生命周期
    private void OnEnable()
    {
        RefreshPageData();

        m_needRefresh = true;
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        drawLeftSelection();
        drawRightSelection();
        EditorGUILayout.EndHorizontal();

        if (m_needRefresh)
            m_needRefresh = false;
    }
    #endregion


    #region [左侧部分]
    private Vector2 leftScrollPos;
    private int selectedPageIndex;
    private void drawLeftSelection()
    {
        refreshSelectedIndex();
        GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(200), GUILayout.MinHeight(position.height));

        leftScrollPos = GUILayout.BeginScrollView(leftScrollPos);
        {
            selectedPageIndex = GUILayout.SelectionGrid(selectedPageIndex, keys.ToArray(), 1);
        }
        GUILayout.EndScrollView();

        if (GUILayout.Button("刷新"))
        {
            RefreshPageData();
        }
        GUILayout.EndVertical();
    }

    private void refreshSelectedIndex()
    {
        if (!m_needRefresh) return;

        if (!string.IsNullOrEmpty(m_tag))
        {
            selectedPageIndex = keys.IndexOf(m_tag);

            if (selectedPageIndex == -1)
            {
                Debug.LogWarning($"\"{m_tag}\"不存在");
                selectedPageIndex = 0;
            }
        }
    }
    #endregion


    #region [右侧部分]
    private void drawRightSelection()
    {
        if (keys.Count == 0) return;

        string pageName = keys[selectedPageIndex];
        Type classType = values[selectedPageIndex];

        MethodInfo method = classType.GetMethod("DrawWndUI", new Type[] { typeof(EditorWindow), typeof(object) });
        object o = GetPageClass(classType);

        // TODO 待优化
        if (m_needRefresh)
            method.Invoke(o, new object[] { this, m_data });
        else
            method.Invoke(o, new object[] { this, null });
    }
    #endregion


    #region [刷新数据]
    private List<string> keys = new List<string>();
    private List<Type> values = new List<Type>();
    private void RefreshPageData()
    {
        List<KeyValuePair<string, Type>> list = new List<KeyValuePair<string, Type>>();

        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (Assembly assembly in assemblies)
        {
            var classTypes = assembly.GetTypes();
            for (int i = 0; i < classTypes.Length; i++)
            {
                if (classTypes[i].IsInterface) continue;

                // 所有有接口的类
                Type ins = classTypes[i].GetInterface("IPage");
                MethodInfo method = classTypes[i].GetMethod("GetPageName", Type.EmptyTypes);

                if (ins != null && method != null)
                {
                    object o = GetPageClass(classTypes[i]);
                    string moduleName = method.Invoke(o, null).ToString();

                    list.Add(new KeyValuePair<string, Type>(moduleName, classTypes[i]));
                }
            }
        }

        list.Sort((a, b) => a.Key.CompareTo(b.Key));

        keys.Clear();
        values.Clear();
        foreach (var item in list)
        {
            keys.Add(item.Key);
            values.Add(item.Value);
        }
    }
    #endregion



    private Dictionary<Type, object> pageClass = new Dictionary<Type, object>();
    /// <summary>
    /// 实例化Page对象
    /// </summary>
    private object GetPageClass(Type type)
    {
        if (!pageClass.ContainsKey(type))
            pageClass[type] = Activator.CreateInstance(type);

        return pageClass[type];
    }
}
