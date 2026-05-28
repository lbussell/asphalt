// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt;

/// <summary>
/// The lexical scope of a focusable widget. Returned by widget builders
/// (<c>SelectableList</c>, <c>Button</c>, <c>InputText</c>, ...) and intended
/// for use with <c>using</c>. While the scope is open, calls to
/// <see cref="AsphaltContext.KeyDown(ConsoleKey)"/> are gated on the
/// widget's focus state for this frame.
/// </summary>
public sealed class WidgetScope : IDisposable
{
    private readonly Action _close;
    private bool _disposed;

    public WidgetScope(Action close)
    {
        _close = close ?? throw new ArgumentNullException(nameof(close));
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _close();
        _disposed = true;
    }
}
