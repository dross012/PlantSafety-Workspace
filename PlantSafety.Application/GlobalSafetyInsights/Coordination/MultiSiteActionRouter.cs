// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights.Coordination
//  Layer:          Application
//  File:           MultiSiteActionRouter.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §3 Multi-Site Coordination Workflows)
//  SchemaVersion:  2026.1
//  Description:    Real orchestration point for the Multi-Site Coordination workflow:
//                  1. Validates tenant/region/residency (real ITenantRegionRouter/
//                     IDataResidencyEnforcer, Phase 7 pattern) -- reject/flag on violation.
//                  2. Applies IMultiSiteSafetyPolicy to determine the actual eligible site set and
//                     whether approval is required.
//                  3. Either starts a real Phase 5 Workflow WorkflowInstance against a tenant's
//                     configured "MultiSiteCoordinationApproval" WorkflowDefinition (when approval
//                     is required and one is configured) -- reusing the Workflow module's real
//                     HumanApproval-step/approval-chain infrastructure rather than building a
//                     second, parallel approval system -- or routes the command to each eligible
//                     site's target module (the 4 CrossSite*Command types) when no approval is
//                     required.
//                  4. Emits CrossSiteActionEvent (already modeled, Phase 8 §4) via
//                     ICrossSiteActionEventPublisher for every routed/pending action, and calls the
//                     existing RecordCrossSiteActionCommand to persist into the module's audit
//                     trail.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Enterprise.MultiRegion;
using PlantSafety.Application.Enterprise.MultiRegion.DataResidency;
using PlantSafety.Application.GlobalSafetyInsights.Commands;
using PlantSafety.Application.GlobalSafetyInsights.Coordination.Commands;
using PlantSafety.Application.GlobalSafetyInsights.Coordination.Models;
using PlantSafety.Application.GlobalSafetyInsights.Coordination.Publishers;
using PlantSafety.Domain.GlobalSafetyInsights.Coordination.Enums;
using PlantSafety.Domain.GlobalSafetyInsights.Coordination.Policies;
using PlantSafety.Domain.GlobalSafetyInsights.Events;
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects.Ids;
using PlantSafety.SharedKernel.Contracts.Enterprise.MultiRegion;
using PlantSafety.Domain.MultiRegion.DataResidency;
using PlantSafety.Domain.Workflow.Enums;
using PlantSafety.Domain.Workflow.Interfaces;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Application.GlobalSafetyInsights.Coordination;

/// <summary>Real orchestration point for the Multi-Site Coordination workflow: validate, decide, route, audit.</summary>
public interface IMultiSiteActionRouter
{
    /// <summary>Validates, decides eligibility/approval, and routes (or defers pending approval) a coordinated action.</summary>
    Task<Result<MultiSiteActionRouteResult>> RouteAsync(MultiSiteActionRequest request, CancellationToken cancellationToken);
}

