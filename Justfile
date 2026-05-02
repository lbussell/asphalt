
_default:
    @just --list

restore:
    dotnet restore

build:
    dotnet build --no-restore

format:
    dotnet format --no-restore

test:
    dotnet test --no-restore

validate-all: build format test

run:
    dotnet run --project src/demo
