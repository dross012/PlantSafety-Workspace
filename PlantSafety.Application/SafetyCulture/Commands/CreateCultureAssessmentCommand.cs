// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Command to create a new CultureAssessment record within the SafetyCulture module. Carries the full command envelope (CorrelationId, CausationId, CommandId, RequestedAtUtc, ApiVersionToken) per Handbook Rule 2.1.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyCulture.Commands;

/// <summary>
/// Creates a new CultureAssessment record under the SafetyCulture bounded context.
/// </summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="OwnerId">Identity of the accountable owner / CRUD-authorship user.</param>
/// <param name="AssessmentTitle">Human-readable assessment title.</param>
/// <param name="PeriodStartUtc">UTC start of the period this assessment covers.</param>
/// <param name="PeriodEndUtc">UTC end of the period this assessment covers.</param>
/// <param name="Summary">Narrative summary of assessment findings and themes.</param>
public sealed record CreateCultureAssessmentCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    Guid OwnerId,
    string AssessmentTitle,
    DateTime PeriodStartUtc,
    DateTime PeriodEndUtc,
    string Summary) : ICommand<Result<CreateCultureAssessmentResult>>
{
    /// <inheritdoc/>
    public Guid ActorId => OwnerId;
    Guid? ICommand<Result<CreateCultureAssessmentResult>>.CausationId => CausationId;

    /// <summary>
    /// Builds a <see cref="CreateCultureAssessmentCommand"/> from raw, untrusted transport values, normalizing
    /// strings and generating envelope identifiers where the caller did not supply them.
    /// </summary>
    public static CreateCultureAssessmentCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        Guid ownerId,
        string assessmentTitle,
        DateTime periodStartUtc,
        DateTime periodEndUtc,
        string summary,
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
            AssessmentTitle: (assessmentTitle ?? string.Empty).Trim(),
            PeriodStartUtc: periodStartUtc,
            PeriodEndUtc: periodEndUtc,
            Summary: (summary ?? string.Empty).Trim());
}