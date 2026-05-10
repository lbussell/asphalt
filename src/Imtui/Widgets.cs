// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

public enum TextWrapMode
{
    Space,
    Force,
    Clip,
}

public sealed class BorderPanel(BorderStyle borderStyle) : IWidget, IMeasurableWidget
{
    private readonly BorderStyle _borderStyle = borderStyle;

    public BorderPanel()
        : this(BorderStyle.Round) { }

    public Dimensions Measure() => new(1, 1);

    public void Render(Rect bounds, ICanvas canvas)
    {
        int width = bounds.Dimensions.Width;
        int height = bounds.Dimensions.Height;

        if (width <= 0 || height <= 0)
            return;

        int left = bounds.Position.X;
        int top = bounds.Position.Y;
        int right = left + width - 1;
        int bottom = top + height - 1;

        DrawHorizontalBorder(
            canvas,
            left,
            right,
            top,
            _borderStyle.TopLeft,
            _borderStyle.TopRight,
            _borderStyle.Horizontal
        );

        for (int y = top + 1; y < bottom; y++)
        {
            Draw(canvas, left, y, _borderStyle.Vertical);
            if (width > 1)
                Draw(canvas, right, y, _borderStyle.Vertical);
        }

        if (height > 1)
            DrawHorizontalBorder(
                canvas,
                left,
                right,
                bottom,
                _borderStyle.BottomLeft,
                _borderStyle.BottomRight,
                _borderStyle.Horizontal
            );
    }

    private static void DrawHorizontalBorder(
        ICanvas canvas,
        int left,
        int right,
        int y,
        char leftCorner,
        char rightCorner,
        char horizontal
    )
    {
        Draw(canvas, left, y, leftCorner);

        for (int x = left + 1; x < right; x++)
            Draw(canvas, x, y, horizontal);

        if (right > left)
            Draw(canvas, right, y, rightCorner);
    }

    private static void Draw(ICanvas canvas, int x, int y, char character) =>
        canvas.Draw(new Position(x, y), character);
}

public static class BorderPanelWidgetExtensions
{
    public static ImtuiContext.WidgetScope BorderPanel(
        this ImtuiContext context,
        BorderStyle? borderStyle = null,
        LayoutStyle? style = null,
        Direction direction = Direction.Vertical
    )
    {
        return context.PushNode(
            direction,
            new BorderPanel(borderStyle ?? BorderStyle.Round),
            AddBorderPadding(style ?? LayoutStyle.Default)
        );
    }

    private static LayoutStyle AddBorderPadding(LayoutStyle style)
    {
        ArgumentNullException.ThrowIfNull(style);

        Padding padding = style.Padding;
        return style with
        {
            Padding = new Padding(
                padding.Left + 1,
                padding.Top + 1,
                padding.Right + 1,
                padding.Bottom + 1
            ),
        };
    }
}

