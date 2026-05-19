// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

public static class FocusScopeExtensions
{
    extension(ImtuiContext context)
    {
        /// <summary>
        /// Opens a focus scope.
        /// </summary>
        /// <param name="id">Stable id for this scope within its parent scope.</param>
        /// <returns>A disposable scope that closes this focus scope when disposed.</returns>
        public FocusScopeHandle BeginFocusScope(string id)
        {
            context.PushFocusScope(id);
            return new FocusScopeHandle(context.CloseFocusScope);
        }
    }
}
