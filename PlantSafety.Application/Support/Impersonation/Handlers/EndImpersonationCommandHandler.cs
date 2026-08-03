// AUTHOR: PlantSafety Platform Team (Phase 7 §6.3 Support Tooling)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for EndImpersonationCommand. Ends an active session (removing its
//   cache entry so it can never be resumed) and writes a real audit record via
//   IPermissionUsageLogRepository, mirroring StartImpersonationCommandHandler.
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

namespace PlantSafety.Application.Support.Impersonation.Handlers;

/// <summary>Handles <see cref="EndImpersonationCommand"/>.</summary>
public sealed class EndImpersonationCommandHandler(
    ICacheService cacheService,
    IPermissionUsageLogRepository permissionUsageLogRepository,
    ILogger<EndImpersonationCommandHandler> logger)
    : IRequestHandler<EndImpersonationCommand, Result<ImpersonationSessionDto>>
{
    /// <inheritdoc/>
    public async Task<Result<ImpersonationSessionDto>> Handle(EndImpersonationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var cacheKey = ImpersonationCacheKeys.ForSession(request.SessionId);
            var session = await cacheService.GetAsync<ImpersonationSessionDto>(cacheKey, cancellationToken);

            if (session is null)
            {
                return Result<ImpersonationSessionDto>.Failure(new ApplicationError(
                    "Support.Impersonation.SessionNotFound", "No active impersonation session was found (it may have already ended or expired).", Guid.NewGuid()));
            }

            if (session.AdminUserId != request.ActorId)
            {
                return Result<ImpersonationSessionDto>.Failure(new ApplicationError(
                    "Support.Impersonation.NotSessionOwner", "Only the administrator who started this session may end it.", Guid.NewGuid()));
            }

            var endedAtUtc = DateTime.UtcNow;
            var endedSession = session with { EndedAtUtc = endedAtUtc };

            await cacheService.RemoveAsync(cacheKey, cancellationToken);

            await permissionUsageLogRepository.RecordAsync(
                new PermissionUsageLogEntry(
                    TenantId: session.TenantId,
                    UserId: request.ActorId,
                    Permission: "Support.Impersonation.End",
                    RequestType: nameof(EndImpersonationCommand),
                    Decision: "Allowed",
                    CorrelationId: session.CorrelationId,
                    OccurredAtUtc: endedAtUtc),
                cancellationToken);

            logger.LogWarning(
                "Impersonation session {SessionId} ended for Tenant {TenantId} by Admin {AdminUserId}.",
                session.SessionId, session.TenantId, request.ActorId);

            return Result<ImpersonationSessionDto>.Success(endedSession);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to end impersonation session {SessionId}", request.SessionId);

            return Result<ImpersonationSessionDto>.Failure(new ApplicationError(
                "Support.Impersonation.EndFailed", "Unable to end the impersonation session at this time.", Guid.NewGuid()));
        }
    }
}
