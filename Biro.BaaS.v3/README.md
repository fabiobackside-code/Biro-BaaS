# BIRO BaaS v3

## Project Overview

BIRO BaaS v3 is the third iteration of the Banking as a Service (BaaS) platform, designed from the ground up for high performance, scalability, and observability. This project merges the architectural vision of **BIRO 2.0** with the business requirements outlined in the **Proposta Portí** and the core domain rules from the **Premissas** document.

The primary goal is to build a robust, English-first, cloud-native BaaS platform capable of handling a high volume of financial transactions with minimal latency, leveraging the **bks.sdk** for core transactional capabilities.

## Core Architectural Principles

This project is built upon a modern, event-driven microservices architecture, adhering to the following principles:

- **High Performance**: Every component is designed for low latency and high throughput.
    - **Data Access**: **Dapper** is used as the exclusive micro-ORM for full control over SQL and maximum performance.
    - **APIs**: **.NET 8 Minimal APIs** are used to create lightweight, fast endpoints.
- **Asynchronous Processing**: **Staged Event-Driven Architecture (SEDA)** is implemented to handle concurrent operations efficiently and ensure system resilience.
- **Clean Architecture**: The solution is structured following Clean Architecture principles, separating concerns into distinct layers:
    - `Core`: Domain entities and application interfaces.
    - `Infrastructure`: Technical implementations (persistence, messaging, caching).
    - `Blocks`: Atomic microservices.
    - `Gateway`: A single entry point for external clients.
- **Observability**: Comprehensive monitoring is built-in using **OpenTelemetry**, with support for distributed tracing, metrics, and structured logging.
- **Idempotency**: All transactional endpoints are designed to be idempotent to prevent duplicate processing.

## Technology Stack

- **Runtime**: .NET 8
- **Language**: C# 12
- **Data Access**: Dapper
- **Messaging**: MassTransit with RabbitMQ
- **Caching**: Redis
- **API Gateway**: YARP (Yet Another Reverse Proxy)
- **Database**: PostgreSQL / SQL Server
- **Observability**: OpenTelemetry, Jaeger, Grafana, Serilog
- **Containerization**: Docker, Kubernetes
- **CI/CD**: GitHub Actions

## Project Structure

```
Biro.BaaS.v3/
│
├── src/
│   ├── Core/
│   ├── Infrastructure/
│   ├── Orchestration/
│   ├── Blocks/
│   ├── Gateway/
│   └── Shared/
│
├── tests/
│   ├── Unit/
│   └── Integration/
│
└── infra/
    ├── docker/
    ├── kubernetes/
    └── terraform/
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- Docker Desktop
- A running instance of PostgreSQL, RabbitMQ, and Redis.

### Setup

1.  **Clone the repository**:
    ```bash
    git clone <repository-url>
    ```
2.  **Configure services**:
    - Update connection strings in the `appsettings.json` file for each microservice.
3.  **Build the solution**:
    ```bash
    dotnet build
    ```
4.  **Run the application**:
    - Use `docker-compose up` from the `infra/docker` directory (once configured).
    - Or run each microservice individually using `dotnet run`.

## C4 Models

*(C4 Model documentation will be added here in a text-based format or linked to a separate document.)*
