// ============================================================================
// File: CultureRequests.cs
// Project: PlantSafety.Api
// Module: Analytics.Culture
// Created: 2026-08-02
// Description: Request DTOs bound from the HTTP request body for the Analytics.Culture V1
//              controller. Covers the full survey-period lifecycle: Create -> Review -> Publish ->
//              Close -> Archive. Every mutating lifecycle request carries an optional ETag field so
//              clients can supply the optimistic-concurrency token captured from a prior read (e.g.
//              CultureItemDto.ETag); a stale value results in a concurrency-conflict error. Mirrors
//              PlantSafety.Api.Compliance.Permits.Contracts.Requests.PermitsRequests.cs.
// ============================================================================

namespace PlantSafety.Api.Analytics.Culture.Contracts.Requests;

/// <summary>
/// Request body for submitting a new Culture survey period. Empty by design -- the canonical
/// <c>CreateCultureCommand</c> reference implementation is identity-only (TenantId/UserId/
/// CorrelationId/CausationId/Actor, all sourced from the envelope), matching
/// <c>PlantSafety.Application.Analytics.Culture.Commands.CreateCultureCommand</c>'s existing shape.
/// Reserved for future business-field parity with <c>Culture.Create</c>'s optional parameters
/// (SurveyPeriod/CultureIndexScore/EngagementScore/ParticipationRate/TopRiskTheme).
/// </summary>
public sealed record CreateCultureRequest;

/// <summary>Request body for reviewing a submitted Culture survey period (Created -&gt; Active).</summary>
public sealed record ReviewCultureRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Culture record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for publishing a reviewed Culture survey period's results (Active -&gt; Completed).</summary>
public sealed record PublishCultureRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Culture record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for formally closing a published Culture survey period (Completed -&gt; Closed).</summary>
public sealed record CloseCultureRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Culture record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving a closed Culture survey period for long-term retention (Closed -&gt; Archived).</summary>
public sealed record ArchiveCultureRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Culture record. Null skips the check.</summary>
    public string? ETag { get; init; }
}
