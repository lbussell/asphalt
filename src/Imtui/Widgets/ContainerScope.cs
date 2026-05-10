// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui.Widgets;

public sealed class ContainerScope : IDisposable
{
    public ContainerScope(ImtuiContext context, int closeCount = 1)
    {
        if (closeCount <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(closeCount),
                "Close count must be positive."
            );

        Context = context;
        CloseCount = closeCount;
    }

    private bool _disposed;

    public ImtuiContext Context { get; }
    public int CloseCount { get; }

    public void Dispose()
    {
        if (_disposed)
            return;

        for (int i = 0; i < CloseCount; i++)
            Context.CloseElement();

        _disposed = true;
    }
}
