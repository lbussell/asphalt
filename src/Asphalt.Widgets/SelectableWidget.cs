// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt.Widgets;

using System.Runtime.CompilerServices;
using Asphalt.Rendering;

/// <summary>
/// A row-shaped, selectable item. Useful as the building block for lists,
/// menus, and pickers. Fills the available width by default so the selection
/// highlight spans the whole row. Pressing <see cref="ConsoleKey.Enter"/>
/// while focused activates the item.
/// </summary>
public static class SelectableWidget
{
    extension(AsphaltContext context)
    {
        /// <summary>
        /// Declares a selectable row for this frame. The caller owns the
        /// <paramref name="selected"/> state.
        /// </summary>
        /// <param name="label">Text drawn inside the row.</param>
        /// <param name="selected">
        /// Whether this row is currently selected. Drives the selected
        /// highlight even when the row is not focused.
        /// </param>
        /// <param name="style">Optional layout overrides. Defaults to a
        /// row that grows horizontally and is one cell tall.</param>
        /// <param name="uniqueKey">
        /// Optional unique key to differentiate multiple selectables that
        /// share a call site (e.g. when rendered in a loop).
        /// </param>
        /// <param name="labelExpression">Compiler-supplied; do not pass.</param>
        /// <param name="filePath">Compiler-supplied; do not pass.</param>
        /// <param name="lineNumber">Compiler-supplied; do not pass.</param>
        /// <returns>
        /// <c>true</c> on the single frame in which Enter was pressed while
        /// the row was focused; otherwise <c>false</c>.
        /// </returns>
        public bool Selectable(
            string label,
            bool selected = false,
            LayoutStyle? style = null,
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
                new Implementation(label, selected, inputState.Focused),
                style ?? s_defaultStyle
            );
            context.CloseElement();

            return inputState.ConsumeKeys(static key => key.Key == ConsoleKey.Enter);
        }

        /// <summary>
        /// Declares a selectable row whose <paramref name="selected"/> state
        /// the widget toggles in response to Enter.
        /// </summary>
        /// <param name="label">Text drawn inside the row.</param>
        /// <param name="selected">
        /// Toggled between <c>true</c> and <c>false</c> each time Enter is
        /// pressed while the row is focused.
        /// </param>
        /// <param name="style">Optional layout overrides. Defaults to a
        /// row that grows horizontally and is one cell tall.</param>
        /// <param name="uniqueKey">
        /// Optional unique key to differentiate multiple selectables that
        /// share a call site (e.g. when rendered in a loop).
        /// </param>
        /// <param name="labelExpression">Compiler-supplied; do not pass.</param>
        /// <param name="filePath">Compiler-supplied; do not pass.</param>
        /// <param name="lineNumber">Compiler-supplied; do not pass.</param>
        /// <returns>
        /// <c>true</c> on the single frame in which <paramref name="selected"/>
        /// was toggled; otherwise <c>false</c>.
        /// </returns>
        public bool Selectable(
            string label,
            ref bool selected,
            LayoutStyle? style = null,
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
                new Implementation(label, selected, inputState.Focused),
                style ?? s_defaultStyle
            );
            context.CloseElement();

            bool toggled = inputState.ConsumeKeys(static key => key.Key == ConsoleKey.Enter);
            if (toggled)
                selected = !selected;

            return toggled;
        }
    }

    private static readonly LayoutStyle s_defaultStyle = new()
    {
        Width = LayoutLength.Grow(),
        Height = LayoutLength.Fixed(1),
    };

    internal sealed record Implementation(string Label, bool Selected, bool Focused) : IWidget
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

            // Focus implies selected: the focused row always shows the
            // selected highlight. Bold is layered on top so callers can still
            // tell focus apart from a merely-selected unfocused row.
            TextStyle style = TextStyle.None;
            if (Selected || Focused)
                style |= TextStyle.Reverse;
            if (Focused)
                style |= TextStyle.Bold;

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
