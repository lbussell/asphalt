// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

using Imtui.Rendering;

public static class PanelExtensions
{
    private static readonly Padding s_panelPadding = new(1, 0);
    private static readonly TerminalColor s_titleBackgroundColor = TerminalColor.Rgb(
        0x29,
        0x4A,
        0x80
    );
    private static readonly TerminalColor s_backgroundColor = TerminalColor.Rgb(0x0F, 0x0F, 0x0F);

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

            return new ContainerScope(context);
        }

        public ContainerScope Panel(
            string title,
            LayoutStyle? style = null,
            int? gap = null,
            Direction? direction = null,
            TerminalColor titleBackgroundColor = default,
            TerminalColor backgroundColor = default
        )
        {
            ArgumentNullException.ThrowIfNull(title);

            LayoutStyle layoutStyle =
                style
                ?? new LayoutStyle { Width = LayoutLength.Grow(), Height = LayoutLength.Fit() };
            Direction bodyDirection = direction ?? layoutStyle.Direction;

            if (titleBackgroundColor == default)
                titleBackgroundColor = s_titleBackgroundColor;

            if (backgroundColor == default)
                backgroundColor = s_backgroundColor;

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
                    backgroundColor: titleBackgroundColor
                )
            )
            {
                context.Text(
                    title,
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
                backgroundColor: backgroundColor
            );

            return new ContainerScope(context, closeCount: 2);
        }
    }
}
