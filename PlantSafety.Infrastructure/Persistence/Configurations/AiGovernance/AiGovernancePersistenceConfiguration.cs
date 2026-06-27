// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Persistence.Configurations.AiGovernance
//  Layer:          Infrastructure
//  File:           AiGovernancePersistenceConfiguration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Persistence configuration constants and schema mappings for the AiGovernance area.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.Configurations.AiGovernance;

/// <summary>
/// Schema and stored-procedure name constants for the <c>AiGovernance</c> area.
/// Centralises all SQL artefact names to avoid magic strings in repositories.
/// </summary>
public static class AiGovernancePersistenceConfiguration
{
    /// <summary>SQL schema name for AiGovernance tables and stored procedures.</summary>
    public const string Schema = "AiGovernance";

    // TODO: Add stored-procedure name constants per aggregate, e.g.:
    // public const string UspInsert  = "[AiGovernance].[usp_ModuleName_Insert]";
    // public const string UspGetById = "[AiGovernance].[usp_ModuleName_GetById]";
    // public const string UspGetAll  = "[AiGovernance].[usp_ModuleName_GetAll]";
}
