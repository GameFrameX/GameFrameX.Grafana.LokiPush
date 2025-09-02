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

    /// <summary>
    /// 批量插入日志数据，根据事件名称自动选择对应的表结构
    /// </summary>
    /// <param name="logs">待插入的日志条目列表</param>
    /// <returns>表示异步操作的任务，返回插入操作是否成功</returns>
    /// <remarks>
    /// 该方法会解析每个日志条目中的事件数据，根据事件名称自动选择对应的表结构进行插入。
    /// 如果找不到对应的表结构，则会将数据插入到默认的LokiLogEntry表中。
    /// </remarks>
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
                    // 添加系统字段
                    eventData["id"] = YitIdHelper.NextId();
                    eventData["created_time"] = DateTime.UtcNow;
                    eventData["event_time"] = DateTimeOffset.FromUnixTimeMilliseconds(pendingLogEntry.TimestampNs / 1_000_000).UtcDateTime;
                    var isInstall = _lokiZeroDbContextOptions.GetDbContext(eventDataModel.EventName, out var zeroDbContext);
                    if (isInstall)
                    {
                        // 根据事件名称选择对应的表结构进行插入

                        var success = InsertToSpecificTable(zeroDbContext, eventData);

                        if (!success)
                        {
                            _logger.LogWarning("未找到事件 {EventName} 对应的表结构，将使用默认表结构", eventDataModel.EventName);
                            InsertToDefaultTable(pendingLogEntry, eventDataModel);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("未找到事件 {EventName} 对应的表结构，将使用默认表结构", eventDataModel.EventName);
                        InsertToDefaultTable(pendingLogEntry, eventDataModel);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "插入事件 {EventName} 到对应表失败，尝试插入到默认表", eventDataModel.EventName);
                    InsertToDefaultTable(pendingLogEntry, eventDataModel);
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

    /// <summary>
    /// 根据事件名称将数据插入到对应的表结构中
    /// </summary>
    /// <param name="zeroDbContext">事件名称，用于确定目标表</param>
    /// <param name="eventData">事件数据字典</param>
    /// <returns>如果成功插入到对应表则返回true，否则返回false</returns>
    private bool InsertToSpecificTable(ZeroDbContext zeroDbContext, Dictionary<string, object> eventData)
    {
        try
        {
            // 使用ZeroDbContext根据事件名称插入到对应的表
            var result = zeroDbContext.Insert(eventData);
            _logger.LogDebug("成功插入事件 {EventName} 到对应表结构", zeroDbContext);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "插入事件 {EventName} 到对应表结构失败", zeroDbContext);
            return false;
        }
    }

    /// <summary>
    /// 将数据插入到默认的LokiLogEntry表中
    /// </summary>
    /// <param name="pendingLogEntry">待处理的日志条目</param>
    /// <param name="eventDataModel">事件数据模型</param>
    private void InsertToDefaultTable(PendingLogEntry pendingLogEntry, EventDataModel eventDataModel)
    {
        var logEntry = new LokiLogEntry
        {
            TimestampNs = pendingLogEntry.TimestampNs,
            Name = eventDataModel.EventName,
            Content = eventDataModel.EventData,
            Labels = JsonHelper.Serialize(pendingLogEntry.Labels),
            LogTime = DateTimeOffset.FromUnixTimeMilliseconds(pendingLogEntry.TimestampNs / 1_000_000).UtcDateTime,
            CreatedTime = DateTime.UtcNow
        };

        _freeSql.Insert(logEntry).ExecuteAffrows();
        _logger.LogDebug("成功插入事件 {EventName} 到默认表 LokiLogEntry", eventDataModel.EventName);
    }
}