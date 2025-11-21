# 🏦 BIRO - Arquitetura de Solução de Transacionamento Bancário

## 📋 Sumário Executivo

BIRO é uma solução de plataforma bancária moderna construída em três camadas principais: **Core de Autorização e Transacionamento**, **Camada BaaS (Banking as a Service)** e **Camada de Gateways e Integração Multicanal**. A arquitetura é projetada para alta performance, escalabilidade horizontal, consistência transacional e flexibilidade para suportar múltiplos produtos financeiros e canais de atendimento.

**Princípios Arquiteturais Fundamentais:**
- Event Sourcing Light com imutabilidade de dados transacionais
- Processamento assíncrono com Async Request-Reply Pattern
- Arquitetura desacoplada inspirada em SEDA (Staged Event-Driven Architecture)
- Separação clara de responsabilidades entre camadas
- Performance otimizada através de Dapper e SQL Functions

---

## 1. 🎯 Core de Autorização e Transacionamento (Núcleo Principal)

O Core é o motor financeiro da solução BIRO, responsável por toda a lógica crítica de processamento de transações, cálculo de saldos, validação de regras de negócio e garantia de consistência transacional. É implementado com foco em **performance extrema**, **integridade de dados** e **auditabilidade total**.

### 1.1. 🔬 Transações Atômicas (Low-Level Transactions)

As transações atômicas representam as operações fundamentais e indivisíveis que formam a base de todas as operações bancárias. São implementadas como comandos imutáveis que geram eventos de movimentação no sistema.

#### 1.1.1. **DEBIT (Débito)**

**Definição:** Operação que reduz o saldo disponível de uma conta.

**Características:**
- Registra uma movimentação do tipo `DEBIT` no histórico transacional
- Impacta negativamente o cálculo de saldo disponível
- Requer validação prévia de saldo suficiente
- Gera evento imutável no log de transações

**Uso:** Base para pagamentos, transferências de saída, saques, tarifas.

**Implementação:**
```
Transaction {
  Id: GUID
  AccountId: GUID
  TransactionType: DEBIT
  Amount: Decimal (positivo)
  Timestamp: DateTime
  CorrelationId: GUID
  Metadata: JSON
}
```

**Validações:**
- Saldo disponível >= Amount
- Conta ativa e não bloqueada
- Limites de transação respeitados

#### 1.1.2. **CREDIT (Crédito)**

**Definição:** Operação que aumenta o saldo disponível de uma conta.

**Características:**
- Registra uma movimentação do tipo `CREDIT` no histórico transacional
- Impacta positivamente o cálculo de saldo disponível
- Não requer validação de saldo prévio
- Gera evento imutável no log de transações

**Uso:** Base para recebimentos, transferências de entrada, depósitos, estornos.

**Implementação:**
```
Transaction {
  Id: GUID
  AccountId: GUID
  TransactionType: CREDIT
  Amount: Decimal (positivo)
  Timestamp: DateTime
  CorrelationId: GUID
  Metadata: JSON
}
```

**Validações:**
- Conta ativa e receptora válida
- Limites de recebimento respeitados (se aplicável)

#### 1.1.3. **TRANSFER (Transferência)**

**Definição:** Operação composta que move valor entre duas contas de forma atômica.

**Características:**
- Operação ACID garantida (Atomicidade, Consistência, Isolamento, Durabilidade)
- Internamente executa um DEBIT na conta origem e um CREDIT na conta destino
- Ambas operações devem ser bem-sucedidas ou nenhuma é efetivada
- Utiliza transação de banco de dados ou compensação

**Uso:** TED, DOC, PIX, transferências entre contas do mesmo titular.

**Implementação:**
```
Transfer {
  Id: GUID
  SourceAccountId: GUID
  DestinationAccountId: GUID
  Amount: Decimal
  TransferType: TED | DOC | PIX | INTERNAL
  Timestamp: DateTime
  Status: PENDING | COMPLETED | FAILED | COMPENSATED
}

// Gera internamente:
// 1. DEBIT na SourceAccountId
// 2. CREDIT na DestinationAccountId
```

**Validações:**
- Validações do DEBIT para conta origem
- Validações do CREDIT para conta destino
- Contas não podem ser idênticas (para transferências externas)

#### 1.1.4. **BLOCK (Bloqueio de Valor)**

**Definição:** Operação que reserva temporariamente um valor da conta, reduzindo o saldo disponível sem efetuar a saída definitiva.

**Características:**
- Registra movimentação do tipo `BLOCK`
- Reduz saldo disponível imediatamente
- Não representa saída efetiva de dinheiro ainda
- Possui data/hora de expiração
- Pode ser desfeita (UNBLOCK) ou efetivada (SETTLE)

**Uso:** Pré-autorização de cartões, reservas de pagamento, validação de fundos.

**Implementação:**
```
Transaction {
  Id: GUID
  AccountId: GUID
  TransactionType: BLOCK
  Amount: Decimal
  ExpirationDateTime: DateTime
  Reference: String
  Timestamp: DateTime
  Status: ACTIVE | EXPIRED | RELEASED | SETTLED
}
```

**Validações:**
- Saldo disponível >= Amount
- Conta ativa

**Fórmula de Impacto:**
```
Saldo Disponível = (CREDIT + INITIAL_BALANCE) - (DEBIT + BLOCK + RESERVATION)
```

#### 1.1.5. **UNBLOCK (Desbloqueio de Valor)**

**Definição:** Operação que libera um bloqueio previamente estabelecido, devolvendo o valor ao saldo disponível.

**Características:**
- Cancela um BLOCK específico
- Restaura o valor ao saldo disponível
- Não gera nova movimentação financeira (apenas marca o BLOCK como liberado)
- Pode ser parcial ou total

**Uso:** Cancelamento de pré-autorização de cartão, liberação de reserva não utilizada.

**Implementação:**
```
UnblockCommand {
  BlockTransactionId: GUID
  UnblockAmount: Decimal (se parcial)
  Reason: String
  Timestamp: DateTime
}

// Atualiza o status do BLOCK original
// Pode gerar um registro de auditoria complementar
```

#### 1.1.6. **RESERVATION (Reserva de Valor)**

**Definição:** Similar ao BLOCK, mas com semântica de negócio diferente - usado para reservas temporárias em operações mais complexas.

**Características:**
- Registra movimentação do tipo `RESERVATION`
- Comportamento idêntico ao BLOCK em termos de impacto no saldo
- Diferenciação semântica para contextos de negócio específicos
- Suporta efetivação (SETTLE) ou cancelamento (CANCEL)

**Uso:** Reserva de limite para operações multi-step, garantias temporárias, **e especialmente para operações PIX (conforme regras BACEN/SPI)**.

**Diferença entre BLOCK e RESERVATION:**
- **BLOCK:** Usado para autorizações de cartão e operações que podem expirar naturalmente
- **RESERVATION:** Usado para PIX, transferências TED/DOC e operações que exigem confirmação explícita de sistemas externos

**Implementação:**
```
Transaction {
  Id: GUID
  AccountId: GUID
  TransactionType: RESERVATION
  Amount: Decimal
  ExpirationDateTime: DateTime
  OperationContext: String
  Timestamp: DateTime
  Status: ACTIVE | EXPIRED | CANCELLED | SETTLED
}
```

#### 1.1.7. **SETTLE (Efetivação de Reserva)**

**Definição:** Operação que converte uma RESERVATION ou BLOCK em débito definitivo.

**Características:**
- Transforma uma reserva temporária em DEBIT efetivo
- O valor já estava bloqueado, não requer nova validação de saldo
- Operação atômica que libera a reserva e registra o débito
- Pode ser parcial (efetiva menos que o valor reservado)

**Uso:** Captura de cartão após pré-autorização, conclusão de operação em duas fases.

**Implementação:**
```
SettleCommand {
  ReservationId: GUID (ou BlockId)
  SettleAmount: Decimal
  Timestamp: DateTime
  Metadata: JSON
}

// Resultado:
// 1. Marca RESERVATION/BLOCK como SETTLED
// 2. Gera DEBIT pelo valor efetivado
// 3. Se parcial, libera diferença
```

#### 1.1.8. **CANCEL_RESERVATION (Cancelamento de Reserva)**

**Definição:** Operação que cancela uma RESERVATION, liberando o valor reservado.

**Características:**
- Cancela uma RESERVATION específica
- Restaura valor ao saldo disponível
- Equivalente ao UNBLOCK para reservas
- Irreversível após execução

**Uso:** Timeout de operação, cancelamento de transação antes da captura.

**Implementação:**
```
CancelReservationCommand {
  ReservationId: GUID
  CancelReason: String
  Timestamp: DateTime
}

// Marca RESERVATION como CANCELLED
// Libera o valor bloqueado
```

#### 1.1.9. **INITIAL_BALANCE (Lançamento Inicial)**

**Definição:** Operação especial para estabelecimento de saldo inicial de uma conta.

**Características:**
- Registra movimentação do tipo `INITIAL_BALANCE`
- Usado na abertura de contas ou migração de saldos
- Impacta positivamente o cálculo de saldo
- Geralmente executado apenas uma vez por conta

**Uso:** Abertura de conta, migração de sistema legado, ajustes de conciliação.

**Implementação:**
```
Transaction {
  Id: GUID
  AccountId: GUID
  TransactionType: INITIAL_BALANCE
  Amount: Decimal
  Timestamp: DateTime
  OriginSystem: String
  AuthorizingUser: String
}
```

