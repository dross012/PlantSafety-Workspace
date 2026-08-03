// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Command to generate a printable/exportable audit report (PDF/CSV/JSON) for a
//   target entity (Handbook Phase 5 §6.2). The Timeline section is always auto-populated by
//   querying GetAuditTimelineForEntityQuery; the other four canonical sections (Entity Identity,
//   Approvals, Exceptions, Signatures) are supplied as pre-serialized JSON by the caller, since
//   their shape is inherently specific to each of the dozens of entity types this platform can
//   report on. Carries the full command envelope per Handbook Rule 2.1.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Auditing.AuditReport.Models;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Auditing.AuditReport.Commands;

/// <summary>
/// Generates a printable/exportable audit report for a target entity.
/// </summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope.</param>
/// <param name="RequestedBy">Identity of the user requesting the report.</param>
/// <param name="TargetEntityType">Type discriminator of the entity being documented.</param>
/// <param name="TargetEntityId">Identity of the entity being documented.</param>
/// <param name="Format">Requested output format code ("Pdf", "Csv", or "Json").</param>
/// <param name="EntityIdentityJson">Pre-serialized JSON for the Entity Identity section.</param>
/// <param name="ApprovalsJson">Pre-serialized JSON for the Approvals section.</param>
/// <param name="ExceptionsJson">Pre-serialized JSON for the Exceptions section.</param>
/// <param name="SignaturesJson">Pre-serialized JSON for the Signatures/Attestations section.</param>
public sealed record GenerateAuditReportCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    Guid RequestedBy,
    string TargetEntityType,
    Guid TargetEntityId,
    string Format,
    string EntityIdentityJson,
    string ApprovalsJson,
    string ExceptionsJson,
    string SignaturesJson) : ICommand<Result<AuditReportDto>>
{
    /// <inheritdoc/>
    public Guid ActorId => RequestedBy;
    Guid? ICommand<Result<AuditReportDto>>.CausationId => CausationId;

    /// <summary>Builds a command from raw values, generating envelope identifiers where not supplied.</summary>
    public static GenerateAuditReportCommand FromDto(
        Guid tenantId, Guid? siteId, Guid requestedBy, string targetEntityType, Guid targetEntityId, string format,
        string entityIdentityJson, string approvalsJson, string exceptionsJson, string signaturesJson,
        Guid? correlationId = null, Guid? causationId = null, string apiVersionToken = "v1")
        => new(
            Guid.NewGuid(), correlationId ?? Guid.NewGuid(), causationId ?? Guid.NewGuid(), DateTime.UtcNow, apiVersionToken,
            tenantId, siteId, requestedBy, (targetEntityType ?? string.Empty).Trim(), targetEntityId, (format ?? string.Empty).Trim(),
            entityIdentityJson ?? "{}", approvalsJson ?? "{}", exceptionsJson ?? "{}", signaturesJson ?? "{}");
}
