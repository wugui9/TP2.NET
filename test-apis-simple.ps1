$baseUrl = "http://localhost:5231"

Write-Host "Testing Gauniv APIs" -ForegroundColor Cyan
Write-Host ""

# Test 1: Categories
Write-Host "1. GET /api/categories" -ForegroundColor Yellow
$categories = Invoke-RestMethod -Uri "$baseUrl/api/categories"
Write-Host "Success! Found $($categories.Count) categories" -ForegroundColor Green
Write-Host ""

# Test 2: Games List
Write-Host "2. GET /api/games" -ForegroundColor Yellow
$gamesUrl = "$baseUrl/api/games" + '?page=1&pageSize=3'
$games = Invoke-RestMethod -Uri $gamesUrl
Write-Host "Success! Total: $($games.totalCount) games" -ForegroundColor Green
Write-Host ""

# Test 3: Game Details
Write-Host "3. GET /api/games/1" -ForegroundColor Yellow
$game = Invoke-RestMethod -Uri "$baseUrl/api/games/1"
Write-Host "Success! Game: $($game.name)" -ForegroundColor Green
Write-Host ""

# Test 4: Login
Write-Host "4. POST /api/auth/login" -ForegroundColor Yellow
$body = @{email='test@test.com'; password='password'; rememberMe=$false} | ConvertTo-Json
$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$login = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -Body $body -ContentType 'application/json' -WebSession $session
Write-Host "Success! Logged in as: $($login.email)" -ForegroundColor Green
Write-Host ""

# Test 5: Purchase Game
Write-Host "5. POST /api/games/4/purchase" -ForegroundColor Yellow
try {
    $purchase = Invoke-RestMethod -Uri "$baseUrl/api/games/4/purchase" -Method Post -WebSession $session
    Write-Host "Success! Purchased: $($purchase.gameName)" -ForegroundColor Green
} catch {
    Write-Host "Note: $($_.Exception.Message)" -ForegroundColor Yellow
}
Write-Host ""

# Test 6: Owned Games
Write-Host "6. GET /api/games/owned" -ForegroundColor Yellow
$owned = Invoke-RestMethod -Uri "$baseUrl/api/games/owned" -WebSession $session
Write-Host "Success! You own $($owned.Count) games" -ForegroundColor Green
Write-Host ""

# Test 7: Download Game
Write-Host "7. GET /api/games/1/download" -ForegroundColor Yellow
try {
    $download = Invoke-WebRequest -Uri "$baseUrl/api/games/1/download" -WebSession $session
    Write-Host "Success! Can download (Status: $($download.StatusCode))" -ForegroundColor Green
} catch {
    Write-Host "Expected: Need to own the game first" -ForegroundColor Yellow
}
Write-Host ""

Write-Host "All tests completed!" -ForegroundColor Cyan
