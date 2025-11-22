# Estrutura da Solution Microservice.API

Com base no seu template e práticas adotadas, aqui está a descrição da estrutura de solution:

## Organização Arquitetural

```
microservice.api.sln
├── src/
│   ├── Domain/                          # Core - Coração do Sistema
│   │   ├── Core/
│   │   │   ├── Entities/               # Entidades de domínio
│   │   │   ├── ValueObjects/           # Objetos de valor
│   │   │   ├── Aggregates/             # Agregados DDD
│   │   │   └── Ports/                  # Interfaces (Contratos)
│   │   │       ├── Inbound/            # Portas de entrada (Use Cases)
│   │   │       └── Outbound/           # Portas de saída (Necessidades)
│   │   └── Exceptions/                 # Exceções de domínio
│   │
│   ├── Application/                     # Orquestração e Casos de Uso
│   │   ├── UseCases/                   # Implementação dos casos de uso
│   │   ├── Services/                   # Serviços de aplicação
│   │   └── DTOs/                       # Objetos de transferência
│   │
│   ├── Adapters/
│   │   │
│   │   ├── Inbound/                    # DRIVING SIDE - Quem INICIA a ação
│   │   │   ├── REST/                   # APIs RESTful
│   │   │   │   ├── MinimalApis/        # Minimal APIs (.NET)
│   │   │   │   └── Controllers/        # Controllers MVC
│   │   │   ├── gRPC/                   # Serviços gRPC
│   │   │   ├── SOAP/                   # Web Services SOAP
│   │   │   ├── GraphQL/                # Endpoints GraphQL
│   │   │   ├── WebSockets/             # Server Sockets / SignalR
│   │   │   └── Messaging/              # Consumers de Mensageria
│   │   │       ├── Kafka/              # Kafka Consumer
│   │   │       ├── RabbitMQ/           # RabbitMQ Consumer
│   │   │       └── PubSub/             # Google Pub/Sub Subscriber
│   │   │
│   │   └── Outbound/                   # DRIVEN SIDE - O que a aplicação USA
│   │       │
│   │       ├── Persistence/            # Camada de Persistência
│   │       │   ├── SQL/                # Bancos relacionais
│   │       │   │   ├── SqlServer/      # SQL Server
│   │       │   │   └── PostgreSQL/     # PostgreSQL
│   │       │   ├── NoSQL/              # Bancos não-relacionais
│   │       │   │   ├── MongoDB/        # MongoDB
│   │       │   │   ├── CosmosDB/       # Azure Cosmos DB
│   │       │   │   └── DynamoDB/       # AWS DynamoDB
│   │       │   └── Cache/              # Camada de cache
│   │       │       └── Redis/          # Redis
│   │       │
│   │       ├── Messaging/              # Mensageria (Publishers)
│   │       │   ├── Kafka/              # Kafka Producer
│   │       │   ├── RabbitMQ/           # RabbitMQ Publisher
│   │       │   └── PubSub/             # Google Pub/Sub Publisher
│   │       │
│   │       ├── Observability/          # Telemetria e Observabilidade
│   │       │   ├── Metrics/            # Métricas
│   │       │   │   ├── Prometheus/     # Prometheus exporter
│   │       │   │   └── AppInsights/    # Application Insights
│   │       │   ├── Logging/            # Logs estruturados
│   │       │   │   └── Serilog/        # Serilog
│   │       │   └── Tracing/            # Distributed Tracing
│   │       │       └── OpenTelemetry/  # OpenTelemetry
│   │       │
│   │       ├── ExternalServices/       # Integrações Externas
│   │       │   ├── PaymentGateway/     # Gateway de pagamento
│   │       │   ├── NotificationService/# Serviço de notificações
│   │       │   └── ThirdPartyAPIs/     # APIs de terceiros
│   │       │
│   │       └── Communication/          # Canais de Comunicação
│   │           ├── Email/              # SMTP / SendGrid
│   │           ├── SMS/                # Twilio / SNS
│   │           └── Push/               # Push Notifications
│   │
│   └── Microservice.API/               # Projeto de Inicialização
│       ├── Program.cs                  # Ponto de entrada
│       ├── appsettings.json            # Configurações
│       └── DependencyInjection/        # Registro de dependências
```

## Descrição das Camadas

### **1. Domain (Core) - O Coração Agnóstico**

