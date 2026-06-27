// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Persistence.Configurations.AiSafetyIntelligence
//  Layer:          Infrastructure
//  File:           AiSafetyIntelligencePersistenceConfiguration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Persistence configuration constants and schema mappings for the AiSafetyIntelligence area.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.Configurations.AiSafetyIntelligence;

/// <summary>
/// Schema and stored-procedure name constants for the <c>AiSafetyIntelligence</c> area.
/// Centralises all SQL artefact names to avoid magic strings in repositories.
/// </summary>
public static class AiSafetyIntelligencePersistenceConfiguration
{
    /// <summary>SQL schema name for AiSafetyIntelligence tables and stored procedures.</summary>
    public const string Schema = "AiSafetyIntelligence";

    // TODO: Add stored-procedure name constants per aggregate, e.g.:
    // public const string UspInsert  = "[AiSafetyIntelligence].[usp_ModuleName_Insert]";
    // public const string UspGetById = "[AiSafetyIntelligence].[usp_ModuleName_GetById]";
    // public const string UspGetAll  = "[AiSafetyIntelligence].[usp_ModuleName_GetAll]";
}
