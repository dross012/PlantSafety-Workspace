// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for RegisterCommunityContactCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.CommunitySafety.CommunityContact.Enums;
using PlantSafety.Domain.CommunitySafety.CommunityContact.Interfaces;
using Agg = PlantSafety.Domain.CommunitySafety.CommunityContact.Aggregates.CommunityContact;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.CommunitySafety.Commands;

/// <summary>Handles <see cref="RegisterCommunityContactCommand"/>.</summary>
public sealed class RegisterCommunityContactCommandHandler : IRequestHandler<RegisterCommunityContactCommand, Result<CommunityContactActionResult>>
{
    private readonly ICommunityContactRepository _repository;
    private readonly ILogger<RegisterCommunityContactCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RegisterCommunityContactCommandHandler(ICommunityContactRepository repository, ILogger<RegisterCommunityContactCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CommunityContactActionResult>> Handle(RegisterCommunityContactCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!Enum.TryParse<CommunityContactType>(request.ContactType, ignoreCase: true, out var contactType))
            {
                return Result<CommunityContactActionResult>.Failure(new ApplicationError(
                    "CommunitySafety.CommunityContact.InvalidContactType",
                    $"'{request.ContactType}' is not a recognized CommunityContact type.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            var aggregate = Agg.Register(
                createdBy: performedBy,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(performedBy),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                displayName: request.DisplayName,
                contactType: contactType,
                communityArea: request.CommunityArea,
                email: request.Email,
                organization: request.Organization,
                phone: request.Phone,
                optedInForNotifications: request.OptedInForNotifications);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CommunityContactActionResult>.Success(new CommunityContactActionResult(
                aggregate.CommunityContactId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register CommunityContact for Tenant {TenantId}", request.TenantId);

            return Result<CommunityContactActionResult>.Failure(new ApplicationError(
                "CommunitySafety.CommunityContact.RegisterFailed",
                "Unable to register the CommunityContact record at this time.",
                Guid.NewGuid()));
        }
    }
}
