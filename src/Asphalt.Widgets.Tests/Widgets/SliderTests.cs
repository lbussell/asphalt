// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt.Tests.Widgets;

using Asphalt.Widgets;

[TestClass]
public class SliderTests
{
    private static readonly Dimensions s_terminalDimensions = new Dimensions(40, 5);

    private static FrameInput Frame(params ConsoleKeyInfo[] keys) =>
        new FrameInput(keys.Length == 0 ? [] : keys);

    private static ConsoleKeyInfo Key(ConsoleKey key) =>
        new ConsoleKeyInfo('\0', key, shift: false, alt: false, control: false);

    private static ConsoleKeyInfo Char(char character) =>
        new ConsoleKeyInfo(character, ConsoleKey.None, shift: false, alt: false, control: false);

    private sealed record SliderRunResult<T>(
        T FinalValue,
        IReadOnlyList<bool> ChangedPerFrame,
        SliderWidget.Implementation LastRendered
    );

    private static SliderRunResult<int> RunIntSlider(
        int initialValue,
        int min,
        int max,
        int step,
        params FrameInput[] frames
    )
    {
        AsphaltContext context = new AsphaltContext();
        int value = initialValue;
        List<bool> changed = [];
        SliderWidget.Implementation? lastRendered = null;

        foreach (FrameInput frame in frames)
        {
            context.BeginLayout(s_terminalDimensions, frame);
            int before = value;
            using (context.Slider(ref value, min, max, step)) { }
            changed.Add(value != before);
            LayoutNode root = context.EndLayout();
            lastRendered = (SliderWidget.Implementation)
                root.NodesWithWidget<SliderWidget.Implementation>().Single().Widget!;
        }

        return new SliderRunResult<int>(value, changed, lastRendered!);
    }

    [TestMethod]
    public void Equals_IncrementsByStep()
    {
        SliderRunResult<int> result = RunIntSlider(
            initialValue: 10,
            min: 0,
            max: 100,
            step: 5,
            Frame(Char('='))
        );

        Assert.AreEqual(15, result.FinalValue);
        Assert.IsTrue(result.ChangedPerFrame[0]);
    }

    [TestMethod]
    public void Plus_AliasesEquals()
    {
        SliderRunResult<int> result = RunIntSlider(
            initialValue: 10,
            min: 0,
            max: 100,
            step: 5,
            Frame(Char('+'))
        );

        Assert.AreEqual(15, result.FinalValue);
    }

    [TestMethod]
    public void Minus_DecrementsByStep()
    {
        SliderRunResult<int> result = RunIntSlider(
            initialValue: 10,
            min: 0,
            max: 100,
            step: 5,
            Frame(Char('-'))
        );

        Assert.AreEqual(5, result.FinalValue);
    }

    [TestMethod]
    public void Increment_ClampsAtMax()
    {
        SliderRunResult<int> result = RunIntSlider(
            initialValue: 98,
            min: 0,
            max: 100,
            step: 5,
            Frame(Char('='))
        );

        Assert.AreEqual(100, result.FinalValue);
    }

    [TestMethod]
    public void Decrement_ClampsAtMin()
    {
        SliderRunResult<int> result = RunIntSlider(
            initialValue: 2,
            min: 0,
            max: 100,
            step: 5,
            Frame(Char('-'))
        );

        Assert.AreEqual(0, result.FinalValue);
    }

    [TestMethod]
    public void Home_JumpsToMin()
    {
        SliderRunResult<int> result = RunIntSlider(
            initialValue: 50,
            min: 0,
            max: 100,
            step: 5,
            Frame(Key(ConsoleKey.Home))
        );

        Assert.AreEqual(0, result.FinalValue);
    }

    [TestMethod]
    public void End_JumpsToMax()
    {
        SliderRunResult<int> result = RunIntSlider(
            initialValue: 50,
            min: 0,
            max: 100,
            step: 5,
            Frame(Key(ConsoleKey.End))
        );

        Assert.AreEqual(100, result.FinalValue);
    }

    [TestMethod]
    public void MultipleKeysInOneFrame_AllApplied()
    {
        SliderRunResult<int> result = RunIntSlider(
            initialValue: 0,
            min: 0,
            max: 100,
            step: 1,
            Frame(Char('='), Char('='), Char('='))
        );

        Assert.AreEqual(3, result.FinalValue);
    }