/// <inheritdoc cref="IMultiSiteActionRouter"/>
public sealed class MultiSiteActionRouter(
    ITenantRegionRouter regionRouter,
    IDataResidencyEnforcer residencyEnforcer,
    IMultiSiteSafetyPolicy policy,
    ISender sender,
    ICrossSiteActionEventPublisher crossSiteActionEventPublisher,
    IWorkflowDefinitionRepository workflowDefinitionRepository,
    IWorkflowInstanceFactory workflowInstanceFactory,
    IWorkflowInstanceRepository workflowInstanceRepository,
    ILogger<MultiSiteActionRouter> logger)
    : IMultiSiteActionRouter
{
    /// <summary>
    /// Well-known name of the tenant-configured WorkflowDefinition (WorkflowType.Approval) used to
    /// gate cross-site coordinated actions that require approval. A tenant that has not configured
    /// a WorkflowDefinition with this name has no approval chain available -- see remarks on
    /// <see cref="RouteAsync"/> for the honest fallback behavior in that case.
    /// </summary>
    public const string ApprovalWorkflowDefinitionName = "MultiSiteCoordinationApproval";

    /// <summary>Well-known system-actor UserId used when no human initiated the coordinated action.</summary>
    public static readonly Guid SystemUserId = Guid.Parse("00000000-0000-0000-0000-0000005a4553");

    /// <inheritdoc/>
    public async Task<Result<MultiSiteActionRouteResult>> RouteAsync(MultiSiteActionRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var correlationId = CorrelationId.FromGuid(request.CorrelationId == Guid.Empty ? Guid.NewGuid() : request.CorrelationId);
        var causationId = CausationId.FromGuid(request.CausationId == Guid.Empty ? correlationId.Value : request.CausationId);

        // Step 1: tenant/region/residency validation (Phase 7 pattern, mirrored exactly).
        try
        {
            residencyEnforcer.EnsureSatisfied(request.TenantId, correlationId, causationId);
        }
        catch (DataResidencyViolationException ex)
        {
            logger.LogWarning(ex, "MultiSiteActionRouter: residency violation for Tenant {TenantId}; action rejected.", request.TenantId);
            return Result<MultiSiteActionRouteResult>.Failure(new ApplicationError(
                "GlobalSafetyInsights.Coordination.ResidencyViolation",
                "The coordinated action was rejected because it would violate the tenant's data-residency policy.",
                request.CorrelationId,
                ErrorSeverity.Domain));
        }

        var region = regionRouter.ResolveRegion(request.TenantId);

        // Step 2: eligibility + approval-chain decision.
        var decision = policy.Evaluate(request.TenantId, request.CandidateSiteIds, request.ActionType, request.TenantExcludedSiteIds);

        if (!decision.IsEligible)
        {
            return Result<MultiSiteActionRouteResult>.Failure(new ApplicationError(
                "GlobalSafetyInsights.Coordination.NoEligibleSites",
                "No candidate site is eligible to participate in this coordinated action.",
                request.CorrelationId,
                ErrorSeverity.Validation));
        }

        var actorId = request.InitiatedByUserId ?? SystemUserId;
        var nowUtc = UtcTimestamp.Now();
        var actor = request.InitiatedByUserId.HasValue ? PerformedByActor.User(UserId.FromGuid(actorId)) : PerformedByActor.System();

        // Step 5: approval required -- defer execution.
        if (decision.RequiresApproval)
        {
            var workflowInstanceId = await TryStartApprovalWorkflowAsync(request, decision, actorId, nowUtc, correlationId, causationId, actor, cancellationToken);

            await AuditAsync(request, decision.EligibleSiteIds, actorId, correlationId, causationId, region,
                actionTypeSuffix: workflowInstanceId is not null ? "PendingApproval" : "PendingApprovalNoWorkflowDefinition",
                workflowInstanceId, cancellationToken);

            return Result<MultiSiteActionRouteResult>.Success(new MultiSiteActionRouteResult(
                decision.EligibleSiteIds, decision.IneligibleSites, RequiresApproval: true, Executed: false,
                DispatchSucceeded: false, DispatchErrorMessage: null, WorkflowInstanceId: workflowInstanceId));
        }

        // Step 3: route to the real target module's command(s).
        var (dispatchSucceeded, dispatchError) = await DispatchAsync(request, decision.EligibleSiteIds, actorId, cancellationToken);

        // Step 4: audit + event mesh.
        await AuditAsync(request, decision.EligibleSiteIds, actorId, correlationId, causationId, region, actionTypeSuffix: null, workflowInstanceId: null, cancellationToken);

        return Result<MultiSiteActionRouteResult>.Success(new MultiSiteActionRouteResult(
            decision.EligibleSiteIds, decision.IneligibleSites, RequiresApproval: false, Executed: true,
            DispatchSucceeded: dispatchSucceeded, DispatchErrorMessage: dispatchError, WorkflowInstanceId: null));
    }

    private async Task<Guid?> TryStartApprovalWorkflowAsync(
        MultiSiteActionRequest request,
        MultiSiteCoordinationDecision decision,
        Guid actorId,
        UtcTimestamp nowUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        CancellationToken cancellationToken)
    {
        var definitions = await workflowDefinitionRepository.ListActiveAsync(request.TenantId, cancellationToken);
        var definition = definitions.FirstOrDefault(d =>
            d.WorkflowType.Equals(WorkflowType.Approval) &&
            string.Equals(d.Name, ApprovalWorkflowDefinitionName, StringComparison.OrdinalIgnoreCase));

        if (definition is null)
        {
            logger.LogWarning(
                "MultiSiteActionRouter: Tenant {TenantId} has no active '{DefinitionName}' approval WorkflowDefinition configured; " +
                "'{ActionType}' affecting {SiteCount} site(s) is recorded as pending-approval without an executable approval chain.",
                request.TenantId, ApprovalWorkflowDefinitionName, request.ActionType, decision.EligibleSiteIds.Count);
            return null;
        }

        var startStep = definition.GetStartStep();

        var instance = workflowInstanceFactory.Start(
            definition,
            UserId.FromGuid(actorId),
            nowUtc,
            correlationId,
            causationId,
            actor,
            request.TenantId,
            siteId: null,
            initialContextData: new Dictionary<string, string>
            {
                ["ActionType"] = request.ActionType.ToString(),
                ["AffectedSiteIds"] = string.Join(",", decision.EligibleSiteIds),
                ["Description"] = request.Description,
                ["ApprovalReason"] = decision.ApprovalReason ?? string.Empty,
            });

        await workflowInstanceRepository.AddAsync(instance, cancellationToken);

        logger.LogInformation(
            "MultiSiteActionRouter: started approval WorkflowInstance {WorkflowInstanceId} (step '{StepName}') for '{ActionType}' " +
            "affecting {SiteCount} site(s), Tenant {TenantId}.",
            instance.WorkflowInstanceId.Value, startStep.Name, request.ActionType, decision.EligibleSiteIds.Count, request.TenantId);

        return instance.WorkflowInstanceId.Value;
    }

    private async Task<(bool Succeeded, string? ErrorMessage)> DispatchAsync(
        MultiSiteActionRequest request, IReadOnlyList<Guid> eligibleSiteIds, Guid actorId, CancellationToken cancellationToken)
    {
        switch (request.ActionType)
        {
            case CrossSiteActionType.IncidentEscalation:
            {
                var result = await sender.Send(new CrossSiteIncidentEscalationCommand(
                    request.TenantId, eligibleSiteIds, request.IncidentEscalationReason ?? request.Description,
                    request.InitiatedByUserId, request.CorrelationId, request.CausationId), cancellationToken);

                return result.IsSuccess ? (true, null) : (false, result.Error.Message);
            }

            case CrossSiteActionType.CorrectiveActionRollout:
            {
                var payload = request.CorrectiveAction
                    ?? throw new InvalidOperationException("CorrectiveActionRollout requires a CorrectiveAction payload.");

                var result = await sender.Send(new CrossSiteCorrectiveActionCommand(
                    request.TenantId, eligibleSiteIds, payload.Title, request.Description, payload.Priority,
                    payload.SourceEventId, payload.SourceEventType, payload.DueAtUtc, payload.AssignedToWorkerId,
                    request.InitiatedByUserId, request.CorrelationId, request.CausationId), cancellationToken);

                return result.IsSuccess ? (true, null) : (false, result.Error.Message);
            }

            case CrossSiteActionType.LockoutPropagation:
            {
                var result = await sender.Send(new CrossSiteLockoutPropagationCommand(
                    request.TenantId, eligibleSiteIds, request.LockoutPropagationReason ?? request.Description,
                    request.InitiatedByUserId, request.CorrelationId, request.CausationId), cancellationToken);

                return result.IsSuccess ? (true, null) : (false, result.Error.Message);
            }

            case CrossSiteActionType.MaintenanceCoordination:
            {
                var payload = request.MaintenanceCoordination
                    ?? throw new InvalidOperationException("MaintenanceCoordination requires a MaintenanceCoordination payload.");

                var result = await sender.Send(new CrossSiteMaintenanceCoordinationCommand(
                    request.TenantId, eligibleSiteIds, payload.MachineId, payload.MachineCode, payload.Priority,
                    request.Description, payload.DueAtUtc, request.InitiatedByUserId, request.CorrelationId, request.CausationId), cancellationToken);

                return result.IsSuccess ? (true, null) : (false, result.Error.Message);
            }

            default:
                throw new ArgumentOutOfRangeException(nameof(request), request.ActionType, "Unknown CrossSiteActionType.");
        }
    }

    private async Task AuditAsync(
        MultiSiteActionRequest request,
        IReadOnlyList<Guid> eligibleSiteIds,
        Guid actorId,
        CorrelationId correlationId,
        CausationId causationId,
        Region region,
        string? actionTypeSuffix,
        Guid? workflowInstanceId,
        CancellationToken cancellationToken)
    {
        var actionType = actionTypeSuffix is null ? request.ActionType.ToString() : $"{request.ActionType}.{actionTypeSuffix}";
        var primarySiteId = eligibleSiteIds.Count > 0 ? eligibleSiteIds[0] : Guid.Empty;

        var evt = new CrossSiteActionEvent(
            GlobalSafetyInsightsEventId.New(), request.TenantId, region.Code, primarySiteId, eligibleSiteIds,
            actionType, request.Description, request.InitiatedByUserId, UtcTimestamp.Now(),
            request.InitiatedByUserId.HasValue ? PerformedByActor.User(UserId.FromGuid(actorId)) : PerformedByActor.System(),
            correlationId, causationId, workflowInstanceId);

        // The idempotent CrossSiteActionConsumer (Phase 8 §4) is the sole authoritative writer of
        // the audit ledger -- it dispatches RecordCrossSiteActionCommand itself once this event is
        // consumed. A prior version of this method ALSO dispatched RecordCrossSiteActionCommand
        // synchronously here, producing two audit-log rows per action with no shared idempotency
        // key between the two write paths (Phase 8 §6 governance-validation finding). Publish only.
        await crossSiteActionEventPublisher.PublishAsync(evt, cancellationToken);
    }
}
