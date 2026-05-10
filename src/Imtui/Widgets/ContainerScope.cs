// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

public record struct ContainerScope(ImtuiContext Context) : IDisposable
{
    private bool _disposed;

    public void Dispose()
    {
        if (!_disposed)
        {
            Context.CloseElement();
            _disposed = true;
        }
    }
}
