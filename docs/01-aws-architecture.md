# AWS Architecture

## Purpose

This document defines the target AWS infrastructure for SilverHub.

The goal should be understandable, responsive, cost-conscious, and production-oriented enough to serve as a strong portfolio project.

## Architecture summary

SilverHub is a full-stack web application with:

- Angular static frontend.
- ASP.NET Core API backend.
- PostgreSQL relational database.
- Public static/media assets.
- CI/CD through GitHub Actions.
- Infrastructure managed primarily with Terraform.
- Containerized API deployment on Amazon ECS Fargate behind an Application Load Balancer.

Target production architecture:

```text
User Browser
  |
  v
Route 53 custom domain
  |
  v
CloudFront distribution
  |----------------------------------|
  |                                  |
  v                                  v
S3 frontend bucket                   API origin
Angular static files                 Application Load Balancer
                                     ECS service on Fargate
                                     ASP.NET Core container
                                     Auto scaling
                                     CloudWatch logs/metrics
  |
  v
S3 asset bucket
public images/content assets

Backend API
  |
  v
RDS PostgreSQL

Build/deploy services:
  - GitHub Actions
  - GitHub OIDC to AWS
  - ECR for backend container images
  - Terraform for infrastructure

Operational services:
  - CloudWatch Logs
  - CloudWatch Metrics
  - CloudWatch Alarms
  - CloudWatch Dashboards
  - AWS Budgets
  - Secrets Manager / SSM Parameter Store
  - IAM roles and least-privilege policies
```

## Primary infrastructure decisions

### Infrastructure-as-code

Use **Terraform**.

Reasons:

- Terraform is widely used across cloud providers and companies.
- It keeps infrastructure changes reviewable through pull requests.
- It avoids one-off manual console setup.
- It creates a portfolio artifact that shows infrastructure discipline beyond application code.
- It is more transferable than a cloud-specific framework.

Terraform should manage:

- S3 buckets.
- CloudFront distribution.
- Route 53 DNS records.
- ACM certificates.
- ECR repository.
- API supporting infrastructure.
- IAM roles and policies.
- RDS PostgreSQL instance.
- Security groups and VPC networking where needed.
- Secrets Manager secrets or SSM parameters.
- CloudWatch log groups, alarms, and dashboards.
- AWS Budgets alerts.
- GitHub OIDC IAM provider and deployment roles.

### Backend hosting

Use **standard Amazon ECS on Fargate** behind an **Application Load Balancer**, fully managed in Terraform.

Reasons:

- Terraform AWS provider has stable, mature support for ECS services, task definitions, ALBs, target groups, and listener rules. Every runtime resource can live in Terraform as the single source of truth.
- Full control over networking, autoscaling policies, security groups, and deployment behavior.
- Predictable, transparent cost (no abstraction surprises).
- Well-documented operational model and large community.

Terraform owns every runtime resource:

```text
- ECR repository
- ECS cluster
- ECS task definition (revisioned)
- ECS service
- Application Load Balancer
- ALB target group and listener
- ALB security group
- ECS task security group
- IAM task role and task execution role
- VPC, subnets, route tables, Internet Gateway
- RDS instance and security group
- Secrets Manager / SSM parameters
- CloudWatch log group, alarms, dashboard
- Route 53 records, ACM certificate
- AWS Budgets
- GitHub OIDC role
```

GitHub Actions owns image build/push and triggers ECS service updates (task definition revisions reference the new image SHA).

## AWS region

Default region:

```text
us-east-1
```

Reasons:

- ACM certificates for CloudFront custom domains must be in `us-east-1`.
- Keeping most infrastructure in one region reduces complexity.
- `us-east-1` has broad AWS service support.

Exceptions:

- CloudFront is global.
- Route 53 is global.
- ACM certificates used by CloudFront are managed in `us-east-1`.

## Terraform structure

Use environment-aware Terraform rather than one flat file.

Recommended structure:

```text
infra/
  terraform/
    modules/
      network/
      frontend-static-site/
      asset-bucket/
      api-ecr/
      api-ecs-fargate/
      database-rds-postgres/
      observability/
      iam-github-oidc/
      dns/
      budget/
    envs/
      staging/
        main.tf
        variables.tf
        terraform.tfvars
        backend.tf
      prod/
        main.tf
        variables.tf
        terraform.tfvars
        backend.tf
```

Both `staging` and `prod` envs are stood up from the start. Local development uses Docker Compose PostgreSQL — there is no AWS `dev` environment.

### Terraform state

Use remote state.

Recommended state backend:

- S3 bucket for Terraform state.
- DynamoDB table for state locking.

State resources should be created manually once or through a small bootstrap Terraform setup.

State bucket requirements:

- Versioning enabled.
- Server-side encryption enabled.
- Public access blocked.
- Deletion protected by policy or process.
- Separate from application buckets.

