-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Auditing.AuditFinding
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_AuditFinding_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Full-row update procedure for [Auditing].[AuditFinding], backing
--                  IAuditFindingRepository.UpdateAsync (TriggerCapa/Remediate/Verify
--                  lifecycle transitions). Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Auditing].[usp_AuditFinding_Update]
    @TenantId                 UNIQUEIDENTIFIER,
    @CorrelationId            UNIQUEIDENTIFIER,
    @ActorId                  UNIQUEIDENTIFIER,
    @NowUtc                   DATETIME2(3),
    @AuditFindingId           UNIQUEIDENTIFIER,
    @Status                   NVARCHAR(20),
    @Severity                 NVARCHAR(20),
    @CorrectiveActionRequired BIT,
    @AssignedToUserId         UNIQUEIDENTIFIER = NULL,
    @DueDateUtc               DATETIME2(3) = NULL,
    @VerifiedAtUtc            DATETIME2(3) = NULL,
    @RemediationNotes         NVARCHAR(2000) = NULL,
    @RemediatedAtUtc          DATETIME2(3) = NULL
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [Auditing].[AuditFinding]
        SET [Status]                   = @Status,
            [Severity]                 = @Severity,
            [CorrectiveActionRequired] = @CorrectiveActionRequired,
            [AssignedToUserId]         = @AssignedToUserId,
            [DueDateUtc]               = @DueDateUtc,
            [VerifiedAtUtc]            = @VerifiedAtUtc,
            [RemediationNotes]         = @RemediationNotes,
            [RemediatedAtUtc]          = @RemediatedAtUtc,
            [LastModifiedAtUtc]        = @NowUtc
        WHERE [AuditFindingId] = @AuditFindingId
          AND [TenantId]       = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
