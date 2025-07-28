using CommandLine;

namespace GameFrameX.Grafana.LokiPush.Models;

/// <summary>
/// 命令行参数选项
/// </summary>
public class LauncherOptions
{
    [Option("connection", Required = false, HelpText = "数据库连接字符串")]
    public string ConnectionString { get; set; }

    [Option("batch-size", Required = false, Default = 100, HelpText = "批处理大小")]
    public int BatchSize { get; set; }

    [Option("flush-interval", Required = false, Default = 5, HelpText = "刷新间隔（秒）")]
    public int FlushIntervalSeconds { get; set; }

    [Option("max-queue", Required = false, Default = 10000, HelpText = "最大队列大小")]
    public int MaxQueueSize { get; set; }

    [Option("port", Required = false, Default = 5000, HelpText = "HTTP服务端口")]
    public int Port { get; set; }

    [Option("environment", Required = false, Default = "Production", HelpText = "运行环境 (Development/Production)")]
    public string Environment { get; set; } = "Production";

    [Option("verbose", Required = false, Default = false, HelpText = "启用详细日志输出")]
    public bool Verbose { get; set; }

    [Option("help", Required = false, Default = false, HelpText = "显示帮助信息")]
    public bool ShowHelp { get; set; }
}