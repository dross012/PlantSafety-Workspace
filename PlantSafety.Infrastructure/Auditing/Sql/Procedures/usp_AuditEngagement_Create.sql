-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Auditing.AuditEngagement
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_AuditEngagement_Create.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [Auditing].[AuditEngagement], backing
--                  IAuditEngagementRepository.AddAsync. Standard error envelope:
--                  TRY/CATCH with transaction rollback and THROW.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Auditing].[usp_AuditEngagement_Create]
    @TenantId          UNIQUEIDENTIFIER,
    @CorrelationId     UNIQUEIDENTIFIER,
    @ActorId           UNIQUEIDENTIFIER,
    @NowUtc            DATETIME2(3),
    @AuditEngagementId UNIQUEIDENTIFIER,
    @SiteId            UNIQUEIDENTIFIER = NULL,
    @Status            NVARCHAR(20),
    @EngagementTitle   NVARCHAR(200),
    @DisciplineType    NVARCHAR(50),
    @Scope             NVARCHAR(2000),
    @LeadAuditorId     UNIQUEIDENTIFIER = NULL,
    @ScheduledAtUtc    DATETIME2(3) = NULL,
    @FindingsCount     INT,
    @ComplianceScore   DECIMAL(5,2) = NULL,
    @ComplianceRating  NVARCHAR(30),
    @CreatedAtUtc      DATETIME2(3),
    @CreatedBy         UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [Auditing].[AuditEngagement]
            ([AuditEngagementId], [TenantId], [SiteId], [Status], [EngagementTitle], [DisciplineType],
             [Scope], [LeadAuditorId], [ScheduledAtUtc], [FindingsCount], [ComplianceScore], [ComplianceRating],
             [CreatedAtUtc], [CreatedBy], [LastModifiedAtUtc])
        VALUES
            (@AuditEngagementId, @TenantId, @SiteId, @Status, @EngagementTitle, @DisciplineType,
             @Scope, @LeadAuditorId, @ScheduledAtUtc, @FindingsCount, @ComplianceScore, @ComplianceRating,
             @CreatedAtUtc, @CreatedBy, @NowUtc);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
