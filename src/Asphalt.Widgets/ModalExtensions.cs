// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt.Widgets;

public static class ModalExtensions
{
    extension(AsphaltContext context)
    {
        /// <summary>
        /// Opens a modal — shorthand for an <see cref="OverlayExtensions.Overlay"/>
        /// combined with <see cref="CaptureInputExtensions.CaptureInput"/>. The
        /// overlay paints on top of the primary tree at <paramref name="anchor"/>,
        /// and input outside the modal is suppressed from the next frame onward.
        /// Add a <c>using (context.Panel(...))</c> inside if you want a bordered
        /// container.
        /// </summary>
        public ContainerScope Modal(Anchor anchor = Anchor.Center, LayoutStyle? style = null)
        {
            context.OpenOverlay(anchor, style);
            context.OpenCaptureInput();

            // Close in reverse order: capture first (innermost), then the
            // overlay element (outermost).
            return new ContainerScope(() =>
            {
                context.CloseCaptureInput();
                context.CloseElement();
            });
        }
    }
}
