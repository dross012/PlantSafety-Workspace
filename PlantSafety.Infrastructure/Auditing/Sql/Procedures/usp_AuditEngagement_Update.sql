-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Auditing.AuditEngagement
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_AuditEngagement_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Full-row update procedure for [Auditing].[AuditEngagement], backing
--                  IAuditEngagementRepository.UpdateAsync (Schedule/Start/RecordFinding/Close
--                  lifecycle transitions). Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Auditing].[usp_AuditEngagement_Update]
    @TenantId          UNIQUEIDENTIFIER,
    @CorrelationId     UNIQUEIDENTIFIER,
    @ActorId           UNIQUEIDENTIFIER,
    @NowUtc            DATETIME2(3),
    @AuditEngagementId UNIQUEIDENTIFIER,
    @Status            NVARCHAR(20),
    @LeadAuditorId     UNIQUEIDENTIFIER = NULL,
    @ScheduledAtUtc    DATETIME2(3) = NULL,
    @CompletedAtUtc    DATETIME2(3) = NULL,
    @FindingsCount     INT,
    @ComplianceScore   DECIMAL(5,2) = NULL,
    @ComplianceRating  NVARCHAR(30)
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [Auditing].[AuditEngagement]
        SET [Status]            = @Status,
            [LeadAuditorId]     = @LeadAuditorId,
            [ScheduledAtUtc]    = @ScheduledAtUtc,
            [CompletedAtUtc]    = @CompletedAtUtc,
            [FindingsCount]     = @FindingsCount,
            [ComplianceScore]   = @ComplianceScore,
            [ComplianceRating]  = @ComplianceRating,
            [LastModifiedAtUtc] = @NowUtc
        WHERE [AuditEngagementId] = @AuditEngagementId
          AND [TenantId]          = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
