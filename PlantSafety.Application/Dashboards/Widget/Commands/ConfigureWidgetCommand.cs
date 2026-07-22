// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Command to reconfigure a Widget's data source, refresh interval, and/or parameters.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Dashboards.Widget.Commands;

/// <summary>Reconfigures a Widget's data source, refresh interval, and/or configuration parameters.</summary>
/// <param name="CorrelationId">Correlation lineage identifier for this operation group.</param>
/// <param name="CausationId">Identifier of the immediate parent command/event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued.</param>
/// <param name="WidgetId">Identifier of the widget to reconfigure.</param>
/// <param name="SourceModule">Optional new owning module for the widget's data source.</param>
/// <param name="QueryOrEndpoint">Optional new query/endpoint identifier. Required when <paramref name="SourceModule"/> is supplied.</param>
/// <param name="RefreshIntervalSeconds">Optional new refresh cadence, in seconds.</param>
/// <param name="Parameters">Optional replacement set of configuration parameters.</param>
/// <param name="ConfiguredByUserId">Identity of the user reconfiguring the widget.</param>
public sealed record ConfigureWidgetCommand(
    Guid CorrelationId,
    Guid? CausationId,
    DateTime RequestedAtUtc,
    Guid WidgetId,
    string? SourceModule,
    string? QueryOrEndpoint,
    int? RefreshIntervalSeconds,
    IReadOnlyDictionary<string, string>? Parameters,
    Guid ConfiguredByUserId) : ICommand<Result<ConfigureWidgetResult>>
{
    /// <summary>Gets the unique identifier of this command instance.</summary>
    public Guid CommandId { get; init; } = Guid.NewGuid();
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";
    /// <summary>Placeholder: no tenant field exists on this command; defaults to <see cref="Guid.Empty"/> until tenant scoping is added.</summary>
    public Guid TenantId { get; init; } = Guid.Empty;
    /// <summary>Maps to <see cref="ConfiguredByUserId"/>.</summary>
    public Guid ActorId => ConfiguredByUserId;
}

/// <summary>Result of <see cref="ConfigureWidgetCommand"/>.</summary>
public sealed record ConfigureWidgetResult(Guid WidgetId);
