// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

using Imtui.Rendering;

public static class LayoutRenderer
{
    public static void Render(LayoutNode root, ICanvas canvas)
    {
        if (root.Widget is not null)
            root.Widget.Render(root.ContentRect, canvas);

        foreach (LayoutNode child in root.Children)
            Render(child, canvas);
    }
}
