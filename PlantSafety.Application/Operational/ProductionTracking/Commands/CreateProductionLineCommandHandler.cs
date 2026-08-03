// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Application
//  File:           CreateProductionLineCommandHandler.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Handler for CreateProductionLineCommand. Pure orchestration: constructs the
//                  ProductionLine aggregate via its domain factory method and persists it through
//                  the domain repository interface. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Domain.Operational.ProductionTracking.Interfaces;

namespace PlantSafety.Application.Operational.ProductionTracking.Commands;

/// <summary>Handles <see cref="CreateProductionLineCommand"/>.</summary>
public sealed class CreateProductionLineCommandHandler(IProductionLineRepository repository, ILogger<CreateProductionLineCommandHandler> logger)
    : IRequestHandler<CreateProductionLineCommand, Result<CreateProductionLineResult>>
{
    /// <inheritdoc/>
    public async Task<Result<CreateProductionLineResult>> Handle(CreateProductionLineCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var requesterId = UserId.FromGuid(request.RequestedByUserId);

            var aggregate = Domain.Operational.ProductionTracking.Aggregates.ProductionLine.Create(
                createdBy: requesterId,
                createdAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(requesterId),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                lineCode: request.LineCode,
                lineName: request.LineName);

            await repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateProductionLineResult>.Success(new CreateProductionLineResult(
                aggregate.ProductionLineId.Value,
                aggregate.LineCode));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create ProductionLine for Tenant {TenantId}", request.TenantId);

            return Result<CreateProductionLineResult>.Failure(new ApplicationError(
                "Operational.ProductionTracking.CreateLineFailed",
                "Unable to register the production line at this time.",
                Guid.NewGuid()));
        }
    }
}
