// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt.Tests.Widgets;

using Asphalt.Rendering;
using Asphalt.Widgets;

[TestClass]
public class ProgressBarTests
{
    private const char FullBlock = '\u2588';
    private const char LeftHalfBlock = '\u258C';

    private static readonly TerminalColor s_fill = TerminalColor.Green;
    private static readonly TerminalColor s_track = TerminalColor.Palette(235);

    private static TerminalCanvas Render(float progress, int width, int height = 1)
    {
        TerminalCanvas canvas = new TerminalCanvas(new Dimensions(width, height));
        ProgressBarWidget.Implementation widget = new ProgressBarWidget.Implementation(
            progress,
            s_fill,
            s_track
        );
        widget.Render(new Rect(new Position(0, 0), new Dimensions(width, height)), canvas);
        return canvas;
    }

    [TestMethod]
    public void Measure_MinimumIsOneByOne()
    {
        ProgressBarWidget.Implementation widget = new ProgressBarWidget.Implementation(0.5f);
        Assert.AreEqual(new Dimensions(1, 1), widget.Measure().Minimum);
    }

    [TestMethod]
    public void ZeroProgress_AllCellsAreTrack()
    {
        TerminalCanvas canvas = Render(0f, width: 10);

        for (int x = 0; x < 10; x++)
        {
            Assert.AreEqual(' ', canvas.GetCell(x, 0).CharacterOrSpace);
            Assert.AreEqual(s_track, canvas.GetCell(x, 0).BackgroundColor);
        }
    }

    [TestMethod]
    public void FullProgress_AllCellsAreFullBlocks()
    {
        TerminalCanvas canvas = Render(1f, width: 10);

        for (int x = 0; x < 10; x++)
        {
            Assert.AreEqual(FullBlock, canvas.GetCell(x, 0).CharacterOrSpace);
            Assert.AreEqual(s_fill, canvas.GetCell(x, 0).ForegroundColor);
        }
    }

    [TestMethod]
    public void HalfProgress_FillsLeftHalfWithFullBlocks()
    {
        TerminalCanvas canvas = Render(0.5f, width: 10);

        for (int x = 0; x < 5; x++)
            Assert.AreEqual(FullBlock, canvas.GetCell(x, 0).CharacterOrSpace);

        for (int x = 5; x < 10; x++)
            Assert.AreEqual(' ', canvas.GetCell(x, 0).CharacterOrSpace);
    }

    [TestMethod]
    public void FractionalCell_RendersEighthBlockGlyph()
    {
        // A single-cell bar at 50% has no whole cells; the leading edge is a
        // four-eighths (left half) block drawn in fill over the track.
        TerminalCanvas canvas = Render(0.5f, width: 1);

        TerminalCell cell = canvas.GetCell(0, 0);
        Assert.AreEqual(LeftHalfBlock, cell.CharacterOrSpace);
        Assert.AreEqual(s_fill, cell.ForegroundColor);
        Assert.AreEqual(s_track, cell.BackgroundColor);
    }

    [TestMethod]
    public void TallBar_FillsEveryRowIdentically()
    {
        TerminalCanvas canvas = Render(0.5f, width: 4, height: 3);

        for (int y = 0; y < 3; y++)
        {
            Assert.AreEqual(FullBlock, canvas.GetCell(0, y).CharacterOrSpace);
            Assert.AreEqual(FullBlock, canvas.GetCell(1, y).CharacterOrSpace);
            Assert.AreEqual(' ', canvas.GetCell(2, y).CharacterOrSpace);
            Assert.AreEqual(' ', canvas.GetCell(3, y).CharacterOrSpace);
        }
    }

    [TestMethod]
    public void ProgressOutsideRange_IsClamped()
    {
        Assert.AreEqual(0f, new ProgressBarWidget.Implementation(-1f).Progress);
        Assert.AreEqual(1f, new ProgressBarWidget.Implementation(2f).Progress);
        Assert.AreEqual(0f, new ProgressBarWidget.Implementation(float.NaN).Progress);
    }

    [TestMethod]
    public void ProgressBar_IsNotFocusable()
    {
        // A ProgressBar must not register a focusable, so a button declared
        // alongside it receives initial focus and activates on Enter. The UI
        // is built from a single call site so the button keeps a stable id
        // across frames.
        AsphaltContext context = new AsphaltContext();
        Dimensions dimensions = new Dimensions(20, 3);
        bool activated = false;

        void Frame()
        {
            context.ProgressBar(0.5f);
            activated = context.Button("Go");
        }

        context.BeginLayout(dimensions);
        Frame();
        context.EndLayout();

        ConsoleKeyInfo enter = new ConsoleKeyInfo(
            '\0',
            ConsoleKey.Enter,
            shift: false,
            alt: false,
            control: false
        );
        context.BeginLayout(dimensions, new FrameInput([enter]));
        Frame();
        context.EndLayout();

        Assert.IsTrue(activated);
    }
}
