// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt.Widgets;

using System.Runtime.CompilerServices;
using Asphalt.Rendering;

/// <summary>
/// Per-frame result of a <see cref="SelectableWidget.Selectable"/> call.
/// Implicitly converts to <see cref="Activated"/> so callers that only care
/// about activation can keep writing <c>if (context.Selectable(...))</c>.
/// </summary>
/// <param name="Activated">
/// <c>true</c> on the single frame in which Enter was pressed while the row
/// was focused; otherwise <c>false</c>.
/// </param>
/// <param name="Focused">
/// <c>true</c> while the row holds keyboard focus this frame.
/// </param>
public readonly record struct SelectableState(bool Activated, bool Focused)
{
    public static implicit operator bool(SelectableState state) => state.Activated;
}

/// <summary>
/// A row-shaped, focusable item. Useful as the building block for lists,
/// menus, and pickers. Fills the available width by default so the focus
/// highlight spans the whole row. Pressing <see cref="ConsoleKey.Enter"/>
/// while focused activates the item.
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
        /// <param name="label">Text drawn inside the row.</param>
        /// <param name="textStyle">
        /// Style applied to the row's text. Combined with
        /// <see cref="TextStyle.Reverse"/> when the row is focused.
        /// </param>
        /// <param name="layoutStyle">Optional layout overrides. Defaults to a
        /// row that grows horizontally and is one cell tall.</param>
        /// <param name="uniqueKey">
        /// Optional unique key to differentiate multiple selectables that
        /// share a call site (e.g. when rendered in a loop).
        /// </param>
        /// <param name="labelExpression">Compiler-supplied; do not pass.</param>
        /// <param name="filePath">Compiler-supplied; do not pass.</param>
        /// <param name="lineNumber">Compiler-supplied; do not pass.</param>
        public SelectableState Selectable(
            string label,
            TextStyle textStyle = TextStyle.None,
            LayoutStyle? layoutStyle = null,
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

            bool activated = inputState.ConsumeKeys(static key => key.Key == ConsoleKey.Enter);
            return new SelectableState(activated, inputState.Focused);
        }
    }

    private static readonly LayoutStyle s_defaultStyle = new()
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
