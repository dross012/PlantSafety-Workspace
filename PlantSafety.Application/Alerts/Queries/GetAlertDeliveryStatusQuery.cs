// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Query to retrieve the multi-channel delivery status for a single Alert.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Alerts.Models;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Alerts.Queries;

/// <summary>Retrieves the multi-channel delivery attempts recorded for a single Alert.</summary>
/// <param name="AlertId">Identifier of the alert whose delivery status is requested.</param>
public sealed record GetAlertDeliveryStatusQuery(Guid AlertId) : IRequest<Result<IReadOnlyList<AlertDeliveryRecordDto>>>;
