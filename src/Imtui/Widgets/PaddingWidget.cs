// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: WTFPL

using Imtui.Rendering;

namespace Imtui.Widgets;

public static class PaddingWidgetExtensions
{
    extension(ImtuiContext context)
    {
        public void Padding(Padding padding, Action<ImtuiContext> children)
        {
            ArgumentNullException.ThrowIfNull(children);
            PaddingWidget widget = new(padding, children);
            context.Submit(widget);
        }

        public void Padding(int amount, Action<ImtuiContext> children) =>
            context.Padding(Imtui.Padding.Uniform(amount), children);

        public void Padding(int horizontal, int vertical, Action<ImtuiContext> children) =>
            context.Padding(Imtui.Padding.Symmetric(horizontal, vertical), children);
    }
}

internal readonly record struct PaddingWidget(Padding Padding, Action<ImtuiContext> Children)
    : IWidget
{
    public void Execute(ImtuiContext context)
    {
        CellPosition origin = context.AllocateWidgetPosition();
        int innerOriginX = origin.X + Padding.Left;
        int innerOriginY = origin.Y + Padding.Top;

        context.PushLayoutFrame(innerOriginX, innerOriginY, LayoutDirection.Vertical);
        Children(context);
        LayoutMeasurement inner = context.PopLayoutFrame();

        // Padding is purely a layout shifter and draws nothing itself. When
        // children produced no cells, the padded region has no extent either.
        if (inner.Width == 0 && inner.Height == 0)
            return;

        // Claim the right-and-bottom padding cells in the parent frame's
        // bounding box so subsequent layout sees the full padded extent.
        int paddedRight = inner.OriginX + inner.Width + Padding.Right;
        int paddedBottom = inner.OriginY + inner.Height + Padding.Bottom;
        context.MarkPosition(new CellPosition(paddedRight - 1, paddedBottom - 1));
    }
}
