// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for IssueCredentialCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.PhysicalAccessControl.Credentials.Aggregates;
using PlantSafety.Domain.PhysicalAccessControl.Credentials.Enums;
using PlantSafety.Domain.PhysicalAccessControl.Credentials.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PhysicalAccessControl.Commands;

/// <summary>Handles <see cref="IssueCredentialCommand"/>. Badge/credential PII fields are audit-logged
/// per the Shared.AuditLogging pattern (see Infrastructure repository).</summary>
public sealed class IssueCredentialCommandHandler : IRequestHandler<IssueCredentialCommand, Result<CredentialActionResult>>
{
    private readonly ICredentialRepository _repository;
    private readonly ILogger<IssueCredentialCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public IssueCredentialCommandHandler(ICredentialRepository repository, ILogger<IssueCredentialCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CredentialActionResult>> Handle(IssueCredentialCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var performedBy = UserId.FromGuid(request.PerformedByUserId);
            var holderType = Enum.TryParse<CredentialHolderType>(request.HolderType, true, out var parsed)
                ? parsed
                : CredentialHolderType.Employee;

            var aggregate = Credential.Create(
                performedBy,
                UtcTimestamp.Now(),
                new CorrelationId(request.CorrelationId),
                new CausationId(request.CausationId),
                PerformedByActor.User(performedBy),
                request.TenantId,
                request.SiteId,
                request.BadgeNumber,
                request.HolderUserId,
                holderType,
                request.AccessLevel,
                request.ExpiresAtUtc.HasValue ? UtcTimestamp.From(request.ExpiresAtUtc.Value) : null);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CredentialActionResult>.Success(new CredentialActionResult(
                aggregate.CredentialId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to issue Credential for Tenant {TenantId}", request.TenantId);

            return Result<CredentialActionResult>.Failure(new ApplicationError(
                "PhysicalAccessControl.Credential.IssueFailed",
                "Unable to issue the Credential record at this time.",
                Guid.NewGuid()));
        }
    }
}
