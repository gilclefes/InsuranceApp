# Engineering Governance

## Branch Strategy
- `main`: protected production branch.
- Feature branches: `feature/<area>-<short-description>`.
- Bugfix branches: `fix/<area>-<short-description>`.
- Hotfix branches: `hotfix/<short-description>`.

## Pull Request Rules
- Minimum one reviewer approval.
- CI checks (restore, build, test) must pass.
- No direct pushes to `main`.
- Database migration PRs must include rollback notes.

## Commit Conventions
- Preferred prefixes:
  - `feat:` new functionality
  - `fix:` bug fix
  - `chore:` maintenance/tooling
  - `docs:` documentation
  - `refactor:` non-functional code restructuring
  - `test:` test-only changes

## Definition of Done
- Code compiles and tests pass.
- Security and compliance checks reviewed for changed scope.
- API/documentation updated for behavior changes.
- Status tracker updated with delivery progress.

## Coding Standards
- C# nullable enabled for all projects.
- Keep domain logic outside controllers.
- Use DI for all external dependencies.
- Avoid hardcoded secrets; use configuration and secret stores.
