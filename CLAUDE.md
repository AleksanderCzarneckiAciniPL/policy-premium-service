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

## Business rules

Use simple, documented premium rules.

Example:

- Base premium = sum insured * 0.005
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

Round the final premium to two decimal places.

## Engineering expectations

- Meaningful commit history
- Automated CI on push and pull request
- CI runs restore, format check, build, tests and Docker build
- Unit tests for premium calculation
- API/integration tests for quote creation and retrieval
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
- No cloud-specific infrastructure unless mocked
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