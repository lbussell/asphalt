// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

using Imtui.Rendering;

public static class PanelExtensions
{
    private static readonly Padding s_panelPadding = new(1, 0);

    extension(ImtuiContext context)
    {
        public ContainerScope Panel(
            LayoutStyle? style = null,
            Padding padding = default,
            int? gap = null,
            Direction? direction = null,
            TerminalColor backgroundColor = default
        )
        {
            LayoutStyle layoutStyle = style ?? LayoutStyle.Default;
            context.OpenElement(
                new PanelWidget(padding, backgroundColor),
                layoutStyle with
                {
                    Direction = direction ?? layoutStyle.Direction,
                    ChildGap = gap ?? layoutStyle.ChildGap,
                    Padding = padding,
                }
            );

            return new ContainerScope(context.CloseElement);
        }

        public ContainerScope Panel(
            string title,
            LayoutStyle? style = null,
            int? gap = null,
            Direction? direction = null
        )
        {
            ArgumentNullException.ThrowIfNull(title);

            LayoutStyle layoutStyle =
                style
                ?? new LayoutStyle { Width = LayoutLength.Fit(), Height = LayoutLength.Fit() };
            Direction bodyDirection = direction ?? layoutStyle.Direction;

            Theme theme = context.Theme;

            context.OpenElement(
                style: layoutStyle with
                {
                    Direction = Direction.Vertical,
                    ChildGap = 0,
                }
            );

            using (
                context.Panel(
                    style: new LayoutStyle
                    {
                        Width = LayoutLength.Grow(),
                        Height = LayoutLength.Fit(),
                    },
                    padding: s_panelPadding,
                    backgroundColor: theme.SurfaceFocused
                )
            )
            {
                context.Text(
                    "▼ " + title,
                    new LayoutStyle { Width = LayoutLength.Grow(), Height = LayoutLength.Fit() },
                    TextWrappingMode.Truncate
                );
            }

            context.Panel(
                style: new LayoutStyle
                {
                    Width = LayoutLength.Grow(),
                    Height = LayoutLength.Grow(),
                },
                padding: s_panelPadding,
                gap: gap ?? layoutStyle.ChildGap,
                direction: bodyDirection,
                backgroundColor: theme.PanelBackground
            );

            return new ContainerScope(() =>
            {
                context.CloseElement();
                context.CloseElement();
            });
        }
    }
}
