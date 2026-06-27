// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Persistence
//  Layer:          Infrastructure
//  File:           ISqlConnectionFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Abstraction over SQL Server connection creation.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.ConnectionFactory;

/// <summary>Abstraction over SQL Server connection creation. Registered as scoped.</summary>
public interface ISqlConnectionFactory
{
    /// <summary>Creates and opens a new SQL connection using the primary connection string.</summary>
    SqlConnection CreateConnection();

    /// <summary>Creates and opens a new SQL connection using the read-replica connection string.</summary>
    SqlConnection CreateReadConnection();
}
