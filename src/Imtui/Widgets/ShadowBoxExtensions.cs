// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

using Imtui.Rendering;

public static class ShadowBoxExtensions
{
    extension(ImtuiContext context)
    {
        public ContainerScope ShadowBox(
            LayoutStyle? style = null,
            Padding padding = default,
            int? gap = null,
            Direction? direction = null,
            TerminalColor shadowColor = default
        )
        {
            LayoutStyle layoutStyle = style ?? LayoutStyle.Default;
            context.OpenElement(
                new ShadowBoxWidget(padding, shadowColor),
                layoutStyle with
                {
                    Direction = direction ?? layoutStyle.Direction,
                    ChildGap = gap ?? layoutStyle.ChildGap,
                    Padding = new Padding(
                        padding.Left,
                        padding.Top,
                        padding.Right + 1,
                        padding.Bottom + 1
                    ),
                }
            );

            return new ContainerScope(context.CloseElement);
        }
    }
}
