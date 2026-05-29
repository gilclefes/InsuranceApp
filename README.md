# InsuranceApp

Digital Insurance Management Platform scaffold for Ghana, aligned to ASP.NET 10, clean architecture principles, and MySQL.

## Documentation
- Business and technical requirements: `InsurancePlatform_BRD_TSD.md`
- Sprint execution plan: `Sprint_Backlog_Plan.md`
- Sprint 0/1 detailed stories and API contracts: `Sprint0_1_UserStories_API_Contracts.md`
- Delivery tracking template: `Implementation_Status_Tracker.md`
- Sprint 3 completion handoff: `Sprint3_Completion_Handoff.md`
- Sprint 4 completion handoff: `Sprint4_Completion_Handoff.md`
- Sprint 5 completion handoff: `Sprint5_Completion_Handoff.md`
- Sprint 6 completion handoff: `Sprint6_Completion_Handoff.md`
- Release 1 candidate notes: `Release1_Release_Notes.md`
- Release 1 commit plan: `Release1_Commit_Plan.md`

## Solution Structure
- `src/InsuranceApp.Web`: ASP.NET MVC web portal
- `src/InsuranceApp.Api`: ASP.NET Core Web API for mobile and partner integrations
- `src/InsuranceApp.Application`: application layer services and use case orchestration
- `src/InsuranceApp.Domain`: core domain entities and enums
- `src/InsuranceApp.Infrastructure`: EF Core MySQL persistence and infrastructure wiring
- `src/InsuranceApp.Contracts`: shared contracts and DTO boundaries
- `src/InsuranceApp.Workers`: background jobs (payments, retries, notifications)
- `tests/InsuranceApp.Tests`: unit and integration test project

## Quick Start
1. Ensure local MySQL is running on localhost:3306 with:
	- username: `root`
	- password: `qwerty123`
2. Update MySQL connection string in:
	- `src/InsuranceApp.Api/appsettings.json`
	- `src/InsuranceApp.Web/appsettings.json`
	- `src/InsuranceApp.Workers/appsettings.json`
	Default local values are preconfigured for localhost.
3. Restore and build:
	- `dotnet restore`
	- `dotnet build InsuranceApp.sln`
4. Apply migrations:
	- `dotnet ef database update --project src/InsuranceApp.Infrastructure --startup-project src/InsuranceApp.Api --context InsuranceDbContext`
5. Run API:
	- `dotnet run --project src/InsuranceApp.Api`
6. Run Web:
	- `dotnet run --project src/InsuranceApp.Web`
7. Run Workers:
	- `dotnet run --project src/InsuranceApp.Workers`