public sealed class Text(string value, TextWrapMode wrapMode = TextWrapMode.Space)
    : IWidget,
        IMeasurableWidget,
        IConstrainedMeasurableWidget
{
    private readonly string _value = value ?? throw new ArgumentNullException(nameof(value));
    private readonly TextWrapMode _wrapMode = wrapMode;

    public Dimensions Measure()
    {
        return _wrapMode switch
        {
            TextWrapMode.Space => new Dimensions(GetLongestWordWidth(), CountLines()),
            TextWrapMode.Force => new Dimensions(HasRenderableCharacters() ? 1 : 0, CountLines()),
            TextWrapMode.Clip => new Dimensions(GetLongestLineWidth(), CountLines()),
            _ => throw new InvalidOperationException("Unknown text wrap mode."),
        };
    }

    public Dimensions Measure(Dimensions constraints)
    {
        Dimensions unconstrained = Measure();

        if (constraints.Width <= 0)
            return unconstrained;

        int height = _wrapMode switch
        {
            TextWrapMode.Space => GetSpaceWrappedLines(constraints.Width).Count,
            TextWrapMode.Force => CountForceWrappedLines(constraints.Width),
            TextWrapMode.Clip => unconstrained.Height,
            _ => throw new InvalidOperationException("Unknown text wrap mode."),
        };

        return new Dimensions(unconstrained.Width, height);
    }

    public void Render(Rect bounds, ICanvas canvas)
    {
        if (bounds.Dimensions.Width <= 0 || bounds.Dimensions.Height <= 0)
            return;

        switch (_wrapMode)
        {
            case TextWrapMode.Space:
                RenderLines(GetSpaceWrappedLines(bounds.Dimensions.Width), bounds, canvas);
                break;
            case TextWrapMode.Force:
                RenderForceWrapped(bounds, canvas);
                break;
            case TextWrapMode.Clip:
                RenderClipped(bounds, canvas);
                break;
        }
    }

    private int GetLongestWordWidth()
    {
        int width = 0;
        int wordWidth = 0;

        foreach (char character in _value)
        {
            if (character == '\r')
                continue;

            if (character == ' ' || character == '\n')
            {
                width = Math.Max(width, wordWidth);
                wordWidth = 0;
                continue;
            }

            wordWidth++;
        }

        return Math.Max(width, wordWidth);
    }

    private int GetLongestLineWidth()
    {
        int width = 0;
        int lineWidth = 0;

        foreach (char character in _value)
        {
            if (character == '\r')
                continue;

            if (character == '\n')
            {
                width = Math.Max(width, lineWidth);
                lineWidth = 0;
                continue;
            }

            lineWidth++;
        }

        return Math.Max(width, lineWidth);
    }

    private int CountLines()
    {
        int height = 1;

        foreach (char character in _value)
        {
            if (character == '\n')
                height++;
        }

        return height;
    }

    private bool HasRenderableCharacters()
    {
        foreach (char character in _value)
        {
            if (character != '\r' && character != '\n')
                return true;
        }

        return false;
    }

    private List<string> GetSpaceWrappedLines(int width)
    {
        List<string> lines = [];
        string line = "";
        string word = "";

        foreach (char character in _value)
        {
            if (character == '\r')
                continue;

            if (character == ' ')
            {
                AddWord();
                continue;
            }

            if (character == '\n')
            {
                AddWord();
                AddLine();
                continue;
            }

            word += character;
        }

        AddWord();
        AddLine();
        return lines;

        void AddWord()
        {
            if (word.Length == 0)
                return;

            if (line.Length == 0)
            {
                line = word;
            }
            else if (line.Length + 1 + word.Length <= width)
            {
                line += " " + word;
            }
            else
            {
                AddLine();
                line = word;
            }

            word = "";
        }

        void AddLine()
        {
            lines.Add(line);
            line = "";
        }
    }

    private int CountForceWrappedLines(int width)
    {
        int height = 1;
        int x = 0;

        foreach (char character in _value)
        {
            if (character == '\r')
                continue;

            if (character == '\n')
            {
                x = 0;
                height++;
                continue;
            }

            if (x >= width)
            {
                x = 0;
                height++;
            }

            x++;
        }

        return height;
    }

    private static void RenderLines(List<string> lines, Rect bounds, ICanvas canvas)
    {
        int y = 0;

        foreach (string line in lines)
        {
            if (y >= bounds.Dimensions.Height)
                return;

            DrawLine(line, bounds, canvas, y);
            y++;
        }
    }

    private void RenderForceWrapped(Rect bounds, ICanvas canvas)
    {
        int x = 0;
        int y = 0;

        foreach (char character in _value)
        {
            if (character == '\r')
                continue;

            if (character == '\n')
            {
                x = 0;
                y++;
                if (y >= bounds.Dimensions.Height)
                    return;

                continue;
            }

            if (x >= bounds.Dimensions.Width)
            {
                x = 0;
                y++;
                if (y >= bounds.Dimensions.Height)
                    return;
            }

            canvas.Draw(new Position(bounds.Position.X + x, bounds.Position.Y + y), character);
            x++;
        }
    }

    private void RenderClipped(Rect bounds, ICanvas canvas)
    {
        int x = 0;
        int y = 0;

        foreach (char character in _value)
        {
            if (character == '\r')
                continue;

            if (character == '\n')
            {
                x = 0;
                y++;
                if (y >= bounds.Dimensions.Height)
                    return;

                continue;
            }

            if (x < bounds.Dimensions.Width)
                canvas.Draw(new Position(bounds.Position.X + x, bounds.Position.Y + y), character);

            x++;
        }
    }

    private static void DrawLine(string line, Rect bounds, ICanvas canvas, int y)
    {
        int width = Math.Min(line.Length, bounds.Dimensions.Width);

        for (int x = 0; x < width; x++)
            canvas.Draw(new Position(bounds.Position.X + x, bounds.Position.Y + y), line[x]);
    }
}

public static class TextWidgetExtensions
{
    public static void Text(
        this ImtuiContext context,
        string value,
        LayoutStyle? style = null,
        TextWrapMode wrapMode = TextWrapMode.Space
    )
    {
        Text text = new Text(value, wrapMode);
        context.AddWidget(text, ResolveStyle(style, text.Measure().Width, wrapMode));
    }

    private static LayoutStyle ResolveStyle(
        LayoutStyle? style,
        int minimumWidth,
        TextWrapMode wrapMode
    )
    {
        LayoutStyle resolvedStyle =
            style
            ?? new LayoutStyle
            {
                Width =
                    wrapMode == TextWrapMode.Clip
                        ? LayoutLength.Fit()
                        : LayoutLength.Grow(minimumWidth),
                Height = LayoutLength.Fit(),
            };

        if (wrapMode != TextWrapMode.Space)
            return resolvedStyle;

        return resolvedStyle with
        {
            Width = AddMinimumWidth(resolvedStyle.Width, minimumWidth),
        };
    }

    private static LayoutLength AddMinimumWidth(LayoutLength length, int minimumWidth)
    {
        if (length.Kind == LayoutLengthKind.Fixed)
            return LayoutLength.Fixed(Math.Max(length.Value, minimumWidth));

        int minimum = Math.Max(length.Minimum, minimumWidth);
        int maximum = Math.Max(length.Maximum, minimum);

        return length.Kind == LayoutLengthKind.Fit
            ? LayoutLength.Fit(minimum, maximum)
            : LayoutLength.Grow(minimum, maximum);
    }
}
