-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.HazardCommunicationRightToKnow
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_HazardCommunicationRightToKnow_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Full-row update procedure for [SafetyProcedures].[HazardCommunicationRightToKnow], backing
--                  IHazardCommunicationRightToKnowRepository.UpdateAsync (all lifecycle transitions persist the
--                  aggregate's full current state). Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyProcedures].[usp_HazardCommunicationRightToKnow_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @HazardCommunicationRightToKnowId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @ProcedureTitle NVARCHAR(400),
    @HazardClass NVARCHAR(100),
    @SdsReference NVARCHAR(400),
    @ApprovedByUserId UNIQUEIDENTIFIER,
    @PublishedAtUtc DATETIME2(3) = NULL,
    @NextReviewDueAtUtc DATETIME2(3) = NULL,
    @LabeledItemCount INT,
    @ComplianceScore DECIMAL(9,2) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [SafetyProcedures].[HazardCommunicationRightToKnow]
        SET
        [SiteId] = @SiteId,
        [Status] = @Status,
        [ProcedureTitle] = @ProcedureTitle,
        [HazardClass] = @HazardClass,
        [SdsReference] = @SdsReference,
        [ApprovedByUserId] = @ApprovedByUserId,
        [PublishedAtUtc] = @PublishedAtUtc,
        [NextReviewDueAtUtc] = @NextReviewDueAtUtc,
        [LabeledItemCount] = @LabeledItemCount,
        [ComplianceScore] = @ComplianceScore,
        [ModifiedAtUtc] = @NowUtc,
        [ModifiedBy] = @ActorId,
        [CorrelationId] = @CorrelationId
        WHERE [HazardCommunicationRightToKnowId] = @HazardCommunicationRightToKnowId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
