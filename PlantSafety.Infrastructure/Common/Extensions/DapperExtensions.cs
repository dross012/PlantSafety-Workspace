// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common
//  Layer:          Infrastructure
//  File:           DapperExtensions.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper helper extensions for StrongId and UtcTimestamp type handling.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Common.Extensions;

/// <summary>
/// Dapper helper extensions. Registers custom type handlers for domain value types
/// so Dapper maps UNIQUEIDENTIFIER columns to StrongId types automatically.
/// </summary>
public static class DapperExtensions
{
    private static bool _handlersRegistered;
    private static readonly object _lock = new();

    /// <summary>
    /// Registers all PlantSafety custom Dapper type handlers.
    /// Call once at application startup from DI registration.
    /// </summary>
    public static void RegisterTypeHandlers()
    {
        if (_handlersRegistered) return;
        lock (_lock)
        {
            if (_handlersRegistered) return;
            SqlMapper.AddTypeHandler(new GuidTypeHandler());
            _handlersRegistered = true;
        }
    }

    /// <summary>Maps UNIQUEIDENTIFIER columns to <see cref="Guid"/>.</summary>
    private sealed class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
    {
        public override void SetValue(IDbDataParameter parameter, Guid value)
        {
            parameter.Value = value;
            parameter.DbType = DbType.Guid;
        }

        public override Guid Parse(object value)
            => value switch
            {
                Guid g   => g,
                string s => Guid.Parse(s),
                _        => throw new InvalidOperationException($"Cannot parse '{value?.GetType().Name}' as Guid.")
            };
    }
}
