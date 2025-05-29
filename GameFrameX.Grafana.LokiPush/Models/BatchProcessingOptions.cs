namespace GameFrameX.Grafana.LokiPush.Models;

/// <summary>
/// 批处理配置选项
/// </summary>
public class BatchProcessingOptions
{
    /// <summary>
    /// 批处理大小
    /// </summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// 刷新间隔（秒）
    /// </summary>
    public int FlushIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// 最大队列大小
    /// </summary>
    public int MaxQueueSize { get; set; } = 10000;
}