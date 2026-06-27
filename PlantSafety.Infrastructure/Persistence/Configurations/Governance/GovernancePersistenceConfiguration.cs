// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Persistence.Configurations.Governance
//  Layer:          Infrastructure
//  File:           GovernancePersistenceConfiguration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Persistence configuration constants and schema mappings for the Governance area.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.Configurations.Governance;

/// <summary>
/// Schema and stored-procedure name constants for the <c>Governance</c> area.
/// Centralises all SQL artefact names to avoid magic strings in repositories.
/// </summary>
public static class GovernancePersistenceConfiguration
{
    /// <summary>SQL schema name for Governance tables and stored procedures.</summary>
    public const string Schema = "Governance";

    // TODO: Add stored-procedure name constants per aggregate, e.g.:
    // public const string UspInsert  = "[Governance].[usp_ModuleName_Insert]";
    // public const string UspGetById = "[Governance].[usp_ModuleName_GetById]";
    // public const string UspGetAll  = "[Governance].[usp_ModuleName_GetAll]";
}
