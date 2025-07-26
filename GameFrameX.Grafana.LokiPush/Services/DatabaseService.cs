using GameFrameX.Grafana.LokiPush.Models;
using GameFrameX.Foundation.Json;
using FreeSql.Extensions.ZeroEntity;
using Yitter.IdGenerator;

namespace GameFrameX.Grafana.LokiPush.Services;

public class DatabaseService : IDatabaseService
{
    private readonly IFreeSql _freeSql;
    private readonly ILogger<DatabaseService> _logger;
    private readonly ZeroDbContext _ctx;

    public DatabaseService(IFreeSql freeSql, ILogger<DatabaseService> logger, ZeroDbContext ctx)
    {
        _freeSql = freeSql;
        _logger = logger;
        _ctx = ctx;
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

    public Task<bool> BatchInsertLogsAsync(List<PendingLogEntry> logs)
    {
        if (!logs.Any())
        {
            return Task.FromResult(true);
        }

        try
        {
            foreach (var pendingLogEntry in logs)
            {
                var eventDataModel = JsonHelper.Deserialize<EventDataModel>(pendingLogEntry.Content);
                var eventData = JsonHelper.Deserialize<Dictionary<string, object>>(eventDataModel.EventData);
                try
                {
                    eventData["id"] = YitIdHelper.NextId();
                    eventData["created_time"] = DateTime.UtcNow;
                    _ctx.Insert(eventData);
                }
                catch (Exception ex)
                {
                    var logEntry = new LokiLogEntry
                    {
                        TimestampNs = pendingLogEntry.TimestampNs,
                        Name = eventDataModel.EventName,
                        Content = eventDataModel.EventData,
                        Labels = JsonHelper.Serialize(pendingLogEntry.Labels),
                        LogTime = DateTimeOffset.FromUnixTimeMilliseconds(pendingLogEntry.TimestampNs / 1_000_000).DateTime,
                        CreatedTime = DateTime.UtcNow
                    };
                    _freeSql.Insert(logEntry);
                    _logger.LogError(ex, "批量插入日志失败，记录数: {Count}", logs.Count);
                }
            }

            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量插入日志失败，记录数: {Count}", logs.Count);
            return Task.FromResult(false);
        }
    }
}