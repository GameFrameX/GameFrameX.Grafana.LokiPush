using GameFrameX.Foundation.Options.Attributes;

namespace GameFrameX.Grafana.LokiPush.Models;

/// <summary>
/// 命令行参数选项
/// </summary>
public sealed class LauncherOptions
{
    [Option(nameof(ConnectionString), Required = true, Description = "数据库连接字符串")]
    public string ConnectionString { get; set; }

    [Option(nameof(BatchSize), Required = false, DefaultValue = 100, Description = "批处理大小")]
    public int BatchSize { get; set; }

    [Option(nameof(FlushIntervalSeconds), Required = false, DefaultValue = 5, Description = "刷新间隔（秒）")]
    public int FlushIntervalSeconds { get; set; }

    [Option(nameof(MaxQueueSize), Required = false, DefaultValue = 10000, Description = "最大队列大小")]
    public int MaxQueueSize { get; set; }

    [Option(nameof(Verbose), Required = false, DefaultValue = false, Description = "启用详细日志输出")]
    public bool Verbose { get; set; }
}