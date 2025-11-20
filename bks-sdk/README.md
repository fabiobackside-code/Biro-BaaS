# BKS SDK - Framework para .NET 8

[![Version](https://img.shields.io/badge/version-2.1.0-blue.svg)](https://github.com/bks-sdk/bks-sdk)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-proprietary-red.svg)]()

Um framework robusto e modular para .NET 8 que oferece uma base sólida para desenvolvimento de aplicações financeiras com processamento de transações, autenticação, observabilidade e eventos de domínio.

> ⚠️ **IMPORTANTE**: Este README apresenta **apenas as funcionalidades realmente implementadas** no SDK. Funcionalidades como cache distribuído não estão implementadas ainda (veja seção "Limitações").

## 🚀 Características Principais

- 🔐 **Autenticação Completa**: Sistema de validação de licença e JWT integrado
- 📊 **Observabilidade Nativa**: OpenTelemetry, Serilog e tracing distribuído
- 🔄 **Processamento de Transações**: Pipeline seguro com tokenização e eventos
- 📡 **Sistema de Eventos**: Suporte para RabbitMQ
- 🏗️ **Clean Architecture**: Separação clara de responsabilidades
- 🔒 **Segurança**: Criptografia, correlação de transações e auditoria

## 📋 Índice

- [Padrões Arquiteturais](#-padrões-arquiteturais)
- [Estrutura e Namespaces](#️-estrutura-e-namespaces)
- [Instalação e Configuração](#-instalação-e-configuração)
- [Exemplos de Uso](#-exemplos-de-uso)
- [Links Úteis](#-links-úteis)

## 🏛️ Padrões Arquiteturais

O BKS SDK implementa os seguintes padrões arquiteturais:

### Clean Architecture
O SDK segue os princípios da Clean Architecture com separação clara entre:
- **Core**: Regras de negócio e configurações centrais
- **Application**: Casos de uso e orquestração
- **Infrastructure**: Implementações técnicas (Cache, Events, Auth)
- **Presentation**: Middlewares e configurações de API

### Domain-Driven Design (DDD)
- **Eventos de Domínio**: Modelagem de eventos importantes do negócio
- **Aggregates**: Transações como agregados com comportamentos encapsulados
- **Value Objects**: Objetos imutáveis para dados de transação
- **Repository Pattern**: Abstração para persistência

### Outros Padrões
- **Pipeline Pattern**: Pré e pós-processamento de transações
- **Factory Pattern**: Criação de processadores e brokers
- **Strategy Pattern**: Diferentes implementações de cache e eventos
- **Decorator Pattern**: Middlewares para cross-cutting concerns
- **Result Pattern**: Tratamento de erros sem exceções

## 🏗️ Estrutura e Namespaces

### `bks.sdk.Core`
**Configuração e inicialização central do SDK**

- **Configuration**: Gerenciamento de configurações via JSON
- **Middlewares**: Cross-cutting concerns (logging, correlação, auth)
- **Initialization**: Bootstrap e registro de dependências via `BKSFrameworkInitializer`
- **Pipeline**: Executor de pipeline para processamento de transações

**Principais Classes:**
- `BKSFrameworkSettings`: Configurações centralizadas
- `BKSFrameworkInitializer`: Inicializador principal do framework
- `IPipelineExecutor`: Interface para execução de pipelines de transação

### `bks.sdk.Security`
**Sistema de autenticação e autorização**

- **License Validation**: Validação de licenças do SDK
- **JWT Management**: Geração e validação de tokens JWT
- **Security**: Criptografia e segurança de dados

**Principais Interfaces:**
- `ILicenseValidator`: Validação de licenças
- `IJwtTokenProvider`: Gerenciamento de tokens JWT

### `bks.sdk.Processing`
**Núcleo do processamento de transações**

- **Transaction Processing**: Processadores específicos de transação
- **Pipeline**: Sistema de pipeline para processamento

**Principais Interfaces:**
- `IPipelineExecutor`: Executor de pipeline de transações

### `bks.sdk.Events`
**Sistema de eventos distribuídos**

- **Domain Events**: Modelagem de eventos de negócio
- **Event Brokers**: Integração com RabbitMQ
- **Dispatching**: Publicação e consumo de eventos

**Principais Interfaces:**
- `IDomainEvent`: Contrato para eventos de domínio
- `IEventBroker`: Abstração para brokers de mensagem
- `DomainEventDispatcher`: Dispatcher interno de eventos

### `bks.sdk.Observability`
**Monitoramento e diagnósticos**

- **Logging**: Integração com Serilog
- **Tracing**: OpenTelemetry para tracing distribuído
- **Metrics**: Coleta de métricas customizadas
- **Correlation**: Rastreamento de correlação entre requisições
- **Performance**: Tracking de performance

**Principais Interfaces:**
- `IBKSTracer`: Interface para tracing distribuído
- `ICorrelationContextAccessor`: Acesso ao contexto de correlação
- `IPerformanceTracker`: Rastreamento de performance

### `bks.sdk.Cache`
**Cache distribuído e local**

- **Abstrações**: Interface unificada para diferentes provedores
- **Implementations**: Redis e In-Memory
- **TTL Management**: Controle de tempo de vida

**Principais Interfaces:**
- `ICacheProvider`: Interface unificada para cache

### `bks.sdk.Validation`
**Sistema de validação**

- **Validation Rules**: Regras de validação de negócio
- **Validators**: Validadores específicos por domínio

## 📦 Instalação e Configuração

### Instalação

```bash
dotnet add package bks.sdk
```

### Configuração no `appsettings.json`

```json
{
  "BKSFramework": {
    "ApplicationName": "TransacoesAPI",
    "Security": {
      "LicenseKey": "BKS-2025-PREMIUM-KEY",
      "Jwt": {
        "SecretKey": "sua-chave-secreta-jwt-muito-segura",
        "Issuer": "TransacoesAPI",
        "Audience": "usuarios-api",
        "ExpirationInMinutes": 60
      }
    },
    "Cache": {
      "Provider": "Redis",
      "Redis": {
        "ConnectionString": "localhost:6379",
        "InstanceName": "transacoes-api",
        "Database": 0
      }
    },
    "Events": {
      "ConnectionString": "amqp://guest:guest@localhost:5672/",
      "AdditionalSettings": {
        "ExchangeName": "transacoes-events",
        "QueuePrefix": "transacoes",
        "RetryAttempts": "3"
      }
    },
    "Observability": {
      "ServiceName": "TransacoesAPI",
      "ServiceVersion": "1.0.0",
      "Logging": {
        "Level": "Information",
        "WriteToConsole": true,
        "WriteToFile": true,
        "FilePath": "logs/{ApplicationName}-.txt"
      },
      "Tracing": {
        "SamplingRate": 1.0,
        "OtlpEndpoint": "http://localhost:4317",
        "EnableConsoleExporter": false
      }
    }
  }
}
```

### Configuração no `Program.cs`

```csharp
using bks.sdk.Core.Initialization;

var builder = WebApplication.CreateBuilder(args);

// Configuração do BKS Framework
builder.Services.AddBKSFramework(builder.Configuration);

// Registro de repositórios e serviços
builder.Services.AddScoped<IContaRepository, ContaRepository>();

var app = builder.Build();

// Configuração de middlewares do BKS Framework
app.UseBKSFramework();

// Mapeamento dos endpoints
app.AddTransactionEndpoints();

app.Run();
```

## 💡 Exemplos de Uso

### Usando o Transaction Processor com Pipeline

#### Transação de Débito

```csharp
// Domain/Transactions/DebitoTransaction.cs
public class DebitoTransaction : BaseTransaction
{
    public string NumeroConta { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public string? Referencia { get; init; }
}

public class DebitoResponse
{
    public decimal NovoSaldo { get; set; }
    public DateTime DataProcessamento { get; set; }
}
```

#### Endpoint usando Pipeline

```csharp
// Endpoint usando Transaction Processor
group.MapPost("/debito", async (
    DebitoRequestDto request,
    IPipelineExecutor pipelineExecutor,
    CancellationToken cancellationToken) =>
{
    var transacao = new DebitoTransaction
    {
        NumeroConta = request.NumeroConta,
        Valor = request.Valor,
        Descricao = request.Descricao,
        Referencia = request.Referencia
    };

    var resultado = await pipelineExecutor.ExecuteAsync<DebitoTransaction, DebitoResponse>(
        transacao, cancellationToken);

    if (resultado.IsSuccess)
    {
        return Results.Ok(new TransacaoResponseDto
        {
            Sucesso = true,
            Mensagem = "Débito processado com sucesso via Transaction Processor!",
            TransacaoId = transacao.Id,
            Valor = request.Valor,
            NovoSaldo = resultado.Value?.NovoSaldo,
            ProcessadoPor = "Transaction Processor Pattern"
        });
    }

    return Results.BadRequest(new TransacaoResponseDto
    {
        Sucesso = false,
        Mensagem = resultado.Error,
        TransacaoId = transacao.Id
    });
})
.WithName("ProcessarDebito")
.WithSummary("Processar débito usando Transaction Processor");
```

### DTOs de Request/Response

```csharp
// DTOs/CreditoRequestDto.cs
public record CreditoRequestDto
{
    public string NumeroConta { get; init; } = string.Empty;
    public decimal Valor { get; init; }
    public string Descricao { get; init; } = string.Empty;
    public string? Referencia { get; init; }
}

public record DebitoRequestDto
{
    public string NumeroConta { get; init; } = string.Empty;
    public decimal Valor { get; init; }
    public string Descricao { get; init; } = string.Empty;
    public string? Referencia { get; init; }
}

public record TransacaoResponseDto
{
    public bool Sucesso { get; init; }
    public string Mensagem { get; init; } = string.Empty;
    public string? TransacaoId { get; init; }
    public decimal? Valor { get; init; }
    public decimal? NovoSaldo { get; init; }
    public string? ProcessadoPor { get; init; }
}
```

### Exemplo de Repositório

```csharp
// Infrastructure/Repositories/ContaRepository.cs
public interface IContaRepository
{
    Task<Conta?> GetByNumeroAsync(int numero, CancellationToken cancellationToken);
    Task UpdateAsync(Conta conta, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(int numero, CancellationToken cancellationToken);
}

public class ContaRepository : IContaRepository
{
    // Implementação específica (Entity Framework, Dapper, etc.)
    public async Task<Conta?> GetByNumeroAsync(int numero, CancellationToken cancellationToken)
    {
        // Implementação da consulta
        throw new NotImplementedException();
    }

    public async Task UpdateAsync(Conta conta, CancellationToken cancellationToken)
    {
        // Implementação da atualização
        throw new NotImplementedException();
    }

    public async Task<bool> ExistsAsync(int numero, CancellationToken cancellationToken)
    {
        // Implementação da verificação
        throw new NotImplementedException();
    }
}
```

### Exemplos de Requisições HTTP

#### Débito via Transaction Processor
```bash
curl -X POST "https://localhost:7001/api/sdk/v1/transactions/debito" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "numeroConta": "12345",
    "valor": 250.00,
    "descricao": "Débito de teste",
    "referencia": "DEB-001"
  }'
```

#### Consulta de Conta
```bash
curl -X GET "https://localhost:7001/api/sdk/v1/transactions/conta/12345" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## 📚 Links Úteis

### Documentação Oficial
- [.NET 8 Documentation](https://docs.microsoft.com/dotnet/core/whats-new/dotnet-8)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/instrumentation/net/)
- [Serilog Documentation](https://serilog.net/)
- [Minimal APIs Guide](https://docs.microsoft.com/aspnet/core/fundamentals/minimal-apis)

### Padrões Arquiteturais
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [Event Sourcing](https://martinfowler.com/eaaDev/EventSourcing.html)

### Observabilidade
- [OpenTelemetry Concepts](https://opentelemetry.io/docs/concepts/)
- [Distributed Tracing](https://opentelemetry.io/docs/concepts/distributed-tracing/)
- [Jaeger Tracing](https://www.jaegertracing.io/docs/)
- [Structured Logging with Serilog](https://serilog.net/)

### Mensageria
- [RabbitMQ .NET Client](https://www.rabbitmq.com/dotnet.html)
- [Event-Driven Architecture](https://martinfowler.com/articles/201701-event-driven.html)

### Livros Recomendados
- "Clean Architecture" - Robert C. Martin
- "Domain-Driven Design" - Eric Evans
- "Implementing Domain-Driven Design" - Vaughn Vernon
- "Patterns of Enterprise Application Architecture" - Martin Fowler
- "Building Microservices" - Sam Newman

### Ferramentas de Desenvolvimento
- **IDEs**: Visual Studio 2022, JetBrains Rider, VS Code
- **Testing**: xUnit, FluentAssertions, Testcontainers
- **Monitoring**: Jaeger, Prometheus, Grafana
- **API Testing**: Postman, Insomnia, REST Client
- **Documentation**: Swagger/OpenAPI, Markdown

---

**BKS SDK v2.1.0** - Desenvolvido com ❤️ pela equipe BKS para acelerar o desenvolvimento de aplicações financeiras robustas e escaláveis.

**Última atualização**: Novembro 2025