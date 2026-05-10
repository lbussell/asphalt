// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

using Imtui.Rendering;

public sealed class TextWidget(
    string text,
    TextWrappingMode wrappingMode = TextWrappingMode.Wrap,
    TerminalColor foregroundColor = default,
    TerminalColor backgroundColor = default
) : IWidget
{
    private Dimensions _contentDimensions;
    private string[] _lines = [];
    private int _layoutWidth = -1;

    public string Text { get; } = text ?? throw new ArgumentNullException(nameof(text));
    public TextWrappingMode WrappingMode { get; } = wrappingMode;
    public TerminalColor ForegroundColor { get; } = foregroundColor;
    public TerminalColor BackgroundColor { get; } = backgroundColor;

    public WidgetLayout Measure()
    {
        Dimensions minimum = new(GetMinimumWidth(), Text.Length == 0 ? 0 : 1);
        Dimensions preferred = new(GetLongestLineLength(Text), GetLineCount(Text));
        return new WidgetLayout(minimum, preferred);
    }

    public Dimensions Layout(Dimensions available)
    {
        if (available.Width <= 0 || available.Height <= 0)
        {
            _layoutWidth = available.Width;
            _contentDimensions = new Dimensions(0, 0);
            _lines = [];
            return _contentDimensions;
        }

        if (available.Width == _layoutWidth)
            return _contentDimensions;

        string wrapped = TextWrapper.WrapText(Text, available.Width, WrappingMode, out int height);
        _lines = GetLines(wrapped);
        _layoutWidth = available.Width;
        _contentDimensions = new Dimensions(GetLongestLineLength(_lines), height);
        return _contentDimensions;
    }

    public void Render(Rect bounds, ICanvas canvas)
    {
        if (bounds.Dimensions.Width <= 0 || bounds.Dimensions.Height <= 0)
            return;

        if (BackgroundColor != default)
            FillBackground(bounds, canvas);

        int height = Math.Min(bounds.Dimensions.Height, _lines.Length);
        for (int y = 0; y < height; y++)
        {
            string line = _lines[y];
            int width = Math.Min(bounds.Dimensions.Width, line.Length);
            for (int x = 0; x < width; x++)
            {
                canvas.Draw(
                    new Position(bounds.Position.X + x, bounds.Position.Y + y),
                    line[x],
                    ForegroundColor,
                    BackgroundColor
                );
            }
        }
    }

    private int GetMinimumWidth() =>
        Text.Length == 0 ? 0
        : WrappingMode == TextWrappingMode.Wrap ? Math.Max(1, GetLongestWordLength(Text))
        : 1;

    private void FillBackground(Rect bounds, ICanvas canvas)
    {
        for (int y = 0; y < bounds.Dimensions.Height; y++)
        {
            for (int x = 0; x < bounds.Dimensions.Width; x++)
            {
                canvas.Draw(
                    new Position(bounds.Position.X + x, bounds.Position.Y + y),
                    ' ',
                    backgroundColor: BackgroundColor
                );
            }
        }
    }

    private static string[] GetLines(string value) => value.Length == 0 ? [] : value.Split('\n');

    private static int GetLineCount(string value) =>
        value.Length == 0 ? 0 : value.Count(character => character == '\n') + 1;

    private static int GetLongestLineLength(string value)
    {
        int length = 0;

        foreach (ReadOnlySpan<char> line in value.AsSpan().EnumerateLines())
            length = Math.Max(length, line.Length);

        return length;
    }

    private static int GetLongestLineLength(string[] lines)
    {
        int length = 0;

        foreach (string line in lines)
            length = Math.Max(length, line.Length);

        return length;
    }

    private static int GetLongestWordLength(string value)
    {
        int longest = 0;
        int current = 0;

        foreach (char character in value)
        {
            if (char.IsWhiteSpace(character) || character == '-')
            {
                longest = Math.Max(longest, current);
                current = 0;
            }
            else
            {
                current++;
            }
        }

        return Math.Max(longest, current);
    }
}
