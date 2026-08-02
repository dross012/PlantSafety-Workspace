// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.DataLake
//  Layer:          Application
//  File:           CreateDataLakeActivityCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to create a new Intelligence.DataLake activity record. Carries the full
//                  command envelope (CorrelationId, CausationId, CommandId, RequestedAtUtc,
//                  ApiVersionToken) per Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.ContinuousImprovement.RCA.Commands.CreateRCACommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Intelligence.DataLake.Commands;

/// <summary>Creates a new Intelligence.DataLake activity record.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="DatasetName">Name of the dataset associated with this record.</param>
/// <param name="Zone">Data lake zone the record resides in (e.g., Raw/Cleansed/Curated/ML/Sandbox).</param>
/// <param name="RecordCount">Number of records processed in this activity.</param>
/// <param name="ProcessedByUserId">Identity of the user or process that performed the activity.</param>
public sealed record CreateDataLakeActivityCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string DatasetName,
    string Zone,
    long? RecordCount,
    Guid ProcessedByUserId) : ICommand<Result<DataLakeActivityStatusResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => ProcessedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<DataLakeActivityStatusResult>>.CausationId => CausationId;
}