---

### 1.2. 🏢 Transações de Negócio (High-Level Business Transactions)

As transações de negócio são operações completas e complexas que orquestram múltiplas transações atômicas para implementar funcionalidades bancárias de alto nível. Estas operações representam produtos e serviços bancários reais oferecidos aos clientes.

#### 1.2.1. **Pagamentos**

**Tipos de Pagamento:**

##### A. **Pagamento de Boleto**

**Fluxo:**
1. Validação do código de barras/linha digitável
2. **RESERVATION** do valor + tarifa na conta pagadora
3. Envio para processamento externo (câmara de compensação)
4. Recebimento de confirmação:
   - Sucesso: **SETTLE** da RESERVATION → DEBIT efetivo
   - Falha: **CANCEL_RESERVATION** → devolução do valor
5. Registro do pagamento no histórico
6. Geração de comprovante

**Transações Atômicas Envolvidas:**
- RESERVATION (valor + tarifa)
- SETTLE ou CANCEL_RESERVATION (conforme resultado)

**Dados Persistidos:**
```
Payment {
  Id: GUID
  AccountId: GUID
  PaymentType: BOLETO
  Barcode: String
  Amount: Decimal
  Fee: Decimal
  DueDate: Date
  PaymentDate: DateTime
  Status: PENDING | AUTHORIZED | SETTLED | FAILED
  ReservationTransactionId: GUID
  SettleTransactionId: GUID (nullable)
}
```

##### B. **Pagamento de Convênios**

**Fluxo:**
Similar ao boleto, mas com integração específica para cada convênio (água, luz, telefone, etc.).

**Características Adicionais:**
- Validação de código de cliente junto ao convênio
- Consulta de segunda via se necessário
- Registro de histórico por convênio

##### C. **Pagamento PIX**

Ver seção dedicada ao PIX (1.2.4).

#### 1.2.2. **Consulta de Saldos**

**Definição:** Operação de leitura que calcula e retorna os diferentes tipos de saldo de uma conta.

**Tipos de Saldo:**

##### A. **Saldo Contábil (Accounting Balance)**
```
Saldo Contábil = Σ(CREDIT + INITIAL_BALANCE) - Σ(DEBIT)
```
Representa o saldo "real" considerando apenas movimentações definitivas.

##### B. **Saldo Disponível (Available Balance)**
```
Saldo Disponível = Σ(CREDIT + INITIAL_BALANCE) - Σ(DEBIT + BLOCK + RESERVATION)
```
Representa o valor que pode ser efetivamente utilizado, considerando bloqueios e reservas.

##### C. **Saldo Bloqueado (Blocked Balance)**
```
Saldo Bloqueado = Σ(BLOCK + RESERVATION where Status = ACTIVE)
```
Representa o total de valores temporariamente indisponíveis.

**Implementação:**

A consulta é realizada através de **SQL Functions** no SQL Server, conforme premissa:

```sql
CREATE FUNCTION dbo.fn_GetAvailableBalance(@AccountId UNIQUEIDENTIFIER)
RETURNS DECIMAL(18,2)
AS
BEGIN
    DECLARE @Balance DECIMAL(18,2);
    
    SELECT @Balance = 
        ISNULL(SUM(CASE 
            WHEN TransactionType IN ('CREDIT', 'INITIAL_BALANCE') THEN Amount
            WHEN TransactionType IN ('DEBIT', 'BLOCK', 'RESERVATION') THEN -Amount
            ELSE 0
        END), 0)
    FROM Transactions
    WHERE AccountId = @AccountId
        AND (TransactionType NOT IN ('BLOCK', 'RESERVATION') 
             OR Status = 'ACTIVE');
    
    RETURN @Balance;
END;
```

**API Response:**
```json
{
  "accountId": "uuid",
  "accountNumber": "12345-6",
  "branchCode": "0001",
  "accountingBalance": 5000.00,
  "availableBalance": 4200.00,
  "blockedBalance": 800.00,
  "timestamp": "2025-11-21T10:30:00Z"
}
```

#### 1.2.3. **Extratos**

**Definição:** Operação de consulta que retorna o histórico de movimentações de uma conta.

**Tipos de Extrato:**

##### A. **Extrato por Período**
```
GET /accounts/{accountId}/statements?startDate=2025-01-01&endDate=2025-01-31
```

##### B. **Extrato por Tipo de Transação**
```
GET /accounts/{accountId}/statements?transactionType=DEBIT&limit=50
```

##### C. **Extrato Completo (Full Audit Trail)**
```
GET /accounts/{accountId}/statements/full?includeBlocks=true
```

**Dados Retornados:**
```json
{
  "accountId": "uuid",
  "period": {
    "start": "2025-01-01",
    "end": "2025-01-31"
  },
  "openingBalance": 1000.00,
  "closingBalance": 5000.00,
  "transactions": [
    {
      "id": "uuid",
      "timestamp": "2025-01-15T14:30:00Z",
      "transactionType": "CREDIT",
      "amount": 4500.00,
      "description": "Transferência recebida",
      "correlationId": "uuid",
      "balanceAfter": 5500.00
    },
    {
      "id": "uuid",
      "timestamp": "2025-01-20T09:15:00Z",
      "transactionType": "DEBIT",
      "amount": 500.00,
      "description": "Pagamento de boleto",
      "correlationId": "uuid",
      "balanceAfter": 5000.00
    }
  ],
  "summary": {
    "totalCredits": 4500.00,
    "totalDebits": 500.00,
    "netChange": 4000.00
  }
}
```

**Implementação:**
- Query direta na tabela de Transactions com filtros
- Ordenação por timestamp
- Paginação para grandes volumes
- Cálculo incremental de saldo após cada transação (para exibição)

#### 1.2.4. **PIX (Sistema de Pagamentos Instantâneos)**

> **⚠️ CONFORMIDADE BACEN/SPI:** Operações PIX utilizam **RESERVATION** (não BLOCK) para conformidade com as regras do Sistema de Pagamentos Instantâneos do BACEN. Isso garante rastreabilidade adequada e aderência aos padrões regulatórios de pagamentos instantâneos.

O PIX representa uma família completa de operações que interagem com o BACEN (Banco Central) através do SPI (Sistema de Pagamentos Instantâneos).

##### A. **PIX - Pagamento (PIX Out)**

**Fluxo:**
1. Validação da chave PIX de destino
2. Consulta de dados do beneficiário (via DICT)
3. **RESERVATION** do valor na conta pagadora (conforme regras BACEN/SPI)
4. Envio da ordem de pagamento ao SPI
5. Recebimento de confirmação do BACEN:
   - Sucesso: **SETTLE** da RESERVATION → DEBIT efetivo
   - Falha: **CANCEL_RESERVATION** → devolução do valor
6. Geração de comprovante com ID end-to-end

**Transações Atômicas:**
- RESERVATION (inicial)
- SETTLE ou CANCEL_RESERVATION (conforme resultado)

**Observação Importante:** PIX utiliza RESERVATION (não BLOCK) para conformidade com as regras do SPI, que exigem rastreabilidade específica e timeout controlado para operações instantâneas.

**Dados:**
```
PixPayment {
  Id: GUID
  EndToEndId: String (único no SPI)
  AccountId: GUID
  PixKeyType: CPF | CNPJ | EMAIL | PHONE | EVP
  PixKeyValue: String
  BeneficiaryName: String
  BeneficiaryTaxId: String
  Amount: Decimal
  Description: String (opcional)
  Timestamp: DateTime
  Status: INITIATED | AUTHORIZED | SETTLED | FAILED | REFUNDED
  ReservationTransactionId: GUID
}
```

##### B. **PIX - Recebimento (PIX In)**

**Fluxo:**
1. Recebimento de notificação do SPI
2. Validação da mensagem e autenticidade
3. Identificação da conta destinatária via chave PIX
4. CREDIT na conta destinatária
5. Notificação ao cliente (push, webhook, etc.)
6. Envio de confirmação ao SPI

**Transações Atômicas:**
- CREDIT

##### C. **PIX - Devolução (PIX Refund)**

**Fluxo:**
1. Solicitação de devolução (total ou parcial)
2. Validação do PIX original
3. Reversão:
   - Conta que recebeu: DEBIT
   - Conta que pagou: CREDIT (via SPI)
4. Atualização do status da transação original

**Transações Atômicas:**
- DEBIT (na conta que recebeu)
- CREDIT (na conta original, via SPI)

##### D. **PIX - Consulta de Chave (DICT Lookup)**

**Operação de Consulta:**
- Consulta no DICT (Diretório de Identificadores de Contas)
- Retorna dados da conta associada à chave
- Não gera movimentação financeira

##### E. **PIX - Cobrança (PIX QR Code)**

**Tipos:**
- **QR Code Estático:** Valor fixo, múltiplos pagamentos
- **QR Code Dinâmico:** Valor específico, pagamento único

**Fluxo:**
1. Geração do QR Code com dados da cobrança
2. Registro no sistema
3. Aguarda pagamento via PIX
4. Processamento como PIX In ao receber
5. Baixa da cobrança

#### 1.2.5. **Operações de Cartão**

##### A. **Autorização de Cartão (Card Authorization)**

