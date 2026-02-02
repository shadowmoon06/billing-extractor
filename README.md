# BillingExtractor

A .NET 10 API for extracting invoice information from images using Google Gemini AI.

## Tech Stack

- .NET 10 / ASP.NET Core Web API
- PostgreSQL (database)
- Redis (distributed cache)
- Google Gemini AI (invoice extraction)
- React + Vite (frontend)

## Quick Start with Docker

### Prerequisites

- [Docker](https://docs.docker.com/get-docker/) and Docker Compose
- [Google Gemini API Key](https://aistudio.google.com/apikey)

### 1. Clone and Configure

```bash
git clone <repository-url>
cd bill-extractor

# Copy environment template
cp .env.example .env
```

### 2. Edit `.env` File

```env
# Required: Your Google Gemini API key
GEMINI_API_KEY=your-gemini-api-key-here

# PostgreSQL settings
POSTGRES_USER=postgres
POSTGRES_PASSWORD=your-secure-password
POSTGRES_DB=billingextractor
POSTGRES_PORT=5432

# Redis settings
REDIS_PORT=6379

# Application ports
API_PORT=1314
FRONTEND_PORT=5200
```

### 3. Start Services

```bash
# Build and start all services
docker-compose up -d --build

# Check status
docker-compose ps
```

> **Note:** Tests run automatically during Docker build. If any test fails, the build will fail and the container won't start.

### 4. Access the Application

| Service | URL |
|---------|-----|
| Frontend | http://localhost:5200 |
| API | http://localhost:1314 |

> **Note:** Swagger UI is disabled in Docker (Production mode). See [Local Development](#local-development-without-docker) to use Swagger.

## Docker Commands

```bash
# Start all services
docker-compose up -d

# Stop all services
docker-compose down

# View logs
docker-compose logs -f

# View specific service logs
docker logs billingextractor-api
docker logs billingextractor-frontend

# Rebuild after code changes
docker-compose up -d --build

# Force rebuild without cache
docker-compose build --no-cache && docker-compose up -d

# Remove all data (volumes)
docker-compose down -v
```

## API Endpoints

### Invoice Controller (`/api/Invoice`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | Get all invoices |
| GET | `/{id}` | Get invoice by ID |
| POST | `/` | Create invoice |
| PUT | `/{id}` | Update invoice |
| DELETE | `/{id}` | Delete invoice |
| POST | `/extract` | Extract invoice info from uploaded images |

### Extract Invoice from Image

```bash
curl -X POST http://localhost:1314/api/Invoice/extract \
  -F "files=@invoice.jpg"
```

## Project Structure

```
bill-extractor/
├── src/
│   ├── BillingExtractor.API/        # ASP.NET Core Web API
│   ├── BillingExtractor.Business/   # Business logic, AI integration
│   └── BillingExtractor.Data/       # EF Core, PostgreSQL
├── frontend/                         # React + Vite frontend
├── docker-compose.yml
└── .env
```

## Troubleshooting

### CORS Errors

The API allows requests from `http://localhost:{FRONTEND_PORT}`. Ensure your `.env` has the correct `FRONTEND_PORT` value.

### Database Connection Failed

```bash
# Check PostgreSQL is healthy
docker-compose ps

# View PostgreSQL logs
docker logs billingextractor-postgres
```

### API Not Starting

```bash
# Check API logs for errors
docker logs billingextractor-api

# Verify Gemini API key is set
docker exec billingextractor-api printenv | grep GEMINI
```

### Rebuild After Code Changes

```bash
docker-compose up -d --build api
```

## Local Development (without Docker)

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/download/)
- [Redis](https://redis.io/download/)
- [Node.js 22+](https://nodejs.org/) (for frontend)

### 1. Setup Database

Start PostgreSQL and Redis locally, then update connection settings in `src/BillingExtractor.API/appsettings.json`.

### 2. Configure API Key

```bash
cd src/BillingExtractor.API
dotnet user-secrets set "GeminiAPIKey" "your-gemini-api-key"
```

### 3. Run the API

```bash
cd src/BillingExtractor.API
dotnet run
```

### 4. Run the Frontend

```bash
cd frontend
npm install
npm run dev
```

### 5. Access Swagger UI

Swagger is only available in **Development** mode:

| Service | URL |
|---------|-----|
| API | https://localhost:5001 |
| Swagger UI | https://localhost:5001/swagger |

> **Note:** Swagger is disabled in Production (Docker). To enable Swagger in Docker, set `ASPNETCORE_ENVIRONMENT=Development` in docker-compose.yml.

## Running Tests

### During Docker Build

Tests run automatically when building the Docker image. If tests fail, the build fails.

```bash
# Build will run tests automatically
docker-compose build api
```

### Locally

```bash
# Run all tests
dotnet test tests/BillingExtractor.Business.Tests/BillingExtractor.Business.Tests.csproj

# Run with verbose output
dotnet test tests/BillingExtractor.Business.Tests/BillingExtractor.Business.Tests.csproj -v normal
```