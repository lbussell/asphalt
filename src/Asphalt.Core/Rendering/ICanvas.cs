// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt.Rendering;

public interface ICanvas
{
    void Draw(
        Position position,
        char character,
        TerminalColor foregroundColor = default,
        TerminalColor backgroundColor = default,
        TextStyle style = TextStyle.None
    );
}
