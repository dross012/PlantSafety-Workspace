// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for CreateUserCommand.
//   Email is never logged; only UserId and TenantId are emitted in log fields.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Clock;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Utilities;
using PlantSafety.Application.Features.EnterpriseFabric.Commands.Users.Create;

namespace PlantSafety.Application.Features.EnterpriseFabric.Handlers.Users;

/// <summary>Handles <see cref="CreateUserCommand"/>. Returns the new UserId.</summary>
public sealed class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Guid>
{
    private readonly IUserRepository _repository;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateUserCommandHandler(
        IUserRepository repository,
        IDateTimeProvider clock,
        ILogger<CreateUserCommandHandler> logger)
    {
        _repository = repository;
        _clock = clock;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var userId = CorrelationIdGenerator.NewId(_clock.UtcNowOffset);

        await _repository.CreateAsync(
            userId: userId,
            tenantId: request.TenantId,
            email: request.Email,
            firstName: request.FirstName,
            lastName: request.LastName,
            roles: request.Roles,
            siteIds: request.SiteIds,
            actorId: request.ActorId,
            correlationId: request.CorrelationId,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "User {UserId} created for Tenant {TenantId} by Actor {ActorId}",
            userId, request.TenantId, request.ActorId);

        return userId;
    }
}
