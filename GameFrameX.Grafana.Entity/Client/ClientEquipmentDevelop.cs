// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using FreeSql.DataAnnotations;

namespace GameFrameX.Grafana.Entity.Client;

/// <summary>
/// 装备养成
/// </summary>
[Table(Name = "client_equipment_develop")]
public class ClientEquipmentDevelop : BaseUserClientData
{
    /// <summary>
    /// 变化原因
    /// </summary>
    [Column(StringLength = 255)]
    public string ChangeReason { get; set; }

    /// <summary>
    /// 本次变化消耗资源
    /// </summary>
    [Column(StringLength = 4096)]
    public string CostThing { get; set; }

    /// <summary>
    /// 养成后装备名称
    /// </summary>
    [Column(StringLength = 4096)]
    public string EquipmentNameAfter { get; set; }

    /// <summary>
    /// 装备名称
    /// </summary>
    [Column(StringLength = 255)]
    public string EquipmentName { get; set; }

    /// <summary>
    /// 养成前装备唯一id
    /// </summary>
    /// <value>获取或设置养成前装备的唯一标识符</value>
    /// <remarks>用于标识养成操作前的装备实例</remarks>
    public long EquipmentUid { get; set; }

    /// <summary>
    /// 养成后装备唯一id
    /// </summary>
    /// <value>获取或设置养成后装备的唯一标识符</value>
    /// <remarks>用于标识养成操作后的装备实例</remarks>
    public long EquipmentUidAfter { get; set; }

    /// <summary>
    /// 装备配置id
    /// </summary>
    /// <value>获取或设置养成前装备的配置标识符</value>
    /// <remarks>用于标识装备的配置信息</remarks>
    public long EquipmentConfigId { get; set; }

    /// <summary>
    /// 养成后装备配置id
    /// </summary>
    /// <value>获取或设置养成后装备的配置标识符</value>
    /// <remarks>用于标识养成后装备的配置信息</remarks>
    public long EquipmentConfigIdAfter { get; set; }

    /// <summary>
    /// 养成前品质
    /// </summary>
    /// <value>获取或设置养成前装备的品质等级</value>
    /// <remarks>表示装备在养成前的品质水平</remarks>
    public long Quality { get; set; }

    /// <summary>
    /// 养成后品质
    /// </summary>
    /// <value>获取或设置养成后装备的品质等级</value>
    /// <remarks>表示装备在养成后的品质水平</remarks>
    public long QualityAfter { get; set; }

    /// <summary>
    /// 养成前星级
    /// </summary>
    /// <value>获取或设置养成前装备的星级等级</value>
    /// <remarks>表示装备在养成前的星级水平</remarks>
    public long Star { get; set; }

    /// <summary>
    /// 养成后星级
    /// </summary>
    /// <value>获取或设置养成后装备的星级等级</value>
    /// <remarks>表示装备在养成后的星级水平</remarks>
    public long StarAfter { get; set; }

    /// <summary>
    /// 本次变化值
    /// </summary>
    /// <value>获取或设置本次养成操作的变化数值</value>
    /// <remarks>表示本次养成操作产生的数值变化</remarks>
    public long ChangeValue { get; set; }
}