O núcleo puro e agnóstico da aplicação que contém:

- **Entidades de Domínio**: representam conceitos de negócio
- **Value Objects**: objetos imutáveis sem identidade
- **Agregados**: clusters de entidades e value objects
- **Ports/Inbound**: interfaces que definem **o que a aplicação oferece** (casos de uso)
- **Ports/Outbound**: interfaces que definem **o que a aplicação precisa** (repositórios, serviços externos)

**Regra de Ouro**: Esta camada **NUNCA** depende de frameworks ou tecnologias específicas. Não pode ter referências a:
- `Microsoft.AspNetCore.*`
- `Microsoft.EntityFrameworkCore.*` 
- `MongoDB.Driver`
- `RabbitMQ.Client`
- Qualquer outra tecnologia de infraestrutura

### **2. Application - Orquestração e Casos de Uso**

Camada que implementa os **Use Cases** (Casos de Uso) definidos nas Ports/Inbound do Domain:

- Orquestra chamadas entre Domain e Adapters Outbound
- Implementa regras de negócio transacionais
- Coordena fluxos complexos de operações
- Recebe dependências via interfaces (Ports)
- Totalmente **agnóstica de tecnologia**

**Exemplo**:
```csharp
public class CreateOrderUseCase : ICreateOrderUseCase
{
    private readonly IOrderRepository _repository;      // Port Outbound
    private readonly IEmailService _emailService;       // Port Outbound
    private readonly IPaymentGateway _paymentGateway;   // Port Outbound
    
    public async Task<Order> Execute(CreateOrderCommand command)
    {
        // Orquestra as operações usando as portas (interfaces)
        var order = Order.Create(command);
        await _repository.Save(order);
        await _paymentGateway.Process(order);
        await _emailService.SendConfirmation(order);
        return order;
    }
}
```

### **3. Adapters/Inbound - DRIVING SIDE (Portas de Entrada)**

**Conceito**: São os adaptadores que **INICIAM** a interação com a aplicação. Eles "dirigem" a aplicação, disparando os casos de uso.

#### **Características dos Inbound Adapters:**
- Recebem requisições/eventos do mundo externo
- Convertem dados externos (JSON, XML, Protobuf) em comandos/queries da aplicação
- São **substituíveis** sem afetar o core
- Chamam os Use Cases através das Ports/Inbound

#### **Tipos de Inbound Adapters:**

**REST APIs:**
- **Minimal APIs**: endpoints HTTP com sintaxe minimalista do .NET
- **Controllers**: controladores MVC/API tradicionais
- Expõem funcionalidades via HTTP (GET, POST, PUT, DELETE)

**gRPC:**
- Serviços de alta performance com Protocol Buffers
- Comunicação síncrona entre microserviços

**SOAP Web Services:**
- Web Services legados com XML
- Integração com sistemas corporativos

**WebSockets/SignalR:**
- Comunicação em tempo real
- Server-sent events
- Conexões bidirecionais persistentes

**Messaging Consumers:**
- **Kafka Consumer**: consome mensagens de tópicos Kafka
- **RabbitMQ Consumer**: consome mensagens de filas RabbitMQ
- **Google Pub/Sub Subscriber**: assina tópicos Pub/Sub
- Processamento assíncrono de eventos

**GraphQL:**
- Queries e mutations customizadas
- Flexibilidade de requisição de dados

**Exemplo de Inbound Adapter:**
```csharp
// Minimal API
app.MapPost("/orders", async (CreateOrderRequest request, ICreateOrderUseCase useCase) =>
{
    var command = request.ToCommand();
    var order = await useCase.Execute(command);
    return Results.Created($"/orders/{order.Id}", order);
});
```

### **4. Adapters/Outbound - DRIVEN SIDE (Portas de Saída)**

**Conceito**: São os adaptadores que a aplicação **USA/PRECISA** para funcionar. Implementam as interfaces (Ports) definidas no Domain.

#### **Características dos Outbound Adapters:**
- Implementam as Ports/Outbound (interfaces) do Domain
- Encapsulam detalhes de infraestrutura
- São **substituíveis** sem afetar o core
- Podem ter múltiplas implementações da mesma Port

#### **Categorias de Outbound Adapters:**

