// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Persistence.Configurations.Training
//  Layer:          Infrastructure
//  File:           TrainingPersistenceConfiguration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Persistence configuration constants and schema mappings for the Training area.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.Configurations.Training;

/// <summary>
/// Schema and stored-procedure name constants for the <c>Training</c> area.
/// Centralises all SQL artefact names to avoid magic strings in repositories.
/// </summary>
public static class TrainingPersistenceConfiguration
{
    /// <summary>SQL schema name for Training tables and stored procedures.</summary>
    public const string Schema = "Training";

    // TODO: Add stored-procedure name constants per aggregate, e.g.:
    // public const string UspInsert  = "[Training].[usp_ModuleName_Insert]";
    // public const string UspGetById = "[Training].[usp_ModuleName_GetById]";
    // public const string UspGetAll  = "[Training].[usp_ModuleName_GetAll]";
}
