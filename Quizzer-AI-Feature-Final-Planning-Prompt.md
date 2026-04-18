# Quizzer — AI-Powered Exam Generation & Interview Prep: Complete Planning Prompt

You are acting as a **senior software architect and technical advisor**.
I need a comprehensive, actionable plan before I begin coding a major new
feature set. Be specific, opinionated, and practical. Where relevant,
provide interface definitions, data model sketches, folder structure examples,
and UI component breakdowns. Do NOT give vague advice — give me things I
can act on directly.

---

## 1. Project Overview

**Project Name:** Quizzer
**Purpose:** A Modular Monolith application for managing online quizzes.
Authors create quizzes; examinees participate.

**Current Tech Stack:**
| Layer | Technology |
|---|---|
| Backend language | C# (.NET 10) |
| Architecture | Modular Monolith |
| Relational DB | Microsoft SQL Server 2022 |
| Cache | Redis 7 |
| Document DB (NEW) | MongoDB 7.0 (being introduced for AI module only) |
| ORM | Entity Framework Core 10 |
| Mediator | MediatR 14 |
| Validation | FluentValidation 12 |
| Logging | Serilog |
| Frontend | TypeScript (~20% of codebase) |

**Existing Modules (under `src/Modules/`):**
- `Identity` — user management, roles, authentication (SQL Server)
- `Quiz` — quiz/question set creation and management (SQL Server)
- `Exam` — exam participation and scoring (SQL Server)
- `AI` (**NEW**) — AI provider config, generation requests, generation history (MongoDB only)

**Key architectural rules:**
- Each module has its own `DbContext` — no cross-module direct DB access.
- Module communication via in-process messaging (MediatR / domain events).
- Central package versioning via `Directory.Packages.props`.

---

## 2. Database Strategy (Final — Do Not Suggest Alternatives)

| Module | Database | Reason |
|---|---|---|
| Identity | SQL Server | Users, roles, auth — relational by nature |
| Quiz | SQL Server | Question sets (manual or AI-generated), structured |
| Exam | SQL Server | Exam sessions, scores, participation |
| AI | MongoDB | Provider configs, raw AI responses, generation history, interview prep materials |

**Why this split:**
- The `AI` module only owns AI-specific artifacts. Once an AI-generated
  question set is finalized, it is handed off to the `Quiz` module via a
  domain event/command and stored in SQL Server — exactly like a manually
  created question set.
- The `Quiz` module does not know or care whether a question set was created
  manually or by AI. Same `DbContext`, same tables.
- MongoDB is used **only** for data that is AI-specific and schema-flexible:
  raw AI responses, generation job history, provider configs, interview prep
  materials (which have no fixed schema).

**MongoDB collections needed:**
- `ai_provider_configs` — per-user AI provider settings and encrypted keys
- `ai_generation_requests` — job history, parameters, status, raw AI response
- `interview_prep_materials` — AI-generated interview prep documents

---

## 3. Module Responsibility Boundaries

| Responsibility | Module | DB |
|---|---|---|
| Manually created question set | Quiz | SQL Server |
| AI-generated question set (final) | Quiz (handed off from AI module) | SQL Server |
| AI provider config per user | AI | MongoDB |
| AI generation request & history | AI | MongoDB |
| Interview prep material | AI | MongoDB |
| Exam sessions & scoring | Exam | SQL Server |
| User identity & roles | Identity | SQL Server |

The `AI` module communicates with the `Quiz` module by publishing a domain
event (e.g., `QuestionSetGeneratedEvent`) after a successful AI generation.
The `Quiz` module listens and persists the question set. This keeps module
boundaries clean.

---

## 4. Feature Requirements

### Feature A: AI Provider Settings (User-Facing)

Every authenticated user can configure their own AI provider.

**Backend:**
- Serve a dynamic list of supported providers (not hardcoded in frontend)
- Store the user's selected provider and secret key in MongoDB
  (`ai_provider_configs` collection)
- The secret key must be **encrypted at rest** using .NET `IDataProtector`
  or AES-256 before storing in MongoDB
- The full key must **never be returned** by any API response after saving
- Expose a **"Test Connection"** endpoint that validates the key against the
  live provider API and stores the result (`lastTestResult`, `lastTestedAt`)

**Supported providers (start with these, more will be added later):**
1. **Google Gemini** (free tier — `gemini-1.5-flash` model)
2. **Groq with Llama 3** (free tier — `llama3-8b-8192` model)

**IAIProvider interface requirement:**
- Provider-agnostic abstraction
- Sends a prompt (system + user message) and receives a structured JSON response
- Resolved per-request based on the current user's configured provider and
  decrypted key
- Easily extensible to new providers (Open/Closed Principle)

---

### Feature B: AI Question Set Generation (Topic-Based)

Any authenticated user can generate a question set by specifying topics.

**Generation parameter wizard:**
- Topics (one or multiple free-text inputs)
- Complexity: `Beginner | Intermediate | Professional | Expert`
- If `Professional` or `Expert`:
  - Total years of experience (numeric input)
  - Expertise / specialization fields (multi-select tags)
  - Additional calibration fields you recommend
