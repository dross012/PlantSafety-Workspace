-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.LotoSystems
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_LotoSystems_GetAll.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted LotoSystems rows, backing
--                  ILotoSystemsRepository.ListAsync (the Domain specification predicate carries
--                  no server-side-translatable filter, so it is applied in-memory in the
--                  Infrastructure repository against this full result set).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [MachineSafety].[usp_LotoSystems_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [LotoSystemsId], [TenantId], [SiteId], [Status], [SystemTitle], [EquipmentTag], [EnergySourceTypes], [IsolationPointDescription], [IsolationPointCount], [VerifiedByUserId], [VerifiedZeroEnergyAtUtc], [AuditedAtUtc], [ComplianceScore], [CreatedAtUtc], [CreatedBy]
    FROM [MachineSafety].[LotoSystems]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO
