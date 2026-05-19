// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

using System.Runtime.CompilerServices;
using Imtui.Rendering;

public static class PanelExtensions
{
    extension(ImtuiContext context)
    {
        public ContainerScope Panel(
            string? title = "",
            BorderStyle? borderStyle = null,
            LayoutStyle? style = null,
            Padding padding = default,
            int? gap = null,
            Direction? direction = null,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string memberName = ""
        )
        {
            LayoutStyle layoutStyle = style ?? LayoutStyle.Default;
            Direction bodyDirection = direction ?? layoutStyle.Direction;

            string id = $"{filePath}:{lineNumber}:{memberName}:Panel:{title}";
            context.PushFocusScope(id);

            TerminalColor borderColor = context.IsFocused(id)
                ? context.Theme.BorderFocused
                : context.Theme.Border;

            context.OpenElement(
                new PanelWidget(borderStyle ?? BorderStyle.Round, title, padding, borderColor),
                layoutStyle with
                {
                    Direction = bodyDirection,
                    ChildGap = gap ?? layoutStyle.ChildGap,
                    Padding = new Padding(
                        padding.Left + 1,
                        padding.Top + 1,
                        padding.Right + 1,
                        padding.Bottom + 1
                    ),
                }
            );

            return new ContainerScope(() =>
            {
                context.CloseElement();
                context.CloseFocusScope();
            });
        }
    }
}
