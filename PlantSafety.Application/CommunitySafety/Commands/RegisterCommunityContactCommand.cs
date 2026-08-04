// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Command to register a new CommunityContact ("CommunitySafety.CommunityContact").
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.CommunitySafety.Commands;

/// <summary>Registers a new CommunityContact record in the Community Contact Registry.</summary>
public sealed record RegisterCommunityContactCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid? SiteId,
    string DisplayName,
    string ContactType,
    string CommunityArea,
    string Email,
    string? Organization,
    string? Phone,
    bool OptedInForNotifications,
    Guid PerformedByUserId) : ICommand<Result<CommunityContactActionResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    Guid? ICommand<Result<CommunityContactActionResult>>.CausationId => CausationId;

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;
}
