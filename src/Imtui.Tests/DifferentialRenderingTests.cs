// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using CsCheck;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Imtui.DifferentialRendering;
using static Imtui.Tests.Generators;

namespace Imtui.Tests;

[TestClass]
public class DifferentialRenderingTests
{
    // ∀S Render(S,S) == []
    [TestMethod]
    public void Property_Render_Identity()
    {
        GenScreen.Sample(screen =>
        {
            TermOp[] ops = Render(screen, screen);
            return ops.Length == 0;
        });
    }

    [TestMethod]
    public void Property_Render_Correctness()
    {
        // Generate a random size
        GenSize
            .SelectMany(randomSize =>
                // For each random size, generate two
                // random screens that are the same size
                Gen.Select(GenScreenOfSize(randomSize), GenScreenOfSize(randomSize))
            )
            // Assertion: applying the transforms that we generated actually
            // results in the transformation that we expected.
            .Sample((prev, next) => Apply(prev, Render(prev, next)).Equals(next));
    }
}
