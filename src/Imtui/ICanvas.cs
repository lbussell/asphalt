// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

public interface ICanvas
{
    void Fill(Rect bounds, TerminalColor backgroundColor);

    void Draw(
        Position position,
        char character,
        TerminalColor foregroundColor = default,
        TerminalColor backgroundColor = default
    );
}
