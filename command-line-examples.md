# 命令行参数使用示例

## 基本用法

### 显示帮助信息
```bash
dotnet run --project GameFrameX.Grafana.LokiPush -- --help
```

### 使用默认配置启动
```bash
dotnet run --project GameFrameX.Grafana.LokiPush
```

## 配置数据库连接

### 通过命令行参数设置数据库连接
```bash
dotnet run --project GameFrameX.Grafana.LokiPush -- --connection "Host=localhost;Port=5432;Database=loki_logs;Username=postgres;Password=your_password"
```

## 配置批处理参数

### 设置批处理大小
```bash
dotnet run --project GameFrameX.Grafana.LokiPush -- --batch-size 200
```

### 设置刷新间隔
```bash
dotnet run --project GameFrameX.Grafana.LokiPush -- --flush-interval 60
```

### 设置最大队列大小
```bash
dotnet run --project GameFrameX.Grafana.LokiPush -- --max-queue 20000
```

## 配置服务端口

### 设置HTTP服务端口
```bash
dotnet run --project GameFrameX.Grafana.LokiPush -- --port 8080
```

## 配置运行环境

### 设置为开发环境
```bash
dotnet run --project GameFrameX.Grafana.LokiPush -- --environment Development
```

### 设置为生产环境
```bash
dotnet run --project GameFrameX.Grafana.LokiPush -- --environment Production
```

## 启用详细日志

### 启用详细日志输出
```bash
dotnet run --project GameFrameX.Grafana.LokiPush -- --verbose
```

## 组合使用多个参数

### 完整配置示例
```bash
dotnet run --project GameFrameX.Grafana.LokiPush -- \
  --connection "Host=localhost;Port=5432;Database=loki_logs;Username=postgres;Password=your_password" \
  --batch-size 150 \
  --flush-interval 45 \
  --max-queue 15000 \
  --port 8080 \
  --environment Development \
  --verbose
```



## 发布后使用

### 发布应用
```bash
dotnet publish GameFrameX.Grafana.LokiPush -c Release -o ./publish
```

### 运行发布的应用
```bash
# Windows
.\publish\GameFrameX.Grafana.LokiPush.exe --connection "Host=localhost;Port=5432;Database=loki_logs;Username=postgres;Password=your_password" --port 8080

# Linux/macOS
./publish/GameFrameX.Grafana.LokiPush --connection "Host=localhost;Port=5432;Database=loki_logs;Username=postgres;Password=your_password" --port 8080
```

## 参数优先级

配置参数的优先级顺序（从高到低）：
1. **命令行参数** - 最高优先级
2. **环境变量** - 中等优先级
3. **配置文件** (appsettings.json) - 最低优先级

例如，如果同时设置了命令行参数 `--batch-size 200`、环境变量 `BATCH_SIZE=150` 和配置文件中的默认值，最终会使用命令行参数的值 `200`。

## 注意事项

1. 在使用 `dotnet run` 时，需要使用 `--` 来分隔 dotnet 的参数和应用程序的参数
2. 包含空格的参数值需要用引号包围
3. 数据库连接字符串中的特殊字符可能需要转义
4. 使用 `--help` 可以查看所有可用的命令行选项