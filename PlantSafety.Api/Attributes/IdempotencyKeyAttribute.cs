// ============================================================================
// File: IdempotencyKeyAttribute.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: Marker attribute applied to POST action methods to indicate that
//              they participate in idempotency enforcement. IdempotencyFilter uses
//              this attribute to opt specific actions into idempotency checking.
// ============================================================================
//
// Onboarding Notes:
// - Apply to POST actions that create durable resources (incident reports, audit
//   findings, inspections, corrective actions) where duplicate submissions must be
//   prevented:
//       [IdempotencyKey]
//       [HttpPost]
//       public async Task<IActionResult> CreateAsync(...) { ... }
// - If applied without an Idempotency-Key header in the request, IdempotencyFilter
//   returns HTTP 422 Unprocessable Entity citing a missing required header.
//   This enforces the API contract: decorated endpoints REQUIRE the header.
// - Use [IdempotencyKey(Required = false)] for endpoints where idempotency is
//   optional (the header is used if present, ignored if absent).
//
// Rationale:
// - Provides a declarative opt-in mechanism so idempotency enforcement is
//   self-documenting at the action level rather than configured via global rules.
//
// Future Extension Seams:
// - Add CacheDuration (TimeSpan) property to override the 24-hour default per
//   endpoint (e.g. 5-minute idempotency for high-frequency telemetry ingestion).
//
// ============================================================================

namespace PlantSafety.Api.Attributes;

/// <summary>
/// Marker attribute indicating that the decorated POST action requires or supports
/// idempotency enforcement via the <c>Idempotency-Key</c> request header.
/// Consumed by <see cref="Filters.IdempotencyFilter"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class IdempotencyKeyAttribute : Attribute
{
    /// <summary>
    /// Gets or sets a value indicating whether the <c>Idempotency-Key</c> header is required.
    /// When <c>true</c> (default), requests without the header receive HTTP 422.
    /// When <c>false</c>, the header is used when present but absence is not an error.
    /// </summary>
    public bool Required { get; set; } = true;
}
