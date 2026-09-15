# CommunityHelper — Frontend (Angular) Guidance

## Stack

- **Framework:** Angular 18+
- **Language:** TypeScript (strict mode enabled)
- **Styling:** Angular Material (CDK + component theming)
- **State:** NgRx (Store, Effects, Selectors, ComponentStore). Use NgRx Signal Store for feature-level state when applicable.
- **Testing:** Jasmine + Karma (unit), Playwright (e2e)
- **HTTP:** Angular `HttpClient` via a dedicated service layer

---

## Folder Structure

Feature-based folder structure. Group by domain feature, not by technical type.

```
frontend/
├── src/
│   ├── app/
│   │   ├── core/                  # App-wide singletons: auth, interceptors, guards
│   │   │   ├── auth/
│   │   │   ├── interceptors/
│   │   │   └── guards/
│   │   ├── shared/                # Reusable UI components, pipes, directives
│   │   │   ├── components/
│   │   │   ├── pipes/
│   │   │   └── directives/
│   │   ├── features/              # One folder per product feature
│   │   │   ├── community-posts/
│   │   │   │   ├── components/    # Dumb/presentational components
│   │   │   │   ├── pages/         # Smart/container components (routed)
│   │   │   │   ├── services/      # HTTP services for this feature
│   │   │   │   ├── models/        # TypeScript interfaces/types
│   │   │   │   └── community-posts.routes.ts
│   │   │   └── ...
│   │   ├── app.component.ts
│   │   ├── app.config.ts
│   │   └── app.routes.ts
│   ├── environments/
│   │   ├── environment.ts
│   │   └── environment.prod.ts
│   └── styles/                    # Global SCSS or Tailwind config entry
├── e2e/                           # Playwright tests
├── angular.json
├── package.json
└── tsconfig.json
```

---

## Component Architecture

### Standalone Components (Required)

Always use standalone components. Do not use NgModules for new code.

```typescript
@Component({
  selector: 'app-post-card',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './post-card.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PostCardComponent { }
```

Always set `changeDetection: ChangeDetectionStrategy.OnPush` on every component.

### Smart / Dumb Component Pattern

- **Smart (page/container) components** — routed components that own data fetching, service calls, and state. They pass data down via `@Input()` and listen for events via `@Output()`.
- **Dumb (presentational) components** — receive data via inputs, emit events via outputs. No service injection. No HTTP calls. No side effects.

A component that lives under `pages/` is smart. A component under `components/` is dumb. Enforce this strictly.

---

## State Management with NgRx

Use NgRx as the primary state management solution. Follow the standard NgRx pattern: Actions → Reducer → Selectors, with Effects for side effects.

### Store Structure

Organize state by feature. Each feature slice lives alongside its feature folder:

```
features/community-posts/
├── store/
│   ├── community-posts.actions.ts
│   ├── community-posts.reducer.ts
│   ├── community-posts.selectors.ts
│   └── community-posts.effects.ts
```

### Actions

Use `createActionGroup` to group related actions:

```typescript
export const CommunityPostsActions = createActionGroup({
  source: 'Community Posts',
  events: {
    'Load Posts': emptyProps(),
    'Load Posts Success': props<{ posts: CommunityPost[] }>(),
    'Load Posts Failure': props<{ error: string }>(),
    'Create Post': props<{ dto: CreatePostDto }>(),
    'Create Post Success': props<{ post: CommunityPost }>(),
    'Create Post Failure': props<{ error: string }>(),
  },
});
```

### Reducer

```typescript
export const communityPostsReducer = createReducer(
  initialState,
  on(CommunityPostsActions.loadPosts, (state) => ({ ...state, loading: true, error: null })),
  on(CommunityPostsActions.loadPostsSuccess, (state, { posts }) => ({ ...state, posts, loading: false })),
  on(CommunityPostsActions.loadPostsFailure, (state, { error }) => ({ ...state, error, loading: false })),
);
```

### Selectors

```typescript
export const selectPostsState = createFeatureSelector<PostsState>('communityPosts');
export const selectAllPosts = createSelector(selectPostsState, (s) => s.posts);
export const selectPostsLoading = createSelector(selectPostsState, (s) => s.loading);
```

### Effects

```typescript
loadPosts$ = createEffect(() =>
  this.actions$.pipe(
    ofType(CommunityPostsActions.loadPosts),
    switchMap(() =>
      this.postsService.getAll().pipe(
        map((posts) => CommunityPostsActions.loadPostsSuccess({ posts })),
        catchError((err) => of(CommunityPostsActions.loadPostsFailure({ error: err.message }))),
      ),
    ),
  ),
);
```

### NgRx Signal Store (for local/feature-scoped state)

Use `@ngrx/signals` (`signalStore`) for component-scoped or lightweight feature state that does not need to be shared globally.

