-- ============================================================================
--  Project:        Shared.FacilityDomain -- Phase1-Lookup
--  File:           02-seed-lookup.sql
--  Created:        2026-08-09
--  Description:    Seeds [Lookup].[SeverityLevel]/[HazardType]/
--                  [RegulatoryJurisdiction] from the real C# enum members
--                  (SeverityLevel.cs, HazardType.cs, RegulatoryJurisdiction.cs)
--                  in their declared ordinal order. Idempotent MERGE --
--                  additive-only, safe to re-run; never removes a published
--                  code.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

MERGE [Lookup].[SeverityLevel] AS target
USING (VALUES
    (N'Minor',        0, N'No significant injury, damage, or regulatory exposure.'),
    (N'Moderate',     1, N'Limited injury/damage; recordable but contained.'),
    (N'Severe',       2, N'Significant injury/damage; likely reportable to a regulator.'),
    (N'Catastrophic', 3, N'Life-threatening, fatal, or facility-wide consequence.')
) AS source ([Code], [OrdinalValue], [Description])
    ON target.[Code] = source.[Code]
WHEN MATCHED THEN
    UPDATE SET [OrdinalValue] = source.[OrdinalValue], [Description] = source.[Description], [UpdatedUtc] = SYSUTCDATETIME(), [UpdatedBy] = SYSTEM_USER
WHEN NOT MATCHED THEN
    INSERT ([Code], [OrdinalValue], [Description]) VALUES (source.[Code], source.[OrdinalValue], source.[Description]);
GO

MERGE [Lookup].[HazardType] AS target
USING (VALUES
    (N'Fire',                 0, N'An uncontrolled fire.'),
    (N'ChemicalSpill',        1, N'An uncontrolled release of a hazardous chemical.'),
    (N'Injury',                2, N'A physical injury to a person.'),
    (N'EnvironmentalRelease', 3, N'A release into the surrounding environment (air, water, soil).'),
    (N'ConfinedSpace',        4, N'A hazard arising from entry into a confined space.'),
    (N'LOTOFailure',          5, N'A failure of lockout/tagout energy-isolation procedures.'),
    (N'HotWork',              6, N'A hazard arising from hot-work activity (welding, cutting, grinding).'),
    (N'FleetIncident',        7, N'An incident involving a fleet vehicle.'),
    (N'Other',                8, N'A hazard that does not match any of the above -- classification default.')
) AS source ([Code], [OrdinalValue], [Description])
    ON target.[Code] = source.[Code]
WHEN MATCHED THEN
    UPDATE SET [OrdinalValue] = source.[OrdinalValue], [Description] = source.[Description], [UpdatedUtc] = SYSUTCDATETIME(), [UpdatedBy] = SYSTEM_USER
WHEN NOT MATCHED THEN
    INSERT ([Code], [OrdinalValue], [Description]) VALUES (source.[Code], source.[OrdinalValue], source.[Description]);
GO

MERGE [Lookup].[RegulatoryJurisdiction] AS target
USING (VALUES
    (N'OSHA',  0, N'U.S. Occupational Safety and Health Administration.'),
    (N'EPA',   1, N'U.S. Environmental Protection Agency.'),
    (N'DOT',   2, N'U.S. Department of Transportation.'),
    (N'NFPA',  3, N'National Fire Protection Association.'),
    (N'State', 4, N'A state-level regulatory body.'),
    (N'Local', 5, N'A local/municipal regulatory body.')
) AS source ([Code], [OrdinalValue], [Description])
    ON target.[Code] = source.[Code]
WHEN MATCHED THEN
    UPDATE SET [OrdinalValue] = source.[OrdinalValue], [Description] = source.[Description], [UpdatedUtc] = SYSUTCDATETIME(), [UpdatedBy] = SYSTEM_USER
WHEN NOT MATCHED THEN
    INSERT ([Code], [OrdinalValue], [Description]) VALUES (source.[Code], source.[OrdinalValue], source.[Description]);
GO
