// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Tests.Widgets;

using Imtui.Widgets;

[TestClass]
public class ScalarInputTests
{
    private static readonly Dimensions s_terminalDimensions = new Dimensions(40, 5);

    private static FrameInput Frame(params ConsoleKeyInfo[] keys) =>
        new FrameInput(keys.Length == 0 ? [] : keys);

    private static ConsoleKeyInfo Key(ConsoleKey key) =>
        new ConsoleKeyInfo('\0', key, shift: false, alt: false, control: false);

    private static ConsoleKeyInfo Char(char character) =>
        new ConsoleKeyInfo(character, ConsoleKey.None, shift: false, alt: false, control: false);

    private sealed record ScalarInputRunResult<T>(
        T FinalValue,
        IReadOnlyList<bool> ChangedPerFrame,
        ScalarInputWidget LastRendered
    );

    private static ScalarInputRunResult<int> RunIntScalarInput(
        int initialValue,
        int min,
        int max,
        int step,
        params FrameInput[] frames
    )
    {
        ImtuiContext context = new ImtuiContext();
        int value = initialValue;
        List<bool> changed = [];
        ScalarInputWidget? lastRendered = null;

        foreach (FrameInput frame in frames)
        {
            context.BeginLayout(s_terminalDimensions, frame);
            bool didChange = context.ScalarInput(ref value, min, max, step);
            changed.Add(didChange);
            LayoutNode root = context.EndLayout();
            lastRendered = (ScalarInputWidget)
                root.NodesWithWidget<ScalarInputWidget>().Single().Widget!;
        }

        return new ScalarInputRunResult<int>(value, changed, lastRendered!);
    }

    [TestMethod]
    public void RightArrow_IncrementsByStep()
    {
        ScalarInputRunResult<int> result = RunIntScalarInput(
            10,
            0,
            100,
            5,
            Frame(Key(ConsoleKey.RightArrow))
        );

        Assert.AreEqual(15, result.FinalValue);
        Assert.IsTrue(result.ChangedPerFrame[0]);
    }

    [TestMethod]
    public void UpArrow_AliasesRightArrow()
    {
        ScalarInputRunResult<int> result = RunIntScalarInput(
            10,
            0,
            100,
            5,
            Frame(Key(ConsoleKey.UpArrow))
        );

        Assert.AreEqual(15, result.FinalValue);
    }

    [TestMethod]
    public void LeftArrow_DecrementsByStep()
    {
        ScalarInputRunResult<int> result = RunIntScalarInput(
            10,
            0,
            100,
            5,
            Frame(Key(ConsoleKey.LeftArrow))
        );

        Assert.AreEqual(5, result.FinalValue);
    }

    [TestMethod]
    public void DownArrow_AliasesLeftArrow()
    {
        ScalarInputRunResult<int> result = RunIntScalarInput(
            10,
            0,
            100,
            5,
            Frame(Key(ConsoleKey.DownArrow))
        );

        Assert.AreEqual(5, result.FinalValue);
    }

    [TestMethod]
    public void Increment_ClampsAtMax()
    {
        ScalarInputRunResult<int> result = RunIntScalarInput(
            98,
            0,
            100,
            5,
            Frame(Key(ConsoleKey.RightArrow))
        );

        Assert.AreEqual(100, result.FinalValue);
    }

    [TestMethod]
    public void Decrement_ClampsAtMin()
    {
        ScalarInputRunResult<int> result = RunIntScalarInput(
            2,
            0,
            100,
            5,
            Frame(Key(ConsoleKey.LeftArrow))
        );

        Assert.AreEqual(0, result.FinalValue);
    }

    [TestMethod]
    public void Home_JumpsToMin()
    {
        ScalarInputRunResult<int> result = RunIntScalarInput(
            50,
            0,
            100,
            5,
            Frame(Key(ConsoleKey.Home))
        );

        Assert.AreEqual(0, result.FinalValue);
    }

    [TestMethod]
    public void End_JumpsToMax()
    {
        ScalarInputRunResult<int> result = RunIntScalarInput(
            50,
            0,
            100,
            5,
            Frame(Key(ConsoleKey.End))
        );

        Assert.AreEqual(100, result.FinalValue);
    }

    [TestMethod]
    public void MultipleKeysInOneFrame_AllApplied()
    {
        ScalarInputRunResult<int> result = RunIntScalarInput(
            0,
            0,
            100,
            1,
            Frame(
                Key(ConsoleKey.RightArrow),
                Key(ConsoleKey.RightArrow),
                Key(ConsoleKey.RightArrow)
            )
        );

        Assert.AreEqual(3, result.FinalValue);
    }

    [TestMethod]
    public void NoInput_ReturnsFalse()
    {
        ScalarInputRunResult<int> result = RunIntScalarInput(50, 0, 100, 5, Frame());
        Assert.IsFalse(result.ChangedPerFrame[0]);
        Assert.AreEqual(50, result.FinalValue);
    }

    [TestMethod]
    public void ValueOutsideRange_ClampedOnEntry()
    {
        ScalarInputRunResult<int> result = RunIntScalarInput(999, 0, 100, 5, Frame());
        Assert.AreEqual(100, result.FinalValue);
    }

