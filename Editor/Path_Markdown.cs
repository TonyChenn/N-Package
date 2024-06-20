using NCore.Editor;

public class Path_Markdown : IPathConfig, IEditorPrefs
{
	public const string TAG = "Markdown";
	public string GetModuleName() => TAG;

	[SettingProperty(FieldType.Toggle, "暗色主题")]
	public static bool UseDarkMarkdownTheme
	{
		get => EditorPrefsHelper.GetBool("Path_Markdown_UseDarkMarkdownTheme", true);
		set => EditorPrefsHelper.SetBool("Path_Markdown_UseDarkMarkdownTheme", value);
	}

	[SettingProperty(FieldType.Toggle, "StripHTML")]
	public static bool StripHTML
	{
		get => EditorPrefsHelper.GetBool("Path_Markdown_StripHTML", true);
		set => EditorPrefsHelper.SetBool("Path_Markdown_StripHTML", value);
	}

	[SettingProperty(FieldType.Toggle, "PipedTables")]
	public static bool PipedTables
	{
		get => EditorPrefsHelper.GetBool("Path_Markdown_PipedTables", true);
		set => EditorPrefsHelper.SetBool("Path_Markdown_PipedTables", value);
	}

	[SettingProperty(FieldType.Toggle, "HeaderSeparator(依赖PipedTables)")]
	public static bool HeaderSeparator
	{
		get => EditorPrefsHelper.GetBool("Path_Markdown_HeaderSeparator", true);
		set => EditorPrefsHelper.SetBool("Path_Markdown_HeaderSeparator", value);
	}

	public void ReleaseEditorPrefs()
	{
		EditorPrefsHelper.DeleteKey("Path_Markdown_UseDarkMarkdownTheme");
		EditorPrefsHelper.DeleteKey("Path_Markdown_StripHTML");
		EditorPrefsHelper.DeleteKey("Path_Markdown_PipedTables");
	}
}
