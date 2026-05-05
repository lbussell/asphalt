// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: WTFPL

namespace Imtui;

/// <summary>
/// Widget focus and activation state for the current frame.
/// </summary>
/// <param name="Focused">The focused widget ID, or default when no widget is focused.</param>
/// <param name="Active">The activated widget ID, or default when no widget is active.</param>
public readonly record struct FocusState(WidgetID Focused, WidgetID Active);
