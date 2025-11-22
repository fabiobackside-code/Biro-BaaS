CREATE TABLE Clients (
    ClientId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TaxId VARCHAR(20) NOT NULL UNIQUE,
    FullName NVARCHAR(200) NOT NULL,
    Email VARCHAR(100),
    Phone VARCHAR(20),
    DateOfBirth DATE,
    Status VARCHAR(20) DEFAULT 'ACTIVE',
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),

    INDEX IX_Client_TaxId (TaxId),
    INDEX IX_Client_Status (Status)
);

CREATE TABLE Accounts (
    AccountId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ClientId UNIQUEIDENTIFIER NOT NULL,
    ProductType VARCHAR(50) NOT NULL,
    BranchCode VARCHAR(10) NOT NULL,
    AccountNumber VARCHAR(20) NOT NULL,
    Status VARCHAR(20) DEFAULT 'ACTIVE',
    OpenedAt DATETIME2 DEFAULT GETUTCDATE(),
    ClosedAt DATETIME2 NULL,

    CONSTRAINT FK_Account_Client FOREIGN KEY (ClientId)
        REFERENCES Clients(ClientId),

    CONSTRAINT UQ_Account_Branch_Number
        UNIQUE (BranchCode, AccountNumber),

    INDEX IX_Account_Client (ClientId),
    INDEX IX_Account_Status (Status)
);

CREATE TABLE Transactions (
    TransactionId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    AccountId UNIQUEIDENTIFIER NOT NULL,
    TransactionType VARCHAR(20) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL CHECK (Amount >= 0),
    Timestamp DATETIME2 DEFAULT GETUTCDATE(),
    CorrelationId UNIQUEIDENTIFIER NOT NULL,
    Status VARCHAR(20) DEFAULT 'ACTIVE',
    ExpirationDateTime DATETIME2 NULL,
    Metadata NVARCHAR(MAX),

    CONSTRAINT FK_Transaction_Account FOREIGN KEY (AccountId)
        REFERENCES Accounts(AccountId),

    INDEX IX_Transaction_Account_Timestamp (AccountId, Timestamp DESC),
    INDEX IX_Transaction_CorrelationId (CorrelationId),
    INDEX IX_Transaction_Status (Status, ExpirationDateTime)
        WHERE TransactionType IN ('BLOCK', 'RESERVATION')
);
