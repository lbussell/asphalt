// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt.Tests;

using Asphalt.Widgets;

[TestClass]
public class FrameInputTests
{
    private static readonly Dimensions s_terminalDimensions = new Dimensions(80, 24);

    private static ConsoleKeyInfo Key(ConsoleKey key) =>
        new ConsoleKeyInfo('\0', key, shift: false, alt: false, control: false);

    private static ConsoleKeyInfo Char(char character) =>
        new ConsoleKeyInfo(character, ConsoleKey.None, shift: false, alt: false, control: false);

    private static List<ConsoleKeyInfo> ConsumeRemainingKeys(AsphaltContext context)
    {
        List<ConsoleKeyInfo> keys = [];
        context.ConsumeKeys(keys.Add);
        return keys;
    }

    [TestMethod]
    public void MultipleKeysPerFrame_DownThenDown_AdvancesFocusTwiceWithinScope()
    {
        // Use one frame to register three focusables, then a second frame with
        // two DownArrow keys to verify both are processed in order. No wrap,
        // so two presses move a->b->c.
        AsphaltContext context = new AsphaltContext();
        ConsoleKeyInfo down = Key(ConsoleKey.DownArrow);

        // Frame 1: register three focusables. First becomes focused.
        context.BeginLayout(s_terminalDimensions);
        context.RegisterFocusable("a");
        context.RegisterFocusable("b");
        context.RegisterFocusable("c");
        context.EndLayout();
        Assert.AreEqual("a", context.FocusedWidgetId);

        // Frame 2: two Downs. First moves a to b, second moves b to c.
        context.BeginLayout(s_terminalDimensions, new FrameInput(Keys: new[] { down, down }));
        context.RegisterFocusable("a");
        context.RegisterFocusable("b");
        context.RegisterFocusable("c");
        context.EndLayout();
        Assert.AreEqual("c", context.FocusedWidgetId);
    }

    [TestMethod]
    public void NonNavigationKey_AvailableViaConsumeKeys()
    {
        AsphaltContext context = new AsphaltContext();
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
        AsphaltContext context = new AsphaltContext();
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
        AsphaltContext context = new AsphaltContext();
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
    public void DownArrow_MovesFocusAtEndLayoutWhenApplicationDoesNotConsumeIt()
    {
        AsphaltContext context = new AsphaltContext();

        context.BeginLayout(s_terminalDimensions);
        context.RegisterFocusable("a");
        context.RegisterFocusable("b");
        context.EndLayout();

        context.BeginLayout(s_terminalDimensions, new FrameInput(Key(ConsoleKey.DownArrow)));
        context.RegisterFocusable("a");
        context.RegisterFocusable("b");
        context.EndLayout();
        context.EndFrame();

        Assert.AreEqual("b", context.FocusedWidgetId);
        Assert.AreEqual(TimeSpan.Zero, context.NextScheduledRedraw);
    }

    [TestMethod]
    public void ApplicationCanConsumeArrowKeyBeforeDefaultFocusNavigation()
    {
        AsphaltContext context = new AsphaltContext();

        context.BeginLayout(s_terminalDimensions);
        context.RegisterFocusable("a");
        context.RegisterFocusable("b");
        context.EndLayout();

        context.BeginLayout(s_terminalDimensions, new FrameInput(Key(ConsoleKey.DownArrow)));
        context.RegisterFocusable("a");
        context.RegisterFocusable("b");

        bool consumedDown = context.ConsumeKeys(static key => key.Key == ConsoleKey.DownArrow);
        Assert.IsTrue(consumedDown);

        context.EndLayout();

        Assert.AreEqual("a", context.FocusedWidgetId);
    }

    [TestMethod]
    public void FocusedWidgetCanConsumeLaterMatchingKeyWithoutConsumingEarlierRejectedKey()
    {
        AsphaltContext context = new AsphaltContext();
        string value = "";

        context.BeginLayout(
            s_terminalDimensions,
            new FrameInput(Keys: new[] { Key(ConsoleKey.Enter), Char('a') })
        );
        using (context.InputText(ref value)) { }

        Assert.AreEqual("a", value);
        List<ConsoleKeyInfo> fallbackKeys = ConsumeRemainingKeys(context);
        Assert.AreEqual(1, fallbackKeys.Count);
        Assert.AreEqual(ConsoleKey.Enter, fallbackKeys[0].Key);

        context.EndLayout();
    }

    [TestMethod]
    public void FocusedWidgetRejectedKey_RemainsAvailableToApplicationFallback()
    {
        AsphaltContext context = new AsphaltContext();
        int value = 5;

        context.BeginLayout(s_terminalDimensions);
        using (context.ScalarInput(ref value, min: 0, max: 10)) { }
        context.EndLayout();

        context.BeginLayout(s_terminalDimensions, new FrameInput(Char('x')));
        using (context.ScalarInput(ref value, min: 0, max: 10)) { }

        List<ConsoleKeyInfo> keys = ConsumeRemainingKeys(context);
        Assert.AreEqual(1, keys.Count);
        Assert.AreEqual('x', keys[0].KeyChar);

        context.EndLayout();
    }

    [TestMethod]
    public void FocusedButton_ConsumesEnterKey()
    {
        AsphaltContext context = new AsphaltContext();
        bool firstPressed = false;
        bool secondPressed = false;

        void RenderButtons()
        {
            using (context.Button("First"))
            {
                if (context.KeyDown(ConsoleKey.Enter))
                    firstPressed = true;
            }
            using (context.Button("Second"))
            {
                if (context.KeyDown(ConsoleKey.Enter))
                    secondPressed = true;
            }
        }

        context.BeginLayout(s_terminalDimensions);
        RenderButtons();
        context.EndLayout();

        context.BeginLayout(s_terminalDimensions, new FrameInput(Key(ConsoleKey.DownArrow)));
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
        AsphaltContext context = new AsphaltContext();
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
