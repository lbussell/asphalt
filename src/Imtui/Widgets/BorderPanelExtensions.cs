// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

public static class BorderPanelExtensions
{
    extension(ImtuiContext context)
    {
        public ContainerScope BorderPanel(
            string? title = "",
            BorderStyle? borderStyle = null,
            LayoutStyle? style = null,
            Padding padding = default,
            int? gap = null,
            Direction? direction = null
        )
        {
            LayoutStyle layoutStyle = style ?? LayoutStyle.Default;
            context.OpenElement(
                new BorderPanelWidget(
                    borderStyle ?? BorderStyle.Round,
                    title,
                    padding,
                    context.Theme.Border,
                    backgroundColor: default
                ),
                layoutStyle with
                {
                    Direction = direction ?? layoutStyle.Direction,
                    ChildGap = gap ?? layoutStyle.ChildGap,
                    Padding = new Padding(
                        padding.Left + 1,
                        padding.Top + 1,
                        padding.Right + 1,
                        padding.Bottom + 1
                    ),
                }
            );

            return new ContainerScope(context.CloseElement);
        }
    }
}
