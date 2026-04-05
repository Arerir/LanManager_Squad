# Work Routing

How to decide who handles what.

## Routing Table

| Work Type | Route To | Examples |
|-----------|----------|----------|
| Aspire orchestration, AppHost, service topology | Morpheus | Add new Aspire service, configure service discovery, cross-service auth |
| Architecture decisions, system design | Morpheus | Data flow design, API contract review, cross-platform consistency |
| React frontend, UI components, state management | Trinity | Event list view, user registration form, check-in status dashboard |
| .NET API endpoints, EF Core, data models | Tank | Events CRUD API, user registration endpoints, check-in/check-out API |
| MAUI apps, check-in/check-out UI | Switch | Check-in screen, check-out flow, tablet operator UI |
| Tests, QA, edge cases | Apoc | API integration tests, Playwright E2E, capacity edge cases |
| Code review | Morpheus | Review PRs, architecture consistency, integration patterns |
| Scope & priorities | Morpheus | What to build next, trade-offs, decisions |
| Session logging | Scribe | Automatic — never needs routing |

## Issue Routing

| Label | Action | Who |
|-------|--------|-----|
| `squad` | Triage: analyze issue, assign `squad:{member}` label | Morpheus |
| `squad:morpheus` | Aspire, architecture, code review | Morpheus |
| `squad:trinity` | React frontend, UI | Trinity |
| `squad:tank` | .NET API, data models | Tank |
| `squad:switch` | MAUI apps | Switch |
| `squad:apoc` | Tests, QA | Apoc |

### How Issue Assignment Works

1. When a GitHub issue gets the `squad` label, the **Lead** triages it — analyzing content, assigning the right `squad:{member}` label, and commenting with triage notes.
2. When a `squad:{member}` label is applied, that member picks up the issue in their next session.
3. Members can reassign by removing their label and adding another member's label.
4. The `squad` label is the "inbox" — untriaged issues waiting for Lead review.

## Rules

1. **Eager by default** — spawn all agents who could usefully start work, including anticipatory downstream work.
2. **Scribe always runs** after substantial work, always as `mode: "background"`. Never blocks.
3. **Quick facts → coordinator answers directly.** Don't spawn an agent for "what port does the server run on?"
4. **When two agents could handle it**, pick the one whose domain is the primary concern.
5. **"Team, ..." → fan-out.** Spawn all relevant agents in parallel as `mode: "background"`.
6. **Anticipate downstream work.** If a feature is being built, spawn the tester to write test cases from requirements simultaneously.
7. **Issue-labeled work** — when a `squad:{member}` label is applied to an issue, route to that member. The Lead handles all `squad` (base label) triage.
