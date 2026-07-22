-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContractorSafety.ActiveMonitoringAuditing
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_ActiveMonitoringAuditing_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Full-row update procedure for [ContractorSafety].[ActiveMonitoringAuditing], backing
--                  IActiveMonitoringAuditingRepository.UpdateAsync (all lifecycle transitions persist the
--                  aggregate's full current state). Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [ContractorSafety].[usp_ActiveMonitoringAuditing_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @ActiveMonitoringAuditingId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @MonitoringTitle NVARCHAR(400),
    @Category NVARCHAR(100),
    @WorkLocation NVARCHAR(400),
    @MonitoredByUserId UNIQUEIDENTIFIER,
    @ObservedAtUtc DATETIME2(3) = NULL,
    @UnsafeConditionCount INT,
    @ComplianceScore DECIMAL(9,2) = NULL,
    @FindingNotes NVARCHAR(2000) = NULL,
    @FollowUpAtUtc DATETIME2(3) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [ContractorSafety].[ActiveMonitoringAuditing]
        SET
        [SiteId] = @SiteId,
        [Status] = @Status,
        [MonitoringTitle] = @MonitoringTitle,
        [Category] = @Category,
        [WorkLocation] = @WorkLocation,
        [MonitoredByUserId] = @MonitoredByUserId,
        [ObservedAtUtc] = @ObservedAtUtc,
        [UnsafeConditionCount] = @UnsafeConditionCount,
        [ComplianceScore] = @ComplianceScore,
        [FindingNotes] = @FindingNotes,
        [FollowUpAtUtc] = @FollowUpAtUtc,
        [ModifiedAtUtc] = @NowUtc,
        [ModifiedBy] = @ActorId,
        [CorrelationId] = @CorrelationId
        WHERE [ActiveMonitoringAuditingId] = @ActiveMonitoringAuditingId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
