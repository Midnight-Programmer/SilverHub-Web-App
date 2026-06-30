# Observability Strategy

## Purpose

This document defines logging, monitoring, tracing, alerting, and operational visibility for SilverHub.

The goal is not enterprise-scale observability. The goal is to be able to answer practical production questions:

- Is the site up?
- Is the API healthy?
- Are requests failing?
- Which endpoint is slow?
- What error happened?
- Which deployment introduced the issue?
- Can a request be traced through logs?

## Observability scope

Initial scope:

- ASP.NET Core API structured logs.
- Request/response metadata.
- Exception logs.
- Health checks.
- CloudWatch log groups.
- CloudWatch metrics and alarms.
- Basic dashboard.
- Frontend deployment/version visibility.

Later scope:

- Distributed tracing.
- Real user monitoring.
- Synthetic checks.
- OpenTelemetry traces.
- Error tracking service.

## Logging

### Backend structured logging

Use the built-in `Microsoft.Extensions.Logging` with the JSON console formatter:

```csharp
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
    options.UseUtcTimestamp = true;
});
```

The container writes JSON logs to stdout. The ECS `awslogs` log driver ships them to the environment's CloudWatch log group.

### Required request log fields

Every completed API request should log:

```text
timestamp
level
traceId
requestId
method
path
query string presence, not full query if sensitive
statusCode
elapsedMs
remoteIp if available and useful
userAgent if useful
environment
appVersion
```

Example:

```json
{
  "level": "Information",
  "message": "HTTP request completed",
  "traceId": "00-f1...",
  "method": "GET",
  "path": "/api/v1/heroes",
  "statusCode": 200,
  "elapsedMs": 34,
  "environment": "Production",
  "appVersion": "abc1234"
}
```

### Error log fields

Unhandled exceptions should log:

```text
traceId
method
path
statusCode
exceptionType
exceptionMessage
stackTrace
appVersion
environment
```

Do not include request body by default.

### Slow request logging

Log warning for requests above threshold.

Initial threshold:

```text
1000 ms
```

Separate threshold for intentionally heavy endpoints:

```text
/tools/gear-optimization: 3000 ms
```

### Tool-specific logging

For gear optimization:

```text
input hash
algorithm version
cache hit/miss
elapsedMs
candidate count if safe
result count
```

Do not log full user input if it can be large or user-provided.

For team builder shares:

```text
share id/code
payload version
payload size
created/retrieved
```

Do not log full payload by default.

## Correlation and trace IDs

Every response should include a trace/request id.

Suggested header:

```text
X-Request-ID
```

Rules:

- If caller supplies a valid request ID, preserve it.
- Otherwise generate one.
- Include it in logs.
- Include it in ProblemDetails error responses.

This lets a user-facing error be connected to server logs.

## ProblemDetails integration

Error responses should include trace id.

Example:

```json
{
  "type": "https://silverhub.app/errors/unexpected",
  "title": "Unexpected server error",
  "status": 500,
  "traceId": "00-f1..."
}
```

Never expose stack traces in production responses.

## Health checks

Expose:

```text
GET /health/live
GET /health/ready
```

### Liveness

Checks:

- process is running

### Readiness

Checks:

- database reachable
- required config present
- optional: asset bucket/config present
- optional: cache dependency reachable if introduced

Readiness should fail when the app should not receive traffic.

## CloudWatch Logs

Create log groups through Terraform.

Recommended log groups:

```text
/silverhub/prod/api
/silverhub/staging/api
/silverhub/prod/frontend-deploy
/silverhub/prod/db-maintenance
```

Set retention periods.

Recommended initial retention:

```text
staging: 7-14 days
production: 30 days
```

Longer retention can be added later if useful.

## CloudWatch Metrics

Track AWS/platform metrics:

### API hosting

- request count
- 4xx count/rate
- 5xx count/rate
- latency
- CPU/memory if available
- instance count
- deployment failures

### RDS

- CPU utilization
- database connections
- free storage
- read/write IOPS
- freeable memory
- deadlocks if available
- backup status

### CloudFront

- requests
- error rate
- cache hit ratio
- bytes downloaded
- origin latency

### S3

- bucket size
- object count
- request errors if enabled

## Custom application metrics

Initial custom metrics can be log-derived rather than explicitly emitted.

Useful metrics:

```text
api.error.count
api.request.duration
gear_optimization.duration
gear_optimization.cache_hit
gear_optimization.cache_miss
team_builder_share.created
team_builder_share.loaded
```

Do not overbuild custom metrics before the app has enough traffic to justify them.

## Alarms

Production alarms:

```text
API health check failing
API 5xx rate above threshold
API high latency above threshold
RDS high CPU
RDS low free storage
RDS high connection count
CloudFront 5xx errors above threshold
AWS budget threshold reached
```

For a portfolio app, alarms can initially notify by email through SNS.

## Dashboard

Create a simple CloudWatch dashboard.

Sections:

```text
API
  request count
  4xx
  5xx
  latency
  latest deployment version

Database
  CPU
  connections
  storage
  memory

Frontend/CDN
  CloudFront requests
  CloudFront error rate
  cache hit ratio

Cost
  budget status if available
```

## Frontend observability

Initial frontend observability should be modest.

Add:

- app version/build SHA
- user-visible fallback error pages
- console logging only in development
- optional analytics later

Do not add invasive analytics by default.

If using frontend error reporting later:

- avoid collecting personal data
- scrub URLs if needed
- sample errors if volume grows

## Deployment observability

Every deployment should record:

```text
commit SHA
image tag
deployment timestamp
environment
actor/workflow
frontend artifact version
database migration version
```

Expose build info in API:

```text
GET /api/v1/system/version
```

or include headers:

```text
X-SilverHub-Version
X-SilverHub-Environment
```

The endpoint should not expose secrets or internal infrastructure details.

## Log querying examples

Common CloudWatch Logs Insights queries should be documented later.

Examples to support:

```text
Find all errors in last hour.
Find requests by trace id.
Find slowest endpoints.
Find gear optimization cache misses.
Find 500 errors after last deployment.
```

## Incident workflow

Basic incident flow:

```text
1. Confirm issue through health check/dashboard.
2. Check latest deployment.
3. Search logs by time window.
4. Search logs by trace id if reported by user.
5. Determine frontend/backend/database/CDN layer.
6. Roll back if deployment-related.
7. Add test or alert if gap was found.
```

## Observability quality checklist

- [ ] API emits structured logs.
- [ ] Logs include request id/trace id.
- [ ] Error responses include trace id.
- [ ] Request durations are logged.
- [ ] Slow requests are warning logs.
- [ ] Unhandled exceptions are logged centrally.
- [ ] Health endpoints exist.
- [ ] CloudWatch log groups have retention.
- [ ] Production alarms exist.
- [ ] Budget alarm exists.
- [ ] Dashboard exists.
- [ ] Deployment version is visible.
- [ ] Logs avoid secrets and full payload bodies.
