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

public enum TermOpKind : byte
{
    MoveCursor,
    Write,
}

public readonly struct TermOp : IEquatable<TermOp>
{
    public TermOpKind Kind { get; }
    private readonly CellPosition _position;
    private readonly Cell _cell;

    private TermOp(TermOpKind kind, CellPosition position, Cell cell)
    {
        Kind = kind;
        _position = position;
        _cell = cell;
    }

    public static TermOp MoveCursor(CellPosition position) =>
        new(TermOpKind.MoveCursor, position, default);

    public static TermOp Write(Cell cell) => new(TermOpKind.Write, default, cell);

    public CellPosition Position =>
        Kind == TermOpKind.MoveCursor
            ? _position
            : throw new InvalidOperationException("Not a MoveCursor operation");

    public Cell Cell =>
        Kind == TermOpKind.Write
            ? _cell
            : throw new InvalidOperationException("Not a Write operation");

    public bool Equals(TermOp other) =>
        Kind == other.Kind
        && Kind switch
        {
            TermOpKind.MoveCursor => _position == other._position,
            TermOpKind.Write => _cell == other._cell,
            _ => false,
        };

    public override bool Equals(object? obj) => obj is TermOp other && Equals(other);

    public override int GetHashCode() =>
        Kind switch
        {
            TermOpKind.MoveCursor => HashCode.Combine(Kind, _position),
            TermOpKind.Write => HashCode.Combine(Kind, _cell),
            _ => Kind.GetHashCode(),
        };

    public static bool operator ==(TermOp left, TermOp right) => left.Equals(right);

    public static bool operator !=(TermOp left, TermOp right) => !left.Equals(right);
}
