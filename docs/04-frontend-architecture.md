# Frontend Architecture

## Purpose

This document defines the Angular frontend architecture for SilverHub.

The frontend should be structured as a production-style single-page application with clear feature boundaries, predictable API access, consistent UI states, and deployable static output.

## Technology choices

- Framework: Angular 22
- Language: TypeScript
- Build/test tooling: Node.js 24 (Active LTS)
- Styling: Tailwind CSS (primary) + Angular Material for complex controls only
- Hosting: S3 + CloudFront
- Testing: Angular unit/component tests and Playwright end-to-end tests
- Analytics: optional and privacy-conscious
- Build output: static files

## Application responsibilities

The frontend is responsible for:

- Displaying public game/wiki content.
- Routing between pages.
- Calling the backend API.
- Managing client-side UI state.
- Handling loading/error/empty states.
- Rendering tools/calculators.
- Supporting shareable tool links.
- Providing responsive layouts.

The frontend is not responsible for:

- Secrets.
- Database access.
- Trusted business validation.
- Expensive calculations that must be protected.
- Authoritative persistence.

## Routing structure

Recommended user-facing routes:

```text
/
 /news/:slug
 /heroes
 /heroes/new-releases
 /heroes/:slug
 /guides
 /guides/:type
 /guides/clan-hunt/:seasonSlug
 /guides/clan-hunt/:seasonSlug/:bossSlug
 /guides/void-of-shadow/:stageSlug
 /guides/void-of-shadow/:stageSlug/:floorNumber
 /guides/tomb-of-the-fallen/:dungeonSlug
 /guides/tomb-of-the-fallen/:dungeonSlug/:floorNumber
 /tools/bp-deficit-calculator
 /tools/gear-optimization
 /tools/team-builder
 /tools/team-builder/s/:id
 /tools/hero-breakdown-layout
```

Route names should be stable because they become public URLs.

If a public route changes, add redirect handling where practical.

## Folder structure

The Angular application lives at `src/SilverHub.Web/` in the monorepo; its
production build output is `dist/SilverHub.Web/browser`. Within the project, the
standard Angular layout applies:

```text
src/
  app/
    core/
      api/
      config/
      errors/
      logging/
    shared/
      components/
      pipes/
      directives/
      models/
      utils/
    layout/
      app-shell/
      nav/
      footer/
    features/
      home/
      news/
      heroes/
      artifacts/
      guides/
        clan-hunt/
        void-of-shadow/
        tomb-of-the-fallen/
      tools/
        bp-deficit/
        gear-optimization/
        team-builder/
        hero-breakdown-layout/
    app.routes.ts
    app.config.ts
  environments/
  assets/
```

Rules:

- `core` is for singletons and app-wide infrastructure.
- `shared` is for reusable presentational pieces.
- `features` contains feature-specific pages/components/services.
- Avoid putting all pages directly under one large `pages` folder as the project grows.
- Avoid feature code importing from unrelated feature internals.

## API access strategy

Centralize API access.

Recommended structure:

```text
core/api/
  api-client.ts
  api-error.ts
  api-url.ts

features/heroes/
  heroes-api.service.ts

features/tools/gear-optimization/
  gear-optimization-api.service.ts
```

The API base URL should come from environment/config:

```ts
apiBaseUrl: string
assetBaseUrl: string
```

Frontend code should not hardcode production AWS URLs throughout the app.

Use a single helper for API URLs:

```ts
buildApiUrl('/api/v1/heroes')
```

## API state model

Every API-backed page should handle:

```text
loading
success with data
empty data
not found
validation error
unexpected error
retry where useful
```

Avoid pages that silently fail or stay blank.

Recommended UI state type:

```ts
type LoadState<T> =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'success'; data: T }
  | { status: 'empty' }
  | { status: 'notFound' }
  | { status: 'error'; message: string };
```

## DTO and frontend model strategy

The backend owns API DTO contracts.

The frontend may define TypeScript interfaces that mirror API DTOs.

Recommended:

```text
features/heroes/models/hero-list-item.model.ts
features/heroes/models/hero-detail.model.ts
```

Keep API DTO models separate from rich UI-only view models if the UI transforms data.

Example:

```text
HeroListItemDto
  -> HeroCardViewModel
```

Do not assume backend database entity shape.

## Styling strategy

Hybrid approach (firm):

- **Tailwind CSS** owns layout, spacing, typography, and the great majority of visual styling.
- **Angular Material** is reserved for complex/accessibility-heavy controls only: dialogs, autocomplete, menus, date pickers, and similar components where building from scratch would mean reimplementing accessibility primitives.

Do not introduce a third component system. Do not use Material for buttons, cards, or layout — Tailwind handles those.

Define:

- spacing conventions
- typography conventions
- card/list patterns
- button variants
- form field patterns
- page header patterns
- loading skeleton patterns
- error alert patterns

## Component design

Prefer components that are:

- Small.
- Focused.
- Explicit about inputs/outputs.
- Easy to test.
- Not responsible for unrelated API calls.

Page components can coordinate data loading.

Presentational components should receive data via inputs.

Example split:

```text
HeroesListPage
  loads data and manages filters

HeroFilterPanel
  emits filter changes

HeroCardGrid
  displays list

HeroCard
  displays one hero
```

## Client-side state

Use the simplest state management that works.

Approach:

