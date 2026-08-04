// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for ValidateSafetyCaseCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.AutonomousVehicleSafety.SafetyCaseValidation.Interfaces;
using PlantSafety.Domain.AutonomousVehicleSafety.SafetyCaseValidation.Enums;
using Agg = PlantSafety.Domain.AutonomousVehicleSafety.SafetyCaseValidation.Aggregates.SafetyCaseValidation;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.AutonomousVehicleSafety.Commands;

/// <summary>Handles <see cref="ValidateSafetyCaseCommand"/>.</summary>
public sealed class ValidateSafetyCaseCommandHandler : IRequestHandler<ValidateSafetyCaseCommand, Result<SafetyCaseValidationActionResult>>
{
    private readonly ISafetyCaseValidationRepository _repository;
    private readonly ILogger<ValidateSafetyCaseCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ValidateSafetyCaseCommandHandler(ISafetyCaseValidationRepository repository, ILogger<ValidateSafetyCaseCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SafetyCaseValidationActionResult>> Handle(ValidateSafetyCaseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var performedBy = UserId.FromGuid(request.PerformedByUserId);
            var validatedBy = UserId.FromGuid(request.ValidatedByUserId);
            var now = UtcTimestamp.From(request.RequestedAtUtc);
            var correlationId = new CorrelationId(request.CorrelationId);
            var causationId = new CausationId(request.CausationId);
            var actor = PerformedByActor.User(performedBy);

            if (!Enum.TryParse<SafetyStandardReference>(request.StandardReference, ignoreCase: true, out var standardReference))
                standardReference = SafetyStandardReference.AnsiRia1508;

            var aggregate = Agg.Create(
                createdBy: performedBy,
                createdAtUtc: now,
                correlationId: correlationId,
                causationId: causationId,
                actor: actor,
                tenantId: request.TenantId,
                siteId: request.SiteId,
                autonomousVehicleAssetId: request.AutonomousVehicleAssetId,
                standardReference: standardReference,
                documentVersion: request.DocumentVersion,
                revalidationCadenceDays: request.RevalidationCadenceDays);

            aggregate.Validate(validatedBy, now, actor, correlationId, causationId);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<SafetyCaseValidationActionResult>.Success(new SafetyCaseValidationActionResult(
                aggregate.SafetyCaseValidationId.Value, aggregate.Status.ToString(), aggregate.ExpiresAtUtc?.Value));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate SafetyCaseValidation for Asset {AutonomousVehicleAssetId}, Tenant {TenantId}", request.AutonomousVehicleAssetId, request.TenantId);

            return Result<SafetyCaseValidationActionResult>.Failure(new ApplicationError(
                "AutonomousVehicleSafety.SafetyCaseValidation.ValidateFailed",
                "Unable to validate the safety case at this time.",
                Guid.NewGuid()));
        }
    }
}
