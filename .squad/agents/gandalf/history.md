# Project Context

- **Owner:** Daniel Eli
- **Project:** LanManager_Squad — LAN party management platform
- **Stack:** .NET Aspire orchestration, React frontend, .NET 9 Web API backend, .NET MAUI check-in/check-out apps
- **Created:** 2026-04-05

## Learnings

- **PR Workflow Established** (2026-04-07): Created 5 PRs for squad branches with clear, commit-based descriptions. PR creation requires local tracking branches for remote-only refs. All squad work now flows through GitHub PRs for team review.

- **Branch Organization Pattern** (2026-04-07): Squad branches follow `squad/{issue-num}-{feature}` naming convention. Fix branches use `fix/{issue-type}`. Feature branches use `feat/{feature-name}`. This pattern supports parallel work streams and clear traceability.

- **Feature Area Independence** (2026-04-07): Auth (JWT), door scanning (MAUI), attendance UI (React), and event context (MAUI) are independent features with separate PRs. No cross-PR dependencies detected. Supports parallel review and merge.

- **Sprint 2026-04-08:** Reviewed all 14 squad PRs for MAUI Crew split + Playwright E2E. Merged 11 PRs (6 Playwright chain + 4 MAUI Crew chain + retroactive fixes). Blocked 4 PRs: #80, #81 rebased (merge conflicts resolved); #82 tests fixed (77/77 passing, merge-ready); #93 rebased (base branch auto-update, merge-ready). All 4 now merge-ready. Sprint 100% complete.

