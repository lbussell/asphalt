// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

using Imtui.Rendering;

public sealed class InputTextWidget(
    string value,
    int cursor,
    bool focused,
    string? placeholder = null,
    TerminalColor backgroundColor = default,
    TerminalColor focusedBackgroundColor = default,
    TerminalColor placeholderColor = default
) : IWidget
{
    private const int DefaultPreferredWidth = 10;

    public string Value { get; } = value ?? throw new ArgumentNullException(nameof(value));
    public int Cursor { get; } = cursor;
    public bool Focused { get; } = focused;
    public string? Placeholder { get; } = placeholder;
    public TerminalColor BackgroundColor { get; } = backgroundColor;
    public TerminalColor FocusedBackgroundColor { get; } = focusedBackgroundColor;
    public TerminalColor PlaceholderColor { get; } = placeholderColor;

    private TerminalColor CurrentBackgroundColor =>
        Focused ? FocusedBackgroundColor : BackgroundColor;

    public WidgetLayout Measure()
    {
        // Reserve one extra column so the cursor can sit just after the last
        // character without overflowing the widget. Use the placeholder width
        // as a hint when the value is empty so empty inputs do not collapse to
        // a single cell.
        int preferredWidth = Math.Max(
            DefaultPreferredWidth,
            Math.Max(Value.Length + 1, Placeholder?.Length ?? 0)
        );
        return new WidgetLayout(new Dimensions(1, 1), new Dimensions(preferredWidth, 1));
    }

    public void Render(Rect bounds, ICanvas canvas)
    {
        if (bounds.Dimensions.Width <= 0 || bounds.Dimensions.Height <= 0)
            return;

        FillBackground(bounds, canvas);

        bool showPlaceholder = Value.Length == 0 && !Focused && Placeholder is { Length: > 0 };
        string displayText = showPlaceholder ? Placeholder! : Value;
        TerminalColor foregroundColor = showPlaceholder ? PlaceholderColor : default;

        int viewOffset = ComputeViewOffset(bounds.Dimensions.Width);

        int width = bounds.Dimensions.Width;
        for (int column = 0; column < width; column++)
        {
            int textIndex = column + viewOffset;
            char character = textIndex < displayText.Length ? displayText[textIndex] : ' ';
            canvas.Draw(
                new Position(bounds.Position.X + column, bounds.Position.Y),
                character,
                foregroundColor,
                CurrentBackgroundColor
            );
        }

        if (Focused)
            DrawCursor(bounds, viewOffset, canvas);
    }

    private int ComputeViewOffset(int width)
    {
        // When the text fits, no scrolling is needed. Otherwise, scroll so the
        // cursor is visible: keep the cursor inside [viewOffset, viewOffset+width-1].
        // Reserve the last column for the cursor itself so it remains visible
        // when positioned past the final character.
        if (Value.Length < width)
            return 0;

        int maxVisibleCursor = width - 1;
        return Cursor <= maxVisibleCursor ? 0 : Cursor - maxVisibleCursor;
    }

    private void DrawCursor(Rect bounds, int viewOffset, ICanvas canvas)
    {
        int cursorColumn = Cursor - viewOffset;
        if (cursorColumn < 0 || cursorColumn >= bounds.Dimensions.Width)
            return;

        char characterUnderCursor = Cursor < Value.Length ? Value[Cursor] : ' ';

        // Render the cursor as a reverse-video cell: background becomes the
        // text color and vice versa, which works on any terminal without
        // requiring a dedicated cursor-style capability on the canvas.
        canvas.Draw(
            new Position(bounds.Position.X + cursorColumn, bounds.Position.Y),
            characterUnderCursor,
            foregroundColor: CurrentBackgroundColor,
            backgroundColor: TerminalColor.White
        );
    }

    private void FillBackground(Rect bounds, ICanvas canvas)
    {
        for (int y = 0; y < bounds.Dimensions.Height; y++)
        {
            for (int x = 0; x < bounds.Dimensions.Width; x++)
            {
                canvas.Draw(
                    new Position(bounds.Position.X + x, bounds.Position.Y + y),
                    ' ',
                    backgroundColor: CurrentBackgroundColor
                );
            }
        }
    }
}
