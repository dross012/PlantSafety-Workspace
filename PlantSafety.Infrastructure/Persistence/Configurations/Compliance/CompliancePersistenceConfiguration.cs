// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Persistence.Configurations.Compliance
//  Layer:          Infrastructure
//  File:           CompliancePersistenceConfiguration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Persistence configuration constants and schema mappings for the Compliance area.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.Configurations.Compliance;

/// <summary>
/// Schema and stored-procedure name constants for the <c>Compliance</c> area.
/// Centralises all SQL artefact names to avoid magic strings in repositories.
/// </summary>
public static class CompliancePersistenceConfiguration
{
    /// <summary>SQL schema name for Compliance tables and stored procedures.</summary>
    public const string Schema = "Compliance";

    // TODO: Add stored-procedure name constants per aggregate, e.g.:
    // public const string UspInsert  = "[Compliance].[usp_ModuleName_Insert]";
    // public const string UspGetById = "[Compliance].[usp_ModuleName_GetById]";
    // public const string UspGetAll  = "[Compliance].[usp_ModuleName_GetAll]";
}
