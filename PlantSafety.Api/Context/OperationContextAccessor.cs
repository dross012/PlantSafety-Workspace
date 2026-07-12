// ============================================================================
// File: OperationContextAccessor.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: Presentation-layer implementation of IOperationContext. Resolves the full
//              correlation lineage envelope from HttpContext.Items (populated by
//              CorrelationIdMiddleware and TenantResolutionMiddleware) and the current
//              System.Diagnostics.Activity for distributed tracing identifiers.
// ============================================================================
//
// Onboarding Notes:
// - Registered as Scoped in DI (one instance per HTTP request), matching the per-request
//   lifetime of IHttpContextAccessor.
// - TraceId/SpanId come from Activity.Current (populated automatically by ASP.NET Core's
//   built-in W3C trace context propagation); a fresh identifier is generated as a fallback
//   for non-HTTP contexts (background jobs, tests) where no Activity is present.
// - CausationId is null for root API-initiated operations, per the Canonical Lineage Rule
//   (§2.17-2.18) — root commands generate a new CausationId only when they themselves cause
//   a child operation, they don't inherit one from nothing.
//
// ============================================================================

using PlantSafety.Api.CrossDomain;
using PlantSafety.Application.Abstractions.Context;

namespace PlantSafety.Api.Context;

/// <summary>
/// Resolves <see cref="IOperationContext"/> from the current HTTP request's middleware-populated
/// <see cref="HttpContext.Items"/> and the ambient <see cref="System.Diagnostics.Activity"/>.
/// </summary>
public sealed class OperationContextAccessor : IOperationContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Guid _requestId = Guid.NewGuid();
    private readonly DateTime _operationStartedAtUtc = DateTime.UtcNow;

    /// <summary>Initialises a new instance of <see cref="OperationContextAccessor"/>.</summary>
    /// <param name="httpContextAccessor">ASP.NET Core HTTP context accessor.</param>
    public OperationContextAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc/>
    public Guid CorrelationId
    {
        get
        {
            var raw = _httpContextAccessor.HttpContext?.Items[Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString();
            return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
        }
    }

    /// <inheritdoc/>
    public Guid? CausationId => null;

    /// <inheritdoc/>
    public Guid RequestId => _requestId;

    /// <inheritdoc/>
    public string TraceId =>
        System.Diagnostics.Activity.Current?.TraceId.ToString() ?? _requestId.ToString("N");

    /// <inheritdoc/>
    public string SpanId =>
        System.Diagnostics.Activity.Current?.SpanId.ToString() ?? _requestId.ToString("N")[..16];

    /// <inheritdoc/>
    public string? ParentOperationId => null;

    /// <inheritdoc/>
    public Guid TenantId =>
        _httpContextAccessor.HttpContext?.Items[TenantContextAccessor.TenantIdKey] is Guid g ? g : Guid.Empty;

    /// <inheritdoc/>
    public Guid? SiteId =>
        _httpContextAccessor.HttpContext?.Items[TenantContextAccessor.SiteIdKey] as Guid?;

    /// <inheritdoc/>
    public DateTime OperationStartedAtUtc => _operationStartedAtUtc;
}
