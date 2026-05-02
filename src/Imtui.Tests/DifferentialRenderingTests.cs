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
    [TestMethod]
    public void Property_Render_Identity()
    {
        GenScreen.Sample(screen => Render(screen, screen).Length == 0);
    }

    [TestMethod]
    public void Property_Render_Correctness()
    {
        GenTwoScreensSameSize.Sample((prev, next) => Apply(prev, Render(prev, next)) == next);
    }

    [TestMethod]
    public void Property_Render_Determinism()
    {
        GenTwoScreensSameSize.Sample(
            (prev, next) => Render(prev, next).SequenceEqual(Render(prev, next))
        );
    }

    [TestMethod]
    public void Property_Render_Composition()
    {
        GenThreeScreensSameSize.Sample(
            (a, b, c) => Apply(a, [.. Render(a, b), .. Render(b, c)]) == c
        );
    }
}
