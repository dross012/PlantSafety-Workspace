// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Persistence.Configurations.SafetyEvents
//  Layer:          Infrastructure
//  File:           SafetyEventsPersistenceConfiguration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Persistence configuration constants and schema mappings for the SafetyEvents area.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.Configurations.SafetyEvents;

/// <summary>
/// Schema and stored-procedure name constants for the <c>SafetyEvents</c> area.
/// Centralises all SQL artefact names to avoid magic strings in repositories.
/// </summary>
public static class SafetyEventsPersistenceConfiguration
{
    /// <summary>SQL schema name for SafetyEvents tables and stored procedures.</summary>
    public const string Schema = "SafetyEvents";

    // TODO: Add stored-procedure name constants per aggregate, e.g.:
    // public const string UspInsert  = "[SafetyEvents].[usp_ModuleName_Insert]";
    // public const string UspGetById = "[SafetyEvents].[usp_ModuleName_GetById]";
    // public const string UspGetAll  = "[SafetyEvents].[usp_ModuleName_GetAll]";
}
