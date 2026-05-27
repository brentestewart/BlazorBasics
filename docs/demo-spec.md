# Component Lab — Demo Project Spec

> **Companion to:** `docs/talk-plan.md`. The plan covers what the talk teaches and in what order. This spec covers what to build so every teaching moment has a clean piece of code to land on.

## Scope

A single .NET 10 Blazor Web App that demonstrates the talk's full feature list using one small domain (LLM Capability Matrix — a leaderboard of LLMs with comparison submissions). The same core component is hosted on four pages with four different render modes so the audience can feel the differences live. Everything else — the form, routing, validation, persistent state — uses the same data and the same handful of components.

## Project structure

Two projects (matches the .NET 10 Blazor Web App template's `.Web` + `.Web.Client` split — no custom Shared project, because models in the `.Client` project are reachable from both sides and that's where `[ValidatableType]` needs them).

```
BlazorBasics.sln
├── BlazorBasics.Web/                      # Server host
│   ├── Components/
│   │   ├── App.razor                      # Root, hosts <Router NotFoundPage=... />
│   │   ├── Routes.razor
│   │   ├── _Imports.razor
│   │   ├── Layout/
│   │   │   ├── MainLayout.razor           # Mode-switcher nav lives here
│   │   │   └── NavMenu.razor
│   │   ├── Pages/
│   │   │   ├── StaticPage.razor           # /static    — no render mode
│   │   │   ├── ServerPage.razor           # /server    — @rendermode InteractiveServer
│   │   │   ├── WasmPage.razor             # /wasm      — @rendermode InteractiveWebAssembly
│   │   │   ├── AutoPage.razor             # /auto      — @rendermode InteractiveAuto
│   │   │   ├── SubmitComparison.razor     # /submit    — the form
│   │   │   ├── ComparisonDetail.razor     # /comparison/{id:int}
│   │   │   └── NotFound.razor             # wired via Router.NotFoundPage
│   │   └── Shared/
│   │       ├── Leaderboard.razor          # The reusable component, mounted on all 4 mode pages
│   │       ├── LeaderboardRow.razor       # Single LLM row with upvote button
│   │       ├── LlmCard.razor              # ChildContent / RenderFragment teaching
│   │       ├── RenderModeBadge.razor      # RendererInfo + AssignedRenderMode label
│   │       └── ResponseCard.razor         # Renders one LLM response in a comparison
│   ├── Services/
│   │   ├── ILlmService.cs                 # Server impl
│   │   ├── LlmService.cs
│   │   ├── IComparisonService.cs
│   │   └── ComparisonService.cs
│   └── Program.cs
└── BlazorBasics.Web.Client/                # WASM project
    ├── Models/                             # All shared models live here (.cs only)
    │   ├── Llm.cs
    │   ├── Comparison.cs
    │   ├── ComparisonResponse.cs
    │   └── Submitter.cs
    ├── Services/
    │   ├── LlmServiceClient.cs             # Client impl (calls minimal API on server)
    │   └── ComparisonServiceClient.cs
    ├── ValidationRegistration.cs           # AddValidation() extension for cross-assembly use
    ├── _Imports.razor
    └── Program.cs
```

## Data model

All models live in `BlazorBasics.Web.Client/Models/` as `.cs` files. Required by the .NET 10 source-gen validator and reachable from both projects.

```csharp
// Llm.cs
public class Llm
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Tagline { get; set; } = "";
    public int Votes { get; set; }
    public Dictionary<string, decimal> Scores { get; set; } = new();
}

// Comparison.cs
[ValidatableType]
public class Comparison
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Prompt is required.")]
    [MinLength(5, ErrorMessage = "Prompt must be at least 5 characters.")]
    public string Prompt { get; set; } = "";

    [Required(ErrorMessage = "Pick a category.")]
    public string Category { get; set; } = "";

    public Submitter Submitter { get; set; } = new();

    public List<ComparisonResponse> Responses { get; set; } = new();
}

// Submitter.cs
public class Submitter
{
    [Required(ErrorMessage = "Name is required.")]
    public string Name { get; set; } = "";

    [Required, EmailAddress(ErrorMessage = "Valid email required.")]
    public string Email { get; set; } = "";

    public string? FavoriteLlm { get; set; }
}

// ComparisonResponse.cs
public class ComparisonResponse
{
    public string LlmName { get; set; } = "";

    [Required(ErrorMessage = "Response text required.")]
    public string Text { get; set; } = "";

    [Range(0, 10)]
    public decimal Rating { get; set; }
}
```

The nested `Submitter` + the `List<ComparisonResponse>` collection are both deliberate — they're what makes source-gen validation traversal worth demonstrating. Reflection-based validation in .NET 9 wouldn't catch errors inside either.

## Pages and routing

