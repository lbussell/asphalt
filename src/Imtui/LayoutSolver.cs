// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

public static class LayoutSolver
{
    public static LayoutNode Solve(Node root, Rect area) => SolveInternal(root, area);

    private static LayoutNode SolveInternal(Node node, Rect area)
    {
        int numChildren = node.Children.Count;
        if (numChildren == 0)
            return new LayoutNode(area, node.Widget, Array.Empty<LayoutNode>());

        bool horizontal = node.Direction == Direction.Horizontal;
        int total = horizontal ? area.Dimensions.Width : area.Dimensions.Height;
        int size = total / numChildren;
        int remainder = total - size * numChildren;

        List<LayoutNode> solvedChildren = new List<LayoutNode>(numChildren);
        int offset = horizontal ? area.Position.X : area.Position.Y;
        for (int i = 0; i < numChildren; i++)
        {
            int slice = size + (i < remainder ? 1 : 0);
            Rect childArea = horizontal
                ? new Rect(new(offset, area.Position.Y), new(slice, area.Dimensions.Height))
                : new Rect(new(area.Position.X, offset), new(area.Dimensions.Width, slice));
            solvedChildren.Add(SolveInternal(node.Children[i], childArea));
            offset += slice;
        }

        return new LayoutNode(area, node.Widget, solvedChildren);
    }
}
