# 命令行参数和环境变量合并功能

## 概述

本项目实现了命令行参数和环境变量的智能合并功能，优先级顺序为：

**命令行参数 > 环境变量 > 默认值**

## 支持的配置参数

### 命令行参数

| 参数 | 默认值 | 描述 |
|------|--------|------|
| `--connection` | - | 数据库连接字符串 |
| `--batch-size` | 100 | 批处理大小 |
| `--flush-interval` | 30 | 刷新间隔（秒） |
| `--max-queue` | 10000 | 最大队列大小 |
| `--port` | 5000 | HTTP服务端口 |
| `--environment` | Production | 运行环境 |
| `--verbose` | false | 启用详细日志 |
| `--help` | - | 显示帮助信息 |

### 环境变量

| 环境变量 | 对应命令行参数 | 描述 |
|----------|----------------|------|
| `DATABASE_CONNECTION_STRING` | `--connection` | 数据库连接字符串 |
| `BATCH_SIZE` | `--batch-size` | 批处理大小 |
| `FLUSH_INTERVAL_SECONDS` | `--flush-interval` | 刷新间隔（秒） |
| `MAX_QUEUE_SIZE` | `--max-queue` | 最大队列大小 |
| `HTTP_PORT` | `--port` | HTTP服务端口 |
| `ASPNETCORE_ENVIRONMENT` | `--environment` | 运行环境 |
| `VERBOSE_LOGGING` | `--verbose` | 启用详细日志 |

## 合并逻辑

### 实现原理

1. **命令行解析**：使用 CommandLineParser 库解析命令行参数
2. **环境变量检查**：对于未通过命令行设置的参数，检查对应的环境变量
3. **默认值应用**：如果命令行和环境变量都未设置，使用默认值

### 核心代码

```csharp
static LauncherOptions MergeOptionsWithEnvironment(LauncherOptions? cmdOptions)
{
    var options = cmdOptions ?? new LauncherOptions();
    
    // 连接字符串：命令行 > 环境变量
    if (string.IsNullOrEmpty(options.ConnectionString))
    {
        options.ConnectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
    }
    
    // 批处理大小：命令行 > 环境变量 > 默认值
    if (cmdOptions?.BatchSize == 100) // 检查是否为默认值
    {
        if (int.TryParse(Environment.GetEnvironmentVariable("BATCH_SIZE"), out var envBatchSize))
        {
            options.BatchSize = envBatchSize;
        }
    }
    
    // ... 其他参数类似处理
    
    return options;
}
```

## 使用示例

### 示例 1：仅使用环境变量

```bash
# 设置环境变量
export BATCH_SIZE=200
export HTTP_PORT=8080
export ASPNETCORE_ENVIRONMENT=Development

# 启动服务
dotnet run --project GameFrameX.Grafana.LokiPush
```

结果：BatchSize=200, Port=8080, Environment=Development

### 示例 2：命令行参数覆盖环境变量

```bash
# 环境变量
export BATCH_SIZE=200
export HTTP_PORT=8080

# 命令行参数覆盖
dotnet run --project GameFrameX.Grafana.LokiPush -- --batch-size 500 --port 9090
```

结果：BatchSize=500（命令行覆盖）, Port=9090（命令行覆盖）

### 示例 3：混合配置

```bash
# 环境变量
export BATCH_SIZE=300
export MAX_QUEUE_SIZE=15000

# 部分命令行参数
dotnet run --project GameFrameX.Grafana.LokiPush -- --port 7777 --verbose
```

结果：
- BatchSize=300（环境变量）
- MaxQueueSize=15000（环境变量）
- Port=7777（命令行）
- Verbose=true（命令行）
- FlushInterval=30（默认值）

## 配置优先级验证

可以使用提供的测试脚本验证配置合并功能：

```powershell
# 运行完整测试
.\test-merge-config.ps1

# 运行简单测试
.\test-simple-merge.ps1
```

## 注意事项

1. **类型转换**：环境变量始终是字符串，系统会自动进行类型转换
2. **布尔值**：环境变量中的布尔值应设置为 "true" 或 "false"
3. **默认值检查**：系统通过比较当前值与默认值来判断是否需要检查环境变量
4. **错误处理**：无效的环境变量值会被忽略，使用默认值

## 调试技巧

启用详细日志模式查看配置加载过程：

```bash
dotnet run --project GameFrameX.Grafana.LokiPush -- --verbose
```

服务启动时会显示最终的配置值：

```
批处理配置: BatchSize=100, FlushInterval=30s, MaxQueue=10000
监听端口: 5000
运行环境: Production
```