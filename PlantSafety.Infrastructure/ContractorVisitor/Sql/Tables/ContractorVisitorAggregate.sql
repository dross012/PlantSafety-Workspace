-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContractorVisitor
--  Layer:          Infrastructure -- SQL Scripts
--  File:           ContractorVisitorAggregate.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Table backing the ContractorVisitor aggregate root
--                  (IContractorVisitorRepository). Tenant-scoped (TenantId NOT NULL
--                  uniqueidentifier). Phase 4 full build-out: this aggregate previously had zero
--                  persistence at all. Mirrors [Compliance].[AuditingAggregate]'s native ROWVERSION
--                  concurrency convention from day one. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'ContractorVisitor')
    EXEC('CREATE SCHEMA ContractorVisitor AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ContractorVisitorAggregate' AND schema_id = SCHEMA_ID('ContractorVisitor'))
BEGIN
    CREATE TABLE [ContractorVisitor].[ContractorVisitorAggregate]
    (
        ContractorVisitorId     UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ContractorVisitorAggregate PRIMARY KEY,
        TenantId                 UNIQUEIDENTIFIER NOT NULL,
        SiteId                   UNIQUEIDENTIFIER NULL,
        Status                   NVARCHAR(50)     NOT NULL,
        FullName                 NVARCHAR(200)    NOT NULL,
        CompanyName               NVARCHAR(200)    NOT NULL,
        VisitorType               NVARCHAR(50)     NOT NULL,
        PurposeOfVisit            NVARCHAR(2000)   NOT NULL,
        HostUserId                UNIQUEIDENTIFIER NOT NULL,
        ScheduledArrivalUtc       DATETIME2(3)     NULL,
        ActualArrivalUtc          DATETIME2(3)     NULL,
        CreatedAtUtc              DATETIME2(3)     NOT NULL,
        CreatedBy                 UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc              DATETIME2(3)     NULL,
        UpdatedBy                 UNIQUEIDENTIFIER NULL,
        IsDeleted                 BIT              NOT NULL CONSTRAINT DF_ContractorVisitorAggregate_IsDeleted DEFAULT (0),
        RowVersion                ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_ContractorVisitorAggregate_TenantId_Site ON [ContractorVisitor].[ContractorVisitorAggregate] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_ContractorVisitorAggregate_Status ON [ContractorVisitor].[ContractorVisitorAggregate] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_ContractorVisitorAggregate_CreatedAtUtc ON [ContractorVisitor].[ContractorVisitorAggregate] (CreatedAtUtc);
END
GO
