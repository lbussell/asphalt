// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt.Tests;

// Helpers for navigating a LayoutNode tree produced by the test harness. Tests
// typically run a frame, then use these to locate specific nodes and assert on
// their dimensions, positions, or relationships.
public static class LayoutTree
{
    // Enumerate the root and all of its descendants in pre-order.
    public static IEnumerable<LayoutNode> Walk(this LayoutNode root)
    {
        ArgumentNullException.ThrowIfNull(root);

        Stack<LayoutNode> stack = new Stack<LayoutNode>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            LayoutNode current = stack.Pop();
            yield return current;

            for (int index = current.Children.Count - 1; index >= 0; index--)
                stack.Push(current.Children[index]);
        }
    }

    // Return every node in the tree whose widget is of the requested type.
    public static IEnumerable<LayoutNode> NodesWithWidget<TWidget>(this LayoutNode root)
        where TWidget : IWidget => root.Walk().Where(node => node.Widget is TWidget);

    // Find the single node whose widget is of the requested type. Throws when
    // the tree contains zero or more than one matching node.
    public static LayoutNode SingleNodeWithWidget<TWidget>(this LayoutNode root)
        where TWidget : IWidget => root.NodesWithWidget<TWidget>().Single();
}
