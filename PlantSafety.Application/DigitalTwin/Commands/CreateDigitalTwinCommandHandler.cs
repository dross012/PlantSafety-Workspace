// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin
//  Layer:          Application
//  File:           CreateDigitalTwinCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CreateDigitalTwinCommand. Pure orchestration: constructs the
//                  DigitalTwin aggregate via its domain factory method and persists it through the
//                  domain repository interface. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.DigitalTwin.Repositories;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.DigitalTwin.Commands;

/// <summary>Handles <see cref="CreateDigitalTwinCommand"/>.</summary>
public sealed class CreateDigitalTwinCommandHandler : IRequestHandler<CreateDigitalTwinCommand, Result<CreateDigitalTwinResult>>
{
    private readonly IDigitalTwinRepository _repository;
    private readonly ILogger<CreateDigitalTwinCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateDigitalTwinCommandHandler(IDigitalTwinRepository repository, ILogger<CreateDigitalTwinCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateDigitalTwinResult>> Handle(CreateDigitalTwinCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var requesterId = UserId.FromGuid(request.RequesterUserId);

            var aggregate = PlantSafety.Domain.DigitalTwin.Aggregates.DigitalTwin.Create(
                createdBy: requesterId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                twinName: request.TwinName,
                assetReferenceId: request.AssetReferenceId,
                modelType: request.ModelType);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateDigitalTwinResult>.Success(new CreateDigitalTwinResult(
                aggregate.DigitalTwinId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create DigitalTwin for Tenant {TenantId}", request.TenantId);

            return Result<CreateDigitalTwinResult>.Failure(new ApplicationError(
                "DigitalTwin.CreateFailed",
                "Unable to register the digital-twin record at this time.",
                Guid.NewGuid()));
        }
    }
}
