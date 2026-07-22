-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.MaterialHandlingStorage
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_MaterialHandlingStorage_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Full-row update procedure for [SafetyProcedures].[MaterialHandlingStorage], backing
--                  IMaterialHandlingStorageRepository.UpdateAsync (all lifecycle transitions persist the
--                  aggregate's full current state). Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyProcedures].[usp_MaterialHandlingStorage_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @MaterialHandlingStorageId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @ProcedureTitle NVARCHAR(400),
    @HandlingCategory NVARCHAR(100),
    @StorageLocation NVARCHAR(400),
    @ApprovedByUserId UNIQUEIDENTIFIER,
    @PublishedAtUtc DATETIME2(3) = NULL,
    @NextReviewDueAtUtc DATETIME2(3) = NULL,
    @ObstructionCount INT,
    @ComplianceScore DECIMAL(9,2) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [SafetyProcedures].[MaterialHandlingStorage]
        SET
        [SiteId] = @SiteId,
        [Status] = @Status,
        [ProcedureTitle] = @ProcedureTitle,
        [HandlingCategory] = @HandlingCategory,
        [StorageLocation] = @StorageLocation,
        [ApprovedByUserId] = @ApprovedByUserId,
        [PublishedAtUtc] = @PublishedAtUtc,
        [NextReviewDueAtUtc] = @NextReviewDueAtUtc,
        [ObstructionCount] = @ObstructionCount,
        [ComplianceScore] = @ComplianceScore,
        [ModifiedAtUtc] = @NowUtc,
        [ModifiedBy] = @ActorId,
        [CorrelationId] = @CorrelationId
        WHERE [MaterialHandlingStorageId] = @MaterialHandlingStorageId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
