// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Command to create a new User in the EnterpriseFabric module.
//   User creation is tenant-scoped and requires admin-level permissions.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.EnterpriseFabric.Commands.Users.Create;

/// <summary>
/// Creates a new user account within the tenant.
/// Caller must hold <c>EnterpriseFabric.Users.Create</c>.
/// A <c>UserCreatedEvent</c> is placed in the outbox on success.
/// </summary>
public sealed record CreateUserCommand : ICommand<Guid>
{
    /// <inheritdoc/>
    public required Guid CommandId { get; init; }

    /// <inheritdoc/>
    public required Guid CorrelationId { get; init; }

    /// <inheritdoc/>
    public required Guid? CausationId { get; init; }

    /// <inheritdoc/>
    public required DateTime RequestedAtUtc { get; init; }

    /// <inheritdoc/>
    public required string ApiVersionToken { get; init; }

    /// <inheritdoc/>
    public required Guid TenantId { get; init; }

    /// <inheritdoc/>
    public required Guid ActorId { get; init; }

    /// <summary>First name. Max 100 characters.</summary>
    public required string FirstName { get; init; }

    /// <summary>Last name. Max 100 characters.</summary>
    public required string LastName { get; init; }

    /// <summary>Email address (unique within tenant). Used as login identity.</summary>
    public required string Email { get; init; }

    /// <summary>Initial roles to assign. Must be valid role names.</summary>
    public required IReadOnlyList<string> Roles { get; init; }

    /// <summary>Site access list. Empty means no site-specific access.</summary>
    public required IReadOnlyList<Guid> SiteIds { get; init; }
}
