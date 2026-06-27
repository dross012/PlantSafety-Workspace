// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Persistence.Configurations.EnvironmentalHealth
//  Layer:          Infrastructure
//  File:           EnvironmentalHealthPersistenceConfiguration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Persistence configuration constants and schema mappings for the EnvironmentalHealth area.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.Configurations.EnvironmentalHealth;

/// <summary>
/// Schema and stored-procedure name constants for the <c>EnvironmentalHealth</c> area.
/// Centralises all SQL artefact names to avoid magic strings in repositories.
/// </summary>
public static class EnvironmentalHealthPersistenceConfiguration
{
    /// <summary>SQL schema name for EnvironmentalHealth tables and stored procedures.</summary>
    public const string Schema = "EnvironmentalHealth";

    // TODO: Add stored-procedure name constants per aggregate, e.g.:
    // public const string UspInsert  = "[EnvironmentalHealth].[usp_ModuleName_Insert]";
    // public const string UspGetById = "[EnvironmentalHealth].[usp_ModuleName_GetById]";
    // public const string UspGetAll  = "[EnvironmentalHealth].[usp_ModuleName_GetAll]";
}
