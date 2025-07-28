using GameFrameX.Grafana.LokiPush.Models;
using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace GameFrameX.Grafana.LokiPush.Services;

/// <summary>
/// 批处理服务实现类，负责管理日志的批量处理和定时刷新
/// </summary>
/// <remarks>
/// 该服务实现了IBatchProcessingService接口，同时也是一个后台托管服务。
/// 它维护一个内存队列来缓存待处理的日志，并定期或在达到批处理大小时将日志批量写入数据库。
/// </remarks>
public class BatchProcessingService : IBatchProcessingService, IHostedService, IDisposable
{
    private readonly IDatabaseService _databaseService;
    private readonly ILogger<BatchProcessingService> _logger;
    private readonly IConfiguration _configuration;
    private readonly ConcurrentQueue<PendingLogEntry> _logQueue;
    private Timer _timer;
    private readonly SemaphoreSlim _processingLock;
    
    // 配置参数
    private readonly int _batchSize;
    private readonly int _batchIntervalSeconds;
    private readonly int _maxQueueSize;

    /// <summary>
    /// 初始化批处理服务实例
    /// </summary>
    /// <param name="databaseService">数据库服务实例，用于执行批量插入操作</param>
    /// <param name="logger">日志记录器实例</param>
    /// <param name="configuration">配置服务实例</param>
    /// <param name="options">批处理配置选项，包含批处理大小、刷新间隔等参数</param>
    public BatchProcessingService(
        IDatabaseService databaseService,
        ILogger<BatchProcessingService> logger,
        IConfiguration configuration,
        IOptions<BatchProcessingOptions> options)
    {
        _databaseService = databaseService;
        _logger = logger;
        _configuration = configuration;
        _logQueue = new ConcurrentQueue<PendingLogEntry>();
        _processingLock = new SemaphoreSlim(1, 1);
        
        // 从 IOptions 读取配置参数
        var batchOptions = options.Value;
        _batchSize = batchOptions.BatchSize;
        _batchIntervalSeconds = batchOptions.FlushIntervalSeconds;
        _maxQueueSize = batchOptions.MaxQueueSize;
        
        _logger.LogInformation("批量处理服务初始化 - 批次大小: {BatchSize}, 间隔: {Interval}秒, 最大队列: {MaxQueue}", 
            _batchSize, _batchIntervalSeconds, _maxQueueSize);
    }

    /// <summary>
    /// 添加日志到批处理队列
    /// </summary>
    /// <param name="logs">待处理的日志条目列表</param>
    /// <remarks>
    /// 该方法会检查队列容量限制，如果队列即将满载会强制触发一次批处理。
    /// 当队列达到批处理大小时，会立即触发异步批处理操作。
    /// </remarks>
    public void AddLogs(List<PendingLogEntry> logs)
    {
        if (!logs.Any())
        {
            return;
        }

        var currentQueueSize = _logQueue.Count;
        
        // 检查队列大小限制
        if (currentQueueSize + logs.Count > _maxQueueSize)
        {
            _logger.LogWarning("队列已满，当前大小: {CurrentSize}, 尝试添加: {AddCount}, 最大限制: {MaxSize}", 
                currentQueueSize, logs.Count, _maxQueueSize);
            
            // 强制处理一次以释放空间
            _ = Task.Run(async () => await ProcessBatchAsync());
            return;
        }

        foreach (var log in logs)
        {
            _logQueue.Enqueue(log);
        }

        _logger.LogDebug("添加 {Count} 条日志到队列，当前队列大小: {QueueSize}", logs.Count, _logQueue.Count);
        
        // 如果队列达到批次大小，立即处理
        if (_logQueue.Count >= _batchSize)
        {
            _ = Task.Run(async () => await ProcessBatchAsync());
        }
    }

    /// <summary>
    /// 异步启动批处理服务
    /// </summary>
    /// <param name="cancellationToken">取消令牌，用于取消启动操作</param>
    /// <returns>表示异步操作的任务</returns>
    /// <remarks>
    /// 启动后会创建定时器，按照配置的时间间隔定期处理队列中的日志数据。
    /// </remarks>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("批量处理服务启动");
        
        _timer = new Timer(async _ => await ProcessBatchAsync(), 
            null, 
            TimeSpan.FromSeconds(_batchIntervalSeconds), 
            TimeSpan.FromSeconds(_batchIntervalSeconds));
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// 异步停止批处理服务
    /// </summary>
    /// <param name="cancellationToken">取消令牌，用于取消停止操作</param>
    /// <returns>表示异步操作的任务</returns>
    /// <remarks>
    /// 停止时会先停止定时器，然后处理队列中剩余的所有日志数据，确保数据不丢失。
    /// </remarks>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("批量处理服务停止中...");
        
        _timer?.Change(Timeout.Infinite, 0);
        
        // 处理剩余的日志
        await ProcessBatchAsync();
        
        _logger.LogInformation("批量处理服务已停止");
    }

    /// <summary>
    /// 异步处理批次数据
    /// </summary>
    /// <returns>表示异步操作的任务</returns>
    /// <remarks>
    /// 该方法会从队列中取出指定数量的日志进行批量处理。
    /// 使用信号量确保同一时间只有一个批处理操作在执行，避免并发问题。
    /// </remarks>
    private async Task ProcessBatchAsync()
    {
        if (_logQueue.IsEmpty)
        {
            return;
        }

        if (!await _processingLock.WaitAsync(100))
        {
            _logger.LogDebug("批量处理正在进行中，跳过此次处理");
            return;
        }

        try
        {
            var batch = new List<PendingLogEntry>();

            // 从队列中取出指定数量的日志
            while (batch.Count < _batchSize && _logQueue.TryDequeue(out var log))
            {
                batch.Add(log);
            }

            if (batch.Any())
            {
                _logger.LogInformation("开始处理批次，数量: {Count}", batch.Count);
                
                var success = await _databaseService.BatchInsertLogsAsync(batch);
                
                if (success)
                {
                    _logger.LogInformation("批次处理成功，已处理 {Count} 条日志，剩余队列: {Remaining}", batch.Count, _logQueue.Count);
                }
                else
                {
                    _logger.LogError("批次处理失败，丢失 {Count} 条日志", batch.Count);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量处理过程中发生异常");
        }
        finally
        {
            _processingLock.Release();
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    /// <remarks>
    /// 释放定时器和信号量等非托管资源。
    /// </remarks>
    public void Dispose()
    {
        _timer?.Dispose();
        _processingLock?.Dispose();
    }
}