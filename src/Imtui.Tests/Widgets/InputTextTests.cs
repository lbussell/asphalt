// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Tests.Widgets;

using Imtui.Widgets;

[TestClass]
public class InputTextTests
{
    private static readonly Dimensions s_terminalDimensions = new Dimensions(40, 5);

    // Helpers ---------------------------------------------------------------

    private static FrameInput Frame(params ConsoleKeyInfo[] keys) =>
        new FrameInput(keys.Length == 0 ? [] : keys);

    private static ConsoleKeyInfo Char(char character) =>
        new ConsoleKeyInfo(character, ConsoleKey.None, shift: false, alt: false, control: false);

    private static ConsoleKeyInfo Key(ConsoleKey key) =>
        new ConsoleKeyInfo('\0', key, shift: false, alt: false, control: false);

    // Runs a sequence of frames against a single InputText widget, threading
    // the value through each frame. Returns the final value, the per-frame
    // "changed" return values, and the rendered widget from the last frame.
    private sealed record InputTextRunResult(
        string FinalValue,
        IReadOnlyList<bool> ChangedPerFrame,
        InputTextWidget.Implementation LastRendered
    );

    private static InputTextRunResult RunInputText(string initialValue, params FrameInput[] frames)
    {
        ImtuiContext context = new ImtuiContext();
        string value = initialValue;
        List<bool> changed = [];
        InputTextWidget.Implementation? lastRendered = null;

        foreach (FrameInput frame in frames)
        {
            context.BeginLayout(s_terminalDimensions, frame);
            bool didChange = context.InputText(ref value);
            changed.Add(didChange);
            LayoutNode root = context.EndLayout();
            lastRendered = (InputTextWidget.Implementation)
                root.NodesWithWidget<InputTextWidget.Implementation>().Single().Widget!;
        }

        return new InputTextRunResult(value, changed, lastRendered!);
    }

    // Tests -----------------------------------------------------------------

    [TestMethod]
    public void Typing_AppendsCharactersToValue()
    {
        InputTextRunResult result = RunInputText("", Frame(Char('h')), Frame(Char('i')));

        Assert.AreEqual("hi", result.FinalValue);
        Assert.AreEqual(2, result.LastRendered.Cursor);
        CollectionAssert.AreEqual(new[] { true, true }, result.ChangedPerFrame.ToArray());
    }

    [TestMethod]
    public void Typing_MultipleKeysInOneFrame_AllApplied()
    {
        InputTextRunResult result = RunInputText("", Frame(Char('a'), Char('b'), Char('c')));

        Assert.AreEqual("abc", result.FinalValue);
        Assert.AreEqual(3, result.LastRendered.Cursor);
    }

    [TestMethod]
    public void Backspace_RemovesCharacterBeforeCursor()
    {
        InputTextRunResult result = RunInputText(
            "",
            Frame(Char('h'), Char('i')),
            Frame(Key(ConsoleKey.Backspace))
        );

        Assert.AreEqual("h", result.FinalValue);
        Assert.AreEqual(1, result.LastRendered.Cursor);
    }

    [TestMethod]
    public void Backspace_AtStart_DoesNothing()
    {
        InputTextRunResult result = RunInputText("", Frame(Key(ConsoleKey.Backspace)));

        Assert.AreEqual("", result.FinalValue);
        Assert.AreEqual(0, result.LastRendered.Cursor);
        Assert.IsFalse(result.ChangedPerFrame[0]);
    }

    [TestMethod]
    public void Delete_RemovesCharacterAtCursor()
    {
        InputTextRunResult result = RunInputText(
            "",
            Frame(Char('h'), Char('i')),
            Frame(Key(ConsoleKey.Home)),
            Frame(Key(ConsoleKey.Delete))
        );

        Assert.AreEqual("i", result.FinalValue);
        Assert.AreEqual(0, result.LastRendered.Cursor);
    }

    [TestMethod]
    public void Delete_AtEnd_DoesNothing()
    {
        InputTextRunResult result = RunInputText(
            "",
            Frame(Char('h')),
            Frame(Key(ConsoleKey.Delete))
        );

        Assert.AreEqual("h", result.FinalValue);
        Assert.IsFalse(result.ChangedPerFrame[1]);
    }

