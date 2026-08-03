// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Application
//  File:           CreateRouteDefinitionCommand.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Command to submit a new RouteDefinition record under the
//                  ProcessManagement.Routing submodule. Carries the full command envelope per
//                  Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.ProcessManagement.Commands.CreateProcessDefinitionCommand,
//                  extended with the route's initial ordered step list -- genuinely new, as
//                  ProcessDefinition has no structural content to create alongside itself.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.ProcessManagement.Routing.Models;

namespace PlantSafety.Application.ProcessManagement.Routing.Commands;

/// <summary>Submits a new RouteDefinition record with its initial ordered step list.</summary>
public sealed record CreateRouteDefinitionCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string PlantName,
    string Name,
    IReadOnlyList<RouteStepInput> Steps,
    Guid OwnerId,
    string OwnerName,
    DateTime EffectiveFromUtc,
    DateTime? EffectiveToUtc,
    Guid RequesterUserId) : ICommand<Result<CreateRouteDefinitionResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CreateRouteDefinitionResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="CreateRouteDefinitionCommand"/> from raw, untrusted transport values.</summary>
    public static CreateRouteDefinitionCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        string plantName,
        string name,
        IReadOnlyList<RouteStepInput> steps,
        Guid ownerId,
        string ownerName,
        DateTime effectiveFromUtc,
        DateTime? effectiveToUtc,
        Guid requesterUserId,
        Guid? correlationId = null,
        Guid? causationId = null,
        string apiVersionToken = "v1")
        => new(
            CommandId: Guid.NewGuid(),
            CorrelationId: correlationId ?? Guid.NewGuid(),
            CausationId: causationId ?? Guid.NewGuid(),
            RequestedAtUtc: DateTime.UtcNow,
            ApiVersionToken: apiVersionToken,
            TenantId: tenantId,
            SiteId: siteId,
            PlantName: (plantName ?? string.Empty).Trim(),
            Name: (name ?? string.Empty).Trim(),
            Steps: steps,
            OwnerId: ownerId,
            OwnerName: (ownerName ?? string.Empty).Trim(),
            EffectiveFromUtc: effectiveFromUtc,
            EffectiveToUtc: effectiveToUtc,
            RequesterUserId: requesterUserId);
}
