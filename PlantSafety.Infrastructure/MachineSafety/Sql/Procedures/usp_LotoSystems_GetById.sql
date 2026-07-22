-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.LotoSystems
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_LotoSystems_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single LotoSystems row by identifier, backing
--                  ILotoSystemsRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [MachineSafety].[usp_LotoSystems_GetById]
(
    @LotoSystemsId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [LotoSystemsId], [TenantId], [SiteId], [Status], [SystemTitle], [EquipmentTag], [EnergySourceTypes], [IsolationPointDescription], [IsolationPointCount], [VerifiedByUserId], [VerifiedZeroEnergyAtUtc], [AuditedAtUtc], [ComplianceScore], [CreatedAtUtc], [CreatedBy]
    FROM [MachineSafety].[LotoSystems]
    WHERE [LotoSystemsId] = @LotoSystemsId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO
