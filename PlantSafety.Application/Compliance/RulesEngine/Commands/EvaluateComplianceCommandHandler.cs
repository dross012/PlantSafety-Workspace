// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Handler for EvaluateComplianceCommand. Orchestrates a full
//   ComplianceEvaluator run, persists the resulting ComplianceEvaluation aggregate (including its
//   domain events -- ComplianceViolationDetectedEvent per failing rule, plus
//   ComplianceEvaluationCompletedEvent -- flushed via the repository's outbox wiring), and maps
//   the result to a DTO. No business rules live here.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.RulesEngine.Enums;
using PlantSafety.Domain.Compliance.RulesEngine.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.RulesEngine.Evaluator;
using PlantSafety.Application.Compliance.RulesEngine.Mapping;
using PlantSafety.Application.Compliance.RulesEngine.Models;
using PlantSafety.Application.Compliance.RulesEngine.Rules;

namespace PlantSafety.Application.Compliance.RulesEngine.Commands;

/// <summary>Handles <see cref="EvaluateComplianceCommand"/>.</summary>
public sealed class EvaluateComplianceCommandHandler : IRequestHandler<EvaluateComplianceCommand, Result<ComplianceEvaluationDto>>
{
    private readonly ComplianceEvaluator _evaluator;
    private readonly IComplianceEvaluationRepository _repository;
    private readonly ILogger<EvaluateComplianceCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public EvaluateComplianceCommandHandler(
        ComplianceEvaluator evaluator, IComplianceEvaluationRepository repository, ILogger<EvaluateComplianceCommandHandler> logger)
    {
        _evaluator = evaluator;
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ComplianceEvaluationDto>> Handle(EvaluateComplianceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var context = new ComplianceRuleContext(
                request.TenantId, request.SiteId, request.TargetEntityType, request.TargetEntityId,
                UtcTimestamp.Now().Value, request.Facts);

            var categories = request.Categories?
                .Where(ComplianceRuleCategory.IsValid)
                .Select(ComplianceRuleCategory.From)
                .ToList();

            var evaluation = _evaluator.Evaluate(
                context,
                UserId.FromGuid(request.TriggeredBy),
                PerformedByActor.User(UserId.FromGuid(request.TriggeredBy)),
                new CorrelationId(request.CorrelationId),
                new CausationId(request.CausationId),
                categories);

            await _repository.AddAsync(evaluation, cancellationToken);

            return Result<ComplianceEvaluationDto>.Success(ComplianceEvaluationMapper.ToDto(evaluation));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to evaluate compliance for {TargetEntityType} {TargetEntityId} Tenant {TenantId}",
                request.TargetEntityType, request.TargetEntityId, request.TenantId);

            return Result<ComplianceEvaluationDto>.Failure(new ApplicationError(
                "Compliance.RulesEngine.EvaluationFailed",
                "Unable to evaluate compliance for the requested entity at this time.",
                Guid.NewGuid()));
        }
    }
}
