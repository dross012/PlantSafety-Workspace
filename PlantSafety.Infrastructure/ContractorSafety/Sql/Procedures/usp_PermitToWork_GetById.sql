-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContractorSafety.PermitToWork
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_PermitToWork_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single PermitToWork row by identifier, backing
--                  IPermitToWorkRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [ContractorSafety].[usp_PermitToWork_GetById]
(
    @PermitToWorkId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [PermitToWorkId], [TenantId], [SiteId], [Status], [PermitTitle], [PermitType], [WorkLocation], [HazardCount], [RequiresAtmosphericTesting], [AtmosphericTestScore], [AuthorizedByUserId], [IssuedAtUtc], [ExpiresAtUtc], [SuspensionReason], [CreatedAtUtc], [CreatedBy]
    FROM [ContractorSafety].[PermitToWork]
    WHERE [PermitToWorkId] = @PermitToWorkId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO
