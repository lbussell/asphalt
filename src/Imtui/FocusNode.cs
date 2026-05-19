// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace Imtui;

internal sealed class FocusNode(string id, string key, bool isScope)
{
    public string Id { get; } = id;
    public string Key { get; } = key;
    public bool IsScope { get; } = isScope;
    public FocusNavigation Navigation { get; set; } = FocusNavigation.None;
    public FocusNode? Parent { get; set; }
    public List<FocusNode> Children { get; } = [];
    public string? FocusedChildId { get; set; }
}
