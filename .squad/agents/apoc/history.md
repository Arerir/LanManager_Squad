# Apoc — Test Coverage Implementation History

## Learnings

### 2026-04-07: Coverlet HTML Coverage Reports Configured
**What:** Added `coverlet.msbuild` package (v6.0.4) to test project. Configured MSBuild properties: `CollectCoverage=true`, `CoverletOutputFormat=cobertura`, `CoverletOutput=Coverage/`. Updated CI workflow to install `reportgenerator` tool and generate HTML reports from Cobertura XML. Coverage HTML now uploaded as CI artifact. Added `**/Coverage/` to `.gitignore`.  
**Why:** Enables automatic coverage reporting on every test run. Cobertura format is industry-standard for conversion to HTML. ReportGenerator produces readable HTML summaries. Test project self-contained — Coverage/ folder lives alongside tests.  
**Impact:** Coverage reports now available in CI artifacts. Local devs can run `dotnet test` and get Coverage/html/ folder. No solution-level changes required. Ready to enforce coverage thresholds in future work.

### 2026-05-02: Comprehensive Test Coverage Expansion
**What:** Significantly expanded test coverage from 14.45% to 37.65% line coverage (160% relative increase, +23.2 percentage points). Added 1182 lines of test code across 5 new test files and expanded 4 existing files. Coverage targets: Events, Users, Registrations, Auth, Tournament controllers (previously 0%), plus expanded Seats, CheckIn, DoorPass, Equipment. All 77 tests passing.  
**Why:** Low coverage (14%) left core API functionality untested. Critical paths like user registration, event CRUD, authentication, and tournament management had zero test coverage. Identified gaps using cobertura XML analysis - targeted methods with 0% line-rate first.  
**Impact:** Core business logic now has meaningful test coverage. Happy paths, validation errors (400/422), authorization (401), not-found (404), and conflict (409) scenarios covered. Tests use in-memory EF DbContext + Moq for dependencies. Established patterns: controller tests mock UserManager/SignalR hubs, use ClaimsHelper for auth context, TestDbContextFactory for isolated DB instances. Ready to enforce coverage thresholds in CI.

