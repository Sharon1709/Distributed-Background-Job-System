# MiniHangfire – Distributed Background Job Processing System

A production-style distributed background job processing system built using .NET 8, PostgreSQL, RabbitMQ, and Docker.

## 🚀 Architecture

API → RabbitMQ → Worker → PostgreSQL

- API service creates jobs
- Jobs are published to RabbitMQ
- Worker consumes jobs asynchronously
- Retry logic with exponential backoff
- Dead Letter Queue (DLQ) support
- Idempotent job processing
- Automatic database migrations
- Fully containerized using Docker Compose

## 🛠 Tech Stack

- .NET 8
- ASP.NET Core
- Entity Framework Core
- PostgreSQL
- RabbitMQ
- Docker & Docker Compose
- Serilog (Structured Logging)

## 📦 Project Structure

```
src/
  ├── JobSystem.API
  ├── JobSystem.Application
  ├── JobSystem.Domain
  ├── JobSystem.Infrastructure
  ├── JobSystem.Worker

tests/
```

## 🧠 Key Features

- Clean Architecture (Domain-driven separation)
- Background Worker using Hosted Services
- Retry mechanism (Max 3 attempts)
- Dead Letter Queue handling
- Idempotency guard
- Environment-based configuration
- Automatic DB migration on startup

## 🐳 Running Locally

1. Create `.env` file:

```
ConnectionStrings__Default=Host=postgres;Port=5432;Database=jobsdb;Username=jobuser;Password=jobpassword
RabbitMq__HostName=rabbitmq
RabbitMq__UserName=guest
RabbitMq__Password=guest
```

2. Run:

```
docker compose up --build
```

3. Test API:

```
POST http://localhost:5000/api/jobs
```

## 📊 Verification

- Check PostgreSQL table `Jobs`
- Check RabbitMQ dashboard at http://localhost:15672
- Check worker logs via `docker logs jobsystem-worker`

## 🌍 Deployment

Designed for container-based cloud deployment (Render / Azure Container Apps / Fly.io).

## 📈 Future Improvements

- Job scheduling (cron support)
- Metrics & observability
- Horizontal worker scaling
- Web dashboard UI
- CI/CD pipeline

---

Built as a production-ready backend portfolio project.
