using GameFrameX.Grafana.LokiPush.Models;
using GameFrameX.Grafana.LokiPush.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace GameFrameX.Grafana.LokiPush.Controllers;

[ApiController]
[Route("loki/api/v1")]
public class LokiController : ControllerBase
{
    private readonly IBatchProcessingService _batchProcessingService;
    private readonly ILogger<LokiController> _logger;

    public LokiController(IBatchProcessingService batchProcessingService, ILogger<LokiController> logger)
    {
        _batchProcessingService = batchProcessingService;
        _logger = logger;
    }

    /// <summary>
    /// 接收Loki推送的日志数据
    /// </summary>
    /// <param name="request">Loki推送请求</param>
    /// <returns></returns>
    [HttpPost("push")]
    public IActionResult Push([FromBody] LokiPushRequest request)
    {
        try
        {
            if (request?.Streams == null || !request.Streams.Any())
            {
                _logger.LogWarning("接收到空的Loki推送请求");
                return BadRequest("No streams provided");
            }

            var pendingLogs = new List<PendingLogEntry>();
            var totalEntries = 0;

            foreach (var stream in request.Streams)
            {
                if (stream.Values == null || !stream.Values.Any())
                {
                    continue;
                }

                foreach (var value in stream.Values)
                {
                    if (value.Count < 2)
                    {
                        _logger.LogWarning("日志条目格式不正确，跳过: {Value}", JsonSerializer.Serialize(value));
                        continue;
                    }

                    // Loki的值格式: [timestamp, log_line]
                    var timestampStr = value[0];
                    var logContent = value[1];

                    if (!long.TryParse(timestampStr, out var timestampNs))
                    {
                        _logger.LogWarning("无法解析时间戳: {Timestamp}", timestampStr);
                        continue;
                    }

                    var pendingLog = new PendingLogEntry
                    {
                        TimestampNs = timestampNs,
                        Content = logContent,
                        Labels = stream.Stream ?? new Dictionary<string, string>(),
                        ReceivedAt = DateTime.UtcNow
                    };

                    pendingLogs.Add(pendingLog);
                    totalEntries++;
                }
            }

            if (pendingLogs.Any())
            {
                _batchProcessingService.AddLogs(pendingLogs);
                _logger.LogDebug("接收到 {StreamCount} 个流，共 {EntryCount} 条日志条目",
                                 request.Streams.Count, totalEntries);
            }
            else
            {
                _logger.LogWarning("没有有效的日志条目可处理");
            }

            return Ok(new { message = "success", entries = totalEntries });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON解析错误");
            return BadRequest("Invalid JSON format");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理Loki推送请求时发生异常");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// 健康检查端点
    /// </summary>
    /// <returns></returns>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            service = "GameFrameX.Grafana.LokiPush"
        });
    }

    /// <summary>
    /// 获取服务信息
    /// </summary>
    /// <returns></returns>
#if DEBUG
    [HttpGet("info")]
    public IActionResult Info()
    {
        return Ok(new
        {
            service = "GameFrameX.Grafana.LokiPush",
            version = "1.0.0",
            description = "Grafana Loki日志推送接收服务",
            endpoints = new
            {
                push = "/loki/api/v1/push",
                health = "/loki/api/v1/health",
                info = "/loki/api/v1/info"
            }
        });
    }
#endif
}