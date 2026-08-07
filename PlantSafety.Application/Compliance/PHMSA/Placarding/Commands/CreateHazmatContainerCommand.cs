// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Application
//  File:           CreateHazmatContainerCommand.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Command to create a new HazmatContainer record under the PHMSA Placarding
//                  (Marking/Labeling/Placarding) bounded context. Carries the full command
//                  envelope (CorrelationId, CausationId, CommandId, RequestedAtUtc,
//                  ApiVersionToken) per Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.PHMSA.Placarding.Commands;

/// <summary>Creates a new HazmatContainer record.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="ContainerType">Physical container/packaging form factor (e.g. Drum, IntermediateBulkContainer, Cylinder).</param>
/// <param name="MaterialId">Reference to the classified material inside this container (Classification module).</param>
/// <param name="PackagingSpecId">Reference to the authorized packaging specification (Packaging module).</param>
/// <param name="RequestedByUserId">Identity of the user requesting creation.</param>
public sealed record CreateHazmatContainerCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string ContainerType,
    Guid MaterialId,
    Guid PackagingSpecId,
    Guid RequestedByUserId) : ICommand<Result<CreateHazmatContainerResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequestedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CreateHazmatContainerResult>>.CausationId => CausationId;
}
