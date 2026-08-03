// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Request DTOs for the SafetyPrograms V1 controller. Domain fields only;
//   envelope fields (CommandId/CorrelationId/TenantId/etc.) are populated server-side by
//   RequestEnvelopeFactory.
// SchemaVersion: 1

namespace PlantSafety.Api.SafetyPrograms.Contracts.Requests;

/// <summary>Query parameters for <c>GET /api/v1/safety-programs</c>.</summary>
public sealed class GetSafetyProgramsRequest
{
    /// <summary>One-based page number.</summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>Items per page.</summary>
    public int PageSize { get; init; } = 25;

    /// <summary>Optional status filter (Planned/Active/OnHold/Completed).</summary>
    public string? Status { get; init; }
}

/// <summary>Request body for <c>POST /api/v1/safety-programs</c>.</summary>
public sealed class CreateSafetyProgramRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identity of the accountable program owner.</summary>
    public required Guid OwnerId { get; init; }

    /// <summary>Program name.</summary>
    public required string Name { get; init; }

    /// <summary>Program description.</summary>
    public string? Description { get; init; }

    /// <summary>Program category (Training/Behavioral/Compliance/Recognition).</summary>
    public required string Category { get; init; }

    /// <summary>Program start date.</summary>
    public required DateOnly StartDate { get; init; }

    /// <summary>Optional program end date.</summary>
    public DateOnly? EndDate { get; init; }

    /// <summary>Initial free-text goals/objectives, if any.</summary>
    public IReadOnlyList<string>? Goals { get; init; }
}

/// <summary>Request body for <c>POST /api/v1/safety-programs/{safetyProgramId}/status</c>.</summary>
public sealed class ChangeSafetyProgramStatusRequest
{
    /// <summary>The target status (Planned/Active/OnHold/Completed).</summary>
    public required string NewStatus { get; init; }
}
