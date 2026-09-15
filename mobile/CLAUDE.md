# CommunityHelper — Mobile (Flutter) Guidance

## Stack

- **Framework:** Flutter 3.x
- **Language:** Dart (null safety enforced — no `!` without justification)
- **State Management:** BLoC (flutter_bloc + bloc)
- **Navigation:** go_router
- **Models:** Freezed + json_serializable
- **HTTP:** Dio (via a repository layer — never call Dio directly in UI)
- **Testing:** flutter_test (widget + unit), integration_test package
- **Linting:** flutter_lints (strictly enforced)

---

## Folder Structure (Feature-First)

Group code by feature domain, not by technical layer. Each feature is self-contained.

```
mobile/
├── lib/
│   ├── main.dart
│   ├── app.dart                        # MaterialApp / router setup
│   ├── core/
│   │   ├── config/                     # AppConfig, environment constants
│   │   ├── error/                      # Failure types, exception handling
│   │   ├── network/                    # Dio client, interceptors
│   │   ├── router/                     # go_router configuration
│   │   └── theme/                      # App theme, color scheme, typography
│   ├── shared/
│   │   ├── widgets/                    # Reusable UI widgets (buttons, cards, etc.)
│   │   ├── extensions/                 # Dart extension methods
│   │   └── utils/                      # Pure utility functions
│   └── features/
│       └── community_posts/
│           ├── data/
│           │   ├── datasources/        # Remote (API) and local (cache) data sources
│           │   ├── models/             # Freezed + json_serializable API models
│           │   └── repositories/       # Repository implementations
│           ├── domain/
│           │   ├── entities/           # Pure Dart domain objects (optional for simple apps)
│           │   └── repositories/       # Repository interfaces (abstract classes)
│           ├── application/
│           │   └── bloc/               # BLoC classes (bloc, cubit, events, states)
│           └── presentation/
│               ├── screens/            # Full-screen widgets (one per route)
│               └── widgets/            # Feature-specific presentational widgets
├── test/
│   ├── unit/
│   ├── widget/
│   └── helpers/                        # Test factories, mock data
├── integration_test/
│   └── app_test.dart
├── pubspec.yaml
└── analysis_options.yaml
```

---

## Models with Freezed

All data models (API response models, domain entities) must use Freezed for immutability and copyWith support. Pair with json_serializable for JSON serialization.

```dart
// community_post.dart
import 'package:freezed_annotation/freezed_annotation.dart';

part 'community_post.freezed.dart';
part 'community_post.g.dart';

@freezed
class CommunityPost with _$CommunityPost {
  const factory CommunityPost({
    required String id,
    required String title,
    required String body,
    required String authorId,
    required DateTime createdAt,
  }) = _CommunityPost;

  factory CommunityPost.fromJson(Map<String, dynamic> json) =>
      _$CommunityPostFromJson(json);
}
```

Run code generation: `dart run build_runner build --delete-conflicting-outputs`

Set up a watch task during development: `dart run build_runner watch`

---

## State Management with BLoC

Use `flutter_bloc` as the primary state management solution. Follow the strict BLoC pattern: **Events → BLoC → States**. Use `Cubit` only for simple, local UI state with no complex event mapping.

### BLoC Structure

Each feature's BLoC lives in `features/<feature>/application/bloc/`:

```
application/bloc/
├── posts_bloc.dart         # BLoC class
├── posts_event.dart        # Sealed event classes
└── posts_state.dart        # Sealed state classes
```

### Events

```dart
// posts_event.dart
sealed class PostsEvent {
  const PostsEvent();
}

final class PostsLoadRequested extends PostsEvent {
  const PostsLoadRequested();
}

final class PostCreateRequested extends PostsEvent {
  const PostCreateRequested(this.dto);
  final CreatePostDto dto;
}
```

### States

```dart
// posts_state.dart
sealed class PostsState {
  const PostsState();
}

final class PostsInitial extends PostsState {
  const PostsInitial();
}

final class PostsLoading extends PostsState {
  const PostsLoading();
}

final class PostsLoaded extends PostsState {
  const PostsLoaded(this.posts);
  final List<CommunityPost> posts;
}

final class PostsFailure extends PostsState {
  const PostsFailure(this.message);
  final String message;
}
```

### BLoC

