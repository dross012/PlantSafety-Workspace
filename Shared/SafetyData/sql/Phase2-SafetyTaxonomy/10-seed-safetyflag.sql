-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase2-SafetyTaxonomy
--  Layer:          Infrastructure - SQL Scripts
--  File:           10-seed-safetyflag.sql
--  Created:        2026-08-08
--  Author:         Platform Team
--  SchemaVersion:  2026.1
--  Description:    Seeds [SafetyTaxonomy].[SafetyFlag] with all 10 flag bits
--                  from SafetyFlag.cs, verbatim (declaration order, bit
--                  values, and XML-doc summaries copied as Description).
--
--  SESSION CONTEXT: sets SafetyTaxonomySeedContext=1 for the duration of
--  this script so [SafetyTaxonomy].[trg_SafetyFlag_BlockMutation]
--  (40-triggers-block-mutation.sql) allows these seed INSERTs through; see
--  that file's header for the full rationale. Reset to 0 at the end so no
--  later statement in the same session accidentally inherits seed
--  privileges.
--
--  Idempotent: MERGE keyed on SafetyFlagId. Safe to re-run.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

EXEC sys.sp_set_session_context @key = N'SafetyTaxonomySeedContext', @value = 1;
GO

MERGE [SafetyTaxonomy].[SafetyFlag] AS target
USING (VALUES
    (1,  N'Machinery',            1,   N'Industrial machinery hazards (rotating/moving equipment).'),
    (2,  N'Chemicals',            2,   N'Handling, storage, or processing of hazardous chemicals.'),
    (3,  N'Flammables',           4,   N'Presence of flammable materials or atmospheres.'),
    (4,  N'HazmatShipping',       8,   N'Shipment of hazardous materials subject to hazmat transport rules.'),
    (5,  N'Fleet',                16,  N'Operation of a vehicle fleet.'),
    (6,  N'EnvironmentalPermits', 32,  N'Subject to environmental permitting requirements.'),
    (7,  N'ExplosionRisk',        64,  N'Elevated risk of explosion.'),
    (8,  N'ConfinedSpaces',       128, N'Presence of confined spaces requiring entry controls.'),
    (9,  N'LOTO',                 256, N'Lockout/Tagout energy-control requirements apply.'),
    (10, N'Dust',                 512, N'Combustible or respirable dust hazards.')
) AS source ([SafetyFlagId], [FlagName], [BitValue], [Description])
ON target.[SafetyFlagId] = source.[SafetyFlagId]
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([SafetyFlagId], [FlagName], [BitValue], [Description])
    VALUES (source.[SafetyFlagId], source.[FlagName], source.[BitValue], source.[Description])
WHEN MATCHED THEN
    UPDATE SET
        target.[FlagName]    = source.[FlagName],
        target.[BitValue]    = source.[BitValue],
        target.[Description] = source.[Description];
GO

EXEC sys.sp_set_session_context @key = N'SafetyTaxonomySeedContext', @value = 0;
GO
