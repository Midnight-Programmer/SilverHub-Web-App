# Security Strategy and Requirements

## Purpose

This document defines the baseline security requirements for SilverHub.

The app is a public portfolio/fan site, so the security model is simpler than a business app with sensitive accounts. However, it still needs production-quality hygiene.

## Security goals

- Do not expose secrets.
- Do not allow unintended database access.
- Do not allow uncontrolled expensive API usage.
- Keep AWS permissions limited.
- Keep frontend and asset buckets protected behind CloudFront.
- Keep dependencies updated.
- Make production deployment controlled and auditable.
- Avoid storing unnecessary personal data.

## Threat model

Likely risks:

- Public users submitting malformed API requests.
- Expensive tool endpoints being abused.
- Stored payloads containing unexpected content.
- Secrets accidentally committed to Git.
- AWS credentials leaked through GitHub Actions.
- S3 buckets accidentally made public.
- Database exposed publicly.
- Dependency vulnerabilities.
- CI/CD workflow misuse.
- Excessive logging of user-submitted data.
- Unexpected AWS cost from abuse or misconfiguration.

Lower-priority risks for initial version:

- Account takeover, because there are no user accounts initially.
- Payment fraud, because there are no payments.
- Complex authorization bugs, unless admin/editor features are added later.

## Authentication and authorization

Initial public app:

- No user login required.
- Public read access to content.
- Public access to safe tool endpoints.
- Write endpoints limited to low-risk actions such as creating team builder share links.

Future admin/editor features require:

- Authentication.
- Role-based authorization.
- Audit logs.
- CSRF consideration if cookie-based auth is used.
- Admin routes separated from public routes.

Do not add admin functionality casually. It significantly increases security scope.

## Secrets management

Secrets must not be committed.

Secrets include:

- Database password.
- Full production connection string.
- AWS credentials.
- API keys.
- Signing keys.
- Admin credentials.

Use:

```text
AWS Secrets Manager
or
AWS SSM Parameter Store SecureString
```

GitHub Actions should use OIDC to assume AWS roles instead of storing long-lived AWS access keys.

Frontend environment files must not contain secrets because frontend bundles are public.

## AWS IAM

Use least privilege.

Separate roles:

```text
GitHub CI role
GitHub staging deploy role
GitHub production deploy role
API runtime role
Terraform apply role
```

Rules:

- CI should not have production deploy permissions.
- Production deploy should only be assumable by protected workflow/environment.
- API runtime role should only access resources it needs.
- Avoid wildcard `*` permissions where practical.
- Review Terraform IAM policies in PRs.

## Network security

### Database

Production database is not publicly accessible. RDS is placed in private subnets with no internet route, and only the ECS task security group can reach it.

Rules:

- RDS accepts PostgreSQL traffic only from the ECS task security group.
- No public `0.0.0.0/0` access to port `5432`.
- Database password stored in Secrets Manager/SSM.
- RDS storage encryption enabled.
- Backups enabled.
- Deletion protection enabled in production.

### API

API should accept HTTPS traffic only through its managed endpoint/custom domain.

CORS must be restricted to allowed frontend origins.

Allowed origins example:

```text
https://silverhub.app
https://www.silverhub.app
https://staging.silverhub.app
```

Avoid:

```text
AllowAnyOrigin in production
```

### S3/CloudFront

S3 buckets should block public access.

CloudFront should access S3 through OAC.

Users should not be able to bypass CloudFront and read from the S3 bucket URL.

## API security

### Input validation

All public endpoints must validate:

- required fields
- string lengths
- numeric ranges
- enum values
- payload size
- array sizes
- nested object limits

Important endpoints:

```text
POST /api/v1/tools/gear-optimization
POST /api/v1/team-builder/shares
```

### Rate limiting

Add rate limiting for:

- gear optimization
- team builder share creation
- any future write endpoint

Initial policy examples:

```text
gear optimization:
  stricter rate limit due to CPU cost

team builder share creation:
  moderate rate limit due to DB/storage writes

read endpoints:
  generous rate limit or CDN caching
```

### Request size limits

Set maximum request body size.

Tool endpoints should reject large payloads before doing expensive work.

### Timeouts

Heavy calculations should have time limits.

If a calculation cannot complete within a reasonable time, return a controlled error.

