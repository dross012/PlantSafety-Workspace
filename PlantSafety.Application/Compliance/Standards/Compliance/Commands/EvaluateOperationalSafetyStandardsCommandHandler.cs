// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Standards.Compliance.OperationalSafety
//  Layer:          Application
//  File:           EvaluateOperationalSafetyStandardsCommandHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for EvaluateOperationalSafetyStandardsCommand. Loads real
//                  ConfinedSpaceItemDto permit records for the tenant/site via the existing
//                  IConfinedSpaceRepository, projects them into the Domain engine's
//                  ConfinedSpacePermitSnapshot, combines that with the caller-supplied
//                  SafetyManagementSystem/SurfaceHazards data, invokes
//                  OperationalSafetyStandardsEngine.EvaluateCompliance(...), and translates the
//                  Domain-layer Result&lt;StandardsComplianceReport&gt; into the Application-layer
//                  Result&lt;StandardsComplianceReportDto&gt;.
//
//                  ASSUMPTION (documented per Handbook ambiguity-resolution rule): IConfinedSpace
//                  Repository.GetConfinedSpaceAsync requires an explicit [from, to] UTC date
//                  range and this command does not carry one (ANSI Z117 evaluation cares about
//                  currently-active/recently-expired permits, not an arbitrary caller-chosen
//                  reporting window). This handler queries a fixed 2-year-back/2-year-forward
//                  window from the evaluation instant, wide enough to include any permit that
//                  could plausibly still be "active-looking" or a recently-expired violation, per
//                  the engine's own IsActiveLookingStatus/expiry checks.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.Standards.OperationalSafety.Services;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Permits.ConfinedSpace.Repositories;
using PlantSafety.Application.Compliance.Standards.Compliance.Models;

namespace PlantSafety.Application.Compliance.Standards.Compliance.Commands;

/// <summary>Handles <see cref="EvaluateOperationalSafetyStandardsCommand"/>.</summary>
public sealed class EvaluateOperationalSafetyStandardsCommandHandler : IRequestHandler<EvaluateOperationalSafetyStandardsCommand, Result<StandardsComplianceReportDto>>
{
    private static readonly OperationalSafetyStandardsEngine Engine = new();

    /// <summary>Width of the confined-space permit query window, applied both backward and forward from the evaluation instant -- see file header ASSUMPTION.</summary>
    private static readonly TimeSpan ConfinedSpaceQueryWindow = TimeSpan.FromDays(730);

    private readonly IConfinedSpaceRepository _confinedSpaceRepository;
    private readonly ILogger<EvaluateOperationalSafetyStandardsCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public EvaluateOperationalSafetyStandardsCommandHandler(
        IConfinedSpaceRepository confinedSpaceRepository,
        ILogger<EvaluateOperationalSafetyStandardsCommandHandler> logger)
    {
        _confinedSpaceRepository = confinedSpaceRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<StandardsComplianceReportDto>> Handle(EvaluateOperationalSafetyStandardsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var evaluatedAtUtc = DateTime.UtcNow;

            var permits = await _confinedSpaceRepository.GetConfinedSpaceAsync(
                request.TenantId,
                request.SiteId,
                evaluatedAtUtc - ConfinedSpaceQueryWindow,
                evaluatedAtUtc + ConfinedSpaceQueryWindow,
                cancellationToken);

            var permitSnapshots = permits.Select(p => new ConfinedSpacePermitSnapshot(
                PermitId: p.PermitId,
                SpaceIdentifier: p.SpaceIdentifier,
                AttendantName: p.AttendantName,
                Status: p.Status,
                IssuedAtUtc: UtcTimestamp.From(p.IssuedAtUtc),
                ExpiresAtUtc: UtcTimestamp.From(p.ExpiresAtUtc))).ToList();

            var smsSnapshot = request.SafetyManagementSystem is null
                ? null
                : new SafetyManagementSystemSnapshot(
                    HasDocumentedPolicy: request.SafetyManagementSystem.HasDocumentedPolicy,
                    HasPlanningProcess: request.SafetyManagementSystem.HasPlanningProcess,
                    HasImplementationSupport: request.SafetyManagementSystem.HasImplementationSupport,
                    HasPerformanceEvaluation: request.SafetyManagementSystem.HasPerformanceEvaluation,
                    HasContinualImprovementProcess: request.SafetyManagementSystem.HasContinualImprovementProcess);

            var surfaceHazards = request.SurfaceHazards
                .Select(h => new WalkingWorkingSurfaceHazardRecord(h.Location, h.HazardDescription, h.Corrected))
                .ToList();

            var input = new OperationalSafetyStandardsInput(
                SafetyManagementSystem: smsSnapshot,
                SurfaceHazards: surfaceHazards,
                ConfinedSpacePermits: permitSnapshots,
                EvaluatedAtUtc: UtcTimestamp.From(evaluatedAtUtc));

            var evaluation = Engine.EvaluateCompliance(input);

            if (!evaluation.IsSuccess)
            {
                return Result<StandardsComplianceReportDto>.Failure(new ApplicationError(
                    "Compliance.Standards.OperationalSafetyEvaluationFailed",
                    evaluation.Error ?? "Unable to evaluate operational-safety-standards compliance for the requested tenant/site.",
                    Guid.NewGuid()));
            }

            return Result<StandardsComplianceReportDto>.Success(StandardsComplianceReportMapper.ToDto(evaluation.Value!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to evaluate operational-safety-standards compliance for Tenant {TenantId}, Site {SiteId}", request.TenantId, request.SiteId);

            return Result<StandardsComplianceReportDto>.Failure(new ApplicationError(
                "Compliance.Standards.OperationalSafetyEvaluationFailed",
                "Unable to evaluate operational-safety-standards compliance at this time.",
                Guid.NewGuid()));
        }
    }
}
