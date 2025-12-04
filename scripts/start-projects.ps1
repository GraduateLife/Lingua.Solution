# 并行启动所有项目（watch 模式，支持热重载）

Write-Host "并行启动所有项目（watch 模式）..." -ForegroundColor Green

$solutionPath = Split-Path -Parent $PSScriptRoot

# 启动 API 项目
Write-Host "启动 Lingua.Api..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$solutionPath\Lingua.Api'; dotnet watch run" -WindowStyle Normal

# 等待 2 秒
Start-Sleep -Seconds 2

# 启动 Web 项目
Write-Host "启动 Lingua.Web..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$solutionPath\Lingua.Web'; dotnet watch run" -WindowStyle Normal

Write-Host ""
Write-Host "✓ 所有项目已启动（支持热重载）！" -ForegroundColor Green
Write-Host "  API: https://localhost:7081" -ForegroundColor Cyan
Write-Host "  Web: http://localhost:5026 / https://localhost:7056" -ForegroundColor Cyan

