# CI/CD Strategy

## Purpose

This document defines the build, test, review, and deployment workflow for SilverHub.

The goal is to use a company-style workflow that is realistic for a solo portfolio project.

## Source control model

Use GitHub pull requests.

Branches:

```text
main
  production-ready branch

feature/*
  normal development branches
```

There is no `staging` branch. Staging is gated by GitHub Environments, not by branch name. Promotion model:

```text
feature branch -> pull request -> main
  -> auto-deploy to staging
  -> manual approval (GitHub Environment: production)
  -> deploy same image/build to production
```

## Branch protection

Protect `main`.

Requirements:

- Pull request required before merge.
- Required status checks must pass.
- Branch must be up to date before merge.
- No direct pushes to `main`.
- Conversation resolution required.
- Require linear history if desired.
- Require approval if collaborating with others later.

For solo development, branch protection still has value because it forces the production workflow to be visible and repeatable.

## GitHub Actions authentication to AWS

Use GitHub OIDC to assume AWS IAM roles.

Do not store long-lived AWS access keys in GitHub secrets.

Use separate IAM roles for:

```text
silverhub-github-ci
silverhub-github-deploy-staging
silverhub-github-deploy-prod
```

Limit production role trust policy to:

- the repository
- the protected branch or GitHub environment
- the deployment workflow

GitHub Actions should receive only the AWS permissions needed for that workflow.

## Workflows

Workflows:

```text
.github/workflows/ci.yml                  (PRs + main: build, test, lint)
.github/workflows/api-deploy.yml          (main: ECR push + ECS update; env input: staging|production)
.github/workflows/frontend-deploy.yml     (main: S3 sync + CloudFront invalidate; env input: staging|production)
.github/workflows/db-migrate.yml          (manual dispatch, env input; production requires approval)
.github/workflows/terraform-plan.yml      (PRs touching infra/**)
.github/workflows/terraform-apply.yml     (manual dispatch, env input)
```

API and frontend deployments are **independent workflows**. Deploying a frontend change does not redeploy the API and vice versa. Each can be re-run independently for rollback.

## CI workflow

### Trigger

```yaml
on:
  pull_request:
  push:
    branches: [main]
```

### Backend checks

Run:

```bash
dotnet restore
dotnet build --configuration Release --no-restore
dotnet test --configuration Release --no-build
dotnet format --verify-no-changes
```

Also run:

```bash
docker build -t silverhub-api:ci .
```

### Frontend checks

From frontend folder:

```bash
npm ci
npm run build
npm test
```

Add lint/format scripts once configured.

### Database checks

Run migration validation:

```bash
dotnet ef database update
```

against a CI PostgreSQL service or Testcontainers-based migration test.

### CI outputs

CI should publish:

- test results
- coverage report where useful
- build artifacts if needed
- Docker image only in deployment workflows, not every PR unless useful

## Terraform plan workflow

### Trigger

```yaml
on:
  pull_request:
    paths:
      - "infra/**"
```

Run:

```bash
terraform fmt -check
terraform init
terraform validate
terraform plan
```

For PRs, publish the plan as a PR comment or workflow artifact.

Do not automatically apply Terraform from arbitrary pull requests.

## Terraform apply workflow

### Trigger

Use manual dispatch or protected environment approval.

```yaml
on:
  workflow_dispatch:
```

Apply only after:

- CI passed.
- Terraform plan reviewed.
- GitHub environment approval completed for production.

Production Terraform apply should be rare compared to application deploys.

## Frontend deployment workflow

### Build

```bash
cd src/SilverHub.Web
npm ci
npm run build
```

### Deploy

```bash
aws s3 sync <angular-dist-path> s3://<frontend-bucket> --delete
aws cloudfront create-invalidation --distribution-id <distribution-id> --paths "/*"
```

The exact Angular dist path should be confirmed in the rewritten app.

### Cache strategy

- Hashed assets can be cached long-term.
- `index.html` should be invalidated on every deploy.
- A broad `/*` invalidation is acceptable for a small portfolio site.
- Later, optimize invalidations to changed files only.

## Backend deployment workflow

### Build and push image

```bash
docker build -t silverhub-api:<git-sha> .
docker tag silverhub-api:<git-sha> <account>.dkr.ecr.<region>.amazonaws.com/silverhub-api:<git-sha>
docker push <account>.dkr.ecr.<region>.amazonaws.com/silverhub-api:<git-sha>
```

### Deploy

Standard ECS Fargate deploy:

