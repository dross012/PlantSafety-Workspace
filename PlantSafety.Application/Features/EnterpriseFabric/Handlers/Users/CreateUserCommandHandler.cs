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
using PlantSafety.Application.Tenancy.Tenant.Repositories;
using PlantSafety.Domain.Billing.Subscription.Catalog;
using PlantSafety.Domain.Tenancy.Tenant.ValueObjects;

namespace PlantSafety.Application.Features.EnterpriseFabric.Handlers.Users;

/// <summary>Thrown when creating a user would exceed the tenant's SubscriptionPlan user cap (Phase 7 §3.4 License Enforcement).</summary>
public sealed class UserLimitExceededException(Guid tenantId, int currentUserCount, int maxUsers)
    : InvalidOperationException($"Tenant '{tenantId}' has reached its subscription plan's user cap ({maxUsers}). Current active users: {currentUserCount}.")
{
    /// <summary>The tenant that has reached its user cap.</summary>
    public Guid TenantId { get; } = tenantId;
}

/// <summary>Handles <see cref="CreateUserCommand"/>. Returns the new UserId.</summary>
public sealed class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Guid>
{
    private readonly IUserRepository _repository;
    private readonly ITenantReadRepository _tenantReadRepository;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateUserCommandHandler(
        IUserRepository repository,
        ITenantReadRepository tenantReadRepository,
        IDateTimeProvider clock,
        ILogger<CreateUserCommandHandler> logger)
    {
        _repository = repository;
        _tenantReadRepository = tenantReadRepository;
        _clock = clock;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Phase 7 §3.4 License Enforcement: reject user creation beyond the tenant's active
        // SubscriptionPlan user cap. Real, enforced check -- not a UI-only warning.
        var tenant = await _tenantReadRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is not null)
        {
            var plan = SubscriptionPlanCatalog.Resolve(SubscriptionPlanCode.From(tenant.PlanCode));
            if (plan.Limits.MaxUsers.HasValue)
            {
                var (_, currentUserCount) = await _repository.ListAsync(
                    request.TenantId, role: null, siteId: null, isActive: true,
                    orderBy: "UserId", sortDirection: "ASC", pageNumber: 1, pageSize: 1, cancellationToken);

                if (plan.Limits.IsUserCapReached(currentUserCount))
                {
                    _logger.LogWarning(
                        "User creation blocked for Tenant {TenantId}: plan '{PlanCode}' user cap {MaxUsers} reached (current {CurrentUserCount}).",
                        request.TenantId, plan.Tier.Code, plan.Limits.MaxUsers, currentUserCount);
                    throw new UserLimitExceededException(request.TenantId, currentUserCount, plan.Limits.MaxUsers.Value);
                }
            }
        }

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
