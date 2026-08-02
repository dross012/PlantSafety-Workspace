-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContractorVisitor
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_ContractorVisitorAggregate_Insert.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Inserts a new ContractorVisitor aggregate root row. Backs
--                  IContractorVisitorRepository.AddAsync. TRY/CATCH with AuditDB/ErrorDB logging
--                  per the canonical write-procedure template. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [ContractorVisitor].[usp_ContractorVisitorAggregate_Insert]
    @ContractorVisitorId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @FullName NVARCHAR(200),
    @CompanyName NVARCHAR(200),
    @VisitorType NVARCHAR(50),
    @PurposeOfVisit NVARCHAR(2000),
    @HostUserId UNIQUEIDENTIFIER,
    @ScheduledArrivalUtc DATETIME2(3) = NULL,
    @ActualArrivalUtc DATETIME2(3) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [ContractorVisitor].[ContractorVisitorAggregate]
        (
            ContractorVisitorId, TenantId, SiteId, Status, FullName, CompanyName, VisitorType,
            PurposeOfVisit, HostUserId, ScheduledArrivalUtc, ActualArrivalUtc, CreatedAtUtc, CreatedBy, IsDeleted
        )
        VALUES
        (
            @ContractorVisitorId, @TenantId, @SiteId, @Status, @FullName, @CompanyName, @VisitorType,
            @PurposeOfVisit, @HostUserId, @ScheduledArrivalUtc, @ActualArrivalUtc, @CreatedAtUtc, @CreatedBy, 0
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'ContractorVisitorAggregate', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ContractorVisitorId=', @ContractorVisitorId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ContractorVisitorId=', @ContractorVisitorId, N'; User=', SYSTEM_USER);
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
