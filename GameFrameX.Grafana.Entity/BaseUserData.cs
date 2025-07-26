using FreeSql.DataAnnotations;

namespace GameFrameX.Grafana.Entity;

/// <summary>
/// 用户数据基类
/// </summary>
public class BaseUserData
{
    /// <summary>
    /// 主键ID
    /// </summary>
    [Column(IsPrimary = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 账号ID
    /// </summary>
    [Column(StringLength = 255)]
    public string AccountId { get; set; } = string.Empty;

    /// <summary>
    /// 角色ID（唯一标识）
    /// </summary>
    [Column(StringLength = 255)]
    public string RoleId { get; set; } = string.Empty;

    /// <summary>
    /// 服务器id
    /// </summary>
    [Column(StringLength = 255)]
    public string ServerId { get; set; } = string.Empty;

    /// <summary>
    /// 国家地区
    /// </summary>
    [Column(StringLength = 255)]
    public string Country { get; set; } = string.Empty;


    /// <summary>
    /// 渠道
    /// </summary>
    [Column(StringLength = 255)]
    public string Channel { get; set; } = string.Empty;

    /// <summary>
    /// 首次出现时的渠道id
    /// </summary>
    [Column(StringLength = 255)]
    public string OrigChannel { get; set; } = string.Empty;

    /// <summary>
    /// 开服时间
    /// </summary>
    [Column(IsNullable = true)]
    public DateTime ServerOpenTime { get; set; }

    /// <summary>
    /// 环境
    /// </summary>
    [Column(StringLength = 255)]
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// 应用（ios/安卓）
    /// </summary>
    [Column(StringLength = 255)]
    public string System { get; set; } = string.Empty;

    /// <summary>
    /// 设备类型（phone/tablet）
    /// </summary>
    [Column(StringLength = 255)]
    public string DeviceType { get; set; } = string.Empty;

    /// <summary>
    /// 行为发生时ip地址
    /// </summary>
    [Column(StringLength = 255)]
    public string Ip { get; set; } = string.Empty;

    /// <summary>
    /// </summary>
    [Column(StringLength = 512)]
    public string DeviceModel { get; set; }

    /// <summary>
    /// </summary>
    [Column(StringLength = 512)]
    public string Os { get; set; }

    /// <summary>
    /// </summary>
    [Column(StringLength = 128)]
    public string AppVersion { get; set; }

    /// <summary>
    /// </summary>
    [Column(StringLength = 128)]
    public string UnityVersion { get; set; }

    /// <summary>
    /// </summary>
    [Column(StringLength = 256)]
    public string Platform { get; set; }

    /// <summary>
    /// 记录创建时间
    /// </summary>
    [Column(IsNullable = true)]
    public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
}