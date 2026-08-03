// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for RecordSecretRotationCommand. Logs the rotation (secret NAME,
//   rotation timestamp, and triggering context -- NEVER the secret value) using the same
//   ILogger-based audit convention already established for feature-flag toggles (see
//   SetFeatureFlagEnabledCommandHandler and docs\architecture\audit-logging.md's documented
//   gap: dbo.AuditLog has no INSERT-capable path from C# for non-tabular security events).
// SchemaVersion: 1

using System;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.Secrets.Commands;
using PlantSafety.SharedKernel.Audit;

namespace PlantSafety.Application.Operations.Secrets.Handlers;

/// <summary>Handles <see cref="RecordSecretRotationCommand"/>.</summary>
public sealed class RecordSecretRotationCommandHandler : IRequestHandler<RecordSecretRotationCommand, Result<bool>>
{
    private readonly ILogger<RecordSecretRotationCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordSecretRotationCommandHandler(ILogger<RecordSecretRotationCommandHandler> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<Result<bool>> Handle(RecordSecretRotationCommand request, CancellationToken cancellationToken)
    {
        var auditEvent = new SecretRotationAuditEvent(request.SecretName, request.RotatedAtUtc, request.TriggeringContext);

        // Deliberately logs SecretName + timestamp + triggering context only -- never a
        // secret value (this command never receives one). See this handler's file header
        // for why this is an ILogger-based audit trail rather than a dbo.AuditLog INSERT.
        _logger.LogWarning(
            "Secret rotation recorded: SecretName={SecretName}; RotatedAtUtc={RotatedAtUtc:O}; TriggeringContext={TriggeringContext}; EventType={EventType}",
            auditEvent.SecretName, auditEvent.RotatedAtUtc, auditEvent.TriggeringContext, auditEvent.EventType);

        return Task.FromResult(Result<bool>.Success(true));
    }
}
