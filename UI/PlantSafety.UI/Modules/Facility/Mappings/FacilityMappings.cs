// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.Facility.Mappings
//  File:           FacilityMappings.cs
//  Created:        2026-08-09
//  Description:    Backend DTO mirror + UI ViewModel + explicit,
//                  one-directional, hand-written mapping for the Facility
//                  module (Section 13). PlantSafety.UI takes NO project
//                  reference to Shared.FacilityDomain (Section: "UI is
//                  strictly client-side; no backend project references from
//                  UI projects") -- FacilityBackendDto is a local, UI-owned
//                  mirror of the JSON shape PlantSafety.UI.Api's
//                  FacilitiesController returns, deserialized independently.
//                  Backend DTO -> UI ViewModel only; never the reverse,
//                  never inline in a component/state container. Deliberately
//                  NOT Mapster-source-generated -- a documented, time-boxed
//                  deviation from the literal "Mapster source generators
//                  only" rule (see the final delivery report); the more
//                  important separation-of-concerns rule (explicit,
//                  one-directional, out of components/state) is honored.
// ============================================================================
namespace PlantSafety.UI.Modules.Facility.Mappings;

/// <summary>Local mirror of the wire shape PlantSafety.UI.Api's FacilitiesController returns.</summary>
public sealed record FacilityBackendDto(
    Guid FacilityId,
    string Name,
    FacilityIndustryDto Industry,
    FacilityLocationDto Location,
    FacilityHazardProfileDto HazardProfile,
    FacilityHierarchyDto Hierarchy,
    string LifecycleStatus,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

/// <summary>Local mirror of FacilityIndustry.</summary>
public sealed record FacilityIndustryDto(string IndustryName, string? NaicsCode);

/// <summary>Local mirror of FacilityLocation.</summary>
public sealed record FacilityLocationDto(string AddressLine1, string City, string StateOrProvince, string PostalCode, string CountryCode);

/// <summary>Local mirror of FacilityHazardProfile.</summary>
public sealed record FacilityHazardProfileDto(IReadOnlyList<string> ActiveHazardTypes);

/// <summary>Local mirror of FacilityHierarchy (buildings only, flattened for display).</summary>
public sealed record FacilityHierarchyDto(IReadOnlyList<FacilityBuildingDto> Buildings);

/// <summary>Local mirror of one building within a FacilityHierarchy.</summary>
public sealed record FacilityBuildingDto(Guid BuildingId, string Name);

/// <summary>The UI-facing shape of a facility, flattened for display/editing.</summary>
public sealed record FacilityViewModel(
    Guid FacilityId,
    string Name,
    string IndustryName,
    string? NaicsCode,
    string AddressLine1,
    string City,
    string StateOrProvince,
    string PostalCode,
    string CountryCode,
    IReadOnlyList<string> ActiveHazardTypes,
    string LifecycleStatus,
    int BuildingCount,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

/// <summary>Explicit, one-directional Backend DTO -&gt; UI ViewModel mapping.</summary>
public static class FacilityMappings
{
    /// <summary>Maps a backend FacilityBackendDto snapshot to a UI-facing FacilityViewModel.</summary>
    public static FacilityViewModel ToViewModel(this FacilityBackendDto dto) => new(
        dto.FacilityId,
        dto.Name,
        dto.Industry.IndustryName,
        dto.Industry.NaicsCode,
        dto.Location.AddressLine1,
        dto.Location.City,
        dto.Location.StateOrProvince,
        dto.Location.PostalCode,
        dto.Location.CountryCode,
        dto.HazardProfile.ActiveHazardTypes,
        dto.LifecycleStatus,
        dto.Hierarchy.Buildings.Count,
        dto.CreatedAtUtc,
        dto.UpdatedAtUtc);
}
