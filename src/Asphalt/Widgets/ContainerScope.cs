// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt.Widgets;

public sealed class ContainerScope : IDisposable
{
    private readonly Action _close;
    private bool _disposed;

    public ContainerScope(Action close)
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
