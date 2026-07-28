-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AccidentManagement
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Accident_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Updates all mutable fields of an Accident aggregate root row. Backs
--                  IAccidentRepository.UpdateAsync. Additive-only evolution.
--  UPDATED:        2026-07-28 -- returns the post-update row's ETag (CONVERT(NVARCHAR(34),
--                  CAST(RowVersion AS BINARY(8)), 1)) via a trailing SELECT so callers have an
--                  up-to-date ETag immediately after update without a separate round-trip. Schema-
--                  only: no @ETag input parameter or conflict-rejection logic is introduced here --
--                  IAccidentRepository.UpdateAsync has no ETag concept to enforce (see
--                  AccidentDapperRepository.AccidentRow.ETag remarks); the pre-existing
--                  IF NOT EXISTS/THROW 50004 existence check above is unchanged.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [AccidentManagement].[usp_Accident_Update]
    @AccidentId                UNIQUEIDENTIFIER,
    @TenantId                  UNIQUEIDENTIFIER,
    @Status                    NVARCHAR(30),
    @Category                  NVARCHAR(100),
    @SiteSecuredAndTriaged     BIT,
    @InvestigatorId            UNIQUEIDENTIFIER = NULL,
    @InvestigatorAssignedAtUtc DATETIME2(3) = NULL,
    @RootCause                 NVARCHAR(30) = NULL,
    @RootCauseNotes            NVARCHAR(MAX) = NULL,
    @RootCauseRecordedAtUtc    DATETIME2(3) = NULL,
    @RegulatoryAgency          NVARCHAR(200) = NULL,
    @RegulatoryReferenceNumber NVARCHAR(100) = NULL,
    @RegulatoryFiledAtUtc      DATETIME2(3) = NULL,
    @ClosedAtUtc               DATETIME2(3) = NULL,
    @UpdatedBy                 UNIQUEIDENTIFIER = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        -- App-layer tenant scoping (defense-in-depth alongside RLS, per F1 in
        -- docs\lifecycle\security\rls-audit.md): a caller supplying another tenant's
        -- AccidentId now finds no matching row here, rather than relying on RLS alone.
        IF NOT EXISTS (SELECT 1 FROM [AccidentManagement].[Accident] WHERE AccidentId = @AccidentId AND TenantId = @TenantId AND IsDeleted = 0)
            THROW 50004, N'Accident not found or has been deleted.', 1;

        UPDATE [AccidentManagement].[Accident]
        SET
            Status = @Status,
            Category = @Category,
            SiteSecuredAndTriaged = @SiteSecuredAndTriaged,
            InvestigatorId = @InvestigatorId,
            InvestigatorAssignedAtUtc = @InvestigatorAssignedAtUtc,
            RootCause = @RootCause,
            RootCauseNotes = @RootCauseNotes,
            RootCauseRecordedAtUtc = @RootCauseRecordedAtUtc,
            RegulatoryAgency = @RegulatoryAgency,
            RegulatoryReferenceNumber = @RegulatoryReferenceNumber,
            RegulatoryFiledAtUtc = @RegulatoryFiledAtUtc,
            ClosedAtUtc = @ClosedAtUtc,
            UpdatedBy = @UpdatedBy,
            UpdatedAtUtc = SYSUTCDATETIME(),
            ConcurrencyToken = NEWID()
        WHERE AccidentId = @AccidentId AND TenantId = @TenantId AND IsDeleted = 0;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'Accident', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'AccidentId=', @AccidentId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [AccidentManagement].[Accident]
        WHERE AccidentId = @AccidentId AND TenantId = @TenantId;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AccidentId=', @AccidentId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
