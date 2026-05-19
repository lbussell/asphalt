// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Tests;

using Imtui.Widgets;

[TestClass]
public class FrameInputTests
{
    private static readonly Dimensions s_terminalDimensions = new Dimensions(80, 24);

    private static ConsoleKeyInfo Key(ConsoleKey key) =>
        new ConsoleKeyInfo('\0', key, shift: false, alt: false, control: false);

    private static ConsoleKeyInfo Char(char character) =>
        new ConsoleKeyInfo(character, ConsoleKey.None, shift: false, alt: false, control: false);

    private static List<ConsoleKeyInfo> ConsumeRemainingKeys(ImtuiContext context)
    {
        List<ConsoleKeyInfo> keys = [];
        context.ConsumeKeys(keys.Add);
        return keys;
    }

    [TestMethod]
    public void MultipleKeysPerFrame_TabThenTab_AdvancesFocusTwiceAcrossFrames()
    {
        // Use one frame to register two focusables, then a second frame with
        // two Tab keys to verify both are processed in order.
        ImtuiContext context = new ImtuiContext();
        ConsoleKeyInfo tab = Key(ConsoleKey.Tab);

        // Frame 1: register two focusables. First becomes focused.
        context.BeginLayout(s_terminalDimensions);
        context.RegisterFocusable("a");
        context.RegisterFocusable("b");
        context.EndLayout();
        Assert.AreEqual("a", context.FocusedWidgetId);

        // Frame 2: two Tabs. First moves a to b, second wraps b to a.
        context.BeginLayout(s_terminalDimensions, new FrameInput(Keys: new[] { tab, tab }));
        context.RegisterFocusable("a");
        context.RegisterFocusable("b");
        context.EndLayout();
        Assert.AreEqual("a", context.FocusedWidgetId);
    }

    [TestMethod]
    public void NonNavigationKey_AvailableViaConsumeKeys()
    {
        ImtuiContext context = new ImtuiContext();
        ConsoleKeyInfo a = Key(ConsoleKey.A);

        context.BeginLayout(s_terminalDimensions, new FrameInput(Keys: new[] { a }));

        List<ConsoleKeyInfo> keys = ConsumeRemainingKeys(context);
        Assert.AreEqual(1, keys.Count);
        Assert.AreEqual(ConsoleKey.A, keys[0].Key);

        context.EndLayout();
    }

    [TestMethod]
    public void MultipleNonNavigationKeys_DeliveredInOrder()
    {
        ImtuiContext context = new ImtuiContext();
        ConsoleKeyInfo a = Key(ConsoleKey.A);
        ConsoleKeyInfo b = Key(ConsoleKey.B);

        context.BeginLayout(s_terminalDimensions, new FrameInput(Keys: new[] { a, b }));

        List<ConsoleKeyInfo> keys = ConsumeRemainingKeys(context);
        Assert.AreEqual(2, keys.Count);
        Assert.AreEqual(ConsoleKey.A, keys[0].Key);
        Assert.AreEqual(ConsoleKey.B, keys[1].Key);

        context.EndLayout();
    }

    [TestMethod]
    public void SpecialKeys_AvailableViaConsumeKeys()
    {
        ImtuiContext context = new ImtuiContext();
        ConsoleKeyInfo tab = Key(ConsoleKey.Tab);
        ConsoleKeyInfo enter = Key(ConsoleKey.Enter);
        ConsoleKeyInfo a = Key(ConsoleKey.A);

        context.BeginLayout(s_terminalDimensions, new FrameInput(Keys: new[] { tab, a, enter }));

        List<ConsoleKeyInfo> keys = ConsumeRemainingKeys(context);
        Assert.AreEqual(3, keys.Count);
        Assert.AreEqual(ConsoleKey.Tab, keys[0].Key);
        Assert.AreEqual(ConsoleKey.A, keys[1].Key);
        Assert.AreEqual(ConsoleKey.Enter, keys[2].Key);

        context.EndLayout();
    }

