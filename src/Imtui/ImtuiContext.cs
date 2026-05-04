// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using Imtui.Rendering;

namespace Imtui;

public class ImtuiContext(TextWriter output)
{
    private readonly TextWriter _output = output;

    public void NewFrame() { }

    public void Render() { }
}

public static class BoxWidget
{
    extension(ImtuiContext context)
    {
        public void Box(int topLeftX, int topLeftY, int bottomLeftX, int bottomLeftY)
        {
            CellPosition topLeft = new(topLeftX, topLeftY);
            CellPosition bottomRight = new(bottomLeftX, bottomLeftY);
            // TODO: Implement
        }
    }
}
