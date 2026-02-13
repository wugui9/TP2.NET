$root = Split-Path -Parent $MyInvocation.MyCommand.Path

Start-Process powershell -ArgumentList @(
    '-NoExit',
    '-Command',
    "Set-Location '$root'; dotnet run --project '.\Gauniv.WebServer\Gauniv.WebServer.csproj' --launch-profile http -p:UseAppHost=false"
)

Start-Process powershell -ArgumentList @(
    '-NoExit',
    '-Command',
    "Set-Location '$root'; dotnet run --project '.\Gauniv.GameServer\Gauniv.GameServer.csproj'"
)

Write-Host 'Started WebServer and GameServer in two new terminal windows.'