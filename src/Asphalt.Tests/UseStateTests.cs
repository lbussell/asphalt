// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt.Tests;

[TestClass]
public class UseStateTests
{
    [TestMethod]
    public void SameId_ReturnsSameInstanceAcrossFrames()
    {
        AsphaltContext context = new AsphaltContext();

        context.BeginLayout(new Dimensions(10, 5));
        State<int> first = context.UseState("counter", 0);
        context.EndLayout();

        context.BeginLayout(new Dimensions(10, 5));
        State<int> second = context.UseState("counter", 0);
        context.EndLayout();

        Assert.AreSame(first, second);
    }

    [TestMethod]
    public void Mutation_PersistsAcrossFrames()
    {
        AsphaltContext context = new AsphaltContext();

        context.BeginLayout(new Dimensions(10, 5));
        context.UseState("counter", 0).Value = 42;
        context.EndLayout();

        context.BeginLayout(new Dimensions(10, 5));
        State<int> reread = context.UseState("counter", 0);
        context.EndLayout();

        Assert.AreEqual(42, reread.Value);
    }

    [TestMethod]
    public void FactoryOverload_NotInvokedOnSubsequentFrames()
    {
        AsphaltContext context = new AsphaltContext();
        int factoryCalls = 0;

        for (int frame = 0; frame < 3; frame++)
        {
            context.BeginLayout(new Dimensions(10, 5));
            context.UseState(
                "counter",
                () =>
                {
                    factoryCalls += 1;
                    return 7;
                }
            );
            context.EndLayout();
        }

        Assert.AreEqual(1, factoryCalls);
    }

    [TestMethod]
    public void IdNotRequested_StateIsPruned()
    {
        AsphaltContext context = new AsphaltContext();

        context.BeginLayout(new Dimensions(10, 5));
        context.UseState("counter", 0).Value = 99;
        context.EndLayout();

        // Frame that does not touch "counter" — state should be pruned.
        context.BeginLayout(new Dimensions(10, 5));
        context.EndLayout();

        context.BeginLayout(new Dimensions(10, 5));
        State<int> reborn = context.UseState("counter", 0);
        context.EndLayout();

        Assert.AreEqual(0, reborn.Value);
    }

    [TestMethod]
    public void TypeMismatch_Throws()
    {
        AsphaltContext context = new AsphaltContext();

        context.BeginLayout(new Dimensions(10, 5));
        context.UseState("shared", 0);

        Assert.ThrowsExactly<InvalidOperationException>(() => context.UseState("shared", "hello"));

        context.EndLayout();
    }

    [TestMethod]
    public void DifferentIds_TrackedIndependently()
    {
        AsphaltContext context = new AsphaltContext();

        context.BeginLayout(new Dimensions(10, 5));
        context.UseState("a", 0).Value = 1;
        context.UseState("b", 0).Value = 2;
        context.EndLayout();

        context.BeginLayout(new Dimensions(10, 5));
        Assert.AreEqual(1, context.UseState("a", 0).Value);
        Assert.AreEqual(2, context.UseState("b", 0).Value);
        context.EndLayout();
    }
}
