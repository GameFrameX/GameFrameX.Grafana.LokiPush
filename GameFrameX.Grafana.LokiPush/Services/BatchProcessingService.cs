using GameFrameX.Grafana.LokiPush.Models;
using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace GameFrameX.Grafana.LokiPush.Services;

public class BatchProcessingService : IBatchProcessingService, IHostedService, IDisposable
{
    private readonly IDatabaseService _databaseService;
    private readonly ILogger<BatchProcessingService> _logger;
    private readonly IConfiguration _configuration;
    private readonly ConcurrentQueue<PendingLogEntry> _logQueue;
    private Timer? _timer;
    private readonly SemaphoreSlim _processingLock;
    
    // 配置参数
    private readonly int _batchSize;
    private readonly int _batchIntervalSeconds;
    private readonly int _maxQueueSize;

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

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("批量处理服务启动");
        
        _timer = new Timer(async _ => await ProcessBatchAsync(), 
            null, 
            TimeSpan.FromSeconds(_batchIntervalSeconds), 
            TimeSpan.FromSeconds(_batchIntervalSeconds));
        
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("批量处理服务停止中...");
        
        _timer?.Change(Timeout.Infinite, 0);
        
        // 处理剩余的日志
        await ProcessBatchAsync();
        
        _logger.LogInformation("批量处理服务已停止");
    }

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

    public void Dispose()
    {
        _timer?.Dispose();
        _processingLock?.Dispose();
    }
}