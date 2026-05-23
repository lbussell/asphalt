#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:project ../src/Asphalt/Asphalt.csproj

using Asphalt;
using Asphalt.Rendering;
using Asphalt.Widgets;

const string keyboardDiagram = """
┌───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───────┐
│ ` │ 1 │ 2 │ 3 │ 4 │ 5 │ 6 │ 7 │ 8 │ 9 │ 0 │ - │ = │ bksp  │
├───┴─┬─┴─┬─┴─┬─┴─┬─┴─┬─┴─┬─┴─┬─┴─┬─┴─┬─┴─┬─┴─┬─┴─┬─┴─┬─────┤
│ tab │ q │ w │ e │ r │ t │ y │ u │ i │ o │ p │ [ │ ] │  \  │
├─────┴┬──┴┬──┴┬──┴┬──┴┬──┴┬──┴┬──┴┬──┴┬──┴┬──┴┬──┴┬──┴─────┤
│ caps │ a │ s │ d │ f │ g │ h │ j │ k │ l │ ; │ ' │ enter  │
├──────┴─┬─┴─┬─┴─┬─┴─┬─┴─┬─┴─┬─┴─┬─┴─┬─┴─┬─┴─┬─┴─┬─┴────────┤
│ shift  │ z │ x │ c │ v │ b │ n │ m │ , │ . │ / │  shift   │
├────┬───┴┬──┴─┬─┴───┴───┴───┴───┴───┴──┬┴───┼───┴┬────┬────┤
│ctl │gui │alt │         space          │alt │gui │menu│ctl │
└────┴────┴────┴────────────────────────┴────┴────┴────┴────┘
""";

string[] keyboardLines = keyboardDiagram.Split('\n');

// Parse cells by scanning each content row for │ separators. The cell label
// is whatever text sits between two consecutive separators on the same row;
// its bounding box is the interior column range, which is what we highlight
// when that key is pressed.
List<KeyCell> cells = [];
for (int row = 1; row < keyboardLines.Length; row += 2)
{
    string line = keyboardLines[row];
    int? lastBar = null;
    for (int column = 0; column < line.Length; column++)
    {
        if (line[column] != '│')
            continue;

        if (lastBar is int start)
        {
            string label = line.Substring(start + 1, column - start - 1).Trim();
            if (label.Length > 0)
                cells.Add(new KeyCell(row, start + 1, column - 1, label));
        }
        lastBar = column;
    }
}

Dictionary<string, KeyCell> cellByLabel = new(StringComparer.OrdinalIgnoreCase);
foreach (KeyCell cell in cells)
    cellByLabel[cell.Label] = cell;

TimeSpan highlightDuration = TimeSpan.FromMilliseconds(500);
KeyCell? highlight = null;
TimeSpan? highlightSetAt = null;
string lastKeyDescription = "(press a key)";
int keyCount = 0;

AsphaltApplication.Run(asphalt =>
{
    bool quit = false;
    asphalt.ConsumeKeys(key =>
    {
        if (key.Key == ConsoleKey.Escape)
        {
            asphalt.QuitAfterThisFrame();
            quit = true;
            return;
        }

        keyCount += 1;
        lastKeyDescription = DescribeKey(key);

        if (FindCellForKey(key, cellByLabel) is KeyCell cell)
        {
            highlight = cell;
            highlightSetAt = asphalt.Time;
        }
    });

    if (quit)
        return;

    if (highlightSetAt is TimeSpan setAt)
    {
        TimeSpan elapsed = asphalt.Time - setAt;
        if (elapsed >= highlightDuration)
        {
            highlight = null;
            highlightSetAt = null;
        }
        else
        {
            asphalt.RequestRedrawIn(highlightDuration - elapsed);
        }
    }

    using (asphalt.Panel("Keyboard Demo"))
    {
        asphalt.HRule("Press escape to quit");
        asphalt.OpenElement(new KeyboardWidget(keyboardLines, highlight));
        asphalt.CloseElement();
        asphalt.Text($"Last key: {lastKeyDescription}");
        asphalt.Text($"Keys pressed: {keyCount}");
    }
});

static KeyCell? FindCellForKey(ConsoleKeyInfo key, Dictionary<string, KeyCell> cellByLabel)
{
    string? specialLabel = key.Key switch
    {
        ConsoleKey.Backspace => "bksp",
        ConsoleKey.Tab => "tab",
        ConsoleKey.Enter => "enter",
        ConsoleKey.Spacebar => "space",
        _ => null,
    };

    if (specialLabel is not null && cellByLabel.TryGetValue(specialLabel, out KeyCell? cell))
        return cell;

    if (key.KeyChar != '\0' && !char.IsControl(key.KeyChar))
    {
        string label = char.ToLowerInvariant(key.KeyChar).ToString();
        if (cellByLabel.TryGetValue(label, out KeyCell? printable))
            return printable;
    }

    return null;
}

static string DescribeKey(ConsoleKeyInfo key)
{
    string prefix = "";
    if (key.Modifiers.HasFlag(ConsoleModifiers.Control))
        prefix += "Ctrl+";
    if (key.Modifiers.HasFlag(ConsoleModifiers.Alt))
        prefix += "Alt+";
    if (key.Modifiers.HasFlag(ConsoleModifiers.Shift))
        prefix += "Shift+";

    if (key.KeyChar != '\0' && !char.IsControl(key.KeyChar))
        return $"{prefix}{key.KeyChar} ({key.Key})";

    return $"{prefix}{key.Key}";
}

internal sealed record KeyCell(int Line, int XStart, int XEnd, string Label);

internal sealed class KeyboardWidget(string[] lines, KeyCell? highlight) : IWidget
{
    private static readonly TerminalColor s_highlightBackground = TerminalColor.Rgb(
        0xE2,
        0x90,
        0x4A
    );

    public WidgetLayout Measure()
    {
        int width = 0;
        foreach (string line in lines)
            width = Math.Max(width, line.Length);

        Dimensions dimensions = new Dimensions(width, lines.Length);
        return new WidgetLayout(dimensions, dimensions);
    }

    public void Render(Rect bounds, ICanvas canvas)
    {
        int rows = Math.Min(bounds.Dimensions.Height, lines.Length);
        for (int y = 0; y < rows; y++)
        {
            string line = lines[y];
            int width = Math.Min(bounds.Dimensions.Width, line.Length);
            for (int x = 0; x < width; x++)
            {
                bool isHighlighted =
                    highlight is not null
                    && y == highlight.Line
                    && x >= highlight.XStart
                    && x <= highlight.XEnd;

                canvas.Draw(
                    new Position(bounds.Position.X + x, bounds.Position.Y + y),
                    line[x],
                    foregroundColor: isHighlighted ? TerminalColor.Black : default,
                    backgroundColor: isHighlighted ? s_highlightBackground : default
                );
            }
        }
    }
}
