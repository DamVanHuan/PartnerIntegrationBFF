# PartnerIntegrationBFF

A .NET 8 Backend-for-Frontend (BFF) service that verifies partner information through an external Partner API and publishes successful verification events to RabbitMQ.

## Architecture

The project follows a layered architecture to separate responsibilities:

```
Presentation (API)
        │
        ▼
Application
    ├── Services
    └── Validation
        │
        ▼
Infrastructure
    ├── Partner API Client
    └── RabbitMQ Publisher
```

### Design choices

- **Domain** - The Domain project is intentionally left empty because the business rules in this exercise are not complex enough to justify a dedicated domain model.

- **Application**
  - The business logic is implemented directly in a service layer to keep the solution simple and focused on the requirements.
  - **FluentValidation** validates incoming requests before business logic is executed
  - In a larger or more complex application, I would consider using MediatR with a CQRS approach. This would allow business logic to be encapsulated in request handlers, while cross-cutting concerns such as validation, logging, and authorization could be implemented through MediatR pipeline behaviors, improving separation of concerns and maintainability.

- **Infrastructure**
  - **HttpClient** wrapped in `Microsoft.Extensions.Http.Resilience`: provides retry and resilience for transient HTTP failures.
  - **RabbitMQ** for message queue.

- **API** — controllers, the mock partner-verification endpoint, a global
  exception handler, and API-key authentication.

- **Dependency Injection** is used throughout the application to improve testability.

### Currency validation

Currency validation uses a small, hardcoded list of supported ISO 4217 currency codes. For the scope of this exercise, this keeps the implementation simple and avoids introducing unnecessary configuration or persistence. In a production system, the supported currencies would typically be loaded from configuration or a database.

### Error Handling in **TransactionServices.CreateTransactionAsync**

**TransactionServices.CreateTransactionAsync** does not wrap the call to
**IPartnerVerificationService** (external api) in a try/catch. If the resilience pipeline
exhausts its retries, the resulting exception (typically **HttpRequestException**)
is allowed to propagate unchanged, all the way to **GlobalExceptionHandler**, which turns it into a **503 Service Unavailable ProblemDetails** response instead of a raw crash.

Exceptions from the external Partner API are intentionally not caught in the service layer. Instead, they are allowed to propagate to the global exception handler.

This approach centralizes exception handling, logging, and error response generation in a single place, avoiding duplicated try-catch blocks across the application while ensuring consistent API responses.

## Prerequisites

- .NET 8 SDK
- Docker & Docker Compose

## Running locally

### 1. Configure secrets

Initialize User Secrets (if not already initialized):

```bash
dotnet user-secrets init --project src/PartnerIntegrationBFF.Api
```

Set the required secrets with the following content:
Note: `PartnerApi:BaseUrl` should point to the locally running mock Partner API (the port depends on your environment).

```
{
  "RabbitMq:Host": "localhost",
  "RabbitMq:Port": 5672,
  "RabbitMq:Username": "guest",
  "RabbitMq:Password": "guest",
  "RabbitMq:QueueName": "partner-transactions",
  "PartnerApi:BaseUrl": "http://localhost:5038",
  "Security:ApiKey": "my-secret-api-key"
}
```

### 2. Start RabbitMQ in Docker

Start only the `RabbitMQ` service:

```bash
docker compose up -d rabbitmq
```

### 3. Run the API

```bash
dotnet run --project src/PartnerIntegrationBFF.Api
```

Swagger is available at:

```
http://localhost:<port>/swagger
```

## Running with Docker

### 1. Configure environment variables

Create a `.env` file in the project root (the same directory as `docker-compose.yml`) with the following content:

```env
RABBITMQ_USERNAME=guest
RABBITMQ_PASSWORD=guest
SECURITY_API_KEY=my-secret-api-key
```

The `docker-compose.yml` uses these values through environment variable substitution.

### 2. Build and start the application

```bash
docker compose up --build
```

Or run in detached mode:

```bash
docker compose up -d --build
```

### 3. Access the application

- API: `http://localhost:8080`

### 4. Stop the application

```bash
docker compose down
```

## API Security

The API is protected using an API security key passed in the `X-API-KEY` request header.

This approach was chosen because:

- API key authentication is appropriate for trusted service-to-service communication and keeps the authentication mechanism lightweight for this exercise.
- It keeps authentication separate from the request payload.
- It aligns with common REST API practices for API key authentication.
- It provides a simple authentication mechanism that can be evolved to stronger solutions such as OAuth2 or JWT as security requirements grow.

Include the following header in every request:

| Header      | Description                                    |
| ----------- | ---------------------------------------------- |
| `X-API-KEY` | API security key configured in the application |

The API key value should match the value configured through the `.env` file:

```env
SECURITY_API_KEY=your-api-key
```

### Example request

```bash
curl -X POST http://localhost:8080/api/v1/partner/transactions \
  -H "Content-Type: application/json" \
  -H "X-API-KEY: your-api-key" \
  -d '{
    "partnerId": "P-1001",
    "transactionReference": "TXN-99823",
    "amount": 250.00,
    "currency": "USD",
    "timestamp": "2026-07-19T10:00:00Z"
  }'
```

Since the mock verification endpoint fails ~30% of the time, expect an
occasional `503` even after retries — call it a few times to see both the
success and failure paths.

## Testing

### Running the tests

Run all tests:

```bash
dotnet test
```

Tests use `xUnit` + `NSubstitute` and live in
`tests/PartnerIntegrationBFF.Application.UnitTests/`:

- **`SubmitTransactionRequestValidatorTests`** — every FluentValidation rule
  (required fields, amount > 0, supported currency codes).
- **`TransactionServiceTests`**:
  - Verified partner -> publishes to the queue and returns an accepted result.
  - Validation failure -> throws `ValidationException`, nothing downstream is called.
  - Unverified partner -> returns a rejected result, nothing is published.
  - Verification service throws after resilience is exhausted -> the
    exception propagates unchanged out of the service (not swallowed into a
    business result), and nothing is published.

### Unit tests cover

- Request validation
- Service business logic
- Retry/resilience behavior
- External dependency interactions using mocks
