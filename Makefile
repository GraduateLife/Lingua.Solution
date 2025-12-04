.PHONY: build run run-api run-web run-all stop clean help

# 默认目标
.DEFAULT_GOAL := help

# 构建所有项目
build:
	@echo "构建所有项目..."
	dotnet build

# 运行 API 项目
run-api:
	@echo "启动 Lingua.Api..."
	cd Lingua.Api && dotnet watch run

# 运行 Web 项目
run-web:
	@echo "启动 Lingua.Web..."
	cd Lingua.Web && dotnet watch run

# 并行运行所有项目（使用 watch 模式，支持热重载）
run-all:
	@powershell -NoProfile -ExecutionPolicy Bypass -File scripts/start-projects.ps1

# 停止所有项目
stop:
	@powershell -NoProfile -ExecutionPolicy Bypass -File scripts/stop-projects.ps1

# 清理构建文件
clean:
	@echo "清理构建文件..."
	dotnet clean
	@echo "清理完成"

# 显示帮助信息
help:
	@echo "可用的命令:"
	@echo "  make build      - 构建所有项目"
	@echo "  make run-api    - 运行 API 项目（watch 模式）"
	@echo "  make run-web    - 运行 Web 项目（watch 模式）"
	@echo "  make run-all    - 并行运行所有项目（watch 模式，支持热重载）"
	@echo "  make stop       - 停止所有运行中的项目"
	@echo "  make clean      - 清理构建文件"
	@echo "  make help       - 显示此帮助信息"

