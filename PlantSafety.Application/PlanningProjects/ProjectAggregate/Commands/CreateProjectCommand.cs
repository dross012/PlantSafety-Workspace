// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PlanningProjects.ProjectAggregate
//  Layer:          Application
//  File:           CreateProjectCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to create a new planning/capital Project. Carries the full command
//                  envelope (CorrelationId, CausationId, CommandId, RequestedAtUtc,
//                  ApiVersionToken) per Handbook Rule 2.1.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PlanningProjects.ProjectAggregate.Commands;

/// <summary>Creates a new planning/capital Project.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="Name">Project name.</param>
/// <param name="TargetCompletionUtc">UTC timestamp the project is targeted to complete.</param>
/// <param name="OwnerUserId">Identity of the project owner.</param>
public sealed record CreateProjectCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string Name,
    DateTime TargetCompletionUtc,
    Guid OwnerUserId) : ICommand<Result<ProjectStatusResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => OwnerUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<ProjectStatusResult>>.CausationId => CausationId;
}
