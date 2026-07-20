// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.SystemHealth
//  Layer:          Application
//  File:           SystemHealthDatabaseVersionDto.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    SQL Server version/edition of the primary tenant-scoped connection
//                  (ISqlConnectionFactory.CreateConnection()), sourced from SERVERPROPERTY.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Operations.SystemHealth.Models;

/// <summary>SQL Server version/edition of the primary tenant-scoped database connection.</summary>
/// <param name="ProductVersion">SERVERPROPERTY('ProductVersion'), e.g. "16.0.1000.6".</param>
/// <param name="ProductLevel">SERVERPROPERTY('ProductLevel'), e.g. "RTM" or "SP1".</param>
/// <param name="Edition">SERVERPROPERTY('Edition'), e.g. "Express Edition".</param>
/// <param name="EngineEdition">SERVERPROPERTY('EngineEdition') numeric code (4 = Express).</param>
public sealed record SystemHealthDatabaseVersionDto(
    string ProductVersion,
    string? ProductLevel,
    string Edition,
    int EngineEdition);
