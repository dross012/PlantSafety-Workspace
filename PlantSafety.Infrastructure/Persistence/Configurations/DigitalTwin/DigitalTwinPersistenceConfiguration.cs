// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Persistence.Configurations.DigitalTwin
//  Layer:          Infrastructure
//  File:           DigitalTwinPersistenceConfiguration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Persistence configuration constants and schema mappings for the DigitalTwin area.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.Configurations.DigitalTwin;

/// <summary>
/// Schema and stored-procedure name constants for the <c>DigitalTwin</c> area.
/// Centralises all SQL artefact names to avoid magic strings in repositories.
/// </summary>
public static class DigitalTwinPersistenceConfiguration
{
    /// <summary>SQL schema name for DigitalTwin tables and stored procedures.</summary>
    public const string Schema = "DigitalTwin";

    // TODO: Add stored-procedure name constants per aggregate, e.g.:
    // public const string UspInsert  = "[DigitalTwin].[usp_ModuleName_Insert]";
    // public const string UspGetById = "[DigitalTwin].[usp_ModuleName_GetById]";
    // public const string UspGetAll  = "[DigitalTwin].[usp_ModuleName_GetAll]";
}
