$ports = @(5231, 7000)

foreach ($port in $ports) {
    $processIds = Get-NetTCPConnection -State Listen -LocalPort $port -ErrorAction SilentlyContinue |
        Select-Object -ExpandProperty OwningProcess -Unique

    if (-not $processIds) {
        Write-Host "[local-stop] port $port not in use"
        continue
    }

    foreach ($processId in $processIds) {
        try {
            Stop-Process -Id $processId -Force -ErrorAction Stop
            Write-Host "[local-stop] stopped PID $processId on port $port"
        }
        catch {
            Write-Host "[local-stop] failed to stop PID $processId on port $port"
        }
    }
}

Write-Host "[local-stop] done."
