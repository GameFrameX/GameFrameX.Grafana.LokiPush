// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using FreeSql.DataAnnotations;

namespace GameFrameX.Grafana.Entity;

/// <summary>
/// 用户数据基类
/// </summary>
public abstract class BaseUserClientData : BaseUserData
{
    /// <summary>
    /// 角色名称
    /// </summary>
    [Column(StringLength = 255)]
    public string NickName { get; set; } = string.Empty;

    /// <summary>
    /// 子渠道
    /// </summary>
    [Column(StringLength = 255)]
    public string SubChannel { get; set; } = string.Empty;

    /// <summary>
    /// 服务器渠道
    /// </summary>
    [Column(StringLength = 255)]
    public string ServerChannel { get; set; } = string.Empty;

    /// <summary>
    /// 支付方式
    /// </summary>
    [Column(StringLength = 255)]
    public string Payment { get; set; } = string.Empty;

    /// <summary>
    /// 设备ID
    /// </summary>
    [Column(StringLength = 255)]
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// 激活时间：设备首次登录
    /// </summary>
    [Column(IsNullable = true)]
    public DateTime ActiveTime { get; set; }

    /// <summary>
    /// 账号历史累充金额
    /// </summary>
    [Column(IsNullable = true)]
    public long AccountHistoryMoney { get; set; }

    /// <summary>
    /// 角色历史累充金额
    /// </summary>
    [Column(IsNullable = true)]
    public long RoleHistoryMoney { get; set; }

    /// <summary>
    /// 设备历史累充金额
    /// </summary>
    [Column(IsNullable = true)]
    public long DeviceHistoryMoney { get; set; }

    /// <summary>
    /// 处理器类型
    /// </summary>
    [Column(StringLength = 255)]
    public string ProcessorType { get; set; } = string.Empty;

    /// <summary>
    /// 处理器数量
    /// </summary>
    [Column(IsNullable = true)]
    public long ProcessorCount { get; set; }

    /// <summary>
    /// 处理器频率（单位：MHz）
    /// </summary>
    [Column(IsNullable = true)]
    public long ProcessorFrequency { get; set; }

    /// <summary>
    /// 系统内存大小（单位：MB）
    /// </summary>
    [Column(IsNullable = true)]
    public long SystemMemorySize { get; set; }

    /// <summary>
    /// 显卡设备名称
    /// </summary>
    [Column(IsNullable = true)]
    public long GraphicsDeviceName { get; set; }

    /// <summary>
    /// GPU类型（显卡型号）
    /// </summary>
    [Column(StringLength = 255, IsNullable = true)]
    public string GraphicsDeviceType { get; set; }

    /// <summary>
    /// GPU内存大小（单位：MB）
    /// </summary>
    [Column(IsNullable = true)]
    public long GraphicsMemorySize { get; set; }

    /// <summary>
    /// GPU版本号（显卡驱动版本）
    /// </summary>
    [Column(StringLength = 255, IsNullable = true)]
    public string GraphicsDeviceVersion { get; set; }

    /// <summary>
    /// GPU着色器级别（shader level）
    /// </summary>
    [Column(IsNullable = true)]
    public long GraphicsShaderLevel { get; set; }

    /// <summary>
    /// 屏幕宽度（像素）
    /// </summary>
    [Column(IsNullable = true)]
    public long ScreenWidth { get; set; }

    /// <summary>
    /// 屏幕高度（像素）
    /// </summary>
    [Column(IsNullable = true)]
    public long ScreenHeight { get; set; }

    /// <summary>
    /// 屏幕分辨率（每英寸像素点数）
    /// </summary>
    [Column(IsNullable = true)]
    public float ScreenDpi { get; set; }

    /// <summary>
    /// 屏幕刷新率（Hz）
    /// </summary>
    [Column(IsNullable = true)]
    public long ScreenRefreshRate { get; set; }

    /// <summary>
    /// 系统语言（操作系统使用的语言）
    /// </summary>
    [Column(StringLength = 255, IsNullable = true)]
    public string SystemLanguage { get; set; }

    /// <summary>
    /// 当前语言（应用程序使用的语言文化）
    /// </summary>
    [Column(StringLength = 255, IsNullable = true)]
    public string CurrentCulture { get; set; }

    /// <summary>
    /// 网络类型（如：WiFi、4G、5G等）
    /// </summary>
    [Column(StringLength = 255, IsNullable = true)]
    public string NetworkType { get; set; }

    /// <summary>
    /// 最后登出时间
    /// </summary>
    [Column(IsNullable = true)]
    public DateTime LatestOnlineTime { get; set; }
}