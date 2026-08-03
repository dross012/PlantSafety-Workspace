// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Application
//  File:           ReviseRouteDefinitionCommand.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Command to revise an active route definition in place (same-status action;
//                  increments Version) with a replaced ordered step list. Mirrors
//                  PlantSafety.Application.ProcessManagement.Commands.ReviseProcessDefinitionCommand's
//                  same-status shape, extended with the replaced step list -- genuinely new, as
//                  ProcessDefinition's Revise has no structural content to replace.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.ProcessManagement.Routing.Models;

namespace PlantSafety.Application.ProcessManagement.Routing.Commands;

/// <summary>Revises an active route definition in place, replacing its ordered step list.</summary>
public sealed record ReviseRouteDefinitionCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid RouteDefinitionId,
    IReadOnlyList<RouteStepInput> Steps,
    Guid RevisedByUserId) : ICommand<Result<RouteDefinitionStatusResult>>, IConcurrencyAwareCommand
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RevisedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<RouteDefinitionStatusResult>>.CausationId => CausationId;

    /// <inheritdoc/>
    public string? ETag { get; init; }
}
