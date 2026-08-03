// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Command to create a new SafetyProgram record. Carries the full command
//   envelope (CorrelationId, CausationId, CommandId, RequestedAtUtc, ApiVersionToken) per
//   Handbook Rule 2.1.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyPrograms.Commands;

/// <summary>Creates a new SafetyProgram record.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="OwnerId">Identity of the accountable program owner.</param>
/// <param name="Name">Program name.</param>
/// <param name="Description">Program description.</param>
/// <param name="Category">Program category (Training/Behavioral/Compliance/Recognition).</param>
/// <param name="StartDate">Program start date.</param>
/// <param name="EndDate">Optional program end date.</param>
/// <param name="Goals">Initial free-text goals/objectives, if any.</param>
public sealed record CreateSafetyProgramCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    Guid OwnerId,
    string Name,
    string Description,
    string Category,
    DateOnly StartDate,
    DateOnly? EndDate,
    IReadOnlyList<string>? Goals) : ICommand<Result<CreateSafetyProgramResult>>
{
    /// <inheritdoc/>
    public Guid ActorId => OwnerId;
    Guid? ICommand<Result<CreateSafetyProgramResult>>.CausationId => CausationId;

    /// <summary>
    /// Builds a <see cref="CreateSafetyProgramCommand"/> from raw, untrusted transport values,
    /// normalizing strings and generating envelope identifiers where the caller did not supply them.
    /// </summary>
    public static CreateSafetyProgramCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        Guid ownerId,
        string name,
        string description,
        string category,
        DateOnly startDate,
        DateOnly? endDate,
        IReadOnlyList<string>? goals = null,
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
            OwnerId: ownerId,
            Name: (name ?? string.Empty).Trim(),
            Description: (description ?? string.Empty).Trim(),
            Category: (category ?? string.Empty).Trim(),
            StartDate: startDate,
            EndDate: endDate,
            Goals: goals);
}
