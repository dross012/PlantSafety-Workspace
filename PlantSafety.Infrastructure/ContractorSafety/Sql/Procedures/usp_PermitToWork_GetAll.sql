-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContractorSafety.PermitToWork
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_PermitToWork_GetAll.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted PermitToWork rows, backing
--                  IPermitToWorkRepository.ListAsync (the Domain specification predicate carries
--                  no server-side-translatable filter, so it is applied in-memory in the
--                  Infrastructure repository against this full result set).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [ContractorSafety].[usp_PermitToWork_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [PermitToWorkId], [TenantId], [SiteId], [Status], [PermitTitle], [PermitType], [WorkLocation], [HazardCount], [RequiresAtmosphericTesting], [AtmosphericTestScore], [AuthorizedByUserId], [IssuedAtUtc], [ExpiresAtUtc], [SuspensionReason], [CreatedAtUtc], [CreatedBy]
    FROM [ContractorSafety].[PermitToWork]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO
