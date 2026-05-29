// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt.Tests;

[TestClass]
public class CaptureInputTests
{
    private static readonly Dimensions s_dimensions = new Dimensions(20, 5);

    private static ConsoleKeyInfo Key(ConsoleKey key) =>
        new ConsoleKeyInfo('\0', key, shift: false, alt: false, control: false);

    [TestMethod]
    public void NoCapture_KeyDownFlowsNormally()
    {
        AsphaltContext context = new AsphaltContext();
        context.BeginLayout(s_dimensions, new FrameInput(Keys: new[] { Key(ConsoleKey.Q) }));

        Assert.IsTrue(context.KeyDown(ConsoleKey.Q));

        context.EndLayout();
    }

    [TestMethod]
    public void CaptureOpenedThisFrame_FirstFrameDoesNotSuppress()
    {
        // The deferred semantic: opening a capture scope on frame N does
        // not retroactively suppress KeyDown calls made earlier in the
        // same frame. Suppression begins on frame N+1.
        AsphaltContext context = new AsphaltContext();
        context.BeginLayout(s_dimensions, new FrameInput(Keys: new[] { Key(ConsoleKey.Q) }));

        // Outside any capture scope, and capture has never been active
        // before \u2014 this should consume the key.
        bool outside = context.KeyDown(ConsoleKey.Q);

        context.OpenCaptureInput();
        context.CloseCaptureInput();
        context.EndLayout();

        Assert.IsTrue(outside);
    }

    [TestMethod]
    public void CaptureFromLastFrame_SuppressesOutsideCallsThisFrame()
    {
        AsphaltContext context = new AsphaltContext();

        // Frame 1: open capture so it becomes active for frame 2.
        context.BeginLayout(s_dimensions);
        context.OpenCaptureInput();
        context.CloseCaptureInput();
        context.EndLayout();

        // Frame 2: a key arrives; an outside-scope KeyDown must NOT consume.
        context.BeginLayout(s_dimensions, new FrameInput(Keys: new[] { Key(ConsoleKey.Q) }));
        bool outside = context.KeyDown(ConsoleKey.Q);
        // But inside a capture scope on the same frame, KeyDown works.
        context.OpenCaptureInput();
        bool inside = context.KeyDown(ConsoleKey.Q);
        context.CloseCaptureInput();
        context.EndLayout();

        Assert.IsFalse(outside, "global KeyDown must be suppressed while capture is active");
        Assert.IsTrue(inside, "KeyDown inside capture scope must still fire");
    }

    [TestMethod]
    public void CaptureClosed_NextFrameRestoresGlobalKeys()
    {
        AsphaltContext context = new AsphaltContext();

        // Frame 1: capture opens.
        context.BeginLayout(s_dimensions);
        context.OpenCaptureInput();
        context.CloseCaptureInput();
        context.EndLayout();

        // Frame 2: capture was active last frame, so still suppressed. Nothing opens this frame.
        context.BeginLayout(s_dimensions, new FrameInput(Keys: new[] { Key(ConsoleKey.Q) }));
        bool suppressed = context.KeyDown(ConsoleKey.Q);
        context.EndLayout();

        // Frame 3: nobody opened capture on frame 2, so suppression lifts.
        context.BeginLayout(s_dimensions, new FrameInput(Keys: new[] { Key(ConsoleKey.Q) }));
        bool restored = context.KeyDown(ConsoleKey.Q);
        context.EndLayout();

        Assert.IsFalse(suppressed);
        Assert.IsTrue(restored);
    }

    [TestMethod]
    public void NestedCapture_DepthCounting()
    {
        AsphaltContext context = new AsphaltContext();
        context.BeginLayout(s_dimensions);

        context.OpenCaptureInput();
        context.OpenCaptureInput();
        context.CloseCaptureInput();
        // Still inside one capture scope.
        context.CloseCaptureInput();

        context.EndLayout();
        // No throw == ok.
    }

    [TestMethod]
    public void UnclosedCapture_ThrowsAtEndLayout()
    {
        AsphaltContext context = new AsphaltContext();
        context.BeginLayout(s_dimensions);
        context.OpenCaptureInput();
        // Forget to close.

        Assert.ThrowsExactly<InvalidOperationException>(() => context.EndLayout());
    }

    [TestMethod]
    public void CloseWithoutOpen_Throws()
    {
        AsphaltContext context = new AsphaltContext();
        context.BeginLayout(s_dimensions);

        Assert.ThrowsExactly<InvalidOperationException>(() => context.CloseCaptureInput());
    }

    [TestMethod]
    public void AddShortcutHint_SuppressedWhileCaptureActiveLastFrame()
    {
        AsphaltContext context = new AsphaltContext();

        // Frame 1: open capture.
        context.BeginLayout(s_dimensions);
        context.OpenCaptureInput();
        context.CloseCaptureInput();
        context.EndLayout();

        // Frame 2: hints registered outside capture scope are dropped;
        // hints registered inside are kept.
        context.BeginLayout(s_dimensions);
        context.AddShortcutHint("Q", "Quit");
        context.OpenCaptureInput();
        context.AddShortcutHint("Enter", "OK");
        context.CloseCaptureInput();
        context.EndLayout();

        Assert.AreEqual(1, context.ShortcutHints.Count);
        Assert.AreEqual("Enter", context.ShortcutHints[0].Label);
    }

    [TestMethod]
    public void WidgetInputScope_StillGatesIndependently()
    {
        // Inside an unfocused widget input scope, KeyDown is suppressed
        // regardless of capture state.
        AsphaltContext context = new AsphaltContext();
        context.BeginLayout(s_dimensions, new FrameInput(Keys: new[] { Key(ConsoleKey.Q) }));

        context.OpenCaptureInput();
        context.PushWidgetInputScope(focused: false);
        bool result = context.KeyDown(ConsoleKey.Q);
        context.PopWidgetInputScope();
        context.CloseCaptureInput();
        context.EndLayout();

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void CaptureKeyDown_WithModifiers_ObeysSameRules()
    {
        AsphaltContext context = new AsphaltContext();

        // Frame 1: arm capture.
        context.BeginLayout(s_dimensions);
        context.OpenCaptureInput();
        context.CloseCaptureInput();
        context.EndLayout();

        // Frame 2: Ctrl+S outside capture is suppressed.
        ConsoleKeyInfo ctrlS = new ConsoleKeyInfo(
            '\0',
            ConsoleKey.S,
            shift: false,
            alt: false,
            control: true
        );
        context.BeginLayout(s_dimensions, new FrameInput(Keys: new[] { ctrlS }));
        bool outside = context.KeyDown(ConsoleKey.S, ConsoleModifiers.Control);
        context.EndLayout();

        Assert.IsFalse(outside);
    }
}
