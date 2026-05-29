# Sprint 0 and Sprint 1 Detailed User Stories and API Contracts

Version: 1.0
Date: 29 May 2026
Source Backlog: Sprint_Backlog_Plan.md

## Scope
This document expands Sprint 0 and Sprint 1 into implementation-ready user stories with acceptance tests and API contracts for immediate engineering planning.

## Sprint 0: Foundation and Architecture

### Story ARC-001: Establish Clean Architecture Solution Skeleton
As an engineering team,
I want a layered solution structure with strict project dependencies,
so that domain and business logic remain independent from delivery channels.

Acceptance Criteria:
1. Given the repository is cloned, when the solution is opened, then all planned projects are present and compile.
2. Given project references, when a project depends on another layer, then dependencies follow Domain <- Application <- Infrastructure <- API/Web/Workers.
3. Given a clean machine, when dotnet restore and dotnet build run, then the build succeeds.

Technical Tasks:
- Create projects for Web, API, Application, Domain, Infrastructure, Contracts, Workers, Tests.
- Add project references to enforce architectural boundaries.
- Add root solution file and baseline README.

---

### Story SEC-001: Configure JWT and Role-Based Access Skeleton
As a policy platform,
I want token-based authentication with role claims,
so that API endpoints can enforce role-based permissions.

Acceptance Criteria:
1. Given valid registration credentials, when the user registers, then the API returns an access token with role claim.
2. Given valid login credentials, when login is requested, then the API returns a JWT token and expiry.
3. Given invalid credentials, when login is requested, then API returns 401 Unauthorized.

Technical Tasks:
- Configure JwtBearer authentication middleware.
- Implement AuthService contract and token factory.
- Define baseline roles: Customer, Agent, Adjuster, Underwriter, Admin.

---

### Story DB-001: Configure MySQL and EF Core Baseline
As the backend platform,
I want a transactional MySQL persistence layer with EF Core,
so that insurance entities and financial records are persisted reliably.

Acceptance Criteria:
1. Given connection strings are configured, when API starts, then DbContext initializes without runtime errors.
2. Given the domain model, when migration generation runs, then baseline migration files are created.
3. Given migration application command is run, then schema can be created in MySQL.

Technical Tasks:
- Configure Pomelo Entity Framework Core provider for MySQL 8.
- Define initial entities and Fluent API constraints.
- Generate initial migration and commit migration artifacts.

---

### Story DEVOPS-001: CI Baseline
As a delivery team,
I want CI checks for restore/build/test,
so that integration errors are detected before merge.

Acceptance Criteria:
1. Given a pull request, when CI pipeline runs, then restore, build, and tests execute.
2. Given build or tests fail, when pipeline completes, then merge is blocked by policy.

Technical Tasks:
- Add GitHub Actions workflow.
- Add branch protection policy docs.
- Publish test results artifact.

---

### Story OBS-001: Baseline Logging and Correlation
As operations,
I want structured logs from API and workers,
so that payment and claim workflows are traceable.

Acceptance Criteria:
1. Given API requests are processed, when logs are emitted, then request traces include timestamps.
2. Given worker jobs execute, when logs are emitted, then cycle start/end events are visible.

Technical Tasks:
- Standardize log format and levels.
- Add correlation ID middleware in API.
- Add worker cycle instrumentation.

## Sprint 1: Onboarding and KYC

### Story IAM-001: Public Registration
As a public applicant,
I want to register with basic personal details,
so that I can access the insurance platform.

Acceptance Criteria:
1. Given required fields are valid, when register endpoint is called, then account creation succeeds and token is returned.
2. Given duplicate email, when register endpoint is called, then API returns 400 with clear message.
3. Given missing required fields, when register endpoint is called, then API returns 400 validation errors.

Definition of Ready:
- Registration field list approved by compliance and product.

Definition of Done:
- Endpoint, validation, tests, and API docs complete.

---

### Story IAM-002: Login and Session Token
As a policyholder,
I want to log in securely,
so that I can manage policies and claims.

Acceptance Criteria:
1. Given valid credentials, when login is requested, then JWT token is returned.
2. Given invalid credentials, when login is requested, then API returns 401.
3. Given token is expired, when protected API is called, then API returns 401.

---

### Story IAM-004: Refresh Token Rotation
As a policyholder,
I want to obtain a new access token using a valid refresh token,
so that I can remain logged in without resubmitting credentials.

Acceptance Criteria:
1. Given a valid active refresh token, when refresh endpoint is called, then API returns new access and refresh tokens.
2. Given a revoked or expired refresh token, when refresh endpoint is called, then API returns 401.
3. Given a refresh action succeeds, when old refresh token is checked, then it is marked revoked.

---

