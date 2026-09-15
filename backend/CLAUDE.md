# CommunityHelper — Backend (ASP.NET Core) Guidance

## Stack

- **Framework:** .NET 8+ (ASP.NET Core Web API)
- **Language:** C# (latest LTS version, nullable reference types enabled)
- **Architecture:** Clean Architecture
- **Pattern:** CQRS with MediatR
- **Database:** MongoDB
- **Driver:** MongoDB.Driver (official C# driver) + MongoDB.Driver.GridFS for file storage
- **Validation:** FluentValidation
- **Authentication:** JWT Bearer tokens
- **Logging:** Serilog (structured logging)
- **Testing:** xUnit + Moq, integration tests with `WebApplicationFactory`

---

## Solution Structure (Clean Architecture)

```
backend/
├── src/
│   ├── CommunityHelper.Domain/          # Entities, value objects, domain events, interfaces
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   ├── Events/
│   │   └── Interfaces/                  # Repository contracts, domain service interfaces
│   │
│   ├── CommunityHelper.Application/     # Use cases, CQRS handlers, DTOs, validators
│   │   ├── Common/
│   │   │   ├── Behaviours/              # MediatR pipeline behaviors (validation, logging)
│   │   │   ├── Exceptions/              # Application-level exceptions
│   │   │   └── Interfaces/              # Application service contracts
│   │   ├── Features/
│   │   │   └── CommunityPosts/
│   │   │       ├── Commands/
│   │   │       │   └── CreatePost/
│   │   │       │       ├── CreatePostCommand.cs
│   │   │       │       ├── CreatePostCommandHandler.cs
│   │   │       │       └── CreatePostCommandValidator.cs
│   │   │       ├── Queries/
│   │   │       │   └── GetPostById/
│   │   │       │       ├── GetPostByIdQuery.cs
│   │   │       │       ├── GetPostByIdQueryHandler.cs
│   │   │       │       └── PostDto.cs
│   │   │       └── EventHandlers/
│   │   └── DependencyInjection.cs
│   │
│   ├── CommunityHelper.Infrastructure/  # MongoDB, repositories, external services
│   │   ├── Persistence/
│   │   │   ├── MongoDbContext.cs        # IMongoDatabase wrapper / collection accessors
│   │   │   ├── Configurations/          # BsonClassMap registrations per document type
│   │   │   └── Documents/              # MongoDB document models (separate from domain entities)
│   │   ├── Repositories/               # Concrete repository implementations using IMongoCollection<T>
│   │   ├── Services/                    # Email, file storage, external API clients
│   │   └── DependencyInjection.cs
│   │
│   └── CommunityHelper.API/             # Controllers/minimal API endpoints, middleware, startup
│       ├── Controllers/
│       ├── Middleware/
│       │   └── ExceptionHandlingMiddleware.cs
│       ├── Filters/
│       ├── Program.cs
│       └── appsettings.json
│
└── tests/
    ├── CommunityHelper.Domain.Tests/
    ├── CommunityHelper.Application.Tests/
    ├── CommunityHelper.Infrastructure.Tests/
    └── CommunityHelper.API.IntegrationTests/
```

**Dependency rule:** Dependencies flow inward only.
`API` → `Application` → `Domain`
`Infrastructure` → `Application` → `Domain`
`Domain` has zero external dependencies.

---

## CQRS with MediatR

Every use case is a Command or a Query. There are no "service methods" in the Application layer — only handlers.

### Command Example

```csharp
// CreatePostCommand.cs
public record CreatePostCommand(string Title, string Body, Guid AuthorId) : IRequest<Guid>;

// CreatePostCommandHandler.cs
public class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, string>
{
    private readonly IPostRepository _posts;

    public CreatePostCommandHandler(IPostRepository posts) => _posts = posts;

    public async Task<string> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var post = CommunityPost.Create(request.Title, request.Body, request.AuthorId);
        await _posts.InsertAsync(post, cancellationToken);
        return post.Id;
    }
}
```

### Query Example

```csharp
// GetPostByIdQuery.cs
public record GetPostByIdQuery(string Id) : IRequest<PostDto?>;

// GetPostByIdQueryHandler.cs
public class GetPostByIdQueryHandler : IRequestHandler<GetPostByIdQuery, PostDto?>
{
    private readonly IPostRepository _posts;

    public GetPostByIdQueryHandler(IPostRepository posts) => _posts = posts;

    public async Task<PostDto?> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        var post = await _posts.FindByIdAsync(request.Id, cancellationToken);
        return post is null ? null : new PostDto(post.Id, post.Title, post.Body, post.CreatedAt);
    }
}
```

### MediatR Pipeline Behaviors

Register pipeline behaviors for cross-cutting concerns in order:

1. **LoggingBehaviour** — logs command/query name and execution time
2. **ValidationBehaviour** — runs FluentValidation validators; throws `ValidationException` on failure
3. **UnhandledExceptionBehaviour** — catches unexpected exceptions and logs them before rethrowing

---

## FluentValidation

Every Command and Query that accepts input must have a corresponding `AbstractValidator<T>`.

```csharp
public class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Body)
            .NotEmpty()
            .MaximumLength(10_000);

        RuleFor(x => x.AuthorId)
            .NotEmpty();
    }
}
```

Validators are registered automatically via assembly scanning. Do not call validators manually — they run via the MediatR pipeline behavior.

---

## API Layer

### Controllers vs Minimal APIs

Use **controller-based APIs** as the default for this project. They integrate cleanly with Swagger, filters, and model binding. Switch to minimal APIs only for specific high-throughput endpoints if profiling justifies it.

### Never Expose Domain Models in Responses

All API responses must use DTOs defined in the Application layer. Domain entities must never be serialized directly.

```csharp
// WRONG
[HttpGet("{id}")]
public async Task<CommunityPost> GetPost(Guid id) { ... }

// CORRECT
[HttpGet("{id}")]
public async Task<ActionResult<PostDto>> GetPost(Guid id, CancellationToken cancellationToken)
{
    var result = await _mediator.Send(new GetPostByIdQuery(id), cancellationToken);
    return result is null ? NotFound() : Ok(result);
}
```

### HTTP Status Codes

| Scenario | Code |
|---|---|
| Successful GET / UPDATE | 200 OK |
| Successful creation | 201 Created (with Location header) |
| Empty response | 204 No Content |
| Validation error | 400 Bad Request (Problem Details) |
| Unauthenticated | 401 Unauthorized |
| Authorized but forbidden | 403 Forbidden |
| Resource not found | 404 Not Found |
| Server error | 500 Internal Server Error |

---

## Global Exception Handling

Register a middleware that catches all unhandled exceptions and returns a consistent Problem Details response (RFC 7807). Never leak stack traces to clients in production.

```csharp
// Program.cs
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

Map known exception types to HTTP status codes:
- `ValidationException` → 400
- `NotFoundException` → 404
- `UnauthorizedAccessException` → 403
- Everything else → 500

---

## MongoDB

### Connection & Context

Register `IMongoDatabase` as a singleton. Wrap collection accessors in a `MongoDbContext` class in `Infrastructure/Persistence/`:

```csharp
// MongoDbContext.cs
public class MongoDbContext
{
    private readonly IMongoDatabase _db;

    public MongoDbContext(IOptions<MongoDbSettings> options)
    {
        var client = new MongoClient(options.Value.ConnectionString);
        _db = client.GetDatabase(options.Value.DatabaseName);
    }

    public IMongoCollection<PostDocument> Posts =>
        _db.GetCollection<PostDocument>("posts");
}
```

Store connection string and database name in `appsettings.json` under a `MongoDb` section — never hardcode them.

### Document Models

Keep MongoDB document models (`PostDocument`) separate from domain entities (`CommunityPost`). Documents live in `Infrastructure/Persistence/Documents/`. Map between them in repository implementations — Application and Domain layers never reference `BsonDocument` or driver types.

```csharp
// PostDocument.cs
public class PostDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = default!;

    public string Title { get; set; } = default!;
    public string Body { get; set; } = default!;
    public string AuthorId { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}
```

Register `BsonClassMap` in `Infrastructure/Persistence/Configurations/` — do not use `[BsonElement]` annotations on domain entities.

### Repository Pattern

MongoDB has no built-in Unit of Work, so the **Repository pattern is required**. Define interfaces in `Domain/Interfaces/` and implement them in `Infrastructure/Repositories/`:

```csharp
// Domain/Interfaces/IPostRepository.cs
public interface IPostRepository
{
    Task<CommunityPost?> FindByIdAsync(string id, CancellationToken ct = default);
    Task<List<CommunityPost>> GetAllAsync(CancellationToken ct = default);
    Task InsertAsync(CommunityPost post, CancellationToken ct = default);
    Task<bool> UpdateAsync(CommunityPost post, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
}

// Infrastructure/Repositories/PostRepository.cs
public class PostRepository : IPostRepository
{
    private readonly IMongoCollection<PostDocument> _collection;

    public PostRepository(MongoDbContext context) =>
        _collection = context.Posts;

    public async Task<CommunityPost?> FindByIdAsync(string id, CancellationToken ct = default)
    {
        var doc = await _collection
            .Find(p => p.Id == id)
            .FirstOrDefaultAsync(ct);
        return doc?.ToDomain();
    }

    public async Task InsertAsync(CommunityPost post, CancellationToken ct = default)
    {
        var doc = post.ToDocument();
        await _collection.InsertOneAsync(doc, cancellationToken: ct);
        post.SetId(doc.Id); // propagate generated ObjectId back to domain
    }
}
```

### Indexes

Define indexes in code at startup — never rely on MongoDB's default `_id` index alone:

```csharp
// Called from DependencyInjection.cs or a hosted service
var indexKeys = Builders<PostDocument>.IndexKeys.Ascending(p => p.AuthorId);
await collection.Indexes.CreateOneAsync(
    new CreateIndexModel<PostDocument>(indexKeys),
    cancellationToken: ct);
```

### Query Rules

- Always pass `CancellationToken` to all driver async calls.
- Use strongly-typed `FilterDefinition<T>` via `Builders<T>.Filter` — avoid raw BsonDocument filters.
- Use projections (`Project`) to fetch only required fields on read-heavy queries.
- Use `FindOneAndUpdateAsync` with `ReturnDocument.After` for atomic update-and-return operations.
- Avoid unbounded queries — always apply a limit or use cursor-based pagination.

---

## Authentication (JWT)

- Use ASP.NET Core JWT Bearer middleware.
- Store JWT secret, issuer, and audience in configuration (never in code).
- Access token lifetime: short (15 minutes). Refresh token lifetime: longer (7 days).
- Always validate `aud`, `iss`, and expiry.
- Use a custom `ICurrentUserService` (injected from `HttpContext`) to access the authenticated user's ID in handlers — do not access `HttpContext` directly in Application layer handlers.

---

## Structured Logging (Serilog)

```csharp
// Program.cs
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .WriteTo.Console(new JsonFormatter())
    .WriteTo.File("logs/log-.json", rollingInterval: RollingInterval.Day));
```

- Log at `Information` for normal operations.
- Log at `Warning` for handled errors and unexpected-but-recoverable conditions.
- Log at `Error` for unhandled exceptions.
- Never log passwords, tokens, or PII.
- Use structured properties, not string interpolation: `Log.Information("Post {PostId} created by {UserId}", postId, userId)`.

---

## Testing Strategy

### Unit Tests (xUnit + Moq)

- Test Application layer handlers in isolation. Mock repository interfaces with Moq — handlers never depend on `IMongoCollection<T>` directly.
- Test domain entity logic (invariants, domain methods) without any mocks.
- Test validators independently — pass valid and invalid inputs, assert rule failures.
- One test class per handler or domain object.

```csharp
public class CreatePostCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewPostId()
    {
        // Arrange
        var mockRepo = new Mock<IPostRepository>();
        mockRepo
            .Setup(r => r.InsertAsync(It.IsAny<CommunityPost>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreatePostCommandHandler(mockRepo.Object);
        var command = new CreatePostCommand("Title", "Body", Guid.NewGuid().ToString());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        mockRepo.Verify(r => r.InsertAsync(It.IsAny<CommunityPost>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

### Integration Tests (WebApplicationFactory)

- Test full HTTP request → response cycles including middleware, routing, validation, and auth.
- Use a dedicated MongoDB test database (spin up via `Testcontainers.MongoDb` or a local instance with a unique DB name per test run).
- Replace external service registrations with fakes/stubs in `WebApplicationFactory.ConfigureWebHost`.
- Cover happy paths and key error paths (validation failure, not found, unauthorized).

---

## Code Style

- Enable `<Nullable>enable</Nullable>` in all projects.
- Use `record` types for DTOs and Commands/Queries (immutable by default).
- Prefer `primary constructors` (C# 12) for simple DI.
- Use `var` when the type is obvious from the right-hand side.
- Async methods must have the `Async` suffix and accept `CancellationToken`.
- No `async void` — use `async Task`.
- Keep controllers thin — one line per action (send MediatR request, return result).