Do not store secrets in committed `.tfvars` files.

## Frontend hosting

### Decision

Host Angular static files in S3 and serve them through CloudFront.

```text
Angular build output
  -> S3 frontend bucket
  -> CloudFront
  -> custom domain
```

### S3 frontend bucket

Use a private S3 bucket.

Do not use public S3 website hosting for production.

CloudFront should access the private bucket using **Origin Access Control (OAC)**.

Reasons:

- The bucket is not publicly readable.
- Users access the site only through CloudFront.
- CloudFront handles HTTPS, caching, compression, and edge delivery.
- OAC is the modern CloudFront-to-S3 access pattern.

### CloudFront SPA behavior

Angular is a single-page application, so direct navigation to routes like `/heroes/some-hero` must return `index.html`.

Configure CloudFront custom error responses:

```text
403 -> /index.html -> 200
404 -> /index.html -> 200
```

Use careful cache behavior:

- `index.html`: short cache or invalidated on deploy.
- Hashed JS/CSS assets: long cache.
- Static images: long cache where filenames are versioned.
- API routes: no static caching unless an endpoint is explicitly cache-safe.

### Frontend deploy process

CI/CD should:

1. Run frontend checks.
2. Build Angular.
3. Sync build output to S3.
4. Delete removed files from S3.
5. Invalidate CloudFront paths.

Expected command shape:

```bash
aws s3 sync dist/SilverHub.Web/browser s3://<frontend-bucket> --delete
aws cloudfront create-invalidation --distribution-id <id> --paths "/*"
```

The exact build output path should be confirmed after the new Angular project is built.

## Public asset hosting

### Decision

Host public assets in S3 and serve them through CloudFront.

Use a separate S3 bucket for public assets:

```text
silverhub-assets-prod
```

Serve assets through the **same CloudFront distribution** as the frontend, under the path:

```text
/content-assets/*
```

A CloudFront cache behavior matches `/content-assets/*` and routes those requests to the asset S3 bucket origin (via OAC). All other paths route to the frontend bucket origin.

Reasons:

- Fewer moving parts.
- One CDN, one domain, one TLS certificate.
- Simpler Terraform.
- Easier local-to-production config.

### Asset access

Most game images and content assets are public, so they can be cached aggressively.

The asset bucket should still be private and accessed through CloudFront OAC.

### Asset naming

Use stable, descriptive object keys:

```text
heroes/<hero-slug>/portrait.webp
heroes/<hero-slug>/full.webp
artifacts/<artifact-slug>.webp
guides/<guide-type>/<slug>/<image-name>.webp
news/<article-slug>/<image-name>.webp
```

Prefer immutable filenames when updating image content:

```text
portrait.v2.webp
```

or content-hashed filenames if generated by tooling.

## Backend hosting

### Decision

Use **standard Amazon ECS on Fargate** behind an Application Load Balancer for the ASP.NET Core backend.

The API remains containerized. Expected API flow:

```text
GitHub Actions
  -> docker build
  -> docker tag with Git SHA
  -> docker push to ECR
  -> register new ECS task definition revision referencing the new image
  -> aws ecs update-service to point service at the new task definition
  -> wait for deployment to stabilize
  -> health check
```

The ASP.NET Core container listens on port:

```text
8080
```

The ECS task definition configures:

- Container image (full ECR URI with Git SHA tag).
- Container port (8080).
- Health check path (`/health/ready` via ALB target group).
- Environment variables.
- Secret references (Secrets Manager ARNs via `secrets` block).
- CPU/memory values.
- Log driver `awslogs` pointing at the environment's CloudWatch log group.

The ECS service configures:

- Task definition.
- Desired count.
- Subnets (public, since no NAT Gateway).
- `assignPublicIp: ENABLED`.
- Security group (ECS task SG; only ALB SG can reach port 8080).
- Target group attachment.
- Health check grace period.
- Deployment circuit breaker enabled.

Application Auto Scaling targets the ECS service for scale-out based on CPU/memory or ALB request count.

### Recommended starting service settings

Initial low-cost production settings:

```text
container port: 8080
health check path: /health/ready
task cpu: 256
task memory: 512 MiB
desired count: 1
autoscaling min: 1
autoscaling max: 2
```

CPU/memory values can be revised after load testing.

### Health checks

The API should expose:

```text
/health/live
/health/ready
```

Use:

```text
/health/ready
```

for the load balancer/service health check, because readiness should verify required dependencies such as the database.

### Custom API domain

Use a custom domain:

```text
api.silverhub.app
```

The API domain should point to the Application Load Balancer.

TLS should use ACM.

## Backend container registry

Use Amazon ECR.

Tagging convention:

