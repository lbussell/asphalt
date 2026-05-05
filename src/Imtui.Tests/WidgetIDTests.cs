// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Imtui.Tests;

[TestClass]
public class WidgetIDTests
{
    [TestMethod]
    public void Hash_SameInputs_ReturnsSameId()
    {
        WidgetID a = WidgetID.Hash("Button", WidgetID.Root);
        WidgetID b = WidgetID.Hash("Button", WidgetID.Root);

        Assert.AreEqual(a, b);
    }

    [TestMethod]
    public void Hash_DifferentLabels_ReturnsDifferentIds()
    {
        WidgetID a = WidgetID.Hash("OK", WidgetID.Root);
        WidgetID b = WidgetID.Hash("Cancel", WidgetID.Root);

        Assert.AreNotEqual(a, b);
    }

    [TestMethod]
    public void Hash_DifferentSeeds_ReturnsDifferentIds()
    {
        WidgetID seed1 = WidgetID.Root;
        WidgetID seed2 = new WidgetID(42);

        WidgetID a = WidgetID.Hash("Button", seed1);
        WidgetID b = WidgetID.Hash("Button", seed2);

        Assert.AreNotEqual(a, b);
    }

    [TestMethod]
    public void Hash_EmptyLabel_ReturnsSeedTimesePrime()
    {
        WidgetID result = WidgetID.Hash("", WidgetID.Root);

        Assert.AreEqual(WidgetID.Root, result);
    }

    [TestMethod]
    public void Hash_IntOverload_IsStable()
    {
        WidgetID a = WidgetID.Hash(7, WidgetID.Root);
        WidgetID b = WidgetID.Hash(7, WidgetID.Root);

        Assert.AreEqual(a, b);
    }

    [TestMethod]
    public void Hash_DifferentInts_ReturnsDifferentIds()
    {
        WidgetID a = WidgetID.Hash(1, WidgetID.Root);
        WidgetID b = WidgetID.Hash(2, WidgetID.Root);

        Assert.AreNotEqual(a, b);
    }

    [TestMethod]
    public void Hash_IntAndString_ProduceDifferentIds()
    {
        WidgetID fromString = WidgetID.Hash("1", WidgetID.Root);
        WidgetID fromInt = WidgetID.Hash(1, WidgetID.Root);

        Assert.AreNotEqual(fromString, fromInt);
    }
}
