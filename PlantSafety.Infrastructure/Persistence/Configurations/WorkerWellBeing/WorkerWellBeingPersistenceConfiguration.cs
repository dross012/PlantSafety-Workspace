// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Persistence.Configurations.WorkerWellBeing
//  Layer:          Infrastructure
//  File:           WorkerWellBeingPersistenceConfiguration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Persistence configuration constants and schema mappings for the WorkerWellBeing area.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.Configurations.WorkerWellBeing;

/// <summary>
/// Schema and stored-procedure name constants for the <c>WorkerWellBeing</c> area.
/// Centralises all SQL artefact names to avoid magic strings in repositories.
/// </summary>
public static class WorkerWellBeingPersistenceConfiguration
{
    /// <summary>SQL schema name for WorkerWellBeing tables and stored procedures.</summary>
    public const string Schema = "WorkerWellBeing";

    // TODO: Add stored-procedure name constants per aggregate, e.g.:
    // public const string UspInsert  = "[WorkerWellBeing].[usp_ModuleName_Insert]";
    // public const string UspGetById = "[WorkerWellBeing].[usp_ModuleName_GetById]";
    // public const string UspGetAll  = "[WorkerWellBeing].[usp_ModuleName_GetAll]";
}
