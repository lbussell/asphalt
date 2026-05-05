// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text;
using Imtui.Rendering;
using Imtui.Widgets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Size = Imtui.Rendering.Size;

namespace Imtui.Tests;

[TestClass]
public class WidgetApiTests
{
    [TestMethod]
    public void Text_WritesTextAndAdvancesToNextLine()
    {
        ImtuiContext context = CreateContext();

        context.Text("Hello");
        context.Text("World");

        AssertCell(context, 0, 0, 'H');
        AssertCell(context, 0, 1, 'W');
    }

    [TestMethod]
    public void Button_ReturnsTrueWhenFocusedAndActivated()
    {
        ImtuiContext context = CreateContext(
            new ImtuiInput(ImtuiInputEvent.FromKey(ImtuiKey.Enter))
        );

        bool clicked = context.Button("OK");

        Assert.IsTrue(clicked);
        AssertCell(context, 0, 0, '[');
        AssertCell(context, 1, 0, 'O');
    }

    [TestMethod]
    public void Button_ReturnsFalseWhenNotFocused()
    {
        ImtuiContext context = CreateContext(
            new ImtuiInput(ImtuiInputEvent.FromKey(ImtuiKey.Enter))
        );

        bool firstClicked = context.Button("One");
        bool secondClicked = context.Button("Two");

        Assert.IsTrue(firstClicked);
        Assert.IsFalse(secondClicked);
    }

    [TestMethod]
    public void Checkbox_TogglesValueWhenActivated()
    {
        ImtuiContext context = CreateContext(
            new ImtuiInput(ImtuiInputEvent.FromKey(ImtuiKey.Space))
        );
        bool value = false;
        context.Checkbox("Enabled", ref value);
        Assert.IsTrue(value);
        AssertCell(context, 1, 0, 'x');
    }

    [TestMethod]
    public void Checkbox_DoesNotChangeWithoutActivation()
    {
        ImtuiContext context = CreateContext();
        bool value = true;
        context.Checkbox("Enabled", ref value);
        Assert.IsTrue(value);
        AssertCell(context, 1, 0, 'x');
    }

    [TestMethod]
    public void TextField_InsertsCharactersAndReportsChanged()
    {
        ImtuiContext context = CreateContext(new ImtuiInput(ImtuiInputEvent.FromCharacter('a')));
        string value = "";
        context.TextField("Name", ref value);
        Assert.AreEqual("a", value);
        AssertCell(context, 0, 0, 'N');
        AssertCell(context, 6, 0, 'a');
    }

    [TestMethod]
    public void TextField_PersistsCursorStateAcrossFrames()
    {
        ImtuiContext context = CreateContext();
        string value = "ab";

        context.TextField("Name", ref value);
        context.NewFrame(
            new Size(20, 4),
            new ImtuiInput(ImtuiInputEvent.FromKey(ImtuiKey.LeftArrow))
        );
        context.TextField("Name", ref value);
        context.NewFrame(new Size(20, 4), new ImtuiInput(ImtuiInputEvent.FromCharacter('X')));
        context.TextField("Name", ref value);
        Assert.AreEqual("aXb", value);
    }

    private static ImtuiContext CreateContext(ImtuiInput input = default)
    {
        ImtuiContext context = new();
        context.NewFrame(new Size(20, 4), input);
        return context;
    }

    private static void AssertCell(ImtuiContext context, int x, int y, char expected)
    {
        Assert.AreEqual(
            new Rune(expected),
            context.CurrentScreen[new CellPosition(x, y)].Glyph,
            $"Expected '{expected}' at ({x}, {y})."
        );
    }
}
