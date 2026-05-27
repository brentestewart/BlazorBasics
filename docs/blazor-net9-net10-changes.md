# Blazor Changes in .NET 9 and .NET 10

> **Purpose:** Research notes compiled to update a "Blazor Basics" conference talk that was originally written against .NET 8. This document covers everything new, changed, or removed in Blazor for .NET 9 and .NET 10. Sources are Microsoft Learn (`learn.microsoft.com/aspnet/core/release-notes/`), the ASP.NET Core team blog posts, and Telerik/community write-ups.
>
> **Audience for the talk:** Professional .NET developers already familiar with .NET 8 Blazor (Blazor Web App template, render modes, SSR, enhanced navigation).

---

## .NET 9 — The "Polish Pass" Release

After the seismic shift in .NET 8 (unified Blazor Web App template, render modes, SSR), .NET 9 is intentionally a smoothing-the-edges release. Upgrading from .NET 8 is straightforward — minimal project file changes.

**Important context:** .NET 9 is a Standard Term Support (STS) release — 18 months of support. .NET 8 (LTS) gets 3 years. Most production shops likely skipped 9 and went 8 → 10 directly.

### Reconnection UX (Blazor Server)

The most user-visible change in .NET 9. The default reconnection UI got exponential backoff, and Blazor now attempts to restore the original SignalR circuit so users don't lose state. If the circuit is dead on the server, it does a full page refresh to ensure client and server state match.

Retry interval is configurable:

```javascript
Blazor.start({
  circuit: {
    reconnectionOptions: {
      retryIntervalMilliseconds: (previousAttempts, maxRetries) =>
        previousAttempts >= maxRetries ? null : previousAttempts * 1000
    },
  },
});
```

### `MapStaticAssets` middleware

Replaces the old `UseStaticFiles` story for most cases. Compresses assets via Gzip, fingerprints all assets at build time with a Base64 hash, and removes caches when Visual Studio Hot Reload is in action. Solves the cache-busting problem cleanly.

### Render-mode introspection

Three new APIs that finally let a component know where and how it's executing:

- `RendererInfo.Name` — where the component is executing
- `RendererInfo.IsInteractive` — whether the component supports interactivity at render time
- `ComponentBase.AssignedRenderMode` — the assigned render mode after prerendering

This is the answer to the "am I prerendering right now?" question everyone was asking in .NET 8. Useful for guarding code paths that can't run during prerender.

### Simplified auth state serialization

New APIs in the Blazor Web App template that serialize auth state on the server and deserialize it in the browser, removing the boilerplate `PersistAuthenticationState` / `AuthenticationStateDeserialization` files that .NET 8 templates included. Clean up two files and auth still works the same.

### Static SSR pages in globally interactive apps

Adding static SSR pages to a globally-interactive Blazor Web App got simpler in 9. The .NET 8 story here was awkward.

### .NET MAUI Blazor Hybrid + Web App template

New solution template that scaffolds both projects sharing a Razor Class Library for components. Targets Android, iOS, Mac, Windows, and Web with a shared UI. New starting point for hybrid apps.

---

## .NET 10 — LTS and a Substantial Release

GA shipped November 11, 2025. LTS — supported until 2028. This is the release the talk audience cares about.

### State management — the headline feature

#### `[PersistentState]` attribute

Before .NET 10, persisting state across the prerender → interactive boundary required injecting `PersistentComponentState`, subscribing in `OnInitializedAsync`, calling `TryTakeFromJson`, registering an `OnPersisting` callback, and implementing `IDisposable`. About 20 lines of ceremony per component.

**Before (.NET 8/9):**

```razor
@page "/movies"
@implements IDisposable
@inject IMovieService MovieService
@inject PersistentComponentState ApplicationState

@code {
    public List<Movie>? MoviesList { get; set; }
    private PersistingComponentStateSubscription? persistingSubscription;

    protected override async Task OnInitializedAsync()
    {
        if (!ApplicationState.TryTakeFromJson<List<Movie>>(nameof(MoviesList),
            out var movies))
        {
            MoviesList = await MovieService.GetMoviesAsync();
        }
        else
        {
            MoviesList = movies;
        }

        persistingSubscription = ApplicationState.RegisterOnPersisting(() =>
        {
            ApplicationState.PersistAsJson(nameof(MoviesList), MoviesList);
            return Task.CompletedTask;
        });
    }

    public void Dispose() => persistingSubscription?.Dispose();
}
```

