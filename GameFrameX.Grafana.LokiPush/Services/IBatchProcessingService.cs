// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using GameFrameX.Grafana.LokiPush.Models;

namespace GameFrameX.Grafana.LokiPush.Services;

/// <summary>
/// 批处理服务接口，定义了日志批量处理的核心操作
/// </summary>
public interface IBatchProcessingService
{
    /// <summary>
    /// 添加日志到批处理队列
    /// </summary>
    /// <param name="logs">待处理的日志条目列表</param>
    /// <remarks>
    /// 该方法会将日志添加到内存队列中，当队列达到批处理大小或超时时会自动触发批量插入操作。
    /// 如果队列已满，会强制触发一次批处理以释放空间。
    /// </remarks>
    void AddLogs(List<PendingLogEntry> logs);
    
    /// <summary>
    /// 异步启动批处理服务
    /// </summary>
    /// <param name="cancellationToken">取消令牌，用于取消启动操作</param>
    /// <returns>表示异步操作的任务</returns>
    /// <remarks>
    /// 启动后会创建定时器，定期处理队列中的日志数据。
    /// </remarks>
    Task StartAsync(CancellationToken cancellationToken);
    
    /// <summary>
    /// 异步停止批处理服务
    /// </summary>
    /// <param name="cancellationToken">取消令牌，用于取消停止操作</param>
    /// <returns>表示异步操作的任务</returns>
    /// <remarks>
    /// 停止时会处理队列中剩余的所有日志数据，确保数据不丢失。
    /// </remarks>
    Task StopAsync(CancellationToken cancellationToken);
}