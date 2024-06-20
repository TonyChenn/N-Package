using System.Linq;
using UnityEditor;
using UnityEngine;

public class UnityEditorGUIExtention
{
	private static GUIStyle redTextStyle;

	[InitializeOnLoadMethod]
	private static void Initialize()
	{
		redTextStyle = new GUIStyle();
		redTextStyle.normal.textColor = Color.red;
		redTextStyle.fontSize = 12;
		redTextStyle.alignment = TextAnchor.MiddleRight;

		EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
		EditorApplication.projectWindowItemOnGUI += ProjectWindowItemOnGUI;
	}


	private static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
	{
		GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
		if (gameObject == null) return;

		if (Path_UnityEditorGUIExtention.HierarchyShowMissingComponent)
		{
			var missingComponent = gameObject.GetComponents<MonoBehaviour>().Any(c => c == null);
			if (!missingComponent) return;

			Rect rect = selectionRect;
			rect.x = rect.xMax;
			rect.width = 3;
			GUI.Label(rect, "!", redTextStyle);
		}
	}

	private static void ProjectWindowItemOnGUI(string guid, Rect selectionRect)
	{
	}
}
