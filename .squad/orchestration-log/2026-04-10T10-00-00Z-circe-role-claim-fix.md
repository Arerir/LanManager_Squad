# Orchestration Log: Circe — JWT Role Claim Fix

**Timestamp:** 2026-04-10T10:00:00Z  
**Agent:** Circe  
**Spawn ID:** circe-role-claim-fix  
**Task:** Diagnosed and fixed JWT role claim mismatch in AuthController

## What Happened

Circe identified and fixed a critical bug in `AuthController.GenerateToken`:
- **Issue:** Role claims were using `ClaimTypes.Role` (full URI) instead of JWT-standard short form `"role"`
- **Root Cause:** When `JwtSecurityTokenHandler.WriteToken` is called on a pre-built `JwtSecurityToken`, it does NOT apply `OutboundClaimTypeMap`
- **Impact:** MAUI `AuthService` performs raw JSON decode, looking for `"role"` key — never found, causing all logins to be denied
- **Fix:** Changed line 56 in `AuthController.cs` from `new Claim(ClaimTypes.Role, r)` to `new Claim("role", r)`

## Outcome

- ✅ One-line change committed to fix/crew-role-claim-type branch
- ✅ PR #128 opened for review
- Decision documented: use short form `"role"` in JWT claims for AuthController