- Component-local state for page-only state.
- Services for shared feature state.
- **Angular Signals** as the primary reactive primitive for component and shared state.
- **RxJS** only where its model fits naturally: `HttpClient` request streams, router events, and event-stream APIs.
- Bridge between the two at the boundary with `toSignal` and `toObservable`. Inside components and stateful services, prefer signals.
- Avoid a global store (NgRx, etc.) unless the app clearly needs it.

Global/shared state candidates:

- current theme
- navigation state
- loaded hero options
- team builder working state
- user/session state if auth is added later

## Forms

Tool pages should use structured forms.

Requirements:

- Client-side validation for user experience.
- Server-side validation remains authoritative.
- Error messages are visible and specific.
- Invalid submissions are prevented where possible.
- Server validation errors are rendered in the relevant area.

Important forms:

- BP deficit calculator.
- Gear optimization.
- Team builder.
- Future content/admin forms.

## Tool pages

Tool pages are the most portfolio-interesting frontend features.

They should be implemented carefully.

### BP deficit calculator

- Pure client-side calculation is acceptable if the formula is not sensitive and cheap.
- If backend endpoint remains, frontend should show API-driven behavior and error handling.

### Gear optimization

- Submit calculation request to backend.
- Show loading state.
- Show validation errors.
- Show result summary.
- Show detailed result breakdown.
- Handle slow request state.
- Avoid duplicate submissions.
- Consider client-side request cancellation.

### Team builder

- Strong candidate for richer frontend architecture.
- Keep state management organized.
- Separate grid state, metadata, artifacts, ultimate order, and share logic.
- Support serialization/deserialization.
- Share links should load from backend and hydrate UI state.

## Asset loading

Assets should load from AWS S3/CloudFront.

Use a single asset base URL config value.

Example:

```ts
assetBaseUrl: 'https://silverhub.app/content-assets'
```

Assets live under the `/content-assets/*` path on the same CloudFront distribution as the frontend (see Doc 01). Staging uses `https://staging.silverhub.app/content-assets` similarly.

Asset references in API DTOs should be keys, not full provider-specific URLs.

Example DTO field:

```json
{
  "portraitImageKey": "heroes/noah/portrait.webp"
}
```

Frontend builds:

```text
assetBaseUrl + "/" + portraitImageKey
// e.g. https://silverhub.app/content-assets/heroes/noah/portrait.webp
```

Benefits:

- Backend/frontend are not hardcoded to S3 internals.
- Asset hosting can change later.
- Test environments can use different asset origins.

## SEO and social sharing

Because this is a public content/wiki site, SEO matters.

Angular SPA options:

1. Static SPA only.
2. Angular SSR/prerender.
3. Generate key content pages statically.

Recommended phased approach:

### Phase 1

Static SPA with good titles/descriptions where possible.

### Phase 2

Evaluate Angular prerender/SSR for:

- home page
- hero detail pages
- news articles
- guide pages

For a portfolio, documenting the tradeoff is enough at first. Implementing full SSR can be deferred unless SEO becomes a goal.

## Accessibility

Minimum requirements:

- Semantic headings.
- Keyboard-accessible navigation.
- Visible focus states.
- Form labels.
- Alt text for meaningful images.
- Decorative images marked appropriately.
- Color contrast checked.
- No clickable `div` when a `button` or `a` is appropriate.

Add automated accessibility checks later through Playwright/axe if practical.

## Performance

Frontend performance requirements:

- Production build budgets configured.
- Lazy-load feature areas where useful.
- Use optimized image formats such as WebP.
- Use CloudFront caching.
- Avoid loading every guide/tool dataset up front.
- Avoid unnecessary change detection churn.
- Avoid oversized third-party dependencies.

Measure:

- initial JS bundle size
- route-level load times
- Lighthouse performance
- CloudFront cache hit ratio if available

## Error handling

Frontend should distinguish:

```text
404 resource missing
400 validation issue
network/API unavailable
500 unexpected server issue
```

Show user-friendly messages but log enough detail in development.

Avoid exposing raw stack traces or server internals.

## Environment configuration

Use environment-specific config files or runtime config.

Required values:

```text
production
apiBaseUrl
assetBaseUrl
appVersion
```

Avoid committing environment files containing secrets.

Frontend values are public.

## Build/version display

Expose app version/build SHA somewhere non-invasive.

Options:

- Footer tooltip.
- `/about` page.
- hidden diagnostics panel.

This helps verify deployments.

## Frontend testing

Test levels:

- Utility unit tests.
- Component tests for meaningful UI logic.
- API service tests with mocked HTTP.
- Playwright smoke tests for full app behavior.

Do not try to test every visual detail.

Prioritize:

- tools/calculators
- team builder state
- API loading/error states
- route behavior
- forms

## Frontend quality checklist

- [ ] API base URL is centralized.
- [ ] Asset base URL is centralized.
- [ ] No secrets in frontend config.
- [ ] Routes are organized and stable.
- [ ] Loading/error/empty states exist.
- [ ] Forms validate client-side.
- [ ] Server validation errors are displayed.
- [ ] Major tools have tests.
- [ ] E2E smoke tests cover critical routes.
- [ ] Build output deploys to S3/CloudFront.
- [ ] CloudFront SPA fallback works.
- [ ] Accessibility basics are covered.
- [ ] Bundle size is monitored.
