# Quizzer — Online Quiz & Exam Platform

[![Build and Test](https://github.com/mdemrulkayes/Quizzer/actions/workflows/dotnet.yml/badge.svg)](https://github.com/mdemrulkayes/Quizzer/actions/workflows/dotnet.yml)
[![CodeQL](https://github.com/mdemrulkayes/Quizzer/actions/workflows/codeql.yml/badge.svg)](https://github.com/mdemrulkayes/Quizzer/actions/workflows/codeql.yml)

A full-stack online examination platform where authors create questions and exams, and examinees take timed assessments with instant results. Built with a **.NET 10 modular monolith** backend and an **Angular 21** frontend.

---

## Purpose

Quizzer is designed to manage the complete lifecycle of online assessments:

- **Administrators** manage users and oversee the entire platform
- **Quiz Authors** create question banks organized by tags and question sets
- **Examinees** register, browse published exams, take timed assessments, and review their results

The project originally started as an ASP.NET Core quiz app on Bitbucket, and has since been modernized with a modular monolith architecture, role-based access control, and a full single-page application portal.

---

## Tech Stack

| Layer | Technology | Version |
|-------|-----------|---------|
| **Backend API** | ASP.NET Core (Modular Monolith) | .NET 10 |
| **Architecture** | CQRS with MediatR | — |
| **Database** | Entity Framework Core + SQL Server | — |
| **Authentication** | JWT Bearer + Refresh Tokens | — |
| **Frontend** | Angular (Standalone Components, Signals) | 21.1 |
| **UI Components** | PrimeNG (Aura Theme) | 21.x |
| **CSS Framework** | Tailwind CSS | 4.x |
| **Testing** | xUnit Functional Tests | — |
| **Containerization** | Docker Compose | — |

---

## Project Structure

```
Quizzer/
├── src/
│   ├── API/
│   │   └── Quizzer.Api/              # ASP.NET Core Web API host
│   ├── Modules/
│   │   ├── Identity/                  # Authentication, users, roles
│   │   │   └── Modules.Identity/
│   │   ├── Quiz/                      # Questions, question sets, tags
│   │   │   ├── Modules.Quiz.Core/
│   │   │   ├── Modules.Quiz.Application/
│   │   │   ├── Modules.Quiz.Infrastructure/
│   │   │   └── Modules.Quiz.Endpoints/
│   │   └── Exam/                      # Exams, attempts, results
│   │       ├── Modules.Exam.Core/
│   │       ├── Modules.Exam.Application/
│   │       ├── Modules.Exam.Infrastructure/
│   │       └── Modules.Exam.Endpoints/
│   └── Shared/
│       ├── Shared.Core/               # Base entities, interfaces
│       ├── Shared.Application/        # Common application contracts
│       └── Shared.Infrastructure/     # EF Core, shared services
├── tests/
│   └── Quizzer.Api.FunctionalTest/    # End-to-end API tests
├── web/
│   └── quizzer-portal/                # Angular 21 SPA
│       └── src/app/
│           ├── core/                  # Auth, guards, interceptors, services, models
│           ├── shared/layout/         # Sidebar, topbar, layouts
│           └── features/
│               ├── auth/              # Login, register
│               ├── dashboard/         # Role-based dashboard
│               ├── user-management/   # User CRUD (admin)
│               ├── question-management/ # Tags, question sets, questions
│               ├── exam-management/   # Exam CRUD, publish/unpublish
│               ├── exam-taking/       # Browse exams, take with timer
│               ├── exam-results/      # My results, detail, admin view
│               └── profile/           # Profile, change password
├── docker-compose.yml
├── Quizzer.sln
└── README.md
```

---

## Key Features

### Identity & Access Control
- **JWT Authentication** with access token + refresh token flow
- **Session persistence** — tokens stored in browser, auto-restored on page reload
- **4 roles** with hierarchical permissions:
  - **SuperAdmin** — full platform access
  - **SupportAdmin** — administrative operations
  - **QuizAuthor** — content creation and management
  - **Examinee** — take exams and view results
- **Public registration** defaults to Examinee role only; admins assign other roles
- Change password functionality

### Quiz Management (Authors & Admins)
- **Tags** — create, edit, delete tags for categorizing questions
- **Question Sets** — group questions into sets, assign/remove tags
- **Questions** — create with multiple-choice options, mark correct answer
- **Option Management** — add, edit, remove options per question with correct answer selection

### Exam Management (Authors & Admins)
- **Create exams** — set title, description, duration, passing score, link to question sets
- **Publish/Unpublish toggle** — control exam visibility to examinees
- **Delete exams** — remove with confirmation

### Exam Taking (Examinees)
- **Browse available exams** — card grid showing published exams with details
- **Start exam** — confirmation dialog before beginning
- **Timed assessment** — countdown timer with auto-submit on expiry
- **Question navigation** — sidebar with question number grid, color-coded answered status
- **Answer tracking** — radio button selection for each question

### Results & Analytics
- **My Results** — examinees view their attempt history with pass/fail status
- **Result Detail** — score card with percentage, per-question breakdown showing correct vs selected answers
- **Admin Results View** — administrators view all results across exams

### User Management (SuperAdmin)
- **User list** — paginated table with search and role badges
- **View user details** — dialog with full profile info
- **Update roles** — assign/change user roles
- **Delete users** — with confirmation dialog

### Frontend Architecture
- **Angular 21** with standalone components (no NgModules)
- **Signal-based state management** — `signal()`, `computed()`, `linkedSignal()`
- **Built-in control flow** — `@if`, `@for`, `@switch` (no CommonModule)
- **Functional guards and interceptors** — `CanActivateFn`, `HttpInterceptorFn`
- **Lazy-loaded routes** — each feature module loaded on demand
- **PrimeNG Aura theme** — rich UI components (tables, dialogs, forms, menus)
- **Tailwind CSS** — utility-first styling, no custom SCSS
- **Role-based navigation** — sidebar menu filters based on user role
- **OnPush change detection** on every component

---

## Roles & Permissions Matrix

| Feature | SuperAdmin | SupportAdmin | QuizAuthor | Examinee |
|---------|:----------:|:------------:|:----------:|:--------:|
| Dashboard (all stats) | ✅ | ✅ | — | — |
| Dashboard (author stats) | — | — | ✅ | — |
| Dashboard (examinee stats) | — | — | — | ✅ |
| User Management | ✅ | — | — | — |
| Tag Management | ✅ | ✅ | ✅ | — |
| Question Sets | ✅ | ✅ | ✅ | — |
| Question Management | ✅ | ✅ | ✅ | — |
| Exam CRUD | ✅ | ✅ | ✅ | — |
| Take Exams | ✅ | ✅ | ✅ | ✅ |
| My Results | ✅ | ✅ | ✅ | ✅ |
| All Exam Results | ✅ | ✅ | — | — |
| Profile & Password | ✅ | ✅ | ✅ | ✅ |

---

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 24+](https://nodejs.org/)
- SQL Server (or use Docker Compose)

### Backend
```bash
cd src/API/Quizzer.Api
dotnet run
```
The API runs on `https://localhost:7001` by default.

### Frontend
```bash
cd web/quizzer-portal
npm install
npx ng serve
```
The portal runs on `http://localhost:4200` and proxies API calls to the backend.

### Docker
```bash
docker-compose up
```

### Running Tests
```bash
dotnet test
```

---

## AI-Assisted Development

This project is a collaboration between **[Emrul Kayes](https://github.com/mdemrulkayes)** (human developer) and **GitHub Copilot** (AI pair programmer). Here's a transparent breakdown of contributions:

### Human Contributions (Emrul Kayes)
- **Original project creation** — designed and built the initial Quizzer application from scratch
- **Architecture decisions** — modular monolith design, CQRS pattern, module boundaries
- **Backend foundation** — .NET upgrades (up to .NET 10), project structure, core domain logic
- **Infrastructure** — Docker Compose setup, GitHub Actions CI/CD, CodeQL security scanning
- **Code review & direction** — guided all AI-generated work with requirements, feedback, and corrections
- **Quality control** — tested the application, identified bugs (token persistence, dark mode), and directed fixes

### AI Contributions (GitHub Copilot)
- **API module completion** — implemented full Identity, Quiz, and Exam module endpoints following established patterns
- **Functional tests** — generated xUnit integration tests for all API modules
- **Angular 21 portal** — scaffolded and built the entire frontend application including:
  - Core infrastructure (auth service, guards, interceptors, API services, models)
  - All 9 feature modules with 21 components
  - PrimeNG integration and Tailwind CSS migration
- **Code refactoring** — split inline components to separate files, migrated SCSS to Tailwind
- **Bug fixes** — JWT persistence, Sass deprecation warnings, responsive design issues

### By the Numbers
- **~96 total commits** in the repository
- **~11 commits** co-authored with GitHub Copilot (latest feature branch)
- The AI accelerated frontend development significantly — the entire Angular portal (21 components, services, guards, interceptors, models) was built in a single session with human direction and review

> **Philosophy**: The human provides the vision, architecture, and quality control. The AI accelerates implementation, handles boilerplate, and enables rapid iteration. Every AI-generated change was reviewed, tested, and refined through human feedback.

---

## License

This project is for educational and demonstration purposes.
