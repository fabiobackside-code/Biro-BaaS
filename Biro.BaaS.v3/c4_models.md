# C4 Models for BIRO BaaS v3

This document outlines the architecture of the BIRO BaaS v3 platform using the C4 model for visualizing software architecture.

## Level 1: System Context

The System Context diagram provides a high-level view of the system, showing how it interacts with its users and other systems.

```
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Context.puml

LAYOUT_WITH_LEGEND()

Person(customer, "Customer", "A customer of the digital bank.")
Person(admin, "Admin", "An administrator of the digital bank.")

System(biro_baas, "BIRO BaaS v3", "The Banking as a Service platform.")

System_Ext(ibass, "iBaaS", "The external banking service provider.")
System_Ext(sendgrid, "SendGrid", "The external email service provider.")

Rel(customer, biro_baas, "Uses")
Rel(admin, biro_baas, "Administers")

Rel(biro_baas, ibass, "Makes API calls to")
Rel(biro_baas, sendgrid, "Sends emails using")
@enduml
```

## Level 2: Container Diagram

The Container diagram zooms into the system, showing the high-level technical building blocks.

```
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Container.puml

LAYOUT_WITH_LEGEND()

Person(customer, "Customer", "A customer of the digital bank.")
Person(admin, "Admin", "An administrator of the digital bank.")

System_Ext(ibass, "iBaaS", "The external banking service provider.")
System_Ext(sendgrid, "SendGrid", "The external email service provider.")

System_Boundary(c1, "BIRO BaaS v3") {
    Container(api_gateway, "API Gateway", "YARP", "Routes incoming requests to the appropriate microservice.")
    Container(auth_service, "Authentication Service", ".NET 8 Minimal API", "Handles user authentication and issues JWT tokens.")
    Container(debit_service, "Debit Service", ".NET 8 Minimal API", "Handles debit transactions.")
    Container(credit_service, "Credit Service", ".NET 8 Minimal API", "Handles credit transactions.")
    Container(transfer_service, "Transfer Service", ".NET 8 Minimal API", "Handles transfers between accounts.")
    Container(balance_service, "Balance Service", ".NET 8 Minimal API", "Provides account balance information.")
    ContainerDb(database, "Database", "PostgreSQL", "Stores user data, accounts, and transaction history.")
    ContainerDb(cache, "Cache", "Redis", "Stores session information and caches frequently accessed data.")
    ContainerQueue(message_bus, "Message Bus", "RabbitMQ", "Handles asynchronous communication between services.")
}

Rel(customer, api_gateway, "Uses", "HTTPS")
Rel(admin, api_gateway, "Uses", "HTTPS")

Rel(api_gateway, auth_service, "Routes to")
Rel(api_gateway, debit_service, "Routes to")
Rel(api_gateway, credit_service, "Routes to")
Rel(api_gateway, transfer_service, "Routes to")
Rel(api_gateway, balance_service, "Routes to")

Rel(debit_service, database, "Reads from and writes to")
Rel(credit_service, database, "Reads from and writes to")
Rel(transfer_service, database, "Reads from and writes to")
Rel(balance_service, database, "Reads from")

Rel(debit_service, message_bus, "Publishes events to")
Rel(credit_service, message_bus, "Publishes events to")
Rel(transfer_service, message_bus, "Publishes events to")

Rel(debit_service, ibass, "Makes API calls to")
Rel(credit_service, ibass, "Makes API calls to")
Rel(transfer_service, ibass, "Makes API calls to")

Rel(auth_service, sendgrid, "Sends emails using")
@enduml
```

## Level 3: Component Diagram (Debit Service)

The Component diagram zooms into a single container to show its internal components.

```
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Component.puml

LAYOUT_WITH_LEGEND()

Container(debit_service, "Debit Service", ".NET 8 Minimal API", "Handles debit transactions.")

System_Boundary(c1, "Debit Service") {
    Component(debit_controller, "Debit Controller", "Minimal API Endpoint", "Receives HTTP requests for debit transactions.")
    Component(debit_handler, "Debit Handler", "C# Class", "Processes debit transactions using a SEDA pipeline.")
    Component(account_repository, "Account Repository", "Dapper Component", "Handles data access to the accounts table.")
    Component(transaction_repository, "Transaction Repository", "Dapper Component", "Handles data access to the transactions table.")
    Component(event_publisher, "Event Publisher", "MassTransit Component", "Publishes domain events to the message bus.")
}

Rel(debit_controller, debit_handler, "Invokes")
Rel(debit_handler, account_repository, "Uses")
Rel(debit_handler, transaction_repository, "Uses")
Rel(debit_handler, event_publisher, "Publishes events using")

@enduml
```
