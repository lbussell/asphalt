// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Size = Imtui.Rendering.Size;

namespace Imtui.Tests;

[TestClass]
public class ImtuiInputTests
{
    [TestMethod]
    public void NewFrame_WithInput_MakesInputAvailableForFrame()
    {
        ImtuiContext context = new();
        ImtuiInput input = new(
            ImtuiInputEvent.FromKey(ImtuiKey.Enter),
            ImtuiInputEvent.FromCharacter('a')
        );

        context.NewFrame(new Size(4, 2), input);

        ImtuiInputEvent[] events = context.ThisFrameInput.Events.ToArray();
        Assert.AreEqual(2, events.Length);
        Assert.AreEqual(ImtuiKey.Enter, events[0].Key);
        Assert.AreEqual(new Rune('a'), events[1].Character);
    }

    [TestMethod]
    public void NewFrame_WithoutInput_ClearsPreviousInput()
    {
        ImtuiContext context = new();
        context.NewFrame(new Size(4, 2), new ImtuiInput(ImtuiInputEvent.FromKey(ImtuiKey.Enter)));

        context.NewFrame(new Size(4, 2));

        Assert.AreEqual(0, context.ThisFrameInput.Events.Length);
    }

    [TestMethod]
    public void FromConsoleKeyInfo_MapsKeys()
    {
        ImtuiInput input = ImtuiInput.FromConsoleKeyInfo(
            new ConsoleKeyInfo('\0', ConsoleKey.Enter, shift: false, alt: false, control: false)
        );

        ImtuiInputEvent inputEvent = AssertSingleEvent(input);
        Assert.AreEqual(ImtuiKey.Enter, inputEvent.Key);
    }

    [TestMethod]
    public void FromConsoleKeyInfo_MapsShiftTab()
    {
        ImtuiInput input = ImtuiInput.FromConsoleKeyInfo(
            new ConsoleKeyInfo('\t', ConsoleKey.Tab, shift: true, alt: false, control: false)
        );

        ImtuiInputEvent inputEvent = AssertSingleEvent(input);
        Assert.AreEqual(ImtuiKey.ShiftTab, inputEvent.Key);
    }

    [TestMethod]
    public void FromConsoleKeyInfo_MapsPrintableCharacter()
    {
        ImtuiInput input = ImtuiInput.FromConsoleKeyInfo(
            new ConsoleKeyInfo('a', ConsoleKey.A, shift: false, alt: false, control: false)
        );

        ImtuiInputEvent inputEvent = AssertSingleEvent(input);
        Assert.AreEqual(new Rune('a'), inputEvent.Character);
    }

    [TestMethod]
    public void FromCharacter_MapsRedirectedControlCharacters()
    {
        ImtuiInput input = ImtuiInput.FromCharacter('\t');

        ImtuiInputEvent inputEvent = AssertSingleEvent(input);
        Assert.AreEqual(ImtuiKey.Tab, inputEvent.Key);
    }

    [TestMethod]
    public void FromCharacter_UnsupportedControlCharacter_ReturnsEmptyInput()
    {
        ImtuiInput input = ImtuiInput.FromCharacter('\0');

        Assert.AreEqual(0, input.Events.Length);
    }

    private static ImtuiInputEvent AssertSingleEvent(ImtuiInput input)
    {
        ImtuiInputEvent[] events = input.Events.ToArray();
        Assert.AreEqual(1, events.Length);
        return events[0];
    }
}
