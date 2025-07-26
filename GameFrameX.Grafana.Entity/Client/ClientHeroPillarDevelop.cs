// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using FreeSql.DataAnnotations;

namespace GameFrameX.Grafana.Entity.Client;

/// <summary>
/// 英雄柱养成
/// </summary>
[Table(Name = "client_hero_pillar_develop")]
public class ClientHeroPillarDevelop : BaseUserClientData
{
    /// <summary>
    /// 英雄柱配置id
    /// </summary>
    public long PillarConfigId { get; set; }

    /// <summary>
    /// 养成后阶级
    /// </summary>
    public long Grade { get; set; }

    /// <summary>
    /// 养成后等级
    /// </summary>
    public long Level { get; set; }

    /// <summary>
    /// 本次变化值
    /// </summary>
    public long ChangeValue { get; set; }

    /// <summary>
    /// 本次变化消耗资源
    /// </summary>
    [Column(StringLength = 4096)]
    public string CostThing { get; set; }

    /// <summary>
    /// 变化原因
    /// </summary>
    [Column(StringLength = 255)]
    public string ChangeReason { get; set; }
}