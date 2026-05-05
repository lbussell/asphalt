// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: WTFPL

using Imtui.Widgets;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Imtui.Tests;

[TestClass]
public class WidgetContractTests
{
    [TestMethod]
    public void Submit_ExecutesWidget()
    {
        ImtuiContext context = new();
        RecordingWidget widget = new();

        context.Submit(widget);

        Assert.IsTrue(widget.Executed);
    }

    [TestMethod]
    public void Submit_ExecutesWidgetAndReturnsResult()
    {
        ImtuiContext context = new();

        int result = context.Submit(new ResultWidget(42));

        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void Submit_RegistersFocusableWidgetBeforeExecuting()
    {
        ImtuiContext context = new();
        FocusableWidget widget = new(context.GetId("Focusable"));

        context.Submit(widget);

        Assert.IsTrue(widget.WasFocusedDuringExecute);
    }

    private sealed class RecordingWidget : IWidget
    {
        public bool Executed { get; private set; }

        public void Execute(ImtuiContext context)
        {
            Executed = true;
        }
    }

    private readonly record struct ResultWidget(int Result) : IStatefulWidget<int>
    {
        public int Execute(ImtuiContext context) => Result;
    }

    private sealed class FocusableWidget(WidgetID id) : IWidget
    {
        public bool WasFocusedDuringExecute { get; private set; }

        public bool IsFocusable => true;

        public WidgetID ID { get; } = id;

        public void Execute(ImtuiContext context)
        {
            WasFocusedDuringExecute = context.IsFocused(ID);
        }
    }
}
