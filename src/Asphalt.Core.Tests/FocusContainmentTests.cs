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

        // Frame 4: modal closes. Background focus is restored to bg-2.
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
