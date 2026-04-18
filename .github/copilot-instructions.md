# Copilot Instructions for Quizzer

## Commands

### Backend (.NET 10)
```bash
# Run the API
cd src/API/Quizzer.Api && dotnet run

# Run all tests
dotnet test

# Run a single test class
dotnet test --filter "FullyQualifiedName~QuestionSetEndPointTest"

# Run a single test method
dotnet test --filter "FullyQualifiedName~QuestionSetEndPointTest.CreateQuestionSet_ShouldReturnOk"

# Add EF migration (replace <Module> and <MigrationName> as needed)
dotnet ef migrations add <MigrationName> --project src/Modules/Quiz/Modules.Quiz.Infrastructure --startup-project src/API/Quizzer.Api
```

### Frontend (Angular 21)
```bash
cd web/quizzer-portal
npm install
npx ng serve          # dev server on http://localhost:4200
npm run build         # production build
npm test              # run unit tests
```

### Docker
```bash
docker-compose up     # starts API + SQL Server + Redis
```

---

## Architecture

### Backend — Modular Monolith
The backend is a single ASP.NET Core host (`src/API/Quizzer.Api`) that composes three self-contained vertical modules:

| Module | Path | Responsibility |
|--------|------|---------------|
| Identity | `src/Modules/Identity/` | Auth, users, roles, JWT |
| Quiz | `src/Modules/Quiz/` | Tags, question sets, questions, options |
| Exam | `src/Modules/Exam/` | Exams, attempts, results |

Each module follows the same four-layer layout:
```
Modules.<Name>.Core/           # Domain entities, interfaces, domain errors
Modules.<Name>.Application/    # CQRS commands/queries + FluentValidation validators
Modules.<Name>.Infrastructure/ # EF Core DbContext, repositories, migrations
Modules.<Name>.Endpoints/      # Minimal API endpoint definitions (IBaseEndpoint)
```

**Registration flow**: Each module exposes a single `Register*Module(services, config, mediatRAssemblies)` extension method called from `Program.cs`. The `mediatRAssemblies` list is mutated by each module to register its own handlers.

**Each module has its own DbContext and SQL schema** (`QuestionModuleConstants.SchemaName`, etc.). EF migrations are per-module and run automatically on startup via `app.Migrate*ModuleDatabase()`.

**Keyed DI for `IUnitOfWork`**: Each module registers `IUnitOfWork` with a key (e.g., `ModuleKeys.Quiz`) to avoid cross-module ambiguity. Inject with `[FromKeyedServices(ModuleKeys.Quiz)]`.

### Shared abstractions (`src/Shared/`)
- **`Shared.Core`** — `BaseEntity`, `BaseAuditableEntity`, `Result<T>`, `Error`, `ICommand<T>`, `IQuery<T>`, `IBaseEndpoint`, caching interfaces, MediatR pipeline behaviour interfaces.
- **`Shared.Application`** — `PagedListDto<T>` and other API contract types.
- **`Shared.Infrastructure`** — Redis cache, `PopulateAuditableEntityInterceptor` (auto-fills audit fields), `IntegrationEventPublisher`/`Processor`.

### CQRS / MediatR pipeline
All requests flow through three global pipeline behaviours registered in `Program.cs`:
1. `RequestLoggingBehaviour` — logs every request
2. `ValidationBehaviour` — runs FluentValidation; returns `Errors` list on failure
3. `QueryCachingBehaviour` / `CacheInvalidationBehaviour` — Redis caching for `ICacheableQuery` / `ICacheInvalidatingCommand`

### Domain model conventions
- **Entities**: private setters, private constructor, static `Create(...)` factory that returns `Result<TEntity>`.
- **Errors**: defined as `static` properties on a `struct` named `<Entity>Errors` in the Core layer (e.g., `TagErrors.TagNotFound`).
- **Domain validation**: happens inside `Create`/`Update` methods and returns `Error` via `Result<T>` implicit conversion.

### Endpoints
Every endpoint class implements `IBaseEndpoint` and is auto-discovered via `RegisterEndpoints(mediatRAssemblies)`. Route constants live in `*ModuleConstants.Route.*`. Authorization policies from `AuthorizationPolicyConstants` in the Identity module.

---

## Frontend Conventions

### Angular patterns
- **No NgModules** — every component is standalone.
- **Signals everywhere**: use `signal()`, `computed()`, `linkedSignal()` for state; avoid `BehaviorSubject` for new code.
- **Built-in control flow**: `@if`, `@for`, `@switch` — never import `CommonModule`.
- **`ChangeDetectionStrategy.OnPush`** on every component.
- **Lazy loading**: all feature routes use `loadComponent` (not `loadChildren`).
- **Functional guards/interceptors**: `CanActivateFn`, `HttpInterceptorFn` — no class-based guards.
- **Tailwind CSS only** — no custom SCSS; utility classes inline.
- **PrimeNG Aura theme** for UI components.

### Auth / role checks (frontend)
`AuthService` (`core/auth/auth.service.ts`) holds the session state as signals. Role checks:
- `authService.isAdmin` — SuperAdmin or SupportAdmin
- `authService.isQuizAuthor` — SuperAdmin, SupportAdmin, or QuizAuthor
- `authService.isExaminee` — Examine role only

Route-level protection uses `roleGuard` with `data: { allowedRoles: [...] }`.

### API base URL
`environment.apiBaseUrl` is injected from Angular environments. The dev proxy (`proxy.conf.json`) forwards `/api` to `https://localhost:7001`.

---

## Key Conventions

### Result pattern
Commands and queries return `Result<T>`. Endpoint handlers call `.ConvertToResult()` (extension from `Shared.Application`) which maps `ErrorType` → HTTP status code. Never return raw `Ok()`/`BadRequest()` in handlers — always use the `Result<T>` pipeline.

### Package versions
All NuGet package versions are managed centrally in `Directory.Packages.props`. Do not add `Version=` attributes in individual `.csproj` files.

### Functional tests
Tests use `WebApplicationFactory<Program>` with **Testcontainers** (real SQL Server in Docker) and a `NoOpCacheService` replacing Redis. The base class `QuizzerBaseFunctionTest` seeds three test users:
- `test1@gmail.com` — Examinee (default)
- `test2@gmail.com` — QuizAuthor (promoted via raw SQL in `RegisterOneTimeUser`)
- `test3@gmail.com` — Examinee

Call `AddTokenToEachRequest()` to attach the QuizAuthor token. Tests require Docker to be running.

### Database column constraints
Always check `HasMaxLength()` in EF entity configurations before using test data or generating values. Key limits:
- `QuestionSet.Name` — max 50 chars
- `QuestionSet.SetCode` — max 50 chars
- `QuestionSet.Details` — max 200 chars
- `QuestionSet.ExpertiseFields` — max 1000 chars

When generating random codes/identifiers in tests, ensure they fit within the column's `HasMaxLength()` constraint. Never assume a column allows unlimited length — always verify the EF configuration first.

### Four roles (backend constants in `Modules.Identity`)
`SuperAdmin`, `SupportAdmin`, `QuizAuthor`, `Examine` (note: "Examine" not "Examinee" in code).
