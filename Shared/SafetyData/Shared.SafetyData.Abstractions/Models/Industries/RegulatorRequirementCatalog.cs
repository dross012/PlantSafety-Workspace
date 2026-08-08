// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries
//  Layer:          Abstractions
//  File:           RegulatorRequirementCatalog.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Deterministic, in-memory catalog of every regulatory
//                  requirement in the Regulatory Subsystem, expressed as
//                  RegulatorRequirement records. Every one of the 15 existing
//                  Regulator values (including FMCSA, ANSI_ISO, and DoD, which
//                  RegulatorCategoryCatalog folds into the OSHA/ATF category
//                  for grouping purposes only) has its own entries here, drawn
//                  from that regulator's real, citable regulatory scope --
//                  never from the category it happens to fold into. Category
//                  is always computed via RegulatorCategoryCatalog.ForRegulator
//                  rather than hardcoded, so the two can never drift apart.
//                  Introduces no new regulators, categories, or industries.
//                  Additive-only evolution -- existing entries must never be
//                  renamed or have their Regulator/Category changed; new
//                  entries may only be added under an existing Regulator
//                  value, drawn from that regulator's real scope.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models.Industries;

/// <summary>
/// Deterministic, read-only catalog of every regulatory requirement in the
/// Regulatory Subsystem.
/// </summary>
public static class RegulatorRequirementCatalog
{
    /// <summary>Every regulatory requirement, grouped by regulator in <see cref="Regulator"/> declaration order.</summary>
    public static IReadOnlyList<RegulatorRequirement> All { get; } = new[]
    {
        // -- OSHA ------------------------------------------------------------
        Requirement("Lockout/Tagout (LOTO)", "Establishes procedures for controlling hazardous energy during service and maintenance of machinery and equipment (29 CFR 1910.147).", Regulator.OSHA),
        Requirement("Hazard Communication (HazCom)", "Requires labeling, Safety Data Sheets, and employee training for hazardous chemicals in the workplace (29 CFR 1910.1200).", Regulator.OSHA),
        Requirement("Powered Industrial Trucks (PIT)", "Establishes training and operating requirements for forklifts and other powered industrial trucks (29 CFR 1910.178).", Regulator.OSHA),
        Requirement("Permit-Required Confined Spaces", "Establishes entry permit, atmospheric testing, and rescue requirements for permit-required confined spaces (29 CFR 1910.146).", Regulator.OSHA),
        Requirement("Machine Guarding", "Requires guarding of points of operation, rotating parts, and other machine hazards (29 CFR 1910.212).", Regulator.OSHA),

        // -- EPA ---------------------------------------------------------------
        Requirement("Spill Prevention, Control, and Countermeasure (SPCC)", "Requires spill prevention planning and secondary containment for facilities storing oil above regulatory thresholds (40 CFR 112).", Regulator.EPA),
        Requirement("Clean Air Act Operating Permit", "Requires permitting and emissions control for stationary sources of air pollutants (40 CFR Part 70).", Regulator.EPA),
        Requirement("Clean Water Act Discharge Permit (NPDES)", "Requires permitting for point-source discharges of pollutants to waters of the United States (40 CFR 122).", Regulator.EPA),

        // -- EPA_RCRA ------------------------------------------------------
        Requirement("RCRA Hazardous Waste Generator Status", "Requires hazardous waste identification, manifesting, and generator-status compliance for facilities generating hazardous waste (40 CFR 262).", Regulator.EPA_RCRA),

        // -- EPA_TSCA ------------------------------------------------------
        Requirement("TSCA Chemical Substance Reporting", "Requires inventory reporting and use restrictions for chemical substances manufactured, processed, or imported (40 CFR Part 711).", Regulator.EPA_TSCA),

        // -- NFPA ------------------------------------------------------------
        Requirement("Flammable and Combustible Liquids Code", "Establishes storage, handling, and dispensing requirements for flammable and combustible liquids (NFPA 30).", Regulator.NFPA),
        Requirement("Hot Work Permit Program", "Requires a permit system and fire watch for welding, cutting, and other hot work operations (NFPA 51B).", Regulator.NFPA),
        Requirement("Fire Protection Systems", "Establishes installation, inspection, and maintenance requirements for fire suppression and detection systems (NFPA 25 / NFPA 72).", Regulator.NFPA),

        // -- NFPA_Dust -------------------------------------------------------
        Requirement("Combustible Dust Hazard Analysis", "Requires a documented dust hazard analysis and housekeeping/explosion-protection controls for processes generating combustible dust (NFPA 652).", Regulator.NFPA_Dust),

        // -- PHMSA -----------------------------------------------------------
        Requirement("Hazardous Materials Shipping Registration", "Requires registration for offerors and carriers of hazardous materials in commerce (49 CFR 107 Subpart G).", Regulator.PHMSA),
        Requirement("Hazardous Materials Packaging", "Establishes performance-tested packaging requirements for hazardous materials shipments (49 CFR Part 178).", Regulator.PHMSA),
        Requirement("Hazardous Materials Placarding", "Requires placarding of transport vehicles carrying hazardous materials above threshold quantities (49 CFR Part 172 Subpart F).", Regulator.PHMSA),

        // -- FMCSA (category-folded into OSHA; own regulatory scope) --------
        Requirement("Motor Carrier Operating Authority", "Requires registration and operating authority for interstate motor carriers (49 CFR Part 365).", Regulator.FMCSA),
        Requirement("Driver Qualification Files", "Requires maintenance of driver qualification files including medical certification and licensing (49 CFR Part 391).", Regulator.FMCSA),
        Requirement("Vehicle Inspection, Repair, and Maintenance", "Requires systematic inspection, repair, and maintenance of commercial motor vehicles (49 CFR Part 396).", Regulator.FMCSA),

        // -- DHS_CFATS ---------------------------------------------------------
        Requirement("Chemical Facility Anti-Terrorism Standards (CFATS) Site Security Plan", "Requires a risk-based site security plan for facilities possessing chemicals of interest above screening threshold quantities (6 CFR Part 27).", Regulator.DHS_CFATS),

        // -- USCG --------------------------------------------------------------
        Requirement("Facility Security Plan (Waterfront)", "Requires an approved facility security plan for waterfront facilities handling regulated cargo (33 CFR Part 105).", Regulator.USCG),
        Requirement("Bulk Liquid Transfer Operations", "Establishes operational and equipment requirements for bulk transfer of liquid cargo between vessel and shore (33 CFR Part 154/156).", Regulator.USCG),
        Requirement("Maritime Security (MARSEC)", "Requires compliance with maritime security levels and access-control measures at regulated facilities (33 CFR Part 101).", Regulator.USCG),

        // -- FDA -------------------------------------------------------------
        Requirement("Current Good Manufacturing Practice (cGMP)", "Establishes quality controls for the manufacture, processing, and packaging of regulated products (21 CFR Part 211 / Part 117).", Regulator.FDA),
        Requirement("Sanitary Facility Operations", "Requires sanitation controls for facilities, equipment, and personnel handling regulated products (21 CFR Part 117 Subpart B).", Regulator.FDA),
        Requirement("Quality Systems Regulation", "Establishes design control and quality management system requirements for regulated device/drug manufacturers (21 CFR Part 820).", Regulator.FDA),

        // -- ANSI_ISO (category-folded into OSHA; own regulatory scope) -----
        Requirement("Consensus Standard Conformance", "Requires conformance to applicable ANSI/ISO consensus standards incorporated by reference into OSHA regulations (e.g., ANSI B11 machine safety, ISO 45001 occupational health and safety management).", Regulator.ANSI_ISO),

        // -- ATF -------------------------------------------------------------
        Requirement("Federal Explosives License/Permit", "Requires licensing for the manufacture, storage, and distribution of explosive materials (27 CFR Part 555).", Regulator.ATF),
        Requirement("Ammunition Manufacturing License", "Requires licensing for the manufacture of ammunition and ammunition components (27 CFR Part 478).", Regulator.ATF),
        Requirement("Explosives Magazine Storage", "Establishes construction, siting, and inventory requirements for explosives storage magazines (27 CFR Part 555 Subpart K).", Regulator.ATF),

        // -- DoD (category-folded into ATF; own regulatory scope) ------------
        Requirement("System Safety Program (MIL-STD-882)", "Requires a documented system safety program identifying and mitigating hazards across the system lifecycle (MIL-STD-882E).", Regulator.DoD),
        Requirement("Explosives Safety Siting", "Requires quantity-distance siting analysis for facilities handling military energetics and munitions (DoD 6055.09-M).", Regulator.DoD),

        // -- MSHA ------------------------------------------------------------
        Requirement("Mine Safety Training", "Requires new-miner and annual refresher safety training for mine employees (30 CFR Part 48).", Regulator.MSHA),
        Requirement("Mining Equipment Safety Standards", "Establishes safety design and maintenance standards for mining equipment (30 CFR Part 56/57).", Regulator.MSHA),
        Requirement("Mine Ventilation Plan", "Requires an approved ventilation plan to control airborne contaminants and methane in mine workings (30 CFR Part 75).", Regulator.MSHA),
    };

    /// <summary>Every requirement drawn from the given regulator, in catalog order.</summary>
    public static IReadOnlyList<RegulatorRequirement> ForRegulator(Regulator regulator) =>
        All.Where(requirement => requirement.Regulator == regulator).ToArray();

    /// <summary>Every requirement whose regulator maps onto the given category, in catalog order.</summary>
    public static IReadOnlyList<RegulatorRequirement> ForCategory(RegulatorCategory category) =>
        All.Where(requirement => requirement.Category == category).ToArray();

    private static RegulatorRequirement Requirement(string name, string description, Regulator regulator) =>
        new(name, description, regulator, RegulatorCategoryCatalog.ForRegulator(regulator));
}
