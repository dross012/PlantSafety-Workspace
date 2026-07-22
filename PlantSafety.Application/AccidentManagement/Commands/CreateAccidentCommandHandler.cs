// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// UPDATED: 2026-07-21 -- realigned with the real Accident.Create() report-time signature.
// FILE DESCRIPTION: Handler for CreateAccidentCommand. Pure orchestration: constructs the Accident aggregate via its domain factory method and persists it through the domain repository interface. No business rules live here.
// SchemaVersion: 2

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.AccidentManagement.Enums;
using PlantSafety.Domain.AccidentManagement.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.AccidentManagement.Commands;

/// <summary>Handles <see cref="CreateAccidentCommand"/>.</summary>
public sealed class CreateAccidentCommandHandler : IRequestHandler<CreateAccidentCommand, Result<CreateAccidentResult>>
{
    private readonly IAccidentRepository _repository;
    private readonly ILogger<CreateAccidentCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateAccidentCommandHandler(IAccidentRepository repository, ILogger<CreateAccidentCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateAccidentResult>> Handle(CreateAccidentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var ownerId = UserId.FromGuid(request.OwnerId);
            var category = InvestigationPhase.IsValid(request.Category) ? InvestigationPhase.From(request.Category) : InvestigationPhase.Default;
            var severity = AccidentSeverity.IsValid(request.Severity) ? AccidentSeverity.From(request.Severity) : AccidentSeverity.Default;

            var aggregate = PlantSafety.Domain.AccidentManagement.Aggregates.Accident.Create(
                createdBy: ownerId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(ownerId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                accidentTitle: request.AccidentTitle,
                location: request.Location,
                severity: severity,
                injuredPersonCount: request.InjuredPersonCount,
                occurredAtUtc: UtcTimestamp.From(request.OccurredAtUtc),
                category: category,
                ownerId: ownerId,
                reviewRequired: request.ReviewRequired);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateAccidentResult>.Success(new CreateAccidentResult(
                aggregate.AccidentId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to report Accident for Tenant {TenantId}", request.TenantId);

            return Result<CreateAccidentResult>.Failure(new ApplicationError(
                "AccidentManagement.Accident.CreateFailed",
                "Unable to create the accident record at this time.",
                Guid.NewGuid()));
        }
    }
}
