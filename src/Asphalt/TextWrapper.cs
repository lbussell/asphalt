// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Diagnostics;

namespace Asphalt;

public static class TextWrapper
{
    public static string[] WrapText(string text, int maxWidth, TextWrappingMode mode = default)
    {
        Debug.Assert(maxWidth > 0, $"{nameof(maxWidth)} must be greater than zero");

        return mode switch
        {
            TextWrappingMode.Truncate => TruncateText(text, maxWidth),
            TextWrappingMode.Force => ForceWrapText(text, maxWidth),
            TextWrappingMode.Wrap => WordBoundaryWrapText(text, maxWidth),
            _ => [text],
        };
    }

    private static string[] TruncateText(string text, int maxWidth)
    {
        List<string> lines = [];

        foreach (ReadOnlySpan<char> line in text.AsSpan().EnumerateLines())
            lines.Add((line.Length <= maxWidth ? line : line[..maxWidth]).ToString());

        return [.. lines];
    }

    private static string[] ForceWrapText(string text, int maxWidth)
    {
        List<string> lines = [];

        foreach (ReadOnlySpan<char> line in text.AsSpan().EnumerateLines())
        {
            ReadOnlySpan<char> remaining = line;

            while (remaining.Length > 0)
            {
                int chunkLength = Math.Min(remaining.Length, maxWidth);
                lines.Add(remaining[..chunkLength].ToString());
                remaining = remaining[chunkLength..];
            }
        }

        return [.. lines];
    }

    // Wrap text at word boundaries by scanning backwards from maxWidth to find
    // a break character (space or hyphen). Falls back to a hard break at maxWidth
    // if no boundary is found.
    private static string[] WordBoundaryWrapText(string text, int maxWidth)
    {
        List<string> lines = [];

        foreach (ReadOnlySpan<char> line in text.AsSpan().EnumerateLines())
        {
            ReadOnlySpan<char> remaining = line;

            while (remaining.Length > 0)
            {
                if (remaining.Length <= maxWidth)
                {
                    lines.Add(remaining.ToString());
                    break;
                }

                // Scan backwards from the boundary to find a break character.
                int breakIndex = -1;
                for (int i = maxWidth - 1; i >= 0; i--)
                {
                    if (remaining[i] is ' ' or '-')
                    {
                        breakIndex = i;
                        break;
                    }
                }

                if (breakIndex >= 0)
                {
                    // Include the break character on this line (e.g. keep the hyphen),
                    // then skip any trailing spaces at the break point.
                    lines.Add(remaining[..(breakIndex + 1)].ToString());
                    remaining = remaining[(breakIndex + 1)..];

                    // Skip leading spaces on the next line.
                    remaining = remaining.TrimStart(' ');
                }
                else
                {
                    // No break character found; hard break at maxWidth.
                    lines.Add(remaining[..maxWidth].ToString());
                    remaining = remaining[maxWidth..];
                }
            }
        }

        return [.. lines];
    }
}
