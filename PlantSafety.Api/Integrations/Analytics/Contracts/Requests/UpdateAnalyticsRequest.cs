// ============================================================================
// File: UpdateAnalyticsRequest.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: External PUT request DTO for updating an existing ingested analytics record.
// ============================================================================
//
// Onboarding Notes:
// - Bound from the request body along with the route id. Controller maps to UpdateAnalyticsCommand.
//
// Rationale:
// - Decouples external contract from internal Application update command type.
//
// Future Extension Seams:
// - Add version token here when optimistic concurrency is required for Analytics records.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.Analytics.Contracts.Requests;

/// <summary>Request body for updating an existing ingested analytics record.</summary>
/// <param name="MetricName">Updated metric name.</param>
/// <param name="MetricValue">Updated metric value.</param>
/// <param name="Status">Updated processing status value.</param>
public sealed record UpdateAnalyticsRequest(
    string  MetricName,
    decimal MetricValue,
    string  Status);
