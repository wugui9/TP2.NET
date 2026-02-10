# SQLite Database Viewer Script
# 查看 Gauniv 数据库内容

$dbPath = "Gauniv.WebServer\gauniv.db"

Write-Host "================================" -ForegroundColor Cyan
Write-Host "Gauniv Database Viewer" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "数据库文件: $dbPath" -ForegroundColor Yellow
Write-Host ""

# Load System.Data.SQLite
Add-Type -Path "System.Data.SQLite.dll" -ErrorAction SilentlyContinue

try {
    # Create connection
    $connectionString = "Data Source=$dbPath;Version=3;Read Only=True;"
    $connection = New-Object System.Data.SQLite.SQLiteConnection($connectionString)
    $connection.Open()
    
    # Function to execute query
    function Execute-Query {
        param($query, $title)
        
        Write-Host "=== $title ===" -ForegroundColor Green
        $command = $connection.CreateCommand()
        $command.CommandText = $query
        $reader = $command.ExecuteReader()
        
        $table = New-Object System.Data.DataTable
        $table.Load($reader)
        $table | Format-Table -AutoSize
        Write-Host ""
    }
    
    # Query tables
    Execute-Query "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;" "数据库表列表"
    Execute-Query "SELECT * FROM Games LIMIT 10;" "游戏列表 (前10条)"
    Execute-Query "SELECT * FROM Categories;" "分类列表"
    Execute-Query "SELECT UserName, Email, FirstName, LastName, Password, SUBSTR(PasswordHash, 1, 30) as PasswordHash FROM AspNetUsers;" "用户列表（含密码）"
    Execute-Query "SELECT COUNT(*) as TotalGames FROM Games;" "游戏总数"
    Execute-Query "SELECT COUNT(*) as TotalUsers FROM AspNetUsers;" "用户总数"
    
    $connection.Close()
}
catch {
    Write-Host "无法使用 System.Data.SQLite，请使用以下方法之一查看数据库：" -ForegroundColor Red
    Write-Host ""
    Write-Host "方法 1: 在浏览器中访问 Swagger" -ForegroundColor Yellow
    Write-Host "   http://localhost:5231/swagger" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "方法 2: 下载 DB Browser for SQLite" -ForegroundColor Yellow
    Write-Host "   https://sqlitebrowser.org/dl/" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "方法 3: 安装 VS Code 扩展" -ForegroundColor Yellow
    Write-Host "   搜索并安装: SQLite Viewer" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "方法 4: 下载 SQLite 命令行工具" -ForegroundColor Yellow
    Write-Host "   https://www.sqlite.org/download.html" -ForegroundColor Cyan
}
