# Quick Run Commands

## 1) Start local servers

`.\run-local.ps1` will start:
- `Gauniv.WebServer` on `http://localhost:5231`
- `Gauniv.GameServer` on TCP port `7000`

```powershell
cd C:\NET\TP2.NET
Set-ExecutionPolicy -Scope Process Bypass
.\run-local.ps1
```

## 2) Website pages on localhost:5231

All web pages use the same port: `5231`.

- Home: `http://localhost:5231/`
- Store home: `http://localhost:5231/Store`
- Store library: `http://localhost:5231/Store/Library`
- Login: `http://localhost:5231/Identity/Account/Login`
- Admin games: `http://localhost:5231/Admin/Games`
- Admin create game: `http://localhost:5231/Admin/CreateGame`
- OpenAPI JSON: `http://localhost:5231/openapi/v1.json`
- Swagger UI: `http://localhost:5231/swagger`

## 3) Start desktop client

```powershell
cd C:\NET\TP2.NET
dotnet run --project .\Gauniv.WpfClient\Gauniv.WpfClient.csproj
```

Client test note:
- In the WPF client, click `Launch GodotGame` 4 times to open 4 game client instances.
- Preset test accounts are available in the Godot login screen: `p1@test.com`, `p2@test.com`, `p3@test.com`, `p4@test.com` (password: `password`).

## 4) Stop local servers

```powershell
cd C:\NET\TP2.NET
.\stop-local.ps1
```
