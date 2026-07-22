-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.PpePolicies
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_PpePolicies_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Full-row update procedure for [SafetyProcedures].[PpePolicies], backing
--                  IPpePoliciesRepository.UpdateAsync (all lifecycle transitions persist the
--                  aggregate's full current state). Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyProcedures].[usp_PpePolicies_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @PpePoliciesId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @PolicyTitle NVARCHAR(400),
    @PpeCategory NVARCHAR(100),
    @AssignedZone NVARCHAR(400),
    @ApprovedByUserId UNIQUEIDENTIFIER,
    @PublishedAtUtc DATETIME2(3) = NULL,
    @NextReviewDueAtUtc DATETIME2(3) = NULL,
    @ReplacementScheduleCount INT,
    @ComplianceScore DECIMAL(9,2) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [SafetyProcedures].[PpePolicies]
        SET
        [SiteId] = @SiteId,
        [Status] = @Status,
        [PolicyTitle] = @PolicyTitle,
        [PpeCategory] = @PpeCategory,
        [AssignedZone] = @AssignedZone,
        [ApprovedByUserId] = @ApprovedByUserId,
        [PublishedAtUtc] = @PublishedAtUtc,
        [NextReviewDueAtUtc] = @NextReviewDueAtUtc,
        [ReplacementScheduleCount] = @ReplacementScheduleCount,
        [ComplianceScore] = @ComplianceScore,
        [ModifiedAtUtc] = @NowUtc,
        [ModifiedBy] = @ActorId,
        [CorrelationId] = @CorrelationId
        WHERE [PpePoliciesId] = @PpePoliciesId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
