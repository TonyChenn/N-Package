using NCore.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path_Markdown : IPathConfig,IEditorPrefs
{
    public const string TAG = "Markdown";
    public string GetModuleName()
    {
        return TAG;
    }

    [SettingProperty(FieldType.Toggle,"暗色主题")]
    public static bool UseDarkMarkdownTheme
    {
        get => EditorPrefsHelper.GetBool("Path_Markdown_UseDarkMarkdownTheme", true);
        set => EditorPrefsHelper.SetBool("Path_Markdown_UseDarkMarkdownTheme", value);
    }

    public void ReleaseEditorPrefs()
    {
        EditorPrefsHelper.DeleteKey("Path_Markdown_UseDarkMarkdownTheme");
    }
}