- Number of questions: user selects 10–50 (default: 20)
  - Hard domain rule: never less than 10, never more than 50

**Processing:**
- Build prompt from parameters, send to user's configured AI provider
- AI must return strictly structured JSON (no free text, no markdown)
- Validate response against schema before storing
- On invalid response: retry once, then return a friendly error
- On success: publish domain event → `Quiz` module persists the question set

**Visibility rules (domain rule):**
- Created by `Examinee` → **private by default**, can be toggled to public
- Created by `Admin` or `SuperAdmin` → **automatically public**

**Duplicate question mitigation:**
Propose a pragmatic strategy for reducing duplicate questions across
different users' generated sets — suitable for early-stage. Consider:
shared question bank, prompt-level deduplication instructions, content
hashing, or lightweight similarity checks. Choose the most practical option.

---

### Feature C: Job Description → Exam OR Interview Material

User inputs a job title and full job description / requirements text.

System asks the user to choose:

**Option 1 — Generate Exam:**
AI produces a question set tailored to the job requirements.
Same structure and storage as Feature B question sets.

**Option 2 — Interview Preparation Material:**
AI produces a structured document containing:
- Key topics to focus on
- Suggested reading materials (title, description, URL, type)
- Short open-ended practice questions with hints
- Preparation tips and advice

Interview prep material is stored in MongoDB (`interview_prep_materials`)
and is always **private to the user who generated it**.

Both options must work for technical and non-technical job descriptions.

---

## 5. JSON Schemas (AI Must Return These Exactly)

These schemas will be embedded in the system prompt sent to the AI provider.
The AI must return only valid JSON — no markdown, no explanation, no wrapping.

### 5A. Question Set
```json
{
  "title": "string — descriptive title",
  "source": "topic | job_description",
  "complexity": "beginner | intermediate | professional | expert",
  "experienceYears": "number | null",
  "expertiseFields": ["string"],
  "topics": ["string"],
  "totalQuestions": "number (10–50)",
  "questions": [
    {
      "sequence": "number",
      "text": "string",
      "type": "multiple_choice | true_false | short_answer",
      "options": [
        { "id": "a", "text": "string" },
        { "id": "b", "text": "string" },
        { "id": "c", "text": "string" },
        { "id": "d", "text": "string" }
      ],
      "correctOptionId": "string | null (null for short_answer)",
      "explanation": "string",
      "tags": ["string"],
      "difficultyScore": "number 1–10"
    }
  ]
}
```

### 5B. Interview Preparation Material
```json
{
  "jobTitle": "string",
  "keyTopics": ["string"],
  "readingMaterials": [
    {
      "title": "string",
      "description": "string",
      "url": "string | null",
      "type": "article | book | video | documentation | course"
    }
  ],
  "practiceQuestions": [
    {
      "question": "string",
      "hint": "string"
    }
  ],
  "preparationTips": ["string"]
}
```

### 5C. AI Provider Config (MongoDB Document)
```json
{
  "_id": "uuid",
  "userId": "uuid — from Identity module",
  "providerId": "gemini | groq",
  "providerName": "string",
  "encryptedSecretKey": "string — encrypted, never returned via API",
  "isActive": "bool",
  "configuredAt": "ISO8601",
  "lastTestedAt": "ISO8601 | null",
  "lastTestResult": "success | failed | null"
}
```

---

## 6. What I Need From You

### Section 1: Architecture & Module Design
- Full folder/project structure for the new `AI` module following the same
  conventions as existing modules (Domain / Application / Infrastructure layers)
- How does the `AI` module communicate with `Quiz` module without breaking
  boundaries? Show the domain event contract.
- How is MongoDB registered as a separate infrastructure concern alongside
  existing EF Core SQL Server contexts?
- What NuGet packages should be added to `Directory.Packages.props`?
  (MongoDB.Driver, AI provider SDKs or HttpClient-based clients, encryption helpers)

### Section 2: IAIProvider Interface & Factory
- Full `IAIProvider` C# interface definition
- `AIProviderFactory` or resolution strategy — how does it resolve the correct
  provider + decrypted key for the current authenticated user at request time?
- Skeleton of Gemini and Groq concrete implementations
- How do we handle JSON mode / structured output for each provider?

### Section 3: Prompt Engineering
- Exact system prompt templates for all three use cases:
  1. Topic-based question set generation
  2. Job description → question set
  3. Job description → interview preparation material
- Prompts must enforce JSON-only output matching the schemas in Section 5
- Retry and fallback strategy when AI returns invalid or partial JSON

### Section 4: Duplicate Question Strategy
- Concrete, implementable approach to reduce duplicate questions across users
- Must be pragmatic for early-stage — no over-engineering
- Recommended approach with implementation sketch

### Section 5: Security
- How to encrypt/decrypt user API keys using .NET `IDataProtector`
- Risks of storing user-provided API keys and mitigations
- How to ensure a user can only access their own AI config and question sets
- Any other security concerns specific to this feature

