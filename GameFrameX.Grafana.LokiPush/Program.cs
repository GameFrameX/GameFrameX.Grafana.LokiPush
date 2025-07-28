using CommandLine;
using FreeSql;
using FreeSql.Extensions.ZeroEntity;
using FreeSql.Internal;
using GameFrameX.Foundation.Json;
using GameFrameX.Grafana.LokiPush;
using GameFrameX.Grafana.LokiPush.Models;
using GameFrameX.Grafana.LokiPush.Services;
using Newtonsoft.Json;
using Yitter.IdGenerator;

// 解析命令行参数
LauncherOptions? cmdOptions = null;
var parseResult = Parser.Default.ParseArguments<LauncherOptions>(args);

parseResult
    .WithParsed(opts => cmdOptions = opts)
    .WithNotParsed(errors =>
    {
        // 检查是否是帮助请求或版本请求
        var isHelpOrVersion = errors.Any(e => e.Tag == CommandLine.ErrorType.HelpRequestedError || e.Tag == CommandLine.ErrorType.VersionRequestedError);

        if (!isHelpOrVersion)
        {
            Console.WriteLine("命令行参数解析失败:");
            foreach (var error in errors)
            {
                Console.WriteLine($"  {error}");
            }

            Environment.Exit(1);
        }
        else
        {
            // 帮助或版本请求，正常退出
            Environment.Exit(0);
        }
    });

// 合并命令行参数和环境变量，创建最终的配置选项
var finalOptions = MergeOptionsWithEnvironment(cmdOptions);

// 合并命令行参数和环境变量的方法
static LauncherOptions MergeOptionsWithEnvironment(LauncherOptions? cmdOptions)
{
    var options = cmdOptions ?? new LauncherOptions();

    // 如果命令行参数未设置，则使用环境变量
    if (string.IsNullOrEmpty(options.ConnectionString))
    {
        options.ConnectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
    }

    if (int.TryParse(Environment.GetEnvironmentVariable("BATCH_SIZE"), out var envBatchSize))
    {
        options.BatchSize = envBatchSize;
    }
    else
    {
        options.BatchSize = 100; // 保持默认值
    }

    if (int.TryParse(Environment.GetEnvironmentVariable("FLUSH_INTERVAL_SECONDS"), out var envFlushInterval))
    {
        options.FlushIntervalSeconds = envFlushInterval;
    }
    else
    {
        options.FlushIntervalSeconds = 30; // 保持默认值
    }

    if (int.TryParse(Environment.GetEnvironmentVariable("MAX_QUEUE_SIZE"), out var envMaxQueueSize))
    {
        options.MaxQueueSize = envMaxQueueSize;
    }
    else
    {
        options.MaxQueueSize = 10000; // 保持默认值
    }

    if (int.TryParse(Environment.GetEnvironmentVariable("HTTP_PORT"), out var envPort))
    {
        options.Port = envPort;
    }
    else
    {
        options.Port = 5000; // 保持默认值
    }

    var envEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    if (!string.IsNullOrEmpty(envEnvironment))
    {
        options.Environment = envEnvironment;
    }
    else
    {
        options.Environment = "Production"; // 默认值
    }

    if (bool.TryParse(Environment.GetEnvironmentVariable("VERBOSE_LOGGING"), out var envVerbose))
    {
        options.Verbose = envVerbose;
    }
    else
    {
        options.Verbose = false; // 默认值
    }

    return options;
}

// 检查 TableDescriptor.json 文件
DirectoryInfo currentDirectory = new DirectoryInfo("./json");
if (!currentDirectory.Exists)
{
    // 如果目录不存在，创建它
    currentDirectory.Create();
}

var fileInfos = currentDirectory.GetFiles();
if (fileInfos.Length == 0)
{
    throw new FileNotFoundException("未找到任何 JSON 文件。请确保 json 目录下存在 TableDescriptor.json 文件。");
}

YitIdHelper.SetIdGenerator(new IdGeneratorOptions(10));
var builder = WebApplication.CreateBuilder(args);

// 根据合并后的参数设置环境
builder.Environment.EnvironmentName = finalOptions.Environment;

// 配置 Kestrel 端口
builder.WebHost.ConfigureKestrel(serverOptions => { serverOptions.ListenAnyIP(finalOptions.Port); });

// 配置日志级别
if (finalOptions.Verbose)
{
    builder.Logging.SetMinimumLevel(LogLevel.Debug);
}

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new() { Title = "GameFrameX Grafana Loki Push API", Version = "v1" }); });

// 配置FreeSql - 支持命令行参数、环境变量和配置文件
var connectionString = finalOptions.ConnectionString
                       ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("数据库连接字符串未配置。请通过以下方式之一设置:\n" +
                                        "1. 命令行参数: --connection \"连接字符串\"\n" +
                                        "2. 环境变量: DATABASE_CONNECTION_STRING\n" +
                                        "3. 配置文件: appsettings.json中的ConnectionStrings:DefaultConnection");
}

Console.WriteLine($"启动参数 {JsonHelper.SerializeFormat(finalOptions)}");
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

    var zeroDbContext = new ZeroDbContext(freeSql, [tableDescriptor,]);
    lokiZeroDbContextOptions.Options[tableDescriptor.Name] = zeroDbContext;
    zeroDbContext.SyncStructure([tableDescriptor,]);
}


builder.Services.AddSingleton<IFreeSql>(freeSql);
builder.Services.AddSingleton<LokiZeroDbContextOptions>(lokiZeroDbContextOptions);

// 注册批处理服务 - 使用合并后的配置选项
builder.Services.Configure<BatchProcessingOptions>(batchOptions =>
{
    batchOptions.BatchSize = finalOptions.BatchSize;
    batchOptions.FlushIntervalSeconds = finalOptions.FlushIntervalSeconds;
    batchOptions.MaxQueueSize = finalOptions.MaxQueueSize;
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

app.UseHttpsRedirection();
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
app.Logger.LogInformation("监听端口: {Port}", finalOptions.Port);
app.Logger.LogInformation("批处理配置: BatchSize={BatchSize}, FlushInterval={FlushInterval}s, MaxQueue={MaxQueue}",
                          finalOptions.BatchSize,
                          finalOptions.FlushIntervalSeconds,
                          finalOptions.MaxQueueSize);
app.Logger.LogInformation("API文档地址: {SwaggerUrl}", app.Environment.IsDevelopment() ? $"http://localhost:{finalOptions.Port}/swagger" : "/swagger");
app.Logger.LogInformation("Loki推送端点: /loki/api/v1/push");
app.Logger.LogInformation("健康检查端点: /health");
app.Logger.LogInformation("服务信息端点: /info");

app.Run();