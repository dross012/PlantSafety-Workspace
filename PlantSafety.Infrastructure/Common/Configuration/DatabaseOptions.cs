// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common
//  Layer:          Infrastructure
//  File:           DatabaseOptions.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Strongly typed options for SQL connection configuration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Common.Configuration;

/// <summary>Strongly typed SQL Server connection options, bound from configuration.</summary>
public sealed class DatabaseOptions
{
    /// <summary>Configuration section key.</summary>
    public const string SectionName = "Database";

    /// <summary>Primary SQL Server connection string.</summary>
    public string ConnectionString { get; init; } = string.Empty;

    /// <summary>Read-replica connection string for query-side operations.</summary>
    public string ReadConnectionString { get; init; } = string.Empty;

    /// <summary>Default command timeout in seconds.</summary>
    public int CommandTimeoutSeconds { get; init; } = 30;

    /// <summary>Maximum pool size.</summary>
    public int MaxPoolSize { get; init; } = 100;
}
