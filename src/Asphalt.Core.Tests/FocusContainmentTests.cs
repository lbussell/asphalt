// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt.Tests;

[TestClass]
public class FocusContainmentTests
{
    private static readonly Dimensions s_dimensions = new Dimensions(20, 5);

    private static ConsoleKeyInfo Key(ConsoleKey key) =>
        new ConsoleKeyInfo('\0', key, shift: false, alt: false, control: false);

    [TestMethod]
    public void Capture_AutoFocusesFirstWidgetInsideScope()
    {
        AsphaltContext context = new AsphaltContext();
        context.BeginLayout(s_dimensions);

        // Background focusables.
        context.RegisterFocusable("bg-1");
        context.RegisterFocusable("bg-2");

        // Capture-bounded subtree with its own focusables.
        context.OpenCaptureInput();
        context.RegisterFocusable("modal-ok");
        context.RegisterFocusable("modal-cancel");
        context.CloseCaptureInput();

        context.EndLayout();

        Assert.AreEqual("modal-ok", context.FocusedWidgetId);
        Assert.IsFalse(context.IsFocused("bg-1"));
        Assert.IsFalse(context.IsFocused("bg-2"));
        Assert.IsTrue(context.IsFocused("modal-ok"));
    }

    [TestMethod]
    public void Capture_ArrowNavigationStaysInsideScope()
    {
        AsphaltContext context = new AsphaltContext();

        // Frame 1: register two background and two modal focusables; modal
        // contains focus, "modal-ok" is the auto-focused first child.
        context.BeginLayout(s_dimensions);
        context.RegisterFocusable("bg-1");
        context.RegisterFocusable("bg-2");
        context.OpenCaptureInput();
        context.RegisterFocusable("modal-ok");
        context.RegisterFocusable("modal-cancel");
        context.CloseCaptureInput();
        context.EndLayout();

        Assert.AreEqual("modal-ok", context.FocusedWidgetId);

        // Frame 2: press Down repeatedly. Focus should cycle to
        // modal-cancel, then stop (no escape to bg).
        context.BeginLayout(
            s_dimensions,
            new FrameInput(Keys: new[] { Key(ConsoleKey.DownArrow) })
        );
        context.RegisterFocusable("bg-1");
        context.RegisterFocusable("bg-2");
        context.OpenCaptureInput();
        context.RegisterFocusable("modal-ok");
        context.RegisterFocusable("modal-cancel");
        context.CloseCaptureInput();
        context.EndLayout();

        Assert.AreEqual("modal-cancel", context.FocusedWidgetId);

        // Frame 3: another Down. Should stay on modal-cancel (last in scope).
        context.BeginLayout(
            s_dimensions,
            new FrameInput(Keys: new[] { Key(ConsoleKey.DownArrow) })
        );
        context.RegisterFocusable("bg-1");
        context.RegisterFocusable("bg-2");
        context.OpenCaptureInput();
        context.RegisterFocusable("modal-ok");
        context.RegisterFocusable("modal-cancel");
        context.CloseCaptureInput();
        context.EndLayout();

        Assert.AreEqual("modal-cancel", context.FocusedWidgetId);
    }

    [TestMethod]
    public void Capture_BackgroundFocusStatePersistsAndRestoresWhenClosed()
    {
        AsphaltContext context = new AsphaltContext();

        // Frame 1: just background, focus lands on bg-1 by default.
        context.BeginLayout(s_dimensions);
        context.RegisterFocusable("bg-1");
        context.RegisterFocusable("bg-2");
        context.EndLayout();
        Assert.AreEqual("bg-1", context.FocusedWidgetId);

        // Frame 2: move background focus to bg-2.
        context.BeginLayout(
            s_dimensions,
            new FrameInput(Keys: new[] { Key(ConsoleKey.DownArrow) })
        );
        context.RegisterFocusable("bg-1");
        context.RegisterFocusable("bg-2");
        context.EndLayout();
        Assert.AreEqual("bg-2", context.FocusedWidgetId);

        // Frame 3: open modal. Focus reports modal's first child.
        context.BeginLayout(s_dimensions);
        context.RegisterFocusable("bg-1");
        context.RegisterFocusable("bg-2");
        context.OpenCaptureInput();
        context.RegisterFocusable("modal-ok");
        context.CloseCaptureInput();
        context.EndLayout();
        Assert.AreEqual("modal-ok", context.FocusedWidgetId);

        // Frame 4: modal closes. Focus containment is one-frame deferred
        // (mirroring KeyDown capture suppression) so this frame still
        // walks from the previous frame's modal scope — no widget is
        // focused because the modal scope has no children this frame.
        context.BeginLayout(s_dimensions);
        context.RegisterFocusable("bg-1");
        context.RegisterFocusable("bg-2");
        context.EndLayout();
        Assert.IsNull(
            context.FocusedWidgetId,
            "modal scope is the active root for one more frame after close"
        );

        // Frame 5: background focus is fully restored to bg-2.
        context.BeginLayout(s_dimensions);
        context.RegisterFocusable("bg-1");
        context.RegisterFocusable("bg-2");
        context.EndLayout();
        Assert.AreEqual("bg-2", context.FocusedWidgetId);
    }

