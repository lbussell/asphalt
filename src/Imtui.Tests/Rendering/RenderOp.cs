// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Tests.Rendering;

using Imtui.Rendering;

// Test-only structured representation of a render operation. The recording
// sink translates IRenderOpsSink calls into instances of these types so that
// tests can assert structurally on what the differ emits.
internal abstract record RenderOp;

internal sealed record MoveTo(int Column, int Row) : RenderOp;

internal sealed record SetForeground(TerminalColor Color) : RenderOp;

internal sealed record SetBackground(TerminalColor Color) : RenderOp;

internal sealed record SetStyle(TextStyle Added, TextStyle Removed) : RenderOp;

internal sealed record ResetSgr : RenderOp;

internal sealed record WriteText(string Text) : RenderOp;
