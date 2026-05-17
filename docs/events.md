# Immediate-Mode Terminal UI Library — Implementation Brief

## Goal
Native AOT-compatible, immediate-mode TUI library for .NET. Inspired by Dear ImGui / Clay. Already implemented: two-pass layout, widget stack with stable IDs, redraw-on-input. This brief covers the **event loop, animations, and mouse input**.

## Core Model: Everything Is an Input Event

Single unified event queue. The loop has one job: wait for the next event, run a frame, repeat. Time-based redraws (animations) are just `Tick` events in the same queue.

```
while (running)
{
    DrainAndCoalesce(frameInput);    // pull events, collapse mouse moves
    BuildFrame(frameInput);          // user callback; widgets may enqueue Ticks
    Render();
    WaitForNextEvent();              // blocks indefinitely if queue empty
}
```

Idle UIs cost nothing (infinite block). Active animations re-arm themselves each frame.

## Event Types

```csharp
abstract record InputEvent;
record KeyEvent(...) : InputEvent;
record MouseButtonEvent(int X, int Y, MouseButton Btn, bool Pressed) : InputEvent;
record MouseWheelEvent(int X, int Y, int Delta) : InputEvent;
record MouseMoveEvent(int X, int Y) : InputEvent;   // ambient; see below
record TickEvent(DateTime At) : InputEvent;
record ResizeEvent(int Cols, int Rows) : InputEvent;
```

## Input Source (Cross-Platform, AOT-Safe)

Background thread reads stdin into a thread-safe queue (`BlockingCollection<InputEvent>` or a custom `Channel`). The main loop blocks on `TryTake(timeout)`.

**Timeout calculation:** scan the queue for the earliest `TickEvent.At`; timeout = `at - now`. No pending ticks → infinite.

P/Invoke `poll()`/`WaitForSingleObject` is an optimization for later; the background-thread approach works cross-platform now.

## Mouse Input

**Enable on startup, disable on shutdown** (wrap in `IDisposable` with `try/finally`):
- Enable: `\x1b[?1000;1006h` (SGR mouse mode, handles coords > 223)
- Disable: `\x1b[?1000;1006l`

**Parser:** state machine over incoming bytes. SGR sequences: `\x1b[<button;x;y(M|m)` where `M` = press, `m` = release. Must handle partial sequences split across reads — keep a parser buffer.

**Coalescing at frame start:**
- Multiple `MouseMoveEvent`s → keep only the latest position
- Press/release/wheel events kept individually
- Build a `FrameInput` snapshot: `MousePos`, `MouseDelta`, `ButtonsPressed`, `ButtonsReleased`, `WheelDelta`

**Mouse moves as ambient state, not redraw triggers:** moves update `ctx.MousePos` and are pulled if a frame happens to run, but do NOT by themselves wake the loop. Hover-sensitive widgets must request a tick if they want to react to moves. (Or: track "was anything hover-sensitive last frame" and only let moves wake the loop in that case. Pick one; start with the simpler "moves don't wake" rule.)

**Mouse capture:** on mousedown, store `activeWidgetId` in the state dictionary. Subsequent moves/release route to that widget regardless of position. Clear on mouseup. Required for sliders/scrollbars to feel right.

## Animations via Tick Events

Widgets that want to animate call something like:

```csharp
ctx.RequestTickAt(ctx.FrameTime + TimeSpan.FromMilliseconds(100));
```

Implementation:
- `RequestTickAt` enqueues a `TickEvent`, or merges with an existing pending tick within a small epsilon (e.g. one frame budget) to dedupe.
- On first appearance of an animating widget, it enqueues its initial tick. From then on, each frame re-arms.
- Orphaned ticks (widget no longer called) fire one wasted frame and die. No cleanup needed.

Spinner example:

```csharp
public static void Spinner(string id)
{
    var ms = ctx.FrameTime.TotalMilliseconds;
    var frame = (int)(ms / 100) % frames.Length;
    DrawChar(frames[frame]);
    ctx.RequestTickAt(ctx.FrameTime + TimeSpan.FromMilliseconds(100));
}
```

## Frame Lifecycle

1. `WaitForNextEvent()` returns (or queue is non-empty).
2. Drain queue; coalesce mouse moves; build `FrameInput`.
3. Set `ctx.FrameTime = DateTime.UtcNow`.
4. Run user callback. Widgets read `FrameInput`, may call `RequestTickAt`.
5. Layout + render (existing).
6. Loop.

## Teardown Safety

Raw mode setup and mouse-mode enable/disable must be in `try/finally` (or `IDisposable`). A crash must not leave the user's terminal in raw mode with mouse reporting on. Also restore cursor visibility.

## Resize

`SIGWINCH` on Unix (signal handler enqueues `ResizeEvent`), poll `Console.WindowWidth/Height` on Windows or use console input records. Invalidates front buffer → forces full redraw next frame.

## Build Order Suggestion

1. Event queue + background stdin reader + unified loop with timeout.
2. `TickEvent` plumbing + `RequestTickAt` + spinner test.
3. SGR mouse parser + button/wheel events + capture mechanism.
4. Mouse move coalescing + ambient position state.
5. Resize handling.
6. Teardown hardening.