### Rules
- Never mutate state directly — always return new objects in reducers.
- All async operations (HTTP, timers) go in Effects — never in reducers or components.
- Components select from the store via selectors — never access raw state slices directly.
- Avoid dispatching actions from inside Effects unless chaining is truly necessary.

---

## HTTP and Service Layer

**Never call `HttpClient` directly in a component.** All HTTP calls go through a feature service.

```typescript
// community-posts.service.ts
@Injectable({ providedIn: 'root' })
export class CommunityPostsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/community-posts`;

  getAll(): Observable<CommunityPost[]> {
    return this.http.get<CommunityPost[]>(this.baseUrl);
  }

  getById(id: string): Observable<CommunityPost> {
    return this.http.get<CommunityPost>(`${this.baseUrl}/${id}`);
  }

  create(dto: CreatePostDto): Observable<CommunityPost> {
    return this.http.post<CommunityPost>(this.baseUrl, dto);
  }
}
```

- Use `inject()` (functional injection) over constructor injection for new code.
- Handle errors in services or via a global HTTP interceptor, not in components.
- Use `HttpInterceptorFn` (functional interceptors) for auth headers, error normalization, and loading state.

---

## Routing

- Use lazy loading for all feature routes.
- Define routes in a `<feature>.routes.ts` file per feature.
- Use route guards (`CanActivateFn`) for protected routes.

```typescript
// app.routes.ts
export const routes: Routes = [
  {
    path: 'posts',
    loadChildren: () =>
      import('./features/community-posts/community-posts.routes')
        .then(m => m.COMMUNITY_POSTS_ROUTES),
  },
  { path: '', redirectTo: 'posts', pathMatch: 'full' },
  { path: '**', loadComponent: () => import('./shared/components/not-found/not-found.component').then(m => m.NotFoundComponent) },
];
```

---

## File Naming Conventions

| Artifact | Convention | Example |
|---|---|---|
| Component | `kebab-case.component.ts` | `post-card.component.ts` |
| Service | `kebab-case.service.ts` | `community-posts.service.ts` |
| Model/Interface | `kebab-case.model.ts` | `community-post.model.ts` |
| Route file | `kebab-case.routes.ts` | `community-posts.routes.ts` |
| Guard | `kebab-case.guard.ts` | `auth.guard.ts` |
| Interceptor | `kebab-case.interceptor.ts` | `auth.interceptor.ts` |
| Pipe | `kebab-case.pipe.ts` | `time-ago.pipe.ts` |
| Spec file | same name + `.spec.ts` | `post-card.component.spec.ts` |

Use `kebab-case` for all file and folder names. Do not use `camelCase` or `PascalCase` for filenames.

---

## Barrel Files

Avoid barrel `index.ts` files unless the public API of a shared library truly needs them. They cause circular dependency issues and slow down the TypeScript compiler on large projects. Import directly from the source file.

---

## TypeScript Conventions

- `strict: true` in `tsconfig.json` — no exceptions.
- Prefer `interface` over `type` for object shapes.
- Prefer `readonly` properties on models and DTOs.
- Never use `any`. Use `unknown` and narrow it, or define a proper type.
- Use `as const` for static lookup objects instead of enums.

---

## Testing Strategy

### Unit Tests (Jasmine + Karma)

- Every service must have a unit test.
- Every component with logic (computed values, event handlers) must have a unit test.
- Use `TestBed.configureTestingModule` with `provideHttpClientTesting` for services.
- For components, test via the DOM (query elements, trigger events) — not internal method calls.
- Mock all service dependencies in component tests.

```typescript
it('should display post title', () => {
  component.post = mockPost;
  fixture.detectChanges();
  const title = fixture.nativeElement.querySelector('[data-testid="post-title"]');
  expect(title.textContent).toContain(mockPost.title);
});
```

Use `data-testid` attributes for test selectors — never couple tests to CSS classes or element structure.

### E2E Tests (Playwright)

- Cover critical user journeys end to end (login, create post, view feed).
- Run against a local dev server or a dedicated test environment.
- Keep e2e tests in `e2e/` at the project root.
- Page Object Model (POM) pattern for reusable page interactions.

---

## Environment Configuration

Use `environment.ts` / `environment.prod.ts` for environment-specific values (API base URL, feature flags). Never hardcode URLs or keys in component or service files.

---

## Styling Guidelines

Use Angular Material exclusively for UI components. Do not introduce TailwindCSS or any other CSS framework.

- Define the app theme in `styles/` using the Angular Material theming API (`@use '@angular/material' as mat`).
- Use `mat.define-theme()` (M3) or `mat.define-light-theme()` (M2) — pick one Material version and stick to it.
- Do not override Material component styles with global CSS. Use the `::ng-deep` escape only as a last resort, and always scope it inside a host selector.
- Custom spacing, layout, and typography that Material does not cover should use plain SCSS variables defined in `styles/_variables.scss`.
- Use Angular CDK (`@angular/cdk`) for overlays, drag-and-drop, virtual scrolling, and accessibility utilities instead of third-party alternatives.
