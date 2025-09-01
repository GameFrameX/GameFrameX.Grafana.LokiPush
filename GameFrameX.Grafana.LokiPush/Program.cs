using FreeSql;
using FreeSql.Extensions.ZeroEntity;
using FreeSql.Internal;
using GameFrameX.Foundation.Options;
using GameFrameX.Grafana.LokiPush;
using GameFrameX.Grafana.LokiPush.Models;
using GameFrameX.Grafana.LokiPush.Services;
using Newtonsoft.Json;
using Yitter.IdGenerator;

// 解析命令行参数
LauncherOptions launcherOptions = OptionsBuilder.CreateWithDebug<LauncherOptions>(args);

// 检查 TableDescriptor.json 文件
DirectoryInfo currentDirectory = new DirectoryInfo("./json");
if (!currentDirectory.Exists)
{
    // 如果目录不存在，创建它
    currentDirectory.Create();
}

var fileInfos = currentDirectory.GetFiles("*.json", SearchOption.AllDirectories);
if (fileInfos.Length == 0)
{
    throw new FileNotFoundException("未找到任何 JSON 文件。请确保 json 目录下存在 TableDescriptor.json 文件。");
}

YitIdHelper.SetIdGenerator(new IdGeneratorOptions(10));
var builder = WebApplication.CreateBuilder(args);

// 配置日志级别
if (launcherOptions.Verbose)
{
    builder.Logging.SetMinimumLevel(LogLevel.Debug);
}

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new() { Title = "GameFrameX Grafana Loki Push API", Version = "v1" }); });

// 配置FreeSql - 支持命令行参数、环境变量和配置文件
var connectionString = launcherOptions.ConnectionString
                       ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("数据库连接字符串未配置。请通过以下方式之一设置:\n" +
                                        "1. 命令行参数: --connection \"连接字符串\"\n" +
                                        "2. 环境变量: DATABASE_CONNECTION_STRING\n" +
                                        "3. 配置文件: appsettings.json中的ConnectionStrings:DefaultConnection");
}

var freeSqlBuilder = new FreeSqlBuilder()
                     .UseConnectionString(DataType.PostgreSQL, connectionString)
                     // .UseAutoSyncStructure(true) // 自动同步实体结构到数据库
                     .UseNameConvert(NameConvertType.PascalCaseToUnderscoreWithLower) // 驼峰转下划线
                     .UseNoneCommandParameter(true);

var freeSql = freeSqlBuilder.Build();
LokiZeroDbContextOptions lokiZeroDbContextOptions = new LokiZeroDbContextOptions();
foreach (var fileInfo in fileInfos)
{
    var tableDescriptorJson = File.ReadAllText(fileInfo.FullName);
    var tableDescriptor = JsonConvert.DeserializeObject<TableDescriptor>(tableDescriptorJson);
    if (tableDescriptor == null || tableDescriptor.Columns.Count == 0)
    {
        Console.WriteLine($"跳过无效的表描述文件: {fileInfo.Name}");
        continue;
    }

    try
    {
        var zeroDbContext = new ZeroDbContext(freeSql, [tableDescriptor]);
        lokiZeroDbContextOptions.Options[tableDescriptor.Name] = zeroDbContext;
        zeroDbContext.SyncStructure([tableDescriptor,]);
    }
    catch (Exception e)
    {
        Console.WriteLine("JSON File: " + fileInfo.FullName);
        Console.WriteLine(e);
        throw;
    }
}


builder.Services.AddSingleton<IFreeSql>(freeSql);
builder.Services.AddSingleton<LokiZeroDbContextOptions>(lokiZeroDbContextOptions);

// 注册批处理服务 - 使用合并后的配置选项
builder.Services.Configure<BatchProcessingOptions>(batchOptions =>
{
    batchOptions.BatchSize = launcherOptions.BatchSize;
    batchOptions.FlushIntervalSeconds = launcherOptions.FlushIntervalSeconds;
    batchOptions.MaxQueueSize = launcherOptions.MaxQueueSize;
});

// 注册服务
builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
builder.Services.AddSingleton<IBatchProcessingService, BatchProcessingService>();
builder.Services.AddHostedService<BatchProcessingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GameFrameX Grafana Loki Push API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseRouting();
app.MapControllers();

// 初始化数据库（可选）
try
{
    using (var scope = app.Services.CreateScope())
    {
        var databaseService = scope.ServiceProvider.GetRequiredService<IDatabaseService>();
        await databaseService.InitializeDatabaseAsync();
        app.Logger.LogInformation("数据库连接成功");
    }
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "数据库连接失败，服务将继续运行但无法存储数据。请检查PostgreSQL连接配置。");
    app.Logger.LogInformation("提示：请确保PostgreSQL服务正在运行，并更新appsettings.json中的连接字符串");
}

app.Logger.LogInformation("GameFrameX Grafana Loki Push 服务启动完成");
app.Logger.LogInformation("运行环境: {Environment}", app.Environment.EnvironmentName);
app.Logger.LogInformation("批处理配置: BatchSize={BatchSize}, FlushInterval={FlushInterval}s, MaxQueue={MaxQueue}",
                          launcherOptions.BatchSize,
                          launcherOptions.FlushIntervalSeconds,
                          launcherOptions.MaxQueueSize);
app.Logger.LogInformation("API文档地址: {SwaggerUrl}", app.Environment.IsDevelopment() ? $"http://localhost:8080/swagger" : "/swagger");
app.Logger.LogInformation("Loki推送端点: /loki/api/v1/push");
app.Logger.LogInformation("健康检查端点: /health");
app.Logger.LogInformation("服务信息端点: /info");

app.Run();