// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text;

namespace Imtui;

public static class TextWrapper
{
    public static string WrapText(string text, int maxWidth, TextWrappingMode mode = default)
    {
        if (maxWidth <= 0 || text.Length <= maxWidth)
            return text;

        return mode switch
        {
            // Truncate: cut each line to maxWidth, discarding the rest.
            TextWrappingMode.Truncate => TruncateText(text, maxWidth),
            // Force: break lines at exactly maxWidth characters, regardless of word boundaries.
            TextWrappingMode.Force => ForceWrapText(text, maxWidth),
            _ => text,
        };
    }

    private static string TruncateText(string text, int maxWidth)
    {
        StringBuilder result = new();

        foreach (ReadOnlySpan<char> line in text.AsSpan().EnumerateLines())
        {
            if (result.Length > 0)
                result.Append('\n');

            result.Append(line.Length <= maxWidth ? line : line[..maxWidth]);
        }

        return result.ToString();
    }

    private static string ForceWrapText(string text, int maxWidth)
    {
        StringBuilder result = new();

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
            }
        }

        return result.ToString();
    }
}
