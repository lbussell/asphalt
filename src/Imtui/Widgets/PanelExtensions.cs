// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

using System.Runtime.CompilerServices;
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
            TerminalColor backgroundColor = default,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string memberName = ""
        )
        {
            LayoutStyle layoutStyle = style ?? LayoutStyle.Default;
            Direction bodyDirection = direction ?? layoutStyle.Direction;

            string id = $"{filePath}:{lineNumber}:{memberName}:Panel";
            context.PushFocusScope(id);

            OpenPanelElement(context, layoutStyle, padding, gap, bodyDirection, backgroundColor);

            return new ContainerScope(() =>
            {
                context.CloseElement();
                context.CloseFocusScope();
            });
        }

        public ContainerScope Panel(
            string title,
            LayoutStyle? style = null,
            int? gap = null,
            Direction? direction = null,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string memberName = ""
        )
        {
            ArgumentNullException.ThrowIfNull(title);

            LayoutStyle layoutStyle =
                style
                ?? new LayoutStyle { Width = LayoutLength.Fit(), Height = LayoutLength.Fit() };
            Direction bodyDirection = direction ?? layoutStyle.Direction;

            string id = $"{filePath}:{lineNumber}:{memberName}:Panel:{title}";
            context.PushFocusScope(id);

            Theme theme = context.Theme;

            // Outer wrapper: vertical stack of [header, body]. Pure layout,
            // no nested focus scope — focus lives on the single scope we just
            // pushed, and the body it wraps is where user widgets register.
            context.OpenElement(
                style: layoutStyle with
                {
                    Direction = Direction.Vertical,
                    ChildGap = 0,
                }
            );

            // Header bar (title text). Layout-only.
            OpenPanelElement(
                context,
                new LayoutStyle { Width = LayoutLength.Grow(), Height = LayoutLength.Fit() },
                padding: s_panelPadding,
                gap: null,
                direction: Direction.Vertical,
                backgroundColor: theme.SurfaceFocused
            );
            context.Text(
                "▼ " + title,
                new LayoutStyle { Width = LayoutLength.Grow(), Height = LayoutLength.Fit() },
                TextWrappingMode.Truncate
            );
            context.CloseElement();

            // Body. Layout-only; the focus scope we already pushed plays the
            // role of "this panel".
            OpenPanelElement(
                context,
                new LayoutStyle { Width = LayoutLength.Grow(), Height = LayoutLength.Grow() },
                padding: s_panelPadding,
                gap: gap ?? layoutStyle.ChildGap,
                direction: bodyDirection,
                backgroundColor: theme.PanelBackground
            );

            return new ContainerScope(() =>
            {
                context.CloseElement(); // body
                context.CloseElement(); // outer wrapper
                context.CloseFocusScope();
            });
        }
    }

    private static void OpenPanelElement(
        ImtuiContext context,
        LayoutStyle layoutStyle,
        Padding padding,
        int? gap,
        Direction direction,
        TerminalColor backgroundColor
    )
    {
        context.OpenElement(
            new PanelWidget(padding, backgroundColor),
            layoutStyle with
            {
                Direction = direction,
                ChildGap = gap ?? layoutStyle.ChildGap,
                Padding = padding,
            }
        );
    }
}
