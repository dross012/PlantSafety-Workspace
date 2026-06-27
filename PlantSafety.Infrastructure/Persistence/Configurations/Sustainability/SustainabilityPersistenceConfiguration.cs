// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Persistence.Configurations.Sustainability
//  Layer:          Infrastructure
//  File:           SustainabilityPersistenceConfiguration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Persistence configuration constants and schema mappings for the Sustainability area.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.Configurations.Sustainability;

/// <summary>
/// Schema and stored-procedure name constants for the <c>Sustainability</c> area.
/// Centralises all SQL artefact names to avoid magic strings in repositories.
/// </summary>
public static class SustainabilityPersistenceConfiguration
{
    /// <summary>SQL schema name for Sustainability tables and stored procedures.</summary>
    public const string Schema = "Sustainability";

    // TODO: Add stored-procedure name constants per aggregate, e.g.:
    // public const string UspInsert  = "[Sustainability].[usp_ModuleName_Insert]";
    // public const string UspGetById = "[Sustainability].[usp_ModuleName_GetById]";
    // public const string UspGetAll  = "[Sustainability].[usp_ModuleName_GetAll]";
}
