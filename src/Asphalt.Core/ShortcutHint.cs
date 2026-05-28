// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Asphalt;

/// <summary>
/// A single shortcut entry registered for the current frame via
/// <see cref="AsphaltContext.AddShortcutHint(string, string)"/>. Typically rendered
/// in a shortcuts bar so users can see which keys are currently meaningful.
/// </summary>
public readonly record struct ShortcutHint(string Label, string Value);
