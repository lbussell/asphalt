// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: WTFPL

using System.Text;
using Imtui.Rendering;
using Imtui.Widgets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Size = Imtui.Rendering.Size;

namespace Imtui.Tests;

[TestClass]
public class PanelWidgetTests
{
    private static readonly Color s_titleBackground = Color.Palette256(31);
    private static readonly Color s_contentBackground = Color.Palette256(235);
    private static readonly Color s_panelForeground = Color.Palette256(231);

    [TestMethod]
    public void Panel_EmptyPanelRendersTitleBarOnly()
    {
        ImtuiContext context = CreateContext();

        context.Panel("Hi", _ => { });

        // Width = len("Hi") + 2 padding = 4. Single row title bar.
        AssertCell(context, 0, 0, ' ', s_panelForeground, s_titleBackground);
        AssertCell(context, 1, 0, 'H', s_panelForeground, s_titleBackground);
        AssertCell(context, 2, 0, 'i', s_panelForeground, s_titleBackground);
        AssertCell(context, 3, 0, ' ', s_panelForeground, s_titleBackground);
        // Beyond the panel width should remain empty.
        Assert.AreEqual(Cell.Empty, context.CurrentScreen[new CellPosition(4, 0)]);
        // Below the title bar should also be empty.
        Assert.AreEqual(Cell.Empty, context.CurrentScreen[new CellPosition(0, 1)]);
    }

    [TestMethod]
    public void Panel_WithChildRendersTitleBarAndContentRow()
    {
        ImtuiContext context = CreateContext();

        context.Panel("T", body => body.Text("hi"));

        // Title bar row (y=0): width = max(len("T")+2, len("hi")+2) = 4.
        AssertCell(context, 0, 0, ' ', s_panelForeground, s_titleBackground);
        AssertCell(context, 1, 0, 'T', s_panelForeground, s_titleBackground);
        AssertCell(context, 2, 0, ' ', s_panelForeground, s_titleBackground);
        AssertCell(context, 3, 0, ' ', s_panelForeground, s_titleBackground);

        // Content row (y=1): "hi" inside 1-cell horizontal padding, so it
        // lands at x=1..2. The panel's foreground/background overlay applies
        // to cells the children wrote with default colors.
        AssertCell(context, 0, 1, ' ', s_panelForeground, s_contentBackground);
        AssertCell(context, 1, 1, 'h', s_panelForeground, s_contentBackground);
        AssertCell(context, 2, 1, 'i', s_panelForeground, s_contentBackground);
        AssertCell(context, 3, 1, ' ', s_panelForeground, s_contentBackground);
    }

    [TestMethod]
    public void Panel_TitleWiderThanContent_ExtendsContentRowWithBackground()
    {
        ImtuiContext context = CreateContext();

        context.Panel("Settings", body => body.Text("a"));

        // Title bar width = len("Settings") + 2 = 10. Content "a" + padding
        // would be 3, so the content row pads rightward to width 10 with
        // the content background.
        for (int x = 0; x < 10; x++)
            AssertBackground(context, x, 0, s_titleBackground);

        for (int x = 0; x < 10; x++)
            AssertBackground(context, x, 1, s_contentBackground);

        // Beyond panel width remains empty.
        Assert.AreEqual(Cell.Empty, context.CurrentScreen[new CellPosition(10, 0)]);
        Assert.AreEqual(Cell.Empty, context.CurrentScreen[new CellPosition(10, 1)]);
    }

    [TestMethod]
    public void Panel_ContentWiderThanTitle_ExtendsTitleBarWithBackground()
    {
        ImtuiContext context = CreateContext();

        context.Panel("X", body => body.Text("Hello World"));

        // Content "Hello World" + 2 padding = 13. Title "X" + 2 = 3. Panel
        // width should be 13; the title bar fills the full width with bg=31.
        for (int x = 0; x < 13; x++)
            AssertBackground(context, x, 0, s_titleBackground);

        for (int x = 0; x < 13; x++)
            AssertBackground(context, x, 1, s_contentBackground);

        Assert.AreEqual(Cell.Empty, context.CurrentScreen[new CellPosition(13, 0)]);
    }

    [TestMethod]
    public void Panel_AdvancesParentCursorPastPanel()
    {
        ImtuiContext context = CreateContext();

        context.Panel("T", body => body.Text("hi"));
        context.Text("AFTER");

        // Panel is 2 rows tall (title + 1 content row); the next widget
        // should land on row 2.
        AssertCell(context, 0, 2, 'A');
    }

    [TestMethod]
    public void Panel_PreservesExplicitChildForeground_OverlaysOnlyDefaultBackground()
    {
        ImtuiContext context = CreateContext();

        context.Panel("T", body => body.Submit(new RedTextWidget("R")));

        // The child sets fg=Red and leaves bg=Default. Per-channel overlay
        // should keep Red as fg and apply the panel's content background.
        Cell cell = context.CurrentScreen[new CellPosition(1, 1)];
        Assert.AreEqual(new Rune('R'), cell.Glyph);
        Assert.AreEqual(Color.Ansi(AnsiColor.Red), cell.Style.Foreground);
        Assert.AreEqual(s_contentBackground, cell.Style.Background);
    }

    private readonly record struct RedTextWidget(string Text) : IWidget
    {
        public void Execute(ImtuiContext context)
        {
            CellPosition position = context.AllocateWidgetPosition();
            CellStyle style = new(Color.Ansi(AnsiColor.Red), Color.Default);
            int x = position.X;
            foreach (Rune glyph in Text.EnumerateRunes())
            {
                context.WriteCell(new CellPosition(x, position.Y), new Cell(glyph, style));
                x++;
            }
        }
    }

    private static ImtuiContext CreateContext()
    {
        ImtuiContext context = new();
        context.NewFrame(new Size(20, 6));
        return context;
    }

    private static void AssertCell(
        ImtuiContext context,
        int x,
        int y,
        char expected,
        Color foreground,
        Color background
    )
    {
        Cell cell = context.CurrentScreen[new CellPosition(x, y)];
        Assert.AreEqual(new Rune(expected), cell.Glyph, $"Glyph at ({x},{y})");
        Assert.AreEqual(foreground, cell.Style.Foreground, $"Foreground at ({x},{y})");
        Assert.AreEqual(background, cell.Style.Background, $"Background at ({x},{y})");
    }

    private static void AssertCell(ImtuiContext context, int x, int y, char expected)
    {
        Cell cell = context.CurrentScreen[new CellPosition(x, y)];
        Assert.AreEqual(new Rune(expected), cell.Glyph, $"Glyph at ({x},{y})");
    }

    private static void AssertBackground(ImtuiContext context, int x, int y, Color background)
    {
        Cell cell = context.CurrentScreen[new CellPosition(x, y)];
        Assert.AreEqual(background, cell.Style.Background, $"Background at ({x},{y})");
    }
}
