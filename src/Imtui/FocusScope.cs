// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

public sealed class FocusScope : IDisposable
{
    private readonly Action _close;
    private bool _disposed;

    public FocusScope(Action close)
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
