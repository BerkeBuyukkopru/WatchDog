# 🛡️ WatchDog — AI-Powered Application Health & Incident Monitoring Platform

> **AI-Driven Application Health & Predictive Incident Monitoring Platform**

WatchDog is a comprehensive, full-stack application health monitoring ecosystem designed to proactively track system vitality, automate incident detection with fault-tolerant rule engines, and perform instant automated Root Cause Analysis (RCA) using a hybrid AI architecture (Local LLM & Cloud AI). Built for modern distributed or monolithic infrastructures, WatchDog ensures high availability and deep observability without disrupting application layers.

---

### 📊 Platform At A Glance

![.NET Version](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![React Version](https://img.shields.io/badge/React-18.x-20232A?style=flat-square&logo=react&logoColor=61DAFB)
![TypeScript](https://img.shields.io/badge/TypeScript-Strict-3178C6?style=flat-square&logo=typescript&logoColor=white)
![Docker Ready](https://img.shields.io/badge/Docker-Compose-2496ED?style=flat-square&logo=docker&logoColor=white)
![Ollama Integration](https://img.shields.io/badge/Ollama-Llama_3.2-000000?style=flat-square&logo=ollama&logoColor=white)
![Architecture](https://img.shields.io/badge/Architecture-Onion_%2B_FSD-FF4081?style=flat-square)

---

### 👁️ Platform Preview

<img width="1919" height="1079" alt="hero-dashboard" src="https://github.com/user-attachments/assets/38e8c747-07be-41ad-82b2-0b4cec14e4ce" />

---

### 🎯 Why WatchDog?

Traditional monitoring tools often adopt a reactive stance, simply notifying infrastructure teams when an application goes down, leaving engineers to sieve manually through millions of raw log entries. **WatchDog** bridges this operational gap by combining traditional polling mechanisms with automated artificial intelligence:

* **Instant Diagnostics via Bespoke Probes:** Non-blocking asynchronous background workers routinely poll registered applications' `/health` endpoints, utilizing custom-built lightweight probing modules instead of heavy framework dependencies.
* **Intelligent 3-Strike Incident Rule:** Eliminates network noise and transient flakiness (false-positives) through a strict, time-aware state machine evaluating multi-strike failures before escalating.
* **AI-Powered Advisor Pipeline:** The millisecond an incident is confirmed, WatchDog passes enriched snapshot metadata (SQL Server, Redis, RabbitMQ, HTTP/TCP performance metrics) to the active AI engine (Ollama or OpenAI), delivering a comprehensive **Root Cause Analysis (RCA) Report** straight to the responsible administrators' inbox.
* **Zero-Trust & Data Privacy:** Built with enterprise compliance in mind, the system integrates seamlessly with local runtime models via **Ollama (Llama 3.2)**, keeping sensitive system topology configurations completely isolated inside your corporate private network.

---

## 🏗️ Architectural Overview & Design Patterns

WatchDog is engineered using highly decoupled, robust, and industry-standard structural patterns across both sides of the network boundary to ensure strict separation of concerns, enterprise-grade scalability, and predictable data flow.

### 🎯 Backend Architecture: Onion Pattern & Tactical DDD
The backend ecosystem strictly adheres to the **Onion Architecture** paradigm in combination with Domain-Driven Design (DDD) tactical patterns. By placing the core business logic at the absolute center, dependencies flow exclusively inward, protecting the enterprise rules from side effects caused by volatile external infrastructure or UI components.

```text
                  ┌─────────────────────────────────────────┐
                  │            Presentation Layer           │
                  │  (Watchdog.Api / Watchdog.Worker Hubs)  │
                  └────────────────────┬────────────────────┘
                                       │
                                       ▼
                  ┌─────────────────────────────────────────┐
                  │          Infrastructure Layer           │
                  │     (EF Core 10 / OpenAI / Ollama)      │
                  └────────────────────┬────────────────────┘
                                       │
                                       ▼
                  ┌─────────────────────────────────────────┐
                  │            Application Core             │
                  │      (Use Cases / CQRS Interfaces)      │
                  └────────────────────┬────────────────────┘
                                       │
                                       ▼
                  ┌─────────────────────────────────────────┐
                  │               Domain Core               │
                  │    (Pure Entities / Business Rules)     │
                  └─────────────────────────────────────────┘
```

* **Domain Core (`Watchdog.Domain`):** The foundational atomic heart of the platform. It encapsulates pure domain entities (`Incident`, `MonitoredApp`, `AiInsight`, `AdminUser`, `SystemConfiguration`), state-machine rule engines (`IncidentRules`), value objects (`DependencyCheckResult`), and core status definitions. It maintains **zero dependencies** on external frameworks, ORMs, or third-party NuGet packages.
* **Application Core (`Watchdog.Application`):** The orchestration engine that defines the use-cases of the system. Implemented via a robust functional wrapper using the **CQRS-like Use Case Pattern** (`IUseCaseAsync<TRequest, TResponse>`), it declares system boundaries, cross-boundary data transfer structures (DTOs), abstract storage wrappers, and third-party communication contracts.
* **Infrastructure Layer (`Watchdog.Infrastructure`):** Implements all specifications defined by the Application core. It acts as the technical bridge hosting **Entity Framework Core 10** configurations for SQL Server, advanced hybrid LLM orchestrations via **OllamaSharp** and **OpenAI SDK**, database-backed email routing with **MailKit**, OS-level native host performance metrics tracking, and concrete data access repositories.
* **Presentation Layer:** The entry point execution pipelines divided across two major deployment runtimes:
  * **`Watchdog.Api`:** High-performance ASP.NET Core REST web services enforcing role-based authentication policies, global custom middleware exception interception, and real-time push engines via **SignalR Status Hubs**.
  * **`Watchdog.Worker`:** A collection of autonomous background engines (`IHostedService` / `BackgroundService`) running parallel execution loops dedicated to continuous health polling, scheduled routine AI advisory generations, strategic weekly baseline forecasting, and automated cold-storage data archiving.
* **Bespoke Modularity (`backend/modules/*`):** To avoid heavy framework or vendor lock-in, WatchDog implements a completely bespoke, plugin-driven probing infrastructure. Each external technology dependency is treated as an isolated custom module extending a unified abstraction contract layer.

### 🎨 Frontend Architecture: Feature-Sliced Design (FSD)
The client application is organized around the **Feature-Sliced Design (FSD)** framework, mapping structural front-end components into predictable domain slices. This shields the frontend from turning into a monolithic tangle of scripts as features scale:
* **Layouts:** High-level compositional navigation wrappers (`SuperAdminLayout`, `AdminLayout`) defining responsive UI scaffolding and workspace partitions based on user privileges.
* **Features:** Independent, self-contained domain matrices (`auth`, `dashboard`, `apps`, `ai-providers`, `ai-tower`, `admin-management`, `system-settings`) bundling their own feature-specific sub-components, custom Axios service hooks, lifecycle states, and forms validations.
* **Context / Shared Components:** Root-level providers acting as cross-cutting reactive pipelines (`SignalRContext`, `AuthContext`) for ambient WebSocket telemetry tracking and global session state mutations.

---

## 📁 Repository Structure

```text
├── backend/
│   ├── modules/                            # Bespoke Plugin-Driven Probing Engines
│   │   ├── HealthChecks.Abstractions/      # Common validation contracts, results & status enums
│   │   ├── HealthChecks.Heartbeat/         # Application lifeline heartbeat verifiers
│   │   ├── HealthChecks.Http/              # Non-blocking async HTTP endpoint network verifiers
│   │   ├── HealthChecks.MongoDb/           # MongoDB database connectivity state probes
│   │   ├── HealthChecks.RabbitMQ/          # RabbitMQ message broker topology and queue probers
│   │   ├── HealthChecks.Redis/             # Redis caching transactional check matrices
│   │   ├── HealthChecks.SqlServer/         # Microsoft SQL Server operational state engines
│   │   ├── HealthChecks.Ssl/               # Cryptographic SSL certificate expiration counters
│   │   ├── HealthChecks.System/            # Host hardware metrics (Cpu, Ram, Storage probes)
│   │   └── HealthChecks.Tcp/               # Raw TCP network port connectivity verifiers
│   │
│   └── src/
│       ├── Core/
│       │   ├── Watchdog.Domain/            # Domain Layer (Zero-Dependency Core)
│       │   │   ├── Common/                 # Audit trail base entities (BaseEntity, SimpleBaseEntity)
│       │   │   ├── Constants/              # Enterprise role boundary definitions (RoleConstants)
│       │   │   ├── Entities/               # DB-mapped domain models (Incident, HealthSnapshot, etc.)
│       │   │   ├── Enums/                  # Core domain status definitions & types
│       │   │   ├── Rules/                  # State-machine evaluation tables (IncidentRules)
│       │   │   └── ValueObjects/           # Immutable domain concepts (DependencyCheckResult)
│       │   │
│       │   └── Watchdog.Application/       # Application Layer (Orchestration & Contracts)
│       │       ├── Attributes/             # Custom structural request validation attributes
│       │       ├── DTOs/                   # Data transfer frames (AI, Apps, Auth, Monitoring, SystemConfig)
│       │       ├── Enums/                  # Application specific bounded system error codes
│       │       ├── Interfaces/             # Core boundaries, repositories, and client abstractions
│       │       └── UseCases/               # CQRS Use Case execution models (AI, Apps, Auth, HealthMonitoring)
│       │
│       ├── Infrastructure/
│       │   └── Watchdog.Infrastructure/    # Infrastructure Layer (External Integrations)
│       │       ├── AiServices/             # Hybrid LLM engine clients factory (Ollama, OpenAI)
│       │       ├── Auth/                   # Secure cryptographic JWT & BCrypt utilities
│       │       ├── Migrations/             # EF Core incremental database schema timelines
│       │       ├── Monitoring/             # Native OS low-level resource telemetry providers
│       │       ├── Notifications/          # Distribution channels (MailKit SMTP, SignalR Broadcaster)
│       │       ├── Persistence/            # WatchdogDbContext configurations & concrete repositories
│       │       └── Probing/                # Concrete HTTP network probing execution clients
│       │
│       └── Presentation/
│           ├── Watchdog.Api/               # Presentation Layer: REST API Runtime
│           │   ├── Controllers/            # Versioned REST API endpoints separating role privileges
│           │   ├── Hubs/                   # Real-time WebSocket synchronization nodes (StatusHub)
│           │   ├── Middlewares/            # Global exception interception & translation layers
│           │   └── Services/               # Ambient identity query providers (CurrentUserService)
│           │
│           └── Watchdog.Worker/            # Presentation Layer: Autonomous Background Services
│               └── BackgroundServices/     # Managed async background loop hosted engines
│                   ├── AiAnalyzerWorker.cs             # Scheduled Routine AI Analysis Engine
│                   ├── CentralMetricsCollectorWorker.cs# System Resource Telemetry Aggregator
│                   ├── DataArchiverWorker.cs           # Automated Cold-Storage JSON.GZ Archiver
│                   ├── HealthPollingWorker.cs          # Continuous Application Polling Loop
│                   ├── StrategicAnalyzerWorker.cs      # Weekly/Monthly Baseline Forecaster
│                   └── WorkerCurrentUserService.cs     # Synthetic service context identity provider
│
├── frontend/
│   ├── src/
│   │   ├── api/                            # Centralized Axios engine clients & endpoint services
│   │   ├── components/                     # Shared global presentation atoms & ProtectedRoute Guard
│   │   ├── context/                        # Root reactive state providers (SignalR WebSocket, Auth)
│   │   ├── features/                       # Feature-Sliced Application Domains
│   │   │   ├── admin-management/           # Access privileges control panels & user matrices
│   │   │   ├── ai-providers/               # Core LLM engine registration, toggle & threshold fields
│   │   │   ├── ai-tower/                   # AI advisory insights & interactive Root Cause Analysis logs
│   │   │   ├── apps/                       # Registry dashboards for target platform scopes
│   │   │   ├── auth/                       # Secure entry gates, identity recovery & password tokens
│   │   │   ├── dashboard/                  # Live telemetry grids, incident histories & metric widgets
│   │   │   └── system-settings/            # Global system alerting threshold parameter fields
│   │   ├── layouts/                        # Compositional dashboard views based on custom roles
│   │   ├── routes/                         # Explicit React Router v6 path mapping tables
│   │   └── types/                          # Strict TypeScript component communication contracts
│   │
│   ├── Dockerfile                          # Nginx compositional client-side build manifest
│   ├── nginx.conf                          # Single Page Application (SPA) reverse routing engine configuration
│   ├── tailwind.config.js                  # Atomic style design definitions
│   └── vite.config.ts                      # Frontend bundling parameters
│
└── docker-compose.yml                      # Orchestrated Multi-Container Production Manifest Layout
