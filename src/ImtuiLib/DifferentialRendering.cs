// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace ImtuiLib;

public static class DifferentialRendering
{
    public static TermOp[] Render(Screen previous, Screen next)
    {
        return [];
    }

    public static Screen Apply(Screen screen, IEnumerable<TermOp> operations)
    {
        return screen;
    }
}

public readonly struct TermOp
{
    public TermOp(Write value) => Value = value;

    public TermOp(MoveCursor value) => Value = value;

    public object Value { get; }
}

public readonly record struct MoveCursor(CellPosition Position);

public readonly record struct Write(Cell Cell);
