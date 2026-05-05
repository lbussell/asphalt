// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: WTFPL

using Imtui.Widgets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Size = Imtui.Rendering.Size;

namespace Imtui.Tests;

[TestClass]
public class ImtuiFocusTests
{
    [TestMethod]
    public void Submit_FocusesFirstFocusableWidget()
    {
        ImtuiContext context = CreateContext();
        WidgetID first = context.GetId("First");

        Assert.IsTrue(context.Submit(new FocusableWidget(first)));
        Assert.IsFalse(context.Submit(new FocusableWidget(context.GetId("Second"))));
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

        context.Submit(new FocusableWidget(id));

        Assert.IsTrue(context.IsActivated(id));
    }

    [TestMethod]
    public void IsActivated_ReturnsFalseForUnfocusedWidget()
    {
        ImtuiContext context = CreateContext(
            new ImtuiInput(ImtuiInputEvent.FromKey(ImtuiKey.Enter))
        );
        WidgetID second = context.GetId("Second");

        context.Submit(new FocusableWidget(context.GetId("First")));
        context.Submit(new FocusableWidget(second));

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
        bool firstFocused = context.Submit(new FocusableWidget(context.GetId("First")));
        bool secondFocused = context.Submit(new FocusableWidget(context.GetId("Second")));
        return (firstFocused, secondFocused);
    }

    private readonly record struct FocusableWidget(WidgetID ID) : IStatefulWidget<bool>
    {
        public bool IsFocusable => true;

        public bool Execute(ImtuiContext context) => context.IsFocused(ID);
    }
}
