-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase2-SafetyTaxonomy
--  Layer:          Infrastructure - SQL Scripts
--  File:           17-seed-regulatoryrequirement.sql
--  Created:        2026-08-08
--  Author:         Platform Team
--  SchemaVersion:  2026.1
--  Description:    Seeds [SafetyTaxonomy].[RegulatoryRequirement] with all
--                  36 requirements from RegulatorRequirementCatalog.All,
--                  verbatim, in source-literal order (RequirementId 1-36).
--                  RegulatorId values follow 12-seed-regulator.sql's
--                  mapping. RegulatorCategoryId is computed the same way
--                  RegulatorRequirementCatalog.Requirement() computes
--                  Category -- via RegulatorCategoryCatalog.ForRegulator(regulator)
--                  -- never hand-entered independently of RegulatorId.
--
--  SESSION CONTEXT: see 10-seed-safetyflag.sql's header -- same pattern.
--  Idempotent: MERGE keyed on RequirementId.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

EXEC sys.sp_set_session_context @key = N'SafetyTaxonomySeedContext', @value = 1;
GO

MERGE [SafetyTaxonomy].[RegulatoryRequirement] AS target
USING (VALUES
    -- OSHA (RegulatorId 1, RegulatorCategoryId 1)
    (1,  N'Lockout/Tagout (LOTO)',
         N'Establishes procedures for controlling hazardous energy during service and maintenance of machinery and equipment (29 CFR 1910.147).', 1, 1),
    (2,  N'Hazard Communication (HazCom)',
         N'Requires labeling, Safety Data Sheets, and employee training for hazardous chemicals in the workplace (29 CFR 1910.1200).', 1, 1),
    (3,  N'Powered Industrial Trucks (PIT)',
         N'Establishes training and operating requirements for forklifts and other powered industrial trucks (29 CFR 1910.178).', 1, 1),
    (4,  N'Permit-Required Confined Spaces',
         N'Establishes entry permit, atmospheric testing, and rescue requirements for permit-required confined spaces (29 CFR 1910.146).', 1, 1),
    (5,  N'Machine Guarding',
         N'Requires guarding of points of operation, rotating parts, and other machine hazards (29 CFR 1910.212).', 1, 1),
    -- EPA (RegulatorId 2, RegulatorCategoryId 2)
    (6,  N'Spill Prevention, Control, and Countermeasure (SPCC)',
         N'Requires spill prevention planning and secondary containment for facilities storing oil above regulatory thresholds (40 CFR 112).', 2, 2),
    (7,  N'Clean Air Act Operating Permit',
         N'Requires permitting and emissions control for stationary sources of air pollutants (40 CFR Part 70).', 2, 2),
    (8,  N'Clean Water Act Discharge Permit (NPDES)',
         N'Requires permitting for point-source discharges of pollutants to waters of the United States (40 CFR 122).', 2, 2),
    -- EPA_RCRA (RegulatorId 3, RegulatorCategoryId 2)
    (9,  N'RCRA Hazardous Waste Generator Status',
         N'Requires hazardous waste identification, manifesting, and generator-status compliance for facilities generating hazardous waste (40 CFR 262).', 3, 2),
    -- EPA_TSCA (RegulatorId 4, RegulatorCategoryId 2)
    (10, N'TSCA Chemical Substance Reporting',
         N'Requires inventory reporting and use restrictions for chemical substances manufactured, processed, or imported (40 CFR Part 711).', 4, 2),
    -- NFPA (RegulatorId 6, RegulatorCategoryId 3)
    (11, N'Flammable and Combustible Liquids Code',
         N'Establishes storage, handling, and dispensing requirements for flammable and combustible liquids (NFPA 30).', 6, 3),
    (12, N'Hot Work Permit Program',
         N'Requires a permit system and fire watch for welding, cutting, and other hot work operations (NFPA 51B).', 6, 3),
    (13, N'Fire Protection Systems',
         N'Establishes installation, inspection, and maintenance requirements for fire suppression and detection systems (NFPA 25 / NFPA 72).', 6, 3),
    -- NFPA_Dust (RegulatorId 7, RegulatorCategoryId 3)
    (14, N'Combustible Dust Hazard Analysis',
         N'Requires a documented dust hazard analysis and housekeeping/explosion-protection controls for processes generating combustible dust (NFPA 652).', 7, 3),
    -- PHMSA (RegulatorId 5, RegulatorCategoryId 4)
    (15, N'Hazardous Materials Shipping Registration',
         N'Requires registration for offerors and carriers of hazardous materials in commerce (49 CFR 107 Subpart G).', 5, 4),
    (16, N'Hazardous Materials Packaging',
         N'Establishes performance-tested packaging requirements for hazardous materials shipments (49 CFR Part 178).', 5, 4),
    (17, N'Hazardous Materials Placarding',
         N'Requires placarding of transport vehicles carrying hazardous materials above threshold quantities (49 CFR Part 172 Subpart F).', 5, 4),
    -- FMCSA (RegulatorId 8, RegulatorCategoryId 1 -- category-folded into OSHA; own regulatory scope)
    (18, N'Motor Carrier Operating Authority',
         N'Requires registration and operating authority for interstate motor carriers (49 CFR Part 365).', 8, 1),
    (19, N'Driver Qualification Files',
         N'Requires maintenance of driver qualification files including medical certification and licensing (49 CFR Part 391).', 8, 1),
    (20, N'Vehicle Inspection, Repair, and Maintenance',
         N'Requires systematic inspection, repair, and maintenance of commercial motor vehicles (49 CFR Part 396).', 8, 1),
    -- DHS_CFATS (RegulatorId 9, RegulatorCategoryId 9)
    (21, N'Chemical Facility Anti-Terrorism Standards (CFATS) Site Security Plan',
         N'Requires a risk-based site security plan for facilities possessing chemicals of interest above screening threshold quantities (6 CFR Part 27).', 9, 9),
    -- USCG (RegulatorId 10, RegulatorCategoryId 8)
    (22, N'Facility Security Plan (Waterfront)',
         N'Requires an approved facility security plan for waterfront facilities handling regulated cargo (33 CFR Part 105).', 10, 8),
    (23, N'Bulk Liquid Transfer Operations',
         N'Establishes operational and equipment requirements for bulk transfer of liquid cargo between vessel and shore (33 CFR Part 154/156).', 10, 8),
    (24, N'Maritime Security (MARSEC)',
         N'Requires compliance with maritime security levels and access-control measures at regulated facilities (33 CFR Part 101).', 10, 8),
    -- FDA (RegulatorId 11, RegulatorCategoryId 5)
    (25, N'Current Good Manufacturing Practice (cGMP)',
         N'Establishes quality controls for the manufacture, processing, and packaging of regulated products (21 CFR Part 211 / Part 117).', 11, 5),
    (26, N'Sanitary Facility Operations',
         N'Requires sanitation controls for facilities, equipment, and personnel handling regulated products (21 CFR Part 117 Subpart B).', 11, 5),
    (27, N'Quality Systems Regulation',
         N'Establishes design control and quality management system requirements for regulated device/drug manufacturers (21 CFR Part 820).', 11, 5),
    -- ANSI_ISO (RegulatorId 12, RegulatorCategoryId 1 -- category-folded into OSHA; own regulatory scope)
    (28, N'Consensus Standard Conformance',
         N'Requires conformance to applicable ANSI/ISO consensus standards incorporated by reference into OSHA regulations (e.g., ANSI B11 machine safety, ISO 45001 occupational health and safety management).', 12, 1),
    -- ATF (RegulatorId 13, RegulatorCategoryId 6)
    (29, N'Federal Explosives License/Permit',
         N'Requires licensing for the manufacture, storage, and distribution of explosive materials (27 CFR Part 555).', 13, 6),
    (30, N'Ammunition Manufacturing License',
         N'Requires licensing for the manufacture of ammunition and ammunition components (27 CFR Part 478).', 13, 6),
    (31, N'Explosives Magazine Storage',
         N'Establishes construction, siting, and inventory requirements for explosives storage magazines (27 CFR Part 555 Subpart K).', 13, 6),
    -- DoD (RegulatorId 14, RegulatorCategoryId 6 -- category-folded into ATF; own regulatory scope)
    (32, N'System Safety Program (MIL-STD-882)',
         N'Requires a documented system safety program identifying and mitigating hazards across the system lifecycle (MIL-STD-882E).', 14, 6),
    (33, N'Explosives Safety Siting',
         N'Requires quantity-distance siting analysis for facilities handling military energetics and munitions (DoD 6055.09-M).', 14, 6),
    -- MSHA (RegulatorId 15, RegulatorCategoryId 7)
    (34, N'Mine Safety Training',
         N'Requires new-miner and annual refresher safety training for mine employees (30 CFR Part 48).', 15, 7),
    (35, N'Mining Equipment Safety Standards',
         N'Establishes safety design and maintenance standards for mining equipment (30 CFR Part 56/57).', 15, 7),
    (36, N'Mine Ventilation Plan',
         N'Requires an approved ventilation plan to control airborne contaminants and methane in mine workings (30 CFR Part 75).', 15, 7)
) AS source ([RequirementId], [Name], [Description], [RegulatorId], [RegulatorCategoryId])
ON target.[RequirementId] = source.[RequirementId]
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([RequirementId], [Name], [Description], [RegulatorId], [RegulatorCategoryId])
    VALUES (source.[RequirementId], source.[Name], source.[Description], source.[RegulatorId], source.[RegulatorCategoryId])
WHEN MATCHED THEN
    UPDATE SET
        target.[Name]                 = source.[Name],
        target.[Description]          = source.[Description],
        target.[RegulatorId]          = source.[RegulatorId],
        target.[RegulatorCategoryId]  = source.[RegulatorCategoryId];
GO

EXEC sys.sp_set_session_context @key = N'SafetyTaxonomySeedContext', @value = 0;
GO
