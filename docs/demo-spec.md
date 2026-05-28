# Component Lab — Demo Project Spec

> **Companion to:** `docs/talk-plan.md`. The plan covers what the talk teaches and in what order. This spec covers what to build so every teaching moment has a clean piece of code to land on.

## Scope

A single .NET 10 Blazor Web App that demonstrates the talk's full feature list using one small domain (a dashboard of widgets, with a form for submitting widget requests). The same core component is hosted on four pages with four different render modes so the audience can feel the differences live. Everything else — the form, routing, validation, persistent state — uses the same data and the same handful of components.

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
│   │   │   ├── SubmitWidgetRequest.razor  # /submit    — the form
│   │   │   ├── WidgetDetail.razor         # /widget/{id:int}
│   │   │   └── NotFound.razor             # wired via Router.NotFoundPage
│   │   └── Shared/
│   │       ├── Dashboard.razor            # The reusable component, mounted on all 4 mode pages
│   │       ├── Widget.razor               # Single widget tile with refresh button
│   │       ├── WidgetCard.razor           # ChildContent / RenderFragment teaching
│   │       ├── RenderModeBadge.razor      # RendererInfo + AssignedRenderMode label
│   │       └── WidgetMetricCard.razor     # Renders one metric inside a widget detail
│   ├── Services/
│   │   ├── IDashboardService.cs           # Server impl
│   │   ├── DashboardService.cs
│   │   ├── IWidgetRequestService.cs
│   │   └── WidgetRequestService.cs
│   └── Program.cs
└── BlazorBasics.Web.Client/                # WASM project
    ├── Models/                             # All shared models live here (.cs only)
    │   ├── WidgetData.cs
    │   ├── WidgetRequest.cs
    │   ├── WidgetMetric.cs
    │   └── Submitter.cs
    ├── Services/
    │   ├── DashboardServiceClient.cs       # Client impl (calls minimal API on server)
    │   └── WidgetRequestServiceClient.cs
    ├── ValidationRegistration.cs           # AddValidation() extension for cross-assembly use
    ├── _Imports.razor
    └── Program.cs
```

## Data model

All models live in `BlazorBasics.Web.Client/Models/` as `.cs` files. Required by the .NET 10 source-gen validator and reachable from both projects.

```csharp
// WidgetData.cs
public class WidgetData
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal Value { get; set; }
    public Dictionary<string, decimal> Metrics { get; set; } = new();
}

// WidgetRequest.cs
[ValidatableType]
public class WidgetRequest
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [MinLength(3, ErrorMessage = "Title must be at least 3 characters.")]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Pick a category.")]
    public string Category { get; set; } = "";

    public Submitter Submitter { get; set; } = new();

    public List<WidgetMetric> Metrics { get; set; } = new();
}

// Submitter.cs
public class Submitter
{
    [Required(ErrorMessage = "Name is required.")]
    public string Name { get; set; } = "";

    [Required, EmailAddress(ErrorMessage = "Valid email required.")]
    public string Email { get; set; } = "";

    public string? Department { get; set; }
}

// WidgetMetric.cs
public class WidgetMetric
{
    [Required(ErrorMessage = "Metric name required.")]
    public string Name { get; set; } = "";

    [Range(0, double.MaxValue, ErrorMessage = "Value must be non-negative.")]
    public decimal Value { get; set; }
}
```

The nested `Submitter` + the `List<WidgetMetric>` collection are both deliberate — they're what makes source-gen validation traversal worth demonstrating. Reflection-based validation in .NET 9 wouldn't catch errors inside either.

## Pages and routing

| Route | File | Render mode | Purpose in the talk |
|---|---|---|---|
| `/static` | `StaticPage.razor` | none (Static SSR) | The "click does nothing" reveal; also where form post-back is shown |
| `/server` | `ServerPage.razor` | `InteractiveServer` | The throttled-latency demo |
| `/wasm` | `WasmPage.razor` | `InteractiveWebAssembly` | The instant-feedback mode used for most teaching |
| `/auto` | `AutoPage.razor` | `InteractiveAuto` | The synthesis (first slow, then fast) |
| `/submit` | `SubmitWidgetRequest.razor` | `InteractiveWebAssembly` (default) | The form + source-gen validation demo |
| `/widget/{id:int}` | `WidgetDetail.razor` | `InteractiveAuto` | Route parameters demo |
| (anything else) | `NotFound.razor` | static | Wired via `<Router NotFoundPage="typeof(NotFound)" />` |

The four mode pages are **near-identical** — each one just mounts `<Dashboard />` and `<RenderModeBadge />`. The single line that differs is the `@rendermode` directive at the top.

```razor
@* WasmPage.razor *@
@page "/wasm"
@rendermode InteractiveWebAssembly

