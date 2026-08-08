-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase2-SafetyTaxonomy
--  Layer:          Infrastructure - SQL Scripts
--  File:           18-seed-safetyprogram.sql
--  Created:        2026-08-08
--  Author:         Platform Team
--  SchemaVersion:  2026.1
--  Description:    Seeds [SafetyTaxonomy].[SafetyProgram] with all 36
--                  programs from ProgramCatalog.All, verbatim, in
--                  ProgramCatalog.All order (Regulator enum declaration
--                  order: OSHA, EPA, EPA_RCRA, EPA_TSCA, PHMSA, NFPA,
--                  NFPA_Dust, FMCSA, DHS_CFATS, USCG, FDA, ANSI_ISO, ATF,
--                  DoD, MSHA -- note this differs from
--                  RegulatorRequirementCatalog.All's own literal source
--                  order, which groups NFPA/NFPA_Dust before PHMSA; the
--                  Program ordering below matches
--                  Enum.GetValues<Regulator>().SelectMany(ForRegulator),
--                  which is what ProgramCatalog.cs actually iterates).
--                  Code, Name, Description, and PrimaryFlagId (nullable)
--                  are copied verbatim per ProgramCatalog.cs's construction
--                  rule and PrimaryFlagFor's documented per-regulator/
--                  per-requirement derivation (see that file's header for
--                  the full rationale table). SourceRequirementId is the
--                  1:1 back-reference to 17-seed-regulatoryrequirement.sql's
--                  RequirementId for the exact same requirement Name.
--
--  SESSION CONTEXT: see 10-seed-safetyflag.sql's header -- same pattern.
--  Idempotent: MERGE keyed on ProgramId.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

EXEC sys.sp_set_session_context @key = N'SafetyTaxonomySeedContext', @value = 1;
GO

