// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using System.Text;

namespace Imtui;

public static class TextWrapper
{
    public static string WrapText(string text, int maxWidth, TextWrappingMode mode, out int height)
    {
        Debug.Assert(maxWidth > 0, $"{nameof(maxWidth)} must be greater than zero");

        return mode switch
        {
            TextWrappingMode.Truncate => TruncateText(text, maxWidth, out height),
            TextWrappingMode.Force => ForceWrapText(text, maxWidth, out height),
            TextWrappingMode.Wrap => WordBoundaryWrapText(text, maxWidth, out height),
            _ => PassThrough(text, out height),
        };
    }

    public static string WrapText(string text, int maxWidth, TextWrappingMode mode = default) =>
        WrapText(text, maxWidth, mode, out _);

    private static string PassThrough(string text, out int height)
    {
        height = 1;
        return text;
    }

    private static string TruncateText(string text, int maxWidth, out int height)
    {
        StringBuilder result = new();
        height = 0;

        foreach (ReadOnlySpan<char> line in text.AsSpan().EnumerateLines())
        {
            if (result.Length > 0)
                result.Append('\n');

            result.Append(line.Length <= maxWidth ? line : line[..maxWidth]);
            height++;
        }

        return result.ToString();
    }

    private static string ForceWrapText(string text, int maxWidth, out int height)
    {
        StringBuilder result = new();
        height = 0;

        foreach (ReadOnlySpan<char> line in text.AsSpan().EnumerateLines())
        {
            ReadOnlySpan<char> remaining = line;

            while (remaining.Length > 0)
            {
                if (result.Length > 0)
                    result.Append('\n');

                int chunkLength = Math.Min(remaining.Length, maxWidth);
                result.Append(remaining[..chunkLength]);
                remaining = remaining[chunkLength..];
                height += 1;
            }
        }

        return result.ToString();
    }

    // Wrap text at word boundaries by scanning backwards from maxWidth to find
    // a break character (space or hyphen). Falls back to a hard break at maxWidth
    // if no boundary is found.
    private static string WordBoundaryWrapText(string text, int maxWidth, out int height)
    {
        StringBuilder result = new();
        height = 0;

        foreach (ReadOnlySpan<char> line in text.AsSpan().EnumerateLines())
        {
            ReadOnlySpan<char> remaining = line;

            while (remaining.Length > 0)
            {
                if (result.Length > 0)
                    result.Append('\n');

                height += 1;

                if (remaining.Length <= maxWidth)
                {
                    result.Append(remaining);
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
                    result.Append(remaining[..(breakIndex + 1)]);
                    remaining = remaining[(breakIndex + 1)..];

                    // Skip leading spaces on the next line.
                    remaining = remaining.TrimStart(' ');
                }
                else
                {
                    // No break character found; hard break at maxWidth.
                    result.Append(remaining[..maxWidth]);
                    remaining = remaining[maxWidth..];
                }
            }
        }

        return result.ToString();
    }
}
