# Production Readiness Checklist

## Purpose

This checklist defines what “production-ready” means for SilverHub.

It is not expected that every item is complete on day one. The checklist should guide implementation and provide a clear portfolio story.

## Status key

Use this status format when tracking readiness:

```text
[ ] Not started
[/] In progress
[x] Complete
[-] Intentionally deferred
```

## Architecture

- [ ] Monorepo structure is clear.
- [ ] Backend projects are separated into Api, Application, Domain, and Infrastructure.
- [ ] Frontend feature folders are organized by domain/feature.
- [ ] Infrastructure is managed by Terraform.
- [ ] Architecture decisions are documented.
- [ ] Local/staging/production environments are defined.
- [ ] Public URLs and domains are documented.
- [ ] Deployment flow is documented.
- [ ] Rollback flow is documented.

## AWS infrastructure

- [ ] Terraform remote state configured.
- [ ] Terraform state bucket has versioning.
- [ ] Terraform state bucket is encrypted.
- [ ] Terraform state bucket blocks public access.
- [ ] S3 frontend bucket created.
- [ ] S3 frontend bucket blocks public access.
- [ ] CloudFront distribution created.
- [ ] CloudFront uses OAC for S3 origin.
- [ ] SPA fallback routing works.
- [ ] Asset S3 bucket created.
- [ ] Asset bucket blocks public access.
- [ ] ECR repository created.
- [ ] ECS cluster, task definition, service, and ALB created.
- [ ] RDS PostgreSQL created.
- [ ] RDS encryption enabled.
- [ ] RDS automated backups enabled.
- [ ] RDS deletion protection enabled in production.
- [ ] Route 53 DNS configured.
- [ ] ACM certificate configured.
- [ ] AWS Budget alert configured.
- [ ] Staging RDS database (`silverhub_staging`) created on shared instance.
- [ ] ECS tasks placed in public subnets with `assignPublicIp` + SG lockdown verified.
- [ ] No NAT Gateway provisioned (cost control).

## Backend application

- [ ] API starts locally.
- [ ] API starts in container.
- [ ] API exposes `/health/live`.
- [ ] API exposes `/health/ready`.
- [ ] Swagger/OpenAPI works in development.
- [ ] Swagger disabled in production.
- [ ] FluentValidation configured for all request DTOs.
- [ ] Built-in JSON console logger configured (`AddJsonConsole`).
- [ ] CORS restricted in production.
- [ ] Global exception handling implemented.
- [ ] ProblemDetails implemented for errors.
- [ ] Request validation implemented.
- [ ] Rate limiting implemented for expensive/write endpoints.
- [ ] Request size limits configured.
- [ ] API versioning convention established.
- [ ] Build/version info available.
- [ ] CancellationToken passed through async flows.
- [ ] Controllers remain thin.
- [ ] Application handlers contain use-case flow.
- [ ] Domain logic avoids infrastructure dependencies.

## Database

- [ ] EF Core DbContext configured.
- [ ] Migrations created and committed.
- [ ] Migration workflow documented.
- [ ] Migrations apply cleanly to blank PostgreSQL database.
- [ ] Local database setup documented.
- [ ] Seed/import strategy implemented.
- [ ] Production does not run dev seeders.
- [ ] Slugs have unique indexes.
- [ ] Foreign keys configured.
- [ ] Common filters have indexes.
- [ ] JSON columns are used intentionally.
- [ ] Team builder payloads are size-limited.
- [ ] Gear optimization cache invalidation/versioning exists.
- [ ] Backup/restore process documented.

## Frontend

- [ ] Angular app builds for production.
- [ ] API base URL is centralized.
- [ ] Asset base URL is centralized.
- [ ] No secrets exist in frontend config.
- [ ] Routes are stable and documented.
- [ ] SPA fallback works through CloudFront.
- [ ] Loading states exist.
- [ ] Empty states exist.
- [ ] Error states exist.
- [ ] Form validation exists for tools.
- [ ] Server validation errors display correctly.
- [ ] Accessibility basics are covered.
- [ ] Responsive layout works on mobile.
- [ ] App version/build SHA is visible somewhere.
- [ ] Image asset strategy is consistent.
- [ ] Angular Signals used as primary state primitive.
- [ ] `/heroes/new-releases` route implemented.

## Testing

- [ ] Backend unit tests run in CI.
- [ ] Domain calculation tests exist.
- [ ] Gear optimization tests exist.
- [ ] Application handler tests exist.
- [ ] Repository integration tests exist.
- [ ] API integration tests exist.
- [ ] Migration tests exist.
- [ ] Frontend tests run in CI.
- [ ] Team builder state tests exist.
- [ ] Tool form tests exist.
- [ ] Playwright smoke tests exist.
- [ ] Tests block pull request merge.
- [ ] Coverage report generated.
- [ ] Coverage expectations documented.

