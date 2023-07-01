using System.Collections.Generic;

public class TreeNode
{
    public string name;
    public TreeNode parent;
    public List<TreeNode> children = null;
    public bool isOpen = false;
    public bool isGroup = false;

    public object data = null;

    public TreeNode(string name)
    {
        this.name = name;
        children = new List<TreeNode>();
    }
    public TreeNode(string name, bool isGroup) : this(name)
    {
        this.isGroup = isGroup;
    }

    public TreeNode(string name, object data) : this(name)
    {
        this.data = data;
        children = new List<TreeNode>();
    }

    public void AddChild(TreeNode node)
    {
        if (children == null) children = new List<TreeNode>();

        children.Add(node);
        node.parent = this;
        if (!isGroup) isGroup = true;
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

    public bool IsGroup { get { return isGroup; } }
}

