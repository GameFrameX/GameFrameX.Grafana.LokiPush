using GameFrameX.Grafana.LokiPush.Models;
using GameFrameX.Foundation.Json;
using FreeSql.Extensions.ZeroEntity;
using Yitter.IdGenerator;

namespace GameFrameX.Grafana.LokiPush.Services;

/// <summary>
/// 数据库服务实现类，负责处理日志数据的存储操作
/// </summary>
public class DatabaseService : IDatabaseService
{
    private readonly IFreeSql _freeSql;
    private readonly ILogger<DatabaseService> _logger;
    private readonly LokiZeroDbContextOptions _lokiZeroDbContextOptions;

    /// <summary>
    /// 初始化数据库服务实例
    /// </summary>
    /// <param name="freeSql">FreeSql实例，用于数据库操作</param>
    /// <param name="logger">日志记录器实例</param>
    /// <param name="lokiZeroDbContextOptions">零实体数据库上下文，用于动态表操作</param>
    public DatabaseService(IFreeSql freeSql, ILogger<DatabaseService> logger, LokiZeroDbContextOptions lokiZeroDbContextOptions)
    {
        _freeSql = freeSql;
        _logger = logger;
        _lokiZeroDbContextOptions = lokiZeroDbContextOptions;
    }

    /// <summary>
    /// 异步初始化数据库，创建必要的表结构
    /// </summary>
    /// <returns>表示异步操作的任务</returns>
    /// <exception cref="Exception">当数据库初始化失败时抛出异常</exception>
    public async Task InitializeDatabaseAsync()
    {
        try
        {
            // 自动创建表结构
            _freeSql.CodeFirst.SyncStructure<LokiLogEntry>();

            // 创建默认表的哈希字段部分唯一索引（仅对非空值）
            try
            {
                // 先删除旧索引（如果存在）
                await _freeSql.Ado.ExecuteNonQueryAsync(
                    "DROP INDEX IF EXISTS idx_loki_log_entry_hash");
                // 创建部分唯一索引
                await _freeSql.Ado.ExecuteNonQueryAsync(
                    "CREATE UNIQUE INDEX idx_loki_log_entry_hash ON loki_log_entry (hash) WHERE hash IS NOT NULL AND hash != ''");
                _logger.LogInformation("默认表哈希唯一索引创建成功");
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "创建默认表哈希索引时出现警告（可能已存在）");
            }

            // 方案C：为每个动态表自动添加 hash 字段（如果不存在）并创建唯一索引
            foreach (var kvp in _lokiZeroDbContextOptions.Options)
            {
                var tableName = kvp.Key;
                try
                {
                    // 使用 SQL 查询检查 hash 列是否存在
                    var checkSql = $@"SELECT COUNT(*) FROM information_schema.columns
                                    WHERE table_name = '{tableName.ToLower()}' AND column_name = 'hash'";
                    var exists = Convert.ToInt32(await _freeSql.Ado.ExecuteScalarAsync(checkSql));

                    _logger.LogInformation("表 {TableName} hash 字段存在: {Exists}", tableName, exists > 0);

                    if (exists == 0)
                    {
                        // 添加 hash 字段（允许为空，避免影响现有数据）
                        await _freeSql.Ado.ExecuteNonQueryAsync(
                            $"ALTER TABLE {tableName} ADD COLUMN IF NOT EXISTS hash VARCHAR(64)");
                        _logger.LogInformation("为表 {TableName} 自动添加 hash 字段", tableName);
                    }
                    else
                    {
                        _logger.LogDebug("表 {TableName} 已存在 hash 字段", tableName);
                    }

                    // 创建部分唯一索引（仅对非空值）
                    var indexName = $"idx_{tableName}_hash";
                    // 先删除旧索引（如果存在）
                    await _freeSql.Ado.ExecuteNonQueryAsync($"DROP INDEX IF EXISTS {indexName}");
                    // 创建部分唯一索引
                    await _freeSql.Ado.ExecuteNonQueryAsync(
                        $"CREATE UNIQUE INDEX {indexName} ON {tableName} (hash) WHERE hash IS NOT NULL AND hash != ''");
                    _logger.LogInformation("表 {TableName} 哈希唯一索引创建成功", tableName);
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(ex, "处理表 {TableName} 时出现警告（可能已存在）", tableName);
                }
            }

            _logger.LogInformation("数据库表结构初始化完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "数据库初始化失败");
            throw;
        }
    }

