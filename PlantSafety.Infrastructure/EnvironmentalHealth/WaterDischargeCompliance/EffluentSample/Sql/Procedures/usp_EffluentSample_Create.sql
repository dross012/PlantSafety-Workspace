-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnvironmentalHealth.WaterDischargeCompliance.EffluentSample
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EffluentSample_Create.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure backing IEffluentSampleRepository.AddAsync. Chain-of-custody is append-only: this is the only write path for sample rows -- there is no delete/mutate path once inserted. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_EffluentSample_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @EffluentSampleId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @DischargePointId UNIQUEIDENTIFIER,
    @Parameter NVARCHAR(50),
    @MeasuredValue DECIMAL(18,4),
    @UnitOfMeasure NVARCHAR(50),
    @SampledAtUtc DATETIME2(3),
    @CollectedByUserId UNIQUEIDENTIFIER,
    @LabSourceSystem NVARCHAR(100),
    @ChainOfCustodyReference NVARCHAR(200),
    @CorrectionOfSampleId UNIQUEIDENTIFIER = NULL,
    @CorrectionReason NVARCHAR(500) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [EnvironmentalHealth].[EffluentSample]
            ([EffluentSampleId], [TenantId], [SiteId], [Status], [DischargePointId], [Parameter], [MeasuredValue],
             [UnitOfMeasure], [SampledAtUtc], [CollectedByUserId], [LabSourceSystem], [ChainOfCustodyReference],
             [CorrectionOfSampleId], [CorrectionReason], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@EffluentSampleId, @TenantId, @SiteId, @Status, @DischargePointId, @Parameter, @MeasuredValue,
             @UnitOfMeasure, @SampledAtUtc, @CollectedByUserId, @LabSourceSystem, @ChainOfCustodyReference,
             @CorrectionOfSampleId, @CorrectionReason, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
        -- Canonical audit trail (Handbook §2.11): the write is recorded after it commits, in its
        -- own TRY/CATCH so an audit failure can never fail the operation it is describing.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'EffluentSample', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'EffluentSampleId=', @EffluentSampleId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'EffluentSampleId=', @EffluentSampleId, N'; User=', SYSTEM_USER);
    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

    INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

    THROW;
END CATCH;
GO