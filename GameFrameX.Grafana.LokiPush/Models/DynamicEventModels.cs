using System.Text.Json.Serialization;

namespace GameFrameX.Grafana.LokiPush.Models;

/// <summary>
/// 事件数据解析模型
/// </summary>
public class EventDataModel
{
    [JsonPropertyName("event_name")] public string EventName { get; set; } = string.Empty;
    [JsonPropertyName("event_data")] public string EventData { get; set; } = string.Empty;
}