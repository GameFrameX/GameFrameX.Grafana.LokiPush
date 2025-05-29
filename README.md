# GameFrameX.Grafana.LokiPush

## 配置说明

### 配置优先级

配置参数按以下优先级顺序生效（从高到低）：
1. **命令行参数** - 最高优先级
2. **环境变量** - 中等优先级  
3. **配置文件** - 最低优先级

### 数据库配置

#### 方式1：命令行参数（推荐用于开发和测试）
```bash
dotnet run --project GameFrameX.Grafana.LokiPush -- --connection "Host=localhost;Port=5432;Database=loki_logs;Username=postgres;Password=your_password"
```

#### 方式2：环境变量（推荐用于Docker部署）
设置环境变量 `DATABASE_CONNECTION_STRING`：

```bash
export DATABASE_CONNECTION_STRING="Host=localhost;Port=5432;Database=loki_logs;Username=postgres;Password=your_password"
```

#### 方式3：配置文件
在 `appsettings.json` 中配置 PostgreSQL 连接字符串：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=loki_logs;Username=postgres;Password=your_password"
  }
}
```

### 批处理配置

支持通过命令行参数、环境变量配置批处理参数：

#### 命令行参数方式
```bash
dotnet run --project GameFrameX.Grafana.LokiPush -- --batch-size 200 --flush-interval 60 --max-queue 20000
```

#### 环境变量方式
| 环境变量 | 默认值 | 说明 |
|---------|--------|------|
| `BATCH_SIZE` | 100 | 批处理大小 |
| `FLUSH_INTERVAL_SECONDS` | 30 | 刷新间隔（秒） |
| `MAX_QUEUE_SIZE` | 10000 | 最大队列大小 |

### 命令行参数完整列表

| 参数 | 默认值 | 说明 |
|------|--------|------|
| `--connection` | - | 数据库连接字符串 |
| `--batch-size` | 100 | 批处理大小 |
| `--flush-interval` | 30 | 刷新间隔（秒） |
| `--max-queue` | 10000 | 最大队列大小 |
| `--port` | 5000 | HTTP服务端口 |
| `--environment` | Production | 运行环境 |
| `--verbose` | false | 启用详细日志 |
| `--help` | false | 显示帮助信息 |

### 快速启动示例

#### 开发环境启动
```bash
# 使用默认配置
dotnet run --project GameFrameX.Grafana.LokiPush

# 指定数据库和端口
dotnet run --project GameFrameX.Grafana.LokiPush -- \
  --connection "Host=localhost;Port=5432;Database=loki_logs;Username=postgres;Password=password" \
  --port 8080 \
  --environment Development \
  --verbose
```

#### 查看帮助信息
```bash
dotnet run --project GameFrameX.Grafana.LokiPush -- --help
```

更多命令行使用示例请参考 [command-line-examples.md](command-line-examples.md)

## Docker 部署

### 使用 Docker Compose（推荐）

1. 复制环境变量配置文件：
```bash
cp docker-env.example .env
```

2. 编辑 `.env` 文件，修改相应的配置值

3. 启动服务：
```bash
docker-compose up -d
```

### 使用 Docker 单独运行

1. 构建镜像：
```bash
docker build -t loki-push .
```

2. 运行容器：
```bash
docker run -d \
  --name loki-push-service \
  -p 5000:5000 \
  -e DATABASE_CONNECTION_STRING="Host=your-db-host;Port=5432;Database=loki_logs;Username=postgres;Password=your_password" \
  -e BATCH_SIZE=100 \
  -e FLUSH_INTERVAL_SECONDS=30 \
  -e MAX_QUEUE_SIZE=10000 \
  loki-push
```

### 环境变量完整列表

| 环境变量 | 说明 | 示例值 |
|---------|------|--------|
| `DATABASE_CONNECTION_STRING` | 数据库连接字符串 | `Host=localhost;Port=5432;Database=loki_logs;Username=postgres;Password=password` |
| `BATCH_SIZE` | 批处理大小 | `100` |
| `FLUSH_INTERVAL_SECONDS` | 刷新间隔（秒） | `30` |
| `MAX_QUEUE_SIZE` | 最大队列大小 | `10000` |
| `ASPNETCORE_ENVIRONMENT` | 运行环境 | `Production` |
| `ASPNETCORE_URLS` | 监听地址 | `http://+:5000` |
| `Logging__LogLevel__Default` | 默认日志级别 | `Information` |
| `Logging__LogLevel__Microsoft.AspNetCore` | ASP.NET Core日志级别 | `Warning` |