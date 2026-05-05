// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: WTFPL

using Imtui.Rendering;

namespace Imtui;

/// <summary>
/// Direction in which child widgets in a layout frame are arranged.
/// </summary>
public enum LayoutDirection
{
    /// <summary>
    /// Widgets are stacked vertically; each submitted widget advances the
    /// frame's cursor down by the height it drew.
    /// </summary>
    Vertical,

    /// <summary>
    /// Widgets are placed horizontally; each submitted widget advances the
    /// frame's cursor right by the width it drew.
    /// </summary>
    Horizontal,

    /// <summary>
    /// Widgets do not advance the frame's cursor. Useful for absolute
    /// placement scenarios.
    /// </summary>
    None,
}

/// <summary>
/// Bounding box of cells written while a layout frame was on top of the
/// stack, returned when the frame is popped. Width and Height are zero when
/// no cells were written.
/// </summary>
public readonly record struct LayoutMeasurement(int OriginX, int OriginY, int Width, int Height);

/// <summary>
/// Tracks the cursor, bounding box, default style, and direction of a single
/// region of the screen while widgets in that region are being submitted.
/// Layout frames are pushed and popped by container widgets to nest layouts.
/// </summary>
internal sealed class LayoutFrame
{
    public LayoutFrame(int originX, int originY, LayoutDirection direction, CellStyle defaultStyle)
    {
        OriginX = originX;
        OriginY = originY;
        CursorX = originX;
        CursorY = originY;
        MaxX = originX;
        MaxY = originY;
        Direction = direction;
        DefaultStyle = defaultStyle;
    }

    public int OriginX { get; }
    public int OriginY { get; }
    public LayoutDirection Direction { get; }
    public CellStyle DefaultStyle { get; }

    public int CursorX { get; set; }
    public int CursorY { get; set; }

    /// <summary>
    /// Largest X coordinate, plus one, ever passed to a write while this frame
    /// was on top of the stack. Equals OriginX when no cells were written.
    /// </summary>
    public int MaxX { get; set; }

    /// <summary>
    /// Largest Y coordinate, plus one, ever passed to a write while this frame
    /// was on top of the stack. Equals OriginY when no cells were written.
    /// </summary>
    public int MaxY { get; set; }
}
