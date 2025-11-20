CREATE TABLE clients (
    id UUID PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    document VARCHAR(255) NOT NULL
);

CREATE TABLE accounts (
    id UUID PRIMARY KEY,
    client_id UUID NOT NULL,
    account_number VARCHAR(255) NOT NULL,
    branch_code VARCHAR(255) NOT NULL,
    product_type INT NOT NULL,
    status INT NOT NULL,
    CONSTRAINT fk_client FOREIGN KEY(client_id) REFERENCES clients(id)
);

CREATE TABLE transactions (
    id UUID PRIMARY KEY,
    account_id UUID NOT NULL,
    transaction_type INT NOT NULL,
    amount DECIMAL(18, 2) NOT NULL,
    created_at TIMESTAMP NOT NULL,
    CONSTRAINT fk_account FOREIGN KEY(account_id) REFERENCES accounts(id)
);

CREATE OR REPLACE FUNCTION get_balance(accountId UUID)
RETURNS DECIMAL AS $$
DECLARE
    balance DECIMAL;
BEGIN
    SELECT
        (SELECT COALESCE(SUM(amount), 0) FROM transactions WHERE account_id = accountId AND transaction_type IN (1, 4)) -
        (SELECT COALESCE(SUM(amount), 0) FROM transactions WHERE account_id = accountId AND transaction_type IN (0, 2, 3))
    INTO balance;
    RETURN balance;
END;
$$ LANGUAGE plpgsql;
