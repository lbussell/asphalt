# Imtui

Imtui is an immediate-mode terminal user interface library for .NET.

## Goals

Imtui's goal is to make TUI programming as easy as possible.
Keep API similar in spirit to Dear ImGui, but use idiomatic .NET patterns where it makes sense.
Provide limited configuration, make sane default choices.
Imtui should be extensible, and built-in widgets should be built using public APIs.

## Developer guide

Use `just` for development to keep things consistent.
Run `just --list` to see list of commands.
Always use `just validate-all` to validate changes.

## Code style

- Absolutely minimal/simple/functional code. Clarity and readability are paramount. Do not shorten/abbreviate variable names.
- Follow John Ousterhout's guidelines on code comments from APOSD.

## Widget design

- Widgets use out ref values for mutable/manipulatable values like checkboxes, text input, slider values, etc.
- Activatable widgets like buttons and collapsible panels return true/false based on whether they are activated.

### BACKLOG.md

Keep BACKLOG.md up-to-date with planned future tasks.