## Current API Endpoints
- `POST /api/v1/auth/register`
- `POST /api/v1/auth/login`
- `POST /api/v1/auth/refresh`
- `POST /api/v1/auth/request-otp`
- `POST /api/v1/auth/verify-otp`
- `POST /api/v1/auth/revoke-all`
- `POST /api/v1/kyc/verify-ghana-card`
- `POST /api/v1/quotes/generate`
- `GET /api/v1/quotes/{quoteReference}`
- `GET /api/v1/quotes?productCode=&status=&fromUtc=&toUtc=`
- `POST /api/v1/quotes/{quoteReference}/reprice`
- `POST /api/v1/quotes/{quoteReference}/expire`
- `POST /api/v1/policies/issue-from-quote`
- `GET /api/v1/policies/{policyNumber}/document`
- `GET /api/v1/policies/dashboard?customerId=&agentUserId=&status=`
- `POST /api/v1/policies/{policyNumber}/endorse`
- `POST /api/v1/policies/{policyNumber}/cancel`
- `POST /api/v1/policies/renewal-reminders/run` (Admin)
- `POST /api/v1/premium-collections/mandates`
- `POST /api/v1/premium-collections/{policyNumber}/schedule`
- `GET /api/v1/premium-collections/{policyNumber}`
- `POST /api/v1/premium-collections/run-due` (Admin)
- `POST /api/v1/premium-collections/retry-failed` (Admin)
- `POST /api/v1/premium-collections/webhooks/provider` (AllowAnonymous, signature required)
- `GET /api/v1/premium-collections/reconciliation?fromUtc=&toUtc=` (Admin)
- `POST /api/v1/claims`
- `GET /api/v1/claims/{claimNumber}`
- `GET /api/v1/claims?status=&assignedAdjusterId=&policyNumber=`
- `POST /api/v1/claims/{claimNumber}/assign` (Admin/Agent)
- `POST /api/v1/claims/{claimNumber}/review` (Admin/Agent)
- `GET /api/v1/claims/{claimNumber}/timeline`
- `GET /api/v1/claims/sla-dashboard?assignedAdjusterId=` (Admin/Agent)
- `POST /api/v1/payouts/initiate` (Admin/Agent)
- `GET /api/v1/payouts/{payoutReference}`
- `GET /api/v1/payouts?status=&claimNumber=&fromUtc=&toUtc=`
- `POST /api/v1/payouts/reconcile/run` (Admin)
- `POST /api/v1/payouts/webhooks/provider` (AllowAnonymous, signature required)
- `GET /api/v1/admin/compliance-reports/summary`
- `GET /api/v1/admin/compliance-reports/summary/export`
- `GET /api/v1/admin/data-retention/summary`
- `POST /api/v1/admin/data-retention/run`
- `GET /api/v1/admin/operations-hardening/readiness`
- `POST /api/v1/admin/operations-hardening/failover-drill/run`
- `GET /api/v1/admin/products`
- `POST /api/v1/admin/products`
- `PUT /api/v1/admin/products/{id}`
- `DELETE /api/v1/admin/products/{id}`
- `GET /api/v1/admin/products/{id}/risk-rules`
- `POST /api/v1/admin/products/{id}/risk-rules`
- `PUT /api/v1/admin/products/{id}/risk-rules/{ruleId}`
- `DELETE /api/v1/admin/products/{id}/risk-rules/{ruleId}`
- `GET /api/v1/admin/products/risk-rules`
- `POST /api/v1/admin/products/risk-rules/bulk-upsert`
- `GET /api/v1/admin/products/{id}/riders`
- `POST /api/v1/admin/products/{id}/riders`
- `PUT /api/v1/admin/products/{id}/riders/{riderId}`
- `DELETE /api/v1/admin/products/{id}/riders/{riderId}`
- `GET /api/v1/admin/audit-logs`
- `GET /api/v1/admin/audit-logs/export`
- `GET /api/v1/health`

### Mobile Quote Contract Notes
- Quote request supports optional `selectedRiderCodes` for rider-aware pricing.
- Quote response includes `appliedRiders` entries with rider code, name, and applied amount.
- Quote lifecycle endpoints support retrieval, filtered listing, repricing, and explicit expiry transitions.

### Policy Communication Notes
- Policy issuance dispatches template-driven notifications across Email, SMS, and WhatsApp channels.
- Renewal reminder cycle scans active policies expiring in 30 days and dispatches reminders with anti-spam guardrails.

### Web Portal Notes
- Login page: `/Account/Login` (cookie-based session for MVC).
- Home page now routes users to Policy Admin, Premium Admin, and Product Admin modules.
- Default seeded admin credentials for local development can be configured via `SeedAdmin` settings in web appsettings.
- Premium Admin page includes reconciliation summary and webhook simulation form to validate idempotency behavior.
- Claims Admin page supports intake, assignment, and review decisions for adjuster workflows.
- Claims Admin includes SLA dashboard metrics and timeline lookup for end-to-end claim visibility.
- Payout Admin supports payout initiation, reconciliation runs, and webhook simulation for payout status updates.
- Compliance Admin provides operational compliance KPIs and exception export across premiums, payouts, claims, and identity audit logs.
- Data Retention Admin provides retention preview and one-click execution for cleanup, anonymization, and webhook payload redaction policies.
- Operations Admin provides readiness snapshots and controlled failover drill execution with step-level outcomes.

## Next Engineering Steps
- Integrate real NIA adapter and OTP provider.
- Add token revocation endpoint and rotate refresh tokens on every use.
- Build out quote, policy, claims, and payment use cases by sprint plan.

## Security and Resilience Notes
- Refresh tokens are single-use and rotated on refresh.
- Session/device metadata is captured and validated during refresh.
- OTP requests have throttling and lockout controls.
- KYC integration has retry and circuit-breaker fallback to `Pending` status when NIA is unavailable.
