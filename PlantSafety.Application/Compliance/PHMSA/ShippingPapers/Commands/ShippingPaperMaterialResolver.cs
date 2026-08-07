// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Application
//  File:           ShippingPaperMaterialResolver.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Shared helper that resolves a HazmatShipment's referenced MaterialIds into the
//                  (UNNumber, ProperShippingName, HazardClass, PackingGroup) tuples that
//                  ShippingPaperGenerator.BuildShippingPaper requires, via the Classification
//                  module's Application-layer read repository. Reused by
//                  GenerateShippingPaperCommandHandler and ValidateShippingPaperCommandHandler so
//                  both build the identical materials list from the same source of truth.
//
//                  KNOWN GAP: PlantSafety.Application.Compliance.PHMSA.Classification.Repositories.
//                  IHazmatMaterialRepository / .Models.HazmatMaterialDto are owned by a parallel
//                  build effort and were not present in this repository at the time this file was
//                  written. This helper references them by their documented expected
//                  namespace/shape (mirroring [Compliance].[PHMSA_HazmatMaterial]'s column set) --
//                  it will not compile until that parallel effort lands those two types.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Classification.Enums;
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects;
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects.Ids;
using PlantSafety.Application.Compliance.PHMSA.Classification.Models;
using PlantSafety.Application.Compliance.PHMSA.Classification.Repositories;

namespace PlantSafety.Application.Compliance.PHMSA.ShippingPapers.Commands;

/// <summary>Resolves HazmatShipment MaterialIds into the tuples ShippingPaperGenerator requires.</summary>
public static class ShippingPaperMaterialResolver
{
    /// <summary>
    /// Resolves each material id to its (UNNumber, ProperShippingName, HazardClass, PackingGroup)
    /// tuple, skipping any material id that cannot be resolved or fails to parse.
    /// </summary>
    public static async Task<List<(UNNumber UNNumber, ProperShippingName ShippingName, HazardClass HazardClass, PackingGroup PackingGroup)>> ResolveAsync(
        IHazmatMaterialRepository materialRepository,
        IReadOnlyList<HazmatMaterialId> materialIds,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var resolved = new List<(UNNumber, ProperShippingName, HazardClass, PackingGroup)>();

        foreach (var materialId in materialIds ?? [])
        {
            HazmatMaterialDto? material = await materialRepository.GetByIdAsync(materialId.Value, tenantId, cancellationToken);
            if (material is null)
                continue;

            if (!PlantSafety.Application.Compliance.PHMSA.Placarding.Commands.HazardClassParser.TryParse(material.HazardClass, out var hazardClass))
                continue;

            if (!Enum.TryParse<PackingGroup>(material.PackingGroup, ignoreCase: true, out var packingGroup))
                packingGroup = PackingGroup.NotApplicable;

            try
            {
                resolved.Add((new UNNumber(material.UNNumber), new ProperShippingName(material.ProperShippingName), hazardClass, packingGroup));
            }
            catch (Exception)
            {
                // Skip materials whose persisted UNNumber/ProperShippingName no longer satisfy
                // the value object's validation rules (UNNumber throws InvalidUNNumberException,
                // ProperShippingName throws ArgumentException) -- defensive, should not occur in
                // practice for data that was itself persisted through those same value objects.
            }
        }

        return resolved;
    }
}
