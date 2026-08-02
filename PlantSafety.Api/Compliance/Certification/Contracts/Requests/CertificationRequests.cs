// ============================================================================
// File: CertificationRequests.cs
// Project: PlantSafety.Api
// Module: Compliance.Certification
// Created: 2026-08-02
// Description: Request DTOs bound from the HTTP request body for the Compliance.Certification V1
//              controller. Covers the full lifecycle: Create -> Verify -> Renew/Expire -> Close ->
//              Archive. Mirrors
//              PlantSafety.Api.Compliance.Permits.Contracts.Requests.PermitsRequests.cs -- every
//              mutating lifecycle request carries an optional ETag field.
// ============================================================================

namespace PlantSafety.Api.Compliance.Certification.Contracts.Requests;

/// <summary>Request body for submitting a new Certification record.</summary>
public sealed record CreateCertificationRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Worker who holds this certification.</summary>
    public required Guid HolderWorkerId { get; init; }

    /// <summary>Name of the certification.</summary>
    public required string CertificationName { get; init; }

    /// <summary>Issuing/certifying body.</summary>
    public required string CertificationBody { get; init; }

    /// <summary>Certificate/credential number as issued by the certifying body.</summary>
    public required string CertificationNumber { get; init; }

    /// <summary>UTC timestamp the certification was issued. Defaults to the request time when omitted.</summary>
    public DateTime? IssuedAtUtc { get; init; }

    /// <summary>UTC timestamp the certification expires, if it has a validity window.</summary>
    public DateTime? ExpiresAtUtc { get; init; }

    /// <summary>Indicates whether this certification requires periodic renewal.</summary>
    public bool RenewalRequired { get; init; } = true;
}

/// <summary>Request body for verifying and activating a certification record (Created -&gt; Active).</summary>
public sealed record VerifyCertificationRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Certification record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for renewing an active certification (status unchanged).</summary>
public sealed record RenewCertificationRequest
{
    /// <summary>The new UTC expiration timestamp for the certification.</summary>
    public required DateTime NewExpiresAtUtc { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this Certification record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for expiring an active certification that passed its validity window (Active -&gt; Completed).</summary>
public sealed record ExpireCertificationRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Certification record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for formally closing a completed/expired certification (Completed -&gt; Closed).</summary>
public sealed record CloseCertificationRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Certification record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving a closed certification for compliance retention (Closed -&gt; Archived).</summary>
public sealed record ArchiveCertificationRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Certification record. Null skips the check.</summary>
    public string? ETag { get; init; }
}
