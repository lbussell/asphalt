// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace ImtuiLib;

/// <summary>
/// Describes the inputs required to start a new immediate-mode frame.
/// </summary>
public readonly record struct FrameRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FrameRequest"/> struct.
    /// </summary>
    /// <param name="size">The viewport size.</param>
    public FrameRequest(ViewportSize size)
    {
        Size = size;
    }

    /// <summary>
    /// Gets the viewport size.
    /// </summary>
    public ViewportSize Size { get; }
}
