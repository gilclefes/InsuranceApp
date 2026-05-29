# Release 1 Commit Plan

Date: 29 May 2026
Branch: main
Objective: Package current Sprint 4-6 delivery into clean, reviewable commits without losing existing workspace changes.

## Pre-Commit Safety
1. Verify working tree:
   - git status --short
2. Optional safety checkpoint:
   - git stash push -u -m "pre-release1-packaging"
   - git stash pop

## Commit Grouping Strategy

### Commit 1 - Premium Collection Hardening (Sprint 4 close)
Message:
feat(premium): add webhook idempotency and reconciliation baseline

Stage:
- src/InsuranceApp.Api/Controllers/PremiumCollectionsController.cs
- src/InsuranceApp.Application/Interfaces/IPremiumCollectionService.cs
- src/InsuranceApp.Contracts/PremiumCollections/PremiumCollectionWebhookRequest.cs
- src/InsuranceApp.Contracts/PremiumCollections/PremiumCollectionWebhookResponse.cs
- src/InsuranceApp.Contracts/PremiumCollections/PremiumReconciliationSummaryResponse.cs
- src/InsuranceApp.Domain/Entities/PremiumTransaction.cs
- src/InsuranceApp.Domain/Entities/PremiumCollectionWebhookLog.cs
- src/InsuranceApp.Infrastructure/Payments/PremiumCollectionService.cs
- src/InsuranceApp.Web/Controllers/PremiumAdminController.cs
- src/InsuranceApp.Web/Models/PremiumAdminViewModel.cs
- src/InsuranceApp.Web/Views/PremiumAdmin/Index.cshtml
- src/InsuranceApp.Workers/Worker.cs
- tests/InsuranceApp.Tests/ApiContracts/PremiumCollectionsControllerContractTests.cs
- tests/InsuranceApp.Tests/Payments/PremiumCollectionServiceTests.cs

Command template:
- git add <paths above>
- git commit -m "feat(premium): add webhook idempotency and reconciliation baseline"

### Commit 2 - Claims Workflow and Fraud/SLA (Sprint 5)
Message:
feat(claims): implement adjudication workflow fraud timeline and SLA dashboard

Stage:
- src/InsuranceApp.Api/Controllers/ClaimsController.cs
- src/InsuranceApp.Application/Interfaces/IClaimService.cs
- src/InsuranceApp.Contracts/Claims/
- src/InsuranceApp.Domain/Entities/Claim.cs
- src/InsuranceApp.Domain/Entities/ClaimTimelineEvent.cs
- src/InsuranceApp.Domain/Entities/ClaimNotification.cs
- src/InsuranceApp.Infrastructure/Claims/
- src/InsuranceApp.Web/Controllers/ClaimsAdminController.cs
- src/InsuranceApp.Web/Models/ClaimsAdminViewModel.cs
- src/InsuranceApp.Web/Views/ClaimsAdmin/
- tests/InsuranceApp.Tests/ApiContracts/ClaimsControllerContractTests.cs
- tests/InsuranceApp.Tests/Claims/

Command template:
- git add <paths above>
- git commit -m "feat(claims): implement adjudication workflow fraud timeline and SLA dashboard"

### Commit 3 - Payout Workflow (Sprint 6 part 1)
Message:
feat(payouts): add payout initiation reconciliation and webhook handling

Stage:
- src/InsuranceApp.Api/Controllers/PayoutsController.cs
- src/InsuranceApp.Application/Interfaces/IPayoutService.cs
- src/InsuranceApp.Contracts/Payouts/
- src/InsuranceApp.Domain/Entities/PayoutTransaction.cs
- src/InsuranceApp.Domain/Entities/PayoutWebhookLog.cs
- src/InsuranceApp.Infrastructure/Payouts/
- src/InsuranceApp.Web/Controllers/PayoutAdminController.cs
- src/InsuranceApp.Web/Models/PayoutAdminViewModel.cs
- src/InsuranceApp.Web/Views/PayoutAdmin/
- tests/InsuranceApp.Tests/ApiContracts/PayoutsControllerContractTests.cs
- tests/InsuranceApp.Tests/Payouts/

Command template:
- git add <paths above>
- git commit -m "feat(payouts): add payout initiation reconciliation and webhook handling"

### Commit 4 - Compliance and Privacy Controls (Sprint 6 part 2)
Message:
feat(compliance): add reporting exports and privacy retention controls

