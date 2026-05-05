// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text;
using Imtui.Rendering;
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

        ImtuiInputEvent[] events = context.CurrentInput.Events.ToArray();
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

        Assert.AreEqual(0, context.CurrentInput.Events.Length);
    }
}
