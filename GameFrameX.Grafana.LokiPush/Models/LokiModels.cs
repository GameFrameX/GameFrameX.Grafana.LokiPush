using System.Text.Json.Serialization;

namespace GameFrameX.Grafana.LokiPush.Models;

/// <summary>
/// Loki推送请求模型
/// </summary>
public class LokiPushRequest
{
    [JsonPropertyName("streams")]
    public List<LokiStream> Streams { get; set; } = new();
}

/// <summary>
/// Loki流数据
/// </summary>
public class LokiStream
{
    [JsonPropertyName("stream")]
    public Dictionary<string, string> Stream { get; set; } = new();

    [JsonPropertyName("values")]
    public List<List<string>> Values { get; set; } = new();
}

/// <summary>
/// 待处理的日志条目（内存中的临时存储）
/// </summary>
public class PendingLogEntry
{
    public long TimestampNs { get; set; }
    public string Content { get; set; } = string.Empty;
    public Dictionary<string, string> Labels { get; set; } = new();
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}