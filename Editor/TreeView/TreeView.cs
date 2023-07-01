using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TreeView
{
    private int skipRenderLayer = 0;
    protected TreeNode curSelectedNode = null;

    public TreeNode CurSelectNode
    {
        get { return curSelectedNode; }
    }
    public void DrawUI(TreeNode root, int skipLayer = 0)
    {
        skipRenderLayer = skipLayer;
        drawNode(root, 0);
    }

    protected virtual void drawNode(TreeNode node, int layer)
    {
        if (node == null) return;

        if (layer < skipRenderLayer)
        {
            node.isOpen = true;

            for (int i = 0, iMax = node.children.Count; i < iMax; i++)
            {
                drawNode(node.children[i], layer + 1);
            }
        }
        else
        {
            if (node.IsGroup)
            {
                drawGroup(node, layer);
            }
            else
            {
                drawItem(node, layer);
            }
        }
    }

    protected virtual void drawGroup(TreeNode node, int layer)
    {
        GUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal();
        drawSpace(layer);
        node.isOpen = EditorGUILayout.Foldout(node.isOpen, node.name, true);
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        if (node.isOpen)
        {
            for (int i = 0, iMax = node.children.Count; i < iMax; i++)
            {
                drawNode(node.children[i], layer + 1);
            }
        }
    }
    protected virtual void drawItem(TreeNode node, int layer)
    {
        GUILayout.BeginHorizontal("box");
        GUIContent content = EditorGUIUtility.IconContent("sv_icon_dot11_sml");
        GUILayout.Label(content, GUILayout.Width(15), GUILayout.Height(20));
        GUILayout.Label(node.name);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        var rect = GUILayoutUtility.GetLastRect();

        if (curSelectedNode == node)
        {
            GUI.Box(rect, "", "SelectionRect");
        }

        if (rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseUp)
        {
            curSelectedNode = node;
            Event.current.Use();
        }
    }

    protected virtual void drawSpace(int layer)
    {
        GUILayout.Space((layer - skipRenderLayer) * 15);
    }
}
