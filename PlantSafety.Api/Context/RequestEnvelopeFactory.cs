// ============================================================================
// File: RequestEnvelopeFactory.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: Populates the canonical Command/Query envelope fields (§2.1-2.2 of the
//              Enterprise Architectural Rule Handbook) from the current request's
//              IOperationContext and ICurrentUserService, so Features/ tree controllers
//              don't duplicate this boilerplate in every action.
// ============================================================================

using PlantSafety.Application.Abstractions.Context;
using PlantSafety.Application.Abstractions.CurrentUser;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Utilities;

namespace PlantSafety.Api.Context;

/// <summary>Common envelope values shared by every <see cref="ICommand{TResponse}"/> instance.</summary>
public sealed record CommandEnvelope(
    Guid CommandId,
    Guid CorrelationId,
    Guid? CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid ActorId);

/// <summary>Common envelope values shared by every <see cref="IQuery{TResponse}"/> instance.</summary>
public sealed record QueryEnvelope(
    string TraceId,
    string SpanId,
    string ApiVersionToken,
    Guid RequestingUserId,
    string RequestingUserRole,
    Guid TenantId,
    Guid CorrelationId,
    CachePreference CachePreference,
    bool SuppressUnauthorized,
    bool StrictMode);

/// <summary>
/// Builds <see cref="CommandEnvelope"/>/<see cref="QueryEnvelope"/> values from the current
/// request's <see cref="IOperationContext"/> and <see cref="ICurrentUserService"/>. Inject this
/// into any Features/ tree controller to populate a command or query's envelope fields.
/// </summary>
public sealed class RequestEnvelopeFactory(
    IOperationContext operationContext,
    ICurrentUserService currentUserService,
    string apiVersionToken = "1.0")
{
    /// <summary>Builds the shared envelope fields for a new command originating at the API boundary.</summary>
    public CommandEnvelope NewCommandEnvelope() => new(
        CommandId: CorrelationIdGenerator.NewId(),
        CorrelationId: operationContext.CorrelationId,
        CausationId: operationContext.CausationId,
        RequestedAtUtc: DateTime.UtcNow,
        ApiVersionToken: apiVersionToken,
        TenantId: operationContext.TenantId,
        ActorId: currentUserService.ActorId);

    /// <summary>Builds the shared envelope fields for a new query originating at the API boundary.</summary>
    public QueryEnvelope NewQueryEnvelope(
        CachePreference cachePreference = CachePreference.Preferred,
        bool suppressUnauthorized = false,
        bool strictMode = false) => new(
        TraceId: operationContext.TraceId,
        SpanId: operationContext.SpanId,
        ApiVersionToken: apiVersionToken,
        RequestingUserId: currentUserService.UserId,
        RequestingUserRole: currentUserService.Roles.Count > 0 ? currentUserService.Roles[0] : string.Empty,
        TenantId: operationContext.TenantId,
        CorrelationId: operationContext.CorrelationId,
        CachePreference: cachePreference,
        SuppressUnauthorized: suppressUnauthorized,
        StrictMode: strictMode);
}
