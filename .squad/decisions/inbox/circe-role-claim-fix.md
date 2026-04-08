# Decision: JWT role claim must use short form "role" in AuthController

- **Date:** 2026-04-09
- **Author:** Circe
- **PR:** #128

## Context

`AuthController.GenerateToken` was using `new Claim(ClaimTypes.Role, r)` with a directly-constructed `JwtSecurityToken`. When `JwtSecurityTokenHandler.WriteToken` is called on a pre-built `JwtSecurityToken`, it does **not** apply the `OutboundClaimTypeMap`. As a result, the full URI `http://schemas.microsoft.com/ws/2008/06/identity/claims/role` was written into the JWT payload instead of the short form `"role"`.

The MAUI `AuthService` performs a raw JSON decode of the JWT payload and looks up the key `"role"`. The URI key was never found, so `CurrentUser.Roles` was always empty. The Crew app's role gate then denied every login with "Access denied."

## Decision

**Always use `new Claim("role", r)` (JWT-standard short form) when adding role claims in `AuthController`.**

Do not use `ClaimTypes.Role` with a directly-constructed `JwtSecurityToken` — the outbound claim type mapping is only applied when the handler creates the token via `CreateToken(SecurityTokenDescriptor)`, not when writing a pre-built token.

## Implications

- Any future claim additions in `AuthController` using long-form `ClaimTypes.*` URIs must verify the JWT payload key that will actually be written.
- `AuthService.ParseJwtClaims` reads `"role"` (raw JSON key) — this is correct and requires no change.
- The fix is limited to one line in `AuthController.cs`; no MAUI code changes required.
