// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single PreQualificationVetting record.
// SchemaVersion: 1

namespace PlantSafety.Application.ContractorSafety.PreQualificationVetting.Models;

/// <summary>A single PreQualificationVetting record (Pre-qualification and vetting screens contractors before facility entry via safety performance metrics (EMR, TRIR), certifications, licenses, insurance coverage, and OSHA training compliance.).</summary>
public sealed record PreQualificationVettingItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid PreQualificationVettingId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Vetting Title.</summary>
    public required string VettingTitle { get; init; }

    /// <summary>Classification: SafetyMetrics, Certifications, InsuranceCoverage, OshaTrainingCompliance.</summary>
    public required string VettingCategory { get; init; }

    /// <summary>Contractor Name.</summary>
    public required string ContractorName { get; init; }

    /// <summary>Overall status classification.</summary>
    public required string VettingStatus { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}