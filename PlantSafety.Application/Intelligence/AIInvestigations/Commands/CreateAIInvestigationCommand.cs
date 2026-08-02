// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AIInvestigations
//  Layer:          Application
//  File:           CreateAIInvestigationCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to open a new AI-assisted safety investigation. Carries the full
//                  command envelope (CorrelationId, CausationId, CommandId, RequestedAtUtc,
//                  ApiVersionToken) per Handbook Rule 2.1.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Intelligence.AIInvestigations.Commands;

/// <summary>Opens a new AI-assisted safety investigation.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="IncidentReference">Reference identifier of the related safety incident.</param>
/// <param name="AiConfidenceScore">AI model confidence score (0.0-1.0) for the investigation findings.</param>
/// <param name="Summary">AI-generated summary of the investigation.</param>
/// <param name="AssignedToUserId">Identity of the user or process assigned to the investigation.</param>
public sealed record CreateAIInvestigationCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string IncidentReference,
    double AiConfidenceScore,
    string? Summary,
    Guid AssignedToUserId) : ICommand<Result<AIInvestigationStatusResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => AssignedToUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<AIInvestigationStatusResult>>.CausationId => CausationId;
}
