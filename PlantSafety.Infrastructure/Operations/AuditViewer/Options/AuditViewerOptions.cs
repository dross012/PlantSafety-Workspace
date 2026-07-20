// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Operations.AuditViewer
//  Layer:          Infrastructure
//  File:           AuditViewerOptions.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strongly typed options for the Audit Viewer. Bound from configuration
//                  section "Operations:AuditViewer". [PlantSafety.AuditDB].dbo.AuditLog is
//                  a shared, cross-database, server-level table (Phase8-Structural\
//                  83-synonyms-audit-error.sql) -- not a tenant database -- so this
//                  repository connects directly to PlantSafety.AuditDB rather than going
//                  through ISqlConnectionFactory (tenant-scoped primary/read-replica
//                  only), matching the Operations.Backup/SchemaGuard pattern.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Operations.AuditViewer.Options;

/// <summary>Strongly typed configuration for the Audit Viewer.</summary>
public sealed class AuditViewerOptions
{
    /// <summary>Configuration section key.</summary>
    public const string SectionName = "Operations:AuditViewer";

    /// <summary>Connection string targeting [PlantSafety.AuditDB], where dbo.AuditLog lives.</summary>
    public string AuditDbConnectionString { get; init; } =
        "Server=(local)\\SQLEXPRESS;Database=PlantSafety.AuditDB;Integrated Security=true;TrustServerCertificate=true;";

    /// <summary>Command timeout (seconds) for AuditLog search queries.</summary>
    public int SearchCommandTimeoutSeconds { get; init; } = 60;

    /// <summary>Maximum page size a caller may request, to bound query and export cost.</summary>
    public int MaxPageSize { get; init; } = 5000;
}
