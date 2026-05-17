// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Tests;

[TestClass]
public class FrameInputTests
{
    [TestMethod]
    public void MultipleKeysPerFrame_TabThenTab_AdvancesFocusTwiceAcrossFrames()
    {
        // Tab navigation reads from focusables registered in the previous
        // frame. Use one frame to register two focusables, then a second
        // frame with two Tab keys to verify both are processed in order.
        ImtuiContext context = new ImtuiContext();
        ConsoleKeyInfo tab = new ConsoleKeyInfo(
            '\t',
            ConsoleKey.Tab,
            shift: false,
            alt: false,
            control: false
        );

        // Frame 1: register two focusables. First becomes focused.
        context.BeginLayout(new Dimensions(80, 24));
        context.RegisterFocusable("a");
        context.RegisterFocusable("b");
        context.EndLayout();
        Assert.AreEqual("a", context.FocusedWidgetId);

        // Frame 2: two Tabs. First moves a→b, second wraps b→a.
        context.BeginLayout(new Dimensions(80, 24), new FrameInput(Keys: new[] { tab, tab }));
        context.RegisterFocusable("a");
        context.RegisterFocusable("b");
        context.EndLayout();
        Assert.AreEqual("a", context.FocusedWidgetId);
    }

    [TestMethod]
    public void NonNavigationKey_AvailableViaTryConsumeKey()
    {
        ImtuiContext context = new ImtuiContext();
        ConsoleKeyInfo a = new ConsoleKeyInfo(
            'a',
            ConsoleKey.A,
            shift: false,
            alt: false,
            control: false
        );

        context.BeginLayout(new Dimensions(80, 24), new FrameInput(Keys: new[] { a }));

        Assert.IsTrue(context.TryConsumeKey(out ConsoleKeyInfo first));
        Assert.AreEqual(ConsoleKey.A, first.Key);
        Assert.IsFalse(context.TryConsumeKey(out _));

        context.EndLayout();
    }

    [TestMethod]
    public void MultipleNonNavigationKeys_DeliveredInOrder()
    {
        ImtuiContext context = new ImtuiContext();
        ConsoleKeyInfo a = new ConsoleKeyInfo(
            'a',
            ConsoleKey.A,
            shift: false,
            alt: false,
            control: false
        );
        ConsoleKeyInfo b = new ConsoleKeyInfo(
            'b',
            ConsoleKey.B,
            shift: false,
            alt: false,
            control: false
        );

        context.BeginLayout(new Dimensions(80, 24), new FrameInput(Keys: new[] { a, b }));

        Assert.IsTrue(context.TryConsumeKey(out ConsoleKeyInfo first));
        Assert.AreEqual(ConsoleKey.A, first.Key);
        Assert.IsTrue(context.TryConsumeKey(out ConsoleKeyInfo second));
        Assert.AreEqual(ConsoleKey.B, second.Key);
        Assert.IsFalse(context.TryConsumeKey(out _));

        context.EndLayout();
    }

    [TestMethod]
    public void NavigationKeys_NotDeliveredViaTryConsumeKey()
    {
        ImtuiContext context = new ImtuiContext();
        ConsoleKeyInfo tab = new ConsoleKeyInfo(
            '\t',
            ConsoleKey.Tab,
            shift: false,
            alt: false,
            control: false
        );
        ConsoleKeyInfo enter = new ConsoleKeyInfo(
            '\r',
            ConsoleKey.Enter,
            shift: false,
            alt: false,
            control: false
        );
        ConsoleKeyInfo a = new ConsoleKeyInfo(
            'a',
            ConsoleKey.A,
            shift: false,
            alt: false,
            control: false
        );

        context.BeginLayout(new Dimensions(80, 24), new FrameInput(Keys: new[] { tab, a, enter }));

        Assert.IsTrue(context.TryConsumeKey(out ConsoleKeyInfo only));
        Assert.AreEqual(ConsoleKey.A, only.Key);
        Assert.IsFalse(context.TryConsumeKey(out _));

        context.EndLayout();
    }

    [TestMethod]
    public void UnconsumedKeys_ClearedBetweenFrames()
    {
        ImtuiContext context = new ImtuiContext();
        ConsoleKeyInfo a = new ConsoleKeyInfo(
            'a',
            ConsoleKey.A,
            shift: false,
            alt: false,
            control: false
        );

        context.BeginLayout(new Dimensions(80, 24), new FrameInput(Keys: new[] { a }));
        context.EndLayout();

        // Start a new frame with no input. The 'a' from the previous frame
        // must not carry over.
        context.BeginLayout(new Dimensions(80, 24));
        Assert.IsFalse(context.TryConsumeKey(out _));
        context.EndLayout();
    }
}
