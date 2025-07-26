// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using FreeSql.DataAnnotations;

namespace GameFrameX.Grafana.Entity.Client;

/// <summary>
/// PVE战斗开始
/// </summary>
/// <remarks>记录PVE战斗的基础日志信息，包括战斗标识、类型、关卡和参与英雄等核心数据</remarks>
[Table(Name = "client_pve_battle_log")]
public class ClientPveBattleLog : BaseUserClientData
{
    /// <summary>
    /// 战斗唯一id
    /// </summary>
    /// <value>获取或设置战斗的唯一标识符</value>
    /// <remarks>用于唯一标识一次战斗实例，确保战斗记录的唯一性</remarks>
    [Column(StringLength = 255, IsNullable = true)]
    public string BattleUid { get; set; }

    /// <summary>
    /// 战斗类型
    /// </summary>
    /// <value>获取或设置战斗的类型分类</value>
    /// <remarks>用于区分不同类型的PVE战斗，如普通副本、精英副本、挑战关卡等</remarks>
    [Column(StringLength = 255)]
    public string BattleType { get; set; }

    /// <summary>
    /// 关卡id
    /// </summary>
    /// <value>获取或设置关卡的配置标识符</value>
    /// <remarks>用于标识具体的关卡配置信息，关联关卡的难度、奖励等属性</remarks>
    public long MapId { get; set; }

    /// <summary>
    /// 所带英雄阵容
    /// </summary>
    /// <value>获取或设置参与战斗的英雄阵容信息</value>
    /// <remarks>包含参与战斗的所有英雄的详细配置信息，如英雄ID、等级、装备等</remarks>
    [Column(StringLength = 4096)]
    public string HeroList { get; set; }
}