**Fluxo:**
1. Recebimento de mensagem ISO 8583 do autorizador
2. Validação de dados do cartão e CVV
3. Validação de senha (se presente)
4. Verificação de limites e saldo
5. BLOCK do valor da compra
6. Resposta ao autorizador (aprovado/negado)

**Transações Atômicas:**
- BLOCK

**Dados:**
```
CardAuthorization {
  Id: GUID
  CardId: GUID
  AccountId: GUID
  MerchantName: String
  MerchantCategory: String
  Amount: Decimal
  Currency: String
  AuthorizationCode: String
  Timestamp: DateTime
  Status: APPROVED | DENIED
  DenialReason: String (nullable)
  BlockTransactionId: GUID (nullable)
}
```

##### B. **Captura de Cartão (Card Capture)**

**Fluxo:**
1. Recebimento de confirmação da captura
2. Identificação do BLOCK correspondente
3. SETTLE do BLOCK → DEBIT definitivo
4. Registro da captura

**Transações Atômicas:**
- SETTLE

##### C. **Cancelamento de Compra**

**Fluxo:**
1. Recebimento da solicitação de cancelamento
2. Identificação da transação original
3. Se ainda em BLOCK:
   - UNBLOCK do valor
4. Se já capturada (DEBIT):
   - CREDIT de estorno

**Transações Atômicas:**
- UNBLOCK ou CREDIT

#### 1.2.6. **Investimentos**

##### A. **Aplicação em Investimento**

**Fluxo:**
1. Seleção do produto de investimento
2. **RESERVATION** do valor + IOF (se aplicável)
3. Envio para processamento da aplicação
4. Confirmação:
   - Sucesso: **SETTLE** → DEBIT + registro da aplicação
   - Falha: **CANCEL_RESERVATION**

**Transações Atômicas:**
- RESERVATION
- SETTLE ou CANCEL_RESERVATION

##### B. **Resgate de Investimento**

**Fluxo:**
1. Solicitação de resgate (total ou parcial)
2. Cálculo de rentabilidade e IR
3. Processamento do resgate
4. CREDIT do valor líquido na conta
5. Atualização do saldo do investimento

**Transações Atômicas:**
- CREDIT

#### 1.2.7. **Empréstimos**

##### A. **Contratação de Empréstimo**

**Fluxo:**
1. Análise de crédito (fora do Core)
2. Aprovação e assinatura do contrato
3. CREDIT do valor liberado
4. Registro da dívida e parcelas

**Transações Atômicas:**
- CREDIT

##### B. **Débito de Parcela**

**Fluxo:**
1. Identificação de parcela vencida
2. DEBIT do valor da parcela
3. Baixa da parcela no contrato
4. Atualização do saldo devedor

**Transações Atômicas:**
- DEBIT

#### 1.2.8. **Marketplace / E-commerce**

##### A. **Split de Pagamento**

**Fluxo:**
1. Recebimento de pagamento do cliente
2. CREDIT na conta do marketplace
3. DEBIT do marketplace + CREDIT para sellers (múltiplos)
4. DEBIT de tarifas

**Transações Atômicas:**
- CREDIT, múltiplos DEBIT, múltiplos CREDIT

##### B. **Custódia de Valores**

**Fluxo:**
1. CREDIT do pagamento em conta de custódia
2. **RESERVATION** até confirmação de entrega
3. Confirmação:
   - Entregue: **SETTLE** → transferência ao vendedor
   - Problema: **CANCEL_RESERVATION** → devolução ao comprador

#### 1.2.9. **TED (Transferência Eletrônica Disponível)**

**Fluxo:**
1. Validação de dados bancários destino
2. **RESERVATION** do valor + tarifa
3. Envio ao SPB (Sistema de Pagamentos Brasileiro)
4. Confirmação:
   - Sucesso: **SETTLE** → DEBIT efetivo
   - Falha: **CANCEL_RESERVATION**
5. Atualização de status

**Transações Atômicas:**
- RESERVATION, SETTLE ou CANCEL_RESERVATION

**Diferença para PIX:**
- Processamento em lotes (janelas de liquidação)
- Tarifa mais alta
- Horário de funcionamento limitado

#### 1.2.10. **Saques e Depósitos**

##### A. **Saque em Caixa Eletrônico (ATM)**

**Fluxo:**
1. Recebimento de mensagem ISO 8583
2. Validação de senha e limites
3. DEBIT do valor + tarifa
4. Autorização ao ATM
5. Confirmação de dispensação

**Transações Atômicas:**
- DEBIT

##### B. **Depósito em Envelope**

**Fluxo:**
1. Registro do depósito pendente
2. Validação física posterior (back-office)
3. Confirmação:
   - Valor confere: CREDIT na conta
   - Divergência: CREDIT ajustado + notificação

**Transações Atômicas:**
- CREDIT

##### C. **Depósito Identificado (Boleto)**

**Fluxo:**
1. Leitura do código de barras
2. Identificação da conta destino
3. CREDIT imediato ou D+1 (conforme política)

**Transações Atômicas:**
- CREDIT

---

### 1.3. 🛡️ Garantias e Princípios do Core

#### A. **Princípios ACID**

Todas as operações atômicas respeitam:
- **Atomicidade:** Operações são completas ou completamente desfeitas
- **Consistência:** Estado sempre válido conforme regras de negócio
- **Isolamento:** Transações concorrentes não interferem entre si
- **Durabilidade:** Transações confirmadas são permanentes

#### B. **Event Sourcing Light**

Conforme premissas:
- Toda movimentação é **INSERT-only**
- Histórico completo e imutável
- Estado atual derivado de eventos passados
- Auditoria completa e inquestionável

#### C. **Diferenciação entre BLOCK e RESERVATION**

**BLOCK (Bloqueio):**
- **Uso:** Autorizações de cartão, pré-autorizações
- **Característica:** Pode expirar automaticamente por timeout
- **Cancelamento:** UNBLOCK (desbloqueio)
- **Exemplo:** Autorização de compra no cartão de crédito

**RESERVATION (Reserva):**
- **Uso:** PIX, TED, DOC, Boletos, Transferências que dependem de confirmação externa
- **Característica:** Requer confirmação explícita de sistemas externos (BACEN/SPI, SPB, câmaras)
- **Cancelamento:** CANCEL_RESERVATION (cancelamento explícito)
- **Exemplo:** PIX aguardando confirmação do SPI
- **Conformidade:** Segue regras específicas do BACEN para rastreabilidade

**Regra Geral:**
- Se a operação interage com **sistema externo regulado** (BACEN, SPI, SPB) → use **RESERVATION**
- Se a operação é **interna** ou com timeout automático → use **BLOCK**

#### D. **Validações de Autorização**

Antes de qualquer DEBIT, BLOCK ou RESERVATION:
1. Saldo disponível suficiente (via SQL Function)
2. Conta ativa e não bloqueada
3. Limites transacionais respeitados
4. Regras de horário (se aplicável)
5. Regras de compliance (AML, fraude)

#### D. **Idempotência**

Todas as operações utilizam:
- **CorrelationId** para identificação única
- Detecção de duplicatas
- Resposta consistente para requisições repetidas

---

## 2. ☁️ Camada BaaS (Banking as a Service)

A camada BaaS atua como **camada de orquestração e exposição** do Core, transformando as capacidades transacionais em produtos e serviços financeiros configuráveis, versionados e adaptáveis a diferentes clientes e modelos de negócio.

### 2.1. 🎭 Responsabilidades Principais

#### A. **Orquestração de Fluxos de Negócio**

A camada BaaS **não executa lógica financeira**, mas coordena chamadas ao Core seguindo workflows de negócio:

**Exemplo - Abertura de Conta:**
```
1. Validação de dados cadastrais (CPF, endereço)
2. Consulta a bureaus de crédito
3. Chamada ao Core: Criar entidade Client
4. Chamada ao Core: Criar entidade Account (INITIAL_BALANCE = 0)
5. Registro de documentos digitais
6. Envio de comunicação de boas-vindas
7. Registro de auditoria de compliance
```

**Exemplo - Pagamento de Boleto:**
```
1. Recebimento da requisição do canal
2. Validação da linha digitável
3. Consulta de dados do boleto (emissor, valor, vencimento)
4. Chamada ao Core: RESERVATION(valor + tarifa)
5. Envio à câmara de compensação
6. Aguarda confirmação (Async Request-Reply)
7. Callback/Webhook:
   - Sucesso: Chamada ao Core: SETTLE
   - Falha: Chamada ao Core: CANCEL_RESERVATION
8. Notificação ao cliente
9. Persistência de comprovante
```

#### B. **Adaptação de Regras Comerciais por Cliente**

A BaaS permite **customização de produtos** sem alterar o Core:

**Estratégias de Customização:**

##### 1. **Política de Tarifas**
```json
{
  "clientId": "banco-parceiro-a",
  "product": "conta-corrente",
  "fees": {
    "ted": {
      "amount": 10.00,
      "freeMonthly": 2
    },
    "pix": {
      "amount": 0.00
    },
    "boleto": {
      "amount": 3.50,
      "freeMonthly": 5
    }
  }
}
```

##### 2. **Limites Transacionais**
```json
{
  "clientId": "fintech-xyz",
  "accountType": "digital",
  "limits": {
    "dailyPixLimit": 5000.00,
    "monthlyPixLimit": 20000.00,
    "singleTransferLimit": 2000.00,
    "atmWithdrawalDaily": 1000.00
  }
}
```

