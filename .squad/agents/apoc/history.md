# Apoc — Test Coverage Implementation History

## Learnings

### 2026-04-07: Coverlet HTML Coverage Reports Configured
**What:** Added `coverlet.msbuild` package (v6.0.4) to test project. Configured MSBuild properties: `CollectCoverage=true`, `CoverletOutputFormat=cobertura`, `CoverletOutput=Coverage/`. Updated CI workflow to install `reportgenerator` tool and generate HTML reports from Cobertura XML. Coverage HTML now uploaded as CI artifact. Added `**/Coverage/` to `.gitignore`.  
**Why:** Enables automatic coverage reporting on every test run. Cobertura format is industry-standard for conversion to HTML. ReportGenerator produces readable HTML summaries. Test project self-contained — Coverage/ folder lives alongside tests.  
**Impact:** Coverage reports now available in CI artifacts. Local devs can run `dotnet test` and get Coverage/html/ folder. No solution-level changes required. Ready to enforce coverage thresholds in future work.
