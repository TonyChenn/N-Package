using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TreeNode
{
    public string name;
    public TreeNode parent;
    public List<TreeNode> children = null;
    public bool isOpen = false;

    public object data = null;

    public TreeNode(string name)
    {
        this.name = name;
        children = new List<TreeNode>();
    }
    public TreeNode(string name, object data)
    {
        this.name = name;
        this.data = data;
        children = new List<TreeNode>();
    }

    public void AddChild(TreeNode node)
    {
        if (children == null) children = new List<TreeNode>();

        children.Add(node);
        node.parent = this;
    }

    public bool HasChild(string name)
    {
        return GetChild(name) != null;
    }

    public TreeNode GetChild(string name)
    {
        if (children == null) return null;
        if (children.Count == 0) return null;

        for (int i = 0, iMax = children.Count; i < iMax; i++)
        {
            if (children[i].name == name) return children[i];
        }
        return null;
    }

    public bool IsGroup { get { return children.Count > 0; } }
    public bool IsItem { get { return children.Count == 0; } }
}

public static class TreeView
{
    private static int skipRenderLayer = 0;
    private static TreeNode curSelectedNode = null;

    public static TreeNode CurSelectNode
    {
        get { return curSelectedNode; }
    }
    public static void DrawUI(TreeNode root, int skipLayer = 0)
    {
        skipRenderLayer = skipLayer;
        drawNode(root, 0);
    }

    private static void drawNode(TreeNode node, int layer)
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
                GUILayout.BeginVertical("box");
                node.isOpen = EditorGUILayout.Foldout(node.isOpen, node.name, true);
                GUILayout.EndVertical();

                if (node.isOpen)
                {
                    for (int i = 0, iMax = node.children.Count; i < iMax; i++)
                    {
                        drawNode(node.children[i], layer + 1);
                    }
                }
            }
            else
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
        }
    }
}
