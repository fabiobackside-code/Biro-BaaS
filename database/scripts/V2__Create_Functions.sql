CREATE FUNCTION dbo.fn_GetAvailableBalance(
    @AccountId UNIQUEIDENTIFIER
)
RETURNS DECIMAL(18,2)
AS
BEGIN
    DECLARE @Balance DECIMAL(18,2);

    SELECT @Balance = ISNULL(
        SUM(
            CASE
                WHEN TransactionType IN ('CREDIT', 'INITIAL_BALANCE')
                    THEN Amount
                WHEN TransactionType IN ('DEBIT')
                    THEN -Amount
                WHEN TransactionType IN ('BLOCK', 'RESERVATION')
                     AND Status = 'ACTIVE'
                    THEN -Amount
                ELSE 0
            END
        ), 0)
    FROM Transactions
    WHERE AccountId = @AccountId;

    RETURN @Balance;
END;
GO

CREATE FUNCTION dbo.fn_GetAccountingBalance(
    @AccountId UNIQUEIDENTIFIER
)
RETURNS DECIMAL(18,2)
AS
BEGIN
    DECLARE @Balance DECIMAL(18,2);

    SELECT @Balance = ISNULL(
        SUM(
            CASE
                WHEN TransactionType IN ('CREDIT', 'INITIAL_BALANCE')
                    THEN Amount
                WHEN TransactionType = 'DEBIT'
                    THEN -Amount
                ELSE 0
            END
        ), 0)
    FROM Transactions
    WHERE AccountId = @AccountId
        AND TransactionType NOT IN ('BLOCK', 'RESERVATION');

    RETURN @Balance;
END;
GO
