// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for CharacterizeWasteStreamCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Enums;
using PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Interfaces;
using PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Aggregates;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.HazardousWasteManagement.Commands;

/// <summary>Handles <see cref="CharacterizeWasteStreamCommand"/>.</summary>
public sealed class CharacterizeWasteStreamCommandHandler : IRequestHandler<CharacterizeWasteStreamCommand, Result<WasteStreamActionResult>>
{
    private readonly IWasteStreamRepository _repository;
    private readonly ILogger<CharacterizeWasteStreamCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CharacterizeWasteStreamCommandHandler(IWasteStreamRepository repository, ILogger<CharacterizeWasteStreamCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<WasteStreamActionResult>> Handle(CharacterizeWasteStreamCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var performedBy = UserId.FromGuid(request.PerformedByUserId);
            var now = UtcTimestamp.Now();

            if (!Enum.TryParse<WasteHazardClass>(request.HazardClass, ignoreCase: true, out var hazardClass))
                hazardClass = WasteHazardClass.NonHazardous;

            var aggregate = WasteStream.Create(
                performedBy, now,
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId), PerformedByActor.User(performedBy),
                request.TenantId, request.SiteId, request.StreamName, request.EpaWasteCode, hazardClass, request.GeneratorEpaId);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<WasteStreamActionResult>.Success(new WasteStreamActionResult(
                aggregate.WasteStreamId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute CharacterizeWasteStream for Tenant {TenantId}", request.TenantId);

            return Result<WasteStreamActionResult>.Failure(new ApplicationError(
                "HazardousWasteManagement.WasteStream.CharacterizeFailed",
                "Unable to characterize the waste stream at this time.",
                Guid.NewGuid()));
        }
    }
}
