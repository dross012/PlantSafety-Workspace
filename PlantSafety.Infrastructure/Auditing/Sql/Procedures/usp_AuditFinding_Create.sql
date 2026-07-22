-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Auditing.AuditFinding
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_AuditFinding_Create.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [Auditing].[AuditFinding], backing
--                  IAuditFindingRepository.AddAsync. Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Auditing].[usp_AuditFinding_Create]
    @TenantId                 UNIQUEIDENTIFIER,
    @CorrelationId            UNIQUEIDENTIFIER,
    @ActorId                  UNIQUEIDENTIFIER,
    @NowUtc                   DATETIME2(3),
    @AuditFindingId           UNIQUEIDENTIFIER,
    @SiteId                   UNIQUEIDENTIFIER = NULL,
    @Status                   NVARCHAR(20),
    @AuditEngagementId        UNIQUEIDENTIFIER,
    @FindingTitle             NVARCHAR(200),
    @Severity                 NVARCHAR(20),
    @CorrectiveActionRequired BIT,
    @AssignedToUserId         UNIQUEIDENTIFIER = NULL,
    @DueDateUtc               DATETIME2(3) = NULL,
    @CreatedAtUtc             DATETIME2(3),
    @CreatedBy                UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [Auditing].[AuditFinding]
            ([AuditFindingId], [TenantId], [SiteId], [Status], [AuditEngagementId], [FindingTitle],
             [Severity], [CorrectiveActionRequired], [AssignedToUserId], [DueDateUtc],
             [CreatedAtUtc], [CreatedBy], [LastModifiedAtUtc])
        VALUES
            (@AuditFindingId, @TenantId, @SiteId, @Status, @AuditEngagementId, @FindingTitle,
             @Severity, @CorrectiveActionRequired, @AssignedToUserId, @DueDateUtc,
             @CreatedAtUtc, @CreatedBy, @NowUtc);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
