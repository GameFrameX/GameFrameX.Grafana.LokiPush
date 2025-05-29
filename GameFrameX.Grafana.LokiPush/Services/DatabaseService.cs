using FreeSql;
using GameFrameX.Grafana.LokiPush.Models;
using System.Text.Json;

namespace GameFrameX.Grafana.LokiPush.Services;

public class DatabaseService : IDatabaseService
{
    private readonly IFreeSql _freeSql;
    private readonly ILogger<DatabaseService> _logger;

    public DatabaseService(IFreeSql freeSql, ILogger<DatabaseService> logger)
    {
        _freeSql = freeSql;
        _logger = logger;
    }

    public Task InitializeDatabaseAsync()
    {
        try
        {
            // 自动创建表结构
            _freeSql.CodeFirst.SyncStructure<LokiLogEntry>();
            _logger.LogInformation("数据库表结构初始化完成");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "数据库初始化失败");
            throw;
        }
    }

    public async Task<bool> BatchInsertLogsAsync(List<PendingLogEntry> logs)
    {
        if (!logs.Any())
        {
            return true;
        }

        try
        {
            var entities = logs.Select(log => new LokiLogEntry
            {
                TimestampNs = log.TimestampNs,
                Content = log.Content,
                Labels = JsonSerializer.Serialize(log.Labels),
                LogTime = DateTimeOffset.FromUnixTimeMilliseconds(log.TimestampNs / 1_000_000).DateTime,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            var result = await _freeSql.Insert(entities).ExecuteAffrowsAsync();
            
            _logger.LogInformation("批量插入 {Count} 条日志记录，影响行数: {AffectedRows}", logs.Count, result);
            
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量插入日志失败，记录数: {Count}", logs.Count);
            return false;
        }
    }
}