# WebApplication1 — Making an existing app testable

This repository demonstrates a progressive approach to making an existing ASP.NET application testable. The example branches show incremental changes you can apply to move from a simple app that has no tests to a design that supports generated E2E tests, focused unit tests with extracted logic, and dependency injection (DI) so unit tests don't require full integration testing.

Branches (progression):

- `Start-NoTests-SqlLite`: Baseline application using SQLite or a simple DB provider. No automated tests are included.
- `PlaywrightTests`: Adds end-to-end (E2E) tests powered by Playwright (.NET). Demonstrates how to run browser-driven tests against the running app using a test fixture.
- `Xunit`: Adds focused unit tests using xUnit where logic has been extracted from UI/PageModels into testable services and classes.
- `WithDI`: Introduces/cleans up dependency injection so business logic and external dependencies can be mocked. This branch aims to avoid integration tests for most scenarios by enabling true unit testing.

Why this structure

- Break branches into small, reviewable steps so you can see the transformation from an untested app to a testable architecture.
- Demonstrates patterns: extract logic into services, register interfaces in DI, mock dependencies in unit tests, and use Playwright for high-confidence E2E tests when needed.

Quick start — common commands

- Run solution build and all tests:

	`dotnet restore`

	`dotnet build`

	`dotnet test WebApplication1.sln`

- Run unit tests (xUnit project):

	`dotnet test TestProject1/TestProject1.csproj`

- Run E2E tests (Playwright project):

	`dotnet test WebApplication1.E2ETests/WebApplication1.E2ETests.csproj`

- If Playwright CLI/tools are needed for recording or installing browsers (optional):

	`dotnet tool install --global Microsoft.Playwright.CLI`

	`playwright install`

What to look for in each branch

- `Start-NoTests-SqlLite`:
	- Application code lives with UI and logic closely coupled (Razor PageModels or controllers contain business logic).
	- Use this to create a baseline and identify hard-to-test areas.

- `PlaywrightTessts`:
	- Contains Playwright fixtures and page objects under `PageObjects/` and `WebApplication1.E2ETests/`.
	- Use E2E tests for high-level verification of flows (create, edit, delete).
	- Keep E2E tests focused and small; they are slower and more brittle than unit tests.

- `Xunit`:
	- Logic extracted into services/classes so you can exercise core behavior in isolation.
	- Look for tests that directly target service methods instead of rendering pages.
	- Use test doubles (mocks/fakes) for external dependencies (repositories, email senders, time providers).

- `WithDI`:
	- Shows how constructor injection is used to supply dependencies (interfaces) to PageModels/services.
	- Enables swapping real implementations for fakes/mocks in unit tests so tests don't require a live database or SMTP server.

Guidance and patterns

- Extract logic: move business logic out of PageModel/Controller into classes in `Services/` or `Domain/` and expose behavior via methods accepting plain data.

- Program to interfaces: create small interfaces (e.g., `ILoanProspectService`) and register them in the DI container in `Program.cs`.

- Use composition for infrastructure: keep EF Core, email, and other adapters at the edges of the system behind interfaces.

- Unit tests: test the pure logic classes directly with xUnit and a mocking library (Moq, NSubstitute). Avoid hitting real DB or external services in unit tests.

- E2E tests: limit scope to critical user flows. Use Playwright fixtures to start the app and run tests against a test host.

Example checklist to make a feature unit-testable

1. Identify logic in a PageModel that needs testing.
2. Extract the logic to a service class with clear inputs/outputs.
3. Define an interface for the service and register it with DI.
4. Replace direct DB or service calls with injected interfaces.
5. Add unit tests for the service; mock collaborations.
6. Keep one or two E2E tests to exercise the end-to-end flow and guard integration assumptions.

Helpful tips

- Keep tests fast and deterministic: prefer pure logic tests and mock external systems.
- Use small, focused E2E suites; they are for confidence, not for replacing unit tests.
- Keep page objects and Playwright helpers in `PageObjects/` to share across E2E tests.

Next steps

- Checkout the branches listed above locally to review the changes incrementally.
- Run `dotnet test` on each project to confirm tests are working in your environment.

Files of interest

- `WebApplication1/Program.cs` — DI registration and app startup
- `WebApplication1/Pages/LoanProspects/*` — example UI and PageModels to refactor
- `TestProject1/` — unit tests and fakes
- `WebApplication1.E2ETests/` and `PageObjects/` — Playwright E2E tests and helpers