**After (.NET 10):**

```razor
@page "/movies"
@inject IMovieService MovieService

@code {
    [PersistentState]
    public List<Movie>? MoviesList { get; set; }

    protected override async Task OnInitializedAsync()
    {
        MoviesList ??= await MovieService.GetMoviesAsync();
    }
}
```

The framework uses reflection on public properties marked with the attribute. **Properties must be `public`** (reflection is used for trimming and source generation).

State can also be registered at the service level via `RegisterPersistentService` on the Razor components builder.

**Useful knobs on `[PersistentState]`:**

- `AllowUpdates = true` — lets state update during enhanced navigation (default is load-once to protect things like in-flight form data)
- `RestoreBehavior.SkipInitialValue` — skip restore during prerendering
- `RestoreBehavior.SkipLastSnapshot` — skip restore on reconnect (forces fresh data)

```csharp
[PersistentState(AllowUpdates = true)]
public WeatherForecast[]? Forecasts { get; set; }

[PersistentState(RestoreBehavior = RestoreBehavior.SkipInitialValue)]
public string NoPrerenderedData { get; set; }

[PersistentState(RestoreBehavior = RestoreBehavior.SkipLastSnapshot)]
public int CounterNotRestoredOnReconnect { get; set; }
```

Call `PersistentComponentState.RegisterOnRestoring` for imperative control over restoration, similar to the existing `RegisterOnPersisting`.

#### Circuit state persistence (Blazor Server)

Separate from `[PersistentState]`. The server can now persist a user's circuit state when the connection drops for an extended period or is proactively paused. Scenarios:

- Browser tab throttling
- Mobile device users switching apps
- Network interruptions
- Proactive resource management (pausing inactive circuits)
- Enhanced navigation

As long as there's no full-page refresh, users come back to their session intact.

#### Custom persistent state serializers

Register `PersistentComponentStateSerializer<T>` for types where default JSON serialization isn't right:

```csharp
builder.Services.AddSingleton<PersistentComponentStateSerializer<TUser>,
    CustomUserSerializer>();
```

Then mark properties with `[PersistentState]` as usual.

#### Persistent state support for enhanced navigation

By default, persistent state is only loaded by interactive components when they're initially loaded on the page (prevents overwriting things like in-flight form data). Set `AllowUpdates = true` to opt into updates during enhanced navigation — useful for read-only cached data.

### Validation — finally usable for real models

Pre-.NET 10 form validation was reflection-based and didn't traverse nested objects or collections. .NET 10 introduces a source-generator-based validation system.

**Setup:**

