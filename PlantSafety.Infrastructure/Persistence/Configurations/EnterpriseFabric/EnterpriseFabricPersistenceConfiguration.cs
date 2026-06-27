// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Persistence.Configurations.EnterpriseFabric
//  Layer:          Infrastructure
//  File:           EnterpriseFabricPersistenceConfiguration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Persistence configuration constants and schema mappings for the EnterpriseFabric area.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.Configurations.EnterpriseFabric;

/// <summary>
/// Schema and stored-procedure name constants for the <c>EnterpriseFabric</c> area.
/// Centralises all SQL artefact names to avoid magic strings in repositories.
/// </summary>
public static class EnterpriseFabricPersistenceConfiguration
{
    /// <summary>SQL schema name for EnterpriseFabric tables and stored procedures.</summary>
    public const string Schema = "EnterpriseFabric";

    // TODO: Add stored-procedure name constants per aggregate, e.g.:
    // public const string UspInsert  = "[EnterpriseFabric].[usp_ModuleName_Insert]";
    // public const string UspGetById = "[EnterpriseFabric].[usp_ModuleName_GetById]";
    // public const string UspGetAll  = "[EnterpriseFabric].[usp_ModuleName_GetAll]";
}
