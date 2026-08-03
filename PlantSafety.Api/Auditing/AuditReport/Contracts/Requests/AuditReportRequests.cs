// ============================================================================
// File: AuditReportRequests.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (API Contract Validation Agent)
// Created: 2026-08-02
// Last Updated: 2026-08-02
// Description: External-facing request DTOs for the Auditing.AuditReport module's V1 controller.
//              Mirrors PlantSafety.UI.Modules.Auditing.Models.GenerateAuditReportRequest field-for-
//              field so the UI's typed client can round-trip without translation.
// ============================================================================

namespace PlantSafety.Api.Auditing.AuditReport.Contracts.Requests;

/// <summary>Request body for generating a printable/exportable AuditReport.</summary>
public sealed record GenerateAuditReportRequest
{
    /// <summary>Optional site scope.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Type discriminator of the entity being documented.</summary>
    public string TargetEntityType { get; init; } = string.Empty;

    /// <summary>Identity of the entity being documented.</summary>
    public Guid TargetEntityId { get; init; }

    /// <summary>Requested output format code ("Pdf", "Csv", or "Json").</summary>
    public string Format { get; init; } = "Pdf";

    /// <summary>Pre-serialized JSON for the Entity Identity section.</summary>
    public string EntityIdentityJson { get; init; } = "{}";

    /// <summary>Pre-serialized JSON for the Approvals section.</summary>
    public string ApprovalsJson { get; init; } = "{}";

    /// <summary>Pre-serialized JSON for the Exceptions section.</summary>
    public string ExceptionsJson { get; init; } = "{}";

    /// <summary>Pre-serialized JSON for the Signatures/Attestations section.</summary>
    public string SignaturesJson { get; init; } = "{}";
}
