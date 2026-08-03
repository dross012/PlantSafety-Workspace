// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Infrastructure.Options
//  Layer:          Infrastructure
//  File:           SafetyDataOptions.cs
//  Created:        2026-07-27
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strongly typed options for Shared.SafetyData. Bound from
//                  configuration section "Shared:SafetyData". Unlike
//                  Shared.AuditLogging (which targets the separate, server-level
//                  PlantSafety.AuditDB), the tables backing this module
//                  ([RiskMgmt].[Risk], [RiskMgmt].[ControlMeasure],
//                  [ContinuousImprovement].[CAPA]) live in PlantSafety's main
//                  tenant database, PlantSafety.CoreDB (see RiskManagement\Sql\
//                  Tables\Risk.sql's "USE [PlantSafety.CoreDB]"), so this
//                  repository connects there directly.
//                  Additive-only evolution.
// ============================================================================
namespace Shared.SafetyData.Infrastructure.Options;

/// <summary>Strongly typed configuration for Shared.SafetyData.</summary>
public sealed class SafetyDataOptions
{
    /// <summary>Configuration section key.</summary>
    public const string SectionName = "Shared:SafetyData";

    /// <summary>Connection string targeting [PlantSafety.CoreDB], where the RiskMgmt and ContinuousImprovement schemas live.</summary>
    public string CoreDbConnectionString { get; init; } =
        "Server=(local)\\SQLEXPRESS;Database=PlantSafety.CoreDB;Integrated Security=true;TrustServerCertificate=true;";

    /// <summary>Command timeout (seconds) for Hazard Summary / Corrective Action / Safety Compliance queries.</summary>
    public int QueryCommandTimeoutSeconds { get; init; } = 60;

    /// <summary>Maximum page size a caller may request, to bound query and report-generation cost.</summary>
    public int MaxPageSize { get; init; } = 5000;
}
