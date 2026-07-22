-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.MaterialHandlingStorage
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_MaterialHandlingStorage_GetAll.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted MaterialHandlingStorage rows, backing
--                  IMaterialHandlingStorageRepository.ListAsync (the Domain specification predicate carries
--                  no server-side-translatable filter, so it is applied in-memory in the
--                  Infrastructure repository against this full result set).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyProcedures].[usp_MaterialHandlingStorage_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [MaterialHandlingStorageId], [TenantId], [SiteId], [Status], [ProcedureTitle], [HandlingCategory], [StorageLocation], [ApprovedByUserId], [PublishedAtUtc], [NextReviewDueAtUtc], [ObstructionCount], [ComplianceScore], [CreatedAtUtc], [CreatedBy]
    FROM [SafetyProcedures].[MaterialHandlingStorage]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO
