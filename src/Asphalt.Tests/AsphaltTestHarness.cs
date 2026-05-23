// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt.Tests;

// A minimal harness for exercising an Asphalt application inside a unit test. It
// runs a single frame against an isolated AsphaltContext sized to the given
// terminal dimensions and returns the resulting layout tree so tests can make
// structural assertions about how widgets were laid out.
public static class AsphaltTestHarness
{
    public static LayoutNode RunFrame(
        Action<AsphaltContext> frame,
        Dimensions terminalDimensions,
        FrameInput input = default
    )
    {
        ArgumentNullException.ThrowIfNull(frame);

        AsphaltContext context = new AsphaltContext();
        context.BeginLayout(terminalDimensions, input);
        frame(context);
        return context.EndLayout();
    }

    public static LayoutNode RunFrame(Action<AsphaltContext> frame, int width, int height) =>
        RunFrame(frame, new Dimensions(width, height));
}
