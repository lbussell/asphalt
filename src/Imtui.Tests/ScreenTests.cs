// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

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
}
