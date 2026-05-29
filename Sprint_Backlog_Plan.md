# Sprint Backlog Plan
## InsuranceApp Delivery Roadmap (ASP.NET 10 + MySQL)

Version: 1.0
Date: 29 May 2026
Based on: InsurancePlatform_BRD_TSD.md

## Planning Assumptions
- Sprint length: 2 weeks
- Team composition: 1 Product Owner, 1 Scrum Master, 1 Architect/Lead, 4 Engineers, 1 QA, 1 DevOps
- Estimation scale: Story points (SP)
- Definition of Done: Code complete, peer reviewed, tests passed, security checks passed, deployed to staging

## Program Goals (Release 1)
- Deliver digital onboarding for Motor Third-Party and Funeral products
- Enable Ghana Card (NIA) verification workflow
- Enable premium collections through MoMo with retry logic
- Enable claims initiation, tracking, and payout workflow
- Provide baseline operational and compliance reporting

## Epic Breakdown
1. Identity, Access, and KYC
- Outcome: Secure registration/login with Ghana Card verification and RBAC
- Target SP: 55

2. Product Catalog and Quote Engine
- Outcome: Configurable insurance products and risk-based quote generation
- Target SP: 60

3. Policy Issuance and Document Delivery
- Outcome: End-to-end quote-to-policy issuance with digital documents
- Target SP: 50

4. Premium Collection and Reconciliation
- Outcome: Automated collections (MoMo first), retries, and ledger posting
- Target SP: 75

5. Claims Management and Payouts
- Outcome: Claims intake, review workflow, approval, and disbursement
- Target SP: 70

6. Compliance, Reporting, and Audit
- Outcome: NIC-ready reports, audit logs, and privacy controls
- Target SP: 45

7. Platform Reliability and Operations
- Outcome: Caching, observability, backup/failover, and release pipelines
- Target SP: 55

## Sprint Plan

## Sprint 0: Foundation and Architecture (2 weeks)
Goal: Establish baseline architecture, environments, security skeleton, and CI/CD.

Stories:
- Set up solution structure and clean architecture boundaries (8 SP)
- Configure ASP.NET Identity, JWT, RBAC skeleton (8 SP)
- Configure MySQL connectivity, EF Core migrations baseline (8 SP)
- Establish CI/CD pipeline with build, test, migration checks (8 SP)
- Set up logging, tracing, and correlation IDs (5 SP)
- Define coding standards, branch strategy, PR templates (3 SP)

Deliverables:
- Running baseline web + API + workers
- Initial schema migration
- Staging deployment pipeline

## Sprint 1: Onboarding, KYC, and Authentication
Goal: Deliver secure account onboarding with Ghana Card verification and OTP.

Progress update (29 May 2026):
- Risk-based OTP enforcement added to login flow with device trust policy.
- NIA provider payload mapping and optional response signature-validation scaffolding implemented.
- Quote engine foundation started (product master/risk tables + quote generation endpoint).
- Quote history persistence implemented and validated with unit tests.
- Quote-to-policy issuance service and API endpoint scaffolded for Sprint 3 acceleration.
- Product admin CRUD endpoints added for product/rule management bootstrap.
- Identity action audit logging added for auth and KYC endpoints with correlation and client context.
- API contract tests added for Auth, KYC, and Policies controller response contracts.
- Admin audit log consumption endpoints added for filtered queries and CSV export baseline.
- User profile and consent capture API delivered for onboarding completion.
- Agent-assisted onboarding API delivered with role-gated access for Agent/Admin.
- Sprint 1 stories are now closed with zero carry-over.

Stories:
- Public registration and login flows (8 SP)
- OTP verification via SMS/Email adapters (8 SP)
- NIA verification integration adapter with retry/fallback (13 SP)
- User profile and consent capture screens (8 SP)
- Agent-assisted onboarding flow (8 SP)
- Audit log for identity actions (5 SP)

Acceptance focus:
- KYC status transitions: Pending, Verified, Failed
- JWT token issuance and secure session management

## Sprint 2: Product Setup and Quote Generation
Goal: Enable configurable products and dynamic quote computation.

Progress update (29 May 2026):
- Sprint 2 kickoff completed with admin CRUD APIs for product risk rules (risk factor master data).
- Added contract tests for create/update/delete risk rule endpoints.
- Added dedicated list/filter endpoint for risk factors by product/parameter/adjustment/operator.
- Added bulk upsert workflow for underwriting risk-rule import and update operations.
- Added quote lifecycle APIs for retrieval, listing, repricing, and explicit expiry transitions.
- Added deterministic quote replay tests to confirm identical pricing outcomes for identical inputs.
- Added rider configuration support (API + MVC admin screens) and rider-aware quote pricing flow.
- Sprint 2 stories are now closed with zero carry-over.

Stories:
- Product/rider configuration admin screens (13 SP)
- Risk factor master data management (8 SP)
- Quote engine rules for Motor and Funeral (13 SP)
- Quote API and mobile consumption contract (8 SP)
- Quote versioning and expiry logic (5 SP)
- Unit tests for pricing rules (8 SP)

Acceptance focus:
- Deterministic quote output for identical inputs
- Auditability of quote assumptions

## Sprint 3: Policy Issuance and Communication
Goal: Convert quotes to active policies and deliver documents digitally.

