// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Persistence.Configurations.ContractorVisitor
//  Layer:          Infrastructure
//  File:           ContractorVisitorPersistenceConfiguration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Persistence configuration constants and schema mappings for the ContractorVisitor area.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.Configurations.ContractorVisitor;

/// <summary>
/// Schema and stored-procedure name constants for the <c>ContractorVisitor</c> area.
/// Centralises all SQL artefact names to avoid magic strings in repositories.
/// </summary>
public static class ContractorVisitorPersistenceConfiguration
{
    /// <summary>SQL schema name for ContractorVisitor tables and stored procedures.</summary>
    public const string Schema = "ContractorVisitor";

    // TODO: Add stored-procedure name constants per aggregate, e.g.:
    // public const string UspInsert  = "[ContractorVisitor].[usp_ModuleName_Insert]";
    // public const string UspGetById = "[ContractorVisitor].[usp_ModuleName_GetById]";
    // public const string UspGetAll  = "[ContractorVisitor].[usp_ModuleName_GetAll]";
}
