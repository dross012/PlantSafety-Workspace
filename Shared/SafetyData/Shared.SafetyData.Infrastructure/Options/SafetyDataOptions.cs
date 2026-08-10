// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Infrastructure.Options
//  Layer:          Infrastructure
//  File:           SafetyDataOptions.cs
//  Created:        2026-07-27
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strongly typed options for Shared.SafetyData. Bound from
//                  configuration section "Shared:SafetyData". Phase 4.2:
//                  connection string and command timeout are no longer
//                  configured here -- SafetyDataDapperRepository now goes
//                  through Shared.SqlGateway's ISqlGateway (registered via
//                  AddSqlGateway inside AddSharedSafetyData), which owns
//                  the connection (Shared:SqlGateway:ConnectionString,
//                  pointed at PlantSafety.CoreDB -- see
//                  RiskManagement\Sql\Tables\Risk.sql's
//                  "USE [PlantSafety.CoreDB]") and command timeout
//                  centrally. Only the paging bound stays domain-specific.
//                  Additive-only evolution.
// ============================================================================
namespace Shared.SafetyData.Infrastructure.Options;

/// <summary>Strongly typed configuration for Shared.SafetyData.</summary>
public sealed class SafetyDataOptions
{
    /// <summary>Configuration section key.</summary>
    public const string SectionName = "Shared:SafetyData";

    /// <summary>Maximum page size a caller may request, to bound query and report-generation cost.</summary>
    public int MaxPageSize { get; init; } = 5000;
}