##### 3. **Fluxos de Aprovação**
```json
{
  "clientId": "corporate-bank",
  "approvalRules": {
    "transfer": {
      "above": 10000.00,
      "requiresApprovers": 2,
      "approverRoles": ["manager", "director"]
    }
  }
}
```

#### C. **Versionamento de APIs**

A BaaS gerencia múltiplas versões de contratos de serviço:

```
/api/v1/payments/pix      (deprecated - mantido por SLA)
/api/v2/payments/pix      (current - features adicionais)
/api/v3/payments/pix      (beta - novos campos)
```

**Estratégias:**
- Versionamento por URL
- Versionamento por header (Accept-Version)
- Deprecation warnings
- Migração assistida para clientes

#### D. **Criação de Produtos Financeiros Compostos**

A BaaS compõe produtos complexos a partir de primitivas do Core:

**Exemplo - Conta Universitária:**
```yaml
product: conta-universitaria
features:
  - isentaTarifas: true
  - limitePix: 500.00/dia
  - cartaoDebito: true
  - cartaoCredito: false
  - investimentoAutomatico:
      enabled: true
      minBalance: 100.00
      investmentProduct: "tesouro-selic"
```

**Exemplo - Conta Empresarial Premium:**
```yaml
product: conta-empresarial-premium
features:
  - tedIlimitados: true
  - pixIlimitado: true
  - linhaCredito: 50000.00
  - gestaoMultiUsuarios: true
  - cobrancaIntegrada: true
  - conciliacaoAutomatica: true
```

#### E. **Controle de Segurança e Políticas**

##### 1. **Autenticação e Autorização**
- Integração com OAuth 2.0 / OpenID Connect
- Gerenciamento de tokens JWT
- Controle de permissões por perfil (RBAC)
- MFA (Multi-Factor Authentication)

##### 2. **Políticas de Segurança**
```json
{
  "clientId": "partner-bank",
  "securityPolicy": {
    "requireMFA": true,
    "sessionTimeout": 900,
    "ipWhitelist": ["192.168.1.0/24"],
    "apiRateLimit": {
      "requestsPerMinute": 1000,
      "burstAllowance": 100
    },
    "fraudDetection": {
      "enabled": true,
      "riskThreshold": 0.75,
      "blockOnHighRisk": true
    }
  }
}
```

##### 3. **Compliance e Regulatório**
- Registro de PLD (Prevenção à Lavagem de Dinheiro)
- Monitoramento de transações suspeitas
- Relatórios regulatórios (BACEN, CVM)
- LGPD / GDPR compliance

#### F. **Gestão de Estado e Contexto**

A BaaS mantém estado de operações complexas e assíncronas:

```json
{
  "operationId": "uuid",
  "operationType": "ted-transfer",
  "status": "awaiting-confirmation",
  "clientId": "partner-x",
  "accountId": "account-uuid",
  "context": {
    "step": "sent-to-spb",
    "reservationTransactionId": "reservation-uuid",
    "attempts": 1,
    "lastAttempt": "2025-11-21T10:30:00Z",
    "expiresAt": "2025-11-21T11:30:00Z"
  }
}
```

---

### 2.2. 🔄 Async Request-Reply Pattern

Conforme premissa, a BaaS implementa processamento assíncrono para operações de longa duração.

#### A. **Fluxo de Requisição Assíncrona**

```
Cliente → [POST /payments/ted]
    ↓
BaaS: Valida requisição
    ↓
BaaS: Persiste operação com status PENDING
    ↓
BaaS: Enfileira mensagem (RabbitMQ / Azure Service Bus)
    ↓
BaaS: Retorna 202 Accepted + operationId
    ↓
Cliente ← { "operationId": "uuid", "status": "PENDING" }
```

#### B. **Processamento em Background**

```
Worker consume mensagem da fila
    ↓
Worker: Chama Core para RESERVATION do valor
    ↓
Worker: Envia para sistema externo (SPB, câmara)
    ↓
Worker: Atualiza status para PROCESSING
    ↓
Aguarda callback/confirmação externa
    ↓
Callback recebido
    ↓
Worker: Chama Core para SETTLE ou CANCEL_RESERVATION
    ↓
Worker: Atualiza status final (COMPLETED / FAILED)
    ↓
Worker: Dispara notificação (webhook/push)
```

#### C. **Estratégias de Retorno de Status**

##### 1. **Webhook (Push Notification)**

Cliente registra URL de callback:
```json
{
  "webhookUrl": "https://cliente.com/api/notifications",
  "webhookSecret": "secret-key",
  "events": ["payment.completed", "payment.failed"]
}
```

BaaS envia notificação quando operação conclui:
```http
POST https://cliente.com/api/notifications
Content-Type: application/json
X-Webhook-Signature: sha256=...

{
  "operationId": "uuid",
  "operationType": "ted-transfer",
  "status": "COMPLETED",
  "timestamp": "2025-11-21T10:35:00Z",
  "details": { ... }
}
```

##### 2. **Polling (Cliente consulta status)**

Cliente consulta periodicamente:
```http
GET /operations/{operationId}/status

Response:
{
  "operationId": "uuid",
  "status": "COMPLETED",
  "completedAt": "2025-11-21T10:35:00Z",
  "result": { ... }
}
```

**Estratégia de Polling Eficiente:**
- Exponential backoff (1s, 2s, 4s, 8s, ...)
- Status PENDING retorna header `Retry-After: 5`
- Status final (COMPLETED/FAILED) não precisa mais polling

---

### 2.3. 🏗️ Componentes da Camada BaaS

#### A. **API Gateway (BaaS Layer)**
- Roteamento de requisições
- Versionamento de APIs
- Rate limiting por cliente
- Transformação de payloads

#### B. **Orquestrador de Workflows**
- Implementação de sagas
- Compensação de transações
- Retry policies
- Timeout management

#### C. **Gerenciador de Produtos**
- Catálogo de produtos financeiros
- Configuração de regras por cliente
- Templates de produtos

#### D. **Gerenciador de Políticas**
- Tarifas e preços
- Limites transacionais
- Regras de aprovação
- Políticas de compliance

#### E. **Gerenciador de Estado**
- Persistência de contexto operacional
- Rastreamento de operações assíncronas
- Histórico de mudanças de estado

#### F. **Notificador**
- Envio de webhooks
- Push notifications
- Email / SMS
- Gerenciamento de retry para falhas

---

### 2.4. 🗄️ Persistência na Camada BaaS

A BaaS possui seus próprios bancos de dados, **separados do Core**, para:

#### A. **Dados de Produto e Configuração**
```
ProductConfigurations
  - ClientId
  - ProductType
  - Fees (JSON)
  - Limits (JSON)
  - Features (JSON)

ClientPolicies
  - ClientId
  - SecurityPolicy (JSON)
  - ComplianceRules (JSON)
```

#### B. **Operações Assíncronas**
```
AsyncOperations
  - OperationId (PK)
  - ClientId
  - OperationType
  - Status (PENDING, PROCESSING, COMPLETED, FAILED)
  - Context (JSON)
  - CreatedAt
  - UpdatedAt
  - CompletedAt
```

#### C. **Webhooks e Notificações**
```
WebhookConfigurations
  - ClientId
  - WebhookUrl
  - Secret
  - Events (array)

WebhookDeliveries
  - DeliveryId (PK)
  - OperationId
  - Attempt
  - Status
  - ResponseCode
  - SentAt
```

---

## 3. 🌐 Camada de Gateways, API Gateway e BFF

A camada mais externa da arquitetura BIRO, responsável por adaptar e expor os serviços para múltiplos canais de atendimento, dispositivos e protocolos.

### 3.1. 🎯 Responsabilidades Principais

#### A. **Adaptação de Protocolos**

Cada canal pode utilizar protocolos diferentes:

- **HTTP/REST** → Mobile, Web, APIs públicas
- **ISO 8583** → ATMs, autorizadores de cartão
- **WebSocket** → Chat, notificações real-time
- **gRPC** → Comunicação interna de alta performance
- **GraphQL** → Front-ends com requisitos flexíveis
- **SOAP** → Integrações legadas

#### B. **Backend for Frontend (BFF)**

Cada canal possui um BFF específico que agrega e formata dados conforme necessidade do cliente:

##### 1. **Mobile BFF**
```json
GET /mobile/v1/home-dashboard

Response:
{
  "balance": {
    "available": 4200.00,
    "blocked": 800.00
  },
  "recentTransactions": [
    { "type": "pix", "amount": -50.00, "description": "Lanchonete", "timestamp": "..." },
    { "type": "credit", "amount": 1000.00, "description": "Salário", "timestamp": "..." }
  ],
  "notifications": [
    { "type": "info", "message": "Nova funcionalidade: Investimentos disponíveis!" }
  ],
  "quickActions": ["pix", "pay-bill", "transfer"]
}
```

**Características:**
- Payload otimizado (dados mínimos)
- Agregação de múltiplas chamadas
- Cache agressivo

##### 2. **Web Banking BFF**
```json
GET /web/v1/dashboard

Response:
{
  "accounts": [
    { "id": "...", "type": "checking", "balance": 4200.00, "accountNumber": "12345-6" },
    { "id": "...", "type": "savings", "balance": 10000.00, "accountNumber": "98765-4" }
  ],
  "investments": {
    "totalInvested": 50000.00,
    "totalEarnings": 2500.00,
    "positions": [ ... ]
  },
  "creditCards": [ ... ],
  "recentActivity": [ ... ],
  "charts": {
    "spending": { "categories": [ ... ] },
    "income": { "monthly": [ ... ] }
  }
}
```

