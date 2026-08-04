// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for GenerateManifestCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.HazardousWasteManagement.Manifests.Interfaces;
using PlantSafety.Domain.HazardousWasteManagement.Manifests.Aggregates;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.HazardousWasteManagement.Commands;

/// <summary>Handles <see cref="GenerateManifestCommand"/>.</summary>
public sealed class GenerateManifestCommandHandler : IRequestHandler<GenerateManifestCommand, Result<HazardousWasteManifestActionResult>>
{
    private readonly IHazardousWasteManifestRepository _repository;
    private readonly ILogger<GenerateManifestCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GenerateManifestCommandHandler(IHazardousWasteManifestRepository repository, ILogger<GenerateManifestCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HazardousWasteManifestActionResult>> Handle(GenerateManifestCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var performedBy = UserId.FromGuid(request.PerformedByUserId);
            var now = UtcTimestamp.Now();

            var aggregate = HazardousWasteManifest.Create(
                performedBy, now,
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId), PerformedByActor.User(performedBy),
                request.TenantId, request.SiteId, request.ManifestNumber, request.GeneratorEpaId, request.WasteStreamId);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<HazardousWasteManifestActionResult>.Success(new HazardousWasteManifestActionResult(
                aggregate.HazardousWasteManifestId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute GenerateManifest for Tenant {TenantId}", request.TenantId);

            return Result<HazardousWasteManifestActionResult>.Failure(new ApplicationError(
                "HazardousWasteManagement.Manifest.GenerateFailed",
                "Unable to generate the manifest at this time.",
                Guid.NewGuid()));
        }
    }
}