### Story KYC-001: Ghana Card Verification
As a compliance process,
I want to verify Ghana Card details,
so that KYC is completed before policy issuance.

Acceptance Criteria:
1. Given valid Ghana Card format, when verify endpoint is called, then response returns Verified in mock/sandbox mode.
2. Given invalid format, when verify endpoint is called, then response returns Failed with reason.
3. Given NIA adapter is unavailable, when verify endpoint is called, then status returns Pending and error reason is logged.

---

### Story IAM-005: OTP Challenge and Verification
As a platform user,
I want one-time passcodes for high-risk authentication events,
so that account access can be verified securely.

Acceptance Criteria:
1. Given valid destination and purpose, when OTP request endpoint is called, then an OTP challenge is created and dispatched.
2. Given repeated requests in the throttle window, when OTP request endpoint is called, then API returns a rate-limit validation error.
3. Given multiple invalid verification attempts, when max attempts is reached, then challenge is locked for configured lockout duration.

---

### Story IAM-003: Role Assignment for New Users
As platform administration,
I want a default role applied at registration,
so that access control can be enforced immediately.

Acceptance Criteria:
1. Given no role specified, when registration occurs, then role defaults to Customer.
2. Given role specified by privileged workflow, when registration occurs, then token includes specified role claim.

---

### Story IAM-006: User Profile and Consent Capture
As a newly registered customer,
I want to submit my profile details and explicit consent,
so that onboarding can proceed with compliance-safe data capture.

Acceptance Criteria:
1. Given required profile fields and consent accepted, when profile endpoint is called, then profile is persisted and returned.
2. Given consent is not accepted, when profile endpoint is called, then API returns 400 with clear message.
3. Given profile already exists for email, when profile endpoint is called, then existing profile is updated.

---

### Story IAM-007: Agent-Assisted Onboarding
As an insurance agent,
I want to onboard a customer on their behalf,
so that assisted channel signups are supported.

Acceptance Criteria:
1. Given authenticated Agent/Admin and valid payload, when agent-assisted endpoint is called, then customer auth account and profile are created.
2. Given missing agent identity in token, when agent-assisted endpoint is called, then API returns 401.
3. Given consent is not accepted, when agent-assisted endpoint is called, then API returns 400.

## API Contracts (Sprint 0/1)

Base URL: /api/v1
Authentication: Bearer token (for protected endpoints)
Content-Type: application/json

### 1) Register User
Endpoint: POST /auth/register

Request:
{
  "firstName": "Ama",
  "lastName": "Mensah",
  "email": "ama.mensah@example.com",
  "phoneNumber": "+233240000000",
  "password": "P@ssw0rd!",
  "role": "Customer"
}

Response 200:
{
  "accessToken": "<jwt>",
  "refreshToken": "<refresh-token>",
  "expiresAtUtc": "2026-05-29T12:00:00Z",
  "role": "Customer"
}

Errors:
- 400 duplicate user or validation error

---

### 2) Login User
Endpoint: POST /auth/login

Request:
{
  "email": "ama.mensah@example.com",
  "password": "P@ssw0rd!"
}

Response 200:
{
  "accessToken": "<jwt>",
  "refreshToken": "<refresh-token>",
  "expiresAtUtc": "2026-05-29T12:00:00Z",
  "role": "Customer"
}

Errors:
- 401 invalid credentials

---

### 3) Refresh Session Token
Endpoint: POST /auth/refresh

Request:
{
  "refreshToken": "<refresh-token>"
}

Response 200:
{
  "accessToken": "<jwt>",
  "refreshToken": "<new-refresh-token>",
  "expiresAtUtc": "2026-05-29T12:30:00Z",
  "role": "Customer"
}

Errors:
- 401 invalid, expired, or revoked refresh token

---

### 4) Verify Ghana Card
Endpoint: POST /kyc/verify-ghana-card

Request:
{
  "ghanaCardNumber": "GHA-123456789-1",
  "firstName": "Ama",
  "lastName": "Mensah",
  "dateOfBirth": "1995-01-15T00:00:00Z"
}

Response 200 (verified):
{
  "isVerified": true,
  "verificationStatus": "Verified",
  "message": "Verification successful in mock mode. Replace with NIA API adapter in integration environment."
}

Response 200 (invalid):
{
  "isVerified": false,
  "verificationStatus": "Failed",
  "message": "Invalid Ghana Card format. Expected format: GHA-123456789-1."
}

Response 200 (provider fallback):
{
  "isVerified": false,
  "verificationStatus": "Pending",
  "message": "NIA verification service unavailable. Request queued for retry."
}

---

### 5) Request OTP
Endpoint: POST /auth/request-otp

Request:
{
  "destination": "+233240000000",
  "purpose": "Login",
  "channel": "SMS"
}

