// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries
//  Layer:          Abstractions
//  File:           IndustryRegistry.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Deterministic, in-memory registry of every industry in the
//                  PlantSafety industry taxonomy, expressed as
//                  IndustryDefinition records. TopLevelCategory assignment
//                  mirrors the taxonomy's top-level grouping order:
//                    1. Core Manufacturing        -- Chemical Manufacturing .. Textiles & Apparel
//                    2. Logistics & Transport      -- Warehousing & Distribution .. Bulk Material Handling
//                    3. High Fire/Explosion Risk   -- Explosives & Pyrotechnics .. Printing & Ink
//                    4. High Chemical Hazard       -- Hazardous Waste Treatment .. Industrial Cleaning Chemical Production
//                    5. Heavy Machinery / Industrial -- Mining & Mineral Processing .. Heavy Industrial Fabrication
//                    6. Water/Marine Interface     -- Marine Terminals .. Waterfront Manufacturing
//                    7. High Environmental Compliance -- Power Generation .. Industrial Laundries
//                    8. Specialized                -- Medical Devices .. Cannabis
//                  Regulators and Flags match the source taxonomy exactly.
//                  Industries with no enumerated sub-industries carry an empty
//                  SubIndustries list rather than a null or placeholder value.
//                  Additive-only evolution -- entries must never be renamed,
//                  reordered in a way that changes meaning, or have their
//                  Regulators/Flags altered without a corresponding taxonomy
//                  change.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models.Industries;

/// <summary>
/// Deterministic, read-only registry of every industry in the PlantSafety
/// industry taxonomy.
/// </summary>
public static class IndustryRegistry
{
    private const string CoreManufacturing = "Core Manufacturing";
    private const string LogisticsAndTransport = "Logistics & Transport";
    private const string HighFireExplosionRisk = "High Fire/Explosion Risk";
    private const string HighChemicalHazard = "High Chemical Hazard";
    private const string HeavyMachineryIndustrial = "Heavy Machinery / Industrial";
    private const string WaterMarineInterface = "Water/Marine Interface";
    private const string HighEnvironmentalCompliance = "High Environmental Compliance";
    private const string Specialized = "Specialized";

    private static readonly IReadOnlyList<string> NoSubIndustries = Array.Empty<string>();

