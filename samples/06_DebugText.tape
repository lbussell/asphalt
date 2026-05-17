Source Setup.tape

Hide
Type@0ms "dotnet 06_DebugText.cs" Enter
# Search the whole screen for the given regex. Must be something output by the
# app that doesn't appear in the line above used to build the application.
Wait+Screen /Example/
Show

Screenshot "06_DebugText.png"
Sleep 100ms
