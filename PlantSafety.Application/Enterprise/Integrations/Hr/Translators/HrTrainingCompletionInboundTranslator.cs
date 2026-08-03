// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Enterprise.Integrations.Hr
//  Layer:          Application
//  File:           HrTrainingCompletionInboundTranslator.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §1.4 Enterprise System Integrations, HR/Identity)
//  SchemaVersion:  2026.1
//  Description:    Translates an inbound HR training-assignment event ("hr.training.assigned.v1")
//                  into the platform's real, public
//                  Features.Operational.Commands.Training.AssignTraining.AssignTrainingCommand --
//                  verified as the correct, real entry point (Operational.Training, checked first per
//                  the originating task brief, is a query-only read model with no write command;
//                  Features.Operational.Commands.Training.AssignTraining is the actual, real
//                  training-write surface). A genuine training-COMPLETION sync
//                  ("hr.training.completed.v1") is explicitly NOT handled here: ITrainingRepository
//                  (the real repository backing AssignTrainingCommand) exposes only
//                  AssignAsync/GetByIdAsync -- no CompleteAsync method exists yet. That EventType is
//                  gracefully rejected with a clear, actionable error rather than silently dropped or
//                  fabricated against a non-existent command (flagged in this Phase's report as the
//                  concrete follow-up: add CompleteTrainingCommand + repository method, then extend
//                  this translator). Additive-only evolution.
// ============================================================================
using System.Text.Json;
using MediatR;
using PlantSafety.Application.Features.Operational.Commands.Training.AssignTraining;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;
using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.SharedKernel.Primitives.Results;

namespace PlantSafety.Application.Enterprise.Integrations.Hr.Translators;

/// <summary>Translates an inbound HR training-assignment event into <see cref="AssignTrainingCommand"/>.</summary>
public sealed class HrTrainingCompletionInboundTranslator(ISender sender) : IEnterpriseInboundEventTranslator
{
    /// <summary>The canonical EventType this translator handles.</summary>
    public const string HandledEventType = "hr.training.assigned.v1";

    /// <summary>Recognised-but-unsupported EventType -- see file header.</summary>
    public const string UnsupportedCompletionEventType = "hr.training.completed.v1";

    /// <inheritdoc/>
    public bool CanHandle(EnterpriseEventEnvelope envelope) =>
        envelope.SourceSystem == EnterpriseSourceSystem.Hr &&
        (string.Equals(envelope.EventType, HandledEventType, StringComparison.Ordinal) ||
         string.Equals(envelope.EventType, UnsupportedCompletionEventType, StringComparison.Ordinal));

    /// <inheritdoc/>
    public async Task<Result<bool>> TranslateAndDispatchAsync(EnterpriseEventEnvelope envelope, CancellationToken cancellationToken)
    {
        if (string.Equals(envelope.EventType, UnsupportedCompletionEventType, StringComparison.Ordinal))
        {
            return Result<bool>.Failure(new Error(
                ErrorCode.Create("enterprise.eventmesh.notsupported"),
                ErrorType.Validation,
                $"HR EventType '{UnsupportedCompletionEventType}' is recognised but not yet " +
                "supported: no real CompleteTrainingCommand exists on the Operational.Training " +
                "write surface. Add the command + ITrainingRepository.CompleteAsync method, then " +
                "extend this translator."));
        }

        var payload = envelope.Payload;

        if (!payload.TryGetProperty("workerId", out var workerIdElement) || !workerIdElement.TryGetGuid(out var workerId))
            return InvalidPayload("'workerId'");

        if (!TryGetString(payload, "trainingCourseCode", out var trainingCourseCode))
            return InvalidPayload("'trainingCourseCode'");

        var trainingCourseName = TryGetString(payload, "trainingCourseName", out var courseName) ? courseName : trainingCourseCode;

        var deliveryMethod = TryGetString(payload, "deliveryMethod", out var method) ? method : "Online";

        var dueDateUtc = payload.TryGetProperty("dueDateUtc", out var dueDateElement) &&
            dueDateElement.ValueKind == JsonValueKind.String && dueDateElement.TryGetDateTimeOffset(out var parsedDueDate)
            ? parsedDueDate
            : DateTimeOffset.UtcNow.AddDays(30);

        var command = new AssignTrainingCommand
        {
            CommandId = Guid.NewGuid(),
            CorrelationId = envelope.CorrelationId,
            CausationId = envelope.CausationId ?? envelope.EventId,
            RequestedAtUtc = DateTime.UtcNow,
            ApiVersionToken = "v1",
            TenantId = envelope.TenantId,
            ActorId = envelope.ActorId ?? Guid.Empty,
            WorkerId = workerId,
            TrainingCourseCode = trainingCourseCode,
            TrainingCourseName = trainingCourseName,
            DeliveryMethod = deliveryMethod,
            DueDateUtc = dueDateUtc,
        };

        try
        {
            await sender.Send(command, cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Result<bool>.Failure(new Error(
                ErrorCode.Create("enterprise.eventmesh.hrtrainingsyncfailed"),
                ErrorType.Failure,
                "Training assignment sync failed. See server logs for details."));
        }
    }

    private static bool TryGetString(JsonElement payload, string propertyName, out string value)
    {
        value = string.Empty;
        if (!payload.TryGetProperty(propertyName, out var element) || element.ValueKind != JsonValueKind.String)
            return false;

        var raw = element.GetString();
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        value = raw;
        return true;
    }

    private static Result<bool> InvalidPayload(string missingField) =>
        Result<bool>.Failure(new Error(
            ErrorCode.Create("enterprise.eventmesh.invalidpayload"),
            ErrorType.Validation,
            $"HR training-assignment payload is missing a valid {missingField}."));
}
