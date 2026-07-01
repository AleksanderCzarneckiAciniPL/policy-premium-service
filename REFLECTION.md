# Reflection

This document records how I used AI (Claude, via Claude Code) and JetBrains Rider while building
the Policy Premium service, and where my own judgement shaped or overrode the AI's output.

## How I worked

I drove the assessment as a series of small, reviewable steps rather than one large prompt. For
each step I gave Claude a focused instruction, read the resulting diff, ran it through Rider and
the `dotnet` CLI, and only then moved on. The intent was to keep every change small enough that I
could actually verify it, not just accept it.

## What I delegated to Claude

- **Boilerplate and wiring**: the Minimal API endpoints, request/response DTOs, the in-memory
  repository, and the composition in `Program.cs`.
- **Validation**: data-annotation validation plus a custom `EnumName` attribute, and an OpenAPI
  schema transformer so Swagger advertises the permitted enum values.
- **Swagger/OpenAPI documentation**: per-endpoint summaries, descriptions, tags and response
  types, plus document-level metadata.
- **Tests**: the xUnit unit tests for `PremiumCalculator` and the `WebApplicationFactory`-based
  API/integration tests.
- **Automation and packaging**: `.github/workflows/ci.yml`, the multi-stage `Dockerfile`,
  `docker-compose.yml`, `.dockerignore`, and the `README.md`.
- **Refactoring**: extracting endpoints into `Endpoints/` and OpenAPI setup into `Extensions/`,
  and a small clean-up pass over the calculator.

The premium *rules* themselves I treated as given and only asked Claude to confirm the existing
implementation matched them — it did, so I deliberately did not let it rewrite working logic.

## How CLAUDE.md constrained the agent

`CLAUDE.md` acted as a standing brief that kept the agent inside the assessment's scope. The
constraints that mattered most in practice:

- **Fixed scope** — only `POST /quotes`, `GET /quotes/{id}`, `GET /health`. This stopped scope
  creep toward extra endpoints or features.
- **In-memory persistence, no database, no auth, no secrets, no cloud infra unless mocked.** When
  I asked for a deployment target, the agent produced a *mocked* deploy job rather than reaching
  for real infrastructure, which is exactly what the brief required.
- **"No unnecessary abstractions; prefer clear code over clever code."** I leaned on this to push
  back when a change felt over-engineered, and to keep the calculator a plain static function.
- **The 3-4 hour time-box** justified skipping whole categories of work (below).
- **The explicit verification commands and required README/REFLECTION sections** gave me a concrete
  definition of "done" to check against.

## How I used Rider

- **Code review**: I read every AI change as a diff in Rider's VCS view before keeping it, rather
  than trusting the summary. The endpoint extraction and the OpenAPI/calculator refactors were all
  reviewed this way.
- **Navigation**: Go to Definition / Find Usages to confirm that, for example, `IQuoteRepository`
  was only wired up where I expected, that `EnumName` was consumed by both the validator and the
  schema transformer, and that nothing still referenced code I had asked to delete.
- **Inspections**: I relied on Rider's inspections to surface unused usings, nullability warnings
  and redundant code after each change — this caught small issues the build alone would not flag as
  errors.
- **Tests**: I ran the unit and integration tests from Rider's test runner to get per-test results
  and quick re-runs while iterating, in addition to the full CLI test pass.

## How I verified the output

- **`dotnet` CLI** — after material changes I ran `dotnet restore`, `dotnet format
  --verify-no-changes`, `dotnet build -c Release` and `dotnet test -c Release`, mirroring CI. The
  format gate caught a line-ending regression introduced by one edit, which I had auto-fixed.
- **Running the app** — I started the API and exercised it directly (valid quote, unknown id,
  invalid payloads) to confirm status codes, the calculated premium (1080.00 for the worked
  example), the RFC 9457 error shape, and that the generated OpenAPI schema actually contained the
  enum and range constraints. Several AI claims only held up once checked this way.
- **Tests** — 15 tests (unit + integration) as the regression safety net; I added a case-insensitive
  test to lock in a behaviour I had explicitly asked for.
- **Docker** — I reviewed the multi-stage `Dockerfile` and confirmed the published assembly name
  and restore paths. My local Docker daemon was not running, so I did **not** build the image
  locally; I rely on the CI `docker-build` job to actually build it. This is an honest gap.
- **GitHub Actions** — the workflow runs lint, build, test and docker build as separate, ordered
  checks. The image is built on every run as a gate, but only saved and uploaded as an artifact on
  merge to `master`, where a mocked deploy then consumes it. Each restoring job caches NuGet
  packages to avoid a cold restore. I also documented how to make those checks required via branch
  protection.

## Where I overrode or narrowed Claude's suggestions

- **Strings over enum-typed binding for `coverage`/`region`.** Claude's first approach bound the
  fields directly as enums, which gave an automatic Swagger enum but turned a bad value into an
  opaque JSON deserialization failure (a bare 400 / stack trace). I rejected that and kept string
  inputs validated by a custom attribute, so errors stay clear and field-keyed, then had it add a
  schema transformer to still advertise the enum.
- **Case-insensitive inputs.** I narrowed the behaviour to accept any case and echo back the
  canonical spelling, replacing the case-sensitive `[AllowedValues]` the agent started with.
- **Calculator input guards.** I had it add `ArgumentOutOfRangeException` guards to
  `PremiumCalculator` so the "invalid input" unit tests exercise the calculator itself, not just
  the API layer.
- **Mocked, gate-only delivery.** I kept the Docker step as a build gate on PRs that only persists
  an image artifact on merge to `master`, plus a *mocked* deploy — rather than letting it push to a
  registry or wire real infrastructure.
- **Trimming over-reach.** Where a suggested abstraction or extra option didn't earn its keep, I
  left it out per the "no unnecessary abstractions" rule.

## What I intentionally skipped because of the time-box

- A real datastore — persistence is an in-memory dictionary.
- Authentication/authorization and any secrets handling.
- A real deployment: no registry push, no environment provisioning (the deploy job is mocked).
- Externalising the rating table as configuration.
- Structured logging, request correlation and a readiness (vs liveness) health check.
- Property-based tests and OpenAPI snapshot/contract tests.
- Currency/units and idempotency keys on quote creation.

## Limitations of this workflow

- **Plausible-but-wrong output.** The AI confidently produced code that looked right but wasn't —
  e.g. assuming `[AllowedValues]` would surface as an OpenAPI enum (it didn't), and the enum-binding
  error experience. These only surfaced because I ran the app and read the generated schema.
- **Recent framework surface.** .NET 10 features (minimal-API validation, the OpenAPI transformer
  API, Microsoft.OpenApi types) are new enough that I verified behaviour empirically rather than
  trusting the model's recollection.
- **Tooling/encoding quirks.** One generated file was written in the wrong text encoding with
  mangled characters; I caught it by inspecting the bytes and rewrote it. Local Docker being
  unavailable also meant I leaned on CI for the image build.
- **The AI has no memory of my Rider actions.** The reflection on Rider is my own account; the
  agent only ever saw the files and command output, not how I navigated or reviewed in the IDE.
- **Verification is the bottleneck, by design.** The workflow is only as trustworthy as the time I
  spent checking each change. Going faster would have meant accepting unverified code, which is the
  main risk of working this way.
