# 002 — Deepen `Screen` to own cell addressing

## Files

`Terminal.cs`, `Demo/Program.cs`

## Problem

`Screen` is a data bag — a record struct with a public `Cell[]`. Every caller must compute `position.Y * screen.Size.Width + x` by hand to write a cell (see Demo line 32). The interface (raw array + manual index math) is as complex as what it hides. Screen is shallow.

## Solution

Add methods to `Screen` that encapsulate cell addressing — e.g., indexer by `CellPosition`, a `WriteText` method, or similar. Callers stop doing index arithmetic.

## Benefits

- **Locality** — cell-addressing bugs can only exist in one place.
- **Leverage** — every caller (demo, future widgets, tests) gets safe cell writing for free.
- Testable through Screen's own interface instead of relying on callers to get the math right.