### Section 6: Backend API Endpoints
Define REST contracts (method, path, request body, response body) for:
- `GET    /api/ai-providers/supported`
- `POST   /api/user/ai-provider`
- `GET    /api/user/ai-provider`
- `DELETE /api/user/ai-provider`
- `POST   /api/user/ai-provider/test`
- `POST   /api/ai/generate/question-set`
- `POST   /api/ai/generate/from-job-description`
- `GET    /api/question-sets`
- `GET    /api/question-sets/{id}`
- `PATCH  /api/question-sets/{id}/visibility`
- `GET    /api/ai/interview-prep`
- `GET    /api/ai/interview-prep/{id}`
- `GET    /api/ai/generation-history`

### Section 7: Frontend Requirements

The frontend is written in **TypeScript**. Cover all of the following:

#### 7A. New Pages / Views Required
List every new page/view that needs to be created:
- **AI Settings page** — select provider, enter/update/delete API key, test connection, show status
- **Generate Question Set wizard** — multi-step form: topics → complexity → experience (conditional) → question count → preview & confirm → result
- **Job Description page** — input job title + description, choose mode (Exam or Interview Prep), show result
- **Question Sets list page** — show user's private sets + all public sets, filter by topic/complexity/source
- **Question Set detail page** — read-only view of a question set with all questions, options, explanations
- **Interview Prep Material detail page** — view AI-generated interview material
- **Generation History page** — list of past AI generation requests with status (pending/completed/failed)

#### 7B. TypeScript API Client Contracts
- Define TypeScript interfaces/types for every API request and response shape
  listed in Section 6
- How should the API client be structured? (e.g., typed fetch wrapper, axios,
  auto-generated from OpenAPI spec)
- How do we handle API errors, loading states, and AI latency (generation
  can take 3–10 seconds) gracefully in the UI?

#### 7C. UI/UX Considerations
- The question set generation involves a multi-step wizard — recommend the
  best approach to manage wizard state in TypeScript (local state, state
  machine, form library)
- How should we show AI generation progress to the user? (polling, server-sent
  events, websockets — recommend the simplest viable option)
- How should we handle the case where the user has NOT configured an AI
  provider yet — what should the UI show when they try to generate?
- Visibility toggle (private/public) on question sets — where and how should
  this be presented in the UI?
- How should masked API keys be displayed and updated in the AI Settings page?

#### 7D. Component Structure
- Suggest a component breakdown for the most complex views:
  - The multi-step generation wizard
  - The question set detail view (with all question types rendered correctly)
  - The AI Settings page

### Section 8: Feasibility & Risk Assessment
- Is this fully buildable on a Modular Monolith with the described stack?
- Biggest technical risks
- UX/product risks (AI latency, inconsistent outputs, quota exhaustion)
- What happens when a user's free-tier quota runs out?
- Hard decisions that need to be made before starting

### Section 9: Existing Project Improvements
- What improvements to the existing codebase should be made before adding this
  feature to keep things maintainable?
- Patterns to adopt now: Result pattern, outbox for domain events, module
  contracts, etc.

### Section 10: Required Reading & Learning Materials
Curated reading list covering:
- Modular Monolith in .NET (architecture, module boundaries, communication patterns)
- MongoDB with C# / .NET (MongoDB.Driver, document design, indexing)
- AI API integration in C# (HttpClient, Polly resilience, structured output)
- Prompt engineering for structured JSON output
- Secure API key management in .NET (`IDataProtector`, encryption at rest)
- Duplicate/semantic similarity detection (lightweight approaches)
- TypeScript API client design and typed fetch patterns
- Multi-step form/wizard state management in TypeScript

### Section 11: Implementation Roadmap
Break the work into phases for **both backend and frontend**:
- **Phase 1:** Foundation (what must be built first — backend and frontend)
- **Phase 2:** Core AI features (what builds on Phase 1)
- **Phase 3:** Polish and secondary features
- Which phases or tasks can be worked on in parallel by backend and frontend?
- What frontend work can begin with mocked API responses before the backend
  is ready?

---

## 7. Constraints & Non-Negotiables
- Backend: C# (.NET 10), Modular Monolith, module boundaries must be respected
- Database split is final: SQL Server for Identity/Quiz/Exam, MongoDB for AI module only
- Manually created and AI-generated question sets both live in `Quiz` module / SQL Server
- `IAIProvider` interface must make adding a 3rd provider trivial — no changes to existing providers
- All AI responses must be JSON — never parse free text
- User API keys encrypted at rest — never returned in full by any API
- Question sets: minimum 10, maximum 50 questions — hard domain rule
- Frontend: TypeScript — all API contracts must be fully typed
- Solution must be pragmatic — not over-engineered for an early-stage product
- When in doubt or something is unclear during planning, ask me before assuming

---

Be as specific and actionable as possible.
Where you recommend a backend pattern, show the C# interface or class skeleton.
Where you recommend a MongoDB schema, show the document structure.
Where you recommend a frontend component, show its props interface in TypeScript.
Where you recommend a prompt, write the actual prompt text.
If anything in this brief is contradictory or unclear, flag it and ask me
before proceeding.