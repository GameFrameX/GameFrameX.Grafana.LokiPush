using FreeSql.DataAnnotations;

namespace GameFrameX.Grafana.Entity.Client;

/// <summary>
/// 控件（按钮）点击事件表
/// (必填)玩家点击客户端控件/按钮链接时记录
/// </summary>
[Table(Name = "client_obj_click")]
public class ClientObjClick : BaseUserClientData
{
    /// <summary>
    /// 操作事件名称
    /// </summary>
    [Column(StringLength = 512)]
    public string ObjName { get; set; } = string.Empty;
}