| Route | File | Render mode | Purpose in the talk |
|---|---|---|---|
| `/static` | `StaticPage.razor` | none (Static SSR) | The "click does nothing" reveal; also where form post-back is shown |
| `/server` | `ServerPage.razor` | `InteractiveServer` | The throttled-latency demo |
| `/wasm` | `WasmPage.razor` | `InteractiveWebAssembly` | The instant-feedback mode used for most teaching |
| `/auto` | `AutoPage.razor` | `InteractiveAuto` | The synthesis (first slow, then fast) |
| `/submit` | `SubmitComparison.razor` | `InteractiveWebAssembly` (default) | The form + source-gen validation demo |
| `/comparison/{id:int}` | `ComparisonDetail.razor` | `InteractiveAuto` | Route parameters demo |
| (anything else) | `NotFound.razor` | static | Wired via `<Router NotFoundPage="typeof(NotFound)" />` |

The four mode pages are **near-identical** — each one just mounts `<Leaderboard />` and `<RenderModeBadge />`. The single line that differs is the `@rendermode` directive at the top.

```razor
@* WasmPage.razor *@
@page "/wasm"
@rendermode InteractiveWebAssembly

<Leaderboard />
<RenderModeBadge />
```

That sameness is the point — it lets you visually flip between the files mid-talk and show "this is the only line that's different."

## Components

### `Leaderboard.razor`

The heart of the demo. Hosted on all four mode pages.

- Injects `ILlmService`.
- Holds the LLM list in `[PersistentState] public List<Llm>? Llms { get; set; }`.
- Fetches in `OnInitializedAsync` if `Llms` is null.
- Renders a list via `@foreach`.
- Each row is a `<LeaderboardRow />` with an upvote `EventCallback`.
- Provides the rapid-click latency moment (upvote button).
- Demonstrates: DI, `[PersistentState]`, `@foreach`, fetched-on-load, event handling.

### `LeaderboardRow.razor`

Single LLM row. Pure parameter-driven component.

- `[Parameter] public Llm Llm { get; set; }`
- `[Parameter] public EventCallback<Llm> OnUpvote { get; set; }`
- Demonstrates: `[Parameter]`, `EventCallback<T>`, conditional rendering via `@if`.

### `LlmCard.razor`

Wrapper component with `ChildContent`. Used to teach `RenderFragment`. Wraps the leaderboard or a section of it during the component-model walkthrough.

- `[Parameter] public string Title { get; set; }`
- `[Parameter] public RenderFragment? ChildContent { get; set; }`
- Demonstrates: child content, composition.

### `RenderModeBadge.razor`

The label that's visible on every page. Lives in the layout footer or in each page.

```razor
<div class="render-mode-badge">
    rendering on: @RendererInfo.Name
    @if (AssignedRenderMode is not null) { <span>(@AssignedRenderMode)</span> }
    @if (RendererInfo.IsInteractive) { <span>interactive</span> } else { <span>static</span> }
</div>
```

- Demonstrates: `RendererInfo.Name`, `RendererInfo.IsInteractive`, `ComponentBase.AssignedRenderMode`.

### `ResponseCard.razor`

Renders one LLM response inside a comparison detail. Demonstrates `[Parameter]` and conditional CSS classes.

### `SubmitComparison.razor`

The form page. Uses `<EditForm Model="Model" OnValidSubmit="...">` with `<DataAnnotationsValidator />` and several `<InputText>` / `<InputSelect>` / `<ValidationMessage>` instances.

- Demonstrates: `EditForm`, `InputText`, `InputSelect`, `DataAnnotationsValidator`, `ValidationMessage`, nested-object validation, collection validation, source-gen validator traversal.
- The form is reachable from any of the four mode pages; mode-comparison happens by running the form under SSR (works via post-back) and under Interactive (works live).

### `ComparisonDetail.razor`

Shows a single comparison by id. Uses `[Parameter] public int Id { get; set; }` bound to the route parameter. Renders the comparison's responses via `<ResponseCard />` in a loop.

- Demonstrates: route parameters, page-level fetch, list rendering.

### `MainLayout.razor` + `NavMenu.razor`

The mode switcher lives here. Always visible. Four buttons: Static / Server / WASM / Auto. Plus a "Submit" button. `NavLink` instances; `NavLink active` class behavior is the .NET 10 default (ignores query/fragment).

## Services

### `ILlmService` / `LlmService` (server) / `LlmServiceClient` (client)

Returns the seed LLM list and handles upvote increments. Server impl holds the canonical state in a singleton. Client impl calls a minimal API on the server.

```csharp
public interface ILlmService
{
    Task<List<Llm>> GetLeaderboardAsync();
    Task<int> UpvoteAsync(int llmId);
}
```

Why both implementations: shows the audience that the *same* component (`Leaderboard.razor`) works under all four render modes because the service is registered on each side appropriately. This is the DI lesson with teeth.

### `IComparisonService` / `ComparisonService` / `ComparisonServiceClient`

Lists comparisons, fetches one by id, submits new ones. Same pattern as `LlmService`. Submission endpoint accepts the validated `Comparison` model.

### Minimal API endpoints in `Program.cs`

```csharp
app.MapGet("/api/llms", (ILlmService svc) => svc.GetLeaderboardAsync());
app.MapPost("/api/llms/{id}/upvote", (int id, ILlmService svc) => svc.UpvoteAsync(id));
app.MapGet("/api/comparisons", (IComparisonService svc) => svc.ListAsync());
app.MapGet("/api/comparisons/{id}", (int id, IComparisonService svc) => svc.GetAsync(id));
app.MapPost("/api/comparisons", (Comparison c, IComparisonService svc) => svc.SubmitAsync(c));
```

