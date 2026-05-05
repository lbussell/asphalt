// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: WTFPL

using System.Text;
using Imtui.Rendering;
using Imtui.Widgets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Size = Imtui.Rendering.Size;

namespace Imtui.Tests;

[TestClass]
public class PaddingWidgetTests
{
    [TestMethod]
    public void Padding_ShiftsChildrenByLeftAndTop()
    {
        ImtuiContext context = CreateContext();

        context.Padding(
            new Padding(Left: 2, Right: 0, Top: 1, Bottom: 0),
            padded => padded.Text("hi")
        );

        // "hi" should land at column 2, row 1 (shifted right 2, down 1).
        AssertCell(context, 2, 1, 'h');
        AssertCell(context, 3, 1, 'i');
        // Cells the children skipped should remain empty.
        AssertCell(context, 0, 0, ' ');
        AssertCell(context, 1, 0, ' ');
        AssertCell(context, 0, 1, ' ');
        AssertCell(context, 1, 1, ' ');
    }

    [TestMethod]
    public void Padding_SymmetricShiftsHorizontallyOnly()
    {
        ImtuiContext context = CreateContext();

        context.Padding(horizontal: 2, vertical: 0, padded => padded.Text("hi"));

        AssertCell(context, 2, 0, 'h');
        AssertCell(context, 3, 0, 'i');
    }

    [TestMethod]
    public void Padding_AdvancesParentCursorPastPaddedRegion()
    {
        ImtuiContext context = CreateContext();

        context.Padding(1, padded => padded.Text("xy"));
        context.Text("AFTER");

        // Padded "xy" occupies (1,1)-(2,1), so total padded extent is rows
        // 0..3 (top+content+bottom). The next widget should land on row 3.
        AssertCell(context, 1, 1, 'x');
        AssertCell(context, 2, 1, 'y');
        AssertCell(context, 0, 3, 'A');
    }

    [TestMethod]
    public void Padding_WithNoChildrenConsumesNoSpace()
    {
        ImtuiContext context = CreateContext();

        context.Padding(2, _ => { });
        context.Text("A");

        // Empty padding should not advance the parent cursor; "A" must
        // render at row 0.
        AssertCell(context, 0, 0, 'A');
    }

    [TestMethod]
    public void Padding_NestsCorrectly()
    {
        ImtuiContext context = CreateContext();

        context.Padding(1, outer => outer.Padding(2, inner => inner.Text("z")));

        // Outer shifts by 1, inner adds 2 more horizontally and 2 more
        // vertically, so "z" lands at (3, 3).
        AssertCell(context, 3, 3, 'z');
    }

    private static ImtuiContext CreateContext()
    {
        ImtuiContext context = new();
        context.NewFrame(new Size(20, 8));
        return context;
    }

    private static void AssertCell(ImtuiContext context, int x, int y, char expected)
    {
        Assert.AreEqual(
            new Rune(expected),
            context.CurrentScreen[new CellPosition(x, y)].Glyph,
            $"Expected '{expected}' at ({x}, {y})."
        );
    }
}
