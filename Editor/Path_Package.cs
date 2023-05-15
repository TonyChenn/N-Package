using NCore.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path_Package : IPathConfig, IEditorPrefs
{
    private static string default_ex_pkg_install_folder => $"{Application.dataPath}/Modules";

    [SettingProperty(FieldType.Folder,"��װĿ¼��")]
    public static string InstallFolder
    {
        get { return EditorPrefsHelper.GetString("Path_Package_InstallFolder", default_ex_pkg_install_folder); }
        set => EditorPrefsHelper.SetString("Path_Package_InstallFolder", value);
    }

    #region IPathConfig,IEditorPrefs
    public const string TAG = "������������";
    public string GetModuleName() { return TAG; }

    public void ReleaseEditorPrefs()
    {
        EditorPrefsHelper.DeleteKey("Path_Package_InstallFolder");
    }
    #endregion
}
