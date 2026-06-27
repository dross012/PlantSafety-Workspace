// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common
//  Layer:          Infrastructure
//  File:           SqlConstants.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    SQL schema names, system procedure names, and canonical column names.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Common.Constants;

/// <summary>SQL schema names and canonical column/procedure constants.</summary>
public static class SqlConstants
{
    /// <summary>System schema hosting outbox, processed-events, and migration tables.</summary>
    public const string SystemSchema = "System";

    /// <summary>Outbox messages table name (fully qualified).</summary>
    public const string OutboxTable = "[System].[OutboxMessages]";

    /// <summary>Processed events idempotency table (fully qualified).</summary>
    public const string ProcessedEventsTable = "[System].[ProcessedEvents]";

    /// <summary>Schema migrations tracking table (fully qualified).</summary>
    public const string SchemaMigrationsTable = "[System].[SchemaMigrations]";

    /// <summary>Canonical stored procedure parameter: tenant identity.</summary>
    public const string ParamTenantId = "@TenantId";

    /// <summary>Canonical stored procedure parameter: correlation identity.</summary>
    public const string ParamCorrelationId = "@CorrelationId";

    /// <summary>Canonical stored procedure parameter: actor identity.</summary>
    public const string ParamActorId = "@ActorId";

    /// <summary>Canonical stored procedure parameter: current UTC time.</summary>
    public const string ParamNowUtc = "@NowUtc";
}
