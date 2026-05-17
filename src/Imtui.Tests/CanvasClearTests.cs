// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Tests;

using Imtui.Rendering;
using Imtui.Widgets;

[TestClass]
public class CanvasClearTests
{
    [TestMethod]
    public void ShrinkingWidgetAcrossFrames_LeavesNoStaleCharacters()
    {
        // Frame 1 renders a wide piece of text, Frame 2 renders a narrower
        // one in the same position. Without canvas clearing, the cells past
        // the new text would keep the previous frame's characters.
        Dimensions dimensions = new Dimensions(20, 3);
        TerminalCanvas canvas = new TerminalCanvas(dimensions);
        ImtuiContext context = new ImtuiContext();

        RenderFrame(context, canvas, "loading...");
        AssertRowStartsWith(canvas, row: 0, expected: "loading...");

        // Mimic what ImtuiApplication.RunAsync does between frames.
        canvas.Clear();

        RenderFrame(context, canvas, "ok");
        AssertRowStartsWith(canvas, row: 0, expected: "ok");

        // Cells 2..9 used to hold "ading..." and must now be blank.
        for (int x = 2; x < 10; x++)
        {
            char actual = canvas.GetCell(x, 0).CharacterOrSpace;
            Assert.AreEqual(' ', actual, $"Stale character at column {x}: '{actual}'");
        }
    }

    [TestMethod]
    public void Clear_OnlyAffectsCells_NotDimensions()
    {
        TerminalCanvas canvas = new TerminalCanvas(new Dimensions(10, 5));
        canvas.Draw(new Position(0, 0), 'X');
        canvas.Clear();

        Assert.AreEqual(new Dimensions(10, 5), canvas.Dimensions);
        Assert.AreEqual(' ', canvas.GetCell(0, 0).CharacterOrSpace);
    }

    private static void RenderFrame(ImtuiContext context, TerminalCanvas canvas, string text)
    {
        context.BeginLayout(canvas.Dimensions);
        context.Text(text);
        LayoutNode root = context.EndLayout();
        LayoutRenderer.Render(root, canvas);
    }

    private static void AssertRowStartsWith(TerminalCanvas canvas, int row, string expected)
    {
        for (int x = 0; x < expected.Length; x++)
        {
            char actual = canvas.GetCell(x, row).CharacterOrSpace;
            Assert.AreEqual(
                expected[x],
                actual,
                $"At column {x}: expected '{expected[x]}', got '{actual}'"
            );
        }
    }
}
