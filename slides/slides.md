---
theme: frankfurt
infoLine: false
title: "Blazor Fundamentals: Getting Started with Modern .NET Web Development"
info: |
  ## Blazor Fundamentals
  Getting Started with Modern .NET Web Development
  Brent Stewart — Alien Arc Technologies
class: text-center
highlighter: shiki
lineNumbers: false
drawings:
  persist: false
transition: slide-left
mdc: true
---

# Blazor Fundamentals

## Getting Started with Modern .NET Web Development

Brent Stewart — Alien Arc Technologies

<!--
Title slide. Stay here while the room settles.
-->

---
layout: center
---

<div class="item" style="width: 36rem;">
  <header style="padding: 1.5rem; text-align: center;">
    <div class="text-5xl font-bold">Brent Stewart</div>
    <div class="text-lg opacity-80 mt-2">Entrepreneur · Maker · Community Organizer</div>
  </header>
  <main style="padding: 1.5rem 2rem;">
    <div class="space-y-3 text-lg">
      <div><strong>Day Job:</strong> Co-founder, Alien Arc Technologies</div>
      <div><strong>Community:</strong> Co-Organizer, KC .NET User Group</div>
      <div><strong>Code available at:</strong> <code>github.com/brentestewart</code></div>
      <div><strong>Hometown:</strong> Blue Springs, MO</div>
    </div>
  </main>
</div>

<!--
Hi, I'm Brent Stewart from Kansas City. I'm an entrepreneur, community organizer, maker, photographer, and home automation enthusiast.

I love using technology to solve problems and find practical ways to make our lives better.

I co-founded Alien Arc Technologies where I get to use my experience to deliver technology solutions to a wide range of clients and bring some of my own ideas to life.
-->

---

# The four render modes

| Mode | What it is |
|---|---|
| **Static SSR** | Server returns HTML; no interactivity unless you opt in |
| **InteractiveServer** | Live UI via a SignalR circuit to the server |
| **InteractiveWebAssembly** | The component runs in the browser via WASM |
| **InteractiveAuto** | Server first, WebAssembly once the bundle's cached |

<br>

Set globally, per page, per component, or per instance. Children inherit from their parent's mode.

<!--
This is the one-slide synthesis of the cold-open demo. The audience just *felt* the differences. Now they have names for them.

Key point to land: render mode is configurable at every level — set globally on App.razor (Routes + HeadOutlet) and the whole app runs that mode, or scope down to a specific component. Children inherit from the parent's mode within a subtree.

Most real apps pick one mode and live there. The demo shows all four because the audience should feel each before committing — not because mixing is the goal.
-->

---

# `[StreamRendering]`

Static SSR doesn't have to mean slow blocking.

```csharp
@page "/dashboard"
@attribute [StreamRendering]

<h1>Dashboard</h1>
<DataPanel />
```

The initial HTML streams immediately; async work patches in once ready.

<!--
30-second beat. Strengthens the render-mode story — Static SSR is more capable than the "click does nothing" demo suggested.
-->

---
layout: section
---

# Components

<div class="absolute left-0 right-0 bottom-0 top-24 flex items-center justify-center">
  <div class="text-6xl font-bold text-center">
    The Lego blocks of Blazor
  </div>
</div>

---

# Anatomy of a component

A component is one or more files that live together.

```
MyCustomComponent.razor          # markup + (optional) @code
MyCustomComponent.razor.cs       # optional code-behind partial class
MyCustomComponent.razor.css      # optional scoped styles
```

- Scoped CSS is automatic — selectors only match this component
- Use `::deep` to pierce into descendant component markup

```css
.row { padding: 0.5rem; }       /* this component only */
.row ::deep a { color: cyan; }  /* descendants too */
```

<!--
Open Widget.razor + Widget.razor.css side-by-side in the IDE here.
-->

---

# Parameters

```csharp
[Parameter] public string Title { get; set; } = "";
[Parameter] public WidgetData? Data { get; set; }
[Parameter] public EventCallback<WidgetData> OnRefresh { get; set; }
```

<br>

**Catch-all — forward any unmatched attribute:**

```csharp
[Parameter(CaptureUnmatchedValues = true)]
public Dictionary<string, object>? Attributes { get; set; }
```

```razor
<button @attributes="Attributes">Click</button>
```

<!--
Catch-all is how component-library authors splat `class`, `id`, ARIA attrs onto the inner element without declaring every parameter.
-->

---

# ChildContent / RenderFragment

```razor
<WidgetCard Title="Sales">
  <p>$42,580 this week</p>
</WidgetCard>
```

```csharp
// WidgetCard.razor
[Parameter] public string Title { get; set; } = "";
[Parameter] public RenderFragment? ChildContent { get; set; }
```

```razor
<div class="card">
  <h3>@Title</h3>
  @ChildContent
</div>
```

