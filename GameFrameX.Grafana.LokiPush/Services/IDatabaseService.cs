// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using GameFrameX.Grafana.LokiPush.Models;

namespace GameFrameX.Grafana.LokiPush.Services;

/// <summary>
/// 数据库服务接口，定义了日志数据存储的核心操作
/// </summary>
public interface IDatabaseService
{
    /// <summary>
    /// 批量插入日志数据到数据库
    /// </summary>
    /// <param name="logs">待插入的日志条目列表，包含时间戳、内容和标签信息</param>
    /// <returns>表示异步操作的任务，返回值指示批量插入操作是否成功</returns>
    /// <remarks>
    /// 该方法会根据日志中的事件名称自动选择对应的表结构进行插入。
    /// 如果找不到匹配的表结构，则会使用默认的LokiLogEntry表进行存储。
    /// </remarks>
    Task<bool> BatchInsertLogsAsync(List<PendingLogEntry> logs);
    
    /// <summary>
    /// 异步初始化数据库，创建必要的表结构
    /// </summary>
    /// <returns>表示异步操作的任务</returns>
    /// <exception cref="Exception">当数据库初始化失败时抛出异常</exception>
    /// <remarks>
    /// 该方法会确保数据库中存在必要的表结构，包括默认的LokiLogEntry表。
    /// 通常在应用程序启动时调用一次。
    /// </remarks>
    Task InitializeDatabaseAsync();
}