## CI/CD

- [ ] GitHub Actions CI workflow exists.
- [ ] Backend restore/build/test run in CI.
- [ ] Frontend install/build/test run in CI.
- [ ] Docker build runs in CI.
- [ ] Terraform fmt/validate/plan runs for infra changes.
- [ ] GitHub OIDC configured for AWS.
- [ ] No long-lived AWS keys stored in GitHub.
- [ ] Main branch protected.
- [ ] Pull requests required.
- [ ] Required checks enabled.
- [ ] Production deploy workflow exists.
- [ ] Production deploy requires approval.
- [ ] Frontend deployment syncs to S3.
- [ ] Frontend deployment invalidates CloudFront.
- [ ] Backend image pushed to ECR.
- [ ] Backend deploy uses immutable image tag.
- [ ] Deployment verifies health checks.
- [ ] Rollback process tested or documented.
- [ ] Separate frontend-deploy and api-deploy workflows exist.
- [ ] Production DB migration workflow requires manual approval.

## Observability

- [ ] Structured API logging implemented.
- [ ] Request logs include method/path/status/duration.
- [ ] Logs include trace/request id.
- [ ] Error responses include trace id.
- [ ] Unhandled exceptions logged centrally.
- [ ] Slow request logging implemented.
- [ ] Gear optimization cache hit/miss logged.
- [ ] CloudWatch log groups created.
- [ ] Log retention configured.
- [ ] API health alarm configured.
- [ ] 5xx alarm configured.
- [ ] High latency alarm configured.
- [ ] RDS CPU/storage alarms configured.
- [ ] CloudFront error alarm configured.
- [ ] CloudWatch dashboard created.
- [ ] Deployment metadata logged.

## Security

- [ ] Secrets are not committed.
- [ ] Secrets stored in AWS Secrets Manager or SSM.
- [ ] CORS restricted in production.
- [ ] S3 buckets block public access.
- [ ] CloudFront OAC used for S3 origins.
- [ ] RDS access restricted.
- [ ] IAM roles are least-privilege.
- [ ] GitHub Actions uses OIDC.
- [ ] Production deploy environment protected.
- [ ] Rate limits configured.
- [ ] Request body size limits configured.
- [ ] Markdown/HTML sanitized.
- [ ] Security headers configured.
- [ ] Dependency scanning enabled.
- [ ] Swagger disabled/protected in production.
- [ ] Logs avoid secrets and sensitive payloads.

## Performance

- [ ] Frontend bundle size checked.
- [ ] CloudFront caching configured.
- [ ] Images optimized.
- [ ] API list endpoints avoid excessive payloads.
- [ ] Pagination implemented where needed.
- [ ] Database queries reviewed for common endpoints.
- [ ] Indexes support common filters.
- [ ] Gear optimization has timeout/limits.
- [ ] Expensive calculations are cached.
- [ ] Basic load/smoke testing performed.

## Documentation

- [ ] AWS architecture doc exists.
- [ ] Backend architecture doc exists.
- [ ] Database architecture doc exists.
- [ ] Frontend architecture doc exists.
- [ ] Testing strategy doc exists.
- [ ] CI/CD strategy doc exists.
- [ ] Observability strategy doc exists.
- [ ] Security strategy doc exists.
- [ ] Production readiness checklist exists.
- [ ] Local setup guide exists.
- [ ] Deployment guide exists.
- [ ] Troubleshooting/runbook exists.
- [ ] README updated with final architecture summary.
- [ ] Portfolio screenshots/GIFs added.
- [ ] Known tradeoffs documented.

## Portfolio presentation

- [ ] Project can be explained in 60 seconds.
- [ ] Architecture diagram exists.
- [ ] Live demo URL works.
- [ ] GitHub repo is clean and navigable.
- [ ] README links to architecture docs.
- [ ] Tests and CI are visible.
- [ ] Deployment workflow is visible.
- [ ] Production readiness checklist shows progress.
- [ ] Tradeoffs are honest.

## Release readiness

Before declaring a production release:

- [ ] CI passing on main.
- [ ] Database migration completed.
- [ ] Frontend deployed.
- [ ] Backend deployed.
- [ ] Health checks passing.
- [ ] Smoke tests passing.
- [ ] No new 5xx errors in logs.
- [ ] CloudFront distribution serving current build.
- [ ] Build SHA matches expected commit.
- [ ] Rollback target known.