    [TestMethod]
    public void LeftRight_MoveCursor_ThenTypingInsertsAtCursor()
    {
        InputTextRunResult result = RunInputText(
            "",
            Frame(Char('a'), Char('c')),
            Frame(Key(ConsoleKey.LeftArrow)),
            Frame(Char('b'))
        );

        Assert.AreEqual("abc", result.FinalValue);
        Assert.AreEqual(2, result.LastRendered.Cursor);
    }

    [TestMethod]
    public void HomeAndEnd_JumpCursorToEnds()
    {
        InputTextRunResult result = RunInputText(
            "",
            Frame(Char('a'), Char('b'), Char('c')),
            Frame(Key(ConsoleKey.Home))
        );

        Assert.AreEqual(0, result.LastRendered.Cursor);

        result = RunInputText(
            "",
            Frame(Char('a'), Char('b'), Char('c')),
            Frame(Key(ConsoleKey.Home)),
            Frame(Key(ConsoleKey.End))
        );

        Assert.AreEqual(3, result.LastRendered.Cursor);
    }

    [TestMethod]
    public void NoKeys_ReturnsFalse()
    {
        InputTextRunResult result = RunInputText("hi", Frame());

        Assert.AreEqual("hi", result.FinalValue);
        Assert.IsFalse(result.ChangedPerFrame[0]);
    }

    [TestMethod]
    public void CursorMovement_DoesNotReportChange()
    {
        InputTextRunResult result = RunInputText(
            "",
            Frame(Char('a')),
            Frame(Key(ConsoleKey.LeftArrow))
        );

        Assert.IsTrue(result.ChangedPerFrame[0]);
        Assert.IsFalse(result.ChangedPerFrame[1]);
    }

    [TestMethod]
    public void UnfocusedInputText_IgnoresTypedCharacters()
    {
        // Two InputTexts. First is focused by default. Tab moves focus to the
        // second. Typing thereafter should only affect the second.
        ImtuiContext context = new ImtuiContext();
        string first = "";
        string second = "";

        void RunFrame(FrameInput frame)
        {
            context.BeginLayout(s_terminalDimensions, frame);
            context.InputText(ref first);
            context.InputText(ref second);
            context.EndLayout();
        }

        RunFrame(Frame()); // register focusables; first is focused by default
        RunFrame(Frame(Char('a'))); // focused: first
        RunFrame(Frame(Key(ConsoleKey.DownArrow))); // shift focus to second
        RunFrame(Frame(Char('b'))); // focused: second

        Assert.AreEqual("a", first);
        Assert.AreEqual("b", second);
    }

    [TestMethod]
    public void EmptyAndUnfocused_RendersPlaceholder()
    {
        // Two InputTexts so we can move focus off the first.
        ImtuiContext context = new ImtuiContext();
        string first = "";
        string second = "";
        InputTextWidget.Implementation? firstRendered = null;

        void RunFrame(FrameInput frame)
        {
            context.BeginLayout(s_terminalDimensions, frame);
            context.InputText(ref first, placeholder: "hint");
            context.InputText(ref second);
            LayoutNode root = context.EndLayout();
            firstRendered = (InputTextWidget.Implementation)
                root.NodesWithWidget<InputTextWidget.Implementation>().First().Widget!;
        }

        RunFrame(Frame()); // register focusables (first input is focused by default)
        RunFrame(Frame(Key(ConsoleKey.DownArrow))); // shift focus after this frame
        RunFrame(Frame()); // render the updated focus state

        Assert.IsFalse(firstRendered!.Focused);
        Assert.AreEqual("", firstRendered.Value);
        Assert.AreEqual("hint", firstRendered.Placeholder);
    }

    [TestMethod]
    public void NonEmpty_DoesNotShowPlaceholderInValue()
    {
        InputTextRunResult result = RunInputText("", Frame(Char('x')));

        Assert.AreEqual("x", result.LastRendered.Value);
    }

    [TestMethod]
    public void NullValue_Throws()
    {
        ImtuiContext context = new ImtuiContext();
        context.BeginLayout(s_terminalDimensions);
        string? value = null;
        Assert.ThrowsExactly<ArgumentNullException>(() => context.InputText(ref value!));
        context.EndLayout();
    }

    [TestMethod]
    public void CursorPersistsAcrossFramesWithNoInput()
    {
        InputTextRunResult result = RunInputText(
            "",
            Frame(Char('a'), Char('b'), Char('c')),
            Frame(Key(ConsoleKey.LeftArrow)),
            Frame() // no input — cursor should not jump
        );

        Assert.AreEqual(2, result.LastRendered.Cursor);
    }
}
