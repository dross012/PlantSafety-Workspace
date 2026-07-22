-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AccidentManagement
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Accident_Insert.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Inserts a new Accident aggregate root row. Backs IAccidentRepository.AddAsync.
--                  TRY/CATCH with AuditDB/ErrorDB logging per the canonical write-procedure
--                  template. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [AccidentManagement].[usp_Accident_Insert]
    @AccidentId               UNIQUEIDENTIFIER,
    @TenantId                 UNIQUEIDENTIFIER,
    @SiteId                   UNIQUEIDENTIFIER = NULL,
    @Status                   NVARCHAR(30),
    @AccidentTitle            NVARCHAR(400),
    @Location                 NVARCHAR(400),
    @Severity                 NVARCHAR(30),
    @InjuredPersonCount       INT,
    @OccurredAtUtc            DATETIME2(3),
    @Category                 NVARCHAR(100),
    @SiteSecuredAndTriaged    BIT,
    @InvestigatorId           UNIQUEIDENTIFIER = NULL,
    @InvestigatorAssignedAtUtc DATETIME2(3) = NULL,
    @RootCause                NVARCHAR(30) = NULL,
    @RootCauseNotes           NVARCHAR(MAX) = NULL,
    @RootCauseRecordedAtUtc   DATETIME2(3) = NULL,
    @RegulatoryFilingRequired BIT,
    @RegulatoryAgency         NVARCHAR(200) = NULL,
    @RegulatoryReferenceNumber NVARCHAR(100) = NULL,
    @RegulatoryFiledAtUtc     DATETIME2(3) = NULL,
    @ClosedAtUtc              DATETIME2(3) = NULL,
    @OwnerId                  UNIQUEIDENTIFIER,
    @ReviewRequired           BIT,
    @CreatedAtUtc             DATETIME2(3),
    @CreatedBy                UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [AccidentManagement].[Accident]
        (
            AccidentId, TenantId, SiteId, Status, AccidentTitle, Location, Severity, InjuredPersonCount,
            OccurredAtUtc, Category, SiteSecuredAndTriaged, InvestigatorId, InvestigatorAssignedAtUtc,
            RootCause, RootCauseNotes, RootCauseRecordedAtUtc, RegulatoryFilingRequired, RegulatoryAgency,
            RegulatoryReferenceNumber, RegulatoryFiledAtUtc, ClosedAtUtc, OwnerId, ReviewRequired,
            CreatedAtUtc, CreatedBy, IsDeleted
        )
        VALUES
        (
            @AccidentId, @TenantId, @SiteId, @Status, @AccidentTitle, @Location, @Severity, @InjuredPersonCount,
            @OccurredAtUtc, @Category, @SiteSecuredAndTriaged, @InvestigatorId, @InvestigatorAssignedAtUtc,
            @RootCause, @RootCauseNotes, @RootCauseRecordedAtUtc, @RegulatoryFilingRequired, @RegulatoryAgency,
            @RegulatoryReferenceNumber, @RegulatoryFiledAtUtc, @ClosedAtUtc, @OwnerId, @ReviewRequired,
            @CreatedAtUtc, @CreatedBy, 0
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'Accident', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'AccidentId=', @AccidentId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
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
