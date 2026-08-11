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
        -- Canonical audit trail (Handbook §2.11): the write is recorded after it commits, in its
        -- own TRY/CATCH so an audit failure can never fail the operation it is describing.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'AuditFinding', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'AuditFindingId=', @AuditFindingId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AuditFindingId=', @AuditFindingId, N'; User=', SYSTEM_USER);
    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

    INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

    THROW;
END CATCH;
GO
