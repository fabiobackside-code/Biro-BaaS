# BIRO BaaS - Hexagonal Architecture

This project implements the Core de Autorização e Transacionamento of the BIRO BaaS platform, following a detailed Hexagonal Architecture (Ports & Adapters) pattern. This architecture isolates the core business logic from external concerns, such as databases, APIs, and other services, making the application highly maintainable, testable, and flexible.

## Arquitetura Hexagonal

The solution is structured into two main layers: the **Core** (Domain and Application) and the **Adapters** (Inbound and Outbound).

### Core

The Core contains the application's business logic and is divided into two projects:

*   **Domain.Core**: This project contains the domain entities, value objects, and the interfaces for the ports (inbound use cases and outbound repositories). It has no dependencies on any other layer.
*   **Application**: This project contains the implementation of the use cases defined in the Domain layer. It depends only on the Domain layer.

### Adapters

The Adapters are the implementation of the ports defined in the Domain layer. They are divided into two types:

*   **Inbound Adapters**: These adapters drive the application's business logic. In this solution, the Inbound Adapter is the REST API.
    *   **Adapters.Inbound.REST**: This project contains the API controllers, which receive requests from the outside world and call the application's use cases.
*   **Outbound Adapters**: These adapters are driven by the application's business logic. In this solution, the Outbound Adapter is the SQL Server database.
    *   **Adapters.Outbound.Persistence.SqlServer**: This project contains the concrete implementations of the repository and Unit of Work interfaces defined in the Domain layer. It uses Dapper to interact with the SQL Server database.

## Projetos

### `src/Domain/Core`

*   **Entities**: `Client`, `Account`, `Transaction`
*   **Ports/Inbound**: `IClientUseCases`, `IAccountUseCases`, `ITransactionUseCases`
*   **Ports/Outbound**: `IClientRepository`, `IAccountRepository`, `ITransactionRepository`, `IUnitOfWork`

### `src/Application`

*   **UseCases**: `ClientUseCases`, `AccountUseCases`, `TransactionUseCases`

### `src/Adapters/Inbound/REST`

*   **Controllers**: `ClientsController`, `AccountsController`, `TransactionsController`
*   **DependencyInjection**: `ApplicationModuleDependency`

### `src/Adapters/Outbound/Persistence/SqlServer`

*   **Repositories**: `ClientRepository`, `AccountRepository`, `TransactionRepository`
*   **UnitOfWork**: `UnitOfWork`
*   **DependencyInjection**: `PersistenceModuleDependency`

## Banco de Dados

The scripts to create the database and its objects are in the `database/scripts` directory.

*   `V1__Create_Tables.sql`: Creates the `Clients`, `Accounts`, and `Transactions` tables.
*   `V2__Create_Functions.sql`: Creates the `fn_GetAvailableBalance` and `fn_GetAccountingBalance` functions.

## Como Executar

1.  Execute the database scripts to create the database.
2.  Set the connection string in the `appsettings.json` file in the `Adapters.Inbound.REST` project.
3.  Run the `Adapters.Inbound.REST` project.
