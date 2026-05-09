// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

public static class Renderer
{
    public static void Render(LayoutNode node, ICanvas canvas)
    {
        // If a node defines a widget, then render it within its bounds.
        node.Widget?.Render(node.Bounds, canvas);

        // Then render all of its children.
        foreach (LayoutNode child in node.Children)
            Render(child, canvas);
    }
}
