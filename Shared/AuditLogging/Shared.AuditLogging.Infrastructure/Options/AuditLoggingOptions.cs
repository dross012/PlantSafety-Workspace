// ============================================================================
//  Project:        Shared.AuditLogging
//  Module:         Infrastructure.Options
//  Layer:          Infrastructure
//  File:           AuditLoggingOptions.cs
//  Created:        2026-07-26
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strongly typed options for Shared.AuditLogging. Bound from
//                  configuration section "Shared:AuditLogging". [PlantSafety.AuditDB].
//                  dbo.AuditLog is a shared, cross-database, server-level table (see
//                  PlantSafety.Infrastructure's Persistence\Migrations\Phase8-Structural\
//                  83-synonyms-audit-error.sql) -- not a tenant database -- so this
//                  repository connects directly to PlantSafety.AuditDB rather than going
//                  through a tenant-scoped connection factory. Renamed from PlantSafety's
//                  Operations.AuditViewer.Options.AuditViewerOptions (created 2026-07-20;
//                  section was "Operations:AuditViewer") when extracted here 2026-07-26 --
//                  the section name changed because this module is no longer PlantSafety-
//                  specific; PlantSafety and Reporting both bind to the same
//                  "Shared:AuditLogging" section against the same physical database.
//                  Additive-only evolution.
// ============================================================================
namespace Shared.AuditLogging.Infrastructure.Options;

/// <summary>Strongly typed configuration for Shared.AuditLogging.</summary>
public sealed class AuditLoggingOptions
{
    /// <summary>Configuration section key.</summary>
    public const string SectionName = "Shared:AuditLogging";

    /// <summary>Connection string targeting [PlantSafety.AuditDB], where dbo.AuditLog lives.</summary>
    public string AuditDbConnectionString { get; init; } =
        "Server=(local)\\PLANTSAFETY;Database=PlantSafety.AuditDB;Integrated Security=true;TrustServerCertificate=true;";

    /// <summary>Command timeout (seconds) for AuditLog search queries.</summary>
    public int SearchCommandTimeoutSeconds { get; init; } = 60;

    /// <summary>Maximum page size a caller may request, to bound query and export/report cost.</summary>
    public int MaxPageSize { get; init; } = 5000;
}
