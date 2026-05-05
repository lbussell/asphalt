// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

internal readonly record struct CheckboxResult(bool Value, bool Changed);

internal readonly record struct TextFieldResult(string Value, bool Changed);
