// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common
//  Layer:          Infrastructure
//  File:           StructuredLogger.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Canonical structured logging helper ensuring all required SIEM fields are present.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Common.Logging;

/// <summary>
/// Canonical structured logging helper. Ensures every log entry carries the
/// required observability fields: CorrelationId, ActorId, Module, Operation.
/// </summary>
public static class StructuredLogger
{
    /// <summary>Logs a structured entry with all canonical SIEM fields.</summary>
    public static void Log(
        ILogger logger,
        LogLevel level,
        string message,
        string module,
        string operation,
        string correlationId,
        string? actorId = null,
        Exception? exception = null)
    {
        using var scope = logger.BeginScope(new Dictionary<string, object?>
        {
            ["Module"]        = module,
            ["Operation"]     = operation,
            ["CorrelationId"] = correlationId,
            ["ActorId"]       = actorId ?? "system",
        });

        if (exception is null)
            logger.Log(level, message);
        else
            logger.Log(level, exception, message);
    }

    /// <summary>Logs at <see cref="LogLevel.Information"/>.</summary>
    public static void Info(ILogger logger, string message, string module, string operation, string correlationId, string? actorId = null)
        => Log(logger, LogLevel.Information, message, module, operation, correlationId, actorId);

    /// <summary>Logs at <see cref="LogLevel.Warning"/>.</summary>
    public static void Warn(ILogger logger, string message, string module, string operation, string correlationId, string? actorId = null)
        => Log(logger, LogLevel.Warning, message, module, operation, correlationId, actorId);

    /// <summary>Logs at <see cref="LogLevel.Error"/>.</summary>
    public static void Error(ILogger logger, string message, string module, string operation, string correlationId, Exception exception, string? actorId = null)
        => Log(logger, LogLevel.Error, message, module, operation, correlationId, actorId, exception);
}