```dart
// posts_bloc.dart
class PostsBloc extends Bloc<PostsEvent, PostsState> {
  PostsBloc(this._repository) : super(const PostsInitial()) {
    on<PostsLoadRequested>(_onLoadRequested);
    on<PostCreateRequested>(_onCreateRequested);
  }

  final PostRepository _repository;

  Future<void> _onLoadRequested(
    PostsLoadRequested event,
    Emitter<PostsState> emit,
  ) async {
    emit(const PostsLoading());
    try {
      final posts = await _repository.getPosts();
      emit(PostsLoaded(posts));
    } catch (e) {
      emit(PostsFailure(e.toString()));
    }
  }

  Future<void> _onCreateRequested(
    PostCreateRequested event,
    Emitter<PostsState> emit,
  ) async {
    try {
      await _repository.createPost(event.dto);
      add(const PostsLoadRequested()); // refresh list
    } catch (e) {
      emit(PostsFailure(e.toString()));
    }
  }
}
```

### UI — BlocBuilder / BlocListener

```dart
BlocBuilder<PostsBloc, PostsState>(
  builder: (context, state) => switch (state) {
    PostsInitial() => const SizedBox.shrink(),
    PostsLoading() => const CircularProgressIndicator(),
    PostsLoaded(:final posts) => PostList(posts: posts),
    PostsFailure(:final message) => ErrorView(message: message),
  },
);
```

- Use `BlocBuilder` for rebuilding UI based on state.
- Use `BlocListener` for one-time side effects (navigation, snackbars) — never in `build`.
- Use `BlocConsumer` when you need both in the same widget.
- Provide BLoCs at the appropriate scope using `BlocProvider` — feature-level, not global unless truly shared.

### Cubit (for simple local state only)

```dart
class CounterCubit extends Cubit<int> {
  CounterCubit() : super(0);
  void increment() => emit(state + 1);
}
```

Use `Cubit` only for simple UI state (toggles, tab indices, form steps). Use `Bloc` for anything involving async operations or multiple event types.

### Rules

- All event and state classes must be `sealed` (exhaustive pattern matching).
- No business logic in widgets — dispatch events, consume states.
- Never call `emit` after an async gap without checking `isClosed`.
- Keep BLoCs testable: inject all dependencies via constructor.
- One BLoC per feature area — do not create a single monolithic BLoC.

---

## Navigation with go_router

Define all routes in `core/router/`. Use path-based routing. Avoid pushing raw `MaterialPageRoute` widgets.

```dart
// app_router.dart
final appRouter = GoRouter(
  initialLocation: '/posts',
  routes: [
    GoRoute(
      path: '/posts',
      builder: (context, state) => const PostsScreen(),
      routes: [
        GoRoute(
          path: ':id',
          builder: (context, state) => PostDetailScreen(
            id: state.pathParameters['id']!,
          ),
        ),
      ],
    ),
  ],
);
```

- Use `ShellRoute` for bottom navigation bar layouts.
- Pass only IDs in route parameters — fetch full objects in the destination screen via a provider.
- Use `GoRouter.of(context).go(...)` for navigation. Never use `Navigator.push` directly unless integrating a third-party flow.

---

## Repository Pattern

All data access goes through a repository interface. UI and providers never talk to Dio or a local database directly.

```dart
// domain/repositories/post_repository.dart
abstract class PostRepository {
  Future<List<CommunityPost>> getPosts();
  Future<CommunityPost> getPostById(String id);
  Future<CommunityPost> createPost(CreatePostDto dto);
}

// data/repositories/post_repository_impl.dart
class PostRepositoryImpl implements PostRepository {
  final PostRemoteDataSource _remote;

  PostRepositoryImpl(this._remote);

  @override
  Future<List<CommunityPost>> getPosts() async {
    final dtos = await _remote.fetchPosts();
    return dtos.map((d) => d.toDomain()).toList();
  }
}
```

Register the implementation via your DI solution (e.g., `get_it`):

```dart
// injection.dart
sl.registerLazySingleton<PostRepository>(
  () => PostRepositoryImpl(sl<PostRemoteDataSource>()),
);
sl.registerFactory(() => PostsBloc(sl<PostRepository>()));
```

---

## No Logic in UI Widgets

Widgets are for layout and presentation only. They do not:

- Make HTTP calls
- Parse data
- Contain `if/else` business rules
- Access SharedPreferences or local storage

If you find yourself writing business logic in a widget, move it to a BLoC or Cubit.

---

## Error Handling

