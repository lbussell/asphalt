
_default:
    @just --list

_build:
    dotnet build

_format:
    dotnet format

validate-all: _build _format
    dotnet test

run:
    dotnet run --project src/demo