MERGE [SafetyTaxonomy].[SafetyProgram] AS target
USING (VALUES
    -- OSHA (PrimaryRegulatorId 1, RegulatorCategoryId 1) -- per-requirement PrimaryFlag rule
    (1,  N'PROG-OSHA-01',      N'Lockout/Tagout (LOTO)',
         N'Establishes procedures for controlling hazardous energy during service and maintenance of machinery and equipment (29 CFR 1910.147).', 1,  1, 1, 9),
    (2,  N'PROG-OSHA-02',      N'Hazard Communication (HazCom)',
         N'Requires labeling, Safety Data Sheets, and employee training for hazardous chemicals in the workplace (29 CFR 1910.1200).', 2,  1, 1, 2),
    (3,  N'PROG-OSHA-03',      N'Powered Industrial Trucks (PIT)',
         N'Establishes training and operating requirements for forklifts and other powered industrial trucks (29 CFR 1910.178).', 3,  1, 1, 1),
    (4,  N'PROG-OSHA-04',      N'Permit-Required Confined Spaces',
         N'Establishes entry permit, atmospheric testing, and rescue requirements for permit-required confined spaces (29 CFR 1910.146).', 4,  1, 1, 8),
    (5,  N'PROG-OSHA-05',      N'Machine Guarding',
         N'Requires guarding of points of operation, rotating parts, and other machine hazards (29 CFR 1910.212).', 5,  1, 1, 1),
    -- EPA / EPA_RCRA / EPA_TSCA -> PrimaryFlag EnvironmentalPermits (literally that flag's own definition)
    (6,  N'PROG-EPA-01',       N'Spill Prevention, Control, and Countermeasure (SPCC)',
         N'Requires spill prevention planning and secondary containment for facilities storing oil above regulatory thresholds (40 CFR 112).', 6,  2, 2, 6),
    (7,  N'PROG-EPA-02',       N'Clean Air Act Operating Permit',
         N'Requires permitting and emissions control for stationary sources of air pollutants (40 CFR Part 70).', 7,  2, 2, 6),
    (8,  N'PROG-EPA-03',       N'Clean Water Act Discharge Permit (NPDES)',
         N'Requires permitting for point-source discharges of pollutants to waters of the United States (40 CFR 122).', 8,  2, 2, 6),
    (9,  N'PROG-EPA_RCRA-01',  N'RCRA Hazardous Waste Generator Status',
         N'Requires hazardous waste identification, manifesting, and generator-status compliance for facilities generating hazardous waste (40 CFR 262).', 9,  3, 2, 6),
    (10, N'PROG-EPA_TSCA-01',  N'TSCA Chemical Substance Reporting',
         N'Requires inventory reporting and use restrictions for chemical substances manufactured, processed, or imported (40 CFR Part 711).', 10, 4, 2, 6),
    -- PHMSA -> HazmatShipping
    (11, N'PROG-PHMSA-01',     N'Hazardous Materials Shipping Registration',
         N'Requires registration for offerors and carriers of hazardous materials in commerce (49 CFR 107 Subpart G).', 15, 5, 4, 4),
    (12, N'PROG-PHMSA-02',     N'Hazardous Materials Packaging',
         N'Establishes performance-tested packaging requirements for hazardous materials shipments (49 CFR Part 178).', 16, 5, 4, 4),
    (13, N'PROG-PHMSA-03',     N'Hazardous Materials Placarding',
         N'Requires placarding of transport vehicles carrying hazardous materials above threshold quantities (49 CFR Part 172 Subpart F).', 17, 5, 4, 4),
    -- NFPA -> Flammables; NFPA_Dust -> Dust
    (14, N'PROG-NFPA-01',      N'Flammable and Combustible Liquids Code',
         N'Establishes storage, handling, and dispensing requirements for flammable and combustible liquids (NFPA 30).', 11, 6, 3, 3),
    (15, N'PROG-NFPA-02',      N'Hot Work Permit Program',
         N'Requires a permit system and fire watch for welding, cutting, and other hot work operations (NFPA 51B).', 12, 6, 3, 3),
    (16, N'PROG-NFPA-03',      N'Fire Protection Systems',
         N'Establishes installation, inspection, and maintenance requirements for fire suppression and detection systems (NFPA 25 / NFPA 72).', 13, 6, 3, 3),
    (17, N'PROG-NFPA_Dust-01', N'Combustible Dust Hazard Analysis',
         N'Requires a documented dust hazard analysis and housekeeping/explosion-protection controls for processes generating combustible dust (NFPA 652).', 14, 7, 3, 10),
    -- FMCSA -> Fleet (category-folded into OSHA; own regulatory scope)
    (18, N'PROG-FMCSA-01',     N'Motor Carrier Operating Authority',
         N'Requires registration and operating authority for interstate motor carriers (49 CFR Part 365).', 18, 8, 1, 5),
    (19, N'PROG-FMCSA-02',     N'Driver Qualification Files',
         N'Requires maintenance of driver qualification files including medical certification and licensing (49 CFR Part 391).', 19, 8, 1, 5),
    (20, N'PROG-FMCSA-03',     N'Vehicle Inspection, Repair, and Maintenance',
         N'Requires systematic inspection, repair, and maintenance of commercial motor vehicles (49 CFR Part 396).', 20, 8, 1, 5),
    -- DHS_CFATS -> Chemicals
    (21, N'PROG-DHS_CFATS-01', N'Chemical Facility Anti-Terrorism Standards (CFATS) Site Security Plan',
         N'Requires a risk-based site security plan for facilities possessing chemicals of interest above screening threshold quantities (6 CFR Part 27).', 21, 9, 9, 2),
    -- USCG -> HazmatShipping
    (22, N'PROG-USCG-01',      N'Facility Security Plan (Waterfront)',
         N'Requires an approved facility security plan for waterfront facilities handling regulated cargo (33 CFR Part 105).', 22, 10, 8, 4),
    (23, N'PROG-USCG-02',      N'Bulk Liquid Transfer Operations',
         N'Establishes operational and equipment requirements for bulk transfer of liquid cargo between vessel and shore (33 CFR Part 154/156).', 23, 10, 8, 4),
    (24, N'PROG-USCG-03',      N'Maritime Security (MARSEC)',
         N'Requires compliance with maritime security levels and access-control measures at regulated facilities (33 CFR Part 101).', 24, 10, 8, 4),
    -- FDA -> NULL (quality/food-safety programs, no hazard-flag correlate)
    (25, N'PROG-FDA-01',       N'Current Good Manufacturing Practice (cGMP)',
         N'Establishes quality controls for the manufacture, processing, and packaging of regulated products (21 CFR Part 211 / Part 117).', 25, 11, 5, NULL),
    (26, N'PROG-FDA-02',       N'Sanitary Facility Operations',
         N'Requires sanitation controls for facilities, equipment, and personnel handling regulated products (21 CFR Part 117 Subpart B).', 26, 11, 5, NULL),
    (27, N'PROG-FDA-03',       N'Quality Systems Regulation',
         N'Establishes design control and quality management system requirements for regulated device/drug manufacturers (21 CFR Part 820).', 27, 11, 5, NULL),
    -- ANSI_ISO -> Machinery (category-folded into OSHA; own regulatory scope)
    (28, N'PROG-ANSI_ISO-01',  N'Consensus Standard Conformance',
         N'Requires conformance to applicable ANSI/ISO consensus standards incorporated by reference into OSHA regulations (e.g., ANSI B11 machine safety, ISO 45001 occupational health and safety management).', 28, 12, 1, 1),
    -- ATF / DoD -> ExplosionRisk (DoD category-folded into ATF; own regulatory scope)
    (29, N'PROG-ATF-01',       N'Federal Explosives License/Permit',
         N'Requires licensing for the manufacture, storage, and distribution of explosive materials (27 CFR Part 555).', 29, 13, 6, 7),
    (30, N'PROG-ATF-02',       N'Ammunition Manufacturing License',
         N'Requires licensing for the manufacture of ammunition and ammunition components (27 CFR Part 478).', 30, 13, 6, 7),
    (31, N'PROG-ATF-03',       N'Explosives Magazine Storage',
         N'Establishes construction, siting, and inventory requirements for explosives storage magazines (27 CFR Part 555 Subpart K).', 31, 13, 6, 7),
    (32, N'PROG-DoD-01',       N'System Safety Program (MIL-STD-882)',
         N'Requires a documented system safety program identifying and mitigating hazards across the system lifecycle (MIL-STD-882E).', 32, 14, 6, 7),
    (33, N'PROG-DoD-02',       N'Explosives Safety Siting',
         N'Requires quantity-distance siting analysis for facilities handling military energetics and munitions (DoD 6055.09-M).', 33, 14, 6, 7),
    -- MSHA -> Dust
    (34, N'PROG-MSHA-01',      N'Mine Safety Training',
         N'Requires new-miner and annual refresher safety training for mine employees (30 CFR Part 48).', 34, 15, 7, 10),
    (35, N'PROG-MSHA-02',      N'Mining Equipment Safety Standards',
         N'Establishes safety design and maintenance standards for mining equipment (30 CFR Part 56/57).', 35, 15, 7, 10),
    (36, N'PROG-MSHA-03',      N'Mine Ventilation Plan',
         N'Requires an approved ventilation plan to control airborne contaminants and methane in mine workings (30 CFR Part 75).', 36, 15, 7, 10)
) AS source ([ProgramId], [Code], [Name], [Description], [SourceRequirementId], [PrimaryRegulatorId], [RegulatorCategoryId], [PrimaryFlagId])
ON target.[ProgramId] = source.[ProgramId]
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([ProgramId], [Code], [Name], [Description], [SourceRequirementId], [PrimaryRegulatorId], [RegulatorCategoryId], [PrimaryFlagId])
    VALUES (source.[ProgramId], source.[Code], source.[Name], source.[Description], source.[SourceRequirementId], source.[PrimaryRegulatorId], source.[RegulatorCategoryId], source.[PrimaryFlagId])
WHEN MATCHED THEN
    UPDATE SET
        target.[Code]                 = source.[Code],
        target.[Name]                 = source.[Name],
        target.[Description]          = source.[Description],
        target.[SourceRequirementId]  = source.[SourceRequirementId],
        target.[PrimaryRegulatorId]   = source.[PrimaryRegulatorId],
        target.[RegulatorCategoryId]  = source.[RegulatorCategoryId],
        target.[PrimaryFlagId]        = source.[PrimaryFlagId];
GO

EXEC sys.sp_set_session_context @key = N'SafetyTaxonomySeedContext', @value = 0;
GO
