# 🚀 Minimal API com Resiliência e Observabilidade

Projeto demonstrativo de uma Minimal API moderna em ASP.NET Core focada em resiliência, boas práticas de consumo HTTP e observabilidade, simulando cenários reais de falhas em serviços externos.

Este repositório foi criado com foco prático, seguindo padrões usados em microserviços em produção, não apenas exemplos didáticos.

---

## 🧱 Stack Utilizada

- .NET 8
- ASP.NET Core Minimal APIs
- IHttpClientFactory
- Polly
  - Retry exponencial
  - Timeout
  - Circuit Breaker
- Logging estruturado (ILogger)
- Correlation ID
- OpenTelemetry
  - Tracing distribuído
  - Instrumentação ASP.NET Core
  - Instrumentação HttpClient
- Swagger / OpenAPI

---

## 🎯 Objetivos do Projeto

Este projeto demonstra, na prática:

- Uso correto de HttpClient evitando socket exhaustion
- Resiliência contra falhas externas (timeouts, erros 5xx, instabilidade de rede)
- Retry exponencial e circuit breaker com Polly
- Tratamento adequado de exceções sem derrubar a aplicação
- Retorno de status HTTP apropriados (502, 504)
- Logging estruturado com contexto
- Tracing distribuído ponta a ponta com OpenTelemetry
- Base pronta para observabilidade enterprise (Jaeger, Zipkin, Application Insights)

---

## 📂 Estrutura do Projeto

ResilientApi/
- Program.cs
- ResilientApi.csproj
- appsettings.json
- appsettings.Development.json
- README.md

---

## ▶️ Como Executar

### Pré-requisitos
- .NET SDK 8+

### Executar localmente

dotnet restore  
dotnet run  

A aplicação ficará disponível em:

http://localhost:5112/swagger

---

## 🔌 Endpoints Disponíveis

### GET /health

Endpoint simples de health check.

---

### GET /external

Simula o consumo de um serviço externo instável (https://httpstat.us/500), aplicando:

- Timeout configurado
- Retry exponencial
- Circuit breaker
- Tratamento de exceções
- Tracing distribuído

Possíveis respostas:
- 200 OK – sucesso (em cenários simulados)
- 504 Gateway Timeout – timeout do serviço externo
- 502 Bad Gateway – falha inesperada na chamada externa

---

## 🔁 Resiliência com Polly

O HttpClient é configurado via IHttpClientFactory com:

- Timeout explícito
- Retry exponencial
- Circuit breaker

Benefícios:
- Proteção contra serviços lentos ou instáveis
- Evita sobrecarregar dependências externas
- Garante estabilidade da API mesmo sob falhas

---

## 🔍 Observabilidade e Tracing

O projeto utiliza OpenTelemetry para tracing automático.

Instrumentações habilitadas:
- ASP.NET Core (requisições de entrada)
- HttpClient (chamadas externas)

O que é possível observar:
- TraceId único por requisição
- Spans pai/filho entre endpoint e chamadas externas
- Latência de cada operação
- Status de erro e timeout

Atualmente os traces são exportados para o console, mas a configuração está pronta para integração com:
- Jaeger
- Zipkin
- OTLP
- Application Insights

---

## 🧠 Correlation ID

Cada requisição recebe um Correlation ID:

- Propagado via header (X-Correlation-ID)
- Injetado nos logs
- Facilita rastreabilidade ponta a ponta

---

## 🧪 Cenários Simulados

Este projeto permite simular cenários reais, como:

- Serviço externo lento
- Timeout de requisição
- Falhas de transporte
- Retry automático
- Circuit breaker aberto/fechado

Ideal para estudo e demonstração em entrevistas técnicas.

---

## 🚧 Possíveis Evoluções

- Exportação de traces via OTLP / Jaeger
- Métricas com Prometheus
- Rate limiting
- Health checks avançados
- Testes automatizados
- Dockerização
- Cache distribuído (Redis)

---

## 🧠 O Que Este Projeto Demonstra

Este projeto demonstra a construção de uma API resiliente utilizando IHttpClientFactory, Polly para políticas de resiliência, tratamento adequado de exceções, logging estruturado e tracing distribuído com OpenTelemetry.

---

## 👤 Autor

Alax Pereira  
Desenvolvedor .NET

---

## ⭐ Observação Final

Este projeto não é um hello world.

Ele representa padrões reais usados em microserviços modernos, com foco em:

- estabilidade
- observabilidade
- boas práticas
- leitura clara de logs e traces