<Dashboard />
<RenderModeBadge />
```

That sameness is the point — it lets you visually flip between the files mid-talk and show "this is the only line that's different."

## Components

### `Dashboard.razor`

The heart of the demo. Hosted on all four mode pages.

- Injects `IDashboardService`.
- Holds the widget list in `[PersistentState] public List<WidgetData>? Widgets { get; set; }`.
- Fetches in `OnInitializedAsync` if `Widgets` is null.
- Renders a grid via `@foreach`.
- Each tile is a `<Widget />` with a refresh `EventCallback`.
- Provides the rapid-click latency moment (refresh button).
- Demonstrates: DI, `[PersistentState]`, `@foreach`, fetched-on-load, event handling.

### `Widget.razor`

Single widget tile. Pure parameter-driven component.

- `[Parameter] public WidgetData Data { get; set; }`
- `[Parameter] public EventCallback<WidgetData> OnRefresh { get; set; }`
- Demonstrates: `[Parameter]`, `EventCallback<T>`, conditional rendering via `@if`.

### `WidgetCard.razor`

Wrapper component with `ChildContent`. Used to teach `RenderFragment`. Wraps a widget or a section of the dashboard during the component-model walkthrough.

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

### `WidgetMetricCard.razor`

Renders one metric inside a widget detail page. Demonstrates `[Parameter]` and conditional CSS classes.

### `SubmitWidgetRequest.razor`

The form page. Uses `<EditForm Model="Model" OnValidSubmit="...">` with `<DataAnnotationsValidator />` and several `<InputText>` / `<InputSelect>` / `<ValidationMessage>` instances.

- Demonstrates: `EditForm`, `InputText`, `InputSelect`, `DataAnnotationsValidator`, `ValidationMessage`, nested-object validation, collection validation, source-gen validator traversal.
- The form is reachable from any of the four mode pages; mode-comparison happens by running the form under SSR (works via post-back) and under Interactive (works live).

### `WidgetDetail.razor`

Shows a single widget by id. Uses `[Parameter] public int Id { get; set; }` bound to the route parameter. Renders the widget's metrics via `<WidgetMetricCard />` in a loop.

- Demonstrates: route parameters, page-level fetch, list rendering.

### `MainLayout.razor` + `NavMenu.razor`

The mode switcher lives here. Always visible. Four buttons: Static / Server / WASM / Auto. Plus a "Submit" button. `NavLink` instances; `NavLink active` class behavior is the .NET 10 default (ignores query/fragment).

## Services

### `IDashboardService` / `DashboardService` (server) / `DashboardServiceClient` (client)

Returns the seed widget list and handles refresh actions. Server impl holds the canonical state in a singleton. Client impl calls a minimal API on the server.

```csharp
public interface IDashboardService
{
    Task<List<WidgetData>> GetWidgetsAsync();
    Task<WidgetData> RefreshAsync(int widgetId);
}
```

Why both implementations: shows the audience that the *same* component (`Dashboard.razor`) works under all four render modes because the service is registered on each side appropriately. This is the DI lesson with teeth.

### `IWidgetRequestService` / `WidgetRequestService` / `WidgetRequestServiceClient`

Lists widget requests, fetches one by id, submits new ones. Same pattern as `DashboardService`. Submission endpoint accepts the validated `WidgetRequest` model.

### Minimal API endpoints in `Program.cs`

```csharp
app.MapGet("/api/widgets", (IDashboardService svc) => svc.GetWidgetsAsync());
app.MapPost("/api/widgets/{id}/refresh", (int id, IDashboardService svc) => svc.RefreshAsync(id));
app.MapGet("/api/widget-requests", (IWidgetRequestService svc) => svc.ListAsync());
app.MapGet("/api/widget-requests/{id}", (int id, IWidgetRequestService svc) => svc.GetAsync(id));
app.MapPost("/api/widget-requests", (WidgetRequest r, IWidgetRequestService svc) => svc.SubmitAsync(r));
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

builder.Services.AddSingleton<IDashboardService, DashboardService>();
builder.Services.AddSingleton<IWidgetRequestService, WidgetRequestService>();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorBasics.Web.Client._Imports).Assembly);

