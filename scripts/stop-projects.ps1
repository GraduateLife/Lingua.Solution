# 停止所有项目
Write-Host "停止所有项目..."

$ports = @(5026, 7056, 7081)
foreach ($port in $ports) {
    $conn = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue
    if ($conn) {
        $proc = Get-Process -Id $conn.OwningProcess -ErrorAction SilentlyContinue
        if ($proc) {
            Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
            Write-Host "已停止端口 $port 上的进程"
        }
    }
}

# 停止所有 dotnet 进程（Lingua 相关）
Get-Process dotnet -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

Write-Host "已停止所有项目"

