using FreeSql.DataAnnotations;

namespace GameFrameX.Grafana.Entity.Server;

/// <summary>
/// 充值表
/// 玩家充值成功时记录
/// </summary>
[Table(Name = "server_pay_log")]
public class ServerPayLog : BaseUserData
{
    /// <summary>
    /// 支付货币类型
    /// </summary>
    [Column(StringLength = 50)]
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// 支付类型
    /// </summary>
    [Column(StringLength = 100)]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 商品id
    /// </summary>
    public long ProductId { get; set; }

    /// <summary>
    /// 商品名称（唯一信息）
    /// </summary>
    [Column(StringLength = 255)]
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// 付费金额
    /// </summary>
    public long Payment { get; set; }

    /// <summary>
    /// 订单号
    /// </summary>
    [Column(StringLength = 255)]
    public string OrderId { get; set; } = string.Empty;

    /// <summary>
    /// 第三方支付订单号
    /// </summary>
    [Column(StringLength = 255)]
    public string ThirdPartyOrderId { get; set; } = string.Empty;

    /// <summary>
    /// 支付平台（如：支付宝、微信、苹果等）
    /// </summary>
    [Column(StringLength = 100)]
    public string PaymentPlatform { get; set; } = string.Empty;

    /// <summary>
    /// 支付状态（成功、失败、处理中等）
    /// </summary>
    [Column(StringLength = 50)]
    public string PaymentStatus { get; set; } = "success";

    /// <summary>
    /// 支付完成时间
    /// </summary>
    public DateTime PaymentTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 事件发生时间戳（纳秒）
    /// </summary>
    public long TimestampNs { get; set; }

    /// <summary>
    /// 商品类型（月卡、礼包、道具等）
    /// </summary>
    [Column(StringLength = 100)]
    public string ProductType { get; set; } = string.Empty;

    /// <summary>
    /// 商品分类
    /// </summary>
    [Column(StringLength = 100)]
    public string ProductCategory { get; set; } = string.Empty;

    /// <summary>
    /// 折扣信息
    /// </summary>
    [Column(StringLength = 100)]
    public string DiscountInfo { get; set; } = string.Empty;

    /// <summary>
    /// 原价（未折扣前的价格）
    /// </summary>
    public long OriginalPrice { get; set; }

    /// <summary>
    /// 是否首次充值
    /// </summary>
    public bool IsFirstPay { get; set; }

    /// <summary>
    /// 累计充值次数
    /// </summary>
    public int TotalPayCount { get; set; }

    /// <summary>
    /// 累计充值金额
    /// </summary>
    public long TotalPayAmount { get; set; }

    /// <summary>
    /// 充值来源页面
    /// </summary>
    [Column(StringLength = 255)]
    public string SourcePage { get; set; } = string.Empty;

    /// <summary>
    /// 推广活动ID
    /// </summary>
    [Column(StringLength = 100)]
    public string PromotionId { get; set; } = string.Empty;

    /// <summary>
    /// 额外参数（JSON格式）
    /// </summary>
    [Column(DbType = "TEXT")]
    public string ExtraParams { get; set; } = string.Empty;

    /// <summary>
    /// 会话ID
    /// </summary>
    [Column(StringLength = 255)]
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// 用户VIP等级
    /// </summary>
    public int VipLevel { get; set; }

    /// <summary>
    /// 充值前游戏币数量
    /// </summary>
    public long GameCurrencyBefore { get; set; }

    /// <summary>
    /// 充值后游戏币数量
    /// </summary>
    public long GameCurrencyAfter { get; set; }
}