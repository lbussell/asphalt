// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: WTFPL

namespace Imtui.Widgets;

public static class TextFieldWidgetExtensions
{
    extension(ImtuiContext context)
    {
        public void TextField(string label, ref string value)
        {
            ArgumentNullException.ThrowIfNull(label);
            ArgumentNullException.ThrowIfNull(value);
            TextFieldResult result = context.Submit(new TextFieldWidget(label, value));
            value = result.Value;
        }
    }
}

internal readonly record struct TextFieldResult(string Value, bool Changed);

internal readonly record struct TextFieldWidget(string Label, string Value)
    : IWidget<TextFieldResult>
{
    public TextFieldResult Execute(ImtuiContext context)
    {
        WidgetID id = context.GetId(Label);
        bool focused = context.RegisterFocusable(id);
        TextFieldState state = context.GetWidgetState<TextFieldState>(id);
        state.EnsureInitialized(Value);

        string value = Value;
        bool changed = focused && ApplyInput(context, state, ref value);

        context.WriteText(
            context.AllocateWidgetPosition(),
            $"{Label}: {value}",
            WidgetStyles.ForFocus(focused)
        );

        return new TextFieldResult(value, changed);
    }

    private static bool ApplyInput(ImtuiContext context, TextFieldState state, ref string value)
    {
        bool changed = false;

        foreach (ImtuiInputEvent inputEvent in context.ThisFrameInput.Events.Span)
        {
            if (inputEvent.Character is { } character)
            {
                string text = character.ToString();
                value = value.Insert(state.CursorIndex, text);
                state.CursorIndex += text.Length;
                changed = true;
                continue;
            }

            switch (inputEvent.Key)
            {
                case ImtuiKey.LeftArrow:
                    state.CursorIndex = Math.Max(0, state.CursorIndex - 1);
                    break;
                case ImtuiKey.RightArrow:
                    state.CursorIndex = Math.Min(value.Length, state.CursorIndex + 1);
                    break;
                case ImtuiKey.Backspace when state.CursorIndex > 0:
                    value = value.Remove(state.CursorIndex - 1, 1);
                    state.CursorIndex--;
                    changed = true;
                    break;
                case ImtuiKey.Delete when state.CursorIndex < value.Length:
                    value = value.Remove(state.CursorIndex, 1);
                    changed = true;
                    break;
            }
        }

        return changed;
    }

    private sealed class TextFieldState
    {
        private bool _initialized;

        public int CursorIndex { get; set; }

        public void EnsureInitialized(string value)
        {
            if (!_initialized)
            {
                CursorIndex = value.Length;
                _initialized = true;
            }

            CursorIndex = Math.Clamp(CursorIndex, 0, value.Length);
        }
    }
}
