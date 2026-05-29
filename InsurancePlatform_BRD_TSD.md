# Business Requirement Document (BRD) and Technical Specification Document (TSD)
## Digital Insurance Management Platform - Ghana

Version: 1.0
Date: 29 May 2026
Prepared for: Product, Engineering, Operations, Compliance, and Executive Stakeholders
Target Stack: ASP.NET MVC 10 (C#), ASP.NET Core 10 Web API, MySQL 8

---

## 1. Executive Summary and Project Scope

### 1.1 Project Overview
This project delivers a unified digital insurance platform for Ghana that supports public self-enrollment, policy management, claims automation, and premium collection across web and mobile channels. The platform will modernize insurance operations by replacing fragmented manual processes with secure, API-driven workflows that integrate directly with national ID, payment rails, and regulator-facing services.

The solution is designed to:
- Increase policy acquisition through mobile-first onboarding.
- Reduce claim settlement cycle times via workflow automation.
- Improve premium collection performance through recurring payment mandates.
- Ensure compliance with Ghanaian insurance and data protection regulations.
- Provide operational transparency using real-time dashboards and audit trails.

### 1.2 Vision Statement
Deliver a trusted, scalable, and regulation-compliant insurance ecosystem where Ghanaian individuals and businesses can buy, manage, and claim insurance digitally with minimal friction.

### 1.3 Target Market
Primary audiences:
- Retail consumers in urban and rural Ghana.
- SME and corporate policyholders.
- Insurance intermediaries (agents and brokers).

Market characteristics considered:
- Mobile-first behavior and strong Mobile Money penetration.
- Mix of smartphone and low-bandwidth users.
- High demand for fast, low-friction claims experience.
- Need for transparent and flexible premium collection options.

### 1.4 Product Scope
In-scope insurance products:
1. Motor Insurance
- Third-Party Motor (aligned to statutory NIC expectations).
- Comprehensive Motor.
- MID integration support for motor verification/sticker workflows where applicable.

2. Educational Policies
- Long-term savings + protection plans.
- Configurable contribution frequency (monthly, quarterly, annual).
- Maturity and beneficiary workflows.

3. Funeral Policies
- High-volume micro-insurance model.
- Simplified onboarding and claim documentation checks.
- Fast disbursement workflows and beneficiary validation.

4. Custom Policies
- Product builder for modular covers, riders, and add-ons.
- Rule-driven underwriting and premium computation.

### 1.5 Business Objectives
- Achieve at least 60% digital policy enrollment within 18 months.
- Reduce average claims turnaround time by 40% in year 1.
- Achieve 95% automated premium collection success (inclusive of retries).
- Ensure 100% auditable transaction trail for policy, claims, and financial operations.

### 1.6 Out of Scope (Phase 1)
- Full reinsurance treaty administration.
- Cross-border insurance products.
- Advanced AI fraud scoring (deferred to phase 2, rule-based fraud controls included in phase 1).

---

## 2. Business Requirements and User Journeys

### 2.1 User Personas
1. Public Applicant
- Registers, verifies identity, requests quote, purchases policy.

2. Policyholder
- Views active policies, pays premiums, raises claims, tracks status.

3. Insurance Agent/Broker
- Captures assisted enrollments, manages client portfolios, tracks commissions.

4. Claims Adjuster
- Reviews evidence, requests additional documents, recommends settlement.

5. Underwriter
- Approves risk, applies loadings/discounts, defines exclusions.

6. Super Administrator
- Manages users, products, integrations, audit policies, and system configurations.

### 2.2 Functional Business Requirements

#### 2.2.1 Customer Onboarding and Policy Subscription
- FR-001: System shall support self-service registration through web and mobile apps.
- FR-002: System shall validate Ghana Card details via NIA integration.
- FR-003: System shall support OTP verification through SMS/Email.
- FR-004: System shall support assisted onboarding by agents/brokers.
- FR-005: System shall generate dynamic quotes based on risk parameters (vehicle value, age, location, claims history proxy, coverage limits, rider selection).
- FR-006: System shall support quote-to-policy conversion with digital acceptance.
- FR-007: System shall issue policy documents digitally in PDF and send through Email, SMS link, and WhatsApp.
- FR-008: System shall support e-sign consent capture and timestamping.

#### 2.2.2 Policy Administration
- FR-009: System shall support endorsements, renewals, and cancellations.
- FR-010: System shall compute pro-rata premium adjustments for mid-term policy changes.
- FR-011: System shall schedule renewal reminders and auto-renew attempts for eligible policies.

#### 2.2.3 Claims Lifecycle
- FR-012: System shall allow claim initiation through web/mobile.
- FR-013: System shall support evidence upload (images, PDFs, certificates, reports).
- FR-014: System shall classify claims by product line and severity.
- FR-015: System shall route claims to adjusters/underwriters based on configurable rules.
- FR-016: System shall track claim states: Filed, Under Review, Awaiting Documents, Approved, Rejected, Disbursed, Closed.
- FR-017: System shall generate settlement advice and trigger disbursement workflows.
- FR-018: System shall maintain immutable claim audit logs.

#### 2.2.4 Notifications and Communications
- FR-019: System shall notify customers of policy events, payment reminders, claim updates, and settlement outcomes.
- FR-020: System shall support communication templates by channel (SMS, Email, WhatsApp).

### 2.3 User Journey Flows

#### 2.3.1 Digital Enrollment Journey
1. User selects product.
2. User enters personal details and Ghana Card number.
3. Platform validates identity with NIA.
4. User enters risk details.
5. Pricing engine returns quote options.
6. User selects plan and payment mandate option.
7. Payment authorization completed via MoMo/bank.
8. Policy is issued and distributed digitally.
9. Policy appears in customer dashboard.

#### 2.3.2 Claims Journey
1. Policyholder submits claim with incident details.
2. Evidence upload and validation checks run.
3. Claims engine performs triage and assignment.
4. Adjuster review and decision recommendation.
5. Underwriter or supervisor approval (if threshold exceeded).
6. Payout instruction sent to payment partner.
7. Status updated to Disbursed and customer notified.

### 2.4 Business Rules
- BR-001: Active policy and paid-up status required to submit non-exempt claims.
- BR-002: Motor claims require mandatory accident evidence and police report for defined thresholds.
- BR-003: Funeral claims require beneficiary and death certificate verification.
- BR-004: Premium grace period defaults to 15 days (configurable per product).
- BR-005: Policy suspension occurs after grace period expires with failed retries.
- BR-006: Claims above configurable threshold require dual approval.
- BR-007: All underwriting decisions must be versioned and auditable.

### 2.5 Reporting Requirements
- Operational reports: policy sales, renewals, lapse ratio, collection rate, claims aging, claim ratio.
- Finance reports: premium due vs collected, failed mandates, outstanding liabilities, payout ledger.
- Compliance reports: KYC completion, audit trail exports, data access logs, regulator submissions.

---

## 3. Financial Integrations and Automated Premium Collection (Ghana Ecosystem)

### 3.1 Integration Principles
- API-first architecture with adapter pattern per payment provider.
- Idempotent payment requests and webhook handlers.
- End-to-end traceability using transaction correlation IDs.
- Reconciliation support with daily settlement files/API polling.

### 3.2 Payment Gateway and Aggregator Integrations
Supported provider classes (phase 1):
- Hubtel
- Paystack
- Flutterwave
- Zeepay

Requirements:
- FI-001: Initiate one-time and recurring collections.
- FI-002: Validate transaction status asynchronously via webhooks.
- FI-003: Handle reversals and refunds.
- FI-004: Provide settlement and fee breakdown per transaction.
- FI-005: Support fallback routing if primary provider fails.

### 3.3 Mobile Money Auto-Deduction Mandates
Networks:
- MTN Mobile Money
- Telecel Cash
- AT Money

Requirements:
- FI-006: Capture customer consent and mandate reference.
- FI-007: Tokenize and store provider mandate IDs securely.
- FI-008: Trigger recurring debit jobs via scheduler.
- FI-009: Retry failed debits using policy-defined cadence.
- FI-010: Notify users before and after debit attempts.

### 3.4 Bank Integrations (GhIPSS)
Channels:
- GhIPSS Instant Pay (GIP)
- ACH Direct Debit

Requirements:
- FI-011: Support direct debit setup and mandate status tracking.
- FI-012: Process monthly premium deductions and callback status.
- FI-013: Reconcile bank returns, rejects, and chargebacks.

### 3.5 Failed Payment Retry Logic
Default retry pattern (configurable):
- Attempt 1: Due date.
- Attempt 2: Due date + 2 days.
- Attempt 3: Due date + 5 days.
- Attempt 4: Due date + 10 days.
- Grace expiry check at day 15.

Rules:
- If all retries fail, policy moves to Suspended state.
- Claims blocked during suspension except policy-specific exceptions.
- Auto-reactivation occurs after successful catch-up payment.

### 3.6 Reconciliation and Ledger Posting
- Every payment event posts to premium ledger with double-entry style references.
- End-of-day reconciliation compares internal transactions with provider settlement data.
- Exceptions generate finance work items for resolution.

---

## 4. Technical Architecture and System Design (ASP.NET 10 and MySQL)

## 4.1 Architectural Decision
Recommended delivery model:
- Phase 1: Modular monolith using clean architecture for faster time-to-market and lower operational complexity.
- Phase 2: Controlled extraction of high-throughput modules (Payments, Claims Automation, Notifications) into independent services when scale and team maturity justify.

Rationale:
- Supports enterprise boundaries now while preserving migration path to microservices.
- Reduces distributed system complexity early in program lifecycle.

### 4.2 Logical Architecture Layers
1. Presentation Layer
- ASP.NET MVC 10 web portals (public + admin).
- Mobile clients (Flutter/React Native) consuming secure APIs.

2. Application Layer
- Use-case orchestration, validation, workflow coordination.

3. Domain Layer
- Core insurance entities, business rules, policy and claim engines.

4. Infrastructure Layer
- EF Core repositories, MySQL persistence, integration adapters, messaging, caching.

5. Cross-Cutting
- Authentication, authorization, logging, auditing, observability, encryption.

### 4.3 Deployment Topology
- API and MVC hosted behind reverse proxy/load balancer.
- MySQL primary-replica setup.
- Redis for distributed cache and background job coordination.
- Object storage for claim evidence documents.
- Message broker (optional phase 1.5+) for resilient async integration processing.

### 4.4 Suggested Solution Structure
- InsuranceApp.Web (ASP.NET MVC)
- InsuranceApp.Api (ASP.NET Core Web API)
- InsuranceApp.Application
- InsuranceApp.Domain
- InsuranceApp.Infrastructure
- InsuranceApp.Contracts
- InsuranceApp.Workers (background services for retries, notifications, reconciliation)

### 4.5 Data Layer Design (MySQL + EF Core)
Primary package:
- Pomelo.EntityFrameworkCore.MySql

Key principles:
- ACID transactions for policy issuance, premium posting, and claim payout.
- Explicit transaction boundaries for financial operations.
- Optimistic concurrency (row versioning approach) on mutable records.
- Strict foreign key constraints and indexed lookup fields.

EF Core configuration patterns:
- Use DbContext pooling.
- Apply migration strategy per environment with gated deployment.
- Use resilient retries for transient DB connectivity errors.

### 4.6 Boilerplate EF Core Entity Schemas

    public class Customer
    {
        public long Id { get; set; }
        public string CustomerNumber { get; set; } = null!;
        public string GhanaCardNumberHash { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public string Email { get; set; } = null!;
        public bool KycVerified { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }

    public class Policy
    {
        public long Id { get; set; }
        public string PolicyNumber { get; set; } = null!;
        public long CustomerId { get; set; }
        public string ProductType { get; set; } = null!; // Motor, Education, Funeral, Custom
        public string CoverageType { get; set; } = null!;
        public decimal PremiumAmount { get; set; }
        public string CurrencyCode { get; set; } = "GHS";
        public DateTime InceptionDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Status { get; set; } = null!; // Draft, Active, Suspended, Lapsed, Cancelled
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }

    public class Claim
    {
        public long Id { get; set; }
        public string ClaimNumber { get; set; } = null!;
        public long PolicyId { get; set; }
        public DateTime IncidentDate { get; set; }
        public string ClaimType { get; set; } = null!;
        public string Status { get; set; } = null!; // Filed, UnderReview, Approved, Rejected, Disbursed, Closed
        public decimal ClaimedAmount { get; set; }
        public decimal? ApprovedAmount { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }

    public class PremiumTransaction
    {
        public long Id { get; set; }
        public long PolicyId { get; set; }
        public string TransactionReference { get; set; } = null!;
        public string Provider { get; set; } = null!;
        public string PaymentChannel { get; set; } = null!; // MoMo, Bank, Card
        public decimal Amount { get; set; }
        public string Status { get; set; } = null!; // Pending, Success, Failed, Reversed
        public DateTime DueDateUtc { get; set; }
        public DateTime? ProcessedAtUtc { get; set; }
        public int RetryCount { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }

    public class PayoutTransaction
    {
        public long Id { get; set; }
        public long ClaimId { get; set; }
        public string PayoutReference { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Status { get; set; } = null!; // Initiated, Processing, Disbursed, Failed
        public string DestinationChannel { get; set; } = null!;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? DisbursedAtUtc { get; set; }
    }

    public class IntegrationWebhookLog
    {
        public long Id { get; set; }
        public string SourceSystem { get; set; } = null!;
        public string EventType { get; set; } = null!;
        public string Signature { get; set; } = null!;
        public string PayloadJson { get; set; } = null!;
        public bool Processed { get; set; }
        public DateTime ReceivedAtUtc { get; set; }
        public DateTime? ProcessedAtUtc { get; set; }
    }

### 4.7 Core MySQL Table and Index Recommendations
- customers: unique index on customer_number, index on phone_number, email.
- policies: unique index on policy_number, composite index on customer_id + status.
- claims: unique index on claim_number, index on policy_id + status + created_at_utc.
- premium_transactions: unique index on transaction_reference, index on policy_id + due_date_utc + status.
- payout_transactions: unique index on payout_reference, index on claim_id + status.
- webhook_logs: index on source_system + event_type + received_at_utc.

### 4.8 API Management and Endpoint Design
API style:
- Versioned REST endpoints under /api/v1.

Core endpoints:
- POST /auth/register
- POST /auth/login
- POST /kyc/verify-ghana-card
- POST /quotes/generate
- POST /policies/issue
- GET /policies/{policyNumber}
- POST /claims
- POST /claims/{claimNumber}/documents
- GET /claims/{claimNumber}/timeline
- POST /payments/mandates
- POST /payments/collect
- POST /webhooks/{provider}

Design requirements:
- OAuth/JWT bearer for mobile and external clients.
- Idempotency-Key header for financial mutation endpoints.
- Correlation-Id header for traceability.
- Rate limiting and request signature validation for webhooks.

### 4.9 Workflow Automation Patterns
- Background worker for payment retries and renewals.
- State machine pattern for claim and policy status transitions.
- Outbox pattern for reliable event publishing to notifications and integrations.

---

## 5. Regulatory Compliance and Security (Ghana Context)

### 5.1 NIC Compliance
- Align policy wording and underwriting operations to NIC guidelines.
- Provide report exports required for regulator oversight.
- Support Motor Insurance Database workflows for motor products where required.

### 5.2 Data Protection Act, 2012 (Act 843)
Requirements:
- Lawful basis and consent capture for personal data processing.
- Data minimization and purpose limitation.
- Retention schedules with purge/anonymization workflows.
- Data subject rights handling (access, correction, deletion where legally permissible).

### 5.3 KYC and Identity Verification
- Real-time NIA integration for Ghana Card verification.
- Store only required identity attributes.
- Sensitive identifiers stored hashed/encrypted, not plain text.

### 5.4 Security Controls
Authentication and authorization:
- ASP.NET Identity for user lifecycle and password policies.
- RBAC with role hierarchies (Customer, Agent, Adjuster, Underwriter, Admin).
- MFA for privileged accounts.

API security:
- JWT access tokens with short TTL and refresh token rotation.
- HMAC signature validation for incoming provider webhooks.
- TLS 1.2+ end-to-end.

Data security:
- Column-level encryption for high-sensitivity fields.
- Key management through secure vault service.
- Database access via least privilege accounts.

Audit and monitoring:
- Immutable audit logs for sensitive actions.
- SIEM integration for anomaly detection.
- Alerting for suspicious login and payment patterns.

---

## 6. Non-Functional Requirements

### 6.1 Performance
- NFR-001: API p95 response time under 400 ms for read operations under normal load.
- NFR-002: Quote generation p95 under 2 seconds.
- NFR-003: Support at least 10,000 daily premium transactions in phase 1.

Optimization strategy:
- Response caching for static/reference endpoints.
- Distributed caching (Redis) for session and computed quote artifacts.
- Query tuning with selective indexes and pagination.

### 6.2 Availability and Disaster Recovery
- NFR-004: 99.9% monthly uptime target.
- NFR-005: RPO <= 15 minutes, RTO <= 2 hours.

Approach:
- MySQL primary-replica topology.
- Automated backups: full daily, incremental every 15 minutes.
- Periodic restore drills and failover tests.

### 6.3 Scalability
- Horizontal scaling for web/API nodes via load balancer.
- Async job workers scaled independently for retries and notifications.
- Provider adapters decoupled to prevent single integration bottlenecks.

### 6.4 Offline and Low-Bandwidth Support
- Mobile app local queue for draft forms and delayed sync.
- Compressed payloads and image optimization before upload.
- Progressive form save and resume.
- USSD fallback integration architecture for basic policy inquiry, premium reminders, and claim status checks in low-connectivity areas.

### 6.5 Observability and Supportability
- Structured logging with correlation IDs.
- Distributed tracing for transaction flow across APIs and workers.
- Business and technical dashboards for operations center.

---

## 7. Detailed Claims and Payment Workflow Logic

### 7.1 Claims Workflow State Rules
Allowed transitions:
- Filed -> Under Review
- Under Review -> Awaiting Documents
- Awaiting Documents -> Under Review
- Under Review -> Approved
- Under Review -> Rejected
- Approved -> Disbursed
- Disbursed -> Closed

Control points:
- Approval threshold routing by claim amount and product risk class.
- Fraud rule checks before final approval.
- Payout initiation blocked if beneficiary validation is incomplete.

### 7.2 Premium Collection Workflow
1. Generate premium schedule at policy issuance.
2. Pre-debit reminder sent 24 hours before due date.
3. Attempt debit via preferred channel.
4. On failure, execute retry cadence and fallback channel.
5. Update policy status according to grace and suspension rules.
6. Post all events to ledger and reconciliation queues.

### 7.3 Exception Handling Patterns
- Duplicate callbacks handled through idempotency keys.
- Poison message handling with dead-letter queue for repeated integration failures.
- Manual intervention queue for unresolved reconciliation items.

---

## 8. Data Governance and Master Data

### 8.1 Core Master Data Sets
- Product catalog and rider definitions.
- Risk factor tables (vehicle classes, occupation bands, regions).
- Payment provider configuration and fee structures.
- Claims cause and document type catalogs.

### 8.2 Data Quality Controls
- Mandatory field enforcement by product type.
- Rule-based validation for KYC, contact details, and beneficiary mappings.
- Duplicate detection for customer and policy records.

### 8.3 Auditability
- Versioned policy terms and quote assumptions.
- Immutable event trail for financial postings and claim decisions.

---

## 9. Integration Specifications (External Systems)

### 9.1 NIA Integration
- Operation: Verify Ghana Card identity.
- Mode: Synchronous API call during onboarding and key account updates.
- Fail-safe: Queue request and mark KYC Pending when NIA is unavailable.

### 9.2 Payment Providers
- Operation: Collect premium, process payout, receive status callbacks.
- Mode: API request + webhook callback.
- Fail-safe: Retry policy + provider fallback + manual finance queue.

### 9.3 NIC/MID Integration
- Operation: Motor policy verification and sticker process support.
- Mode: API/file exchange depending on regulator interface.
- Fail-safe: Retry with trace logs and compliance alert escalation.

### 9.4 Notification Providers
- Operation: Send OTP, reminders, and status notifications.
- Channels: SMS, Email, WhatsApp.
- Fail-safe: Channel fallback and retry with template failover.

---

## 10. Delivery Plan and Sprint-Ready Epics

### 10.1 Proposed Epics
1. Identity, Authentication, and Access Control
2. Product Configuration and Quote Engine
3. Policy Issuance and Document Delivery
4. Payment Mandates and Premium Collection
5. Claims Intake, Review, and Payout
6. Reporting, Reconciliation, and Audit
7. Compliance and Data Protection Controls
8. Mobile and Low-Bandwidth Experience

### 10.2 MVP Scope (First Release)
- Motor Third-Party and Funeral products.
- Web and mobile self-service onboarding.
- NIA KYC verification.
- MoMo premium collection with retries.
- Basic claims intake and workflow.
- Core dashboards and operational reports.

### 10.3 Sample Acceptance Criteria
- AC-001: Given a valid Ghana Card, when onboarding is submitted, then KYC status becomes Verified within 10 seconds or Pending with clear retry notice.
- AC-002: Given an active mandate, when premium due date arrives, then collection attempt is executed and reflected in policy ledger within 2 minutes.
- AC-003: Given a claim with complete documents, when adjuster approves, then payout instruction is generated and claim status changes to Approved.

---

## 11. Risks, Assumptions, and Dependencies

### 11.1 Key Risks
- External API downtime (NIA, payment providers, regulator systems).
- Regulatory rule changes affecting product and reporting formats.
- Reconciliation complexity across multiple payment channels.

### 11.2 Assumptions
- Required API access and credentials for NIA, payment providers, and GhIPSS are available.
- Product teams provide finalized policy and underwriting rules.
- Legal/compliance team signs off retention and consent models.

### 11.3 Dependencies
- Integration contracts from third-party providers.
- Notification vendor SLAs.
- Infrastructure provisioning and security baseline approvals.

---

## 12. Recommended Engineering Standards

- Coding standards: C# style analyzers and mandatory code review gates.
- Testing: unit, integration, contract, and end-to-end flows for policy and financial transactions.
- CI/CD: automated build, test, migration checks, and environment promotion approvals.
- Release strategy: feature flags for controlled rollout by product and region.

---

## 13. Summary Recommendation

This BRD and TSD define a practical enterprise roadmap for a Ghana-focused digital insurance platform using ASP.NET MVC 10 and MySQL. The proposed architecture enables rapid delivery through a modular monolith while preserving a clear evolution path toward service decomposition. The design emphasizes regulatory compliance, secure payment automation, claims efficiency, and mobile-first usability, making it suitable for immediate sprint planning and phased production rollout.