app.MapStaticAssets();
```

In `BlazorBasics.Web.Client/Program.cs`:

```csharp
builder.Services.AddScoped<IDashboardService, DashboardServiceClient>();
builder.Services.AddScoped<IWidgetRequestService, WidgetRequestServiceClient>();
BlazorBasics.Web.Client.ValidationRegistration.AddClientValidation(builder.Services);
```

The cross-assembly validation pattern (per the research doc): `BlazorBasics.Web.Client/ValidationRegistration.cs` exposes an extension method that registers `AddValidation()` on both sides.

## Feature → file map

When teaching each concept, this is the file to open:

| Teaching moment | Files | Notes |
|---|---|---|
| Component model + `[Parameter]` | `Widget.razor`, `WidgetCard.razor` | Walk through markup + `@code` |
| `ChildContent` / `RenderFragment` | `WidgetCard.razor` | Wrap a widget briefly to show composition |
| `@bind` + `@bind:after` | `SubmitWidgetRequest.razor` (title field) | One-liner |
| `@onclick` + `EventCallback<T>` | `Widget.razor` → `Dashboard.razor` | Show parent-child event flow |
| `@if`, `@foreach` | `Dashboard.razor` | Conditional status icon, list rendering |
| Routing + `@page` | `WasmPage.razor`, `WidgetDetail.razor` | Show route attribute + parameter |
| `NavLink`, `NavigationManager` | `NavMenu.razor` | Mode switcher itself |
| `NotFoundPage` (Tier 2) | `App.razor`, `NotFound.razor` | Mention; navigate to a bad URL |
| `@inject` / typed `HttpClient` / service lifetimes | `Dashboard.razor` + `DashboardServiceClient.cs` | Show DI in component + the client service uses `HttpClient` |
| JS interop | `Dashboard.razor` (refresh handler) | One `IJSRuntime.InvokeVoidAsync("console.log", ...)` line |
| Render modes (all four) | `StaticPage.razor`, `ServerPage.razor`, `WasmPage.razor`, `AutoPage.razor` | The four-file diff is the lesson |
| `RendererInfo` / `AssignedRenderMode` | `RenderModeBadge.razor` | The always-visible label |
| Static SSR form post-back | `SubmitWidgetRequest.razor` viewed from `/static` route | Form posts the old-school way |
| `EditForm` + `DataAnnotationsValidator` | `SubmitWidgetRequest.razor` | The form |
| `AddValidation()` + `[ValidatableType]` (Tier 2) | `Program.cs`, `WidgetRequest.cs` | Trigger nested validation errors |
| `[PersistentState]` (Tier 2) | `Dashboard.razor` | Toggle the attribute on/off to show before/after fetch behavior |
| WASM Hot Reload (Tier 2) | Any `.razor` file under `/wasm` | Live edit during demo |
| Auto mode behavior (Tier 2) | `AutoPage.razor` | First load slow, reload fast, badge flips |

## What I'm explicitly not building

- **Identity / auth scaffolding** — strip from the template. Auth is a different talk.
- **Database** — in-memory singleton state. SQLite would add DI complexity that doesn't earn its keep.
- **Real backend data sources** — the demo never actually hits an external service. The data is seeded.
- **Tests** — demo code, not production.
- **CI/CD config** — demo code.
- **Custom theming beyond what the template gives** — visual polish is the user's call; spec stays neutral.
- **`InputHidden`, QuickGrid, passkeys, JS interop constructor APIs, MAUI hybrid** — all on the cut list in the talk plan.

## Decided

- **No Identity / auth scaffolding.** Stripped from the .NET 10 Blazor Web App template entirely. Removes two pages, a DB context, migrations, and a fistful of services that have nothing to do with the talk.
- **One shared `Dashboard` component for all four mode pages.** The four pages each just mount `<Dashboard />` with a different `@rendermode`.

## Open technical decisions

1. **Singleton vs scoped for in-memory data on the server.** Singleton makes widget state survive page navigation. Scoped wipes per circuit. Singleton is the right answer for demo continuity, but worth a sentence on stage about real-world scoping.
2. **Seed dataset size.** Default: 6 widgets across 3 categories, 5–6 pre-seeded widget requests. Enough for visual variety, not so much that the screen is cluttered. User to confirm.
3. **JS interop placement.** Default: a `console.log` from `Dashboard.razor` on refresh. Lowest-ceremony way to demonstrate the API without a side quest.
