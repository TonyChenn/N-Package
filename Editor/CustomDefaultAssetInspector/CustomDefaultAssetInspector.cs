using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[APIInfo("NPackage", "ICustomDefaultAssetInspector", @"
对DefaultAsset类型资源进行自定义 Inspector 面板，基于反射实现，详细实现参看：CustomDefaultAssetInspector

只需实现接口：ICustomDefaultAssetInspector

示例1：版控文件的预览：
```csharp
public class VersionFileInspector : ICustomDefaultAssetInspector
{
	public bool CanDraw(string path)
	{
		string fileName = Path.GetFileName(path);
		return fileName.StartsWith(""version"") && fileName.EndsWith("".data"");
	}

	public void Draw(string path){ }
}
```
![](https://raw.githubusercontent.com/TonyChenn/BlogPicture/master/2023/q3/custom_default_asset_data.jpg)

示例2：lua文件支持：
```csharp
public class LuaInspector : ICustomDefaultAssetInspector
{
	public bool CanDraw(string path)
	{
		return path.EndsWith("".lua"");
	}

	public void Draw(string path)
	{
		string text = File.ReadAllText(path);
		GUILayout.TextArea(text);
	}
}
```
![](https://raw.githubusercontent.com/TonyChenn/BlogPicture/master/2023/q3/custom_default_asset_lua.jpg)
")]
public interface ICustomDefaultAssetInspector
{
	bool CanDraw(string path);
	void Draw(string path);
}

[CanEditMultipleObjects, CustomEditor(typeof(DefaultAsset))]
public class CustomDefaultAssetInspector : Editor
{
	readonly Dictionary<object, Type> dict = new();
	private void OnEnable()
	{
		dict.Clear();

		List<Type> clazzList = ReflectionUtil.GetImplementsInterfaceClass<ICustomDefaultAssetInspector>();
		foreach (var clazz in clazzList)
		{
			object o = ReflectionUtil.CreateInstance(clazz);
			dict[o] = clazz;
		}
	}
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		string path = AssetDatabase.GetAssetPath(Selection.activeInstanceID);

		foreach (var item in dict)
		{
			object _ins = item.Key;
			bool canDraw = _ins.InvokeMethod<bool>("CanDraw", path);
			if (canDraw)
			{
				bool enabled = GUI.enabled;
				GUI.enabled = true;
				_ins.InvokeMethod("Draw", path);
				GUI.enabled = enabled;
			}
		}
	}
}
