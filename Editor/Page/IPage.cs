using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[APIInfo("NPackage", "IPage", @"继承此接口，会在NPackage窗口添加一个菜单项。
菜单项的名称在`GetPageName`方法中设置。
新页面绘制在DrawWndUI()方法中实现")]
/// <summary>
/// N-Package 左侧页签
/// </summary>
public interface IPage
{
    /// <summary>
    /// 页签名称
    /// </summary>
    /// <returns></returns>
    string GetPageName();

    /// <summary>
    /// 绘制UI
    /// </summary>
    void DrawWndUI(EditorWindow window, object data = null);
}
