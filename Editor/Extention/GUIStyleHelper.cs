using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GUITextStyle
{
    public static GUIStyle GetTextStyle(FontStyle fontStyle, int fontSize)
    {
        GUIStyle style = new GUIStyle();
        style.fontStyle = fontStyle;
        style.fontSize = fontSize;

        return style;
    }
}
