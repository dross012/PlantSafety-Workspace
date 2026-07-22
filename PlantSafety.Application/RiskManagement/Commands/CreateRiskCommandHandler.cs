// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// UPDATED: 2026-07-21 -- realigned with the real Risk.Create() hazard-identification signature.
// FILE DESCRIPTION: Handler for CreateRiskCommand. Pure orchestration: constructs the Risk aggregate via its domain factory method and persists it through the domain repository interface. No business rules live here.
// SchemaVersion: 2

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.RiskManagement.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.RiskManagement.Commands;

/// <summary>Handles <see cref="CreateRiskCommand"/>.</summary>
public sealed class CreateRiskCommandHandler : IRequestHandler<CreateRiskCommand, Result<CreateRiskResult>>
{
    private readonly IRiskRepository _repository;
    private readonly ILogger<CreateRiskCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateRiskCommandHandler(IRiskRepository repository, ILogger<CreateRiskCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateRiskResult>> Handle(CreateRiskCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var ownerId = UserId.FromGuid(request.OwnerId);

            var aggregate = PlantSafety.Domain.RiskManagement.Aggregates.Risk.Create(
                createdBy: ownerId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(ownerId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                riskTitle: request.RiskTitle,
                location: request.Location,
                hazardSource: request.HazardSource,
                ownerId: ownerId,
                reviewRequired: request.ReviewRequired);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateRiskResult>.Success(new CreateRiskResult(
                aggregate.RiskId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to identify Risk for Tenant {TenantId}", request.TenantId);

            return Result<CreateRiskResult>.Failure(new ApplicationError(
                "RiskManagement.Risk.CreateFailed",
                "Unable to create the risk record at this time.",
                Guid.NewGuid()));
        }
    }
}