**Persistence (Persistência):**
- **SQL Adapters**:
  - SQL Server com ADO.NET ou Dapper
  - PostgreSQL com Npgsql
  - Entity Framework Core (opcional)
- **NoSQL Adapters**:
  - MongoDB com MongoDB.Driver
  - Cosmos DB
  - DynamoDB
- **Cache Adapters**:
  - Redis para cache distribuído
  - Memcached

**Messaging (Mensageria - Publishers):**
- **Kafka Producer**: publica mensagens em tópicos
- **RabbitMQ Publisher**: envia mensagens para exchanges/queues
- **Google Pub/Sub Publisher**: publica em tópicos Pub/Sub
- Comunicação assíncrona entre serviços

**Observability (Telemetria e Observabilidade):**
- **Metrics**:
  - Prometheus exporter para métricas
  - Application Insights
- **Logging**:
  - Serilog estruturado
  - ELK Stack integration
- **Tracing**:
  - OpenTelemetry para rastreamento distribuído
  - Jaeger/Zipkin

**External Services (Integrações Externas):**
- APIs de terceiros (REST, SOAP, gRPC)
- Gateways de pagamento (Stripe, PayPal)
- Serviços de geolocalização
- ERPs e CRMs externos
- Microserviços vizinhos

**Communication (Canais de Comunicação):**
- **Email**: SMTP, SendGrid, Amazon SES
- **SMS**: Twilio, Amazon SNS
- **Push Notifications**: Firebase, OneSignal

**Exemplo de Outbound Adapter:**
```csharp
// Implementação de uma Port Outbound
public class SqlServerOrderRepository : IOrderRepository
{
    private readonly string _connectionString;
    
    public async Task Save(Order order)
    {
        using var connection = new SqlConnection(_connectionString);
        // Implementação específica do SQL Server
    }
}
```

## Fundamentos Arquiteturais

### **Arquitetura Hexagonal (Ports and Adapters)**

**Princípio Central**: Separação entre **"Inside"** (core) e **"Outside"** (infraestrutura)

```
       DRIVING SIDE              CORE              DRIVEN SIDE
    (Quem inicia)          (Agnóstico)         (O que é usado)
    
    [Minimal API] ──→ [Adapter] ──→ [Port] ──→ [Application] ──→ [Port] ──→ [Adapter] ──→ [SQL Server]
    [gRPC]       ──→ [Adapter] ──→           ──→            ──→          ──→ [Adapter] ──→ [Kafka]
    [WebSocket]  ──→ [Adapter] ──→           ──→            ──→          ──→ [Adapter] ──→ [Redis]
    [Kafka Cons] ──→ [Adapter] ──→           ──→            ──→          ──→ [Adapter] ──→ [Email]
```

### **Regras Fundamentais**

1. **Core Agnóstico**: Domain e Application não conhecem tecnologias específicas
2. **Dependency Inversion**: Infraestrutura depende do Core, nunca o contrário
3. **Ports como Contratos**: Interfaces definem o que o core precisa/oferece
4. **Adapters como Implementações**: Detalhes técnicos concretos
5. **Substituibilidade**: Trocar tecnologias sem modificar o core
6. **Isolamento de Módulos**: Cada adapter gerencia suas próprias dependências

### **Fluxo de Comunicação**

**Nenhum Port fala diretamente com outro Adapter** - sempre através do Core:

```
Inbound Adapter → Application Service → Outbound Adapter
```

**Exemplo Completo**:
```
[Minimal API] 
    → [CreateOrderAdapter] 
        → [CreateOrderUseCase (Application)]
            → [IOrderRepository (Port)]
                → [SqlServerOrderRepository (Adapter)]
                    → [SQL Server Database]
```

## Benefícios da Estrutura

1. **Flexibilidade Total de Entrada**: Suporta qualquer protocolo de comunicação (HTTP, gRPC, SOAP, WebSocket, mensageria)
2. **Independência de Infraestrutura**: Troque bancos, sistemas de mensageria ou APIs externas sem impactar o negócio
3. **Testabilidade Extrema**: Core testável com mocks simples, sem dependência de frameworks pesados
4. **Observabilidade Integrada**: Telemetria como adapter, não poluindo o código de negócio
5. **Evolução Arquitetural**: Migre de tecnologias facilmente (ex: SQL Server → PostgreSQL)
6. **Múltiplos Protocolos Simultâneos**: mesma lógica servida por REST, gRPC, SOAP e mensageria
7. **Manutenibilidade**: Código de negócio puro, legível para analistas de negócio

