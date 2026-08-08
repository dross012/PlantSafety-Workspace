-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase2-SafetyTaxonomy
--  Layer:          Infrastructure - SQL Scripts
--  File:           16-seed-industryflag.sql
--  Created:        2026-08-08
--  Author:         Platform Team
--  SchemaVersion:  2026.1
--  Description:    Seeds [SafetyTaxonomy].[IndustryFlag] with every
--                  (industry, flag) pair implied by IndustryRegistry.All's
--                  Flags bitmasks -- equivalent to
--                  SafetyFlagCatalog.Decompose(industry.Flags) for each
--                  industry. SafetyFlagId values below follow
--                  10-seed-safetyflag.sql's mapping: 1=Machinery,
--                  2=Chemicals, 3=Flammables, 4=HazmatShipping, 5=Fleet,
--                  6=EnvironmentalPermits, 7=ExplosionRisk, 8=ConfinedSpaces,
--                  9=LOTO, 10=Dust. Rows are listed in ascending SafetyFlagId
--                  order per industry for readability; IndustryFlag carries
--                  no OrdinalPosition column (unlike IndustrySubIndustry/
--                  IndustryRegulator) because SafetyFlag is a bitmask in the
--                  C# source, not an ordered list -- membership is the only
--                  thing that matters, decomposition order is an
--                  implementation detail of SafetyFlagCatalog.AllFlags, not
--                  part of the source-of-truth data itself.
--
--  SESSION CONTEXT: see 10-seed-safetyflag.sql's header -- same pattern.
--  Idempotent: MERGE keyed on (IndustryId, SafetyFlagId).
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

EXEC sys.sp_set_session_context @key = N'SafetyTaxonomySeedContext', @value = 1;
GO

MERGE [SafetyTaxonomy].[IndustryFlag] AS target
USING (VALUES
    -- 1 Chemical Manufacturing
    (1,1),(1,2),(1,3),(1,4),(1,5),(1,6),(1,7),(1,8),(1,9),
    -- 2 Petrochemical & Refining
    (2,1),(2,2),(2,3),(2,4),(2,5),(2,6),(2,7),(2,8),(2,9),
    -- 3 Pharmaceuticals & Biotech
    (3,1),(3,2),(3,6),(3,8),(3,9),
    -- 4 Food & Beverage
    (4,1),(4,2),(4,3),(4,6),(4,8),(4,9),
    -- 5 Automotive & Heavy Equipment
    (5,1),(5,3),(5,5),(5,6),(5,8),(5,9),
    -- 6 Aerospace & Defense
    (6,1),(6,2),(6,3),(6,6),(6,7),(6,8),(6,9),
    -- 7 Electronics & Semiconductor
    (7,1),(7,2),(7,3),(7,4),(7,6),(7,8),(7,9),
    -- 8 Plastics & Rubber
    (8,1),(8,2),(8,3),(8,6),(8,8),(8,9),
    -- 9 Metal Fabrication & Foundries
    (9,1),(9,3),(9,6),(9,8),(9,9),(9,10),
    -- 10 Wood Products & Furniture
    (10,1),(10,3),(10,6),(10,8),(10,9),(10,10),
    -- 11 Paper & Pulp
    (11,1),(11,2),(11,3),(11,6),(11,8),(11,9),
    -- 12 Textiles & Apparel
    (12,1),(12,2),(12,3),(12,6),(12,8),(12,9),
    -- 13 Warehousing & Distribution
    (13,1),(13,4),(13,5),(13,8),(13,9),
    -- 14 Cold Storage
    (14,1),(14,4),(14,5),(14,8),(14,9),
    -- 15 E‑commerce Fulfillment
    (15,1),(15,4),(15,5),(15,8),(15,9),
    -- 16 Third‑Party Logistics
    (16,1),(16,4),(16,5),(16,8),(16,9),
    -- 17 Bulk Material Handling
    (17,1),(17,4),(17,5),(17,8),(17,9),(17,10),
    -- 18 Explosives & Pyrotechnics
    (18,1),(18,2),(18,3),(18,6),(18,7),(18,8),(18,9),
    -- 19 Ammunition & Firearms
    (19,1),(19,2),(19,3),(19,6),(19,7),(19,8),(19,9),
    -- 20 Paints, Coatings, Adhesives
    (20,1),(20,2),(20,3),(20,6),(20,7),(20,8),(20,9),
    -- 21 Printing & Ink
    (21,1),(21,2),(21,3),(21,6),(21,8),(21,9),
    -- 22 Hazardous Waste Treatment
    (22,1),(22,2),(22,4),(22,6),(22,8),(22,9),
    -- 23 Chemical Blending / Tolling
    (23,1),(23,2),(23,4),(23,6),(23,8),(23,9),
    -- 24 Industrial Cleaning Chemical Production
    (24,1),(24,2),(24,4),(24,6),(24,8),(24,9),
    -- 25 Mining & Mineral Processing
    (25,1),(25,3),(25,6),(25,8),(25,9),(25,10),
    -- 26 Cement & Concrete
    (26,1),(26,6),(26,8),(26,9),(26,10),
    -- 27 Steel Mills & Rolling
    (27,1),(27,3),(27,6),(27,8),(27,9),
    -- 28 Heavy Industrial Fabrication
    (28,1),(28,3),(28,6),(28,8),(28,9),
    -- 29 Marine Terminals
    (29,1),(29,4),(29,6),(29,8),(29,9),
    -- 30 Bulk Liquid Transfer
    (30,1),(30,2),(30,4),(30,6),(30,8),(30,9),
    -- 31 Waterfront Manufacturing
    (31,1),(31,2),(31,4),(31,6),(31,8),(31,9),
    -- 32 Power Generation
    (32,1),(32,2),(32,3),(32,6),(32,8),(32,9),
    -- 33 Renewable Energy Manufacturing
    (33,1),(33,2),(33,3),(33,6),(33,8),(33,9),
    -- 34 Wastewater Treatment
    (34,1),(34,2),(34,6),(34,8),(34,9),
    -- 35 Industrial Laundries
    (35,1),(35,2),(35,6),(35,8),(35,9),
    -- 36 Medical Devices
    (36,1),(36,6),(36,8),(36,9),
    -- 37 Cosmetics
    (37,1),(37,2),(37,6),(37,8),(37,9),
    -- 38 Agricultural Processing
    (38,1),(38,2),(38,6),(38,8),(38,9),(38,10),
    -- 39 Tobacco
    (39,1),(39,2),(39,6),(39,8),(39,9),
    -- 40 Cannabis
    (40,1),(40,2),(40,3),(40,6),(40,8),(40,9)
) AS source ([IndustryId], [SafetyFlagId])
ON target.[IndustryId] = source.[IndustryId] AND target.[SafetyFlagId] = source.[SafetyFlagId]
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([IndustryId], [SafetyFlagId])
    VALUES (source.[IndustryId], source.[SafetyFlagId]);
GO

EXEC sys.sp_set_session_context @key = N'SafetyTaxonomySeedContext', @value = 0;
GO
