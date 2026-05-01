// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using CsCheck;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Imtui.Tests;

[TestClass]
public class DifferentialRenderingTests
{
    [TestMethod]
    public void Property_Render_Identity()
    {
        Generators.GenScreen.Sample(screen =>
        {
            TermOp[] ops = DifferentialRendering.Render(screen, screen);
            return ops.Length == 0;
        });
    }
}