**Características:**
- Mais dados por requisição
- Visualizações e gráficos
- Dashboard complexo

##### 3. **ATM Gateway (ISO 8583)**

Traduz mensagens ISO 8583 para chamadas REST ao BaaS:

**Mensagem ISO 8583 Recebida:**
```
MTI: 0200 (Financial Transaction Request)
Field 2: Primary Account Number
Field 3: Processing Code (000000 = Balance Inquiry)
Field 4: Amount
Field 7: Transmission Date/Time
Field 11: System Trace Audit Number
...
```

**Tradução para REST:**
```http
POST /baas/v1/atm/balance-inquiry
{
  "cardNumber": "encrypted",
  "timestamp": "2025-11-21T10:30:00Z",
  "terminalId": "ATM-001",
  "networkId": "TECBAN",
  "traceNumber": "123456"
}
```

**Resposta REST → ISO 8583:**
```
MTI: 0210 (Financial Transaction Response)
Field 39: Response Code (00 = Approved)
Field 54: Available Balance
```

##### 4. **URA (Unidade de Resposta Audível)**

**Fluxo de Integração:**
```
Cliente liga → Sistema telefônico → URA
    ↓
URA: Identifica cliente (via telefone ou digitação de conta)
    ↓
URA chama API: GET /ura/v1/authenticate
    ↓
Cliente escolhe opção → "Consultar saldo"
    ↓
URA chama API: GET /ura/v1/balance
    ↓
URA: Sintetiza voz com o saldo
```

**API específica para URA:**
```json
GET /ura/v1/balance?accountId=uuid

Response:
{
  "balance": {
    "available": 4200.00,
    "availableText": "quatro mil e duzentos reais"
  }
}
```

##### 5. **Chatbot BFF**

**Características:**
- NLP (Natural Language Processing) para interpretar intenções
- Contexto conversacional
- Integração com IA generativa

**Exemplo de Interação:**
```
Usuário: "Quanto tenho de saldo?"
    ↓
Chatbot: Identifica intenção = BALANCE_INQUIRY
    ↓
Chatbot chama: GET /chatbot/v1/balance
    ↓
Chatbot responde: "Seu saldo disponível é R$ 4.200,00 😊"

Usuário: "Quero fazer um PIX de 100 reais pro João"
    ↓
Chatbot: Identifica intenção = PIX_PAYMENT
    ↓
Chatbot: "Encontrei 3 contatos com o nome João. Qual deles?"
    ↓
[usuário seleciona]
    ↓
Chatbot chama: POST /chatbot/v1/pix/initiate
    ↓
Chatbot: "Confirme o PIX de R$ 100,00 para João Silva (CPF ***123.456-**)"
```

#### C. **Segurança e Autenticação**

A camada de Gateway gerencia:

##### 1. **Autenticação Multi-Canal**

- **Mobile:** OAuth 2.0 + Biometria
- **Web:** OAuth 2.0 + 2FA
- **ATM:** PIN + Chip
- **API Externa:** API Keys + OAuth Client Credentials

##### 2. **API Gateway Security**

```
Requisição → API Gateway
    ↓
Validação de token JWT
    ↓
Rate Limiting (por cliente/IP)
    ↓
Verificação de IP Whitelist (se aplicável)
    ↓
Verificação de assinatura de requisição
    ↓
Proxy para BFF/BaaS
```

##### 3. **Encriptação de Dados Sensíveis**

- Dados de cartão: Tokenização (PCI-DSS)
- Senhas: Hash com bcrypt/argon2
- PINs: HSM (Hardware Security Module)
- Dados em trânsito: TLS 1.3
- Dados em repouso: Encryption at rest

#### D. **Composição de Respostas**

O Gateway/BFF pode agregar dados de múltiplas fontes:

**Exemplo - Tela de Home do Mobile:**
```
GET /mobile/v1/home
    ↓
BFF chama em paralelo:
  - GET /baas/v1/accounts/{id}/balance
  - GET /baas/v1/accounts/{id}/transactions?limit=5
  - GET /baas/v1/notifications?userId={id}&unread=true
  - GET /baas/v1/cards/{id}/summary
    ↓
BFF agrega respostas
    ↓
BFF formata payload otimizado para mobile
    ↓
Retorna resposta única
```

**Vantagens:**
- Uma única requisição do cliente
- Redução de latência
- Menor consumo de bateria (mobile)

#### E. **Formatação de Payloads**

O BFF adapta a resposta ao formato esperado pelo canal:

**Mesma operação, formatos diferentes:**

**Mobile (JSON compacto):**
```json
{
  "bal": 4200.00,
  "txs": [
    {"t": "pix", "a": -50.00, "d": "Lanche", "ts": 1637501234}
  ]
}
```

**Web (JSON verboso):**
```json
{
  "availableBalance": 4200.00,
  "currency": "BRL",
  "transactions": [
    {
      "transactionId": "uuid",
      "type": "pix-payment",
      "amount": -50.00,
      "description": "Pagamento PIX - Lanchonete",
      "timestamp": "2025-11-21T10:30:00Z",
      "status": "completed"
    }
  ]
}
```

**ATM (ISO 8583):**
```
Campo 54: 000000420000 (balance in cents)
```

---

### 3.2. 🏗️ Componentes da Camada de Gateway

#### A. **API Gateway (Camada Externa)**

**Tecnologias:** Kong, AWS API Gateway, Azure API Management, NGINX

**Responsabilidades:**
- Rate limiting global
- Autenticação inicial
- Roteamento para BFFs
- Métricas e logs centralizados
- WAF (Web Application Firewall)

**Configuração Exemplo:**
```yaml
routes:
  - name: mobile-bff
    paths: ["/mobile/*"]
    plugins:
      - name: rate-limiting
        config:
          minute: 100
      - name: jwt
        config:
          key_claim_name: kid
  
  - name: atm-gateway
    paths: ["/atm/*"]
    plugins:
      - name: ip-restriction
        config:
          whitelist: ["10.0.0.0/8"]
```

#### B. **Mobile BFF**

**Stack:** Node.js / .NET Core
**Responsabilidades:**
- Agregação de chamadas
- Formatação de payloads mobile
- Gerenciamento de push notifications
- Cache de dados frequentes

#### C. **Web BFF**

**Stack:** Node.js / .NET Core / Java
**Responsabilidades:**
- Composição de dashboards
- Server-side rendering (se aplicável)
- Gerenciamento de sessão web

#### D. **ISO 8583 Gateway (ATM/POS)**

**Stack:** Java / C++ (alta performance)
**Responsabilidades:**
- Parsing de mensagens ISO 8583
- Tradução para REST/gRPC
- Gerenciamento de conexões persistentes
- HSM integration para criptografia de PINs

**Arquitetura:**
```
ATM → ISO 8583 Message → Gateway
    ↓
Parse MTI (Message Type Indicator)
    ↓
Route para handler específico:
  - 0200 (Financial Request) → TransactionHandler
  - 0100 (Authorization Request) → AuthorizationHandler
  - 0400 (Reversal) → ReversalHandler
    ↓
Converter para Request DTO
    ↓
Chamar BaaS via gRPC (alta performance)
    ↓
Converter Response para ISO 8583
    ↓
Retornar ao ATM
```

#### E. **Chatbot Gateway**

**Stack:** Python (NLP libraries) + Node.js
**Responsabilidades:**
- Processamento de linguagem natural
- Manutenção de contexto conversacional
- Integração com LLMs (GPT, BERT, etc.)
- Tradução de intenções para chamadas de API

**Exemplo de Mapeamento de Intenções:**
```python
intents = {
    "balance_inquiry": lambda ctx: api_client.get_balance(ctx.account_id),
    "pix_payment": lambda ctx: api_client.initiate_pix(ctx.account_id, ctx.amount, ctx.recipient),
    "statement": lambda ctx: api_client.get_statement(ctx.account_id, ctx.start_date, ctx.end_date)
}
```

#### F. **Partner API Gateway**

**Responsabilidades:**
- Exposição de APIs para parceiros externos
- Gerenciamento de API keys
- Throttling por parceiro
- SLA monitoring
- Billing por uso (se aplicável)

---

### 3.3. 🔒 Segurança em Camadas

#### Layer 1: **Perímetro Externo**
- WAF (Web Application Firewall)
- DDoS protection
- SSL/TLS termination

#### Layer 2: **API Gateway**
- Autenticação OAuth 2.0
- Rate limiting
- IP whitelisting

#### Layer 3: **BFF**
- Validação de tokens
- Autorização por perfil
- Input sanitization

#### Layer 4: **BaaS**
- Verificação de permissões
- Auditoria de operações sensíveis
- Fraud detection

#### Layer 5: **Core**
- Validação final de regras de negócio
- Log de auditoria imutável

---

### 3.4. 📊 Observabilidade

#### A. **Distributed Tracing**
```
Request ID: abc123
  ↓
API Gateway [10ms]
  ↓
Mobile BFF [50ms]
    ├→ GET Balance [20ms]
    └→ GET Transactions [30ms]
  ↓
BaaS [200ms]
    └→ Core [150ms]
```

