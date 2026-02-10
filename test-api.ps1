# Test API Script for Gauniv WebServer
Write-Host "=== Testing Gauniv API ===" -ForegroundColor Cyan

$baseUrl = "http://localhost:5231"

# Test 1: Get Categories
Write-Host "`n1. Testing GET /api/categories" -ForegroundColor Yellow
try {
    $categories = Invoke-RestMethod -Uri "$baseUrl/api/categories" -Method Get
    Write-Host "✓ Success! Found $($categories.Count) categories" -ForegroundColor Green
    $categories | ConvertTo-Json -Depth 5 | Write-Host
} catch {
    Write-Host "✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: Get Games List
Write-Host "`n2. Testing GET /api/games" -ForegroundColor Yellow
try {
    $gamesUrl = "$baseUrl/api/games" + '?page=1&pageSize=5'
    $games = Invoke-RestMethod -Uri $gamesUrl -Method Get
    Write-Host "✓ Success! Total games: $($games.totalCount), Page: $($games.page)" -ForegroundColor Green
    $games | ConvertTo-Json -Depth 5 | Write-Host
} catch {
    Write-Host "✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Get Game Details
Write-Host "`n3. Testing GET /api/games/1" -ForegroundColor Yellow
try {
    $game = Invoke-RestMethod -Uri "$baseUrl/api/games/1" -Method Get
    Write-Host "✓ Success! Game: $($game.name)" -ForegroundColor Green
    $game | ConvertTo-Json -Depth 5 | Write-Host
} catch {
    Write-Host "✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 4: Login
Write-Host "`n4. Testing POST /api/auth/login" -ForegroundColor Yellow
try {
    $loginBody = @{
        email = "test@test.com"
        password = "password"
        rememberMe = $false
    } | ConvertTo-Json

    # Create a session to store cookies
    $session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
    
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json" -WebSession $session
    Write-Host "✓ Login successful! User: $($loginResponse.email)" -ForegroundColor Green
    $loginResponse | ConvertTo-Json | Write-Host
} catch {
    Write-Host "✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
    exit
}

# Test 5: Purchase Game
Write-Host "`n5. Testing POST /api/games/4/purchase (Authenticated)" -ForegroundColor Yellow
try {
    $purchaseResponse = Invoke-RestMethod -Uri "$baseUrl/api/games/4/purchase" -Method Post -WebSession $session
    Write-Host "✓ Purchase successful! Game: $($purchaseResponse.gameName)" -ForegroundColor Green
    $purchaseResponse | ConvertTo-Json | Write-Host
} catch {
    Write-Host "✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 6: Get Owned Games
Write-Host "`n6. Testing GET /api/games/owned (Authenticated)" -ForegroundColor Yellow
try {
    $ownedGames = Invoke-RestMethod -Uri "$baseUrl/api/games/owned" -Method Get -WebSession $session
    Write-Host "✓ Success! You own $($ownedGames.Count) games" -ForegroundColor Green
    $ownedGames | ConvertTo-Json -Depth 5 | Write-Host
} catch {
    Write-Host "✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 7: Download Game (just test if we get a response)
Write-Host "`n7. Testing GET /api/games/1/download (Authenticated)" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/games/1/download" -Method Get -WebSession $session
    Write-Host "✓ Download endpoint accessible! Status: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "Content-Type: $($response.Headers['Content-Type'])" -ForegroundColor Gray
    Write-Host "Content-Length: $($response.Headers['Content-Length']) bytes" -ForegroundColor Gray
} catch {
    Write-Host "Expected: You might not own this game yet" -ForegroundColor Yellow
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Gray
}

# Test 8: Get Categories by ID
Write-Host "`n8. Testing GET /api/categories/1" -ForegroundColor Yellow
try {
    $category = Invoke-RestMethod -Uri "$baseUrl/api/categories/1" -Method Get
    Write-Host "✓ Success! Category: $($category.name)" -ForegroundColor Green
    $category | ConvertTo-Json -Depth 5 | Write-Host
} catch {
    Write-Host "✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 9: Get Games by Category
Write-Host "`n9. Testing GET /api/categories/1/games" -ForegroundColor Yellow
try {
    $categoryGames = Invoke-RestMethod -Uri "$baseUrl/api/categories/1/games" -Method Get
    Write-Host "✓ Success! Found $($categoryGames.Games.Count) games in category '$($categoryGames.Category.name)'" -ForegroundColor Green
    $categoryGames | ConvertTo-Json -Depth 5 | Write-Host
} catch {
    Write-Host "✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== All API Tests Completed ===" -ForegroundColor Cyan