Progress update (29 May 2026):
- Policy issuance flow now persists policy schedule document artifacts.
- Added `GET /api/v1/policies/{policyNumber}/document` for document retrieval.
- Added policy document service/controller tests validating generation and download contracts.
- Added policy dashboard endpoint for customer/agent filtered policy views.
- Added endorsement and cancellation endpoints for baseline lifecycle operations.
- Added multi-channel notification templates and dispatch logs for PolicyIssued and RenewalReminder events.
- Added renewal reminder cycle execution via API trigger and worker background job.
- Sprint 3 stories are now closed with zero carry-over.

Stories:
- Quote acceptance and policy issuance workflow (13 SP)
- Policy numbering and document generation (PDF) (8 SP)
- Multi-channel notification templates (Email/SMS/WhatsApp) (8 SP)
- Renewal schedule setup and reminder jobs (8 SP)
- Endorsement and cancellation baseline (8 SP)
- Policy dashboard for customer and agent (8 SP)

Acceptance focus:
- Policy state machine stability
- Document delivery traceability

## Sprint 4: Premium Mandates and Collections
Goal: Launch recurring premium collection with failure handling and reconciliation.

Progress update (29 May 2026):
- Added premium collection API endpoints for mandate capture, policy-level schedule creation, due-run execution, failed retry execution, and transaction listing.
- Implemented `IPremiumCollectionService` baseline with persisted premium transaction lifecycle handling.
- Added MVC login flow with cookie authentication for user/admin access to operations pages.
- Added MVC policy admin forms for creating policies from quotes and running endorsement/cancellation/document actions.
- Added MVC premium admin forms for mandate capture, schedule setup, due-run trigger, retry trigger, and policy transaction history.
- Added focused API contract and service tests for premium collection behavior.

Stories:
- MoMo mandate capture and token references (13 SP)
- Scheduled collections and retry engine (13 SP)
- Payment webhook verification and idempotency (8 SP)
- Premium ledger postings and transaction history (13 SP)
- Failed payment notifications and suspension rules (8 SP)
- Daily reconciliation report with exception queue (13 SP)

Acceptance focus:
- End-to-end due-to-collected timeline
- Retry logic and grace-period rules validated

## Sprint 5: Claims Intake and Adjudication
Goal: Enable customer claims flow and internal review lifecycle.

Stories:
- Claims initiation and evidence upload (13 SP)
- Claims assignment and adjuster work queue (8 SP)
- Review states and approval workflow (13 SP)
- Fraud rule checks (rule-based baseline) (8 SP)
- Claim notifications and timeline visibility (8 SP)
- Claims SLA dashboard (8 SP)

Acceptance focus:
- Full state transitions from Filed to Closed
- Threshold-based approval routing

## Sprint 6: Payouts, Compliance, and Hardening
Goal: Complete disbursement workflow, compliance reporting, and operational hardening.

Stories:
- Claim payout initiation and status reconciliation (13 SP)
- Compliance reports (KYC, claims, policy ledger extracts) (8 SP)
- Data retention and privacy controls (8 SP)
- Backup/restore automation and failover drill (8 SP)
- Performance tuning and cache optimization (8 SP)
- UAT bug fixes and release readiness (13 SP)

Acceptance focus:
- 99.9% availability controls in place
- UAT sign-off and production go-live checklist complete

## Cross-Sprint Technical Enablers
- Threat modeling and periodic security scanning
- Test automation growth plan (unit, integration, API contract, smoke)
- Feature flagging for phased release by product/channel
- Integration monitoring and provider SLA dashboards

## Release Milestones
1. Milestone A (end Sprint 2): Secure onboarding + quotes demo
2. Milestone B (end Sprint 4): Policy issuance + automated collections
3. Milestone C (end Sprint 6): Claims + payouts + compliance release candidate

## Risks and Mitigation in Sprint Execution
- External API instability (NIA/payment): use adapter retries, circuit breakers, and staging simulators
- Scope pressure: enforce MVP boundary and manage change requests through steering committee
- Data quality drift: add validation and reconciliation checks from Sprint 1

## Initial Backlog Seeds (Ready for Jira Import)
- IAM-001: Register public user with OTP and JWT
- KYC-001: Verify Ghana Card using NIA adapter
- QTE-001: Generate motor third-party quote
- QTE-002: Generate funeral policy quote
- POL-001: Convert quote to policy and issue policy number
- DOC-001: Generate policy PDF and store metadata
- PAY-001: Create MoMo mandate and persist token reference
- PAY-002: Run scheduled premium collection job
- PAY-003: Handle payment webhook idempotently
- PAY-004: Trigger retry and suspension workflow
- CLM-001: Submit claim with evidence upload
- CLM-002: Move claim through review and approval states
- PAYOUT-001: Trigger payout for approved claim
- REP-001: Build daily reconciliation and exception report
- CMP-001: Export KYC and audit report for compliance

## Governance Cadence
- Daily stand-up (15 min)
- Sprint planning (2 hrs)
- Backlog refinement (1 hr weekly)
- Sprint review + demo (1.5 hrs)
- Retrospective (1 hr)

## Exit Criteria for Release 1
- Critical and high defects closed
- Mandatory compliance controls signed off
- Payment success/retry flows proven in staging with test cases
- Observability and alerting dashboards operational
- Operational runbook approved by support and DevOps
