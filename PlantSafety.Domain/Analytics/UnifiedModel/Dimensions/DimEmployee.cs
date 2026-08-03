// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.UnifiedModel
//  Layer:          Domain
//  File:           DimEmployee.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Employee dimension using the Slowly Changing Dimension Type 2 pattern.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Analytics.UnifiedModel.Abstractions;
using PlantSafety.Domain.Analytics.UnifiedModel.ValueObjects.Ids;

namespace PlantSafety.Domain.Analytics.UnifiedModel.Dimensions;

/// <summary>
/// SCD Type 2 dimension row describing an employee/user as they were configured (role, site
/// assignment, shift) during the row's effective window.
/// </summary>
public sealed record DimEmployee : ISlowlyChangingDimension
{
    /// <inheritdoc/>
    public required Guid SurrogateKey { get; init; }

    /// <summary>Stable UserId (natural key), same across all row versions.</summary>
    public required Guid BusinessKey { get; init; }

    /// <inheritdoc/>
    public required UtcTimestamp EffectiveFromUtc { get; init; }

    /// <inheritdoc/>
    public UtcTimestamp? EffectiveToUtc { get; init; }

    /// <inheritdoc/>
    public required bool IsCurrent { get; init; }

    /// <inheritdoc/>
    public required int RevisionNumber { get; init; }

    /// <summary>Tenant that owns this employee record.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Display name as of this row version.</summary>
    public required string DisplayName { get; init; }

    /// <summary>Job title/role as of this row version.</summary>
    public required string JobTitle { get; init; }

    /// <summary>Department as of this row version.</summary>
    public required string Department { get; init; }

    /// <summary>Surrogate key of the currently assigned site as of this row version.</summary>
    public Guid? AssignedSiteKey { get; init; }

    /// <summary>Employment status as of this row version (e.g. "Active", "OnLeave", "Terminated").</summary>
    public required string EmploymentStatus { get; init; }
}
