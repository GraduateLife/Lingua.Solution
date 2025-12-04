# Scripts 目录

此目录包含项目运行和维护相关的脚本文件。

## 脚本列表

### `start-projects.ps1`

并行启动所有项目（API 和 Web）。

**功能：**

- 在独立的 PowerShell 窗口中启动 Lingua.Api
- 在独立的 PowerShell 窗口中启动 Lingua.Web
- 使用 `dotnet watch run` 模式，支持热重载

**使用方法：**

```bash
make run-all
```

或直接运行：

```powershell
powershell -ExecutionPolicy Bypass -File scripts/start-projects.ps1
```

### `stop-projects.ps1`

停止所有运行中的项目进程。

**功能：**

- 检查并停止指定端口（5026, 7056, 7081）上的进程
- 停止所有相关的 dotnet 进程

**使用方法：**

```bash
make stop
```

或直接运行：

```powershell
powershell -ExecutionPolicy Bypass -File scripts/stop-projects.ps1
```
