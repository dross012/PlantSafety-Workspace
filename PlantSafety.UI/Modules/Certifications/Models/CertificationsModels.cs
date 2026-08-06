// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs for the Certifications module. Backed by
//   7 PlantSafety.Api Compliance Certification read-models (Contractor, DOT,
//   EPA, IndustrySpecific, ISO14001, ISO45001, OSHACertifications) — each
//   wraps its payload in the platform-wide ApiResponse<T> shape
//   (Success/Data/Errors/Metadata). ReadModelEnvelopeDto/ReadModelPayloadDto
//   mirror that wire shape exactly so SafetyApiClientBase.GetAsync<T> can
//   deserialize it directly. No domain logic present — display only.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PlantSafety.UI.Modules.Certifications.Models;

/// <summary>A single contractor certification record.</summary>
public sealed record ContractorCertItemDto
{
    public Guid CertificationId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    /// <summary>Name of the certified contractor or contracting firm.</summary>
    public string ContractorName { get; init; } = string.Empty;
    /// <summary>Type of certification held (e.g., Insurance, Safety Orientation, Trade License).</summary>
    public string CertificationType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime IssuedAtUtc { get; init; }
    public DateTime ExpiresAtUtc { get; init; }
    public Guid VerifiedBy { get; init; }
}

/// <summary>A single DOT driver/vehicle certification record.</summary>
public sealed record DotCertItemDto
{
    public Guid CertificationId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    /// <summary>Name of the certified driver.</summary>
    public string DriverName { get; init; } = string.Empty;
    /// <summary>DOT license class (e.g., Class A CDL, Class B CDL).</summary>
    public string LicenseClass { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime IssuedAtUtc { get; init; }
    public DateTime ExpiresAtUtc { get; init; }
    public Guid VerifiedBy { get; init; }
}

/// <summary>A single EPA environmental certification/permit record.</summary>
public sealed record EpaCertItemDto
{
    public Guid CertificationId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    /// <summary>Name of the certified permit holder.</summary>
    public string PermitHolderName { get; init; } = string.Empty;
    /// <summary>EPA regulatory program (e.g., RCRA, CAA, CWA).</summary>
    public string EpaProgram { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime IssuedAtUtc { get; init; }
    public DateTime ExpiresAtUtc { get; init; }
    public Guid VerifiedBy { get; init; }
}

/// <summary>A single industry-specific certification record.</summary>
public sealed record IndustrySpecificCertItemDto
{
    public Guid CertificationId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    /// <summary>Name of the certified individual or asset.</summary>
    public string HolderName { get; init; } = string.Empty;
    /// <summary>Industry-specific standard the certification is issued against.</summary>
    public string IndustryStandard { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime IssuedAtUtc { get; init; }
    public DateTime ExpiresAtUtc { get; init; }
    public Guid VerifiedBy { get; init; }
}

/// <summary>A single ISO 14001 environmental management certification record.</summary>
public sealed record Iso14001CertItemDto
{
    public Guid CertificationId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    /// <summary>Name of the accredited certifying body.</summary>
    public string CertifyingBody { get; init; } = string.Empty;
    /// <summary>Scope of the certification (e.g., site, process line).</summary>
    public string Scope { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime IssuedAtUtc { get; init; }
    public DateTime ExpiresAtUtc { get; init; }
    public Guid VerifiedBy { get; init; }
}

/// <summary>A single ISO 45001 occupational health and safety management certification record.</summary>
public sealed record Iso45001CertItemDto
{
    public Guid CertificationId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    /// <summary>Name of the accredited certifying body.</summary>
    public string CertifyingBody { get; init; } = string.Empty;
    /// <summary>Scope of the certification (e.g., site, process line).</summary>
    public string Scope { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime IssuedAtUtc { get; init; }
    public DateTime ExpiresAtUtc { get; init; }
    public Guid VerifiedBy { get; init; }
}

/// <summary>A single OSHA certification record.</summary>
public sealed record OshaCertItemDto
{
    public Guid CertificationId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    /// <summary>Name of the certified individual.</summary>
    public string HolderName { get; init; } = string.Empty;
    /// <summary>OSHA standard the certification covers (e.g., OSHA 10-Hour, OSHA 30-Hour).</summary>
    public string OshaStandard { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime IssuedAtUtc { get; init; }
    public DateTime ExpiresAtUtc { get; init; }
    public Guid VerifiedBy { get; init; }
}

/// <summary>
/// A display-normalized certification row used to render any of the 7 certification
/// categories through a single DataGridComponent column set. Built client-side by
/// CertificationsMapper from each category's own item DTO shape.
/// </summary>
public sealed record CertificationRowViewModel
{
    public Guid CertificationId { get; init; }
    /// <summary>The category key (e.g., "contractor", "dot") — drives the detail page route.</summary>
    public string CategoryKey { get; init; } = string.Empty;
    /// <summary>Display label for the category (e.g., "Contractor", "DOT").</summary>
    public string CategoryLabel { get; init; } = string.Empty;
    /// <summary>The certification holder/subject name.</summary>
    public string HolderLabel { get; init; } = string.Empty;
    /// <summary>The certification sub-type/standard/program label.</summary>
    public string SubTypeLabel { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime IssuedAtUtc { get; init; }
    public DateTime ExpiresAtUtc { get; init; }
    public Guid VerifiedBy { get; init; }
    public Guid? SiteId { get; init; }
}

/// <summary>Read-model payload envelope, matching PlantSafety.Api's per-endpoint Response records.</summary>
/// <typeparam name="TItem">The item type contained in <see cref="Items"/>.</typeparam>
public sealed record ReadModelPayloadDto<TItem>
{
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public DateTime GeneratedAt { get; init; }
    public IReadOnlyList<TItem> Items { get; init; } = [];
}

/// <summary>
/// Client-side mirror of PlantSafety.Api.Models.ApiResponse&lt;T&gt; — the platform-wide
/// controller envelope (Success/Data/Errors/Metadata) used by all Compliance/Operational/
/// CrossDomain/Analytics read-model endpoints.
/// </summary>
/// <typeparam name="TItem">The item type contained in the payload's <c>Items</c> list.</typeparam>
public sealed record ReadModelEnvelopeDto<TItem>
{
    public bool Success { get; init; }
    public ReadModelPayloadDto<TItem>? Data { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];
}

// ============================================================
// Compliance.Certification write/lifecycle surface (api/v1/compliance/certification) —
// Create -> Verify -> Renew/Expire -> Close -> Archive. Distinct generic
// personnel-certification aggregate from the 7 read-only category verticals above.
// ============================================================

/// <summary>Full read-model record for a single generic Certification (mirrors <c>CertificationItemDto</c>), backing GET v1/compliance/certification/{id}.</summary>
public sealed record CertificationItemDto
{
    /// <summary>Record identifier.</summary>
    public Guid CertificationId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Created/Active/Completed/Closed/Archived).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Worker who holds this certification.</summary>
    public Guid? HolderWorkerId { get; init; }