```bash
# Render new task definition with the new image SHA
aws ecs describe-task-definition --task-definition silverhub-api-<env> \
  | jq '.taskDefinition | { ... new image ... }' > new-task-def.json

aws ecs register-task-definition --cli-input-json file://new-task-def.json

aws ecs update-service \
  --cluster silverhub-<env> \
  --service silverhub-api-<env> \
  --task-definition silverhub-api-<env>:<new-revision>

aws ecs wait services-stable \
  --cluster silverhub-<env> \
  --services silverhub-api-<env>
```

After stability:

- Hit `/health/ready` through the ALB to confirm readiness.
- Record the deployed image SHA and task definition revision.

The deployment circuit breaker on the ECS service will automatically roll back if the new task fails health checks repeatedly.

## Database migration deployment

**Production migrations always require manual approval.** They run via the dedicated `db-migrate.yml` workflow against the `production-migrations` GitHub Environment, which is configured to require a reviewer.

Staging migrations may run automatically as part of the staging API deploy.

Production flow:

```text
PR merged to main
  -> staging API deploy runs staging migrations automatically
  -> developer manually dispatches db-migrate.yml with env=production
  -> reviewer approves the production-migrations environment
  -> migration runs against silverhub_prod
  -> production API deploy promotes the API image
  -> /health/ready verifies
```

For risky migrations:

```text
manual RDS snapshot
  -> apply backward-compatible migration (expand)
  -> deploy API that writes/reads new schema
  -> verify
  -> remove old columns in a later deploy (contract)
```

Never run a destructive migration in the same deploy that depends on the old data shape.

## Environments

Use GitHub Environments:

```text
staging
production
```

Production environment should require manual approval.

Environment-level secrets/variables:

```text
AWS_ROLE_ARN
AWS_REGION
ECR_REPOSITORY
FRONTEND_BUCKET
CLOUDFRONT_DISTRIBUTION_ID
API_SERVICE_ID
```

Avoid duplicating secrets across workflows.

## Deployment promotion

Production promotion (per-component):

```text
merge to main
  -> CI passes
  -> api-deploy.yml runs with env=staging      (auto)
  -> frontend-deploy.yml runs with env=staging (auto)
  -> Playwright smoke tests against staging    (auto, post-deploy)
  -> manual approval via GitHub Environment: production
  -> api-deploy.yml runs with env=production using the same image SHA
  -> frontend-deploy.yml runs with env=production using the same build artifact
```

Production must reuse the exact image SHA and frontend artifact that passed staging. The api-deploy workflow takes the image SHA as an input parameter when targeting production.

## Rollback strategy

### Frontend rollback

Options:

- Re-sync previous build artifact to S3.
- Use S3 versioning to restore previous objects.
- Re-run deployment workflow for previous commit.

### Backend rollback

Options:

- Re-deploy previous ECR image tag.
- Register a task definition referencing the prior image SHA and `aws ecs update-service` back to it. The ECS service's deployment circuit breaker will also auto-roll-back during the failing forward deploy if the new tasks fail health checks repeatedly.

### Database rollback

Database rollback is harder.

Rules:

- Prefer backward-compatible migrations.
- Take snapshot before risky migrations.
- Use expand/contract migration pattern:
  1. Add new schema.
  2. Deploy app that writes both or reads both.
  3. Backfill.
  4. Switch reads.
  5. Remove old schema later.

## Release tagging

Optional but useful:

```text
v0.1.0
v0.2.0
v1.0.0
```

Tags can trigger production deployment or create GitHub releases.

For a portfolio project, tags help show milestone progression.

## Status badges

Add later to README:

```text
CI status
Production deploy status
Test coverage if useful
```

Do not add badges until workflows are stable.

## Artifact retention

Store:

- test results
- coverage reports
- Terraform plans
- built frontend artifact if needed
- deployment metadata

Retention can be short to control storage.

## Deployment verification

After deployment:

- Check `/health/live`.
- Check `/health/ready`.
- Run read-only API smoke test.
- Run frontend smoke test.
- Check CloudWatch logs for new errors.

Production deploy should fail if health checks fail.

## CI/CD quality checklist

- [ ] Pull requests trigger CI.
- [ ] Backend build/test required.
- [ ] Frontend build/test required.
- [ ] Docker build required.
- [ ] Terraform plan runs for infrastructure changes.
- [ ] Production deploy requires protected approval.
- [ ] AWS auth uses OIDC, not long-lived keys.
- [ ] Production deploy uses immutable image tags.
- [ ] Migrations are controlled.
- [ ] Frontend deploy invalidates CloudFront.
- [ ] Backend deploy verifies health.
- [ ] Rollback process is documented.
