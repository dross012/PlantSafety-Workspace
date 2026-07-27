// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-26
// UPDATED: 2026-07-27 -- removed the manual PlantSafety.SharedKernel.Abstractions.IUnitOfWork and
//   PlantSafety.Application.Common.Interfaces.Persistence.IOutboxWriter dependencies. Both were
//   orphaned, single-consumer abstractions with no Infrastructure implementation (the same kind of
//   drift that left ICultureRepository unimplemented) and duplicated work the platform already does
//   centrally: TransactionBehavior<,> (via Application.Behaviors.Transactions.IUnitOfWork) wraps
//   every ICommand and flushes the scoped Infrastructure IOutboxWriter after the handler returns,
//   and CultureRepository.AddAsync enqueues the aggregate's domain events into that same writer --
//   see AccidentDapperRepository/CreateAccidentCommandHandler for the identical, already-working
//   pattern used by every other module.
// FILE DESCRIPTION: Handler for CreateCultureCommand. Builds a Culture aggregate and persists it via
//   ICultureRepository. Handlers contain ONLY orchestration logic.
// SchemaVersion: 2

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Interfaces.Persistence;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using CultureAggregate = PlantSafety.Domain.Analytics.Culture.Aggregates.Culture;

namespace PlantSafety.Application.Analytics.Culture.Commands;

/// <summary>Handles <see cref="CreateCultureCommand"/>.</summary>
public sealed class CreateCultureCommandHandler : IRequestHandler<CreateCultureCommand>
{
    private readonly ICultureRepository _repository;
    private readonly ILogger<CreateCultureCommandHandler> _logger;

    /// <summary>Initializes dependencies.</summary>
    public CreateCultureCommandHandler(
        ICultureRepository repository,
        ILogger<CreateCultureCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task Handle(CreateCultureCommand request, CancellationToken cancellationToken)
    {
        var culture = CultureAggregate.Create(
            createdBy:      request.UserId,
            createdAtUtc:   UtcTimestamp.From(DateTime.UtcNow),
            correlationId:  request.CorrelationId,
            causationId:    request.CausationId,
            actor:          request.Actor,
            tenantId:       request.TenantId.Value);

        await _repository.AddAsync(culture, cancellationToken);

        _logger.LogInformation(
            "Created Culture {CultureId} | TenantId={TenantId} | CorrelationId={CorrelationId}",
            culture.CultureId,
            request.TenantId,
            request.CorrelationId);
    }
}
