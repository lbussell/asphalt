// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

public static class FocusScopeExtensions
{
    extension(ImtuiContext context)
    {
        /// <summary>
        /// Opens a focus scope with explicit navigation keys.
        /// </summary>
        /// <param name="id">Stable id for this scope within its parent scope.</param>
        /// <param name="navigation">Keys that move to the previous and next target in this scope.</param>
        /// <returns>A disposable scope that closes this focus scope when disposed.</returns>
        public FocusScope BeginFocusScope(string id, FocusNavigation navigation)
        {
            context.OpenFocusScope(id, navigation);
            return new FocusScope(context.CloseFocusScope);
        }
    }
}
