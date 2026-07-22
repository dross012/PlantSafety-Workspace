// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to close out an AuditEngagement (Audit Workflow Step 5: Reporting), recording the final compliance score/rating. Carries the full command envelope per Handbook Rule 2.1.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Auditing.Commands;

/// <summary>Closes out an AuditEngagement under the Auditing bounded context.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="OwnerId">Identity of the accountable owner / CRUD-authorship user.</param>
/// <param name="AuditEngagementId">Identity of the AuditEngagement to close.</param>
/// <param name="ComplianceScore">Final computed compliance score (0-100).</param>
/// <param name="ComplianceRating">Final compliance rating classification: Satisfactory, NeedsImprovement, or Unsatisfactory.</param>
public sealed record CloseAuditEngagementCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid OwnerId,
    Guid AuditEngagementId,
    decimal? ComplianceScore,
    string ComplianceRating) : ICommand<Result<CloseAuditEngagementResult>>
{
    /// <summary>Placeholder: no tenant field exists on this command's payload; defaults to <see cref="Guid.Empty"/> until tenant scoping is threaded through.</summary>
    public Guid TenantId { get; init; } = Guid.Empty;
    /// <inheritdoc/>
    public Guid ActorId => OwnerId;
    Guid? ICommand<Result<CloseAuditEngagementResult>>.CausationId => CausationId;

    /// <summary>
    /// Builds a <see cref="CloseAuditEngagementCommand"/> from raw, untrusted transport values, normalizing
    /// strings and generating envelope identifiers where the caller did not supply them.
    /// </summary>
    public static CloseAuditEngagementCommand FromDto(
        Guid ownerId,
        Guid auditEngagementId,
        decimal? complianceScore,
        string complianceRating,
        Guid? correlationId = null,
        Guid? causationId = null,
        string apiVersionToken = "v1")
        => new(
            CommandId: Guid.NewGuid(),
            CorrelationId: correlationId ?? Guid.NewGuid(),
            CausationId: causationId ?? Guid.NewGuid(),
            RequestedAtUtc: DateTime.UtcNow,
            ApiVersionToken: apiVersionToken,
            OwnerId: ownerId,
            AuditEngagementId: auditEngagementId,
            ComplianceScore: complianceScore,
            ComplianceRating: (complianceRating ?? "NotRated").Trim());
}
