# Gauniv.Game (Godot Client)

This is the Godot 4 + C# game client for `Gauniv.GameServer`.

## Current Stage

Formal multi-scene flow is in place:
- `LoginScreen` -> connect + auth
- `RoomListScreen` -> room list/create/join entry
- `RoleSelectScreen` -> choose Player/Observer
- `LobbyScreen` -> players/observers + ready + leave
- `GameScreen` -> board interaction
- `ResultScreen` -> round rankings + back to rooms

Main orchestration and protocol dispatch are in `scripts/Main.cs`.
Transport is in `scripts/NetworkClient.cs` using JSON line protocol.

## Run

1. Start `Gauniv.WebServer` on `http://localhost:5231`.
2. Start `Gauniv.GameServer` on `tcp://0.0.0.0:7000`.
3. Open this folder in Godot 4.x (.NET/Mono build).
4. Run `Main.tscn`.

## Note

This environment cannot restore `Godot.NET.Sdk` from NuGet due network restrictions,
so compile verification here is limited to static file checks.
