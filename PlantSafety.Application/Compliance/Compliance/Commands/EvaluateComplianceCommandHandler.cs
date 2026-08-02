// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Compliance
//  Layer:          Application
//  File:           EvaluateComplianceCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for EvaluateComplianceCommand. Mirrors ActivateComplianceCommandHandler's
//                  tenant-scoped lookup / NotFound-severity / concurrency-propagation conventions.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.Compliance.Interfaces;
using PlantSafety.Domain.Compliance.Compliance.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.Compliance.Commands;

/// <summary>Handles <see cref="EvaluateComplianceCommand"/>.</summary>
public sealed class EvaluateComplianceCommandHandler : IRequestHandler<EvaluateComplianceCommand, Result<ComplianceStatusResult>>
{
    private readonly IComplianceRepository _repository;
    private readonly ILogger<EvaluateComplianceCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public EvaluateComplianceCommandHandler(IComplianceRepository repository, ILogger<EvaluateComplianceCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ComplianceStatusResult>> Handle(EvaluateComplianceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = ComplianceId.FromGuid(request.ComplianceId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<ComplianceStatusResult>.Failure(new ApplicationError(
                    "Compliance.Compliance.NotFound",
                    "The requested compliance record could not be found.",
                    request.ComplianceId,
                    ErrorSeverity.NotFound));
            }

            var evaluatedBy = UserId.FromGuid(request.EvaluatedByUserId);

            aggregate.Evaluate(
                evaluatedBy: evaluatedBy,
                complianceScore: request.ComplianceScore,
                nonConformanceCount: request.NonConformanceCount,
                evaluatedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(evaluatedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<ComplianceStatusResult>.Success(new ComplianceStatusResult(
                aggregate.ComplianceId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to evaluate Compliance {ComplianceId}", request.ComplianceId);

            return Result<ComplianceStatusResult>.Failure(new ApplicationError(
                "Compliance.Compliance.EvaluateFailed",
                "Unable to evaluate the compliance record at this time.",
                Guid.NewGuid()));
        }
    }
}
