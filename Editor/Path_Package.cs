	using NCore.Editor;
using UnityEngine;

public class Path_Package : IPathConfig, IEditorPrefs
{
    private static string default_ex_pkg_install_folder => $"{Application.dataPath}/Modules";

    [SettingProperty(FieldType.Folder,"安装目录：")]
    public static string InstallFolder
    {
        get { return EditorPrefsHelper.GetString("Path_Package_InstallFolder", default_ex_pkg_install_folder); }
        set => EditorPrefsHelper.SetString("Path_Package_InstallFolder", value);
    }

    #region IPathConfig,IEditorPrefs
    public const string TAG = "包管理器配置";
    public string GetModuleName() { return TAG; }

    public void ReleaseEditorPrefs()
    {
        EditorPrefsHelper.DeleteKey("Path_Package_InstallFolder");
    }
    #endregion
}
