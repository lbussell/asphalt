// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Tests;

[TestClass]
public class RequestRedrawTests
{
    [TestMethod]
    public void NoRequest_NextScheduledRedrawIsNull()
    {
        ImtuiContext context = new ImtuiContext();
        context.BeginLayout(new Dimensions(80, 24));
        context.EndLayout();

        Assert.IsNull(context.NextScheduledRedraw);
    }

    [TestMethod]
    public void SingleRequest_NextScheduledRedrawMatches()
    {
        ImtuiContext context = new ImtuiContext();
        context.BeginLayout(new Dimensions(80, 24));
        context.RequestRedrawIn(TimeSpan.FromMilliseconds(250));
        context.EndLayout();

        Assert.AreEqual(TimeSpan.FromMilliseconds(250), context.NextScheduledRedraw);
    }

    [TestMethod]
    public void MultipleRequests_AggregateByMinimum()
    {
        ImtuiContext context = new ImtuiContext();
        context.BeginLayout(new Dimensions(80, 24));
        context.RequestRedrawIn(TimeSpan.FromMilliseconds(500));
        context.RequestRedrawIn(TimeSpan.FromMilliseconds(100));
        context.RequestRedrawIn(TimeSpan.FromMilliseconds(250));
        context.EndLayout();

        Assert.AreEqual(TimeSpan.FromMilliseconds(100), context.NextScheduledRedraw);
    }

    [TestMethod]
    public void NegativeDelay_ClampedToZero()
    {
        ImtuiContext context = new ImtuiContext();
        context.BeginLayout(new Dimensions(80, 24));
        context.RequestRedrawIn(TimeSpan.FromMilliseconds(-50));
        context.EndLayout();

        Assert.AreEqual(TimeSpan.Zero, context.NextScheduledRedraw);
    }

    [TestMethod]
    public void RedrawRequest_ResetsBetweenFrames()
    {
        ImtuiContext context = new ImtuiContext();

        context.BeginLayout(new Dimensions(80, 24));
        context.RequestRedrawIn(TimeSpan.FromMilliseconds(100));
        context.EndLayout();
        Assert.AreEqual(TimeSpan.FromMilliseconds(100), context.NextScheduledRedraw);

        context.BeginLayout(new Dimensions(80, 24));
        context.EndLayout();
        Assert.IsNull(context.NextScheduledRedraw);
    }
}
