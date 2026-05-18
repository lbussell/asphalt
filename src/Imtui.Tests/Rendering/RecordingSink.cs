// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Tests.Rendering;

using Imtui.Rendering;

// Captures every call made to it as a structured RenderOp for test assertions.
// Performs no optimization, deduplication, or reordering.
internal sealed class RecordingSink : IRenderOpsSink
{
    private readonly List<RenderOp> _operations = [];

    public IReadOnlyList<RenderOp> Operations => _operations;

    public void MoveTo(int column, int row) => _operations.Add(new MoveTo(column, row));

    public void SetForeground(TerminalColor color) => _operations.Add(new SetForeground(color));

    public void SetBackground(TerminalColor color) => _operations.Add(new SetBackground(color));

    public void ResetSgr() => _operations.Add(new ResetSgr());

    public void WriteText(ReadOnlySpan<char> text) =>
        _operations.Add(new WriteText(text.ToString()));
}
