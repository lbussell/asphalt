// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImtuiLib.Tests;

[TestClass]
public class ImtuiLibTests
{
    [TestMethod]
    public void ImtuiLib_CanBeInstantiated()
    {
        Assert.IsNotNull(typeof(Imtui));
    }
}
