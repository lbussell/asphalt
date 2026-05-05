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

    private sealed class RecordingWidget : IWidget
    {
        public bool Executed { get; private set; }

        public void Execute(ImtuiContext context)
        {
            Executed = true;
        }
    }

    private readonly record struct ResultWidget(int Result) : IWidget<int>
    {
        public int Execute(ImtuiContext context) => Result;
    }
}