Define a sealed `Failure` class hierarchy for typed error handling:

```dart
sealed class Failure {
  const Failure(this.message);
  final String message;
}

class NetworkFailure extends Failure {
  const NetworkFailure(super.message);
}

class ServerFailure extends Failure {
  const ServerFailure(super.message);
}

class NotFoundFailure extends Failure {
  const NotFoundFailure(super.message);
}
```

Repositories return `Failure` subtypes (or use `Result`/`Either` from `fpdart` if the team adopts it). Notifiers convert failures into user-visible error messages.

---

## Dart Null Safety Rules

- Never use the null-forgiving operator (`!`) without a comment explaining why the value is guaranteed non-null.
- Prefer `?.` and `??` for safe null handling.
- Use `required` on named parameters instead of making them nullable.
- Use `late` only for fields initialized before first use (e.g., in `initState`) — not as a workaround for null safety.

---

## File Naming Conventions

- All files: `snake_case.dart`
- All classes: `PascalCase`
- All variables, functions, parameters: `camelCase`
- Constants: `camelCase` (Dart convention) or `SCREAMING_SNAKE_CASE` for true compile-time constants

| Artifact | Suffix | Example |
|---|---|---|
| Screen widget | `_screen.dart` | `post_detail_screen.dart` |
| Reusable widget | `_widget.dart` or descriptive | `post_card.dart` |
| BLoC class | `_bloc.dart` | `posts_bloc.dart` |
| BLoC events | `_event.dart` | `posts_event.dart` |
| BLoC states | `_state.dart` | `posts_state.dart` |
| Cubit class | `_cubit.dart` | `counter_cubit.dart` |
| Freezed model | `_model.dart` or domain name | `community_post.dart` |
| Repository interface | `_repository.dart` | `post_repository.dart` |
| Repository impl | `_repository_impl.dart` | `post_repository_impl.dart` |
| Data source | `_data_source.dart` | `post_remote_data_source.dart` |

---

## Testing Strategy

### Widget Tests

Test that widgets render correctly and respond to user input. Provide a mock BLoC using `MockBloc` from `bloc_test`.

```dart
testWidgets('PostsScreen shows list when loaded', (tester) async {
  final mockBloc = MockPostsBloc();
  whenListen(
    mockBloc,
    Stream.fromIterable([PostsLoaded(fakePosts)]),
    initialState: const PostsLoading(),
  );

  await tester.pumpWidget(
    MaterialApp(
      home: BlocProvider<PostsBloc>.value(
        value: mockBloc,
        child: const PostsScreen(),
      ),
    ),
  );
  await tester.pump();

  expect(find.text('Test Post Title'), findsOneWidget);
});
```

### Unit Tests

Test BLoCs with `bloc_test` and repositories in isolation with `mocktail`.

```dart
blocTest<PostsBloc, PostsState>(
  'emits [Loading, Loaded] when PostsLoadRequested succeeds',
  build: () {
    when(() => mockRepository.getPosts()).thenAnswer((_) async => fakePosts);
    return PostsBloc(mockRepository);
  },
  act: (bloc) => bloc.add(const PostsLoadRequested()),
  expect: () => [const PostsLoading(), PostsLoaded(fakePosts)],
);
```

- Test repository implementations by mocking the data source.
- Test BLoCs using `bloc_test` — never test BLoC internals, only emitted states.

### Integration Tests

Use the `integration_test` package for full app flows on a real device or emulator. Cover:
- Login flow
- Core feature flows (create post, view feed)
- Navigation paths

---

## Linting

`analysis_options.yaml` must extend `flutter_lints`:

```yaml
include: package:flutter_lints/flutter.yaml

linter:
  rules:
    - always_declare_return_types
    - avoid_dynamic_calls
    - avoid_print
    - prefer_const_constructors
    - prefer_const_declarations
    - use_super_parameters
```

All lint warnings are treated as errors in CI. Do not suppress lints with `// ignore:` without a comment explaining why.

---

## Performance Guidelines

- Use `const` constructors wherever possible — this prevents unnecessary widget rebuilds.
- Use `ListView.builder` (not `ListView`) for lists of unknown or large length.
- Avoid rebuilding large widget trees — scope `BlocBuilder` as low in the tree as possible, and use `buildWhen` to filter unnecessary rebuilds.
- Profile with Flutter DevTools before optimizing. Do not optimize speculatively.