    /// <summary>
    /// 批量插入日志数据，根据事件名称自动选择对应的表结构
    /// </summary>
    /// <param name="logs">待插入的日志条目列表</param>
    /// <returns>表示异步操作的任务，返回插入操作是否成功</returns>
    /// <remarks>
    /// 该方法会解析每个日志条目中的事件数据，根据事件名称自动选择对应的表结构进行插入。
    /// 如果找不到对应的表结构，则会将数据插入到默认的LokiLogEntry表中。
    /// 使用哈希字段进行去重，依赖数据库唯一索引保证原子性。
    /// </remarks>
    public Task<bool> BatchInsertLogsAsync(List<PendingLogEntry> logs)
    {
        if (!logs.Any())
        {
            return Task.FromResult(true);
        }

        var duplicateCount = 0;

        try
        {
            foreach (var pendingLogEntry in logs)
            {
                var eventDataModel = JsonHelper.Deserialize<EventDataModel>(pendingLogEntry.Content);
                var eventData = JsonHelper.Deserialize<Dictionary<string, object>>(eventDataModel.EventData);

                try
                {
                    // 使用Controller层已生成的哈希
                    var hash = pendingLogEntry.Hash;

                    // 添加系统字段
                    eventData["id"] = YitIdHelper.NextId();
                    eventData["hash"] = hash;
                    eventData["created_time"] = DateTime.UtcNow;
                    eventData["event_utc_time"] = DateTimeOffset.FromUnixTimeMilliseconds(pendingLogEntry.TimestampNs / 1_000_000).UtcDateTime;
                    eventData["event_local_time"] = DateTime.Now;
                    eventData["event_time"] = DateTimeOffset.FromUnixTimeMilliseconds(pendingLogEntry.TimestampNs / 1_000_000).UtcDateTime;
                    var isInstall = _lokiZeroDbContextOptions.GetDbContext(eventDataModel.EventName, out var zeroDbContext);
                    if (isInstall)
                    {
                        // 根据事件名称选择对应的表结构进行插入

                        var success = InsertToSpecificTable(eventDataModel.EventName, zeroDbContext, eventData, ref duplicateCount);

                        if (!success)
                        {
                            _logger.LogWarning("未找到事件 {EventName} 对应的表结构，将使用默认表结构", eventDataModel.EventName);
                            InsertToDefaultTable(pendingLogEntry, eventDataModel, ref duplicateCount);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("未找到事件 {EventName} 对应的表结构，将使用默认表结构", eventDataModel.EventName);
                        InsertToDefaultTable(pendingLogEntry, eventDataModel, ref duplicateCount);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "插入事件 {EventName} 到对应表失败，尝试插入到默认表", eventDataModel.EventName);
                    InsertToDefaultTable(pendingLogEntry, eventDataModel, ref duplicateCount);
                }
            }

            if (duplicateCount > 0)
            {
                _logger.LogInformation("批次处理完成，跳过 {DuplicateCount} 条重复记录", duplicateCount);
            }

            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量插入日志失败，记录数: {Count}", logs.Count);
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// 根据事件名称将数据插入到对应的表结构中
    /// </summary>
    /// <param name="tableName">表名</param>
    /// <param name="zeroDbContext">ZeroDbContext实例</param>
    /// <param name="eventData">事件数据字典</param>
    /// <param name="duplicateCount">重复计数引用</param>
    /// <returns>如果成功插入到对应表则返回true，否则返回false</returns>
    private bool InsertToSpecificTable(string tableName, ZeroDbContext zeroDbContext, Dictionary<string, object> eventData, ref int duplicateCount)
    {
        try
        {
            var hash = eventData.TryGetValue("hash", out var hashValue) ? hashValue?.ToString() : string.Empty;
            if (string.IsNullOrEmpty(hash))
            {
                _logger.LogWarning("缺少哈希值，跳过插入");
                return false;
            }

            _logger.LogDebug("尝试插入记录到表 {TableName}，Hash: {Hash}", tableName, hash);

            // 使用标准 INSERT 语句（不使用 ON CONFLICT），由唯一索引保证去重
            // 构建列名和参数
            var columns = string.Join(", ", eventData.Keys);
            var values = string.Join(", ", eventData.Values.Select(v => $"'{v?.ToString()?.Replace("'", "''")}'"));

            var sql = $"INSERT INTO {tableName} ({columns}) VALUES ({values})";
            _freeSql.Ado.ExecuteNonQuery(sql);

            _logger.LogInformation("成功插入事件到表 {TableName}，Hash: {Hash}", tableName, hash);
            return true;
        }
        catch (Exception ex)
        {
            // 检查是否是唯一索引冲突（重复数据）
            var errorMessage = ex.Message.ToLowerInvariant();
            var hash = eventData.TryGetValue("hash", out var hashValue) ? hashValue?.ToString() : string.Empty;

            if (errorMessage.Contains("duplicate") || errorMessage.Contains("unique") || errorMessage.Contains("constraint") || errorMessage.Contains("23505"))
            {
                _logger.LogInformation("记录已存在（表 {TableName}，Hash: {Hash}），跳过重复插入", tableName, hash);
                Interlocked.Increment(ref duplicateCount);
                return true; // 重复也算"成功"处理
            }

            _logger.LogError(ex, "插入事件到表失败，Hash: {Hash}", hash);
            return false;
        }
    }

    /// <summary>
    /// 将数据插入到默认的LokiLogEntry表中
    /// </summary>
    /// <param name="pendingLogEntry">待处理的日志条目</param>
    /// <param name="eventDataModel">事件数据模型</param>
    /// <param name="duplicateCount">重复计数引用</param>
    private void InsertToDefaultTable(PendingLogEntry pendingLogEntry, EventDataModel eventDataModel, ref int duplicateCount)
    {
        // 使用Controller层已生成的哈希
        var hash = pendingLogEntry.Hash;

        var logEntry = new LokiLogEntry
        {
            TimestampNs = pendingLogEntry.TimestampNs,
            Name = eventDataModel.EventName,
            Content = eventDataModel.EventData,
            Labels = JsonHelper.Serialize(pendingLogEntry.Labels),
            LogTime = DateTimeOffset.FromUnixTimeMilliseconds(pendingLogEntry.TimestampNs / 1_000_000).UtcDateTime,
            CreatedTime = DateTime.UtcNow,
            Hash = hash
        };

        try
        {
            _freeSql.Insert(logEntry).ExecuteAffrows();
            _logger.LogDebug("成功插入事件 {EventName} 到默认表 LokiLogEntry, Hash: {Hash}", eventDataModel.EventName, hash);
        }
        catch (Exception ex)
        {
            // 检查是否是唯一索引冲突（重复数据）
            var errorMessage = ex.Message.ToLowerInvariant();
            if (errorMessage.Contains("duplicate") || errorMessage.Contains("unique") || errorMessage.Contains("constraint"))
            {
                _logger.LogDebug("记录已存在（Hash: {Hash}），跳过重复插入", hash);
                Interlocked.Increment(ref duplicateCount);
            }
            else
            {
                throw;
            }
        }
    }
}