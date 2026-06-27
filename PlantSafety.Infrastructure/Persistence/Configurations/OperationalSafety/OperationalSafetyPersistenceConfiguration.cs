// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Persistence.Configurations.OperationalSafety
//  Layer:          Infrastructure
//  File:           OperationalSafetyPersistenceConfiguration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Persistence configuration constants and schema mappings for the OperationalSafety area.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.Configurations.OperationalSafety;

/// <summary>
/// Schema and stored-procedure name constants for the <c>OperationalSafety</c> area.
/// Centralises all SQL artefact names to avoid magic strings in repositories.
/// </summary>
public static class OperationalSafetyPersistenceConfiguration
{
    /// <summary>SQL schema name for OperationalSafety tables and stored procedures.</summary>
    public const string Schema = "OperationalSafety";

    // TODO: Add stored-procedure name constants per aggregate, e.g.:
    // public const string UspInsert  = "[OperationalSafety].[usp_ModuleName_Insert]";
    // public const string UspGetById = "[OperationalSafety].[usp_ModuleName_GetById]";
    // public const string UspGetAll  = "[OperationalSafety].[usp_ModuleName_GetAll]";
}