Stage:
- src/InsuranceApp.Api/Controllers/ComplianceReportsController.cs
- src/InsuranceApp.Api/Controllers/DataRetentionController.cs
- src/InsuranceApp.Application/Interfaces/IComplianceReportingService.cs
- src/InsuranceApp.Application/Interfaces/IDataRetentionService.cs
- src/InsuranceApp.Contracts/Compliance/
- src/InsuranceApp.Contracts/Privacy/
- src/InsuranceApp.Infrastructure/Compliance/
- src/InsuranceApp.Infrastructure/Privacy/
- src/InsuranceApp.Web/Controllers/ComplianceAdminController.cs
- src/InsuranceApp.Web/Controllers/DataRetentionAdminController.cs
- src/InsuranceApp.Web/Models/ComplianceAdminViewModel.cs
- src/InsuranceApp.Web/Models/DataRetentionAdminViewModel.cs
- src/InsuranceApp.Web/Views/ComplianceAdmin/
- src/InsuranceApp.Web/Views/DataRetentionAdmin/
- tests/InsuranceApp.Tests/ApiContracts/ComplianceReportsControllerContractTests.cs
- tests/InsuranceApp.Tests/ApiContracts/DataRetentionControllerContractTests.cs
- tests/InsuranceApp.Tests/Compliance/
- tests/InsuranceApp.Tests/Privacy/

Command template:
- git add <paths above>
- git commit -m "feat(compliance): add reporting exports and privacy retention controls"

### Commit 5 - Operations Hardening and Scheduling (Sprint 6 part 3)
Message:
feat(ops): add readiness checks failover drill and scheduled retention cycle

Stage:
- src/InsuranceApp.Api/Controllers/OperationsHardeningController.cs
- src/InsuranceApp.Application/Interfaces/IOperationsHardeningService.cs
- src/InsuranceApp.Contracts/Operations/
- src/InsuranceApp.Infrastructure/Operations/
- src/InsuranceApp.Web/Controllers/OperationsAdminController.cs
- src/InsuranceApp.Web/Models/OperationsAdminViewModel.cs
- src/InsuranceApp.Web/Views/OperationsAdmin/
- tests/InsuranceApp.Tests/ApiContracts/OperationsHardeningControllerContractTests.cs
- tests/InsuranceApp.Tests/Operations/
- src/InsuranceApp.Workers/Worker.cs

Command template:
- git add <paths above>
- git commit -m "feat(ops): add readiness checks failover drill and scheduled retention cycle"

### Commit 6 - Database Migrations Consolidation
Message:
chore(db): add migrations for premium claims and payout workflow extensions

Stage:
- src/InsuranceApp.Infrastructure/Persistence/Migrations/20260529123156_AddPremiumWebhookAndReconciliation.cs
- src/InsuranceApp.Infrastructure/Persistence/Migrations/20260529123156_AddPremiumWebhookAndReconciliation.Designer.cs
- src/InsuranceApp.Infrastructure/Persistence/Migrations/20260529124509_AddClaimsWorkflowFields.cs
- src/InsuranceApp.Infrastructure/Persistence/Migrations/20260529124509_AddClaimsWorkflowFields.Designer.cs
- src/InsuranceApp.Infrastructure/Persistence/Migrations/20260529125949_AddClaimsFraudTimelineAndSla.cs
- src/InsuranceApp.Infrastructure/Persistence/Migrations/20260529125949_AddClaimsFraudTimelineAndSla.Designer.cs
- src/InsuranceApp.Infrastructure/Persistence/Migrations/20260529132126_AddPayoutWorkflowAndReconciliation.cs
- src/InsuranceApp.Infrastructure/Persistence/Migrations/20260529132126_AddPayoutWorkflowAndReconciliation.Designer.cs
- src/InsuranceApp.Infrastructure/Persistence/Migrations/InsuranceDbContextModelSnapshot.cs
- src/InsuranceApp.Infrastructure/Persistence/InsuranceDbContext.cs

Command template:
- git add <paths above>
- git commit -m "chore(db): add migrations for premium claims and payout workflow extensions"

### Commit 7 - Documentation and Handoffs
Message:
docs(release): close sprint 6 and update release readiness artifacts

Stage:
- README.md
- Sprint_Backlog_Plan.md
- Implementation_Status_Tracker.md
- Sprint4_Completion_Handoff.md
- Sprint5_Completion_Handoff.md
- Sprint6_Completion_Handoff.md
- src/InsuranceApp.Web/Views/Home/Index.cshtml
- src/InsuranceApp.Web/Views/Shared/_Layout.cshtml

Command template:
- git add <paths above>
- git commit -m "docs(release): close sprint 6 and update release readiness artifacts"

## Verification After Each Commit
- dotnet build InsuranceApp.sln
- dotnet test InsuranceApp.sln

## Final Verification
1. git log --oneline -n 10
2. dotnet test InsuranceApp.sln
3. git status --short
