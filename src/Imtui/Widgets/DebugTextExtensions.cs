// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

using System.Globalization;

public static class DebugTextExtensions
{
    extension(ImtuiContext context)
    {
        // Renders a multi-line block of runtime debug information using the
        // built-in Text widget. Useful for diagnosing layout, input, and
        // performance while developing.
        public void DebugText()
        {
            context.Text(
                $"""
                frames: {context.FrameCount}
                last frame: {context.LastFrameTime.TotalMilliseconds:F2} ms
                size: {context.Dimensions.Width}(w) x {context.Dimensions.Height}(h)
                focus: {context.FocusedWidgetId ?? "(none)"}
                """,
                wrappingMode: TextWrappingMode.Force
            );
        }
    }
}
