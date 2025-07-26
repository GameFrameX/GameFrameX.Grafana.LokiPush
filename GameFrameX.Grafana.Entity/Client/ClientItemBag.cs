// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using FreeSql.DataAnnotations;

namespace GameFrameX.Grafana.Entity.Client;

/// <summary>
/// 道具背包
/// </summary>
/// <remarks>记录玩家道具背包的相关信息，包含道具的详细列表数据</remarks>
[Table(Name = "client_item_bag")]
public class ClientItemBag : BaseUserClientData
{
    /// <summary>
    /// 道具列表
    /// </summary>
    /// <value>获取或设置道具背包中的道具列表信息</value>
    /// <remarks>包含背包中所有道具的详细信息，如道具ID、数量、属性等</remarks>
    [Column(StringLength = 4096)]
    public string ItemList { get; set; }
}