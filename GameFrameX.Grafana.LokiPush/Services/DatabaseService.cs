using FreeSql;
using GameFrameX.Grafana.LokiPush.Models;
using System.Text.Json;
using GameFrameX.Foundation.Json;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Text;
using FreeSql.DataAnnotations;
using GameFrameX.Foundation.Extensions;
using GameFrameX.Grafana.Entity;

namespace GameFrameX.Grafana.LokiPush.Services;

public class DatabaseService : IDatabaseService
{
    private readonly IFreeSql _freeSql;
    private readonly ILogger<DatabaseService> _logger;
    private static readonly Dictionary<string, PropertyInfo> _baseUserDataPropertyMap;
    private static readonly Dictionary<PropertyInfo, string> _propertyToColumnMap;

    static DatabaseService()
    {
        _baseUserDataPropertyMap = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
        _propertyToColumnMap = new Dictionary<PropertyInfo, string>();

        var properties = typeof(BaseUserData).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                             .Where(p => p.CanWrite && p.PropertyType == typeof(string) ||
                                                         p.PropertyType == typeof(DateTime) ||
                                                         p.PropertyType == typeof(long) ||
                                                         p.PropertyType == typeof(int));

        foreach (var property in properties)
        {
            // 将属性名转换为下划线命名
            var columnName = property.Name.ConvertToSnakeCase();
            _baseUserDataPropertyMap[columnName] = property;
            _propertyToColumnMap[property] = columnName;

            // 同时支持原始属性名
            _baseUserDataPropertyMap[property.Name] = property;
        }
    }

    public DatabaseService(IFreeSql freeSql, ILogger<DatabaseService> logger)
    {
        _freeSql = freeSql;
        _logger = logger;
    }

