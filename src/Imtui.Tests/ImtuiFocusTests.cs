// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using Imtui.Rendering;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Size = Imtui.Rendering.Size;

namespace Imtui.Tests;

[TestClass]
public class ImtuiFocusTests
{
    [TestMethod]
    public void RegisterFocusable_FocusesFirstWidget()
    {
        ImtuiContext context = CreateContext();
        WidgetID first = context.GetId("First");
        WidgetID second = context.GetId("Second");

        Assert.IsTrue(context.RegisterFocusable(first));
        Assert.IsFalse(context.RegisterFocusable(second));
        Assert.AreEqual(first, context.FocusedWidgetId);
    }

    [TestMethod]
    public void NewFrame_WithTab_MovesFocusToNextWidget()
    {
        ImtuiContext context = CreateContext();
        RegisterTwoWidgets(context);

        context.NewFrame(new Size(10, 3), new ImtuiInput(ImtuiInputEvent.FromKey(ImtuiKey.Tab)));
        (bool firstFocused, bool secondFocused) = RegisterTwoWidgets(context);

        Assert.IsFalse(firstFocused);
        Assert.IsTrue(secondFocused);
    }

    [TestMethod]
    public void NewFrame_WithShiftTab_MovesFocusToPreviousWidget()
    {
        ImtuiContext context = CreateContext();
        RegisterTwoWidgets(context);

        context.NewFrame(
            new Size(10, 3),
            new ImtuiInput(ImtuiInputEvent.FromKey(ImtuiKey.ShiftTab))
        );
        (bool firstFocused, bool secondFocused) = RegisterTwoWidgets(context);

        Assert.IsFalse(firstFocused);
        Assert.IsTrue(secondFocused);
    }

    [TestMethod]
    public void IsActivated_ReturnsTrueForFocusedWidgetOnEnter()
    {
        ImtuiContext context = CreateContext(
            new ImtuiInput(ImtuiInputEvent.FromKey(ImtuiKey.Enter))
        );
        WidgetID id = context.GetId("Button");

        context.RegisterFocusable(id);

        Assert.IsTrue(context.IsActivated(id));
    }

    [TestMethod]
    public void IsActivated_ReturnsFalseForUnfocusedWidget()
    {
        ImtuiContext context = CreateContext(
            new ImtuiInput(ImtuiInputEvent.FromKey(ImtuiKey.Enter))
        );
        WidgetID first = context.GetId("First");
        WidgetID second = context.GetId("Second");

        context.RegisterFocusable(first);
        context.RegisterFocusable(second);

        Assert.IsFalse(context.IsActivated(second));
    }

    private static ImtuiContext CreateContext(ImtuiInput input = default)
    {
        ImtuiContext context = new();
        context.NewFrame(new Size(10, 3), input);
        return context;
    }

    private static (bool FirstFocused, bool SecondFocused) RegisterTwoWidgets(ImtuiContext context)
    {
        bool firstFocused = context.RegisterFocusable(context.GetId("First"));
        bool secondFocused = context.RegisterFocusable(context.GetId("Second"));
        return (firstFocused, secondFocused);
    }
}
