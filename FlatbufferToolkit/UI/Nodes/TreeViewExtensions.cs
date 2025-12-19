namespace FlatbufferToolkit.UI.Nodes;

public static class TreeViewExtensions
{
    public static void AddNodesToTree(this TreeView tree, TreeNode nodes)
    {
        if (tree.InvokeRequired)
        {
            tree.Invoke(() =>
            {
                tree.Nodes.Add(nodes);
            });
        }
        else
        {
            tree.Nodes.Add(nodes);
        }
    }
}