// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-05
// FILE DESCRIPTION: Gathers Permit facts for the Compliance Rules Engine by calling the real,
//   already-existing GetPermitsByIdQuery (Compliance.Permits.Queries). No new query or schema
//   introduced -- this is the same read-model the Permits UI/API already use.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Compliance.Permits.Models;
using PlantSafety.Application.Compliance.Permits.Queries;

namespace PlantSafety.Application.Compliance.RulesEngine.Rules.FactProviders;

/// <summary>Gathers real Permit facts from the Compliance.Permits read-model.</summary>
public sealed class PermitComplianceFactProvider(ISender sender)
{
    /// <summary>Returns the real Permits record for <paramref name="permitsId"/>, or <see langword="null"/> when not found/failed.</summary>
    public async Task<PermitsItemDto?> GetPermitAsync(Guid tenantId, Guid permitsId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPermitsByIdQuery(tenantId, permitsId), cancellationToken);
        return result.IsSuccess ? result.Value : null;
    }
}
