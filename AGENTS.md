# Asphalt - Coding Agent Instructions
Asphalt is an immediate-mode terminal UI library for .NET.

## Project layout

| Path | Description |
| --- | --- |
| `src/Asphalt.Core/` | Core library - layout, application loop, etc. |
| `src/Asphalt.Core/Rendering/` | Everything that outputs to the terminal |
| `src/Asphalt.Widgets/` | Built-in widgets (separately packaged) |
| `src/Asphalt/` | Metapackage csproj. Empty - just references Core + Widgets so consumers can `dotnet add package Asphalt` to get both. |
| `src/Asphalt.Core.Tests/` | MSTest project for core. Uses CsCheck for property tests. |
| `src/Asphalt.Widgets.Tests/` | MSTest project for widgets (and any tests that combine core+widgets). |
| `docs/` | Generated docs and source templates under `docs/.templates/`. |
| `samples/` | Self-contained samples. |
| `scripts/` | Self-contained utility scripts for release, readme generation, etc. |

## Build / test / run

- Build and test from the repo root: `dotnet build` and `dotnet test`.
- Run a sample (always pass `--no-cache` after library changes to avoid stale cached output):
  `dotnet run --no-cache --file samples/01_LayoutDemo.cs`
- Regenerate generated docs:
  `dotnet scripts/GenerateReadmes.cs`
  To regenerate only the widget gallery:
  `dotnet scripts/GenerateReadmes.cs docs/widgets.md`

CSharpier runs formatting as part of `dotnet build` and rewrites `.cs` files in place.

## Design

Everything flows through `AsphaltContext`. One frame is:

1. `BeginLayout(dimensions, input)`
2. User code calls widget extension methods (`asphalt.Panel(...)`, `asphalt.Button(...)`, `asphalt.VStack(...)`, etc.) which call `OpenElement` / `CloseElement` to build a `LayoutNode` tree.
3. `EndLayout()` returns the root `LayoutNode` with computed sizes/positions.
4. `LayoutRenderer` rasterizes the tree onto a `TerminalCanvas`.
5. `TerminalCanvasPresenter.Present` diffs against the previous frame and writes ANSI to the terminal via `AnsiSink`.

`AsphaltApplication.Run` is the standard event loop driving steps 1–5.
Keyboard input is read in a background thread by `KeyboardReader` and sent to widgets via `KeyboardDispatcher`.
State that must survive across frames lives on `AsphaltContext` keyed by widget/scope id.
Widget and focus scope IDs must be globally unique per frame - collisions are rejected.

## Conventions

- Public API surface is intentionally small. Prefer adding new features via extension methods instead of adding to classes.
- Theme: widgets read colors from `context.Theme`.
- Code style: absolutely minimal/simple/functional; clarity is paramount.
  - Keep variable names descriptive.
  - Cognitive load is what matters.

## After public API changes

When changing public APIs or widgets:
- Search all Markdown files for old names, signatures, and explanations. The docs set is small enough that API-changing work should inspect every `*.md` file.
- Check `AGENTS.md` for stale developer guidance.
- Check `samples/` files that use the changed API.
- Check `docs/.templates/widgets.template.md` and `docs/.templates/widgets/*.cs` for widget-gallery updates.
- Regenerate generated docs with `dotnet scripts/GenerateReadmes.cs` (takes a few minutes), or target `docs/widgets.md` while iterating on the gallery.
- Update XML docs on changed public APIs.
- Run the relevant build/tests after generated docs are refreshed.

## Adding a widget

Built-in widgets live in `src/Asphalt.Widgets/`. Each widget is a single file.
Widgets are exposed as extension methods on `AsphaltContext`.
Widgets must be built using public APIs. This ensures that Asphalt's API remains flexible.
Use `WidgetTemplate.cs` as a starting point.

### New widget checklist

- [ ] New widget file under `src/Asphalt.Widgets/*Widget.cs`.
- [ ] Only public API is an extension method on `AsphaltContext`
  - [ ] Use C# 14 extension member syntax (`extension(AsphaltContext context) { ... }`)
  - [ ] Public extension methods are documented.
- [ ] `IWidget` implementation is private (or internal if needed in tests).
- [ ] Widget does not use internal APIs