    public Task InitializeDatabaseAsync()
    {
        try
        {
            // 自动创建表结构

            _freeSql.CodeFirst.SyncStructure(EventMapManager.GetEventNames().ToArray());
            _freeSql.CodeFirst.SyncStructure<LokiLogEntry>();
            _logger.LogInformation("数据库表结构初始化完成");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "数据库初始化失败");
            throw;
        }
    }

    public async Task<bool> BatchInsertLogsAsync(List<PendingLogEntry> logs)
    {
        if (!logs.Any())
        {
            return true;
        }

        try
        {
            var entities = new List<LokiLogEntry>();
            var eventEntities = new Dictionary<string, List<Dictionary<string, object>>>();

            foreach (var pendingLogEntry in logs)
            {
                var eventDataModel = JsonHelper.Deserialize<EventDataModel>(pendingLogEntry.Content);
                var eventType = EventMapManager.GetEventType(eventDataModel.EventName);
                var eventData = JsonHelper.Deserialize<Dictionary<string, object>>(eventDataModel.EventData);

                var entity = new Dictionary<string, object>();

                // 使用反射映射Labels到BaseUserData属性
                var baseUserData = MapLabelsToBaseUserDataUsingReflection(pendingLogEntry.Labels);

                // 将BaseUserData的属性添加到entity中
                AddBaseUserDataToEntityUsingReflection(entity, baseUserData);

                // 添加Labels中未映射到BaseUserData的其他字段
                foreach (var field in pendingLogEntry.Labels)
                {
                    if (!IsBaseUserDataProperty(field.Key))
                    {
                        entity[field.Key] = field.Value;
                    }
                }

                foreach (var field in eventData)
                {
                    entity[field.Key] = field.Value;
                }

                if (eventType == null)
                {
                    entities.Add(new LokiLogEntry
                    {
                        TimestampNs = pendingLogEntry.TimestampNs,
                        Name = eventDataModel.EventName,
                        Content = eventDataModel.EventData,
                        Labels = JsonSerializer.Serialize(pendingLogEntry.Labels),
                        LogTime = DateTimeOffset.FromUnixTimeMilliseconds(pendingLogEntry.TimestampNs / 1_000_000).DateTime,
                        CreatedTime = DateTime.UtcNow
                    });
                }
                else
                {
                    foreach (var field in entity)
                    {
                        // 如果字段是BaseUserData的属性，则跳过
                        if (IsBaseUserDataProperty(field.Key))
                        {
                            continue;
                        }

                        // 如果字段在事件类型中不存在，则跳过
                        if (!eventType.GetProperties().Any(p => p.Name.Equals(field.Key, StringComparison.OrdinalIgnoreCase)))
                        {
                            // _logger.LogWarning("事件 '{EventName}' 中缺少字段 '{FieldName}'，将其忽略", eventDataModel.EventName, field.Key);
                            entity.Remove(field.Key);
                        }
                    }

                    if (eventEntities.TryGetValue(eventDataModel.EventName, out var list))
                    {
                        list.Add(entity);
                    }
                    else
                    {
                        eventEntities.Add(eventDataModel.EventName, [entity,]);
                    }
                }
            }

            foreach (var eventEntity in eventEntities)
            {
                await _freeSql.InsertDict(eventEntity.Value).AsTable(eventEntity.Key).ExecuteAffrowsAsync();
            }

            // 批量插入不能入库的事件数据
            if (entities.Any())
            {
                var result = await _freeSql.Insert(entities).ExecuteAffrowsAsync();

                _logger.LogInformation("批量插入 {Count} 条日志记录，影响行数: {AffectedRows}", logs.Count, result);
                return result > 0;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量插入日志失败，记录数: {Count}", logs.Count);
            return false;
        }
    }

    /// <summary>
    /// 使用反射将Labels映射到BaseUserData属性
    /// </summary>
    private BaseUserData MapLabelsToBaseUserDataUsingReflection(Dictionary<string, string> labels)
    {
        var baseUserData = new BaseUserData
        {
            CreatedTime = DateTime.UtcNow
        };

        foreach (var label in labels)
        {
            if (_baseUserDataPropertyMap.TryGetValue(label.Key, out var property))
            {
                try
                {
                    var convertedValue = ConvertValue(label.Value, property.PropertyType);
                    if (convertedValue != null)
                    {
                        property.SetValue(baseUserData, convertedValue);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "无法将值 '{Value}' 转换为属性 '{PropertyName}' 的类型 '{PropertyType}'",
                                       label.Value, property.Name, property.PropertyType.Name);
                }
            }
        }

        return baseUserData;
    }

    /// <summary>
    /// 使用反射将BaseUserData的属性添加到实体字典中
    /// </summary>
    private void AddBaseUserDataToEntityUsingReflection(Dictionary<string, object> entity, BaseUserData baseUserData)
    {
        var properties = typeof(BaseUserData).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                             .Where(p => p.CanRead);

        foreach (var property in properties)
        {
            var value = property.GetValue(baseUserData);
            if (value != null && !IsDefaultValue(value, property.PropertyType))
            {
                var columnName = _propertyToColumnMap.TryGetValue(property, out var name) ? name : property.Name.ConvertToSnakeCase();
                entity[columnName] = value;
            }
        }
    }

    /// <summary>
    /// 检查字段是否为BaseUserData的属性
    /// </summary>
    private bool IsBaseUserDataProperty(string fieldName)
    {
        return _baseUserDataPropertyMap.ContainsKey(fieldName);
    }

    /// <summary>
    /// 将值转换为指定类型
    /// </summary>
    private object? ConvertValue(string value, Type targetType)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        try
        {
            if (targetType == typeof(string))
                return value;

            if (targetType == typeof(DateTime))
            {
                if (DateTime.TryParse(value, out var dateTime))
                    return dateTime;
                return null;
            }

            if (targetType == typeof(long))
            {
                if (long.TryParse(value, out var longValue))
                    return longValue;
                return null;
            }

            if (targetType == typeof(int))
            {
                if (int.TryParse(value, out var intValue))
                    return intValue;
                return null;
            }

            // 使用Convert.ChangeType作为后备方案
            return Convert.ChangeType(value, targetType);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 检查值是否为默认值
    /// </summary>
    private bool IsDefaultValue(object value, Type type)
    {
        if (type == typeof(string))
            return string.IsNullOrEmpty((string)value);

        if (type == typeof(DateTime))
            return (DateTime)value == default(DateTime);

        if (type == typeof(long))
            return (long)value == 0;

        if (type == typeof(int))
            return (int)value == 0;

        return value.Equals(Activator.CreateInstance(type));
    }
}