    [TestMethod]
    public void NoInput_ReturnsFalse()
    {
        SliderRunResult<int> result = RunIntSlider(50, 0, 100, 5, Frame());
        Assert.IsFalse(result.ChangedPerFrame[0]);
        Assert.AreEqual(50, result.FinalValue);
    }

    [TestMethod]
    public void ValueOutsideRange_ClampedOnEntry()
    {
        SliderRunResult<int> result = RunIntSlider(
            initialValue: 999,
            min: 0,
            max: 100,
            step: 5,
            Frame()
        );

        Assert.AreEqual(100, result.FinalValue);
    }

    [TestMethod]
    public void HandlePositionReflectsValue()
    {
        SliderRunResult<int> minResult = RunIntSlider(0, 0, 100, 1, Frame());
        SliderRunResult<int> midResult = RunIntSlider(50, 0, 100, 1, Frame());
        SliderRunResult<int> maxResult = RunIntSlider(100, 0, 100, 1, Frame());

        Assert.AreEqual(0.0, minResult.LastRendered.NormalizedPosition);
        Assert.AreEqual(0.5, midResult.LastRendered.NormalizedPosition);
        Assert.AreEqual(1.0, maxResult.LastRendered.NormalizedPosition);
    }

    [TestMethod]
    public void UnfocusedSlider_IgnoresInput()
    {
        AsphaltContext context = new AsphaltContext();
        int first = 10;
        int second = 10;

        void RunFrame(FrameInput frame)
        {
            context.BeginLayout(s_terminalDimensions, frame);
            using (context.Slider(ref first, 0, 100, 1)) { }
            using (context.Slider(ref second, 0, 100, 1)) { }
            context.EndLayout();
        }

        RunFrame(Frame()); // register focusables
        RunFrame(Frame(Char('='))); // affects first only
        RunFrame(Frame(Key(ConsoleKey.DownArrow))); // shift focus to second
        RunFrame(Frame(Char('='))); // affects second only

        Assert.AreEqual(11, first);
        Assert.AreEqual(11, second);
    }

    [TestMethod]
    public void UniqueKey_AllowsRepeatedCallSite()
    {
        AsphaltContext context = new AsphaltContext();
        int[] values = [0, 0];

        context.BeginLayout(s_terminalDimensions);
        for (int index = 0; index < values.Length; index++)
            using (context.Slider(ref values[index], min: 0, max: 10, uniqueKey: index.ToString()))
            { }
        LayoutNode root = context.EndLayout();

        Assert.AreEqual(2, root.NodesWithWidget<SliderWidget.Implementation>().Count());
    }

    [TestMethod]
    public void DoubleSlider_StepsByFloatingPointAmount()
    {
        AsphaltContext context = new AsphaltContext();
        double value = 0.0;

        context.BeginLayout(s_terminalDimensions, Frame(Char('=')));
        double before = value;
        using (context.Slider(ref value, -1.0, 1.0, 0.25)) { }
        bool changed = value != before;
        context.EndLayout();

        Assert.IsTrue(changed);
        Assert.AreEqual(0.25, value, 1e-9);
    }

    [TestMethod]
    public void MinGreaterThanMax_Throws()
    {
        AsphaltContext context = new AsphaltContext();
        context.BeginLayout(s_terminalDimensions);
        int value = 0;
        Assert.ThrowsExactly<ArgumentException>(() =>
            context.Slider(ref value, min: 10, max: 5).Dispose()
        );
        context.EndLayout();
    }

    [TestMethod]
    public void NegativeStep_Throws()
    {
        AsphaltContext context = new AsphaltContext();
        context.BeginLayout(s_terminalDimensions);
        int value = 0;
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            context.Slider(ref value, min: 0, max: 100, step: -1).Dispose()
        );
        context.EndLayout();
    }

    [TestMethod]
    public void MinEqualsMax_NormalizedPositionIsZero()
    {
        SliderRunResult<int> result = RunIntSlider(5, 5, 5, 1, Frame());
        Assert.AreEqual(0.0, result.LastRendered.NormalizedPosition);
        Assert.AreEqual(5, result.FinalValue);
    }
}
