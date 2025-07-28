# GameFrameX.Grafana.LokiPush

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15+-blue.svg)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-supported-blue.svg)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

一个高性能的 Grafana Loki 日志推送接收服务，专为游戏数据分析而设计。该服务接收来自 Grafana Loki 的日志推送，并将其解析、转换后批量存储到 PostgreSQL 数据库中，支持动态表结构和高并发处理。

## ✨ 核心特性

- 🚀 **高性能批处理** - 支持批量处理和异步写入，提升数据库写入性能
- 🔄 **动态表结构** - 基于 JSON 配置文件自动创建和同步数据库表结构
- 🎮 **游戏数据专用** - 针对游戏行为数据进行优化，支持丰富的游戏事件字段
- 🐳 **容器化部署** - 完整的 Docker 和 Docker Compose 支持
- ⚙️ **灵活配置** - 支持命令行参数、环境变量和配置文件多种配置方式
- 📊 **监控友好** - 内置健康检查和详细日志记录
- 🔧 **易于扩展** - 模块化设计，易于添加新的数据处理逻辑

## 🏗️ 系统架构

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Grafana Loki  │───▶│  LokiPush API    │───▶│   PostgreSQL    │
│                 │    │                  │    │                 │
│  日志推送源      │    │  • 接收日志数据   │    │  • 游戏事件表    │
│                 │    │  • 批处理队列     │    │  • 用户行为表    │
│                 │    │  • 数据转换       │    │  • 系统日志表    │
└─────────────────┘    └──────────────────┘    └─────────────────┘
```

### 主要组件

- **LokiController** - RESTful API 控制器，处理 Loki 推送请求
- **BatchProcessingService** - 批处理服务，管理日志队列和批量写入
- **DatabaseService** - 数据库服务，处理动态表创建和数据插入
- **ZeroDbContext** - 基于 FreeSql 的零实体数据库上下文

## 🚀 快速开始

### 环境要求

- .NET 8.0 或更高版本
- PostgreSQL 12+ 数据库
- Docker (可选，用于容器化部署)

### 1. 克隆项目

```bash
git clone https://github.com/GameFrameX/GameFrameX.Grafana.LokiPush.git
cd GameFrameX.Grafana.LokiPush
```

### 2. 配置数据库

创建 PostgreSQL 数据库：

```sql
CREATE DATABASE loki_logs;
CREATE USER loki_user WITH PASSWORD 'your_password';
GRANT ALL PRIVILEGES ON DATABASE loki_logs TO loki_user;
```

### 3. 运行服务

#### 方式一：直接运行

```bash
dotnet run --project GameFrameX.Grafana.LokiPush -- \
  --connection "Host=localhost;Port=5432;Database=loki_logs;Username=loki_user;Password=your_password" \
  --port 5000
```

#### 方式二：Docker Compose (推荐)

```bash
# 复制环境变量配置文件
cp docker-env.example .env

# 编辑 .env 文件，修改数据库密码等配置
nano .env

# 启动服务
docker-compose up -d
```

### 4. 验证服务

```bash
# 健康检查
curl http://localhost:5000/loki/api/v1/health

# 服务信息 (仅开发环境)
curl http://localhost:5000/loki/api/v1/info
```

## ⚙️ 配置说明

### 配置优先级

配置参数按以下优先级顺序生效（从高到低）：

1. **命令行参数** - 最高优先级
2. **环境变量** - 中等优先级
3. **配置文件** - 最低优先级

### 命令行参数

| 参数               | 默认值     | 说明             |
| ------------------ | ---------- | ---------------- |
| `--connection`     | -          | 数据库连接字符串 |
| `--batch-size`     | 100        | 批处理大小       |
| `--flush-interval` | 30         | 刷新间隔（秒）   |
| `--max-queue`      | 10000      | 最大队列大小     |
| `--port`           | 5000       | HTTP 服务端口    |
| `--environment`    | Production | 运行环境         |
| `--verbose`        | false      | 启用详细日志     |
| `--help`           | false      | 显示帮助信息     |

### 环境变量

| 环境变量                     | 说明             | 示例值                                                                            |
| ---------------------------- | ---------------- | --------------------------------------------------------------------------------- |
| `DATABASE_CONNECTION_STRING` | 数据库连接字符串 | `Host=localhost;Port=5432;Database=loki_logs;Username=postgres;Password=password` |
| `BATCH_SIZE`                 | 批处理大小       | `100`                                                                             |
| `FLUSH_INTERVAL_SECONDS`     | 刷新间隔（秒）   | `30`                                                                              |
| `MAX_QUEUE_SIZE`             | 最大队列大小     | `10000`                                                                           |
| `ASPNETCORE_ENVIRONMENT`     | 运行环境         | `Production`                                                                      |
| `ASPNETCORE_URLS`            | 监听地址         | `http://+:5000`                                                                   |

