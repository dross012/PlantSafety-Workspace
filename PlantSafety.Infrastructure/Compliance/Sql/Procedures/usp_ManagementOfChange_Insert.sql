-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.ManagementOfChange
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_ManagementOfChange_Insert.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Inserts a new ManagementOfChange aggregate root row. Backs
--                  IManagementOfChangeRepository.AddAsync. TRY/CATCH with AuditDB/ErrorDB logging
--                  per the canonical write-procedure template (mirrors usp_Permits_Insert.sql).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_ManagementOfChange_Insert]
    @ManagementOfChangeId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @ChangeTitle NVARCHAR(400),
    @ChangeType NVARCHAR(100),
    @Justification NVARCHAR(2000),
    @RiskLevel NVARCHAR(50),
    @RequestedByUserId UNIQUEIDENTIFIER,
    @ApprovedByUserId UNIQUEIDENTIFIER = NULL,
    @ApprovalStatus NVARCHAR(50),
    @ImplementationAtUtc DATETIME2(3) = NULL,
    @ReviewDueAtUtc DATETIME2(3) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [Compliance].[ManagementOfChange]
        (
            ManagementOfChangeId, TenantId, SiteId, Status, ChangeTitle, ChangeType, Justification, RiskLevel, RequestedByUserId, ApprovedByUserId, ApprovalStatus, ImplementationAtUtc, ReviewDueAtUtc, CreatedAtUtc, CreatedBy, IsDeleted
        )
        VALUES
        (
            @ManagementOfChangeId, @TenantId, @SiteId, @Status, @ChangeTitle, @ChangeType, @Justification, @RiskLevel, @RequestedByUserId, @ApprovedByUserId, @ApprovalStatus, @ImplementationAtUtc, @ReviewDueAtUtc, @CreatedAtUtc, @CreatedBy, 0
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'ManagementOfChange', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ManagementOfChangeId=', @ManagementOfChangeId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ManagementOfChangeId=', @ManagementOfChangeId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
