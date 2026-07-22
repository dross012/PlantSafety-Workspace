-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.MaterialHandlingStorage
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_MaterialHandlingStorage_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single MaterialHandlingStorage row by identifier, backing
--                  IMaterialHandlingStorageRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyProcedures].[usp_MaterialHandlingStorage_GetById]
(
    @MaterialHandlingStorageId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [MaterialHandlingStorageId], [TenantId], [SiteId], [Status], [ProcedureTitle], [HandlingCategory], [StorageLocation], [ApprovedByUserId], [PublishedAtUtc], [NextReviewDueAtUtc], [ObstructionCount], [ComplianceScore], [CreatedAtUtc], [CreatedBy]
    FROM [SafetyProcedures].[MaterialHandlingStorage]
    WHERE [MaterialHandlingStorageId] = @MaterialHandlingStorageId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO
