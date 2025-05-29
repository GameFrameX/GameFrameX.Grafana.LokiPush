using FreeSql.DataAnnotations;
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
/// 日志条目数据库实体
/// </summary>
[Table(Name = "loki_logs")]
public class LokiLogEntry
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public long Id { get; set; }

    /// <summary>
    /// 时间戳（纳秒）
    /// </summary>
    [Column(Name = "timestamp_ns")]
    public long TimestampNs { get; set; }

    /// <summary>
    /// 日志内容
    /// </summary>
    [Column(StringLength = -1)]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 标签（JSON格式存储）
    /// </summary>
    [Column(StringLength = -1)]
    public string Labels { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    [Column(Name = "created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 日志时间（从纳秒时间戳转换）
    /// </summary>
    [Column(Name = "log_time")]
    public DateTime LogTime { get; set; }
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