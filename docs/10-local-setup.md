# Local Setup

How to run SilverHub locally. The architecture docs (`01`–`09`) cover the design
and the reasoning behind these choices; this file is just the steps to get it
running.

## Prerequisites

| Tool           | Version         | Install                                                    |
| -------------- | --------------- | ---------------------------------------------------------- |
| .NET SDK       | 10 (LTS)        | <https://dotnet.microsoft.com/download/dotnet/10.0>        |
| Docker Desktop | latest          | <https://www.docker.com/products/docker-desktop/>          |
| Node.js        | 24 (Active LTS) | <https://nodejs.org> — or `nvm`/`fnm`, which read `.nvmrc` |

The .NET SDK version is pinned in `global.json` and the Node version in `.nvmrc`,
so version managers select the right toolchain automatically.

Verify:

```bash
dotnet --version   # 10.0.x
docker --version
node --version     # v24.x
```

## Getting started

```bash
git clone https://github.com/Midnight-Programmer/SilverHub-Web-App.git
cd SilverHub-Web-App
```

## Running locally

### 1. Start the database

```bash
docker compose up -d --wait
```

Starts Postgres 18 (exposed on host port `5433`) and waits until it is healthy.
Stop it with `docker compose down` — add `-v` to also delete the data volume and
start fresh next time.

### 2. Run the backend API

```bash
dotnet tool restore                       # first time only: restores dotnet-ef
dotnet run --project src/SilverHub.Api
```

The API listens on **http://localhost:5173**. In Development it automatically
applies EF Core migrations and seeds sample heroes on startup, so a running
database is the only prerequisite.

Endpoints to try:

| URL | What it is |
|---|---|
| <http://localhost:5173/health/live> | Liveness (process is up) |
| <http://localhost:5173/health/ready> | Readiness (can reach the database) |
| <http://localhost:5173/api/v1/heroes> | Hero list (JSON) |
| <http://localhost:5173/openapi/v1.json> | OpenAPI document (Development/Staging only) |

To apply migrations without starting the API:

```bash
dotnet ef database update --project src/SilverHub.Infrastructure --startup-project src/SilverHub.Api
```

### 3. Frontend — _(added in Task 3)_

### 4. Full stack, end to end — _(added in Task 4)_
