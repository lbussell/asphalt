_default:
    @just --list

restore:
    dotnet restore

_build:
    dotnet build --no-restore

_format:
    dotnet format --no-restore

_test:
    dotnet test --no-restore

validate-all: _build _format _test
