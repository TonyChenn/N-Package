using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class SettingPage : IPage
{
	public const string TAG = "2.设置";
	private const string DEFAULE_GROUP = "默认分组";
	private GUIStyle titleTextStyle;

	public SettingPage()
	{
		titleTextStyle = new GUIStyle();
		titleTextStyle.fontSize = 20;
		titleTextStyle.fontStyle = FontStyle.Bold;
		titleTextStyle.normal.textColor = Color.white;

		RefreshData();
	}

	public string GetPageName() => TAG;

	#region [Draw UI]
	private Vector3 settingPos;
	private string selectKey;
	private Dictionary<string, List<Type>> dict = new Dictionary<string, List<Type>>();
	private Dictionary<string, List<MemberInfoItem>> groupDict = new Dictionary<string, List<MemberInfoItem>>(32);

	public void DrawWndUI(EditorWindow window, object data = null)
	{
		refreshSelectedIndex(data);

		#region left section
		GUILayout.BeginVertical(GUILayout.Width(200), GUILayout.MinHeight(window.position.height - 10));
		GUILayout.Space(5);
		settingPos = EditorGUILayout.BeginScrollView(settingPos);
		{
			foreach (string key in dict.Keys)
			{
				GUILayout.BeginHorizontal("box");
				GUILayout.Space(10);
				GUILayout.Label(key);
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();

				var rect = GUILayoutUtility.GetLastRect();

				if (selectKey == key)
				{
					GUI.Box(rect, "", "SelectionRect");
				}
				if (rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseUp)
				{
					selectKey = key;
					Event.current.Use();
				}
			}
		}
		EditorGUILayout.EndScrollView();
		GUILayout.EndVertical();

		GUILayout.Button("", GUILayout.Width(1), GUILayout.Height(window.position.height));
		#endregion

		#region right section
		GUILayout.BeginVertical();
		GUILayout.Space(5);

		selectKey ??= dict.Keys.FirstOrDefault();

		foreach (Type type_item in dict[selectKey])
		{
			groupDict.Clear();
			// 整理字段
			fillPropertyInfo(type_item.GetProperties(), ref groupDict);
			// 整理方法
			fillMethodInfo(type_item.GetMethods(), ref groupDict);

			// 分组绘制
			foreach (var item in groupDict)
			{
				drawOneGroup(item.Key, item.Value);
			}
		}
		GUILayout.EndVertical();
		#endregion
	}
	private void refreshSelectedIndex(object data = null)
	{
		if (data == null) return;
		string tag = (string)data;
		if (string.IsNullOrEmpty(tag)) return;

		if (dict.ContainsKey(tag))
		{
			selectKey = tag;
		}
		else
		{
			Debug.LogWarning($"\"{tag}\"不存在");
			selectKey = dict.Keys.First();
		}
	}

	private void fillPropertyInfo(PropertyInfo[] properties, ref Dictionary<string, List<MemberInfoItem>> dict)
	{
		foreach (PropertyInfo property in properties)
		{
			SettingPropertyAttribute attr = property.GetCustomAttribute<SettingPropertyAttribute>(false);
			if (attr == null) continue;

			string groupName = DEFAULE_GROUP;
			if (attr.GroupName != null && !string.IsNullOrWhiteSpace(attr.GroupName))
			{
				groupName = attr.GroupName;
			}
			if (!dict.ContainsKey(groupName))
			{
				dict[groupName] = new List<MemberInfoItem>();
			}
			dict[groupName].Add(new MemberInfoItem(property, null));
		}
	}

	private void fillMethodInfo(MethodInfo[] methods, ref Dictionary<string, List<MemberInfoItem>> dict)
	{
		foreach (MethodInfo method in methods)
		{
			SettingMethodAttribute attr = method.GetCustomAttribute<SettingMethodAttribute>(false);
			if (attr == null) continue;

			string groupName = DEFAULE_GROUP;
			if (attr.GroupName != null && !string.IsNullOrWhiteSpace(attr.GroupName))
			{
				groupName = attr.GroupName;
			}
			if (!dict.ContainsKey(groupName))
			{
				dict[groupName] = new List<MemberInfoItem>();
			}
			dict[groupName].Add(new MemberInfoItem(null, method));
		}
	}
	#endregion

	private struct MemberInfoItem
	{
		internal PropertyInfo propertyInfo;
		internal MethodInfo methodInfo;

		internal MemberInfoItem(PropertyInfo propertyInfo, MethodInfo methodInfo)
		{
			this.propertyInfo = propertyInfo;
			this.methodInfo = methodInfo;
		}
	}


	/// <summary>
	/// 绘制一个分组
	/// </summary>
	/// <param name="groupName"></param>
	/// <param name="list"></param>
	private void drawOneGroup(string groupName, List<MemberInfoItem> list)
	{
		GUILayout.Label(groupName, titleTextStyle);
		GUILayout.Space(5);
		foreach (var item in list)
		{
			if (item.propertyInfo != null)
			{
				DrawProperty(item.propertyInfo);
			}
			else if (item.methodInfo != null)
			{
				DrawMethod(item.methodInfo);
			}
		}
		GUILayout.Space(10);
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
				case FieldType.Enum:
					DrawEnum(property, title);
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





	#region [绘制设置组件]
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

	/// <summary>
	/// 绘制下拉框
	/// </summary>
	private void DrawEnum(PropertyInfo property, string fieldName)
	{
		GUILayout.Label(fieldName, GUILayout.Width(150));
		GUILayout.Space(30);
		BuildAssetBundleOptions option = (BuildAssetBundleOptions)property.GetValue(null);
		option = (BuildAssetBundleOptions)EditorGUILayout.EnumPopup(option);
		property.SetValue(null, option);
	}

	#endregion

	#region [刷新数据]

	private void RefreshData()
	{
		dict.Clear();

		List<Type> clazzList = ReflectionUtil.GetImplementsInterfaceClass<IPathConfig>();
		foreach (var clazz in clazzList)
		{
			MethodInfo methodInfo = ReflectionUtil.GetMethodInfo(clazz, "GetModuleName");
			if (methodInfo == null) continue;

			object ins = Activator.CreateInstance(clazz);
			string moduleName = ins.InvokeMethod<string>("GetModuleName");

			if (!dict.ContainsKey(moduleName)) dict[moduleName] = new List<Type>();
			dict[moduleName.ToString()].Add(clazz);
		}
	}

	#endregion
}
