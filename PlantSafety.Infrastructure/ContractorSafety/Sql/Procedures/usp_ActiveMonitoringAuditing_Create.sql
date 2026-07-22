-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContractorSafety.ActiveMonitoringAuditing
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_ActiveMonitoringAuditing_Create.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [ContractorSafety].[ActiveMonitoringAuditing], backing
--                  IActiveMonitoringAuditingRepository.AddAsync. Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [ContractorSafety].[usp_ActiveMonitoringAuditing_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
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
        INSERT INTO [ContractorSafety].[ActiveMonitoringAuditing]
            ([ActiveMonitoringAuditingId], [TenantId], [SiteId], [Status], [MonitoringTitle], [Category], [WorkLocation], [MonitoredByUserId], [ObservedAtUtc], [UnsafeConditionCount], [ComplianceScore], [FindingNotes], [FollowUpAtUtc], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@ActiveMonitoringAuditingId, @TenantId, @SiteId, @Status, @MonitoringTitle, @Category, @WorkLocation, @MonitoredByUserId, @ObservedAtUtc, @UnsafeConditionCount, @ComplianceScore, @FindingNotes, @FollowUpAtUtc, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
