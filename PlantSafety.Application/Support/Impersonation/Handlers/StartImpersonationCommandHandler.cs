// AUTHOR: PlantSafety Platform Team (Phase 7 §6.3 Support Tooling)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for StartImpersonationCommand. Verifies the target tenant exists,
//   creates a hard-expiring session record in the existing distributed cache (ICacheService --
//   Redis-backed, already registered platform-wide), and writes a real audit record via
//   IPermissionUsageLogRepository. The session's cache entry itself expires at the time-box
//   boundary, so an impersonation session can never silently persist past its bound even if
//   EndImpersonationCommand is never called.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Caching;
using PlantSafety.Application.Auditing.ComplianceMonitoring.PermissionUsage.Models;
using PlantSafety.Application.Auditing.ComplianceMonitoring.PermissionUsage.Repositories;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Support.Impersonation.Commands;
using PlantSafety.Application.Support.Impersonation.Models;
using PlantSafety.Application.Tenancy.Tenant.Repositories;

namespace PlantSafety.Application.Support.Impersonation.Handlers;

/// <summary>Handles <see cref="StartImpersonationCommand"/>.</summary>
public sealed class StartImpersonationCommandHandler(
    ITenantReadRepository tenantReadRepository,
    ICacheService cacheService,
    IPermissionUsageLogRepository permissionUsageLogRepository,
    ILogger<StartImpersonationCommandHandler> logger)
    : IRequestHandler<StartImpersonationCommand, Result<ImpersonationSessionDto>>
{
    /// <summary>The hard time-box every impersonation session is bound to, regardless of operator activity.</summary>
    public static readonly TimeSpan SessionTimeBox = TimeSpan.FromMinutes(30);

    /// <inheritdoc/>
    public async Task<Result<ImpersonationSessionDto>> Handle(StartImpersonationCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return Result<ImpersonationSessionDto>.Failure(new ApplicationError(
                "Support.Impersonation.ReasonRequired", "A business reason is required to start an impersonation session.", Guid.NewGuid()));
        }

        try
        {
            var tenant = await tenantReadRepository.GetByIdAsync(request.TenantId, cancellationToken);
            if (tenant is null)
            {
                return Result<ImpersonationSessionDto>.Failure(new ApplicationError(
                    "Support.Impersonation.TenantNotFound", "The requested tenant was not found.", Guid.NewGuid()));
            }

            var startedAtUtc = DateTime.UtcNow;
            var session = new ImpersonationSessionDto(
                SessionId: Guid.NewGuid(),
                TenantId: request.TenantId,
                AdminUserId: request.ActorId,
                Reason: request.Reason.Trim(),
                StartedAtUtc: startedAtUtc,
                ExpiresAtUtc: startedAtUtc.Add(SessionTimeBox),
                EndedAtUtc: null,
                CorrelationId: request.CorrelationId);

            await cacheService.SetAsync(ImpersonationCacheKeys.ForSession(session.SessionId), session, SessionTimeBox, cancellationToken);

            await permissionUsageLogRepository.RecordAsync(
                new PermissionUsageLogEntry(
                    TenantId: request.TenantId,
                    UserId: request.ActorId,
                    Permission: "Support.Impersonation.Start",
                    RequestType: nameof(StartImpersonationCommand),
                    Decision: "Allowed",
                    CorrelationId: request.CorrelationId,
                    OccurredAtUtc: startedAtUtc),
                cancellationToken);

            logger.LogWarning(
                "Impersonation session {SessionId} started for Tenant {TenantId} by Admin {AdminUserId}, expires {ExpiresAtUtc}.",
                session.SessionId, request.TenantId, request.ActorId, session.ExpiresAtUtc);

            return Result<ImpersonationSessionDto>.Success(session);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to start impersonation session for Tenant {TenantId}", request.TenantId);

            return Result<ImpersonationSessionDto>.Failure(new ApplicationError(
                "Support.Impersonation.StartFailed", "Unable to start the impersonation session at this time.", Guid.NewGuid()));
        }
    }
}
