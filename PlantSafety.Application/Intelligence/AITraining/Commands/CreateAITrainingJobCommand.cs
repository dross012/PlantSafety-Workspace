// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AITraining
//  Layer:          Application
//  File:           CreateAITrainingJobCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to create a new Intelligence.AITraining training job record. Carries the
//                  full command envelope (CorrelationId, CausationId, CommandId, RequestedAtUtc,
//                  ApiVersionToken) per Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.ContinuousImprovement.RCA.Commands.CreateRCACommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Intelligence.AITraining.Commands;

/// <summary>Creates a new Intelligence.AITraining training job record.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="ModelName">Name of the model being trained.</param>
/// <param name="ModelVersion">Version identifier of the trained model artifact.</param>
/// <param name="DatasetReference">Reference to the training dataset used for this run.</param>
/// <param name="InitiatedByUserId">Identity of the user or process that initiated the training job.</param>
public sealed record CreateAITrainingJobCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string ModelName,
    string ModelVersion,
    string? DatasetReference,
    Guid InitiatedByUserId) : ICommand<Result<AITrainingJobStatusResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => InitiatedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<AITrainingJobStatusResult>>.CausationId => CausationId;
}
