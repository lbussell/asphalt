// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt.Widgets;

public static class OverlayExtensions
{
    extension(AsphaltContext context)
    {
        /// <summary>
        /// Opens a detached overlay subtree positioned by <paramref name="anchor"/>.
        /// The overlay does not consume space in its parent and is rendered after
        /// the primary tree so it paints on top. Children compose normally with
        /// any widget. The overlay is fit-sized to its content by default;
        /// pass an explicit <paramref name="style"/> to control direction,
        /// padding, gap, or to grow on one or both axes.
        /// </summary>
        public ContainerScope Overlay(Anchor anchor = Anchor.Center, Layout? style = null)
        {
            context.OpenOverlay(anchor, style);
            return new ContainerScope(context.CloseElement);
        }
    }
}
