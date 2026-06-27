// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common
//  Layer:          Infrastructure
//  File:           PersistenceException.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Thrown on unrecoverable SQL/Dapper persistence failures.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Common.Exceptions;

/// <summary>Thrown when a persistence (SQL/Dapper) operation fails unrecoverably.</summary>
public sealed class PersistenceException : InfrastructureException
{
    /// <summary>Name of the stored procedure that failed.</summary>
    public string ProcedureName { get; }

    /// <summary>Creates a persistence exception for the given stored procedure.</summary>
    public PersistenceException(string procedureName, string correlationId, Exception? inner = null)
        : base($"Persistence operation failed in stored procedure '{procedureName}'.",
               "Persistence.StoredProcFailed", correlationId, inner)
    {
        ProcedureName = procedureName;
    }
}