    /// <summary>Name of the certification.</summary>
    public string CertificationName { get; init; } = string.Empty;

    /// <summary>Issuing/certifying body.</summary>
    public string CertificationBody { get; init; } = string.Empty;

    /// <summary>Certificate/credential number as issued by the certifying body.</summary>
    public string CertificationNumber { get; init; } = string.Empty;

    /// <summary>UTC timestamp the certification was issued.</summary>
    public DateTime IssuedAtUtc { get; init; }

    /// <summary>UTC timestamp the certification expires, if it has a validity window.</summary>
    public DateTime? ExpiresAtUtc { get; init; }

    /// <summary>Indicates whether this certification requires periodic renewal.</summary>
    public bool RenewalRequired { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public Guid CreatedBy { get; init; }

    /// <summary>Optimistic-concurrency token. Must be echoed back on subsequent mutating commands.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for submitting a new Certification record (POST v1/compliance/certification). Mirrors <c>CreateCertificationRequest</c>.</summary>
public sealed class CreateCertificationRequest
{
    /// <summary>Gets or sets the optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; set; }

    /// <summary>Gets or sets the worker who holds this certification.</summary>
    [Required]
    public Guid HolderWorkerId { get; set; }

    /// <summary>Gets or sets the name of the certification.</summary>
    [Required]
    [StringLength(400)]
    public string CertificationName { get; set; } = string.Empty;

    /// <summary>Gets or sets the issuing/certifying body.</summary>
    [Required]
    [StringLength(400)]
    public string CertificationBody { get; set; } = string.Empty;

    /// <summary>Gets or sets the certificate/credential number as issued by the certifying body.</summary>
    [Required]
    [StringLength(200)]
    public string CertificationNumber { get; set; } = string.Empty;

    /// <summary>Gets or sets the UTC timestamp the certification was issued. Defaults to the request time when omitted.</summary>
    public DateTime? IssuedAtUtc { get; set; }

    /// <summary>Gets or sets the UTC timestamp the certification expires, if it has a validity window.</summary>
    public DateTime? ExpiresAtUtc { get; set; }

    /// <summary>Gets or sets a value indicating whether this certification requires periodic renewal.</summary>
    public bool RenewalRequired { get; set; } = true;
}

/// <summary>Result of submitting a new Certification record. Mirrors <c>CreateCertificationResult</c>.</summary>
public sealed record CreateCertificationResultDto(Guid CertificationId, string Status);

/// <summary>Result payload shared by the Certification lifecycle-transition commands (Verify/Renew/Expire/Close/Archive). Mirrors <c>CertificationStatusResult</c>.</summary>
public sealed record CertificationStatusResultDto(Guid CertificationId, string Status);

/// <summary>Request body carrying only an optimistic-concurrency ETag (verify/expire/close/archive).</summary>
public sealed class EtagRequest
{
    /// <summary>Gets or sets the optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; set; }
}

/// <summary>Request body for renewing an active certification (POST .../renew). Mirrors <c>RenewCertificationRequest</c>.</summary>
public sealed class RenewCertificationRequest
{
    /// <summary>Gets or sets the new UTC expiration timestamp for the certification.</summary>
    [Required]
    public DateTime NewExpiresAtUtc { get; set; }

    /// <summary>Gets or sets the optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; set; }
}
