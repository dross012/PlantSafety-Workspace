-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase2-SafetyTaxonomy
--  Layer:          Infrastructure - SQL Scripts
--  File:           15-seed-industryregulator.sql
--  Created:        2026-08-08
--  Author:         Platform Team
--  SchemaVersion:  2026.1
--  Description:    Seeds [SafetyTaxonomy].[IndustryRegulator] with every
--                  (industry, regulator) pair from IndustryRegistry.All's
--                  Regulators arrays, verbatim, in source order
--                  (OrdinalPosition). RegulatorId values below follow
--                  12-seed-regulator.sql's mapping: 1=OSHA, 2=EPA,
--                  3=EPA_RCRA, 4=EPA_TSCA, 5=PHMSA, 6=NFPA, 7=NFPA_Dust,
--                  8=FMCSA, 9=DHS_CFATS, 10=USCG, 11=FDA, 12=ANSI_ISO,
--                  13=ATF, 14=DoD, 15=MSHA.
--
--  SESSION CONTEXT: see 10-seed-safetyflag.sql's header -- same pattern.
--  Idempotent: MERGE keyed on (IndustryId, RegulatorId).
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

EXEC sys.sp_set_session_context @key = N'SafetyTaxonomySeedContext', @value = 1;
GO

MERGE [SafetyTaxonomy].[IndustryRegulator] AS target
USING (VALUES
    -- 1 Chemical Manufacturing: OSHA, EPA_RCRA, EPA_TSCA, PHMSA, NFPA, FMCSA, DHS_CFATS
    (1, 1, 1), (1, 3, 2), (1, 4, 3), (1, 5, 4), (1, 6, 5), (1, 8, 6), (1, 9, 7),
    -- 2 Petrochemical & Refining: OSHA, EPA, PHMSA, NFPA, FMCSA, USCG
    (2, 1, 1), (2, 2, 2), (2, 5, 3), (2, 6, 4), (2, 8, 5), (2, 10, 6),
    -- 3 Pharmaceuticals & Biotech: OSHA, FDA, EPA, NFPA
    (3, 1, 1), (3, 11, 2), (3, 2, 3), (3, 6, 4),
    -- 4 Food & Beverage: OSHA, FDA, EPA, NFPA
    (4, 1, 1), (4, 11, 2), (4, 2, 3), (4, 6, 4),
    -- 5 Automotive & Heavy Equipment: OSHA, NFPA, EPA, FMCSA, ANSI_ISO
    (5, 1, 1), (5, 6, 2), (5, 2, 3), (5, 8, 4), (5, 12, 5),
    -- 6 Aerospace & Defense: OSHA, EPA, NFPA, ATF, DoD
    (6, 1, 1), (6, 2, 2), (6, 6, 3), (6, 13, 4), (6, 14, 5),
    -- 7 Electronics & Semiconductor: OSHA, EPA, NFPA, PHMSA
    (7, 1, 1), (7, 2, 2), (7, 6, 3), (7, 5, 4),
    -- 8 Plastics & Rubber: OSHA, EPA, NFPA
    (8, 1, 1), (8, 2, 2), (8, 6, 3),
    -- 9 Metal Fabrication & Foundries: OSHA, NFPA, EPA
    (9, 1, 1), (9, 6, 2), (9, 2, 3),
    -- 10 Wood Products & Furniture: OSHA, NFPA_Dust, EPA
    (10, 1, 1), (10, 7, 2), (10, 2, 3),
    -- 11 Paper & Pulp: OSHA, EPA, NFPA
    (11, 1, 1), (11, 2, 2), (11, 6, 3),
    -- 12 Textiles & Apparel: OSHA, EPA, NFPA
    (12, 1, 1), (12, 2, 2), (12, 6, 3),
    -- 13 Warehousing & Distribution: FMCSA, PHMSA, OSHA, EPA
    (13, 8, 1), (13, 5, 2), (13, 1, 3), (13, 2, 4),
    -- 14 Cold Storage: FMCSA, PHMSA, OSHA, EPA
    (14, 8, 1), (14, 5, 2), (14, 1, 3), (14, 2, 4),
    -- 15 E‑commerce Fulfillment: FMCSA, PHMSA, OSHA, EPA
    (15, 8, 1), (15, 5, 2), (15, 1, 3), (15, 2, 4),
    -- 16 Third‑Party Logistics: FMCSA, PHMSA, OSHA, EPA
    (16, 8, 1), (16, 5, 2), (16, 1, 3), (16, 2, 4),
    -- 17 Bulk Material Handling: FMCSA, PHMSA, OSHA, EPA
    (17, 8, 1), (17, 5, 2), (17, 1, 3), (17, 2, 4),
    -- 18 Explosives & Pyrotechnics: NFPA, ATF, OSHA, EPA
    (18, 6, 1), (18, 13, 2), (18, 1, 3), (18, 2, 4),
    -- 19 Ammunition & Firearms: NFPA, ATF, OSHA, EPA
    (19, 6, 1), (19, 13, 2), (19, 1, 3), (19, 2, 4),
    -- 20 Paints, Coatings, Adhesives: NFPA, OSHA, EPA
    (20, 6, 1), (20, 1, 2), (20, 2, 3),
    -- 21 Printing & Ink: NFPA, OSHA, EPA
    (21, 6, 1), (21, 1, 2), (21, 2, 3),
    -- 22 Hazardous Waste Treatment: EPA_RCRA, PHMSA, OSHA, NFPA
    (22, 3, 1), (22, 5, 2), (22, 1, 3), (22, 6, 4),
    -- 23 Chemical Blending / Tolling: EPA_RCRA, PHMSA, OSHA, NFPA
    (23, 3, 1), (23, 5, 2), (23, 1, 3), (23, 6, 4),
    -- 24 Industrial Cleaning Chemical Production: EPA_RCRA, PHMSA, OSHA, NFPA
    (24, 3, 1), (24, 5, 2), (24, 1, 3), (24, 6, 4),
    -- 25 Mining & Mineral Processing: OSHA, NFPA, MSHA, EPA
    (25, 1, 1), (25, 6, 2), (25, 15, 3), (25, 2, 4),
    -- 26 Cement & Concrete: OSHA, NFPA, EPA
    (26, 1, 1), (26, 6, 2), (26, 2, 3),
    -- 27 Steel Mills & Rolling: OSHA, NFPA, EPA
    (27, 1, 1), (27, 6, 2), (27, 2, 3),
    -- 28 Heavy Industrial Fabrication: OSHA, NFPA, EPA
    (28, 1, 1), (28, 6, 2), (28, 2, 3),
    -- 29 Marine Terminals: USCG, EPA, OSHA, PHMSA
    (29, 10, 1), (29, 2, 2), (29, 1, 3), (29, 5, 4),
    -- 30 Bulk Liquid Transfer: USCG, EPA, OSHA, PHMSA
    (30, 10, 1), (30, 2, 2), (30, 1, 3), (30, 5, 4),
    -- 31 Waterfront Manufacturing: USCG, EPA, OSHA, PHMSA
    (31, 10, 1), (31, 2, 2), (31, 1, 3), (31, 5, 4),
    -- 32 Power Generation: EPA, OSHA, NFPA
    (32, 2, 1), (32, 1, 2), (32, 6, 3),
    -- 33 Renewable Energy Manufacturing: EPA, OSHA, NFPA
    (33, 2, 1), (33, 1, 2), (33, 6, 3),
    -- 34 Wastewater Treatment: EPA, OSHA, NFPA
    (34, 2, 1), (34, 1, 2), (34, 6, 3),
    -- 35 Industrial Laundries: EPA, OSHA, NFPA
    (35, 2, 1), (35, 1, 2), (35, 6, 3),
    -- 36 Medical Devices: FDA, OSHA
    (36, 11, 1), (36, 1, 2),
    -- 37 Cosmetics: FDA, OSHA
    (37, 11, 1), (37, 1, 2),
    -- 38 Agricultural Processing: OSHA, EPA
    (38, 1, 1), (38, 2, 2),
    -- 39 Tobacco: OSHA, FDA
    (39, 1, 1), (39, 11, 2),
    -- 40 Cannabis: OSHA, EPA, NFPA
    (40, 1, 1), (40, 2, 2), (40, 6, 3)
) AS source ([IndustryId], [RegulatorId], [OrdinalPosition])
ON target.[IndustryId] = source.[IndustryId] AND target.[RegulatorId] = source.[RegulatorId]
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([IndustryId], [RegulatorId], [OrdinalPosition])
    VALUES (source.[IndustryId], source.[RegulatorId], source.[OrdinalPosition])
WHEN MATCHED THEN
    UPDATE SET target.[OrdinalPosition] = source.[OrdinalPosition];
GO

EXEC sys.sp_set_session_context @key = N'SafetyTaxonomySeedContext', @value = 0;
GO
