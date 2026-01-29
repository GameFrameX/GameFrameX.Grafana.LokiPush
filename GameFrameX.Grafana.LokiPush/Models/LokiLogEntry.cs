// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using FreeSql.DataAnnotations;

namespace GameFrameX.Grafana.LokiPush.Models;

/// <summary>
/// 日志条目数据库实体
/// </summary>
[Table]
public class LokiLogEntry
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public long Id { get; set; }

    /// <summary>
    /// 时间戳（纳秒）
    /// </summary>
    public long TimestampNs { get; set; }

    public string Name { get; set; } = string.Empty;
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
    public DateTime CreatedTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 日志时间（从纳秒时间戳转换）
    /// </summary>
    public DateTime LogTime { get; set; }

    /// <summary>
    /// 内容哈希值（用于去重，基于 TimestampNs + Content + Labels 生成）
    /// </summary>
    [Column(StringLength = 64)]
    public string Hash { get; set; } = string.Empty;
}