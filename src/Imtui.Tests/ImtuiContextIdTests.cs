// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Size = Imtui.Rendering.Size;

namespace Imtui.Tests;

[TestClass]
public class ImtuiContextIdTests
{
    private sealed class TestWidgetState
    {
        public int Value { get; set; }
    }

    private static ImtuiContext CreateContext(int width = 10, int height = 5)
    {
        ImtuiContext ctx = new();
        ctx.NewFrame(new Size(width, height));
        return ctx;
    }

    [TestMethod]
    public void GetId_SameLabel_ReturnsSameId()
    {
        ImtuiContext ctx = CreateContext();

        WidgetID a = ctx.GetId("OK");
        WidgetID b = ctx.GetId("OK");

        Assert.AreEqual(a, b);
    }

    [TestMethod]
    public void GetId_DifferentLabels_ReturnsDifferentIds()
    {
        ImtuiContext ctx = CreateContext();

        WidgetID a = ctx.GetId("OK");
        WidgetID b = ctx.GetId("Cancel");

        Assert.AreNotEqual(a, b);
    }

    [TestMethod]
    public void GetId_IntOverload_Works()
    {
        ImtuiContext ctx = CreateContext();

        WidgetID a = ctx.GetId(0);
        WidgetID b = ctx.GetId(1);

        Assert.AreNotEqual(a, b);
    }

    [TestMethod]
    public void PushId_ChangesScopeForSameLabel()
    {
        ImtuiContext ctx = CreateContext();

        WidgetID before = ctx.GetId("Button");
        ctx.PushId("Panel1");
        WidgetID scoped = ctx.GetId("Button");
        ctx.PopId();

        Assert.AreNotEqual(before, scoped);
    }

    [TestMethod]
    public void PopId_RestoresPreviousScope()
    {
        ImtuiContext ctx = CreateContext();

        WidgetID before = ctx.GetId("Button");
        ctx.PushId("Panel1");
        ctx.PopId();
        WidgetID after = ctx.GetId("Button");

        Assert.AreEqual(before, after);
    }

    [TestMethod]
    public void PopId_AtRoot_Throws()
    {
        ImtuiContext ctx = CreateContext();

        Assert.ThrowsExactly<InvalidOperationException>(() => ctx.PopId());
    }

    [TestMethod]
    public void PushId_NestedScopes_ProduceDifferentIds()
    {
        ImtuiContext ctx = CreateContext();

        ctx.PushId("A");
        WidgetID inA = ctx.GetId("Button");
        ctx.PushId("B");
        WidgetID inAB = ctx.GetId("Button");
        ctx.PopId();
        ctx.PopId();

        Assert.AreNotEqual(inA, inAB);
    }

    [TestMethod]
    public void PushId_DifferentScopes_SameLabel_DifferentIds()
    {
        ImtuiContext ctx = CreateContext();

        ctx.PushId("Panel1");
        WidgetID id1 = ctx.GetId("Button");
        ctx.PopId();

        ctx.PushId("Panel2");
        WidgetID id2 = ctx.GetId("Button");
        ctx.PopId();

        Assert.AreNotEqual(id1, id2);
    }

    [TestMethod]
    public void NewFrame_ResetsIdStack()
    {
        ImtuiContext ctx = CreateContext();

        WidgetID before = ctx.GetId("X");
        ctx.NewFrame(new Size(10, 5));
        WidgetID after = ctx.GetId("X");

        Assert.AreEqual(before, after);
    }

    [TestMethod]
    public void GetWidgetState_PersistsAcrossFrames()
    {
        ImtuiContext ctx = CreateContext();
        WidgetID id = ctx.GetId("Widget");

        TestWidgetState state = ctx.GetWidgetState<TestWidgetState>(id);
        state.Value = 42;

        ctx.NewFrame(new Size(10, 5));

        TestWidgetState retrieved = ctx.GetWidgetState<TestWidgetState>(id);

        Assert.AreEqual(42, retrieved.Value);
    }

    [TestMethod]
    public void GetWidgetState_ReturnsSameInstance()
    {
        ImtuiContext ctx = CreateContext();
        WidgetID id = ctx.GetId("Widget");

        TestWidgetState first = ctx.GetWidgetState<TestWidgetState>(id);
        TestWidgetState second = ctx.GetWidgetState<TestWidgetState>(id);

        Assert.AreSame(first, second);
    }
}
