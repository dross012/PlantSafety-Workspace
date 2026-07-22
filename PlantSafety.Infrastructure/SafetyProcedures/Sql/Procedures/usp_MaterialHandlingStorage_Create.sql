-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.MaterialHandlingStorage
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_MaterialHandlingStorage_Create.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [SafetyProcedures].[MaterialHandlingStorage], backing
--                  IMaterialHandlingStorageRepository.AddAsync. Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyProcedures].[usp_MaterialHandlingStorage_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
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
        INSERT INTO [SafetyProcedures].[MaterialHandlingStorage]
            ([MaterialHandlingStorageId], [TenantId], [SiteId], [Status], [ProcedureTitle], [HandlingCategory], [StorageLocation], [ApprovedByUserId], [PublishedAtUtc], [NextReviewDueAtUtc], [ObstructionCount], [ComplianceScore], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@MaterialHandlingStorageId, @TenantId, @SiteId, @Status, @ProcedureTitle, @HandlingCategory, @StorageLocation, @ApprovedByUserId, @PublishedAtUtc, @NextReviewDueAtUtc, @ObstructionCount, @ComplianceScore, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