    [TestMethod]
    public void TabKey_MovesFocusAtEndLayoutWhenApplicationDoesNotConsumeIt()
    {
        ImtuiContext context = new ImtuiContext();

        context.BeginLayout(s_terminalDimensions);
        context.RegisterFocusable("a");
        context.RegisterFocusable("b");
        context.EndLayout();

        context.BeginLayout(s_terminalDimensions, new FrameInput(Key(ConsoleKey.Tab)));
        context.RegisterFocusable("a");
        context.RegisterFocusable("b");
        context.EndLayout();

        Assert.AreEqual("b", context.FocusedWidgetId);
        Assert.AreEqual(TimeSpan.Zero, context.NextScheduledRedraw);
    }

    [TestMethod]
    public void ApplicationCanConsumeTabKeyBeforeDefaultFocusNavigation()
    {
        ImtuiContext context = new ImtuiContext();

        context.BeginLayout(s_terminalDimensions);
        context.RegisterFocusable("a");
        context.RegisterFocusable("b");
        context.EndLayout();

        context.BeginLayout(s_terminalDimensions, new FrameInput(Key(ConsoleKey.Tab)));
        context.RegisterFocusable("a");
        context.RegisterFocusable("b");

        bool consumedTab = context.ConsumeKeys(static key => key.Key == ConsoleKey.Tab);
        Assert.IsTrue(consumedTab);

        context.EndLayout();

        Assert.AreEqual("a", context.FocusedWidgetId);
    }

    [TestMethod]
    public void FocusedWidgetCanConsumeLaterMatchingKeyWithoutConsumingEarlierRejectedKey()
    {
        ImtuiContext context = new ImtuiContext();
        string value = "";

        context.BeginLayout(
            s_terminalDimensions,
            new FrameInput(Keys: new[] { Key(ConsoleKey.Enter), Char('a') })
        );
        context.InputText(ref value);

        Assert.AreEqual("a", value);
        List<ConsoleKeyInfo> fallbackKeys = ConsumeRemainingKeys(context);
        Assert.AreEqual(1, fallbackKeys.Count);
        Assert.AreEqual(ConsoleKey.Enter, fallbackKeys[0].Key);

        context.EndLayout();
    }

    [TestMethod]
    public void FocusedWidgetRejectedKey_RemainsAvailableToApplicationFallback()
    {
        ImtuiContext context = new ImtuiContext();
        int value = 5;

        context.BeginLayout(s_terminalDimensions);
        context.ScalarInput(ref value, min: 0, max: 10);
        context.EndLayout();

        context.BeginLayout(s_terminalDimensions, new FrameInput(Char('x')));
        context.ScalarInput(ref value, min: 0, max: 10);

        List<ConsoleKeyInfo> keys = ConsumeRemainingKeys(context);
        Assert.AreEqual(1, keys.Count);
        Assert.AreEqual('x', keys[0].KeyChar);

        context.EndLayout();
    }

    [TestMethod]
    public void FocusedButton_ConsumesEnterKey()
    {
        ImtuiContext context = new ImtuiContext();
        bool firstPressed = false;
        bool secondPressed = false;

        void RenderButtons()
        {
            firstPressed = context.Button("First");
            secondPressed = context.Button("Second");
        }

        context.BeginLayout(s_terminalDimensions);
        RenderButtons();
        context.EndLayout();

        context.BeginLayout(s_terminalDimensions, new FrameInput(Key(ConsoleKey.Tab)));
        RenderButtons();
        context.EndLayout();

        context.BeginLayout(
            s_terminalDimensions,
            new FrameInput(Keys: new[] { Key(ConsoleKey.Enter), Key(ConsoleKey.Enter) })
        );
        RenderButtons();

        Assert.IsFalse(firstPressed);
        Assert.IsTrue(secondPressed);
        Assert.IsFalse(context.ConsumeKeys(static _ => true));

        context.EndLayout();
    }

    [TestMethod]
    public void UnconsumedKeys_ClearedBetweenFrames()
    {
        ImtuiContext context = new ImtuiContext();
        ConsoleKeyInfo a = Key(ConsoleKey.A);

        context.BeginLayout(s_terminalDimensions, new FrameInput(Keys: new[] { a }));
        context.EndLayout();

        // Start a new frame with no input. The 'a' from the previous frame
        // must not carry over.
        context.BeginLayout(s_terminalDimensions);
        Assert.IsFalse(context.ConsumeKeys(static _ => true));
        context.EndLayout();
    }
}
