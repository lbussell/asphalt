// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: WTFPL

using System.Text;
using Imtui.Rendering;

namespace Imtui.Widgets;

public static class PanelWidgetExtensions
{
    extension(ImtuiContext context)
    {
        public void Panel(string title, Action<ImtuiContext> children)
        {
            ArgumentNullException.ThrowIfNull(title);
            ArgumentNullException.ThrowIfNull(children);
            WidgetID id = context.GetId(title);
            PanelWidget widget = new(id, title, children);
            context.Submit(widget);
        }
    }
}

internal readonly record struct PanelWidget(
    WidgetID ID,
    string Title,
    Action<ImtuiContext> Children
) : IWidget
{
    private static readonly Color s_titleBackground = Color.Palette256(31);
    private static readonly Color s_contentBackground = Color.Palette256(235);
    private static readonly Color s_panelForeground = Color.Palette256(231);
    private const int TitleBarPadding = 1;
    private const int ContentHorizontalPadding = 1;
    private const int ContentVerticalPadding = 0;

    public WidgetID ID { get; } = ID;

    public void Execute(ImtuiContext context)
    {
        CellPosition origin = context.AllocateWidgetPosition();
        int titleBarRow = origin.Y;
        int contentOriginY = titleBarRow + 1;

        // Render children inside a layout frame whose default style supplies
        // the panel's foreground/background colors. The internal Padding
        // widget then provides the 1-cell horizontal margin around content.
        CellStyle contentStyle = new(s_panelForeground, s_contentBackground);
        context.PushLayoutFrame(origin.X, contentOriginY, LayoutDirection.Vertical, contentStyle);
        context.Padding(
            horizontal: ContentHorizontalPadding,
            vertical: ContentVerticalPadding,
            Children
        );
        LayoutMeasurement content = context.PopLayoutFrame();

        // Compute the panel's outer width: title bar fits the title plus its
        // 1-cell horizontal padding; content row fits children plus their
        // padding (already included in content.Width). The panel grows to
        // whichever is wider.
        int titleWidth = CountRunes(Title);
        int titleBarWidth = titleWidth + 2 * TitleBarPadding;
        int panelWidth = Math.Max(titleBarWidth, content.Width);

        DrawTitleBar(context, origin.X, titleBarRow, panelWidth);
        FillContentBackground(context, origin.X, contentOriginY, panelWidth, content.Height);
    }

    private void DrawTitleBar(ImtuiContext context, int originX, int rowY, int panelWidth)
    {
        CellStyle barStyle = new(s_panelForeground, s_titleBackground);
        Cell barCell = new(new Rune(' '), barStyle);
        for (int x = 0; x < panelWidth; x++)
            context.WriteCell(new CellPosition(originX + x, rowY), barCell);

        int titleX = originX + TitleBarPadding;
        foreach (Rune glyph in Title.EnumerateRunes())
        {
            context.WriteCell(new CellPosition(titleX, rowY), new Cell(glyph, barStyle));
            titleX++;
        }
    }

    private static void FillContentBackground(
        ImtuiContext context,
        int originX,
        int originY,
        int panelWidth,
        int contentHeight
    )
    {
        if (contentHeight == 0)
            return;

        // Fill cells in the content rect that the children did not write
        // (still Cell.Empty in the screen). Children-written cells already
        // inherited the panel's background via the per-channel default-style
        // overlay, so they don't need to be re-filled.
        Cell fillCell = new(new Rune(' '), new CellStyle(s_panelForeground, s_contentBackground));
        Screen screen = context.CurrentScreen;
        for (int y = originY; y < originY + contentHeight; y++)
        {
            for (int x = originX; x < originX + panelWidth; x++)
            {
                CellPosition position = new(x, y);
                if (!screen.IsInBounds(position))
                    continue;
                if (screen[position] == Cell.Empty)
                    context.WriteCell(position, fillCell);
            }
        }
    }

    private static int CountRunes(string text)
    {
        int count = 0;
        foreach (Rune _ in text.EnumerateRunes())
            count++;
        return count;
    }
}
