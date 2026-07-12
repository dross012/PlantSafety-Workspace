// ============================================================================
// File: PermitsRequests.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: Request DTOs bound from the HTTP request body/query string for the
//              Compliance/Permits Features/ controller.
// ============================================================================

namespace PlantSafety.Api.Features.Compliance.Permits.Contracts.Requests;

/// <summary>Request body for creating a new Permit to Work.</summary>
public sealed record CreatePermitRequest
{
    /// <summary>Site where the work is authorised.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Type of permit (e.g., HotWork, ColdWork, ConfinedSpace, Electrical, LockoutTagout).</summary>
    public required string PermitType { get; init; }

    /// <summary>Description of work to be performed. Max 2000 characters.</summary>
    public required string WorkDescription { get; init; }

    /// <summary>Physical location. Max 500 characters.</summary>
    public required string WorkLocation { get; init; }

    /// <summary>Worker or crew lead performing the work.</summary>
    public required Guid WorkLeadId { get; init; }

    /// <summary>UTC timestamp when the permit becomes valid.</summary>
    public required DateTimeOffset ValidFromUtc { get; init; }

    /// <summary>UTC timestamp when the permit expires.</summary>
    public required DateTimeOffset ValidToUtc { get; init; }
}

/// <summary>Request body for approving a Permit to Work.</summary>
public sealed record ApprovePermitRequest
{
    /// <summary>Optional approval notes.</summary>
    public string? ApprovalNotes { get; init; }

    /// <summary>Optional concurrency token (row version) supplied by the client.</summary>
    public string? ETag { get; init; }
}

/// <summary>Query-string bound filters/paging for listing Permits.</summary>
public sealed record ListPermitsRequest
{
    /// <summary>1-based page number.</summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>Items per page.</summary>
    public int PageSize { get; init; } = 25;

    /// <summary>Sort field (ValidFromUtc, ValidToUtc, Status, PermitType).</summary>
    public string OrderBy { get; init; } = "ValidFromUtc";

    /// <summary>Sort direction.</summary>
    public string SortDirection { get; init; } = "DESC";

    /// <summary>Optional site filter.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Optional permit type filter.</summary>
    public string? PermitType { get; init; }

    /// <summary>Optional status filter.</summary>
    public string? Status { get; init; }

    /// <summary>Inclusive lower bound on validity start.</summary>
    public DateTimeOffset? ValidFromUtc { get; init; }

    /// <summary>Inclusive upper bound on validity end.</summary>
    public DateTimeOffset? ValidToUtc { get; init; }
}
