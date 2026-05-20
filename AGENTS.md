# Imtui - Coding Agent Instructions
Imtui is an immediate-mode terminal UI library for .NET.

## Project layout

| Path | Description |
| --- | --- |
| `src/Imtui/` | Core library |
| `src/Imtui/Widgets/` | Built-in widgets |
| `src/Imtui/Rendering/` | Terminal canvas, differential renderer, ANSI sink, alt-screen support. `TerminalCanvasPresenter` is the production pipeline (calls `DifferentialRenderer.Diff` → `AnsiSink`). `AnsiSink` is the only place that emits SGR escapes. |
| `src/Imtui.Tests/` | MSTest project using the MSTest runner (`EnableMSTestRunner=true`). Uses CsCheck for property tests. `ImtuiTestHarness.RunFrame` runs one frame against an isolated `ImtuiContext` and returns the root `LayoutNode` for structural assertions. |
| `samples/` | File-based C# programs (`#!/usr/bin/env dotnet` + `#:project` directive). Each sample is independently runnable. |
| `scripts/` | File-based C# utility scripts (release, readme generation, etc.). |
| `Imtui.slnx` | Solution. |

## Build / test / run

- Build and test from the repo root: `dotnet build` and `dotnet test`.
- Run a sample (always pass `--no-cache` after library changes to avoid stale cached output):
  `dotnet run --no-cache --file samples/01_LayoutDemo.cs`
- Regenerate `samples/README.md` from `samples/README.template.md`:
  `dotnet scripts/GenerateReadmes.cs`

CSharpier runs as part of `dotnet build` via `CSharpier.MSBuild` and **rewrites `.cs` files in place**.
Don't fight the formatter.

## Architecture: the frame loop

Everything flows through `ImtuiContext`. One frame is:

1. `BeginLayout(dimensions, input)` — clears per-frame state, increments `FrameCount`, pushes the implicit root focus scope.
2. User code calls widget extension methods (`imtui.Panel(...)`, `imtui.Button(...)`, `imtui.VStack(...)`, etc.) which call `OpenElement` / `CloseElement` to build a `LayoutNode` tree.
3. `EndLayout()` returns the root `LayoutNode` with computed sizes/positions.
4. `LayoutRenderer` rasterizes the tree onto a `TerminalCanvas`.
5. `TerminalCanvasPresenter.Present` diffs against the previous frame and writes ANSI to the terminal via `AnsiSink`.

`ImtuiApplication.Run` is the standard event loop driving steps 1–5 plus keyboard input via `KeyboardDispatcher` / `KeyboardReader`. Samples use it directly; tests bypass it via `ImtuiTestHarness`.

State that must survive across frames (focus position per scope, `UseState` values, `WakeOn` tasks, `RequestRedraw`) lives on `ImtuiContext` keyed by widget/scope id. Ids must be globally unique per frame — `ClaimFocusId` rejects collisions and the empty string is reserved for the root scope.

## Conventions

- **Public API surface is intentionally small.** Anything that can be an extension method on `ImtuiContext` MUST be an extension method, not a member of `ImtuiContext`. Use the C# 14 `extension(ImtuiContext context) { ... }` block syntax (see `ContainerExtensions.cs`). Built-in widgets are themselves built on public APIs — don't reach into internals to add new widgets.
- **Code style:** absolutely minimal/simple/functional; clarity over cleverness. **Do not abbreviate variable names.** Comments are good where they clarify; pointless comments are bad.
- **Theme:** widget extension methods read colors from `context.Theme` when the caller doesn't supply one. `Theme` is mutable mid-frame.
- **Samples are executable scripts.** Always `chmod +x` new samples (the executable bit is part of the convention).

## Adding a widget

Each widget has a `*Widget.cs` (implements `IWidget`) and a `*Extensions.cs` exposing the ergonomic API as extension methods on `ImtuiContext`.

- [ ] `Widget.cs` (class implementing `IWidget`)
- [ ] `FooExtensions.cs` under `src/Imtui/Widgets/` (see the layout table for the split). The extension is responsible for `OpenElement`/`CloseElement`, claiming a focus id if focusable, and reading defaults from `context.Theme`.
- [ ] Add a sample `samples/NN_Foo.cs` and re-run the README generator.
- [ ] Add tests in `src/Imtui.Tests/Widgets/` using the `LayoutTree` helpers.