### Caching

Cache expensive deterministic calculations where possible.

Cache keys should be based on normalized input and algorithm version.

## Error handling security

Production errors must not expose:

- stack traces
- connection strings
- SQL
- internal file paths
- secrets
- raw exception dumps

Use ProblemDetails with trace id.

Log full exception server-side.

## Logging security

Do not log:

- secrets
- authorization headers
- cookies
- database connection strings
- full request bodies by default
- full team builder payloads by default
- user-submitted notes unless redacted

Safe logging:

- route
- method
- status code
- elapsed time
- trace id
- request size
- payload version
- input hash
- cache hit/miss

## Dependency security

Use dependency scanning.

GitHub features:

- Dependabot alerts.
- Dependabot security updates.
- CodeQL if useful.
- npm audit in CI if configured carefully.
- dotnet package vulnerability checks.

CI should fail for serious known vulnerabilities once the workflow is stable.

## CI/CD security

Rules:

- Do not run production deployments from untrusted pull request code.
- Do not expose AWS credentials to PRs from forks.
- Use GitHub environments for production approval.
- Use OIDC with restricted trust policy.
- Pin third-party GitHub Actions to trusted versions.
- Review workflow changes carefully.
- Terraform apply requires protected approval.

## Content security

Content imported from files should be sanitized or rendered safely.

If Markdown is rendered:

- Sanitize HTML.
- Disable raw HTML unless explicitly safe.
- Avoid script injection.
- Treat content files as trusted only if they are reviewed through PRs.

If user-submitted notes are rendered:

- Escape by default.
- Do not render arbitrary HTML.
- Validate length and characters.
- Consider stripping control characters.

## Frontend security

- No secrets in frontend code.
- Use HTTPS-only production URLs.
- Avoid dangerously setting inner HTML.
- Sanitize Markdown/HTML.
- Add security headers through CloudFront where practical.

Recommended headers:

```text
Strict-Transport-Security
X-Content-Type-Options
Referrer-Policy
Content-Security-Policy
Permissions-Policy
```

CSP can be added gradually to avoid breaking the app.

## Data privacy

Avoid collecting personal data.

Current allowed data:

- public team builder share payload
- optional player name fields
- optional notes

Rules:

- Explain that share links are public to anyone with the link if UI exposes sharing.
- Allow pruning/expiration if needed.
- Do not collect email addresses unless accounts are added later.
- Do not store analytics identifiers unless intentionally chosen.

## Backups and deletion protection

Production requirements:

- RDS automated backups enabled.
- RDS deletion protection enabled.
- S3 versioning enabled where useful.
- Terraform state bucket versioning enabled.
- Manual snapshot before risky migrations.

## Security checklist

### AWS

- [ ] S3 public access blocked.
- [ ] CloudFront uses OAC for private S3 buckets.
- [ ] RDS is not publicly accessible where practical.
- [ ] RDS encrypted storage enabled.
- [ ] RDS backups enabled.
- [ ] RDS deletion protection enabled in production.
- [ ] IAM roles are least-privilege.
- [ ] GitHub Actions uses OIDC.
- [ ] AWS Budgets configured.

### API

- [ ] CORS restricted in production.
- [ ] Request validation exists.
- [ ] Rate limiting exists for expensive/write endpoints.
- [ ] Request body size limits exist.
- [ ] ProblemDetails hides internals.
- [ ] Logs do not contain secrets or full payloads.
- [ ] Health endpoints do not expose sensitive data.
- [ ] Swagger is disabled in production.

### Frontend

- [ ] No secrets in frontend config.
- [ ] Markdown/HTML rendering is sanitized.
- [ ] Security headers configured.
- [ ] Error pages do not expose internals.
- [ ] Build output does not include environment secrets.

### CI/CD

- [ ] Protected branch required.
- [ ] Required checks enabled.
- [ ] Production environment approval enabled.
- [ ] Workflow permissions are minimal.
- [ ] Terraform apply protected.
- [ ] Dependency scanning enabled.

## Future security improvements

- WAF on CloudFront/API if abuse appears.
- Auth for admin/editor features.
- Audit log for content edits.
- OpenTelemetry tracing with sensitive-data filters.
- Security-focused E2E tests.
- Automated dependency update PRs.
