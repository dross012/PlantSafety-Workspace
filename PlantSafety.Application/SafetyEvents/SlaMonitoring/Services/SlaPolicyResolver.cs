// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Application-layer helper that resolves a per-tenant SLA policy lookup
//   (persisted rows via ISlaPolicyRepository, falling back to SlaPolicy.Default for any severity
//   with no persisted row). Shared by the three SLA monitor query handlers.
// SchemaVersion: 1

using PlantSafety.Application.SafetyEvents.SlaMonitoring.Repositories;
using PlantSafety.Domain.SafetyEvents.SlaMonitoring.ValueObjects;

namespace PlantSafety.Application.SafetyEvents.SlaMonitoring.Services;

/// <summary>Resolves the effective SLA policy for a given severity, tenant-scoped with default fallback.</summary>
public sealed class SlaPolicyResolver
{
    private readonly ISlaPolicyRepository _slaPolicyRepository;

    /// <summary>Initialises dependencies.</summary>
    public SlaPolicyResolver(ISlaPolicyRepository slaPolicyRepository)
    {
        _slaPolicyRepository = slaPolicyRepository;
    }

    /// <summary>
    /// Builds a severity -&gt; <see cref="SlaPolicy"/> lookup for the tenant, using persisted rows
    /// where available and <see cref="SlaPolicy.Default"/> for any severity without one.
    /// </summary>
    public async Task<IReadOnlyDictionary<string, SlaPolicy>> ResolveAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var persisted = await _slaPolicyRepository.GetAllAsync(tenantId, cancellationToken);

        var lookup = new Dictionary<string, SlaPolicy>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in persisted)
        {
            lookup[row.Severity] = new SlaPolicy(
                row.Severity,
                TimeSpan.FromMinutes(row.AcknowledgeWithinMinutes),
                TimeSpan.FromMinutes(row.ResolveWithinMinutes));
        }

        foreach (var severity in new[] { "Minor", "Moderate", "Serious", "Critical" })
        {
            if (!lookup.ContainsKey(severity))
                lookup[severity] = SlaPolicy.Default(severity);
        }

        return lookup;
    }

    /// <summary>Looks up the policy for <paramref name="severity"/>, falling back to <see cref="SlaPolicy.Default"/>.</summary>
    public static SlaPolicy Resolve(IReadOnlyDictionary<string, SlaPolicy> lookup, string severity)
        => lookup.TryGetValue(severity ?? string.Empty, out var policy) ? policy : SlaPolicy.Default(severity ?? string.Empty);
}
