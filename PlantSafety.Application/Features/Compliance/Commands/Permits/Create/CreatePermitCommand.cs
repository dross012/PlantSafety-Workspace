// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Command to create a new Permit to Work (PTW).
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.Compliance.Commands.Permits.Create;

/// <summary>
/// Creates a new Permit to Work in Draft status.
/// Caller must hold <c>Compliance.Permits.Create</c>.
/// The permit must be explicitly submitted for approval via a separate command.
/// </summary>
public sealed record CreatePermitCommand : ICommand<Guid>
{
    /// <inheritdoc/>
    public required Guid CommandId { get; init; }

    /// <inheritdoc/>
    public required Guid CorrelationId { get; init; }

    /// <inheritdoc/>
    public required Guid CausationId { get; init; }

    /// <inheritdoc/>
    public required DateTimeOffset RequestedAtUtc { get; init; }

    /// <inheritdoc/>
    public required string ApiVersionToken { get; init; }

    /// <inheritdoc/>
    public required Guid TenantId { get; init; }

    /// <inheritdoc/>
    public required Guid ActorId { get; init; }

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
