using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Xml.Linq;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using UnityEditor.Callbacks;


[EditorWindowTitle(title = "Perferences...", icon = "d_SettingsIcon")]
public class PackageWindow : EditorWindow
{
	internal static PackageWindow window { get; private set; }

	private static bool m_needRefresh
	{
		get;
		set;
	}
	private static string m_tag = null;
	private static string m_data = null;

	[MenuItem("Tools/Perference New")]
	public static void ShowWnd()
	{
		window = GetWindow<PackageWindow>();
	}

	public static void ShowWnd(string tag, string data = null)
	{
		m_tag = tag;
		m_data = data;

		ShowWnd();
	}

	TwoSplitView splitView = null;
	VisualElement leftPane = null;
	VisualElement rightPane = null;
	public void CreateGUI()
	{
		RefreshPageData();

		splitView = new TwoSplitView(0, 180f, TwoPaneSplitViewOrientation.Horizontal);
		rootVisualElement.Add(splitView);

		leftPane = new VisualElement();
		leftPane.name = "leftPane";
		splitView.Add(leftPane);

		rightPane = new VisualElement();
		rightPane.name = "rightPane";
		splitView.Add(rightPane);

		leftPane.Clear();
		rightPane.Clear();

		drawLeftSelection(leftPane);
	}



	#region [左侧部分]
	private void drawLeftSelection(VisualElement leftPane)
	{
		var listView = new ListView();
		listView.Clear();

		leftPane.Add(listView);
		listView.makeItem = () => new Label();
		listView.itemsSource = datas;
		listView.bindItem = (btn, index) =>
		{
			(btn as Label).text = datas[index].Key;
		};
		listView.onSelectedIndicesChange += leftItemIndexChanged;
		listView.SetSelection(1);
	}
	private void leftItemIndexChanged(IEnumerable<int> indexList)
	{
		int index = indexList.First();
		drawRightSelection(index);

	}
	#endregion

	#region [右侧部分]
	private void drawRightSelection(int index)
	{
		rightPane.Clear();
		if (index >= datas.Count) return;

		IMGUIContainer container = new IMGUIContainer();
		rightPane.Add(container);
		
		Type classType = datas[index].Value;
		object ins = GetPageClass(classType);
		 
		container.onGUIHandler = () =>
		{
			MethodInfo method = classType.GetMethod("DrawWndUI", new Type[] { typeof(EditorWindow), typeof(object) });

			// TODO 待优化
			if (m_needRefresh)
				method.Invoke(ins, new object[] { this, m_data });
			else
				method.Invoke(ins, new object[] { this, null });
		};
	}
	#endregion


	#region [刷新数据]
	List<KeyValuePair<string, Type>> datas = new List<KeyValuePair<string, Type>>();

	private void RefreshPageData()
	{
		datas.Clear();
		datas = new List<KeyValuePair<string, Type>>();

		var clazzList = ReflectionUtil.GetImplementsInterfaceClass<IPage>();
		foreach (var clazz in clazzList)
		{
			MethodInfo method = ReflectionUtil.GetMethodInfo(clazz, "GetPageName");
			if (method == null) continue;

			object ins = GetPageClass(clazz);
			string moduleName = ins.InvokeMethod<string>("GetPageName");
			datas.Add(new KeyValuePair<string, Type>(moduleName, clazz));
		}

		datas.Sort((a, b) => a.Key.CompareTo(b.Key));
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