## Princípios SOLID Aplicados

### **Dependency Inversion Principle (DIP)**
- Core define interfaces (abstrações)
- Infraestrutura implementa essas interfaces
- Dependências apontam sempre para dentro (em direção ao core)

### **Single Responsibility Principle (SRP)**
- Cada adapter tem uma única responsabilidade
- Domain contém apenas lógica de negócio
- Application apenas orquestra

### **Interface Segregation Principle (ISP)**
- Ports específicas por necessidade
- Interfaces enxutas e focadas

### **Open/Closed Principle (OCP)**
- Aberto para extensão: novos adapters sem modificar o core
- Fechado para modificação: core estável

### **Liskov Substitution Principle (LSP)**
- Qualquer implementação de uma Port pode substituir outra
- Polimorfismo através de interfaces

## Gestão de Dependências

Cada módulo/adapter gerencia suas próprias dependências através de classes de extensão:

```csharp
// Exemplo: EmailModuleDependency.cs
public static class EmailModuleDependency
{
    public static IServiceCollection AddEmailAdapter(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.Configure<EmailSettings>(
            configuration.GetSection("Email"));
        
        services.AddTransient<IEmailService, SendGridEmailService>();
        
        return services;
    }
}
```

No `Program.cs`:
```csharp
// Registro modular de dependências
builder.Services.AddDomainServices();
builder.Services.AddApplicationServices();
builder.Services.AddSqlServerAdapter(builder.Configuration);
builder.Services.AddKafkaAdapter(builder.Configuration);
builder.Services.AddEmailAdapter(builder.Configuration);
builder.Services.AddPrometheusMetrics();
```

## Testabilidade

### **Testes Unitários do Core**
```csharp
[Fact]
public async Task CreateOrder_ShouldSaveAndNotify()
{
    // Arrange
    var mockRepository = new Mock<IOrderRepository>();
    var mockEmail = new Mock<IEmailService>();
    var useCase = new CreateOrderUseCase(
        mockRepository.Object, 
        mockEmail.Object);
    
    // Act
    await useCase.Execute(new CreateOrderCommand());
    
    // Assert
    mockRepository.Verify(r => r.Save(It.IsAny<Order>()), Times.Once);
    mockEmail.Verify(e => e.SendConfirmation(It.IsAny<Order>()), Times.Once);
}
```

### **Testes de Integração**
```csharp
[Fact]
public async Task MinimalAPI_ShouldCreateOrder()
{
    // Testa o adapter Inbound (Minimal API) com repositório real
    var client = _factory.CreateClient();
    var response = await client.PostAsJsonAsync("/orders", new { ... });
    
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
}
```

## Evolução e Manutenção

### **Substituir Tecnologias**
Para trocar SQL Server por PostgreSQL:
1. Criar novo adapter: `PostgreSqlOrderRepository : IOrderRepository`
2. Modificar apenas o registro de dependência no `Program.cs`
3. Core e Application permanecem intactos

### **Adicionar Novos Protocolos**
Para adicionar suporte gRPC:
1. Criar adapter Inbound: `Adapters/Inbound/gRPC/OrderService.cs`
2. Implementar proto files
3. Chamar os mesmos Use Cases do core
4. Registrar no `Program.cs`

### **Trocar Sistema de Mensageria**
Para migrar de RabbitMQ para Kafka:
1. Implementar `KafkaOrderPublisher : IOrderPublisher`
2. Trocar registro de dependência
3. Lógica de negócio não muda

## Conclusão

Esta estrutura garante que seu microserviço seja:
- **Resiliente**: mudanças em infraestrutura não quebram o negócio
- **Adaptável**: novas tecnologias podem ser adotadas rapidamente
- **Testável**: alta cobertura com testes rápidos e confiáveis
- **Manutenível**: separação clara de responsabilidades
- **Escalável**: fácil adicionar novos casos de uso e adapters
- **Preparado para o Futuro**: independente de frameworks e tecnologias específicas

A Arquitetura Hexagonal transforma seu código em um sistema verdadeiramente plug-and-play, onde componentes podem ser trocados como peças de LEGO, sem comprometer a integridade do sistema.
