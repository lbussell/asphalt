// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Tests;

// A minimal harness for exercising an Imtui application inside a unit test. It
// runs a single frame against an isolated ImtuiContext sized to the given
// terminal dimensions and returns the resulting layout tree so tests can make
// structural assertions about how widgets were laid out.
public static class ImtuiTestHarness
{
    public static LayoutNode RunFrame(Action<ImtuiContext> frame, Dimensions terminalDimensions)
    {
        ArgumentNullException.ThrowIfNull(frame);

        ImtuiContext context = new ImtuiContext();
        context.BeginLayout(terminalDimensions);
        frame(context);
        return context.EndLayout();
    }

    public static LayoutNode RunFrame(Action<ImtuiContext> frame, int width, int height) =>
        RunFrame(frame, new Dimensions(width, height));
}
