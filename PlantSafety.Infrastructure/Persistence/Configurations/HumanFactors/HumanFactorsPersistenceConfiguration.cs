// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Persistence.Configurations.HumanFactors
//  Layer:          Infrastructure
//  File:           HumanFactorsPersistenceConfiguration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Persistence configuration constants and schema mappings for the HumanFactors area.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.Configurations.HumanFactors;

/// <summary>
/// Schema and stored-procedure name constants for the <c>HumanFactors</c> area.
/// Centralises all SQL artefact names to avoid magic strings in repositories.
/// </summary>
public static class HumanFactorsPersistenceConfiguration
{
    /// <summary>SQL schema name for HumanFactors tables and stored procedures.</summary>
    public const string Schema = "HumanFactors";

    // TODO: Add stored-procedure name constants per aggregate, e.g.:
    // public const string UspInsert  = "[HumanFactors].[usp_ModuleName_Insert]";
    // public const string UspGetById = "[HumanFactors].[usp_ModuleName_GetById]";
    // public const string UspGetAll  = "[HumanFactors].[usp_ModuleName_GetAll]";
}
