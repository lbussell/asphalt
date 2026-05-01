// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text;
using CsCheck;

namespace Imtui.Tests;

public static class Generators
{
    public static readonly Gen<Color> GenColor = Gen.OneOf(
        Gen.Const(Color.Default),
        Gen.Int[0, 15].Select(i => Color.Ansi((AnsiColor)i)),
        Gen.Byte.Select(Color.Palette256),
        Gen.Select(Gen.Byte, Gen.Byte, Gen.Byte, Color.Rgb)
    );

    public static readonly Gen<Cell> GenCell = Gen.Select(
        // Printable ASCII range: space (0x20) through tilde (0x7E)
        Gen.Int[0x20, 0x7E].Select(codepoint => new Rune(codepoint)),
        GenColor,
        GenColor,
        (glyph, foreground, background) => new Cell(glyph, new CellStyle(foreground, background))
    );

    public static readonly Gen<Screen> GenScreen = Gen.Select(Gen.Int[1, 500], Gen.Int[1, 500])
        .SelectMany(
            (width, height) =>
                GenCell.Array[width * height].Select(cells => new Screen(width, height, cells))
        );
}