**Ferramentas:** OpenTelemetry, Jaeger, Zipkin

#### B. **Logging Centralizado**

Todos os componentes enviam logs para agregador central:

```json
{
  "timestamp": "2025-11-21T10:30:00Z",
  "level": "INFO",
  "service": "mobile-bff",
  "requestId": "abc123",
  "userId": "user-uuid",
  "action": "balance-inquiry",
  "duration": 50,
  "status": "success"
}
```

**Ferramentas:** ELK Stack, Splunk, Azure Monitor

#### C. **Métricas**

- **Taxa de requisições** (requests/second)
- **Latência** (p50, p95, p99)
- **Taxa de erro** (erros/total)
- **Disponibilidade** (uptime %)

**Ferramentas:** Prometheus + Grafana

---

## 4. 🗄️ Arquitetura de Dados (Data Architecture)

A arquitetura de dados do BIRO é **estratificada por camada**, garantindo isolamento, performance e adequação ao propósito de cada componente.

### 4.1. 💎 Camada Core - Modelo Transacional

#### A. **Princípios de Design**

##### 1. **Event Sourcing Light**
Conforme premissa, o modelo de dados é baseado em **imutabilidade**:

- ✅ **INSERT-only** para movimentações
- ❌ **Evitar UPDATE e DELETE** de transações
- ✅ Histórico completo e auditável
- ✅ Performance otimizada (operações mais baratas)

##### 2. **Normalização Moderada**
- 3FN (Third Normal Form) para entidades mestres
- Desnormalização seletiva para performance de consultas críticas

##### 3. **Particionamento**
- Particionamento horizontal por `AccountId` (shard key)
- Particionamento temporal para transações antigas

#### B. **Modelo de Dados - Core**

##### Entidade: **Client (Cliente)**
```sql
CREATE TABLE Clients (
    ClientId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TaxId VARCHAR(20) NOT NULL UNIQUE, -- CPF/CNPJ
    FullName NVARCHAR(200) NOT NULL,
    Email VARCHAR(100),
    Phone VARCHAR(20),
    DateOfBirth DATE,
    Status VARCHAR(20) DEFAULT 'ACTIVE', -- ACTIVE, SUSPENDED, CLOSED
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    
    INDEX IX_Client_TaxId (TaxId),
    INDEX IX_Client_Status (Status)
);
```

##### Entidade: **Account (Conta)**
```sql
CREATE TABLE Accounts (
    AccountId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ClientId UNIQUEIDENTIFIER NOT NULL,
    ProductType VARCHAR(50) NOT NULL, -- CHECKING_ACCOUNT, SAVINGS_ACCOUNT
    BranchCode VARCHAR(10) NOT NULL,
    AccountNumber VARCHAR(20) NOT NULL,
    Status VARCHAR(20) DEFAULT 'ACTIVE', -- ACTIVE, BLOCKED, CLOSED
    OpenedAt DATETIME2 DEFAULT GETUTCDATE(),
    ClosedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Account_Client FOREIGN KEY (ClientId) 
        REFERENCES Clients(ClientId),
    
    CONSTRAINT UQ_Account_Branch_Number 
        UNIQUE (BranchCode, AccountNumber),
    
    INDEX IX_Account_Client (ClientId),
    INDEX IX_Account_Status (Status)
);
```

##### Entidade: **Transaction (Movimentação)**

**Tabela Principal (Event Log):**
```sql
CREATE TABLE Transactions (
    TransactionId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    AccountId UNIQUEIDENTIFIER NOT NULL,
    TransactionType VARCHAR(20) NOT NULL, 
        -- DEBIT, CREDIT, BLOCK, RESERVATION, INITIAL_BALANCE
    Amount DECIMAL(18,2) NOT NULL CHECK (Amount >= 0),
    Timestamp DATETIME2 DEFAULT GETUTCDATE(),
    CorrelationId UNIQUEIDENTIFIER NOT NULL, -- Para idempotência
    Status VARCHAR(20) DEFAULT 'ACTIVE', 
        -- ACTIVE, SETTLED, CANCELLED, EXPIRED
    ExpirationDateTime DATETIME2 NULL, -- Para BLOCK e RESERVATION
    Metadata NVARCHAR(MAX), -- JSON com dados adicionais
    
    CONSTRAINT FK_Transaction_Account FOREIGN KEY (AccountId)
        REFERENCES Accounts(AccountId),
    
    -- Índices otimizados para consultas frequentes
    INDEX IX_Transaction_Account_Timestamp (AccountId, Timestamp DESC),
    INDEX IX_Transaction_CorrelationId (CorrelationId),
    INDEX IX_Transaction_Status (Status, ExpirationDateTime) 
        WHERE TransactionType IN ('BLOCK', 'RESERVATION')
) WITH (DATA_COMPRESSION = PAGE);

-- Particionamento temporal (exemplo)
-- Particionar por RANGE (Timestamp) em partições mensais
```

**Observações Importantes:**
- **Sem coluna de saldo calculado** → Saldo é derivado via função
- **Imutabilidade:** Status pode mudar (ACTIVE → SETTLED), mas Amount e TransactionType nunca mudam
- **Compression:** DATA_COMPRESSION = PAGE para economizar espaço

##### Função: **Cálculo de Saldo**

Conforme premissa, o saldo é calculado via **SQL Function** (não Stored Procedure):

```sql
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

-- Uso:
-- SELECT dbo.fn_GetAvailableBalance('account-uuid') AS AvailableBalance;
```

**Função para Saldo Contábil:**
```sql
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
```

#### C. **Estratégias de Performance**

##### 1. **Índices Especializados**
```sql
-- Índice para consulta de saldo (leitura hot path)
CREATE NONCLUSTERED INDEX IX_Transaction_Balance_Calc
ON Transactions (AccountId, TransactionType, Status)
INCLUDE (Amount)
WITH (FILLFACTOR = 90);

-- Índice para extratos por período
CREATE NONCLUSTERED INDEX IX_Transaction_Statement
ON Transactions (AccountId, Timestamp DESC)
INCLUDE (TransactionType, Amount, CorrelationId, Metadata);
```

##### 2. **Particionamento Horizontal (Sharding)**

Para escalabilidade, particionar por `AccountId`:

```
Shard 1: Accounts onde AccountId hash % 10 = 0
Shard 2: Accounts onde AccountId hash % 10 = 1
...
Shard 10: Accounts onde AccountId hash % 10 = 9
```

**Vantagens:**
- Distribuição de carga
- Paralelização de queries
- Isolamento de falhas

##### 3. **Particionamento Temporal (Archive)**

Transações antigas (> 2 anos) movidas para tabela de arquivo:

```sql
CREATE TABLE Transactions_Archive (
    -- Mesma estrutura de Transactions
    ...
) WITH (DATA_COMPRESSION = PAGE);

-- Mover mensalmente via job
INSERT INTO Transactions_Archive
SELECT * FROM Transactions
WHERE Timestamp < DATEADD(YEAR, -2, GETUTCDATE());
```

##### 4. **Caching de Saldo**

Para contas com alto volume transacional, manter cache de saldo:

```sql
CREATE TABLE BalanceCache (
    AccountId UNIQUEIDENTIFIER PRIMARY KEY,
    CachedBalance DECIMAL(18,2),
    LastTransactionId UNIQUEIDENTIFIER,
    CachedAt DATETIME2,
    
    INDEX IX_BalanceCache_UpdatedAt (CachedAt)
);

-- Atualizar cache após cada transação (via trigger ou app logic)
```

**Estratégia:**
- Cache hit: Retornar saldo direto
- Cache miss: Calcular via função + popular cache

#### D. **Replicação e Alta Disponibilidade**

##### 1. **Replicação Síncrona (Primary → Secondary)**
- Garantia de consistência
- Failover automático
- RPO = 0 (Recovery Point Objective)

##### 2. **Read Replicas**
- Para consultas de extrato e saldo
- Reduz carga no master
- Eventually consistent (lag < 1s)

##### 3. **Backup e Disaster Recovery**
- Backup completo diário
- Backup incremental a cada 15 minutos
- Point-in-time recovery
- Replicação geográfica (multi-region)

---

### 4.2. ☁️ Camada BaaS - Bancos Orientados a Serviços

A camada BaaS possui seus próprios bancos de dados para armazenar:

#### A. **Configurações de Produto e Cliente**

**Banco:** PostgreSQL / SQL Server (relacional)

```sql
CREATE TABLE ProductConfigurations (
    ConfigId UNIQUEIDENTIFIER PRIMARY KEY,
    ClientId UNIQUEIDENTIFIER NOT NULL,
    ProductType VARCHAR(50) NOT NULL,
    ConfigurationJson NVARCHAR(MAX) NOT NULL, -- JSON com regras
    EffectiveFrom DATE NOT NULL,
    EffectiveTo DATE NULL,
    Version INT NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    
    INDEX IX_ProductConfig_Client_Product (ClientId, ProductType),
    INDEX IX_ProductConfig_Version (ClientId, ProductType, Version DESC)
);
```

**Exemplo de ConfigurationJson:**
```json
{
  "fees": {
    "pix": 0.00,
    "ted": 10.00,
    "boleto": 3.50
  },
  "limits": {
    "dailyPixLimit": 5000.00,
    "monthlyPixLimit": 20000.00
  },
  "features": {
    "investmentsEnabled": true,
    "loansEnabled": false
  }
}
```