<!--
Composition primitive. Anything inside the tags becomes `ChildContent`.
-->

---

# Data binding

```razor
<input @bind="prompt" />                                  <!-- onchange -->
<input @bind="prompt" @bind:event="oninput" />            <!-- per keystroke -->
<input @bind="prompt" @bind:event="oninput"
       @bind:after="OnPromptChanged" />                   <!-- side-effect -->
```

`@bind` is sugar for `value="@prompt"` + `@onchange="..."`.

<!--
Show the binding running live on /wasm. Type into the prompt input; watch state update.
-->

---

# Component lifecycle

| Hook | Fires when |
|---|---|
| `SetParametersAsync` | Parameters arriving from parent |
| `OnInitialized(Async)` | Once, when component is created |
| `OnParametersSet(Async)` | After parameters are set (initial + subsequent) |
| `OnAfterRender(Async)` | After the DOM is updated; `firstRender: bool` |
| `StateHasChanged()` | Manually trigger a re-render (rarely needed) |

<!--
Brief in-component demo: log each hook firing for first load, parameter change, and local state change.

Mention: most components only need OnInitializedAsync (for fetching) and rely on Blazor's automatic re-renders.
-->

---
layout: section
---

# Behavior

<div class="absolute left-0 right-0 bottom-0 top-24 flex items-center justify-center">
  <div class="text-6xl font-bold text-center">
    Events, routing, dependency injection
  </div>
</div>

---

# Events

```razor
<button @onclick="Refresh">Refresh</button>

<button @onclick="@(() => Refresh(widget))">Refresh</button>

<button @onclick="HandleClickAsync">Async too</button>
```

**EventCallback — pass a handler up to a child:**

```csharp
// Widget.razor
[Parameter] public EventCallback<WidgetData> OnRefresh { get; set; }
```

```razor
<button @onclick="() => OnRefresh.InvokeAsync(Data)">Refresh</button>
```

<!--
EventCallback is preferred over Action for cross-component events — it integrates with Blazor's rendering cycle.
-->

---

# Control flow

```razor
@if (Widgets is null)
{
    <p>Loading…</p>
}
else
{
    <ul>
        @foreach (var widget in Widgets)
        {
            <Widget Data="widget" OnRefresh="HandleRefresh" />
        }
    </ul>
}
```

Razor is just C# inside HTML. `@if`, `@foreach`, `@switch`, all work.

---

# Routing

```razor
@page "/comparison/{id:int}"

@code {
    [Parameter] public int Id { get; set; }
}
```

- One or more `@page` directives per component
- Route parameters use `{name}` syntax
- Constraints use `{name:type}` syntax — type-checked at route time
- Optional with `?` — `{id:int?}`
- Catch-all with `*` — `{*path}`

<!--
Open ComparisonDetail.razor; navigate to /comparison/3.
-->

---

# Route constraints

| Constraint | Example |
|---|---|
| `int`, `long`, `float`, `double`, `decimal`, `bool` | `{id:int}` |
| `datetime`, `guid` | `{when:datetime}` |
| `alpha` | `{slug:alpha}` |
| `regex(pattern)` | `{code:regex(^\\d{{5}}$)}` |
| `min(n)`, `max(n)`, `range(a,b)` | `{n:min(1)}` |
| `minlength(n)`, `maxlength(n)`, `length(n)` | `{slug:minlength(3)}` |

Reference slide — photograph it and keep moving.

---

# Query string parameters

```csharp
@page "/dashboard"

@code {
    [SupplyParameterFromQuery]
    public string? Filter { get; set; }

    [SupplyParameterFromQuery(Name = "sort")]
    public string? SortBy { get; set; }
}
```

`/dashboard?filter=sales&sort=value` populates both properties.

---

# Navigation

```razor
<NavLink href="/wasm" Match="NavLinkMatch.All">WASM</NavLink>
```

```csharp
@inject NavigationManager Nav

void GoHome() => Nav.NavigateTo("/");
void Refresh() => Nav.Refresh();
```

**404s — wire a `NotFoundPage` on the Router:**

```razor
<Router AppAssembly="..." NotFoundPage="typeof(NotFound)" />
```

---

# Dependency injection

```razor
@inject IDashboardService Dashboard
@inject HttpClient Http
```

```csharp
[Inject] public IDashboardService Dashboard { get; set; } = default!;
```

| Lifetime | When to use |
|---|---|
| **Singleton** | App-wide shared state (in-memory cache) |
| **Scoped** | Per-circuit on Server, per-app on WASM |
| **Transient** | Stateless helpers |

<!--
Worth a sentence: "scoped" means something different on Server (per SignalR circuit) vs WASM (per app/tab).
-->

---

# JavaScript interop

```csharp
@inject IJSRuntime JS

async Task LogIt()
{
    await JS.InvokeVoidAsync("console.log", "Hello from C#");
}

var width = await JS.InvokeAsync<int>("getViewportWidth");
```