Response 200:
{
  "challengeId": "a98db0bfadca4bbd9ab0f7b2c1a4f0ef",
  "expiresAtUtc": "2026-05-29T13:20:00Z",
  "retryAfterSeconds": 60
}

Errors:
- 400 throttled or invalid request

---

### 6) Verify OTP
Endpoint: POST /auth/verify-otp

Request:
{
  "challengeId": "a98db0bfadca4bbd9ab0f7b2c1a4f0ef",
  "code": "123456"
}

Response 200:
{
  "isVerified": true,
  "isLocked": false,
  "remainingAttempts": 5,
  "message": "OTP verified successfully."
}

---

### 7) Revoke All Sessions
Endpoint: POST /auth/revoke-all
Auth: Bearer token required

Response 200:
{
  "message": "All active sessions revoked."
}

---

### 8) Capture Onboarding Profile and Consent
Endpoint: POST /onboarding/profile
Auth: Bearer token required

Request:
{
  "email": "ama.mensah@example.com",
  "firstName": "Ama",
  "lastName": "Mensah",
  "dateOfBirth": "1995-01-15T00:00:00Z",
  "phoneNumber": "+233240000000",
  "ghanaCardNumber": "GHA-123456789-1",
  "consentAccepted": true
}

Response 200:
{
  "customerId": 1,
  "customerNumber": "CUS-20260529120000-ABC123",
  "email": "ama.mensah@example.com",
  "firstName": "Ama",
  "lastName": "Mensah",
  "phoneNumber": "+233240000000",
  "kycVerified": false,
  "consentAccepted": true,
  "consentAcceptedAtUtc": "2026-05-29T12:00:00Z",
  "registeredByAgent": false,
  "registeredByAgentId": ""
}

Errors:
- 400 when consentAccepted is false

---

### 9) Agent-Assisted Onboarding
Endpoint: POST /onboarding/agent-assisted
Auth: Bearer token required (Agent or Admin)

Request:
{
  "register": {
    "firstName": "Kojo",
    "lastName": "Owusu",
    "email": "kojo.owusu@example.com",
    "phoneNumber": "+233241111111",
    "password": "P@ssw0rd!"
  },
  "dateOfBirth": "1992-05-20T00:00:00Z",
  "ghanaCardNumber": "GHA-123456789-1",
  "consentAccepted": true
}

Response 200:
{
  "customerId": 2,
  "customerNumber": "CUS-20260529120500-DEF456",
  "email": "kojo.owusu@example.com",
  "firstName": "Kojo",
  "lastName": "Owusu",
  "phoneNumber": "+233241111111",
  "kycVerified": false,
  "consentAccepted": true,
  "consentAcceptedAtUtc": "2026-05-29T12:05:00Z",
  "registeredByAgent": true,
  "registeredByAgentId": "agent-user-id"
}

Errors:
- 401 when agent identity is missing from token
- 400 when consentAccepted is false

## Test Cases (Ready for QA Automation)

### Auth Registration Tests
1. Register_Succeeds_WithValidPayload
2. Register_Fails_OnDuplicateEmail
3. Register_Fails_OnMissingEmail

### Auth Login Tests
1. Login_Succeeds_WithValidCredentials
2. Login_Fails_WithInvalidPassword
3. Login_Fails_WithUnknownUser

### Auth Refresh Tests
1. Refresh_Succeeds_WithActiveToken
2. Refresh_Fails_WithExpiredToken
3. Refresh_Fails_WithRevokedToken

### OTP Tests
1. RequestOtp_Succeeds_WithValidDestination
2. RequestOtp_Fails_WhenThrottled
3. VerifyOtp_Locks_AfterMaxAttempts

### Session Hardening Tests
1. Refresh_Fails_WithSessionMismatch
2. Refresh_Fails_WithDeviceMismatch
3. RevokeAll_RevokesActiveRefreshTokens

### KYC Tests
1. VerifyGhanaCard_Succeeds_WithValidFormat
2. VerifyGhanaCard_Fails_WithInvalidFormat
3. VerifyGhanaCard_ReturnsPending_WhenNiaUnavailable

### Onboarding Profile and Consent Tests
1. CaptureProfile_Succeeds_WithConsentAccepted
2. CaptureProfile_Fails_WhenConsentMissing
3. CaptureProfile_Updates_WhenEmailAlreadyExists

### Agent-Assisted Onboarding Tests
1. AgentAssistedOnboarding_Succeeds_ForAgentRole
2. AgentAssistedOnboarding_Fails_WhenAgentIdentityMissing
3. AgentAssistedOnboarding_Fails_WhenConsentMissing

## Open Decisions
1. OTP provider selection and fraud controls for registration/login hardening.
2. NIA sandbox/production contract fields and SLA handling.
3. Session concurrency policy (max active refresh tokens per user).