    [TestMethod]
    public void PrintableCharacters_DoNotChangeValue()
    {
        ScalarInputRunResult<int> result = RunIntScalarInput(
            42,
            0,
            100,
            1,
            Frame(Char('9'), Char('9'), Char('a'))
        );

        Assert.AreEqual(42, result.FinalValue);
        Assert.IsFalse(result.ChangedPerFrame[0]);
    }

    [TestMethod]
    public void DisplayTextReflectsValue()
    {
        ScalarInputRunResult<int> result = RunIntScalarInput(42, 0, 100, 1, Frame());
        Assert.AreEqual("42", result.LastRendered.DisplayText);
    }

    [TestMethod]
    public void UnfocusedScalarInput_IgnoresInput()
    {
        ImtuiContext context = new ImtuiContext();
        int first = 10;
        int second = 10;

        void RunFrame(FrameInput frame)
        {
            context.BeginLayout(s_terminalDimensions, frame);
            context.ScalarInput(ref first, 0, 100, 1);
            context.ScalarInput(ref second, 0, 100, 1);
            context.EndLayout();
        }

        RunFrame(Frame());
        RunFrame(Frame(Key(ConsoleKey.RightArrow)));
        RunFrame(Frame(Key(ConsoleKey.Tab)));
        RunFrame(Frame(Key(ConsoleKey.RightArrow)));

        Assert.AreEqual(11, first);
        Assert.AreEqual(11, second);
    }

    [TestMethod]
    public void DoubleScalarInput_WithFormat_RendersFormattedText()
    {
        ImtuiContext context = new ImtuiContext();
        double value = 0.5;

        context.BeginLayout(s_terminalDimensions, Frame());
        context.ScalarInput(ref value, 0.0, 1.0, 0.1, format: "0.00");
        LayoutNode root = context.EndLayout();

        ScalarInputWidget widget = (ScalarInputWidget)
            root.NodesWithWidget<ScalarInputWidget>().Single().Widget!;
        Assert.AreEqual("0.50", widget.DisplayText);
    }

    [TestMethod]
    public void DoubleScalarInput_StepsByFloatingPointAmount()
    {
        ImtuiContext context = new ImtuiContext();
        double value = 0.0;

        context.BeginLayout(s_terminalDimensions, Frame(Key(ConsoleKey.RightArrow)));
        bool changed = context.ScalarInput(ref value, -1.0, 1.0, 0.25);
        context.EndLayout();

        Assert.IsTrue(changed);
        Assert.AreEqual(0.25, value, 1e-9);
    }

    [TestMethod]
    public void MinGreaterThanMax_Throws()
    {
        ImtuiContext context = new ImtuiContext();
        context.BeginLayout(s_terminalDimensions);
        int value = 0;
        Assert.ThrowsExactly<ArgumentException>(() =>
            context.ScalarInput(ref value, min: 10, max: 5)
        );
        context.EndLayout();
    }

    [TestMethod]
    public void NegativeStep_Throws()
    {
        ImtuiContext context = new ImtuiContext();
        context.BeginLayout(s_terminalDimensions);
        int value = 0;
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            context.ScalarInput(ref value, min: 0, max: 100, step: -1)
        );
        context.EndLayout();
    }

    [TestMethod]
    public void NonPositiveWidth_Throws()
    {
        ImtuiContext context = new ImtuiContext();
        context.BeginLayout(s_terminalDimensions);
        int value = 0;
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            context.ScalarInput(ref value, min: 0, max: 100, width: 0)
        );
        context.EndLayout();
    }

    [TestMethod]
    public void MinEqualsMax_DoesNotThrow()
    {
        ScalarInputRunResult<int> result = RunIntScalarInput(5, 5, 5, 1, Frame());
        Assert.AreEqual(5, result.FinalValue);
        Assert.AreEqual("5", result.LastRendered.DisplayText);
    }

    [TestMethod]
    public void AutoWidth_FitsWidestFormattedBound()
    {
        ImtuiContext context = new ImtuiContext();
        int value = 0;

        context.BeginLayout(s_terminalDimensions, Frame());
        context.ScalarInput(ref value, min: 0, max: 100);
        LayoutNode root = context.EndLayout();

        ScalarInputWidget widget = (ScalarInputWidget)
            root.NodesWithWidget<ScalarInputWidget>().Single().Widget!;
        // "100".Length (3) + 2 cells of padding = 5
        Assert.AreEqual(5, widget.Measure().Preferred.Width);
    }

    [TestMethod]
    public void ExplicitWidth_OverridesAutoWidth()
    {
        ImtuiContext context = new ImtuiContext();
        int value = 0;

        context.BeginLayout(s_terminalDimensions, Frame());
        context.ScalarInput(ref value, min: 0, max: 100, width: 12);
        LayoutNode root = context.EndLayout();

        ScalarInputWidget widget = (ScalarInputWidget)
            root.NodesWithWidget<ScalarInputWidget>().Single().Widget!;
        Assert.AreEqual(12, widget.Measure().Preferred.Width);
    }
}
