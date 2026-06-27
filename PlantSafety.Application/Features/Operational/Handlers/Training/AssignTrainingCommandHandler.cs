// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for AssignTrainingCommand.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Clock;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Utilities;
using PlantSafety.Application.Features.Operational.Commands.Training.AssignTraining;

namespace PlantSafety.Application.Features.Operational.Handlers.Training;

/// <summary>Handles <see cref="AssignTrainingCommand"/>. Returns the new RecordId.</summary>
public sealed class AssignTrainingCommandHandler : ICommandHandler<AssignTrainingCommand, Guid>
{
    private readonly ITrainingRepository _repository;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<AssignTrainingCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public AssignTrainingCommandHandler(
        ITrainingRepository repository,
        IDateTimeProvider clock,
        ILogger<AssignTrainingCommandHandler> logger)
    {
        _repository = repository;
        _clock = clock;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Guid> Handle(AssignTrainingCommand request, CancellationToken cancellationToken)
    {
        var recordId = CorrelationIdGenerator.NewId(_clock.UtcNowOffset);

        await _repository.AssignAsync(
            recordId: recordId,
            tenantId: request.TenantId,
            workerId: request.WorkerId,
            trainingCourseCode: request.TrainingCourseCode,
            trainingCourseName: request.TrainingCourseName,
            deliveryMethod: request.DeliveryMethod,
            dueDateUtc: request.DueDateUtc,
            actorId: request.ActorId,
            correlationId: request.CorrelationId,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Training {RecordId} assigned to Worker {WorkerId} Course {TrainingCourseCode} Tenant {TenantId} by Actor {ActorId}",
            recordId, request.WorkerId, request.TrainingCourseCode, request.TenantId, request.ActorId);

        return recordId;
    }
}
