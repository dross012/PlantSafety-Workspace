// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.ModelRegistry
//  Layer:          Application
//  File:           RegisterModelCommand.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Command to register a new model version in the Intelligence.ModelRegistry.
//                  Carries the full command envelope (CorrelationId, CausationId, CommandId,
//                  RequestedAtUtc, ApiVersionToken) per Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.Intelligence.DataLake.Commands.CreateDataLakeActivityCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Intelligence.ModelRegistry.Commands;

/// <summary>Registers a new model version in the model registry.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="ModelName">The logical model name.</param>
/// <param name="ModelVersion">The semantic version of this model ("MAJOR.MINOR.PATCH").</param>
/// <param name="DatasetHash">SHA-256 hash of the training dataset, for lineage tracking.</param>
/// <param name="TrainingAccuracyScore">Primary accuracy score from the training job (0.0-1.0).</param>
/// <param name="RegisteredByUserId">Identity of the user or process registering the model.</param>
public sealed record RegisterModelCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string ModelName,
    string ModelVersion,
    string DatasetHash,
    double TrainingAccuracyScore,
    Guid RegisteredByUserId) : ICommand<Result<ModelRegistryEntryStatusResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RegisteredByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<ModelRegistryEntryStatusResult>>.CausationId => CausationId;
}
