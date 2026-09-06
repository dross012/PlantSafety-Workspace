// ============================================================================
//  Project:        Shared.AuditLogging
//  Module:         Infrastructure.Exceptions
//  Layer:          Infrastructure
//  File:           AuditLogQueryException.cs
//  Created:        2026-07-26
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when the dbo.usp_SearchAuditLog stored procedure fails.
//
//                  JUDGMENT CALL: PlantSafety's original AuditLogDapperRepository routed
//                  SqlException handling through PlantSafety.Infrastructure.Common.
//                  Exceptions.ErrorClassifier, which maps into PlantSafety's own
//                  PlantSafety.Application.Common.Errors / PlantSafety.SharedKernel
//                  exception taxonomy. Shared.AuditLogging is consumed by both PlantSafety
//                  and Reporting and MUST NOT depend on PlantSafety's assemblies (that
//                  would invert the dependency direction this extraction exists to fix),
//                  so this module defines its own minimal exception type instead of
//                  reusing ErrorClassifier. Callers in PlantSafety/Reporting that want
//                  ErrorClassifier-style mapping onto their own taxonomy can catch this
//                  type and re-classify at their layer.
//                  Additive-only evolution.
// ============================================================================
namespace Shared.AuditLogging.Infrastructure.Exceptions;

/// <summary>Thrown when a dbo.usp_SearchAuditLog query fails unrecoverably.</summary>
public sealed class AuditLogQueryException : Exception
{
    /// <summary>Creates an exception describing a failed AuditLog query.</summary>
    public AuditLogQueryException(string message, Exception? inner = null)
        : base(message, inner)
    {
    }
}
