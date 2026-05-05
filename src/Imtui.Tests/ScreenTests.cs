// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: WTFPL

using System.Text;
using Imtui.Rendering;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Imtui.Tests;

[TestClass]
public class ScreenTests
{
    [TestMethod]
    public void Constructor_FillsCellsWithEmptyCells()
    {
        Screen screen = new(new Size(2, 2));

        CollectionAssert.AreEqual(
            new[] { Cell.Empty, Cell.Empty, Cell.Empty, Cell.Empty },
            screen.Cells
        );
    }

    [TestMethod]
    public void Indexer_Get_ReturnsCorrectCell()
    {
        Cell expected = new(new Rune('X'), default);
        Screen screen = new(
            new Size(3, 2),
            [Cell.Empty, Cell.Empty, Cell.Empty, Cell.Empty, expected, Cell.Empty]
        );

        Assert.AreEqual(expected, screen[new CellPosition(1, 1)]);
    }

    [TestMethod]
    public void Indexer_Set_WritesCorrectCell()
    {
        Screen screen = new(new Size(3, 2));
        Cell cell = new(new Rune('Z'), default);

        screen[new CellPosition(2, 0)] = cell;

        Assert.AreEqual(cell, screen[new CellPosition(2, 0)]);
    }

    [TestMethod]
    public void WriteText_WritesGlyphsAtPosition()
    {
        Screen screen = new(new Size(5, 1));
        CellStyle style = new(Color.Ansi(AnsiColor.Red), Color.Default);

        screen.WriteText(new CellPosition(1, 0), "Hi", style);

        Assert.AreEqual(Cell.Empty, screen[new CellPosition(0, 0)]);
        Assert.AreEqual(new Cell(new Rune('H'), style), screen[new CellPosition(1, 0)]);
        Assert.AreEqual(new Cell(new Rune('i'), style), screen[new CellPosition(2, 0)]);
        Assert.AreEqual(Cell.Empty, screen[new CellPosition(3, 0)]);
    }
}