Use it when you need the browser; avoid it when you don't.

---
layout: section
---

# Forms

<div class="absolute left-0 right-0 bottom-0 top-24 flex items-center justify-center">
  <div class="text-6xl font-bold text-center">
    EditForm, validation, source-gen
  </div>
</div>

---

# EditForm anatomy

```razor
<EditForm Model="Model" OnValidSubmit="Submit">
    <DataAnnotationsValidator />

    <InputText @bind-Value="Model.Prompt" />
    <ValidationMessage For="@(() => Model.Prompt)" />

    <InputSelect @bind-Value="Model.Category">
        <option value="">Pick one…</option>
        <option value="coding">Coding</option>
    </InputSelect>

    <button type="submit">Submit</button>

    <ValidationSummary />
</EditForm>
```

`OnValidSubmit` / `OnInvalidSubmit` / `OnSubmit` — pick one.

---

# Source-gen validation (.NET 10)

```csharp
// Program.cs
builder.Services.AddValidation();
```

```csharp
// Comparison.cs   ← .cs file, NOT .razor
[ValidatableType]
public class Comparison
{
    [Required, MinLength(5)]
    public string Prompt { get; set; } = "";

    public Submitter Submitter { get; set; } = new();   // nested — traversed
    public List<ComparisonResponse> Responses { get; set; } = new();  // also traversed
}
```

**Gotcha:** `[ValidatableType]` must live in a `.cs` file — the source generator doesn't see `.razor`.

<!--
This is the new .NET 10 source-generated validator. It traverses nested objects and collections — something reflection-based validation in .NET 9 wouldn't do.

Trigger a nested error on stage to show traversal working.
-->

---
layout: section
---

# Beyond the basics

<div class="absolute left-0 right-0 bottom-0 top-24 flex items-center justify-center">
  <div class="text-6xl font-bold text-center">
    Persistent state, Auto mode, hot reload
  </div>
</div>

---

# `[PersistentState]`

**Before — fetched value flickers across prerender → interactive boundary:**

```csharp
[Inject] public IDashboardService Dashboard { get; set; } = default!;
public List<WidgetData>? Items { get; set; }

protected override async Task OnInitializedAsync()
{
    Items = await Dashboard.GetWidgetsAsync();
}
```

**After — value persists, no refetch on hydration:**

```csharp
[PersistentState]
public List<WidgetData>? Items { get; set; }

protected override async Task OnInitializedAsync()
{
    Items ??= await Dashboard.GetWidgetsAsync();
}
```

<!--
This is .NET 10. Replaces the PersistentComponentState dance from .NET 8/9.

Live moment: toggle the attribute on/off; show the network tab refetching when off.
-->

---

# WASM Hot Reload + a lighter runtime

- **WASM Hot Reload (.NET 10)** — edit a `.razor` file with `/wasm` open, browser updates without a refresh. This didn't work in .NET 9.
- **`blazor.web.js` is 76% smaller** — the script that bootstraps every Blazor Web App page is now a fraction of its prior size.

<br>

The perception that "WASM is heavy" is getting less true every release.

---

# When to pick which

| You want… | Reach for |
|---|---|
| Lowest-friction interactive page, no client work | **InteractiveServer** |
| Rich offline-tolerant client UI, no SignalR | **InteractiveWebAssembly** |
| Best-of-both, server-rendered first paint | **InteractiveAuto** |
| Form-driven page, no per-keystroke interactivity | **Static SSR** |
| True offline, installable PWA, static-only hosting | **Standalone WASM** template (different project type) |

Most apps pick one and live there. Pick deliberately.

---

# Miscellaneous Blazor

- `<PageTitle>` — set the browser tab title from inside a component
- `<HeadContent>` — inject `<meta>`, `<link>`, etc. into `<head>`
- **QuickGrid** — Microsoft-supplied data grid component
- **Component libraries** — MudBlazor, Radzen, FluentUI Blazor, Telerik, Syncfusion
- **JS interop deeper APIs** — `IJSObjectReference`, isolated modules

```razor
<PageTitle>Dashboard — Blazor Basics</PageTitle>
<HeadContent>
    <meta name="description" content="Blazor dashboard demo" />
</HeadContent>
```

---
layout: center
class: text-center
---

# Thank you

<div class="mt-8 text-center space-y-2">
  <div><strong>Microsoft docs:</strong> <code>learn.microsoft.com/aspnet/core/blazor</code></div>
  <div><strong>Demo source:</strong> <code>github.com/brentestewart/BlazorBasics</code></div>
  <div><strong>Community:</strong> Kansas City .NET User Group</div>
</div>

<div class="text-3xl mt-12 text-center">
Questions?
</div>

<div class="mt-12 text-center opacity-70">
Brent Stewart — Alien Arc Technologies
</div>
