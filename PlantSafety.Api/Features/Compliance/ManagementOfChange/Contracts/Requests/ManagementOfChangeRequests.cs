// ============================================================================
// File: ManagementOfChangeRequests.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: Request DTOs bound from the HTTP request body for the
//              Compliance/ManagementOfChange Features/ controller.
// ============================================================================

namespace PlantSafety.Api.Features.Compliance.ManagementOfChange.Contracts.Requests;

/// <summary>Request body for initiating a Management of Change request.</summary>
public sealed record CreateMocRequest
{
    /// <summary>Site where the change is proposed.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Title. Max 200 characters.</summary>
    public required string Title { get; init; }

    /// <summary>Description of the change. Max 4000 characters.</summary>
    public required string Description { get; init; }

    /// <summary>Change type (Permanent, Temporary, Emergency).</summary>
    public required string ChangeType { get; init; }

    /// <summary>Risk level (Low, Medium, High, Critical).</summary>
    public required string RiskLevel { get; init; }

    /// <summary>Initiator worker identifier.</summary>
    public required Guid InitiatorId { get; init; }

    /// <summary>Proposed start timestamp for the change.</summary>
    public required DateTimeOffset ProposedStartUtc { get; init; }

    /// <summary>Proposed end timestamp for the change, if known.</summary>
    public DateTimeOffset? ProposedEndUtc { get; init; }
}
