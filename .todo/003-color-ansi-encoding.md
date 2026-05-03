# 003 — Move Color's ANSI encoding into `Color` itself

## Files

`Color.cs`, `AnsiFormatter.cs`

## Problem

`AnsiFormatter.AppendColor` switches on `color.Kind` and reaches into Color's internals (`AnsiColor`, `PaletteIndex`, `R`/`G`/`B`). If a new `ColorKind` is added, both `Color` and `AnsiFormatter` must change in lockstep. The knowledge of how to encode a color is split across two modules.

## Solution

Give `Color` a method like `AppendAnsi(StringBuilder, bool isBackground)` that encapsulates its ANSI encoding. `AnsiFormatter` delegates to it instead of switching on `Kind`.

## Benefits

- **Locality** — adding a color kind only requires changing `Color`. The exhaustive switch on `ColorKind` concentrates in one place.
- `AnsiFormatter` gets simpler.

## Trade-off

Color gains knowledge of ANSI encoding, which is a presentation concern. But Color already models a *terminal* color — ANSI encoding is part of its domain, not a foreign concept.