#### B. **Operações Assíncronas**

**Banco:** MongoDB (NoSQL - flexibilidade de schema)

```javascript
{
  _id: ObjectId("..."),
  operationId: "uuid",
  operationType: "ted-transfer",
  clientId: "partner-x",
  accountId: "account-uuid",
  status: "processing", // pending, processing, completed, failed
  context: {
    step: "sent-to-spb",
    reservationTransactionId: "reservation-uuid",
    externalReference: "spb-123456",
    attempts: 1,
    lastAttemptAt: ISODate("2025-11-21T10:30:00Z")
  },
  createdAt: ISODate("2025-11-21T10:25:00Z"),
  updatedAt: ISODate("2025-11-21T10:30:00Z"),
  completedAt: null,
  expiresAt: ISODate("2025-11-21T11:30:00Z")
}
```

#### C. **Webhooks e Notificações**

**Banco:** PostgreSQL (transacional)

```sql
CREATE TABLE WebhookConfigurations (
    ConfigId UNIQUEIDENTIFIER PRIMARY KEY,
    ClientId UNIQUEIDENTIFIER NOT NULL,
    WebhookUrl VARCHAR(500) NOT NULL,
    Secret VARCHAR(100) NOT NULL, -- Para assinatura HMAC
    Events NVARCHAR(MAX) NOT NULL, -- JSON array
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);

CREATE TABLE WebhookDeliveries (
    DeliveryId UNIQUEIDENTIFIER PRIMARY KEY,
    OperationId UNIQUEIDENTIFIER NOT NULL,
    ConfigId UNIQUEIDENTIFIER NOT NULL,
    Attempt INT NOT NULL,
    Status VARCHAR(20) NOT NULL, -- PENDING, SENT, FAILED, DELIVERED
    HttpStatusCode INT NULL,
    RequestBody NVARCHAR(MAX),
    ResponseBody NVARCHAR(MAX),
    SentAt DATETIME2 NULL,
    
    INDEX IX_WebhookDelivery_Operation (OperationId),
    INDEX IX_WebhookDelivery_Status (Status, SentAt)
);
```

#### D. **Auditoria e Compliance**

**Banco:** PostgreSQL + Elasticsearch (para busca)

```sql
CREATE TABLE AuditLog (
    AuditId UNIQUEIDENTIFIER PRIMARY KEY,
    Timestamp DATETIME2 DEFAULT GETUTCDATE(),
    UserId UNIQUEIDENTIFIER,
    ClientId UNIQUEIDENTIFIER,
    Action VARCHAR(100) NOT NULL,
    Resource VARCHAR(200),
    Changes NVARCHAR(MAX), -- JSON before/after
    IpAddress VARCHAR(50),
    UserAgent VARCHAR(500),
    
    INDEX IX_Audit_Timestamp (Timestamp DESC),
    INDEX IX_Audit_User (UserId, Timestamp DESC),
    INDEX IX_Audit_Client (ClientId, Timestamp DESC)
) WITH (DATA_COMPRESSION = PAGE);
```

**Sincronização com Elasticsearch:**
```javascript
// Document no Elasticsearch
{
  "auditId": "uuid",
  "timestamp": "2025-11-21T10:30:00Z",
  "userId": "user-uuid",
  "action": "transfer.executed",
  "resource": "account/uuid",
  "metadata": {
    "amount": 1000.00,
    "destination": "account-xyz"
  }
}

// Query exemplo: buscar todas as transferências acima de R$ 10.000 no último mês
```

---

### 4.3. 🌐 Camada de Gateway/BFF - Front-end e Cache

#### A. **Gerenciamento de Sessão**

**Banco:** Redis (in-memory, alta performance)

```
Key: session:{userId}:{sessionId}
Value: {
  "userId": "uuid",
  "accountIds": ["uuid1", "uuid2"],
  "roles": ["account_holder"],
  "expiresAt": 1637588800,
  "deviceInfo": { ... }
}
TTL: 900 segundos (15 minutos)
```

#### B. **Cache de Dados Frequentes**

**Banco:** Redis

```
-- Cache de saldo (TTL curto)
Key: balance:{accountId}
Value: {"available": 4200.00, "blocked": 800.00}
TTL: 30 segundos

-- Cache de configurações (TTL longo)
Key: config:{clientId}:{productType}
Value: { "fees": {...}, "limits": {...} }
TTL: 3600 segundos (1 hora)

-- Cache de taxa de câmbio
Key: exchange_rate:USD:BRL
Value: 5.45
TTL: 300 segundos (5 minutos)
```

**Estratégia de Invalidação:**
- TTL automático
- Invalidação ativa após transação (cache-aside pattern)

#### C. **Tokens e Autenticação**

**Banco:** Redis

```
Key: token:{tokenId}
Value: {
  "userId": "uuid",
  "scopes": ["read:balance", "write:payment"],
  "issuedAt": 1637588800,
  "expiresAt": 1637592400
}
TTL: até expiração do token
```

#### D. **Fila de Notificações Push**

**Banco:** Redis (como fila)

```
List Key: push_notifications:pending

Items:
{
  "userId": "uuid",
  "title": "Pagamento aprovado",
  "body": "Seu PIX de R$ 100,00 foi enviado com sucesso",
  "data": { "transactionId": "uuid" },
  "priority": "high"
}
```

#### E. **Dados de Personalização**

**Banco:** MongoDB

```javascript
{
  _id: ObjectId("..."),
  userId: "uuid",
  preferences: {
    theme: "dark",
    language: "pt-BR",
    notifications: {
      push: true,
      email: true,
      sms: false
    }
  },
  favoriteContacts: [
    { name: "João", pixKey: "joao@email.com" },
    { name: "Maria", pixKey: "+5511999999999" }
  ],
  customDashboard: {
    widgets: ["balance", "recent_transactions", "investments"]
  }
}
```

---

### 4.4. 🔐 Trilhas Financeiras e Auditoria

#### A. **Rastreabilidade Completa (Audit Trail)**

Toda operação financeira possui trail completo:

```
TransactionId (Core)
    ↓
OperationId (BaaS)
    ↓
RequestId (Gateway/BFF)
    ↓
UserId, DeviceId, IP, Timestamp
```

**Query de Auditoria Completa:**
```sql
-- Rastrear toda a jornada de uma transação
SELECT 
    c.TransactionId,
    c.TransactionType,
    c.Amount,
    c.Timestamp AS CoreTimestamp,
    b.OperationId,
    b.Status AS OperationStatus,
    a.Action,
    a.IpAddress,
    a.UserAgent
FROM Transactions c
INNER JOIN AsyncOperations b ON c.CorrelationId = b.OperationId
INNER JOIN AuditLog a ON b.OperationId = a.Resource
WHERE c.TransactionId = @TransactionId;
```

#### B. **Compliance Regulatório**

##### 1. **Relatório BACEN (PLD)**

View materializada para relatórios:

```sql
CREATE VIEW vw_TransactionsForPLD AS
SELECT 
    t.TransactionId,
    t.AccountId,
    a.ClientId,
    c.TaxId,
    c.FullName,
    t.TransactionType,
    t.Amount,
    t.Timestamp,
    CASE 
        WHEN t.Amount >= 10000 THEN 'REPORTABLE'
        ELSE 'NORMAL'
    END AS PLDStatus
FROM Transactions t
INNER JOIN Accounts a ON t.AccountId = a.AccountId
INNER JOIN Clients c ON a.ClientId = c.ClientId
WHERE t.TransactionType IN ('DEBIT', 'CREDIT');
```

##### 2. **LGPD / GDPR Compliance**

- Dados pessoais criptografados
- Log de acesso a dados sensíveis
- Capacidade de anonimização/deleção

```sql
CREATE TABLE DataAccessLog (
    AccessId UNIQUEIDENTIFIER PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    DataSubjectId UNIQUEIDENTIFIER NOT NULL, -- ClientId
    AccessReason VARCHAR(200) NOT NULL,
    AccessedData NVARCHAR(MAX), -- Campos acessados
    Timestamp DATETIME2 DEFAULT GETUTCDATE()
);
```

---

### 4.5. 📊 Estratégias de Particionamento e Escalabilidade

#### A. **Sharding Strategy (Particionamento Horizontal)**

**Critério de Sharding:** `AccountId`

```
Shard Key = Hash(AccountId) % N_Shards

Exemplo com 10 shards:
- AccountId: abc-123 → Hash: 1234567890 → 1234567890 % 10 = 0 → Shard 0
- AccountId: def-456 → Hash: 9876543210 → 9876543210 % 10 = 0 → Shard 0
- AccountId: ghi-789 → Hash: 5555555555 → 5555555555 % 10 = 5 → Shard 5
```

**Mapeamento:**
```
Account Range              Shard
---------------------------------
Hash % 10 = 0       →     Shard-0
Hash % 10 = 1       →     Shard-1
...
Hash % 10 = 9       →     Shard-9
```

**Arquitetura:**
```
Application → Shard Router (identifica shard pelo AccountId)
    ↓
Shard 0 [DB Instance 1]
Shard 1 [DB Instance 2]
Shard 2 [DB Instance 3]
...
```

#### B. **Replicação por Shard**

Cada shard possui réplicas:

```
Shard 0:
  - Primary (writes)
  - Secondary 1 (reads)
  - Secondary 2 (reads)

Shard 1:
  - Primary (writes)
  - Secondary 1 (reads)
  - Secondary 2 (reads)
```

