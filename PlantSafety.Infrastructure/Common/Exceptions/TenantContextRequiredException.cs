// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common
//  Layer:          Infrastructure
//  File:           TenantContextRequiredException.cs
//  Created:        2026-07-26
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown by DapperQueryExecutor/DapperCommandExecutor when
//                  TenantContextPropagationOptions.EnableTenantContextPropagation is ON and a
//                  stored-procedure call's parameters carry no TenantId. See
//                  docs\lifecycle\security\rls-audit.md and TenantContextPropagationOptions for
//                  the remediation this enforces. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Common.Exceptions;

/// <summary>
/// Thrown when tenant-context enforcement is enabled but a repository call did not supply a
/// <c>TenantId</c> parameter, so SQL Server <c>SESSION_CONTEXT('TenantId')</c> (and therefore
/// Row-Level Security) could not be engaged for the call.
/// </summary>
public sealed class TenantContextRequiredException : InfrastructureException
{
    /// <summary>Name of the stored procedure that was called without a TenantId parameter.</summary>
    public string ProcedureName { get; }

    /// <summary>Creates a tenant-context-required exception for the given stored procedure.</summary>
    public TenantContextRequiredException(string procedureName)
        : base($"Stored procedure '{procedureName}' was called with tenant-context enforcement " +
               "enabled but no TenantId parameter was supplied. This repository call has not yet " +
               "been migrated to thread TenantId through to SESSION_CONTEXT/RLS.",
               "Persistence.TenantContextRequired", correlationId: string.Empty)
    {
        ProcedureName = procedureName;
    }
}
