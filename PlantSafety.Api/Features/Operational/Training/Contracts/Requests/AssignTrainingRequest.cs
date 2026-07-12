// ============================================================================
// File: AssignTrainingRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: External-facing request DTO for POST /features/operational/training,
//              bound from the HTTP request body.
// ============================================================================

namespace PlantSafety.Api.Features.Operational.Training.Contracts.Requests;

/// <summary>Request body for assigning a safety training course to a worker.</summary>
/// <param name="WorkerId">Worker being assigned the training.</param>
/// <param name="TrainingCourseCode">Training course code. Max 50 characters.</param>
/// <param name="TrainingCourseName">Training course name.</param>
/// <param name="DeliveryMethod">Delivery method (InPerson, Online, OJT).</param>
/// <param name="DueDateUtc">Due date for completion (UTC).</param>
public sealed record AssignTrainingRequest(
    Guid           WorkerId,
    string         TrainingCourseCode,
    string         TrainingCourseName,
    string         DeliveryMethod,
    DateTimeOffset DueDateUtc);