#### C. **CQRS (Command Query Responsibility Segregation)**

**Write Model (Commands):**
- Vai para Primary
- Transações ACID completas

**Read Model (Queries):**
- Vai para Secondaries
- Dados de leitura otimizados (pode incluir desnormalização)
- Eventually consistent

**Exemplo:**
```
Write: POST /transactions/debit → Primary do Shard X
Read: GET /accounts/{id}/balance → Secondary do Shard X (ou cache)
```

---

## 5. 🎛️ Considerações Finais de Arquitetura

### 5.1. 🚀 Escalabilidade

**Horizontal Scaling:**
- Core: Sharding por AccountId
- BaaS: Múltiplas instâncias (stateless)
- Gateway/BFF: Load balancer + múltiplas instâncias

**Vertical Scaling:**
- Databases: Aumentar recursos conforme necessário
- Funções SQL otimizadas para performance

### 5.2. 🔒 Segurança

**Camadas de Segurança:**
1. Network: Firewall, VPN, Private Networks
2. Application: OAuth, JWT, API Keys
3. Data: Encryption at rest, TLS in transit
4. Audit: Logs imutáveis, compliance

### 5.3. 🏗️ Arquitetura SEDA (Staged Event-Driven Architecture)

Conforme premissa, o Core adota princípios SEDA:

**Stages (Estágios):**
```
Request → Validation Stage → Authorization Stage → Execution Stage → Notification Stage
```

Cada estágio:
- Possui fila própria
- Processa de forma assíncrona
- Pode escalar independentemente
- Comunica via eventos

**Vantagens:**
- Desacoplamento total
- Escalabilidade granular
- Resiliência a falhas
- Backpressure natural

### 5.4. 🔄 Resiliência e Recuperação

**Circuit Breaker:**
- Detecta falhas em serviços externos
- Abre circuito temporariamente
- Evita cascata de falhas

**Retry com Backoff:**
- Tentativas automáticas com atraso exponencial
- Jitter para evitar thundering herd

**Compensating Transactions:**
- Saga pattern para operações distribuídas
- Reversão em caso de falha parcial

### 5.5. 📈 Observabilidade e Monitoramento

**Métricas Chave:**
- Transactions per second (TPS)
- Latência (p50, p95, p99)
- Taxa de erro
- Disponibilidade (SLA)

**Alertas:**
- Latência > threshold
- Taxa de erro > 1%
- Saldo negativo detectado (anomalia)
- Falha em replicação

**Dashboards:**
- Operacional: Saúde dos sistemas
- Negócio: Volume transacional, receita
- Segurança: Tentativas de fraude, acessos suspeitos

### 5.6. 🎯 Melhores Práticas: BLOCK vs RESERVATION

Para garantir conformidade regulatória e consistência arquitetural, siga estas diretrizes:

#### Quando usar BLOCK:
✅ Autorizações de cartão (crédito/débito)
✅ Pré-autorizações em estabelecimentos
✅ Reservas de limite temporário
✅ Operações com timeout automático
✅ Validações internas que não dependem de sistemas externos regulados

#### Quando usar RESERVATION:
✅ **PIX** (todas as modalidades) - **OBRIGATÓRIO por conformidade BACEN/SPI**
✅ **TED/DOC** - transferências via SPB
✅ **Boletos** - pagamentos via câmaras de compensação
✅ **Transferências internacionais** - SWIFT/correspondentes
✅ **Investimentos** - aplicações que dependem de custódia externa
✅ Qualquer operação que depende de confirmação de sistema externo regulado

#### Princípio Geral:
```
Se (operação interage com BACEN/SPI/SPB/Câmaras) {
    usar RESERVATION + SETTLE/CANCEL_RESERVATION
} senão se (operação interna ou com timeout) {
    usar BLOCK + SETTLE/UNBLOCK
}
```

Esta diferenciação garante:
- ✅ Conformidade com regulamentações do BACEN
- ✅ Auditoria adequada para órgãos reguladores
- ✅ Rastreabilidade completa de operações financeiras
- ✅ Separação clara de responsabilidades

---

## 6. 🗺️ Diagrama Conceitual da Arquitetura

```
┌─────────────────────────────────────────────────────────────────┐
│                    EXTERNAL CHANNELS                             │
│  [Mobile App] [Web Browser] [ATM] [Chatbot] [Partner APIs]     │
└────────────┬────────────────────────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────────────────┐
│              API GATEWAY / BFF LAYER                            │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐          │
│  │ Mobile   │ │   Web    │ │ ISO 8583 │ │ Chatbot  │          │
│  │   BFF    │ │   BFF    │ │ Gateway  │ │   BFF    │          │
│  └──────────┘ └──────────┘ └──────────┘ └──────────┘          │
│                                                                  │
│  [Redis Cache] [Session Management] [Rate Limiting]            │
└────────────┬────────────────────────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────────────────┐
│                   BaaS LAYER                                    │
│  ┌────────────────────────────────────────────────────────┐    │
│  │  Product Manager │ Policy Manager │ Workflow Orchestrator   │
│  └────────────────────────────────────────────────────────┘    │
│                                                                  │
│  [Async Operations DB] [Config DB] [Webhook Manager]           │
│                                                                  │
│  Responsibilities:                                              │
│   • Orchestrate Core calls                                      │
│   • Apply business rules per client                             │
│   • Manage async request-reply pattern                          │
│   • Version APIs                                                │
└────────────┬────────────────────────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────────────────┐
│              CORE - AUTHORIZATION & TRANSACTIONS                 │
│                                                                  │
│  ┌───────────────── ATOMIC TRANSACTIONS ──────────────────┐    │
│  │  DEBIT │ CREDIT │ TRANSFER │ BLOCK │ UNBLOCK │        │    │
│  │  RESERVATION │ SETTLE │ CANCEL │ INITIAL_BALANCE │     │    │
│  └──────────────────────────────────────────────────────────┘    │
│                                                                  │
│  ┌───────────── BUSINESS TRANSACTIONS ────────────────┐        │
│  │  PIX │ Payments │ Statements │ Cards │ Loans │     │        │
│  │  Investments │ TED │ Withdrawals │ Deposits │      │        │
│  └──────────────────────────────────────────────────────┘        │
│                                                                  │
│  [Transaction Log - Immutable] [SQL Functions for Balance]     │
└────────────┬────────────────────────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────────────────┐
│                    DATA LAYER                                   │
│                                                                  │
│  CORE:                                                          │
│  ┌──────────────────────────────────────────────────────┐      │
│  │ [SQL Server] Transactions (Event Sourcing Light)     │      │
│  │              Clients, Accounts                        │      │
│  │ [Partitioning] By AccountId (Sharding)               │      │
│  │ [Replication] Primary → Secondaries                   │      │
│  └──────────────────────────────────────────────────────┘      │
│                                                                  │
│  BaaS:                                                          │
│  ┌──────────────────────────────────────────────────────┐      │
│  │ [PostgreSQL] Product Configs, Policies                │      │
│  │ [MongoDB] Async Operations                            │      │
│  │ [Elasticsearch] Audit Logs (searchable)               │      │
│  └──────────────────────────────────────────────────────┘      │
│                                                                  │
│  Gateway/BFF:                                                   │
│  ┌──────────────────────────────────────────────────────┐      │
│  │ [Redis] Session, Cache, Tokens                        │      │
│  │ [MongoDB] User Preferences                            │      │
│  └──────────────────────────────────────────────────────┘      │
└─────────────────────────────────────────────────────────────────┘
```

---

## 7. 📝 Resumo das Premissas Implementadas

✅ **Entidades Core:**
- Client (controla N clientes)
- Account (ID único, ProductType: CHECKING_ACCOUNT / SAVINGS_ACCOUNT)
- Transaction (DEBIT, CREDIT, BLOCK, RESERVATION, INITIAL_BALANCE)

✅ **Fórmula de Saldo:**
```
Saldo = (CREDIT + INITIAL_BALANCE) - (DEBIT + BLOCK + RESERVATION)
```

✅ **Tecnologia:**
- Dapper para acesso a dados (não Entity Framework)
- SQL Server Functions para cálculo de saldo (não Stored Procedures)

✅ **Event Sourcing Light:**
- INSERT-only para transações
- Imutabilidade de dados transacionais
- Evitar UPDATE/DELETE

✅ **Arquitetura:**
- Inspirada em SEDA (desacoplamento)
- Async Request-Reply Pattern
- Webhooks e Polling para status

✅ **Camadas:**
- Core: Lógica financeira pura
- BaaS: Orquestração e produtos
- Gateway/BFF: Integração multicanal

---

## 8. 🎓 Conclusão

A arquitetura BIRO representa uma solução bancária moderna, escalável e resiliente, construída sobre princípios sólidos de:

- **Imutabilidade de dados transacionais** para auditoria e performance
- **Separação clara de responsabilidades** entre camadas
- **Processamento assíncrono** para operações de longa duração
- **Flexibilidade** para suportar múltiplos produtos e canais
- **Segurança** em múltiplas camadas
- **Observabilidade** completa para monitoramento e troubleshooting

Esta arquitetura permite que BIRO sirva como plataforma BaaS robusta, capaz de suportar desde fintechs emergentes até instituições financeiras de grande porte, mantendo consistência, performance e compliance regulatório.