    /// <summary>Every industry in the PlantSafety industry taxonomy, in taxonomy order.</summary>
    public static IReadOnlyList<IndustryDefinition> All { get; } = new IndustryDefinition[]
    {
        new(
            Name: "Chemical Manufacturing",
            TopLevelCategory: CoreManufacturing,
            SubIndustries: new[] { "Bulk chemicals", "solvents", "resins", "polymers", "fertilizers", "industrial gases" },
            Regulators: new[] { Regulator.OSHA, Regulator.EPA_RCRA, Regulator.EPA_TSCA, Regulator.PHMSA, Regulator.NFPA, Regulator.FMCSA, Regulator.DHS_CFATS },
            Flags: SafetyFlag.Machinery | SafetyFlag.Chemicals | SafetyFlag.Flammables | SafetyFlag.HazmatShipping | SafetyFlag.Fleet | SafetyFlag.EnvironmentalPermits | SafetyFlag.ExplosionRisk | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Petrochemical & Refining",
            TopLevelCategory: CoreManufacturing,
            SubIndustries: new[] { "Refineries", "lubricants", "fuels", "asphalt" },
            Regulators: new[] { Regulator.OSHA, Regulator.EPA, Regulator.PHMSA, Regulator.NFPA, Regulator.FMCSA, Regulator.USCG },
            Flags: SafetyFlag.Machinery | SafetyFlag.Chemicals | SafetyFlag.Flammables | SafetyFlag.HazmatShipping | SafetyFlag.Fleet | SafetyFlag.EnvironmentalPermits | SafetyFlag.ExplosionRisk | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Pharmaceuticals & Biotech",
            TopLevelCategory: CoreManufacturing,
            SubIndustries: new[] { "Drug manufacturing", "sterile processing", "biologics" },
            Regulators: new[] { Regulator.OSHA, Regulator.FDA, Regulator.EPA, Regulator.NFPA },
            Flags: SafetyFlag.Machinery | SafetyFlag.Chemicals | SafetyFlag.EnvironmentalPermits | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Food & Beverage",
            TopLevelCategory: CoreManufacturing,
            SubIndustries: new[] { "Meat", "dairy", "beverage bottling", "grain milling" },
            Regulators: new[] { Regulator.OSHA, Regulator.FDA, Regulator.EPA, Regulator.NFPA },
            Flags: SafetyFlag.Machinery | SafetyFlag.Chemicals | SafetyFlag.Flammables | SafetyFlag.EnvironmentalPermits | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Automotive & Heavy Equipment",
            TopLevelCategory: CoreManufacturing,
            SubIndustries: new[] { "Assembly", "stamping", "machining", "robotics" },
            Regulators: new[] { Regulator.OSHA, Regulator.NFPA, Regulator.EPA, Regulator.FMCSA, Regulator.ANSI_ISO },
            Flags: SafetyFlag.Machinery | SafetyFlag.Flammables | SafetyFlag.Fleet | SafetyFlag.EnvironmentalPermits | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Aerospace & Defense",
            TopLevelCategory: CoreManufacturing,
            SubIndustries: new[] { "Components", "propulsion", "composites", "electronics" },
            Regulators: new[] { Regulator.OSHA, Regulator.EPA, Regulator.NFPA, Regulator.ATF, Regulator.DoD },
            Flags: SafetyFlag.Machinery | SafetyFlag.Chemicals | SafetyFlag.Flammables | SafetyFlag.ExplosionRisk | SafetyFlag.EnvironmentalPermits | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Electronics & Semiconductor",
            TopLevelCategory: CoreManufacturing,
            SubIndustries: new[] { "Chip fabs", "PCB assembly", "batteries" },
            Regulators: new[] { Regulator.OSHA, Regulator.EPA, Regulator.NFPA, Regulator.PHMSA },
            Flags: SafetyFlag.Machinery | SafetyFlag.Chemicals | SafetyFlag.Flammables | SafetyFlag.HazmatShipping | SafetyFlag.EnvironmentalPermits | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Plastics & Rubber",
            TopLevelCategory: CoreManufacturing,
            SubIndustries: new[] { "Extrusion", "molding", "compounding" },
            Regulators: new[] { Regulator.OSHA, Regulator.EPA, Regulator.NFPA },
            Flags: SafetyFlag.Machinery | SafetyFlag.Chemicals | SafetyFlag.Flammables | SafetyFlag.EnvironmentalPermits | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Metal Fabrication & Foundries",
            TopLevelCategory: CoreManufacturing,
            SubIndustries: new[] { "Welding", "machining", "casting", "forging" },
            Regulators: new[] { Regulator.OSHA, Regulator.NFPA, Regulator.EPA },
            Flags: SafetyFlag.Machinery | SafetyFlag.Flammables | SafetyFlag.Dust | SafetyFlag.EnvironmentalPermits | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Wood Products & Furniture",
            TopLevelCategory: CoreManufacturing,
            SubIndustries: new[] { "Sawmills", "cabinetry", "furniture" },
            Regulators: new[] { Regulator.OSHA, Regulator.NFPA_Dust, Regulator.EPA },
            Flags: SafetyFlag.Machinery | SafetyFlag.Dust | SafetyFlag.Flammables | SafetyFlag.EnvironmentalPermits | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Paper & Pulp",
            TopLevelCategory: CoreManufacturing,
            SubIndustries: new[] { "Pulp mills", "paper plants" },
            Regulators: new[] { Regulator.OSHA, Regulator.EPA, Regulator.NFPA },
            Flags: SafetyFlag.Machinery | SafetyFlag.Chemicals | SafetyFlag.Flammables | SafetyFlag.EnvironmentalPermits | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Textiles & Apparel",
            TopLevelCategory: CoreManufacturing,
            SubIndustries: new[] { "Weaving", "dyeing", "finishing" },
            Regulators: new[] { Regulator.OSHA, Regulator.EPA, Regulator.NFPA },
            Flags: SafetyFlag.Machinery | SafetyFlag.Chemicals | SafetyFlag.Flammables | SafetyFlag.EnvironmentalPermits | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Warehousing & Distribution",
            TopLevelCategory: LogisticsAndTransport,
            SubIndustries: NoSubIndustries,
            Regulators: new[] { Regulator.FMCSA, Regulator.PHMSA, Regulator.OSHA, Regulator.EPA },
            Flags: SafetyFlag.Machinery | SafetyFlag.HazmatShipping | SafetyFlag.Fleet | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Cold Storage",
            TopLevelCategory: LogisticsAndTransport,
            SubIndustries: NoSubIndustries,
            Regulators: new[] { Regulator.FMCSA, Regulator.PHMSA, Regulator.OSHA, Regulator.EPA },
            Flags: SafetyFlag.Machinery | SafetyFlag.HazmatShipping | SafetyFlag.Fleet | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "E‑commerce Fulfillment",
            TopLevelCategory: LogisticsAndTransport,
            SubIndustries: NoSubIndustries,
            Regulators: new[] { Regulator.FMCSA, Regulator.PHMSA, Regulator.OSHA, Regulator.EPA },
            Flags: SafetyFlag.Machinery | SafetyFlag.HazmatShipping | SafetyFlag.Fleet | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Third‑Party Logistics",
            TopLevelCategory: LogisticsAndTransport,
            SubIndustries: NoSubIndustries,
            Regulators: new[] { Regulator.FMCSA, Regulator.PHMSA, Regulator.OSHA, Regulator.EPA },
            Flags: SafetyFlag.Machinery | SafetyFlag.HazmatShipping | SafetyFlag.Fleet | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Bulk Material Handling",
            TopLevelCategory: LogisticsAndTransport,
            SubIndustries: NoSubIndustries,
            Regulators: new[] { Regulator.FMCSA, Regulator.PHMSA, Regulator.OSHA, Regulator.EPA },
            Flags: SafetyFlag.Machinery | SafetyFlag.Dust | SafetyFlag.HazmatShipping | SafetyFlag.Fleet | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Explosives & Pyrotechnics",
            TopLevelCategory: HighFireExplosionRisk,
            SubIndustries: NoSubIndustries,
            Regulators: new[] { Regulator.NFPA, Regulator.ATF, Regulator.OSHA, Regulator.EPA },
            Flags: SafetyFlag.Machinery | SafetyFlag.Chemicals | SafetyFlag.Flammables | SafetyFlag.ExplosionRisk | SafetyFlag.EnvironmentalPermits | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Ammunition & Firearms",
            TopLevelCategory: HighFireExplosionRisk,
            SubIndustries: NoSubIndustries,
            Regulators: new[] { Regulator.NFPA, Regulator.ATF, Regulator.OSHA, Regulator.EPA },
            Flags: SafetyFlag.Machinery | SafetyFlag.Chemicals | SafetyFlag.Flammables | SafetyFlag.ExplosionRisk | SafetyFlag.EnvironmentalPermits | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Paints, Coatings, Adhesives",
            TopLevelCategory: HighFireExplosionRisk,
            SubIndustries: NoSubIndustries,
            Regulators: new[] { Regulator.NFPA, Regulator.OSHA, Regulator.EPA },
            Flags: SafetyFlag.Machinery | SafetyFlag.Chemicals | SafetyFlag.Flammables | SafetyFlag.ExplosionRisk | SafetyFlag.EnvironmentalPermits | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Printing & Ink",
            TopLevelCategory: HighFireExplosionRisk,
            SubIndustries: NoSubIndustries,
            Regulators: new[] { Regulator.NFPA, Regulator.OSHA, Regulator.EPA },
            Flags: SafetyFlag.Machinery | SafetyFlag.Chemicals | SafetyFlag.Flammables | SafetyFlag.EnvironmentalPermits | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Hazardous Waste Treatment",
            TopLevelCategory: HighChemicalHazard,
            SubIndustries: NoSubIndustries,
            Regulators: new[] { Regulator.EPA_RCRA, Regulator.PHMSA, Regulator.OSHA, Regulator.NFPA },
            Flags: SafetyFlag.Machinery | SafetyFlag.Chemicals | SafetyFlag.HazmatShipping | SafetyFlag.EnvironmentalPermits | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Chemical Blending / Tolling",
            TopLevelCategory: HighChemicalHazard,
            SubIndustries: NoSubIndustries,
            Regulators: new[] { Regulator.EPA_RCRA, Regulator.PHMSA, Regulator.OSHA, Regulator.NFPA },
            Flags: SafetyFlag.Machinery | SafetyFlag.Chemicals | SafetyFlag.HazmatShipping | SafetyFlag.EnvironmentalPermits | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Industrial Cleaning Chemical Production",
            TopLevelCategory: HighChemicalHazard,
            SubIndustries: NoSubIndustries,
            Regulators: new[] { Regulator.EPA_RCRA, Regulator.PHMSA, Regulator.OSHA, Regulator.NFPA },
            Flags: SafetyFlag.Machinery | SafetyFlag.Chemicals | SafetyFlag.HazmatShipping | SafetyFlag.EnvironmentalPermits | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Mining & Mineral Processing",
            TopLevelCategory: HeavyMachineryIndustrial,
            SubIndustries: NoSubIndustries,
            Regulators: new[] { Regulator.OSHA, Regulator.NFPA, Regulator.MSHA, Regulator.EPA },
            Flags: SafetyFlag.Machinery | SafetyFlag.Dust | SafetyFlag.Flammables | SafetyFlag.EnvironmentalPermits | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Cement & Concrete",
            TopLevelCategory: HeavyMachineryIndustrial,
            SubIndustries: NoSubIndustries,
            Regulators: new[] { Regulator.OSHA, Regulator.NFPA, Regulator.EPA },
            Flags: SafetyFlag.Machinery | SafetyFlag.Dust | SafetyFlag.EnvironmentalPermits | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Steel Mills & Rolling",
            TopLevelCategory: HeavyMachineryIndustrial,
            SubIndustries: NoSubIndustries,
            Regulators: new[] { Regulator.OSHA, Regulator.NFPA, Regulator.EPA },
            Flags: SafetyFlag.Machinery | SafetyFlag.Flammables | SafetyFlag.EnvironmentalPermits | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Heavy Industrial Fabrication",
            TopLevelCategory: HeavyMachineryIndustrial,
            SubIndustries: NoSubIndustries,
            Regulators: new[] { Regulator.OSHA, Regulator.NFPA, Regulator.EPA },
            Flags: SafetyFlag.Machinery | SafetyFlag.Flammables | SafetyFlag.EnvironmentalPermits | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Marine Terminals",
            TopLevelCategory: WaterMarineInterface,
            SubIndustries: NoSubIndustries,
            Regulators: new[] { Regulator.USCG, Regulator.EPA, Regulator.OSHA, Regulator.PHMSA },
            Flags: SafetyFlag.Machinery | SafetyFlag.HazmatShipping | SafetyFlag.EnvironmentalPermits | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Bulk Liquid Transfer",
            TopLevelCategory: WaterMarineInterface,
            SubIndustries: NoSubIndustries,
            Regulators: new[] { Regulator.USCG, Regulator.EPA, Regulator.OSHA, Regulator.PHMSA },
            Flags: SafetyFlag.Machinery | SafetyFlag.Chemicals | SafetyFlag.HazmatShipping | SafetyFlag.EnvironmentalPermits | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Waterfront Manufacturing",
            TopLevelCategory: WaterMarineInterface,
            SubIndustries: NoSubIndustries,
            Regulators: new[] { Regulator.USCG, Regulator.EPA, Regulator.OSHA, Regulator.PHMSA },
            Flags: SafetyFlag.Machinery | SafetyFlag.Chemicals | SafetyFlag.HazmatShipping | SafetyFlag.EnvironmentalPermits | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Power Generation",
            TopLevelCategory: HighEnvironmentalCompliance,
            SubIndustries: NoSubIndustries,
            Regulators: new[] { Regulator.EPA, Regulator.OSHA, Regulator.NFPA },
            Flags: SafetyFlag.Machinery | SafetyFlag.Chemicals | SafetyFlag.Flammables | SafetyFlag.EnvironmentalPermits | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Renewable Energy Manufacturing",
            TopLevelCategory: HighEnvironmentalCompliance,
            SubIndustries: NoSubIndustries,
            Regulators: new[] { Regulator.EPA, Regulator.OSHA, Regulator.NFPA },
            Flags: SafetyFlag.Machinery | SafetyFlag.Chemicals | SafetyFlag.Flammables | SafetyFlag.EnvironmentalPermits | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Wastewater Treatment",
            TopLevelCategory: HighEnvironmentalCompliance,
            SubIndustries: NoSubIndustries,
            Regulators: new[] { Regulator.EPA, Regulator.OSHA, Regulator.NFPA },
            Flags: SafetyFlag.Machinery | SafetyFlag.Chemicals | SafetyFlag.EnvironmentalPermits | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Industrial Laundries",
            TopLevelCategory: HighEnvironmentalCompliance,
            SubIndustries: NoSubIndustries,
            Regulators: new[] { Regulator.EPA, Regulator.OSHA, Regulator.NFPA },
            Flags: SafetyFlag.Machinery | SafetyFlag.Chemicals | SafetyFlag.EnvironmentalPermits | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Medical Devices",
            TopLevelCategory: Specialized,
            SubIndustries: NoSubIndustries,
            Regulators: new[] { Regulator.FDA, Regulator.OSHA },
            Flags: SafetyFlag.Machinery | SafetyFlag.EnvironmentalPermits | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Cosmetics",
            TopLevelCategory: Specialized,
            SubIndustries: NoSubIndustries,
            Regulators: new[] { Regulator.FDA, Regulator.OSHA },
            Flags: SafetyFlag.Machinery | SafetyFlag.Chemicals | SafetyFlag.EnvironmentalPermits | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Agricultural Processing",
            TopLevelCategory: Specialized,
            SubIndustries: NoSubIndustries,
            Regulators: new[] { Regulator.OSHA, Regulator.EPA },
            Flags: SafetyFlag.Machinery | SafetyFlag.Dust | SafetyFlag.Chemicals | SafetyFlag.EnvironmentalPermits | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Tobacco",
            TopLevelCategory: Specialized,
            SubIndustries: NoSubIndustries,
            Regulators: new[] { Regulator.OSHA, Regulator.FDA },
            Flags: SafetyFlag.Machinery | SafetyFlag.Chemicals | SafetyFlag.EnvironmentalPermits | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),

        new(
            Name: "Cannabis",
            TopLevelCategory: Specialized,
            SubIndustries: NoSubIndustries,
            Regulators: new[] { Regulator.OSHA, Regulator.EPA, Regulator.NFPA },
            Flags: SafetyFlag.Machinery | SafetyFlag.Chemicals | SafetyFlag.Flammables | SafetyFlag.EnvironmentalPermits | SafetyFlag.ConfinedSpaces | SafetyFlag.LOTO),
    };

    /// <summary>Looks up an industry definition by its exact, canonical name.</summary>
    public static IndustryDefinition? FindByName(string name) =>
        All.FirstOrDefault(industry => string.Equals(industry.Name, name, StringComparison.Ordinal));

    /// <summary>All industry definitions belonging to the given top-level category.</summary>
    public static IReadOnlyList<IndustryDefinition> ByTopLevelCategory(string topLevelCategory) =>
        All.Where(industry => string.Equals(industry.TopLevelCategory, topLevelCategory, StringComparison.Ordinal)).ToArray();
}
