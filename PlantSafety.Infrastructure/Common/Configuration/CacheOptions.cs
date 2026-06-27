// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common
//  Layer:          Infrastructure
//  File:           CacheOptions.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Strongly typed distributed cache configuration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Common.Configuration;

/// <summary>Strongly typed distributed cache (Redis) options.</summary>
public sealed class CacheOptions
{
    /// <summary>Configuration section key.</summary>
    public const string SectionName = "Cache";

    /// <summary>Redis connection string.</summary>
    public string ConnectionString { get; init; } = string.Empty;

    /// <summary>Default cache TTL in minutes.</summary>
    public int DefaultTtlMinutes { get; init; } = 10;

    /// <summary>Cache key prefix to namespace all keys for this service.</summary>
    public string KeyPrefix { get; init; } = "ps:";
}