    [TestMethod]
    public void NestedCapture_InnerOwnsFocus()
    {
        AsphaltContext context = new AsphaltContext();
        context.BeginLayout(s_dimensions);

        context.RegisterFocusable("bg-1");

        context.OpenCaptureInput();
        context.RegisterFocusable("outer-modal");

        context.OpenCaptureInput();
        context.RegisterFocusable("inner-modal-a");
        context.RegisterFocusable("inner-modal-b");
        context.CloseCaptureInput();

        context.CloseCaptureInput();

        context.EndLayout();

        Assert.AreEqual("inner-modal-a", context.FocusedWidgetId);
        Assert.IsFalse(context.IsFocused("outer-modal"));
        Assert.IsFalse(context.IsFocused("bg-1"));
    }

    [TestMethod]
    public void Capture_FromPreviousFrame_SuppressesBackgroundFocusInSourceOrder()
    {
        // Regression: background widgets render BEFORE the modal opens in
        // source order. Without the one-frame-deferred fallback to the last
        // frame's active focus root, their register-time IsFocused query
        // returns true (because _activeFocusRoot is still null when they
        // register) and they consume keys like Enter before the modal's
        // button gets a chance.
        AsphaltContext context = new AsphaltContext();

        // Frame 1: modal opens for the first time. _activeFocusRoot is set
        // when OpenCaptureInput runs; bg widgets registered before that
        // still see themselves as focused on this frame (acceptable: the
        // user is currently pressing the key that opened the modal).
        context.BeginLayout(s_dimensions);
        context.RegisterFocusable("bg-1");
        context.OpenCaptureInput();
        context.RegisterFocusable("modal-ok");
        context.CloseCaptureInput();
        context.EndLayout();

        // Frame 2: modal still open. Background bg-1 renders BEFORE the
        // modal in source order, but its IsFocused query must return
        // false because the previous frame's modal scope is now the
        // effective focus root until the modal pushes its own scope this
        // frame.
        context.BeginLayout(s_dimensions);

        // Query order matters: the bg widget registers and queries focus
        // BEFORE OpenCaptureInput runs.
        WidgetInputState bg = context.RegisterFocusable("bg-1");
        bool bgFocusedBeforeModalOpens = bg.Focused;

        context.OpenCaptureInput();
        WidgetInputState modal = context.RegisterFocusable("modal-ok");
        bool modalFocusedAfterModalOpens = modal.Focused;
        context.CloseCaptureInput();
        context.EndLayout();

        Assert.IsFalse(
            bgFocusedBeforeModalOpens,
            "background must not be focused while modal is up"
        );
        Assert.IsTrue(modalFocusedAfterModalOpens, "modal widget must be focused");
    }

    [TestMethod]
    public void CaptureWithNoFocusables_NoFocusedWidget()
    {
        AsphaltContext context = new AsphaltContext();
        context.BeginLayout(s_dimensions);

        context.RegisterFocusable("bg-1");
        context.OpenCaptureInput();
        // No focusables registered inside.
        context.CloseCaptureInput();

        context.EndLayout();

        Assert.IsNull(context.FocusedWidgetId);
        Assert.IsFalse(context.IsFocused("bg-1"));
    }

    [TestMethod]
    public void CaptureFocusState_PersistsAcrossFramesAtSameDepth()
    {
        AsphaltContext context = new AsphaltContext();

        // Frame 1: modal shown, default focus on first child.
        context.BeginLayout(s_dimensions);
        context.OpenCaptureInput();
        context.RegisterFocusable("modal-ok");
        context.RegisterFocusable("modal-cancel");
        context.CloseCaptureInput();
        context.EndLayout();
        Assert.AreEqual("modal-ok", context.FocusedWidgetId);

        // Frame 2: same modal renders again, press Down.
        context.BeginLayout(
            s_dimensions,
            new FrameInput(Keys: new[] { Key(ConsoleKey.DownArrow) })
        );
        context.OpenCaptureInput();
        context.RegisterFocusable("modal-ok");
        context.RegisterFocusable("modal-cancel");
        context.CloseCaptureInput();
        context.EndLayout();
        Assert.AreEqual("modal-cancel", context.FocusedWidgetId);

        // Frame 3: same modal again, no input. Focus persists on modal-cancel.
        context.BeginLayout(s_dimensions);
        context.OpenCaptureInput();
        context.RegisterFocusable("modal-ok");
        context.RegisterFocusable("modal-cancel");
        context.CloseCaptureInput();
        context.EndLayout();
        Assert.AreEqual("modal-cancel", context.FocusedWidgetId);
    }
}
