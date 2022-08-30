using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class SettingPage : IPage
{
    public const string TAG = "2.设置";

    public SettingPage()
    {
        RefreshData();
    }

    public string GetPageName()
    {
        return TAG;
    }

    #region [Draw UI]
    private Vector3 settingPos;
    private int selectIndex;
    private Dictionary<string, List<Type>> dict = new Dictionary<string, List<Type>>();

    public void DrawWndUI(EditorWindow window, object data = null)
    {
        refreshSelectedIndex(data);
        // left section
        GUILayout.BeginVertical(GUILayout.Width(200), GUILayout.MinHeight(window.position.height - 10));
        GUILayout.Space(5);
        settingPos = EditorGUILayout.BeginScrollView(settingPos);
        selectIndex = GUILayout.SelectionGrid(selectIndex, dict.Keys.ToArray(), 1);
        EditorGUILayout.EndScrollView();
        GUILayout.EndVertical();


        GUILayout.Button("", GUILayout.Width(1), GUILayout.Height(window.position.height));


        // right section
        GUILayout.BeginVertical();
        GUILayout.Space(5);
        var item = dict.ElementAt(selectIndex);

        foreach (Type type_item in item.Value)
        {
            // 绘制字段
            foreach (PropertyInfo field in type_item.GetProperties())
            {
                DrawProperty(field);
            }
            // 绘制方法
            foreach (MethodInfo method in type_item.GetMethods())
            {
                DrawMethod(method);
            }
        }
        GUILayout.EndVertical();
    }
    private void refreshSelectedIndex(object data = null)
    {
        if (data == null) return;
        string tag = (string)data;

        if (string.IsNullOrEmpty(tag)) return;

        var list = dict.Keys.ToList<string>();
        selectIndex = list.IndexOf(tag);

        if (selectIndex == -1)
        {
            Debug.LogWarning($"\"{tag}\"不存在");
            selectIndex = 0;
        }
    }

    /// <summary>
    /// 绘制字段
    /// </summary>
    private void DrawProperty(PropertyInfo property)
    {
        SettingPropertyAttribute attr = property.GetCustomAttribute<SettingPropertyAttribute>(false);
        if (attr != null)
        {
            GUILayout.BeginHorizontal();
            //字段标题
            string title = attr.Title;

            switch (attr.FieldType)
            {
                case FieldType.TextField:
                    DrawTextField(property, title);
                    break;
                case FieldType.EditField:
                    DrawEditField(property, title);
                    break;
                case FieldType.Folder:
                    DrawSelectFolder(property, title);
                    break;
                case FieldType.File:
                    DrawSelectFile(property, title);
                    break;
                case FieldType.Toggle:
                    DrawToggle(property, title);
                    break;
                default:
                    GUILayout.Button(property.PropertyType.ToString());
                    break;
            }
            GUILayout.EndHorizontal();
        }


    }

    /// <summary>
    /// 绘制方法
    /// </summary>
    /// <param name="methodInfo"></param>
    private void DrawMethod(MethodInfo methodInfo)
    {
        SettingMethodAttribute attr = methodInfo.GetCustomAttribute<SettingMethodAttribute>(false);
        if (attr != null)
        {
            GUILayout.BeginHorizontal();
            DrawButton(methodInfo, attr.Title, attr.Text);
            GUILayout.EndHorizontal();
        }
    }
    #endregion


    #region [Draw Field]
    private void DrawButton(MethodInfo methodInfo, string title, string text)
    {
        DrawButton(title, text, () =>
        {
            methodInfo.Invoke(this, null);
        });
    }

    private void DrawButton(string title, string text, Action action)
    {
        GUILayout.Label(title, GUILayout.Width(150));
        GUILayout.Space(30);
        if (GUILayout.Button(text))
        {
            action?.Invoke();
        }
    }

    /// <summary>
    /// 文本框(不可编辑)
    /// </summary>
    /// <param name="property"></param>
    /// <param name="fieldName"></param>
    private void DrawTextField(PropertyInfo property, string fieldName)
    {
        GUILayout.Label(fieldName, GUILayout.Width(150));
        GUILayout.Space(30);
        string strValue = property.GetValue(null).ToString();
        GUILayout.TextField(strValue);
    }

    /// <summary>
    /// 文本框(可编辑)
    /// </summary>
    /// <param name="property"></param>
    /// <param name="fieldName"></param>
    private void DrawEditField(PropertyInfo property, string fieldName)
    {
        GUILayout.Label(fieldName, GUILayout.Width(150));
        GUILayout.Space(30);
        string strValue = property.GetValue(null).ToString();
        property.SetValue(null, GUILayout.TextField(strValue));
    }

    /// <summary>
    /// 绘制选择文件夹
    /// </summary>
    private void DrawSelectFolder(PropertyInfo property, string fieldName)
    {
        GUILayout.Label(fieldName, GUILayout.Width(150));

        string strValue = property.GetValue(null).ToString();
        if (GUILayout.Button("...", GUILayout.Width(30)))
        {
            strValue = EditorUtility.SaveFolderPanel("选择" + fieldName, strValue,
                strValue);
            if (!string.IsNullOrEmpty(strValue))
                property.SetValue(null, strValue);
        }

        GUILayout.TextField(strValue);
    }

    /// <summary>
    /// 绘制选择文件
    /// </summary>
    private void DrawSelectFile(PropertyInfo property, string fieldName)
    {
        GUILayout.Label(fieldName, GUILayout.Width(150));
        string strValue = property.GetValue(null).ToString();
        if (GUILayout.Button("...", GUILayout.Width(30)))
        {
            strValue = EditorUtility.OpenFilePanel("选择" + fieldName, strValue, "*");
            if (!string.IsNullOrEmpty(strValue))
                property.SetValue(null, strValue);
        }

        GUILayout.TextField(strValue);
    }

    /// <summary>
    /// 绘制Toggle
    /// </summary>
    private void DrawToggle(PropertyInfo property, string fieldName)
    {
        GUILayout.Label(fieldName, GUILayout.Width(150));

        bool boolValue = (bool)property.GetValue(null);
        boolValue = GUILayout.Toggle(boolValue, "");
        property.SetValue(null, boolValue);
    }

    #endregion


    #region [刷新数据]
    private void RefreshData()
    {
        dict.Clear();

        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (Assembly assembly in assemblies)
        {
            var classTypes = assembly.GetTypes();
            for (int i = 0; i < classTypes.Length; i++)
            {
                if (classTypes[i].IsInterface) continue;

                // 所有有接口的类
                Type ins = classTypes[i].GetInterface("IPathConfig");
                MethodInfo method = classTypes[i].GetMethod("GetModuleName", Type.EmptyTypes);

                if (ins != null && method != null)
                {
                    object o = Activator.CreateInstance(classTypes[i]);
                    string moduleName = method.Invoke(o, new object[] { }).ToString();

                    if (!dict.ContainsKey(moduleName))
                        dict[moduleName] = new List<Type>();
                    dict[moduleName.ToString()].Add(classTypes[i]);
                }
            }
        }
    }
    #endregion
}