In-memory storage (singleton lists). No EF Core, no SQLite — that's noise for this talk.

## Render-mode wiring

In `BlazorBasics.Web/Program.cs`:

```csharp
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddValidation();   // source-gen validator
BlazorBasics.Web.Client.ValidationRegistration.AddClientValidation(builder.Services);

builder.Services.AddSingleton<ILlmService, LlmService>();
builder.Services.AddSingleton<IComparisonService, ComparisonService>();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorBasics.Web.Client._Imports).Assembly);

app.MapStaticAssets();
```

In `BlazorBasics.Web.Client/Program.cs`:

```csharp
builder.Services.AddScoped<ILlmService, LlmServiceClient>();
builder.Services.AddScoped<IComparisonService, ComparisonServiceClient>();
BlazorBasics.Web.Client.ValidationRegistration.AddClientValidation(builder.Services);
```

The cross-assembly validation pattern (per the research doc): `BlazorBasics.Web.Client/ValidationRegistration.cs` exposes an extension method that registers `AddValidation()` on both sides.

## Feature → file map

When teaching each concept, this is the file to open:

| Teaching moment | Files | Notes |
|---|---|---|
| Component model + `[Parameter]` | `LeaderboardRow.razor`, `LlmCard.razor` | Walk through markup + `@code` |
| `ChildContent` / `RenderFragment` | `LlmCard.razor` | Wrap the leaderboard briefly to show composition |
| `@bind` + `@bind:after` | `SubmitComparison.razor` (prompt field) | One-liner |
| `@onclick` + `EventCallback<T>` | `LeaderboardRow.razor` → `Leaderboard.razor` | Show parent-child event flow |
| `@if`, `@foreach` | `Leaderboard.razor` | Conditional star icon, list rendering |
| Routing + `@page` | `WasmPage.razor`, `ComparisonDetail.razor` | Show route attribute + parameter |
| `NavLink`, `NavigationManager` | `NavMenu.razor` | Mode switcher itself |
| `NotFoundPage` (Tier 2) | `App.razor`, `NotFound.razor` | Mention; navigate to a bad URL |
| `@inject` / typed `HttpClient` / service lifetimes | `Leaderboard.razor` + `LlmServiceClient.cs` | Show DI in component + the client service uses `HttpClient` |
| JS interop | `Leaderboard.razor` (upvote handler) | One `IJSRuntime.InvokeVoidAsync("console.log", ...)` line |
| Render modes (all four) | `StaticPage.razor`, `ServerPage.razor`, `WasmPage.razor`, `AutoPage.razor` | The four-file diff is the lesson |
| `RendererInfo` / `AssignedRenderMode` | `RenderModeBadge.razor` | The always-visible label |
| Static SSR form post-back | `SubmitComparison.razor` viewed from `/static` route | Form posts the old-school way |
| `EditForm` + `DataAnnotationsValidator` | `SubmitComparison.razor` | The form |
| `AddValidation()` + `[ValidatableType]` (Tier 2) | `Program.cs`, `Comparison.cs` | Trigger nested validation errors |
| `[PersistentState]` (Tier 2) | `Leaderboard.razor` | Toggle the attribute on/off to show before/after fetch behavior |
| WASM Hot Reload (Tier 2) | Any `.razor` file under `/wasm` | Live edit during demo |
| Auto mode behavior (Tier 2) | `AutoPage.razor` | First load slow, reload fast, badge flips |

## What I'm explicitly not building

- **Identity / auth scaffolding** — strip from the template. Auth is a different talk.
- **Database** — in-memory singleton state. SQLite would add DI complexity that doesn't earn its keep.
- **Real LLM API calls** — the demo never actually hits an LLM. The data is seeded.
- **Tests** — demo code, not production.
- **CI/CD config** — demo code.
- **Custom theming beyond what the template gives** — visual polish is the user's call; spec stays neutral.
- **`InputHidden`, QuickGrid, passkeys, JS interop constructor APIs, MAUI hybrid** — all on the cut list in the talk plan.

## Decided

- **No Identity / auth scaffolding.** Stripped from the .NET 10 Blazor Web App template entirely. Removes two pages, a DB context, migrations, and a fistful of services that have nothing to do with the talk.
- **One shared `Leaderboard` component for all four mode pages.** The four pages each just mount `<Leaderboard />` with a different `@rendermode`.

## Open technical decisions

1. **Singleton vs scoped for in-memory data on the server.** Singleton makes upvotes survive page navigation. Scoped wipes them per circuit. Singleton is the right answer for demo continuity, but worth a sentence on stage about real-world scoping.
2. **Seed dataset size.** Default: 4 LLMs, 3 prompt categories, 5–6 pre-seeded comparisons. Enough for visual variety, not so much that the screen is cluttered. User to confirm.
3. **JS interop placement.** Default: a `console.log` from `Leaderboard.razor` on upvote. Lowest-ceremony way to demonstrate the API without a side quest.
