// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for OpenAccumulationContainerCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Interfaces;
using PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Aggregates;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.HazardousWasteManagement.Commands;

/// <summary>Handles <see cref="OpenAccumulationContainerCommand"/>.</summary>
public sealed class OpenAccumulationContainerCommandHandler : IRequestHandler<OpenAccumulationContainerCommand, Result<WasteContainerActionResult>>
{
    private readonly IWasteContainerRepository _repository;
    private readonly ILogger<OpenAccumulationContainerCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public OpenAccumulationContainerCommandHandler(IWasteContainerRepository repository, ILogger<OpenAccumulationContainerCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<WasteContainerActionResult>> Handle(OpenAccumulationContainerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var performedBy = UserId.FromGuid(request.PerformedByUserId);
            var now = UtcTimestamp.Now();

            var aggregate = WasteContainer.Create(
                performedBy, now,
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId), PerformedByActor.User(performedBy),
                request.TenantId, request.SiteId, request.AccumulationAreaId, request.WasteStreamId,
                request.ContainerLabel, request.CapacityGallons, request.RegulatoryTimeLimitDays);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<WasteContainerActionResult>.Success(new WasteContainerActionResult(
                aggregate.WasteContainerId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute OpenAccumulationContainer for Tenant {TenantId}", request.TenantId);

            return Result<WasteContainerActionResult>.Failure(new ApplicationError(
                "HazardousWasteManagement.WasteContainer.OpenFailed",
                "Unable to open the accumulation container at this time.",
                Guid.NewGuid()));
        }
    }
}