### 配置示例

#### 开发环境

```bash
dotnet run --project GameFrameX.Grafana.LokiPush -- \
  --connection "Host=localhost;Port=5432;Database=loki_logs;Username=postgres;Password=password" \
  --environment Development \
  --verbose \
  --batch-size 50 \
  --flush-interval 10
```

#### 生产环境

```bash
export DATABASE_CONNECTION_STRING="Host=prod-db;Port=5432;Database=loki_logs;Username=loki_user;Password=secure_password"
export BATCH_SIZE=200
export FLUSH_INTERVAL_SECONDS=60
export MAX_QUEUE_SIZE=50000

dotnet GameFrameX.Grafana.LokiPush.dll
```

## 📊 数据结构

### 表结构配置

项目使用 `json/TableDescriptor.json` 文件定义数据库表结构。该文件基于 [FreeSql.Extensions.ZeroEntity](https://github.com/dotnetcore/FreeSql/blob/d94724ca79a6f1344d100ce967c573b64eedf60d/Extensions/FreeSql.Extensions.ZeroEntity/ZeroDescriptor.cs) <mcreference link="https://github.com/dotnetcore/FreeSql/blob/d94724ca79a6f1344d100ce967c573b64eedf60d/Extensions/FreeSql.Extensions.ZeroEntity/ZeroDescriptor.cs" index="0">0</mcreference> 的 `TableDescriptor` 和 `ColumnDescriptor` 类型定义，包含了游戏中各种事件的表定义，如：

- **client_start** - 游戏启动事件
- **client_user_login** - 用户登录事件
- **client_user_register** - 用户注册事件
- **client_start_patch_init** - 补丁初始化事件
- **client_start_patch_done** - 补丁完成事件

### JSON数据结构定义规则

#### TableDescriptor 结构

每个表定义遵循以下JSON结构：

```json
{
  "Name": "表名",
  "Comment": "表注释",
  "Columns": [
    // ColumnDescriptor 数组
  ]
}
```

**字段说明：**

| 字段名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `Name` | `string` | ✅ | 表名，对应数据库中的表名 |
| `Comment` | `string` | ❌ | 表的中文注释，用于说明表的用途 |
| `Columns` | `ColumnDescriptor[]` | ✅ | 列定义数组，包含表中所有字段的定义 |

#### ColumnDescriptor 结构

每个字段定义遵循以下JSON结构：

```json
{
  "Name": "字段名",
  "MapType": "System.String",
  "Comment": "字段注释",
  "IsPrimary": false,
  "IsIdentity": false,
  "IsNullable": true,
  "StringLength": 255,
  "Precision": 0,
  "Scale": 0,
  "ServerTime": "Unspecified",
  "IsVersion": false,
  "InsertValueSql": null
}
```

**字段说明：**

| 字段名 | 类型 | 必填 | 默认值 | 说明 |
|--------|------|------|--------|------|
| `Name` | `string` | ✅ | - | 字段名，对应数据库列名 |
| `MapType` | `string` | ✅ | - | .NET类型映射，如 `System.String`、`System.Int64`、`System.DateTime` |
| `Comment` | `string` | ❌ | `null` | 字段的中文注释说明 |
| `IsPrimary` | `bool` | ❌ | `false` | 是否为主键字段 |
| `IsIdentity` | `bool` | ❌ | `false` | 是否为自增字段 |
| `IsNullable` | `bool` | ❌ | `true` | 是否允许为空值 |
| `StringLength` | `int` | ❌ | `255` | 字符串类型的最大长度限制 |
| `Precision` | `int` | ❌ | `0` | 数值类型的总位数（精度） |
| `Scale` | `int` | ❌ | `0` | 数值类型的小数位数 |
| `ServerTime` | `string` | ❌ | `"Unspecified"` | 时间类型的服务器时区设置 |
| `IsVersion` | `bool` | ❌ | `false` | 是否为版本控制字段 |
| `InsertValueSql` | `string` | ❌ | `null` | 插入时的SQL表达式，如 `"YitIdHelper.NextId()"` |

#### 支持的数据类型

| .NET类型 | 说明 | 示例值 |
|----------|------|--------|
| `System.Int64` | 64位整数 | `1234567890123456789` |
| `System.Int32` | 32位整数 | `1920` |
| `System.String` | 字符串 | `"用户名"` |
| `System.DateTime` | 日期时间 | `"2024-01-01T12:00:00Z"` |
| `System.Boolean` | 布尔值 | `true` |
| `System.Decimal` | 高精度小数 | `99.99` |
| `System.Double` | 双精度浮点数 | `3.14159` |

### 完整示例

以下是一个完整的表定义示例：

```json
{
  "Name": "client_user_login",
  "Comment": "用户登录事件",
  "Columns": [
    {
      "Name": "id",
      "MapType": "System.Int64",
      "Comment": "主键ID",
      "IsPrimary": true,
      "IsIdentity": true,
      "IsNullable": false,
      "InsertValueSql": "YitIdHelper.NextId()"
    },
    {
      "Name": "account_id",
      "MapType": "System.String",
      "Comment": "账号ID",
      "IsNullable": false,
      "StringLength": 50
    },
    {
      "Name": "role_id",
      "MapType": "System.String",
      "Comment": "角色ID",
      "IsNullable": true,
      "StringLength": 50
    },
    {
      "Name": "server_id",
      "MapType": "System.String",
      "Comment": "服务器ID",
      "IsNullable": false,
      "StringLength": 20
    },
    {
      "Name": "login_time",
      "MapType": "System.DateTime",
      "Comment": "登录时间",
      "IsNullable": false,
      "ServerTime": "Utc"
    },
    {
      "Name": "ip_address",
      "MapType": "System.String",
      "Comment": "登录IP地址",
      "IsNullable": true,
      "StringLength": 45
    },
    {
      "Name": "device_type",
      "MapType": "System.String",
      "Comment": "设备类型",
      "IsNullable": true,
      "StringLength": 20
    },
    {
      "Name": "created_time",
      "MapType": "System.DateTime",
      "Comment": "记录创建时间",
      "IsNullable": false,
      "ServerTime": "Utc"
    }
  ]
}
```

### 配置文件位置

- **开发环境**：`GameFrameX.Grafana.LokiPush/json/TableDescriptor.json`
- **运行时**：`./json/TableDescriptor.json`（相对于可执行文件目录）

## 📋 完整字段类型参考 (基于Template.json)

以下是基于 `json/Template.json` 文件的完整字段类型定义和示例，展示了游戏数据分析中常用的所有字段类型：

### 🔑 基础标识字段

| 字段名 | 类型 | 说明 | 示例值 |
|--------|------|------|--------|
| `id` | `System.Int64` | 主ID | `1234567890123456789` |
| `account_id` | `System.String` | 账号ID | `"user_12345"` |
| `role_id` | `System.String` | 角色ID | `"role_67890"` |
| `server_id` | `System.String` | 服务器ID | `"server_001"` |
| `device_id` | `System.String` | 设备ID | `"device_abc123"` |

### 🌍 地域和渠道字段

| 字段名 | 类型 | 说明 | 示例值 |
|--------|------|------|--------|
| `country` | `System.String` | 国家 | `"CN"` |
| `channel` | `System.String` | 渠道 | `"official"` |
| `orig_channel` | `System.String` | 原始渠道 | `"google_play"` |
| `sub_channel` | `System.String` | 子渠道 | `"promotion_001"` |
| `server_channel` | `System.String` | 服务器渠道 | `"asia_server"` |
| `domain` | `System.String` | 环境 | `"production"` |

### 💰 金额相关字段

| 字段名 | 类型 | 说明 | 示例值 |
|--------|------|------|--------|
| `account_history_money` | `System.String` | 账号历史金额 | `"999.99"` |
| `role_history_money` | `System.String` | 角色历史金额 | `"599.99"` |
| `device_history_money` | `System.String` | 设备历史金额 | `"1299.99"` |
| `payment` | `System.String` | 支付方式 | `"alipay"` |

### 🖥️ 系统和设备字段

| 字段名 | 类型 | 说明 | 示例值 |
|--------|------|------|--------|
| `system` | `System.String` | 应用系统 | `"Android"` |
| `device_type` | `System.String` | 设备类型 | `"Mobile"` |
| `device_model` | `System.String` | 设备型号 | `"iPhone 15 Pro"` |
| `os` | `System.String` | 操作系统 | `"iOS 17.1"` |
| `platform` | `System.String` | Unity平台 | `"iOS"` |
| `ip` | `System.String` | 行为发生时ip地址 | `"192.168.1.100"` |

### 🔧 硬件配置字段

| 字段名 | 类型 | 说明 | 示例值 |
|--------|------|------|--------|
| `processor_type` | `System.String` | 处理器类型 | `"Apple A17 Pro"` |
| `processor_count` | `System.String` | 处理器数量 | `"6"` |
| `processor_frequency` | `System.String` | 处理器频率 | `"3.78 GHz"` |
| `system_memory_size` | `System.String` | 系统内存大小 | `"8 GB"` |

### 🎮 图形设备字段

| 字段名 | 类型 | 说明 | 示例值 |
|--------|------|------|--------|
| `graphics_device_name` | `System.String` | 图形设备名称 | `"Apple GPU"` |
| `graphics_device_type` | `System.String` | 图形设备类型 | `"Integrated"` |
| `graphics_memory_size` | `System.String` | 图形内存大小 | `"Shared"` |
| `graphics_device_version` | `System.String` | 图形设备版本 | `"Metal"` |
| `graphics_shader_level` | `System.String` | 图形着色器级别 | `"50"` |

### 📱 屏幕相关字段

| 字段名 | 类型 | 说明 | 示例值 |
|--------|------|------|--------|
| `screen_width` | `System.Int32` | 屏幕宽度 | `1920` |
| `screen_height` | `System.Int32` | 屏幕高度 | `1080` |
| `screen_dpi` | `System.String` | 屏幕DPI | `"460"` |
| `screen_refresh_rate` | `System.String` | 屏幕刷新率 | `"120Hz"` |

### 🌐 语言和网络字段

| 字段名 | 类型 | 说明 | 示例值 |
|--------|------|------|--------|
| `system_language` | `System.String` | 系统语言 | `"zh-CN"` |
| `current_culture` | `System.String` | 当前语言 | `"zh-Hans-CN"` |
| `network_type` | `System.String` | 网络类型 | `"WiFi"` |

### 📅 时间相关字段

| 字段名 | 类型 | 说明 | 示例值 |
|--------|------|------|--------|
| `server_open_time` | `System.DateTime` | 服务器开服时间 | `"2024-01-01T00:00:00Z"` |
| `created_time` | `System.DateTime` | 创建时间 | `"2024-01-15T12:30:45Z"` |
| `active_time` | `System.DateTime` | 激活时间 | `"2024-01-15T12:00:00Z"` |
| `latest_online_time` | `System.DateTime` | 最近在线时间 | `"2024-01-15T15:30:00Z"` |

### 🎯 版本和应用字段

| 字段名 | 类型 | 说明 | 示例值 |
|--------|------|------|--------|
| `app_version` | `System.String` | 应用版本 | `"1.2.3"` |
| `unity_version` | `System.String` | Unity版本 | `"2023.3.0f1"` |
| `nick_name` | `System.String` | 昵称 | `"玩家昵称"` |

### 🎲 游戏业务字段 (示例：古宝系统)

| 字段名 | 类型 | 说明 | 示例值 |
|--------|------|------|--------|
| `relic_id` | `System.Int32` | 古宝ID | `10001` |
| `change_reason` | `System.Int32` | 变化原因 | `1` |
| `level` | `System.Int32` | 古宝等级 | `5` |
| `star` | `System.Int32` | 古宝星数 | `3` |
| `relic_quality` | `System.String` | 古宝品质 | `"Epic"` |
| `cost` | `System.String` | 本次变化消耗资源 | `"{\"gold\":1000,\"gems\":50}"` |

### 📝 完整表定义示例 (基于Template.json)

以下是基于 `Template.json` 的完整表定义示例：

```json
{
  "Name": "client_relic_develop",
  "Comment": "古宝养成",
  "Columns": [
    {
      "Name": "id",
      "MapType": "System.Int64",
      "Comment": "主ID",
      "IsPrimary": true,
      "IsIdentity": true,
      "IsNullable": false,
      "InsertValueSql": "YitIdHelper.NextId()"
    },
    {
      "Name": "account_id",
      "MapType": "System.String",
      "Comment": "账号ID",
      "IsNullable": false,
      "StringLength": 50
    },
    {
      "Name": "role_id",
      "MapType": "System.String",
      "Comment": "角色ID",
      "IsNullable": true,
      "StringLength": 50
    },
    {
      "Name": "server_id",
      "MapType": "System.String",
      "Comment": "服务器ID",
      "IsNullable": false,
      "StringLength": 20
    },
    {
      "Name": "relic_id",
      "MapType": "System.Int32",
      "Comment": "古宝ID",
      "IsNullable": false
    },
    {
      "Name": "change_reason",
      "MapType": "System.Int32",
      "Comment": "变化原因",
      "IsNullable": false
    },
    {
      "Name": "level",
      "MapType": "System.Int32",
      "Comment": "古宝等级",
      "IsNullable": false
    },
    {
      "Name": "star",
      "MapType": "System.Int32",
      "Comment": "古宝星数",
      "IsNullable": false
    },
    {
      "Name": "relic_quality",
      "MapType": "System.String",
      "Comment": "古宝品质",
      "IsNullable": true,
      "StringLength": 20
    },
    {
      "Name": "cost",
      "MapType": "System.String",
      "Comment": "本次变化消耗资源",
      "IsNullable": true,
      "StringLength": 4096
    },
    {
      "Name": "device_type",
      "MapType": "System.String",
      "Comment": "设备类型",
      "IsNullable": true,
      "StringLength": 20
    },
    {
      "Name": "system",
      "MapType": "System.String",
      "Comment": "应用系统",
      "IsNullable": true,
      "StringLength": 50
    },
    {
      "Name": "app_version",
      "MapType": "System.String",
      "Comment": "应用版本",
      "IsNullable": true,
      "StringLength": 20
    },
    {
      "Name": "screen_width",
      "MapType": "System.Int32",
      "Comment": "屏幕宽度",
      "IsNullable": true
    },
    {
      "Name": "screen_height",
      "MapType": "System.Int32",
      "Comment": "屏幕高度",
      "IsNullable": true
    },
    {
      "Name": "network_type",
      "MapType": "System.String",
      "Comment": "网络类型",
      "IsNullable": true,
      "StringLength": 20
    },
    {
      "Name": "created_time",
      "MapType": "System.DateTime",
      "Comment": "创建时间",
      "IsNullable": false,
      "ServerTime": "Utc"
    }
  ]
}
```

### 🎯 字段使用建议

1. **必填字段**：`id`、`account_id`、`server_id`、`created_time` 建议设为必填
2. **字符串长度**：根据实际数据长度合理设置 `StringLength`
3. **索引优化**：为常用查询字段（如 `account_id`、`role_id`、`server_id`）添加索引
4. **时间字段**：统一使用 UTC 时间，设置 `ServerTime` 为 `"Utc"`
5. **JSON数据**：对于复杂数据（如 `cost` 字段），使用JSON格式存储，设置足够的字符串长度

### 📊 数据类型映射表

| .NET类型 | PostgreSQL类型 | 说明 | 最大值/长度 |
|----------|----------------|------|-------------|
| `System.Int64` | `bigint` | 64位整数 | -9,223,372,036,854,775,808 到 9,223,372,036,854,775,807 |
| `System.Int32` | `integer` | 32位整数 | -2,147,483,648 到 2,147,483,647 |
| `System.String` | `varchar(n)` | 可变长字符串 | 根据 `StringLength` 设置 |
| `System.DateTime` | `timestamp` | 时间戳 | 精确到微秒 |
| `System.Boolean` | `boolean` | 布尔值 | true/false |
| `System.Decimal` | `numeric` | 高精度数值 | 根据 `Precision` 和 `Scale` 设置 |

### 标准字段

每个事件表都包含以下标准字段：

```json
{
  "Name": "id",
  "MapType": "System.Int64",
  "Comment": "主ID"
},
{
  "Name": "account_id",
  "MapType": "System.String",
  "Comment": "账号ID"
},
{
  "Name": "role_id",
  "MapType": "System.String",
  "Comment": "角色ID"
},
{
  "Name": "server_id",
  "MapType": "System.String",
  "Comment": "服务器ID"
}
```

### 自定义表结构

可以通过修改 `json/TableDescriptor.json` 文件来添加新的表或字段：

```json
{
  "Name": "custom_event",
  "Comment": "自定义事件",
  "Columns": [
    {
      "Name": "id",
      "MapType": "System.Int64",
      "Comment": "主ID"
    },
    {
      "Name": "event_data",
      "MapType": "System.String",
      "Comment": "事件数据"
    }
  ]
}
```

## 🐳 Docker 部署

### Docker Compose 部署 (推荐)

1. **准备配置文件**

```bash
cp docker-env.example .env
```

2. **编辑环境变量**

```env
# 数据库配置
POSTGRES_DB=loki_logs
POSTGRES_USER=postgres
POSTGRES_PASSWORD=your_secure_password

# 应用配置
BATCH_SIZE=100
FLUSH_INTERVAL_SECONDS=30
MAX_QUEUE_SIZE=10000
```

3. **启动服务**

```bash
docker-compose up -d
```

4. **查看日志**

```bash
docker-compose logs -f loki-push
```

### 单独 Docker 部署

```bash
# 构建镜像
docker build -t gameframex-loki-push .

# 运行容器
docker run -d \
  --name loki-push-service \
  -p 5000:5000 \
  -e DATABASE_CONNECTION_STRING="Host=your-db-host;Port=5432;Database=loki_logs;Username=postgres;Password=your_password" \
  -e BATCH_SIZE=100 \
  -e FLUSH_INTERVAL_SECONDS=30 \
  gameframex-loki-push
```

## 🔌 API 接口

### 推送日志数据

**POST** `/loki/api/v1/push`

接收 Grafana Loki 推送的日志数据。

**请求体示例：**

```json
{
  "streams": [
    {
      "stream": {
        "job": "game-server",
        "instance": "server-01",
        "level": "info"
      },
      "values": [
        [
          "1640995200000000000",
          "{\"event\":\"user_login\",\"user_id\":\"12345\",\"server_id\":\"s1\"}"
        ],
        [
          "1640995201000000000",
          "{\"event\":\"user_logout\",\"user_id\":\"12345\",\"server_id\":\"s1\"}"
        ]
      ]
    }
  ]
}
```

**响应示例：**

```json
{
  "message": "success",
  "entries": 2
}
```

### 健康检查

**GET** `/loki/api/v1/health`

检查服务健康状态。

**响应示例：**

```json
{
  "status": "healthy",
  "timestamp": "2024-01-01T12:00:00Z",
  "service": "GameFrameX.Grafana.LokiPush"
}
```

### 服务信息 (仅开发环境)

**GET** `/loki/api/v1/info`

获取服务详细信息。

## 📈 性能优化

### 批处理配置

根据您的数据量和性能需求调整批处理参数：

- **高吞吐量场景**：增大 `batch-size` 和 `max-queue`，减少 `flush-interval`
- **低延迟场景**：减小 `batch-size` 和 `flush-interval`
- **内存受限场景**：减小 `max-queue` 大小

### 数据库优化

1. **索引优化**：为常用查询字段添加索引
2. **分区表**：对大表按时间进行分区
3. **连接池**：调整数据库连接池大小

### 监控指标

建议监控以下指标：

- 队列长度
- 批处理延迟
- 数据库写入性能
- 内存使用情况

## 🔧 开发指南

### 项目结构

```
GameFrameX.Grafana.LokiPush/
├── Controllers/           # API 控制器
├── Models/               # 数据模型
├── Services/             # 业务服务
├── json/                 # 配置文件
│   ├── TableDescriptor.json
│   └── Template.json
├── Program.cs            # 程序入口
└── appsettings.json      # 应用配置
```

### 添加新的事件类型

1. 在 `json/TableDescriptor.json` 中添加新的表定义
2. 重启服务，表结构会自动同步
3. 配置 Grafana Loki 推送相应的日志数据

### 自定义数据处理

可以通过继承 `IDatabaseService` 接口来实现自定义的数据处理逻辑：

```csharp
public class CustomDatabaseService : IDatabaseService
{
    // 实现自定义逻辑
}
```

## 🤝 贡献指南

1. Fork 项目
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启 Pull Request

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情。

## 🆘 支持

如果您遇到问题或有疑问，请：

1. 查看 [Issues](https://github.com/GameFrameX/GameFrameX.Grafana.LokiPush/issues)
2. 创建新的 Issue
3. 查看 [Wiki](https://github.com/GameFrameX/GameFrameX.Grafana.LokiPush/wiki) 文档

## 📚 相关文档

- [命令行使用示例](command-line-examples.md)
- [配置合并说明](configuration-merge.md)
- [GitHub Actions 设置](github-actions-setup.md)
- [FreeSql 文档](https://freesql.net/)
- [Grafana Loki 文档](https://grafana.com/docs/loki/)
