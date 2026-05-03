# 001 — Merge `DifferentialRendering` + `AnsiFormatter` into one deep rendering module

## Files

`DifferentialRendering.cs`, `AnsiFormatter.cs`, `TermOp.cs`

## Problem

Callers always call these together: `AnsiFormatter.Format(DifferentialRendering.Render(prev, next))`. `TermOp[]` is a pass-through intermediate representation — no caller ever inspects, transforms, or stores it independently. `DifferentialRendering.Render()` doesn't even use its `previous` parameter yet. These are two shallow modules linked by a data type that exists purely to shuttle bytes between them.

**Deletion test on `TermOp`:** imagine deleting it. A single `Render(Screen, Screen) → string` wouldn't push complexity to callers — it would *remove* the need for callers to know about `TermOp` at all. Complexity concentrates, not disperses. `TermOp` fails the deletion test.

## Solution

One module with interface `Render(Screen previous, Screen next) → string`. `TermOp` becomes an internal implementation detail (or vanishes). When actual differential logic arrives, it lives inside this module — the seam between "what changed" and "how to encode it" becomes an internal seam, not a public one.

## Benefits

- **Leverage** — callers get rendering in one call.
- **Locality** — the diff algorithm, ANSI encoding, and any future IR format all concentrate in one place.
- Tests assert on ANSI output given screen pairs, which is the actual contract callers care about.
