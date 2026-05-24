// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt.Tests;

[TestClass]
public class RequestRedrawTests
{
    [TestMethod]
    public void NoRequest_NextScheduledRedrawIsNull()
    {
        AsphaltContext context = new AsphaltContext();
        context.BeginLayout(new Dimensions(80, 24));
        context.EndLayout();

        Assert.IsNull(context.NextScheduledRedraw);
    }

    [TestMethod]
    public void SingleRequest_NextScheduledRedrawMatches()
    {
        AsphaltContext context = new AsphaltContext();
        context.BeginLayout(new Dimensions(80, 24));
        context.RequestRedrawIn(TimeSpan.FromMilliseconds(250));
        context.EndLayout();

        Assert.AreEqual(TimeSpan.FromMilliseconds(250), context.NextScheduledRedraw);
    }

    [TestMethod]
    public void MultipleRequests_AggregateByMinimum()
    {
        AsphaltContext context = new AsphaltContext();
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
        AsphaltContext context = new AsphaltContext();
        context.BeginLayout(new Dimensions(80, 24));
        context.RequestRedrawIn(TimeSpan.FromMilliseconds(-50));
        context.EndLayout();

        Assert.AreEqual(TimeSpan.Zero, context.NextScheduledRedraw);
    }

    [TestMethod]
    public void RedrawRequest_ResetsBetweenFrames()
    {
        AsphaltContext context = new AsphaltContext();

        context.BeginLayout(new Dimensions(80, 24));
        context.RequestRedrawIn(TimeSpan.FromMilliseconds(100));
        context.EndLayout();
        Assert.AreEqual(TimeSpan.FromMilliseconds(100), context.NextScheduledRedraw);

        context.BeginLayout(new Dimensions(80, 24));
        context.EndLayout();
        Assert.IsNull(context.NextScheduledRedraw);
    }

    [TestMethod]
    public void ConsumedKey_SchedulesImmediateRedraw()
    {
        AsphaltContext context = new AsphaltContext();
        FrameInput input = new FrameInput(
            new ConsoleKeyInfo('a', ConsoleKey.A, false, false, false)
        );

        context.BeginLayout(new Dimensions(80, 24), input);
        context.ConsumeKeys(_ => true);
        context.EndLayout();
        context.EndFrame();

        Assert.AreEqual(TimeSpan.Zero, context.NextScheduledRedraw);
    }

    [TestMethod]
    public void UnconsumedKey_DoesNotScheduleRedraw()
    {
        AsphaltContext context = new AsphaltContext();
        FrameInput input = new FrameInput(
            new ConsoleKeyInfo('a', ConsoleKey.A, false, false, false)
        );

        context.BeginLayout(new Dimensions(80, 24), input);
        context.ConsumeKeys(_ => false);
        context.EndLayout();
        context.EndFrame();

        Assert.IsNull(context.NextScheduledRedraw);
    }
}
