# Database Architecture and Schema Strategy

## Purpose

This document defines the database architecture, schema design principles, migration strategy, and data-management expectations for SilverHub.

The database should support a production-style web application while remaining simple enough for a portfolio project.

## Technology

- Database engine: PostgreSQL 18
- Local development: Docker Compose PostgreSQL
- Production: Amazon RDS PostgreSQL
- ORM: Entity Framework Core
- Provider: Npgsql.EntityFrameworkCore.PostgreSQL
- Migration tool: EF Core migrations

## Database environments

Recommended databases:

```text
local:
  silverhub_dev

staging:
  silverhub_staging

production:
  silverhub_prod
```

Staging and production share a single RDS instance, separated as databases `silverhub_staging` and `silverhub_prod`. Each environment's API uses a connection string pointing only at its own database. Production data must never be overwritten by staging imports or test seeders.

## Schema ownership

The application owns its relational schema through EF Core migrations.

Manual production schema changes are not allowed except during emergency recovery. Any manual schema change must be backfilled into a migration.

## Migration strategy

Use EF Core migrations as the source of truth.

Migration rules:

- Every schema change is represented by a migration.
- Migration files are committed.
- Migration names should describe intent.
- Migrations should be reviewed in pull requests.
- CI should validate that the model and migrations are in sync.
- Production migrations should run through a controlled deployment process.

Example migration naming:

```text
AddHeroes
AddArtifacts
AddTeamBuilderShares
AddGearOptimizationCache
AddGuideContentTables
AddHeroSlugUniqueIndex
```

## Production migration process

Preferred process:

1. CI builds the application.
2. CI runs tests.
3. CI validates migrations.
4. Deployment workflow creates a migration artifact or runs an approved migration step.
5. Migration runs before or during API deployment.
6. Deployment verifies health checks.

For early portfolio implementation, automatic migration on production startup is not preferred because failed migrations can break app startup and obscure deployment failures.

Acceptable early alternative:

- A manual `DbMaintenance` tool or GitHub Action runs migrations before API deployment.
- The API fails readiness if expected schema is unavailable.

## Seed/import strategy

Separate seed data from migrations.

Migrations should define schema.

Seed/import tools should populate:

- heroes
- artifacts
- news posts
- guide content
- guide floors/bosses/sections
- static reference data

Recommended source of truth for content:

```text
content/
  heroes/
  artifacts/
  news/
  guides/
```

Content files can be JSON, YAML, or Markdown with front matter.

A content import tool can read structured files and upsert rows.

Benefits:

- Content is reviewable in Git.
- Content changes can go through PRs.
- No admin UI is required at first.
- Database can be rebuilt from source files.

## Database naming conventions

Use consistent naming.

Recommended table naming:

```text
heroes
hero_images
hero_skills
hero_tags
artifacts
artifact_skills
news_posts
news_post_sections
team_builder_shares
gear_optimization_cache_entries
```

Recommended column naming:

```text
id
slug
display_name
created_at_utc
updated_at_utc
deleted_at_utc
```

Use snake_case in the database if configured consistently.

C# property names remain PascalCase.

## Primary keys

Use `uuid`/`Guid` primary keys for most tables.

Reasons:

- Avoids exposing sequential IDs.
- Easy to generate in application code.
- Works well across imports and distributed systems.

Exceptions:

- Join tables can use composite keys.
- Lookup/reference rows can use stable slugs or enum-like codes if appropriate.

## Slugs

Human-facing pages should use slugs.

Examples:

```text
heroes.noah
heroes.transcendent-noah
artifacts.acappella
news.version-1-2-update
```

Slug rules:

- Lowercase.
- URL-safe.
- Unique within resource type.
- Stable once published.
- Changes require redirect/alias strategy if public links exist.

Recommended constraints:

```text
unique index on heroes.slug
unique index on artifacts.slug
unique index on news_posts.slug
unique index on guide slugs by guide type
```

## Auditing columns

Most mutable tables should include:

```text
created_at_utc
updated_at_utc
```

Optional:

```text
created_by
updated_by
```

Do not add user auditing until authentication/editor features exist.

For content imported from Git, also consider:

```text
source_file_path
source_content_hash
last_imported_at_utc
```

These make imports idempotent and easier to debug.

## Soft deletes

Use soft deletes only where needed.

Good candidates:

- User-created team builder shares if deletion is supported.
- Future user accounts/saved teams.

Poor candidates:

- Static reference content that can be controlled through imports.
- Simple join tables.

If soft delete is used, include:

```text
deleted_at_utc
```

and ensure queries filter appropriately.

## Core schema areas

### Heroes

Purpose:

- Store hero reference data.
- Power hero list/detail pages.
- Support filters and guide/tool integrations.

Likely tables:

```text
heroes
hero_images
hero_skills
hero_tags
hero_preferred_artifacts
hero_synergies
```

Important fields:

```text
id
slug
display_name
rarity
faction
class
equip_type
moon_type
damage_type
release_date
limited
boudoir
has_resonantia
portrait_image_key
```

Indexes:

```text
heroes.slug unique
heroes.rarity
heroes.faction
heroes.class
heroes.display_name
```

### Artifacts

Purpose:

- Store artifact reference data.
- Support artifact options in heroes/tools.

Likely tables:

