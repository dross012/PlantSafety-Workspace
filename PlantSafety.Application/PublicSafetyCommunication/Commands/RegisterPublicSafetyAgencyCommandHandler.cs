// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for RegisterPublicSafetyAgencyCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Aggregates;
using PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Enums;
using PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PublicSafetyCommunication.Commands;

/// <summary>Handles <see cref="RegisterPublicSafetyAgencyCommand"/>.</summary>
public sealed class RegisterPublicSafetyAgencyCommandHandler : IRequestHandler<RegisterPublicSafetyAgencyCommand, Result<PublicSafetyAgencyActionResult>>
{
    private readonly IPublicSafetyAgencyRepository _repository;
    private readonly ILogger<RegisterPublicSafetyAgencyCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RegisterPublicSafetyAgencyCommandHandler(IPublicSafetyAgencyRepository repository, ILogger<RegisterPublicSafetyAgencyCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PublicSafetyAgencyActionResult>> Handle(RegisterPublicSafetyAgencyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!Enum.TryParse<PublicSafetyAgencyType>(request.AgencyType, ignoreCase: true, out var agencyType))
            {
                return Result<PublicSafetyAgencyActionResult>.Failure(new ApplicationError(
                    "PublicSafetyCommunication.PublicSafetyAgency.InvalidAgencyType",
                    $"'{request.AgencyType}' is not a recognised PublicSafetyAgencyType.",
                    Guid.NewGuid()));
            }

            var actorId = UserId.FromGuid(request.ActorId);

            var aggregate = PublicSafetyAgency.Create(
                createdBy: actorId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(actorId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                agencyName: request.AgencyName,
                agencyType: agencyType,
                jurisdictionDescription: request.JurisdictionDescription,
                primaryPhoneNumber: request.PrimaryPhoneNumber,
                primaryEmail: request.PrimaryEmail);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<PublicSafetyAgencyActionResult>.Success(new PublicSafetyAgencyActionResult(
                aggregate.PublicSafetyAgencyId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register PublicSafetyAgency {AgencyName}", request.AgencyName);

            return Result<PublicSafetyAgencyActionResult>.Failure(new ApplicationError(
                "PublicSafetyCommunication.PublicSafetyAgency.RegisterFailed",
                "Unable to register the public-safety agency at this time.",
                Guid.NewGuid()));
        }
    }
}
