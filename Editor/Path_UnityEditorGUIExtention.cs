using NCore.Editor;

public class Path_UnityEditorGUIExtention : IPathConfig, IEditorPrefs
{
	private const string TAG = "UnityEditorGUIEx";

	[SettingProperty(FieldType.Toggle, "标记 MissingComponent")]
	public static bool ShowMissingComponent
	{
		get => EditorPrefsHelper.GetBool($"Path_Package_{TAG}_ShowMissingComponent", true);
		set => EditorPrefsHelper.SetBool($"Path_Package_{TAG}_ShowMissingComponent", value);
	}

	[SettingProperty(FieldType.Toggle,"显示BundleName", "Project")]
	public static bool ProjectShowBundleName
	{
		get => EditorPrefsHelper.GetBool($"Path_Package_{TAG}_ProjectShowBundleName", true);
		set => EditorPrefsHelper.SetBool($"Path_Package_{TAG}_ProjectShowBundleName", value);
	}

	[SettingProperty(FieldType.Toggle, "显示文件夹注释", "Inspector")]
	public static bool InspectorShowFolderNote
	{
		get => EditorPrefsHelper.GetBool($"Path_Package_{TAG}_InspectorShowFolderNote", true);
		set => EditorPrefsHelper.SetBool($"Path_Package_{TAG}_InspectorShowFolderNote", value);
	}

	#region
	public string GetModuleName() => TAG;

	public void ReleaseEditorPrefs()
	{
		EditorPrefsHelper.DeleteKey($"Path_Package_{TAG}_HierarchyShowMissingComponent");
		EditorPrefsHelper.DeleteKey($"Path_Package_{TAG}_ProjectShowBundleName");
		EditorPrefsHelper.DeleteKey($"Path_Package_{TAG}_InspectorShowFolderNote");
	}
	#endregion
}
