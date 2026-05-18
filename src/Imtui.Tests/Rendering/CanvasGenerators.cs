// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Tests.Rendering;

using CsCheck;
using Imtui.Rendering;

// CsCheck generators for terminal cells and canvases. Characters are
// restricted to printable ASCII so generated cases are readable when shrunk.
internal static class CanvasGenerators
{
    public static readonly Gen<TerminalColor> Color = Gen.OneOf(
        Gen.Const(TerminalColor.Default),
        Gen.Byte[0, 15].Select(TerminalColor.Ansi),
        Gen.Byte.Select(TerminalColor.Palette),
        Gen.Select(Gen.Byte, Gen.Byte, Gen.Byte, TerminalColor.Rgb)
    );

    // Inclusive range of all printable ASCII characters (0x20..0x7E). Keeps
    // shrunk counterexamples readable in test output.
    public static readonly Gen<char> Character = Gen.Char[' ', '~'];

    public static readonly Gen<TerminalCell> Cell = Gen.Select(
        Character,
        Color,
        Color,
        (character, foreground, background) => new TerminalCell(character, foreground, background)
    );

    // Generates a Dimensions with width and height each independently drawn
    // from [1, 20]. Upper bound is kept small so that randomized cell-content
    // generation doesn't explode test runtime.
    public static readonly Gen<Dimensions> Dimensions = Gen.Select(
        Gen.Int[1, 20],
        Gen.Int[1, 20],
        (width, height) => new Dimensions(width, height)
    );

    /// <summary>
    /// Generates a random <see cref="TerminalCanvas"/> with the given dimensions,
    /// where every cell is independently filled with a random <see cref="TerminalCell"/>.
    /// </summary>
    public static Gen<TerminalCanvas> Canvas(Dimensions dimensions)
    {
        int width = dimensions.Width;
        int height = dimensions.Height;
        return Cell.Array[width * height]
            .Select(cells =>
            {
                TerminalCanvas canvas = new TerminalCanvas(dimensions);
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        canvas.SetCell(x, y, cells[y * width + x]);
                    }
                }
                return canvas;
            });
    }

    /// <summary>
    /// Generates a random <see cref="TerminalCanvas"/> with randomly-generated
    /// <see cref="Imtui.Dimensions"/>.
    /// </summary>
    public static readonly Gen<TerminalCanvas> AnyCanvas = Dimensions.SelectMany(Canvas);

    /// <summary>
    /// Generates two random canvases that share the <em>same</em> randomly-generated
    /// <see cref="Imtui.Dimensions"/>. Both canvases are guaranteed to have matching
    /// width and height so that <c>CanvasDiffer.Diff</c> (which requires equal
    /// dimensions) can be applied.
    /// </summary>
    public static readonly Gen<(TerminalCanvas Previous, TerminalCanvas Next)> CanvasPair =
        Dimensions.SelectMany(dimensions => Gen.Select(Canvas(dimensions), Canvas(dimensions)));

    /// <summary>
    /// Generates three random canvases that share the <em>same</em> randomly-generated
    /// <see cref="Imtui.Dimensions"/>. All three canvases are guaranteed to have
    /// matching width and height so that consecutive <c>CanvasDiffer.Diff</c> calls
    /// between them can be applied.
    /// </summary>
    public static readonly Gen<(
        TerminalCanvas First,
        TerminalCanvas Second,
        TerminalCanvas Third
    )> CanvasTriple = Dimensions.SelectMany(dimensions =>
        Gen.Select(Canvas(dimensions), Canvas(dimensions), Canvas(dimensions))
    );
}
