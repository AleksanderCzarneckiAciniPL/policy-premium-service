# Claude working instructions

You are helping build a small production-quality C#/.NET 10 Policy Premium service for a time-boxed engineering assessment.

## Goal

Deliver a small, coherent, well-tested, automated repository. Do not over-engineer.

## Stack

- C# with .NET 10
- Minimal API
- xUnit for tests
- GitHub Actions for CI
- Docker for packaging
- JetBrains Rider as the primary IDE

## Functional scope

Implement only:

- POST /quotes
- GET /quotes/{id}
- GET /health

Persistence is in-memory only.

All endpoints are documented via OpenAPI/Swagger (served at `/swagger` in Development),
including the permitted enum values and numeric bounds.

## Project structure

```
src/PolicyPremium.Api
  Domain/        # Quote, enums, PremiumCalculator (pure rating logic)
  Contracts/     # Request/response DTOs
  Validation/    # EnumName validation attribute
  Storage/       # IQuoteRepository + in-memory implementation
  Endpoints/     # Minimal API route groups (health, quotes)
  Extensions/    # OpenAPI/Swagger configuration
  Program.cs     # Composition root
tests/PolicyPremium.Tests   # Unit (PremiumCalculator) + API/integration tests
```

## Business rules

Premium is calculated as:

```
premium = max(
  100,
  sumInsured * 0.005 * coverageMultiplier * regionMultiplier * claimsMultiplier
)
```

rounded to two decimal places (away from zero).

- Base rate = sum insured * 0.005
- Coverage multiplier:
    - Basic: 1.00
    - Standard: 1.25
    - Comprehensive: 1.50
- Region multiplier:
    - LowRisk: 0.90
    - Standard: 1.00
    - Urban: 1.15
    - Coastal: 1.20
- Claims loading:
    - +10% per prior claim
    - capped at +50%
- Minimum premium:
    - 100.00

### Validation

- `coverage` and `region` are accepted as case-insensitive strings matching the enum names
  (echoed back in canonical spelling); unknown values are rejected with an RFC 9457 validation
  problem.
- `sumInsured` must be greater than 0; `priorClaims` must be 0 or greater.

## Engineering expectations

- Meaningful commit history
- Automated CI on push to `master` and on pull request, as ordered, separately reported checks:
  lint (format check) -> build -> test -> Docker build -> mocked deploy
- CI caches NuGet restore; the Docker image is saved and uploaded as a workflow artifact only on
  merge to `master`, where a mocked deploy (a `production` GitHub Environment + simulated rollout)
  consumes it. Nothing is pushed to a registry
- Branch protection on `master` can require the lint/build/test/Docker build checks
- Unit tests for premium calculation
- API/integration tests for quote creation and retrieval (via `WebApplicationFactory`)
- Dockerfile and docker-compose.yml
- README.md explaining:
    - how to run
    - API examples
    - business rules
    - assumptions
    - trade-offs
    - what is mocked or intentionally skipped
    - what would be done next
- REFLECTION.md explaining:
    - how AI was used
    - what was delegated
    - how outputs were verified
    - where human judgement overrode AI

## Constraints

- No database
- No authentication
- No cloud-specific infrastructure unless mocked (deployment is mocked; no registry push)
- No secrets
- No unnecessary abstractions
- Prefer clear code over clever code
- Keep the solution suitable for a 3-4 hour time-box

## Verification commands

After material changes, run:

```bash
dotnet restore PolicyPremium.sln
dotnet format PolicyPremium.sln --verify-no-changes
dotnet build PolicyPremium.sln --configuration Release
dotnet test PolicyPremium.sln --configuration Release
docker build -t policy-premium-service .