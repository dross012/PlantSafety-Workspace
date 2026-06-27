// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Persistence.Configurations.Analytics
//  Layer:          Infrastructure
//  File:           AnalyticsPersistenceConfiguration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Persistence configuration constants and schema mappings for the Analytics area.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.Configurations.Analytics;

/// <summary>
/// Schema and stored-procedure name constants for the <c>Analytics</c> area.
/// Centralises all SQL artefact names to avoid magic strings in repositories.
/// </summary>
public static class AnalyticsPersistenceConfiguration
{
    /// <summary>SQL schema name for Analytics tables and stored procedures.</summary>
    public const string Schema = "Analytics";

    // TODO: Add stored-procedure name constants per aggregate, e.g.:
    // public const string UspInsert  = "[Analytics].[usp_ModuleName_Insert]";
    // public const string UspGetById = "[Analytics].[usp_ModuleName_GetById]";
    // public const string UspGetAll  = "[Analytics].[usp_ModuleName_GetAll]";
}
