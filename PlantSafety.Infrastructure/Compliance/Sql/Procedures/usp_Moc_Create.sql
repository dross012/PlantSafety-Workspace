-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.ManagementOfChange
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Moc_Create.sql
--  Created:        2026-07-27
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Write stored procedure backing IMocRepository.CreateAsync. Creates a new MOC
--                  in Draft status.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_Moc_Create]
    @TenantId         UNIQUEIDENTIFIER,
    @CorrelationId    UNIQUEIDENTIFIER,
    @ActorId          UNIQUEIDENTIFIER,
    @NowUtc            DATETIME2(3),
    @MocId             UNIQUEIDENTIFIER,
    @SiteId            UNIQUEIDENTIFIER,
    @Title             NVARCHAR(200),
    @Description       NVARCHAR(4000),
    @ChangeType        NVARCHAR(50),
    @RiskLevel         NVARCHAR(50),
    @InitiatorId       UNIQUEIDENTIFIER,
    @ProposedStartUtc  DATETIME2(3),
    @ProposedEndUtc    DATETIME2(3) = NULL
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;

        INSERT INTO [Compliance].[Moc]
        (
            MocId, TenantId, SiteId, Title, Description, ChangeType, RiskLevel, InitiatorId,
            ProposedStartUtc, ProposedEndUtc, Status, LastModifiedAtUtc, CreatedAtUtc, CreatedBy, IsDeleted
        )
        VALUES
        (
            @MocId, @TenantId, @SiteId, @Title, @Description, @ChangeType, @RiskLevel, @InitiatorId,
            @ProposedStartUtc, @ProposedEndUtc, N'Draft', @NowUtc, @NowUtc, @ActorId, 0
        );

    COMMIT TRANSACTION;

    BEGIN TRY
        INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
        VALUES (N'Moc', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'MocId=', @MocId, N'; TenantId=', @TenantId, N'; CorrelationId=', @CorrelationId, N'; ActorId=', @ActorId), 0, OBJECT_NAME(@@PROCID));
    END TRY
    BEGIN CATCH
    END CATCH;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'MocId=', @MocId, N'; TenantId=', @TenantId, N'; CorrelationId=', @CorrelationId, N'; ActorId=', @ActorId, N'; User=', SYSTEM_USER);
    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

    BEGIN TRY
        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
    END TRY
    BEGIN CATCH
    END CATCH;

    THROW;
END CATCH;
GO