```text
silverhub-api:<git-sha>
silverhub-api:prod-latest
silverhub-api:staging-latest
```

Production deployments should use immutable Git SHA tags, not only `latest`.

ECR lifecycle policy:

- Keep recent image tags.
- Expire old untagged images.
- Preserve currently deployed production image.
- Preserve a limited rollback window.

Example policy intent:

```text
keep last 30 tagged images
expire untagged images after 7 days
```

## Database

### Decision

Use Amazon RDS PostgreSQL.

Recommended production starting point:

- Single-AZ RDS PostgreSQL.
- Small instance size appropriate for low traffic.
- Encrypted storage.
- Automated backups enabled.
- Deletion protection enabled for production.
- Public accessibility disabled.
- CloudWatch metrics enabled.
- Performance Insights optional.

### Development database

Use local Docker Compose PostgreSQL.

### Staging database

A single RDS instance hosts both databases:

```text
silverhub_staging
silverhub_prod
```

Staging imports/seeders must never target the prod database (enforced by connection-string isolation per environment).

### Database access

The API should connect to RDS using credentials stored in Secrets Manager.

The database should not be publicly accessible.

Security group rules:

```text
RDS inbound:
  port 5432
  source: API service security group only
```

## Networking

### VPC layout

A small custom VPC managed by Terraform:

```text
VPC (10.0.0.0/16)
  public subnets (2 AZs, for HA on the ALB):
    - Application Load Balancer
    - ECS Fargate tasks (assignPublicIp: ENABLED)
  private subnets (2 AZs):
    - RDS PostgreSQL (no internet route)
```

### No NAT Gateway, no VPC endpoints

The ECS tasks live in public subnets with public IPs assigned. They reach AWS service endpoints (ECR, CloudWatch Logs, Secrets Manager) **directly through the Internet Gateway** — no NAT Gateway and no VPC interface endpoints required.

This is a deliberate cost-control choice:

- NAT Gateway: ~$32/month per AZ plus per-GB data charges.
- VPC interface endpoints (ECR api + ECR dkr + CloudWatch Logs + Secrets Manager): ~$7/month each, ~$28/month minimum.
- Public subnets + Internet Gateway: $0/month additional.

The tradeoff is that ECS tasks have public IPs. They are **not reachable from the internet** because the ECS task security group blocks all inbound traffic except from the ALB security group on port 8080. The public IP is used only for the task's *outbound* connections to AWS endpoints.

RDS stays in private subnets and has no internet route at all.

### Security groups

```text
ALB SG:
  inbound 443 from 0.0.0.0/0
  outbound to ECS task SG on 8080

ECS task SG:
  inbound 8080 from ALB SG only (no public reachability)
  outbound 443 to 0.0.0.0/0  (ECR, CloudWatch Logs, Secrets Manager, etc.)
  outbound 5432 to RDS SG

RDS SG:
  inbound 5432 from ECS task SG only
  no outbound rules required
```

### Posture summary

- Public internet traffic reaches the ALB over HTTPS.
- The ALB forwards to ECS tasks on port 8080 (HTTP inside the VPC; ALB terminates TLS).
- ECS tasks reach RDS privately within the VPC.
- ECS tasks reach AWS endpoints via the Internet Gateway using their public IPs, but are not reachable from the internet themselves.
- RDS is not internet-reachable at all.

## Secrets and configuration

Use AWS Secrets Manager or SSM Parameter Store.

Recommended split:

### Secrets Manager

Use for sensitive values:

- Database username/password.
- Full DB connection string if not constructed at runtime.
- Any future private API keys.

### SSM Parameter Store

Use for non-secret config:

- API base URL.
- frontend asset base URL.
- allowed CORS origins.
- feature flags.
- logging level overrides.

No production secrets should be stored in:

- GitHub repo.
- Terraform committed variables.
- Angular frontend environment files.
- Docker image.
- GitHub Actions plaintext logs.

Frontend configuration must not include secrets because it is visible to users.

## DNS and TLS

Use:

- Route 53 hosted zone.
- ACM certificate.
- CloudFront alias for frontend domain.
- ALB/API certificate for API domain.

Domains:

```text
silverhub.app
www.silverhub.app
api.silverhub.app
staging.silverhub.app
api-staging.silverhub.app
```

CloudFront certificates must be in `us-east-1`.

ALB/API certificates should be in the same region as the ALB.

## Cost-control principles

This is a portfolio app, so costs matter.

Use:

- S3 + CloudFront for static hosting.
- Small RDS instance.
- Single-AZ database initially.
- Shared RDS instance for staging and prod databases.
- Low minimum ECS task count.
- Autoscaling limits.
- Public subnets + Internet Gateway for ECS tasks (no NAT Gateway, no VPC interface endpoints).
- CloudWatch log retention limits.
- AWS Budgets alerts.
- ECR lifecycle policy.
- No Kubernetes.

