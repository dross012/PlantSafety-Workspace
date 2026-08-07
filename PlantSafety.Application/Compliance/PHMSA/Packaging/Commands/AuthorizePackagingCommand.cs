// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Application
//  File:           AuthorizePackagingCommand.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Command to authorize a new DOT packaging specification (49 CFR Part 178).
//                  Carries the full command envelope per Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.PHMSA.Packaging.Commands;

/// <summary>Authorizes a new DOT packaging specification.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="DOTSpec">DOT packaging specification code, e.g. "1A1", "3A", "4G".</param>
/// <param name="PerformanceTests">Performance tests this packaging has passed.</param>
/// <param name="MaxCapacity">Maximum rated capacity.</param>
/// <param name="CapacityUnit">Unit of measure for <paramref name="MaxCapacity"/>, e.g. "L", "kg".</param>
/// <param name="ClosureInstructions">Closure instructions required for this packaging to remain compliant.</param>
/// <param name="ReuseRestrictions">Restrictions on reuse of this packaging, if any.</param>
/// <param name="AssociatedMaterialId">HazmatMaterial this packaging spec is authorized for, if applicable.</param>
/// <param name="AuthorizedByUserId">Identity of the user authorizing the packaging specification.</param>
public sealed record AuthorizePackagingCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string DOTSpec,
    IReadOnlyList<string> PerformanceTests,
    decimal MaxCapacity,
    string CapacityUnit,
    string ClosureInstructions,
    string? ReuseRestrictions,
    Guid? AssociatedMaterialId,
    Guid AuthorizedByUserId) : ICommand<Result<AuthorizePackagingResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => AuthorizedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<AuthorizePackagingResult>>.CausationId => CausationId;
}
