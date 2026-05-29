// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt.Widgets;

using System.Runtime.CompilerServices;
using Asphalt.Rendering;

/// <summary>
/// A row-shaped, focusable item. Useful as the building block for lists,
/// menus, and pickers. Fills the available width by default so the focus
/// highlight spans the whole row.
/// </summary>
/// <remarks>
/// The widget holds no "selected" state of its own: the caller chooses
/// what (if any) highlight to draw by passing a <see cref="TextStyle"/>.
/// When the row is focused the framework additionally OR-s
/// <see cref="TextStyle.Reverse"/> into the caller-supplied style so focus
/// is always visible.
/// </remarks>
public static class SelectableWidget
{
    extension(AsphaltContext context)
    {
        /// <summary>
        /// Declares a focusable row for this frame.
        /// </summary>
        /// <returns>
        /// True the frame the row is activated by pressing
        /// <see cref="ConsoleKey.Enter"/> while focused. Enter is consumed
        /// so it does not fall through to other widgets.
        /// </returns>
        public bool Selectable(
            string label,
            TextStyle textStyle = TextStyle.None,
            Layout? layoutStyle = null,
            string uniqueKey = "",
            [CallerArgumentExpression(nameof(label))] string? labelExpression = null,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0
        )
        {
            label ??= "NULL";

            string id = $"{filePath}:{lineNumber}:{labelExpression}:{uniqueKey}";
            WidgetInputState inputState = context.RegisterFocusable(id);

            context.OpenElement(
                new Implementation(label, textStyle, inputState.Focused),
                layoutStyle ?? s_defaultStyle
            );
            context.CloseElement();

            return inputState.ConsumeKeys(static key => key.Key == ConsoleKey.Enter);
        }
    }

    private static readonly Layout s_defaultStyle = new()
    {
        Width = LayoutLength.Grow(),
        Height = LayoutLength.Fixed(1),
    };

    internal sealed record Implementation(string Label, TextStyle TextStyle, bool Focused) : IWidget
    {
        public WidgetLayout Measure()
        {
            Dimensions minimum = new(Label.Length, 1);
            Dimensions preferred = new(Label.Length, 1);
            return new WidgetLayout(minimum, preferred);
        }

        public void Render(Rect bounds, ICanvas canvas)
        {
            if (bounds.Dimensions.Width <= 0 || bounds.Dimensions.Height <= 0)
                return;

            TextStyle style = Focused ? TextStyle | TextStyle.Reverse : TextStyle;

            int width = bounds.Dimensions.Width;
            for (int x = 0; x < width; x++)
            {
                char character = x < Label.Length ? Label[x] : ' ';
                canvas.Draw(
                    new Position(bounds.Position.X + x, bounds.Position.Y),
                    character,
                    style: style
                );
            }
        }
    }
}