Likely largest ongoing fixed costs:

1. RDS PostgreSQL.
2. ECS/Fargate API task runtime.
3. Application Load Balancer.
4. CloudWatch logs if retention/volume is not controlled.

## Environments

Recommended environments:

```text
local
staging
production
```

### Local

- Angular dev server.
- ASP.NET Core API.
- PostgreSQL via Docker Compose.
- Local seed/import data.
- No AWS dependency required for normal feature development.

### Staging

- Provisioned from day one.
- AWS resources mirror production shape (separate ECS cluster, service, ALB, CloudFront behavior, log group).
- Shares the RDS instance with production via a separate `silverhub_staging` database.
- Used for deployment validation and Playwright smoke tests before promoting to prod.

### Production

- Public user-facing environment.
- Protected deployment workflow.
- Manual approval or tag-based promotion.
- Backups and deletion protection enabled.
- Alarms and logs enabled.

## Terraform modules to build

Initial module list:

```text
network                   (VPC, subnets, route tables, Internet Gateway, security groups)
frontend_static_site      (S3 frontend bucket + CloudFront distribution + asset behavior)
asset_bucket              (S3 asset bucket + OAC binding to the shared CloudFront)
api_ecr                   (ECR repository + lifecycle policy)
api_ecs_fargate           (ECS cluster, task definition, service, ALB, target group, listener, autoscaling)
database_rds_postgres     (RDS instance + parameter group + subnet group)
github_oidc               (GitHub OIDC provider + per-workflow IAM roles)
observability             (CloudWatch log groups, alarms, dashboard, SNS topic)
dns                       (Route 53 hosted zone + records, ACM certificates)
budget                    (AWS Budgets alerts)
```

Inside `api_ecs_fargate`, the ECS task definition is the resource that GitHub Actions revisions on each deploy (Terraform manages the baseline shape; CI revisions the image SHA).

## CI/CD ownership

Terraform owns infrastructure.

GitHub Actions owns application deployment:

- Run checks and tests.
- Build Angular frontend.
- Upload frontend files to S3.
- Invalidate CloudFront cache.
- Build backend Docker image.
- Push backend image to ECR.
- Deploy image to ECS Fargate service (register new task definition revision and update service).
- Run database migrations through a controlled process.

GitHub Actions should authenticate to AWS using OIDC, not long-lived AWS access keys.

## Backend deployment flow

API and frontend deployments are **separate workflows**. Each runs only when its own component changes.

```text
Pull request:
  - restore/build/test backend
  - install/build/test frontend
  - run lint/format checks
  - build Docker image but do not push
  - run Terraform plan for visibility where appropriate

Merge to main — api-deploy workflow:
  - build and test API
  - build Docker image, tag with Git SHA
  - push image to ECR
  - register new ECS task definition revision
  - aws ecs update-service to staging
  - wait for stability, verify /health/ready
  - run staging DB migrations automatically if any
  - manual approval gate (GitHub Environment: production)
  - aws ecs update-service to production (same image SHA)
  - production DB migrations require their own manual approval workflow

Merge to main — frontend-deploy workflow:
  - install/build/test frontend
  - sync build output to staging S3 bucket
  - invalidate staging CloudFront
  - manual approval gate (GitHub Environment: production)
  - sync to production S3 bucket
  - invalidate production CloudFront
```

## Database migration flow

Use EF Core migrations.

**Production migrations always require manual approval** via a dedicated GitHub Environment-gated workflow. Migrations never run automatically on production deploy.

Staging migrations may run automatically as part of the staging deploy.

Production migration process:

```text
CI builds migration bundle
Manual approval required (GitHub Environment: production-migrations)
Run migration against production database
Deploy API version that expects the migrated schema
```

For risky migrations, use expand/contract:

```text
1. Expand schema with backwards-compatible changes.
2. Deploy app that writes/reads new schema.
3. Backfill data if needed.
4. Remove old schema in a later release.
```

## Operational expectations

Production should support:

- Request logging.
- Error logging.
- API health checks.
- API 5xx alarms.
- High latency alarms.
- Database CPU/storage alarms.
- CloudFront error-rate visibility.
- Budget alerts.
- Log retention limits.
- A documented rollback process.

## Manual changes policy

Do not manually mutate production infrastructure in the AWS Console except during investigation or emergency recovery.

If a manual change is made:

1. Document what changed.
2. Backfill the change into Terraform.
3. Re-run Terraform plan.
4. Confirm Terraform no longer wants to undo the intended state.

## References

- Amazon ECS on Fargate documentation.
- Amazon ECR documentation.
- Amazon RDS PostgreSQL documentation.
- Amazon S3 and CloudFront documentation.
- Terraform AWS Provider documentation.
