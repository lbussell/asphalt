// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt.Widgets;

public static class CaptureInputExtensions
{
    extension(AsphaltContext context)
    {
        /// <summary>
        /// Opens an input capture scope. Keys checked via
        /// <see cref="AsphaltContext.KeyDown(ConsoleKey)"/> and hints registered
        /// via <see cref="AsphaltContext.AddShortcutHint(string, string)"/>
        /// inside the scope behave normally. Calls outside the scope on
        /// subsequent frames are suppressed while any capture remains open,
        /// so application-level hotkeys like "Q: Quit" stop firing while a
        /// modal is visible.
        /// </summary>
        /// <remarks>
        /// Capture is deferred by one frame; see
        /// <see cref="AsphaltContext.OpenCaptureInput"/> for the full rule.
        /// </remarks>
        public ContainerScope CaptureInput()
        {
            context.OpenCaptureInput();
            return new ContainerScope(context.CloseCaptureInput);
        }
    }
}
