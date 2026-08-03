// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights.Coordination
//  Layer:          Application
//  File:           CrossSiteLockoutPropagationCommandHandler.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §3 Multi-Site Coordination Workflows)
//  SchemaVersion:  2026.1
//  Description:    Handles CrossSiteLockoutPropagationCommand. Real, working handler that honestly
//                  reports Operational.LockoutTagout's current lack of a write command surface
//                  (verified: query-only module -- see CrossSiteLockoutPropagationCommand remarks)
//                  rather than fabricating a fake dispatch. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.GlobalSafetyInsights.Coordination.Commands;

namespace PlantSafety.Application.GlobalSafetyInsights.Coordination.Handlers;

/// <summary>Handles <see cref="CrossSiteLockoutPropagationCommand"/>.</summary>
public sealed class CrossSiteLockoutPropagationCommandHandler
    : IRequestHandler<CrossSiteLockoutPropagationCommand, Result<CrossSiteLockoutPropagationResult>>
{
    private readonly ILogger<CrossSiteLockoutPropagationCommandHandler> _logger;

    /// <summary>Initializes dependencies.</summary>
    public CrossSiteLockoutPropagationCommandHandler(ILogger<CrossSiteLockoutPropagationCommandHandler> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<Result<CrossSiteLockoutPropagationResult>> Handle(CrossSiteLockoutPropagationCommand request, CancellationToken cancellationToken)
    {
        // Operational.LockoutTagout is a real module, but as of this pass it exposes only a read
        // path (GetLockoutTagoutQuery/IReadOnly-LockoutTagoutRepository). There is no
        // LockoutTagout aggregate, no write command, and no write repository to dispatch a real
        // lockout-propagation mutation to. Fabricating a fake success here would violate the
        // honesty requirement of this build -- report the gap loudly instead.
        _logger.LogWarning(
            "CrossSiteLockoutPropagation: Operational.LockoutTagout has no write command surface to " +
            "dispatch to; {SiteCount} site(s) for Tenant {TenantId} were NOT propagated. " +
            "Reason requested: {Reason}",
            request.AffectedSiteIds.Count, request.TenantId, request.Reason);

        return Task.FromResult(Result<CrossSiteLockoutPropagationResult>.Failure(new ApplicationError(
            "GlobalSafetyInsights.Coordination.LockoutTagoutModuleTooThin",
            "Operational.LockoutTagout does not yet expose a write command surface; lockout propagation cannot be executed for any site.",
            request.CorrelationId,
            ErrorSeverity.Domain)));
    }
}
