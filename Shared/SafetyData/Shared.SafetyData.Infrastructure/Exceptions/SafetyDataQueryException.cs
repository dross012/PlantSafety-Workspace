// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Infrastructure.Exceptions
//  Layer:          Infrastructure
//  File:           SafetyDataQueryException.cs
//  Created:        2026-07-27
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when dbo.usp_GetHazardSummary, dbo.usp_GetCorrectiveActions,
//                  or dbo.usp_GetSafetyCompliance fails.
//
//                  JUDGMENT CALL: mirrors Shared.AuditLogging's
//                  AuditLogQueryException. Shared.SafetyData is consumed by both
//                  PlantSafety and Reporting and MUST NOT depend on either
//                  module's assemblies (that would defeat the purpose of this
//                  extraction -- see docs\README.md), so this module defines its
//                  own minimal exception type instead of reusing either module's
//                  exception taxonomy (e.g. PlantSafety.Application.Common.
//                  Errors / PlantSafety.SharedKernel, or Reporting's own error
//                  types). Callers that want module-specific error mapping catch
//                  this type and re-classify at their layer.
//                  Additive-only evolution.
// ============================================================================
namespace Shared.SafetyData.Infrastructure.Exceptions;

/// <summary>Thrown when a Shared.SafetyData stored-procedure query fails unrecoverably.</summary>
public sealed class SafetyDataQueryException : Exception
{
    /// <summary>Creates an exception describing a failed Shared.SafetyData query.</summary>
    public SafetyDataQueryException(string message, Exception? inner = null)
        : base(message, inner)
    {
    }
}
