// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handlers for the real Accident investigation workflow commands. Pure
//   orchestration: loads the aggregate, invokes the corresponding domain method, and persists via
//   the repository interface. No business rules live here -- all invariants are enforced inside
//   the Accident aggregate and its CorrectiveAction child entity.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.AccidentManagement.Enums;
using PlantSafety.Domain.AccidentManagement.Interfaces;
using PlantSafety.Domain.AccidentManagement.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.AccidentManagement.Commands;

/// <summary>Handles <see cref="AssignAccidentInvestigatorCommand"/>.</summary>
public sealed class AssignAccidentInvestigatorCommandHandler : IRequestHandler<AssignAccidentInvestigatorCommand, Result<AccidentStatusResult>>
{
    private readonly IAccidentRepository _repository;
    private readonly ILogger<AssignAccidentInvestigatorCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public AssignAccidentInvestigatorCommandHandler(IAccidentRepository repository, ILogger<AssignAccidentInvestigatorCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AccidentStatusResult>> Handle(AssignAccidentInvestigatorCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var aggregate = await _repository.GetByIdAsync(AccidentId.FromGuid(request.AccidentId), request.TenantId, cancellationToken);
            if (aggregate is null)
                return Result<AccidentStatusResult>.Failure(new ApplicationError("AccidentManagement.Accident.NotFound", "Accident not found.", request.AccidentId));

            var investigatorId = UserId.FromGuid(request.InvestigatorId);
            aggregate.AssignInvestigator(investigatorId, UtcTimestamp.Now(), PerformedByActor.User(investigatorId),
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);
            return Result<AccidentStatusResult>.Success(new AccidentStatusResult(aggregate.AccidentId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to assign investigator to Accident {AccidentId}", request.AccidentId);
            return Result<AccidentStatusResult>.Failure(new ApplicationError("AccidentManagement.Accident.AssignInvestigatorFailed", "Unable to assign an investigator.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="RecordAccidentRootCauseCommand"/>.</summary>
public sealed class RecordAccidentRootCauseCommandHandler : IRequestHandler<RecordAccidentRootCauseCommand, Result<AccidentStatusResult>>
{
    private readonly IAccidentRepository _repository;
    private readonly ILogger<RecordAccidentRootCauseCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordAccidentRootCauseCommandHandler(IAccidentRepository repository, ILogger<RecordAccidentRootCauseCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AccidentStatusResult>> Handle(RecordAccidentRootCauseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var aggregate = await _repository.GetByIdAsync(AccidentId.FromGuid(request.AccidentId), request.TenantId, cancellationToken);
            if (aggregate is null)
                return Result<AccidentStatusResult>.Failure(new ApplicationError("AccidentManagement.Accident.NotFound", "Accident not found.", request.AccidentId));

            var recordedBy = UserId.FromGuid(request.RecordedBy);
            var rootCause = RootCauseCategory.IsValid(request.RootCauseCategory) ? RootCauseCategory.From(request.RootCauseCategory) : RootCauseCategory.Default;

            aggregate.RecordRootCause(rootCause, request.Notes, UtcTimestamp.Now(), PerformedByActor.User(recordedBy),
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);
            return Result<AccidentStatusResult>.Success(new AccidentStatusResult(aggregate.AccidentId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record root cause for Accident {AccidentId}", request.AccidentId);
            return Result<AccidentStatusResult>.Failure(new ApplicationError("AccidentManagement.Accident.RecordRootCauseFailed", "Unable to record the root cause.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="CreateAccidentCorrectiveActionCommand"/>.</summary>
public sealed class CreateAccidentCorrectiveActionCommandHandler : IRequestHandler<CreateAccidentCorrectiveActionCommand, Result<CreateAccidentCorrectiveActionResult>>
{
    private readonly IAccidentRepository _repository;
    private readonly ILogger<CreateAccidentCorrectiveActionCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateAccidentCorrectiveActionCommandHandler(IAccidentRepository repository, ILogger<CreateAccidentCorrectiveActionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateAccidentCorrectiveActionResult>> Handle(CreateAccidentCorrectiveActionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var aggregate = await _repository.GetByIdAsync(AccidentId.FromGuid(request.AccidentId), request.TenantId, cancellationToken);
            if (aggregate is null)
                return Result<CreateAccidentCorrectiveActionResult>.Failure(new ApplicationError("AccidentManagement.Accident.NotFound", "Accident not found.", request.AccidentId));

            var createdBy = UserId.FromGuid(request.CreatedBy);
            var responsiblePersonId = UserId.FromGuid(request.ResponsiblePersonId);
            var category = CorrectiveActionCategory.IsValid(request.Category) ? CorrectiveActionCategory.From(request.Category) : CorrectiveActionCategory.Default;

            var action = aggregate.CreateCorrectiveAction(category, request.Description, responsiblePersonId, UtcTimestamp.From(request.DueAtUtc),
                UtcTimestamp.Now(), PerformedByActor.User(createdBy), new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);
            return Result<CreateAccidentCorrectiveActionResult>.Success(new CreateAccidentCorrectiveActionResult(aggregate.AccidentId.Value, action.CorrectiveActionId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create corrective action for Accident {AccidentId}", request.AccidentId);
            return Result<CreateAccidentCorrectiveActionResult>.Failure(new ApplicationError("AccidentManagement.Accident.CreateCorrectiveActionFailed", "Unable to create the corrective action.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="CompleteAccidentCorrectiveActionCommand"/>.</summary>
public sealed class CompleteAccidentCorrectiveActionCommandHandler : IRequestHandler<CompleteAccidentCorrectiveActionCommand, Result<AccidentStatusResult>>
{
    private readonly IAccidentRepository _repository;
    private readonly ILogger<CompleteAccidentCorrectiveActionCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CompleteAccidentCorrectiveActionCommandHandler(IAccidentRepository repository, ILogger<CompleteAccidentCorrectiveActionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AccidentStatusResult>> Handle(CompleteAccidentCorrectiveActionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var aggregate = await _repository.GetByIdAsync(AccidentId.FromGuid(request.AccidentId), request.TenantId, cancellationToken);
            if (aggregate is null)
                return Result<AccidentStatusResult>.Failure(new ApplicationError("AccidentManagement.Accident.NotFound", "Accident not found.", request.AccidentId));

            var completedBy = UserId.FromGuid(request.CompletedBy);
            aggregate.CompleteCorrectiveAction(CorrectiveActionId.FromGuid(request.CorrectiveActionId), request.CompletionEvidence, UtcTimestamp.Now(),
                PerformedByActor.User(completedBy), new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);
            return Result<AccidentStatusResult>.Success(new AccidentStatusResult(aggregate.AccidentId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete corrective action for Accident {AccidentId}", request.AccidentId);
            return Result<AccidentStatusResult>.Failure(new ApplicationError("AccidentManagement.Accident.CompleteCorrectiveActionFailed", "Unable to complete the corrective action.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="FileAccidentRegulatoryReportCommand"/>.</summary>
public sealed class FileAccidentRegulatoryReportCommandHandler : IRequestHandler<FileAccidentRegulatoryReportCommand, Result<AccidentStatusResult>>
{
    private readonly IAccidentRepository _repository;
    private readonly ILogger<FileAccidentRegulatoryReportCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public FileAccidentRegulatoryReportCommandHandler(IAccidentRepository repository, ILogger<FileAccidentRegulatoryReportCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AccidentStatusResult>> Handle(FileAccidentRegulatoryReportCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var aggregate = await _repository.GetByIdAsync(AccidentId.FromGuid(request.AccidentId), request.TenantId, cancellationToken);
            if (aggregate is null)
                return Result<AccidentStatusResult>.Failure(new ApplicationError("AccidentManagement.Accident.NotFound", "Accident not found.", request.AccidentId));

            var filedBy = UserId.FromGuid(request.FiledBy);
            aggregate.FileRegulatoryReport(filedBy, request.RegulatoryAgency, request.ReferenceNumber, UtcTimestamp.Now(),
                PerformedByActor.User(filedBy), new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);
            return Result<AccidentStatusResult>.Success(new AccidentStatusResult(aggregate.AccidentId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to file regulatory report for Accident {AccidentId}", request.AccidentId);
            return Result<AccidentStatusResult>.Failure(new ApplicationError("AccidentManagement.Accident.FileRegulatoryReportFailed", "Unable to file the regulatory report.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="CloseAccidentInvestigationCommand"/>.</summary>
public sealed class CloseAccidentInvestigationCommandHandler : IRequestHandler<CloseAccidentInvestigationCommand, Result<AccidentStatusResult>>
{
    private readonly IAccidentRepository _repository;
    private readonly ILogger<CloseAccidentInvestigationCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CloseAccidentInvestigationCommandHandler(IAccidentRepository repository, ILogger<CloseAccidentInvestigationCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AccidentStatusResult>> Handle(CloseAccidentInvestigationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var aggregate = await _repository.GetByIdAsync(AccidentId.FromGuid(request.AccidentId), request.TenantId, cancellationToken);
            if (aggregate is null)
                return Result<AccidentStatusResult>.Failure(new ApplicationError("AccidentManagement.Accident.NotFound", "Accident not found.", request.AccidentId));

            var closedBy = UserId.FromGuid(request.ClosedBy);
            aggregate.CloseInvestigation(closedBy, UtcTimestamp.Now(), PerformedByActor.User(closedBy),
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);
            return Result<AccidentStatusResult>.Success(new AccidentStatusResult(aggregate.AccidentId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to close investigation for Accident {AccidentId}", request.AccidentId);
            return Result<AccidentStatusResult>.Failure(new ApplicationError("AccidentManagement.Accident.CloseInvestigationFailed", "Unable to close the investigation.", Guid.NewGuid()));
        }
    }
}