1. `builder.Services.AddValidation();` in `Program.cs`
2. Mark the root model with `[ValidatableType]`
3. **Define the model in a `.cs` file, NOT in a `.razor` file** (source generators can't chain, and the Razor compiler is itself a source generator)
4. Use `DataAnnotationsValidator` in `EditForm` as before

**Example:**

`Program.cs`:

```csharp
builder.Services.AddValidation();
```

`Order.cs`:

```csharp
[ValidatableType]
public class Order
{
    public Customer Customer { get; set; } = new();
    public List<OrderItem> OrderItems { get; set; } = [];
}

public class Customer
{
    [Required(ErrorMessage = "Name is required.")]
    public string? FullName { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    public string? Email { get; set; }

    public ShippingAddress ShippingAddress { get; set; } = new();
}
```

`OrderPage.razor`:

```razor
<EditForm Model="Model">
    <DataAnnotationsValidator />

    <h3>Customer Details</h3>
    <div class="mb-3">
        <label>
            Full Name
            <InputText @bind-Value="Model!.Customer.FullName" />
        </label>
        <ValidationMessage For="@(() => Model!.Customer.FullName)" />
    </div>
</EditForm>
```

**Features:**

- Validates nested complex objects and collection items
- Validates rules defined by property attributes, class attributes, and `IValidatableObject` implementations
- `[SkipValidation]` attribute excludes properties/types
- Source-generator-based — AOT-compatible, faster than reflection
- Matches `System.ComponentModel.DataAnnotations.Validator` ordering and short-circuiting: member properties → type-level attributes → `IValidatableObject.Validate`

**Cross-assembly validation models:** Validate forms with models defined in a library or the `.Client` project by creating a method in that project that takes `IServiceCollection` and calls `AddValidation` on it. Then call both that method and `AddValidation` from the host app.

### Routing changes

#### `NotFoundPage` parameter on `Router`

Replaces the old `<NotFound>` render fragment, which is **no longer supported in .NET 10**. New templates include a `NotFound.razor` by default.

```razor
<Router AppAssembly="@typeof(Program).Assembly" NotFoundPage="typeof(Pages.NotFound)">
    <Found Context="routeData">
        <RouteView RouteData="@routeData" />
        <FocusOnNavigate RouteData="@routeData" Selector="h1" />
    </Found>
</Router>
```

#### `NavigationManager.NotFound()` method

Programmatic 404 from anywhere:

- Static SSR: sets HTTP 404
- Interactive rendering: signals the router
- Streaming rendering: swaps in Not Found content without a page reload (when enhanced navigation is active)

Rendering precedence:

1. If `NotFoundEventArgs.Path` is set, render that page
2. If `Router.NotFoundPage` is set, render that page
3. Status Code Pages Re-execution Middleware page, if configured
4. No action otherwise

Subscribe to `NavigationManager.OnNotFound` for notifications. For apps with custom routers, use `UseStatusCodePagesWithReExecute` or subscribe to `OnNotFound` and set `NotFoundEventArgs.Path`.

#### `NavigateTo` no longer scrolls to top on same-page navigations

Behavior change. Updating the query string or fragment for the current page no longer resets the viewport. Worth flagging in any migration slide — the kind of thing QA catches.

#### `NavLinkMatch.All` ignores query string and fragment

A `NavLink` keeps its `active` class when only the query/fragment change. Opt out via the `Microsoft.AspNetCore.Components.Routing.NavLink.EnableMatchAllForQueryStringAndFragment` `AppContext` switch, or by overriding `ShouldMatch`:

```csharp
public class CustomNavLink : NavLink
{
    protected override bool ShouldMatch(string currentUriAbsolute)
    {
        // Custom matching logic
    }
}
```

#### Static SSR `NavigateTo` no longer throws by default

The new template sets `<BlazorDisableThrowNavigationException>true</BlazorDisableThrowNavigationException>`, making static SSR navigation behave consistently with interactive rendering. The old `NavigationException` throw pattern is gone from the default `IdentityRedirectManager` — remove the `[DoesNotReturn]` attributes and the `InvalidOperationException` after `RedirectTo` when migrating.

### WebAssembly performance

#### Blazor script as a static web asset

`blazor.web.js` / `blazor.server.js` / `blazor.webassembly.js` are now served as static web assets with automatic compression and fingerprinting instead of being embedded in the framework.

Often-cited number: **76% reduction in the core `blazor.web.js` script, from 183 KB down to about 43 KB.**

If a project doesn't contain at least one `.razor` file but still needs the Blazor script:

```xml
<RequiresAspNetWebAssets>true</RequiresAspNetWebAssets>
```

#### Boot config inlined

`blazor.boot.json` is now inlined into `dotnet.js`. Only affects code that was tampering with the boot file directly (integrity checks, custom resource loading).

#### Preloaded framework assets

Blazor Web Apps automatically preload framework assets using `Link` headers. Standalone WASM apps opt in via `<OverrideHtmlAssetPlaceholders>` and a `<link rel="preload" id="webassembly" />` in `index.html`.

A new `ResourcePreloader` component replaces the older link-header approach for WASM-in-Web-App scenarios so the `<base href>` is respected correctly:

```html
<head>
    <base href="/" />
    <ResourcePreloader />
</head>
```

#### Client-side fingerprinting for standalone WASM

Server-side fingerprinting shipped in .NET 9. Standalone WASM gets it in .NET 10.

In `index.html`:

```html
<head>
    <script type="importmap"></script>
</head>
<body>
    <script src="_framework/blazor.webassembly#[.{fingerprint}].js"></script>
</body>
```

In `.csproj`:

```xml
<PropertyGroup>
    <OverrideHtmlAssetPlaceholders>true</OverrideHtmlAssetPlaceholders>
</PropertyGroup>
```

For developer-supplied JS modules, use the `#[.{fingerprint}]` marker and register the pattern:

```xml
<ItemGroup>
  <StaticWebAssetFingerprintPattern Include="JSModule" Pattern="*.js"
    Expression="#[.{fingerprint}]!" />
</ItemGroup>
```

#### JavaScript bundler-friendly output

`<WasmBundlerFriendlyBootConfig>true</WasmBundlerFriendlyBootConfig>` produces output that plays nicely with Webpack, Rollup, Gulp.

#### Hot Reload for WASM

Now a general-purpose feature. `WasmEnableHotReload` is `true` by default in Debug. Real WASM hot reload — no more browser refresh on every change.

Disable for Debug:

```xml
<PropertyGroup>
  <WasmEnableHotReload>false</WasmEnableHotReload>
</PropertyGroup>
```

#### WebAssembly diagnostics

New performance profiling and diagnostic counters for WASM apps via browser developer tools and Event Pipe.

#### WASM environment configuration

The `Blazor-Environment` header and `Properties/launchSettings.json` `ASPNETCORE_ENVIRONMENT` no longer control the environment in standalone WASM apps. Set via project file:

```xml
<WasmApplicationEnvironmentName>Staging</WasmApplicationEnvironmentName>
```

Defaults: `Development` for build, `Production` for publish.

#### WASM respects current UI culture

In .NET 9 or earlier, standalone WASM apps loaded UI globalization resources based on `CultureInfo.DefaultThreadCurrentCulture`. .NET 10 also loads globalization for `CultureInfo.DefaultThreadCurrentUICulture`.

#### Custom Blazor cache removed

`BlazorCacheBootResources` MSBuild property is gone — fingerprinting handles caching now. Remove the property if present.

### JS Interop — expanded surface area

You can now create JS objects via constructor functions and read/write JS object properties directly.

**New async methods on `IJSRuntime` / `IJSObjectReference`:**

- `InvokeConstructorAsync(identifier, args)` — invokes with `new`, returns an `IJSObjectReference`
- `GetValueAsync<T>(identifier)` — read JS property (data or accessor)
- `SetValueAsync<T>(identifier, value)` — write JS property (creates the property if it doesn't exist)

**Sync counterparts on `IJSInProcessRuntime` / `IJSInProcessObjectReference`:** `InvokeConstructor`, `GetValue<T>`, `SetValue<T>`.

**Example:**

```csharp
var classRef = await JSRuntime.InvokeConstructorAsync("jsInterop.TestClass", "Blazor!");
var text = await classRef.GetValueAsync<string>("text");
var textLength = await classRef.InvokeAsync<int>("getTextLength");

await JSRuntime.SetValueAsync("jsInterop.testObject.num", 30);
```

Throws `JSException` if a property doesn't exist on get, or isn't writable on set. Overloads accept `CancellationToken` or `TimeSpan` timeout.

### Reconnection UI (Blazor Server, refined)

The Blazor Web App template now ships a `ReconnectModal` component with collocated CSS and JS files. The previous default UI inserted styles programmatically, which violated strict CSP `style-src` policies — that's fixed (the default is still used as fallback).

**New features:**

- `components-reconnect-state-changed` DOM event for state changes
- New `retrying` reconnection state (CSS class + event)

### Security

#### Passkey / WebAuthn support in ASP.NET Core Identity

FIDO2-based passwordless auth, with passkey management and login built into the Blazor Web App template. Production feature, not preview-only.

#### Cookie auth no longer redirects API endpoints to login

Unauthenticated requests to known API endpoints return 401/403 instead of an HTML redirect. Known API endpoints identified via the new `IApiEndpointMetadata` interface:

- `[ApiController]` endpoints
- Minimal API endpoints reading/writing JSON
- Endpoints using `TypedResults` return types
- SignalR endpoints

Override `OnRedirectToLogin` / `OnRedirectToAccessDenied` to keep the old redirect behavior:

```csharp
builder.Services.AddAuthentication()
    .AddCookie(options =>
    {
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    });
```

### Smaller features worth mentioning

- **`QuickGrid.RowClass`** — conditional row styling via a delegate:

  ```razor
  <QuickGrid ... RowClass="GetRowCssClass">

  @code {
      private string GetRowCssClass(MyGridItem item) =>
          item.IsArchived ? "row-archived" : null;
  }
  ```

- **`QuickGrid.HideColumnOptionsAsync()`** — programmatically close column options (e.g., after a filter is applied)

- **`InputHidden` component** — hidden input form field, finally a first-class component:

  ```razor
  <EditForm Model="Parameter" OnValidSubmit="Submit" FormName="InputHidden Example">
      <InputHidden id="hidden" @bind-Value="Parameter" />
      <button type="submit">Submit</button>
  </EditForm>
  ```

- **`OwningComponentBase` implements `IAsyncDisposable`** — async disposal of the DI scope (`DisposeAsync`, `DisposeAsyncCore`)

- **Route template syntax highlighting** in `[Route]` attributes (IDE feature)

- **Comprehensive metrics and tracing** for component lifecycle, navigation, event handling, and circuits — surfaces in Aspire dashboards

- **PWA template** now registers the service worker with `updateViaCache: 'none'` to fix stale service worker issues:

  ```javascript
  navigator.serviceWorker.register('service-worker.js', { updateViaCache: 'none' });
  ```

  Recommended for all PWAs, including .NET 9 and earlier.

---

## Breaking Changes — Migration Slide Material

1. **HttpClient response streaming is on by default in WASM**

   `response.Content.ReadAsStreamAsync()` now returns a `BrowserHttpReadStream`, not a `MemoryStream`. Synchronous reads (`Stream.Read(Span<byte>)`) don't work.

   Opt out globally:

   ```xml
   <WasmEnableStreamingResponse>false</WasmEnableStreamingResponse>
   ```

   Or set `DOTNET_WASM_ENABLE_STREAMING_RESPONSE` env var to `false`/`0`.

   Per request:

   ```csharp
   requestMessage.SetBrowserResponseStreamingEnabled(false);
   ```

2. **`<NotFound>` render fragment removed** — use `Router.NotFoundPage` parameter

3. **`NavigateTo` no longer scrolls to top on same-page navigations**

4. **`NavLinkMatch.All` ignores query string and fragment** by default

5. **Validation models must live in `.cs` files**, not `.razor` (if using the new source-generator validation)

6. **WASM environment** — `Blazor-Environment` header and `launchSettings.json` `ASPNETCORE_ENVIRONMENT` no longer apply to standalone WASM. Use `<WasmApplicationEnvironmentName>` in `.csproj`.

7. **Custom Blazor cache removed** — remove `BlazorCacheBootResources` from project files

8. **Static SSR `NavigateTo` no longer throws `NavigationException`** in new template configuration (`BlazorDisableThrowNavigationException=true`)

---

## Suggested Talk Structure (5-slide addendum)

1. **What .NET 9 brought** — render-mode introspection (`RendererInfo`), `MapStaticAssets`, reconnection UX, simplified auth state
2. **.NET 10 state management** — `[PersistentState]` before/after, plus circuit state persistence
3. **.NET 10 forms & validation** — `AddValidation()` + `[ValidatableType]`, the source-generator angle
4. **.NET 10 perf & WASM** — 76% `blazor.web.js` reduction, fingerprinting, bundler-friendly output, WASM Hot Reload
5. **Migration gotchas** — HttpClient streaming, `<NotFound>` removal, `NavigateTo` scroll behavior, passkeys as the new auth story

---

## Source References

- [What's new in ASP.NET Core in .NET 9](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-9.0?view=aspnetcore-9.0)
- [What's new in ASP.NET Core in .NET 10](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-10.0?view=aspnetcore-10.0)
- [Migrate from ASP.NET Core in .NET 9 to .NET 10](https://learn.microsoft.com/en-us/aspnet/core/migration/90-to-100?view=aspnetcore-10.0)
