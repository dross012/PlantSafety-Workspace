// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-26
// FILE DESCRIPTION: Handler for CreateCultureCommand. Builds a Culture aggregate, persists it via
//   ICultureRepository, and commits the Unit of Work. Handlers contain ONLY orchestration logic.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Interfaces.Persistence;
using PlantSafety.SharedKernel.Abstractions;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using CultureAggregate = PlantSafety.Domain.Analytics.Culture.Aggregates.Culture;

namespace PlantSafety.Application.Analytics.Culture.Commands;

/// <summary>Handles <see cref="CreateCultureCommand"/>.</summary>
public sealed class CreateCultureCommandHandler : IRequestHandler<CreateCultureCommand>
{
    private readonly ICultureRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxWriter _outboxWriter;
    private readonly ILogger<CreateCultureCommandHandler> _logger;

    /// <summary>Initializes dependencies.</summary>
    public CreateCultureCommandHandler(
        ICultureRepository repository,
        IUnitOfWork unitOfWork,
        IOutboxWriter outboxWriter,
        ILogger<CreateCultureCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _outboxWriter = outboxWriter;
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
        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Created Culture {CultureId} | TenantId={TenantId} | CorrelationId={CorrelationId}",
            culture.CultureId,
            request.TenantId,
            request.CorrelationId);
    }
}
