// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt.Tests;

using Asphalt.Widgets;

[TestClass]
public class SpinnerTests
{
    [TestMethod]
    public void TimeZero_RendersFirstGlyph()
    {
        char[] glyphs = ['A', 'B', 'C'];
        LayoutNode root = AsphaltTestHarness.RunFrame(
            context =>
            {
                context.Spinner(glyphs: glyphs, frameDuration: TimeSpan.FromMilliseconds(100));
            },
            new Dimensions(10, 1),
            new FrameInput(Time: TimeSpan.Zero)
        );

        TextWidget.Implementation text =
            root.NodesWithWidget<TextWidget.Implementation>().Single().Widget
                as TextWidget.Implementation
            ?? throw new InvalidOperationException();
        Assert.AreEqual("A", text.Text);
    }

    [TestMethod]
    public void TimeAdvances_AdvancesGlyph()
    {
        char[] glyphs = ['A', 'B', 'C'];

        TextWidget.Implementation RenderAt(TimeSpan time)
        {
            LayoutNode root = AsphaltTestHarness.RunFrame(
                context =>
                {
                    context.Spinner(glyphs: glyphs, frameDuration: TimeSpan.FromMilliseconds(100));
                },
                new Dimensions(10, 1),
                new FrameInput(Time: time)
            );
            return (TextWidget.Implementation)
                root.NodesWithWidget<TextWidget.Implementation>().Single().Widget!;
        }

        Assert.AreEqual("A", RenderAt(TimeSpan.FromMilliseconds(0)).Text);
        Assert.AreEqual("A", RenderAt(TimeSpan.FromMilliseconds(99)).Text);
        Assert.AreEqual("B", RenderAt(TimeSpan.FromMilliseconds(100)).Text);
        Assert.AreEqual("C", RenderAt(TimeSpan.FromMilliseconds(250)).Text);
        Assert.AreEqual("A", RenderAt(TimeSpan.FromMilliseconds(300)).Text);
    }

    [TestMethod]
    public void RequestsRedrawAtNextGlyphBoundary()
    {
        AsphaltContext context = new AsphaltContext();
        context.BeginLayout(
            new Dimensions(10, 1),
            new FrameInput(Time: TimeSpan.FromMilliseconds(30))
        );
        context.Spinner(glyphs: ['A', 'B'], frameDuration: TimeSpan.FromMilliseconds(100));
        context.EndLayout();

        // At t=30ms with a 100ms frame, the next boundary is t=100ms ⇒
        // 70ms from now.
        Assert.AreEqual(TimeSpan.FromMilliseconds(70), context.NextScheduledRedraw);
    }

    [TestMethod]
    public void MultipleSpinnersWithSameDuration_PhaseLockedToSingleWakeUp()
    {
        AsphaltContext context = new AsphaltContext();
        context.BeginLayout(
            new Dimensions(10, 1),
            new FrameInput(Time: TimeSpan.FromMilliseconds(30))
        );
        context.Spinner(frameDuration: TimeSpan.FromMilliseconds(100));
        context.Spinner(frameDuration: TimeSpan.FromMilliseconds(100));
        context.Spinner(frameDuration: TimeSpan.FromMilliseconds(100));
        context.EndLayout();

        // All three request a wake-up at the same boundary; the min
        // aggregation collapses to one.
        Assert.AreEqual(TimeSpan.FromMilliseconds(70), context.NextScheduledRedraw);
    }

    [TestMethod]
    public void EmptyGlyphs_Throws()
    {
        AsphaltContext context = new AsphaltContext();
        context.BeginLayout(new Dimensions(10, 1));

        Assert.ThrowsExactly<ArgumentException>(() => context.Spinner(glyphs: Array.Empty<char>()));
    }

    [TestMethod]
    public void NonPositiveFrameDuration_Throws()
    {
        AsphaltContext context = new AsphaltContext();
        context.BeginLayout(new Dimensions(10, 1));

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            context.Spinner(frameDuration: TimeSpan.FromMilliseconds(-1))
        );
    }
}
