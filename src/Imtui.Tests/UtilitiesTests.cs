// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: WTFPL

using CsCheck;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Imtui.Utilities;

namespace Imtui.Tests;

[TestClass]
public class UtilitiesTests
{
    [TestMethod]
    public void Property_Wrap_Min()
    {
        Gen.Select(Gen.Int.Uniform, Gen.Int.Uniform, Gen.Int.Uniform)
            .Select(
                (i1, i2, i3) =>
                {
                    int value = i1;
                    int min = Math.Min(i2, i3);
                    int max = Math.Max(i2, i3);
                    return (value, min, max);
                }
            )
            .Sample(
                (value, min, max) =>
                {
                    int result = Wrap(value, min, max);
                    return result >= min;
                }
            );
    }

    [TestMethod]
    public void Property_Wrap_Max()
    {
        Gen.Select(Gen.Int.Uniform, Gen.Int.Uniform, Gen.Int.Uniform)
            .Select(
                (i1, i2, i3) =>
                {
                    int value = i1;
                    int min = Math.Min(i2, i3);
                    int max = Math.Max(i2, i3);
                    return (value, min, max);
                }
            )
            .Sample(
                (value, min, max) =>
                {
                    int result = Wrap(value, min, max);
                    return result <= max;
                }
            );
    }
}