```text
artifacts
artifact_skills
```

Important fields:

```text
id
slug
display_name
description
image_key
```

Indexes:

```text
artifacts.slug unique
artifacts.display_name
```

### News

Purpose:

- Store site news/game updates.
- Support article pages and homepage listing.

Likely tables:

```text
news_posts
news_post_sections
```

Important fields:

```text
id
slug
title
summary
published_at_utc
updated_at_utc
hero_image_key
status
```

For sections:

```text
id
news_post_id
section_type
heading
body_markdown
sort_order
```

Indexes:

```text
news_posts.slug unique
news_posts.published_at_utc
news_posts.status
```

### Guides

Purpose:

- Store guide content for game modes.

Guide areas:

```text
clan_hunt
void_of_shadow
tomb_of_the_fallen
```

Use **specialized tables per game mode**. Each mode has a distinct structure (seasons/bosses vs stages/floors vs dungeons/floors), so trying to flatten them into one generalized "guides" table would obscure the model and complicate queries.

Tables:

```text
clan_hunt_seasons
clan_hunt_bosses
void_of_shadow_stages
void_of_shadow_floors
tomb_of_the_fallen_dungeons
tomb_of_the_fallen_floors
```

If a future game mode genuinely shares structure with an existing one, introduce shared child tables only where they clearly reduce duplication. Do not pre-generalize.

### Team builder shares

Purpose:

- Store shareable team builder payloads.
- Allow URL-based sharing.

Likely table:

```text
team_builder_shares
```

Fields:

```text
id
share_code
payload_json
created_at_utc
expires_at_utc
version
```

Constraints/indexes:

```text
share_code unique
created_at_utc index
expires_at_utc index
```

Rules:

- Share code should be opaque and non-sequential.
- Payload size must be limited.
- Payload JSON should be validated before storage.
- Old shares can be pruned if needed.
- Do not store unnecessary personal data.

### Gear optimization cache

Purpose:

- Cache expensive gear optimization calculations.

Likely table:

```text
gear_optimization_cache_entries
```

Fields:

```text
id
input_hash
input_version
result_json
created_at_utc
last_accessed_at_utc
hit_count
expires_at_utc
```

Constraints/indexes:

```text
input_hash unique
expires_at_utc index
last_accessed_at_utc index
```

Rules:

- Normalize input before hashing.
- Include algorithm version in hash or record.
- Invalidate cache when optimization rules change.
- Log slow uncached calculations.

## JSON columns

PostgreSQL `jsonb` is acceptable for:

- Team builder share payloads.
- Gear optimization cached result payloads.
- Flexible imported content blocks where schema changes often.

Avoid `jsonb` for:

- Core hero fields used for filtering.
- Slugs.
- Published dates.
- Frequently joined relationships.
- Data that needs relational constraints.

## Indexing strategy

Add indexes for:

- Slugs.
- Foreign keys.
- Common filters.
- Published/sorted lists.
- Cache lookup hashes.
- Expiration/pruning jobs.

Do not over-index early.

Every index should support a known query or constraint.

## Constraints

Use database constraints for data integrity:

- `NOT NULL` for required fields.
- Unique constraints for slugs and share codes.
- Foreign keys for relationships.
- Check constraints for constrained values where practical.
- Maximum lengths for strings.

Domain/application validation is still needed, but database constraints protect data even if application code has a bug.

## Pagination

List endpoints should support pagination where lists may grow.

Recommended response shape:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 50,
  "totalCount": 123
}
```

For small static lists, simple arrays are acceptable.

## Backup and recovery

Production RDS requirements:

- Automated backups enabled.
- Retention period configured.
- Deletion protection enabled.
- Manual snapshot before major schema changes.
- Restore process documented.

Portfolio-level recovery target:

```text
RPO: 24 hours acceptable
RTO: manual restore acceptable
```

If the site becomes actively used, revisit these targets.

## Data privacy

This app should avoid collecting personal data unless needed.

Potential user-submitted data:

- Team builder share notes.
- Player names.
- Future account/profile data.

Rules:

- Keep text fields length-limited.
- Do not store IP addresses in application tables unless there is a clear reason.
- Do not log payload bodies by default.
- Provide deletion/pruning strategy for share payloads.

## Local development database

Use Docker Compose.

Expected local workflow:

```bash
docker compose up -d postgres
dotnet ef database update
dotnet run --project src/SilverHub.Api
```

Seed/import sample data with:

```bash
dotnet run --project tools/SilverHub.ContentImport -- --environment local
```

## Database testing

Use real PostgreSQL for integration tests.

Recommended tool:

```text
Testcontainers for .NET
```

Test categories:

- Repository query behavior.
- Migration application.
- Unique constraints.
- JSON payload persistence.
- Cache lookup behavior.
- Important indexes indirectly through query behavior.

## Database quality checklist

- [ ] EF Core migrations are committed.
- [ ] Production does not rely on dev seeders.
- [ ] Slugs have unique indexes.
- [ ] Foreign keys are defined.
- [ ] Common filters have indexes.
- [ ] Large payloads have size limits.
- [ ] JSON columns are used intentionally.
- [ ] Backups are enabled in production.
- [ ] Production RDS has deletion protection.
- [ ] Local database can be recreated from migrations and content imports.
- [ ] Migration process is documented.
