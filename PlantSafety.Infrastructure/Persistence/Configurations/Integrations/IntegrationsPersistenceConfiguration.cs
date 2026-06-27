// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Persistence.Configurations.Integrations
//  Layer:          Infrastructure
//  File:           IntegrationsPersistenceConfiguration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Persistence configuration constants and schema mappings for the Integrations area.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.Configurations.Integrations;

/// <summary>
/// Schema and stored-procedure name constants for the <c>Integrations</c> area.
/// Centralises all SQL artefact names to avoid magic strings in repositories.
/// </summary>
public static class IntegrationsPersistenceConfiguration
{
    /// <summary>SQL schema name for Integrations tables and stored procedures.</summary>
    public const string Schema = "Integrations";

    // TODO: Add stored-procedure name constants per aggregate, e.g.:
    // public const string UspInsert  = "[Integrations].[usp_ModuleName_Insert]";
    // public const string UspGetById = "[Integrations].[usp_ModuleName_GetById]";
    // public const string UspGetAll  = "[Integrations].[usp_ModuleName_GetAll]";
}
