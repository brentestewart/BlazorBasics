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

# Welcome

## CommunityDays KC 2026

<!--
Generic opener. Greet the room while people settle in. Then click forward — the first content slide is the Chevy Blazer gag, so the title hasn't been revealed yet.
-->

---

# Blazer Fundamentals: Care and maintenance of your Chevy Blazer

<div class="flex justify-center mt-6">
  <img src="/chevy-blazer.png" class="rounded shadow-lg" style="max-height: 42vh;" />
</div>

<!--
Opening gag — let the room sit with the picture for a beat. They'll wonder if they walked into the wrong room.

Then click forward to the title reveal: "actually we're talking about Blazor, not Blazer." The longer you hold the confusion, the better the pivot lands.
-->

---
layout: cover
---

# Blazor Fundamentals

## Getting Started with Modern .NET Web Development

Brent Stewart — Alien Arc Technologies

<!--
The pivot. After the Chevy gag, this is the "actually we're talking about Blazor" reveal. Brief beat, then move on to introduce yourself.
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

# What is Blazor?

<div class="flex flex-col items-center justify-center text-center" style="min-height: 40vh;">

<div v-click="1" class="text-5xl font-semibold mb-4">
  Browser <span class="opacity-50 mx-4">+</span> Razor
</div>

<div v-click="2" class="text-4xl opacity-70 my-2">
  =
</div>

<div v-click="2" class="text-7xl font-bold mb-8">
  Blazor
</div>

<div v-click="3" class="text-xl max-w-3xl opacity-90 mt-4">
  A component-based web framework from Microsoft. Write your UI in C# instead of JavaScript, and pick where each component runs — server, browser via WebAssembly, or static HTML.
</div>

</div>

<!--
Etymology reveal — Browser + Razor = Blazor. Walk the clicks slowly so the room makes the connection.

Razor is Microsoft's HTML + C# templating engine, around since 2010 in ASP.NET MVC. "Blazor" is just Razor running in the browser (and now everywhere else too).

After the reveal, land the substance line: same C# component, multiple runtime locations. The next slide breaks down those locations.

Calibrate to the room — skim fast if many hands go up on "I already use Blazor."
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

# Prerendering

Interactive modes don't render once &mdash; they render **twice**.

| Phase | Where | What it produces |
|---|---|---|
| **1. Prerender** | Server, during the HTTP response | HTML for fast first paint |
| **2. Interactive** | Browser (WASM) or SignalR circuit (Server) | The live, responsive UI |

<br>

The component goes through `OnInitializedAsync` **once on the server, then again on the client.**
That boundary is where most "huh, that's weird" Blazor moments live &mdash; state lost,
data refetched, content flickering as the takeover happens.

<!--
The /wasm demo you just saw is this in action — server-rendered HTML appears
immediately, then WASM bootstraps and the component re-initializes. /wasm-no-prerender
turns the first phase off so you see the blank-then-pop alternative.

Don't dwell on the problems here. Just name the boundary. Later in the deck,
"Prerender superpowers" introduces [StreamRendering] and [PersistentState] —
the two attributes that fix the most painful symptoms.
-->

---

# Project structure

`dotnet new blazor` produces a two-project solution.

```
MyApp.sln
├── MyApp.Web/              ← server (ASP.NET Core host)
│   ├── Components/
│   │   ├── App.razor       ← <html> shell
│   │   ├── Routes.razor    ← <Router>
│   │   └── Pages/          ← server-only pages
│   └── Program.cs
│
└── MyApp.Web.Client/       ← browser (WebAssembly)
    ├── Pages/              ← can run server AND/OR client
    ├── Components/
    └── Program.cs
```

**Rule of thumb:** components in `.Web.Client` can run *anywhere* &mdash; server prerender,
WebAssembly, or both. Components in `.Web` are server-only.

<!--
Open the solution in the IDE and walk the two projects briefly. The .Web project
is the ASP.NET Core host — it boots Kestrel, configures Razor components, and ships
the WASM bundle. The .Web.Client project is the WebAssembly side — anything in here
gets compiled to WASM and downloaded by the browser.

Why does location matter? Because a component's @rendermode determines where it
runs. A page with @rendermode InteractiveWebAssembly must live in .Web.Client
because the server can't ship code it doesn't have a reference to the browser side of.

The .Web project references .Web.Client, so server pages can use client components.
The reverse isn't true — client components can't pull in server-only code.
-->

---
layout: section
---

# Components

<div class="absolute left-0 right-0 bottom-0 top-24 flex flex-col items-center justify-center gap-8">
  <img src="/lego-blocks.png" style="max-height: 36vh;" />
  <div class="text-5xl font-bold text-center">
    The Lego bricks of Blazor
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

<div class="text-sm opacity-70 font-mono mb-1">Widget.razor.css</div>

```css
.row { padding: 0.5rem; }       /* this component only */
.row ::deep a { color: cyan; }  /* descendants too */
```

<!--
Open Widget.razor + Widget.razor.css side-by-side in the IDE here.
-->

---

# Parameters

<div class="text-sm opacity-70 font-mono mb-1">Widget.razor</div>

```csharp
[Parameter] public string Title { get; set; } = "";
[Parameter] public WidgetData? Data { get; set; }
[Parameter] public EventCallback<WidgetData> OnRefresh { get; set; }
```

<br>

**Catch-all — forward any unmatched attribute:**

<div class="text-sm opacity-70 font-mono mb-1">Widget.razor</div>

```csharp
[Parameter(CaptureUnmatchedValues = true)]
public Dictionary<string, object>? Attributes { get; set; }
```

<div class="text-sm opacity-70 font-mono mb-1">Widget.razor</div>

```razor
<button @attributes="Attributes">Click</button>
```

<!--
Catch-all is how component-library authors splat `class`, `id`, ARIA attrs onto the inner element without declaring every parameter.
-->

---

# ChildContent / RenderFragment

**Definition**

<div class="text-sm opacity-70 font-mono mb-1">WidgetCard.razor</div>

```csharp
[Parameter] public string Title { get; set; } = "";
[Parameter] public RenderFragment? ChildContent { get; set; }
```

<div class="text-sm opacity-70 font-mono mb-1">WidgetCard.razor</div>

```razor
<div class="card">
  <h3>@Title</h3>
  @ChildContent
</div>
```

**Usage**

<div class="text-sm opacity-70 font-mono mb-1">Dashboard.razor</div>

```razor
<WidgetCard Title="Sales">
  <p>$42,580 this week</p>
</WidgetCard>
```

<!--
Composition primitive. Anything inside the tags becomes `ChildContent`.
-->

---
layout: section
---

# Interactivity

<div class="absolute left-0 right-0 bottom-0 top-24 flex items-center justify-center">
  <div class="flex flex-col gap-5">
    <div class="flex items-baseline gap-8">
      <span class="text-6xl font-bold opacity-25 tabular-nums">01</span>
      <span class="text-5xl font-semibold">Events</span>
    </div>
    <div class="flex items-baseline gap-8">
      <span class="text-6xl font-bold opacity-25 tabular-nums">02</span>
      <span class="text-5xl font-semibold">Data binding</span>
    </div>
    <div class="flex items-baseline gap-8">
      <span class="text-6xl font-bold opacity-25 tabular-nums">03</span>
      <span class="text-5xl font-semibold">Lifecycle</span>
    </div>
  </div>
</div>

---

# Events

<div class="text-sm opacity-70 font-mono mb-1">Widget.razor</div>

```razor
<button @onclick="Refresh">Refresh</button>

<button @onclick="@(() => Refresh(widget))">Refresh</button>

<button @onclick="HandleClickAsync">Async too</button>
```

**EventCallback — pass a handler up to a child:**

<div class="text-sm opacity-70 font-mono mb-1">Widget.razor</div>

```csharp
[Parameter] public EventCallback<WidgetData> OnRefresh { get; set; }
```

<div class="text-sm opacity-70 font-mono mb-1">Widget.razor</div>

```razor
<button @onclick="() => OnRefresh.InvokeAsync(Data)">Refresh</button>
```

<!--
EventCallback is preferred over Action for cross-component events — it integrates with Blazor's rendering cycle.
-->

---

# Data binding

<div class="text-sm opacity-70 font-mono mb-1">PromptInput.razor</div>

```razor
<input @bind="prompt" />                                  <!-- onchange -->
<input @bind="prompt" @bind:event="oninput" />            <!-- per keystroke -->
<input @bind="prompt" @bind:event="oninput"
       @bind:after="OnPromptChanged" />                   <!-- side-effect -->
```

`@bind` is syntactic sugar for `value="@prompt"` + `@onchange="..."`.

<!--
Show the binding running live on /wasm. Type into the prompt input; watch state update.
-->

---

# Component lifecycle

| Hook | Fires when |
|---|---|
| `SetParametersAsync(ParameterView parameters)` | Parameters arriving from parent |
| `OnInitialized()`<br>`OnInitializedAsync()` | Once, when component is created |
| `OnParametersSet()`<br>`OnParametersSetAsync()` | After parameters are set (initial + subsequent) |
| `OnAfterRender(bool firstRender)`<br>`OnAfterRenderAsync(bool firstRender)` | After the DOM is updated |

<br>

`StateHasChanged()` &mdash; manually trigger a re-render (rarely needed).

<!--
Brief in-component demo: log each hook firing for first load, parameter change, and local state change.

Mention: most components only need OnInitializedAsync (for fetching) and rely on Blazor's automatic re-renders.
-->

---
layout: section
---

# Composing an app

<div class="absolute left-0 right-0 bottom-0 top-24 flex items-center justify-center">
  <div class="flex flex-col gap-5">
    <div class="flex items-baseline gap-8">
      <span class="text-6xl font-bold opacity-25 tabular-nums">01</span>
      <span class="text-5xl font-semibold">Control flow</span>
    </div>
    <div class="flex items-baseline gap-8">
      <span class="text-6xl font-bold opacity-25 tabular-nums">02</span>
      <span class="text-5xl font-semibold">Routing</span>
    </div>
    <div class="flex items-baseline gap-8">
      <span class="text-6xl font-bold opacity-25 tabular-nums">03</span>
      <span class="text-5xl font-semibold">Dependency injection</span>
    </div>
    <div class="flex items-baseline gap-8">
      <span class="text-6xl font-bold opacity-25 tabular-nums">04</span>
      <span class="text-5xl font-semibold">Cascading values</span>
    </div>
  </div>
</div>

---

# Control flow

<div class="text-sm opacity-70 font-mono mb-1">WidgetList.razor</div>

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

<div class="text-sm opacity-70 font-mono mb-1">User.razor</div>

```razor
@page "/user/{id:int?}"
@page "/catch/{*path}"
```

- One or more `@page` directives per component
- Route parameters: `{name}`, optional `{name?}`, catch-all `{*name}`
- Constraints attach with `:type` — type-checked at route time

| `bool` | `datetime` | `decimal` | `double` | `float` | `guid` | `int` | `long` | `nonfile` |
|---|---|---|---|---|---|---|---|---|

Blazor's router only supports these nine type constraints — MVC's `alpha`, `regex`, `min`, `max`, `range`, `length` are **not** available.

<!--
Open ComparisonDetail.razor; navigate to /comparison/3.

The constraint list is shorter than MVC's on purpose — Blazor's router is stricter. `:nonfile` is the surprise one: stick it on optional/catch-all parameters to stop them from gobbling up `app.styles.css` or `favicon.ico`.
-->

---

# Query string parameters

<div class="text-sm opacity-70 font-mono mb-1">Dashboard.razor</div>

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

<div class="text-sm opacity-70 font-mono mb-1">NavMenu.razor</div>

```razor
<NavLink href="/wasm" Match="NavLinkMatch.All">WASM</NavLink>
```

<div class="text-sm opacity-70 font-mono mb-1">Dashboard.razor</div>

```csharp
@inject NavigationManager Nav

void GoHome() => Nav.NavigateTo("/");
void Refresh() => Nav.Refresh();
```

**404s — wire a `NotFoundPage` on the Router:**

<div class="text-sm opacity-70 font-mono mb-1">Routes.razor</div>

```razor
<Router AppAssembly="..." NotFoundPage="typeof(NotFound)" />
```

---

# Dependency injection

<div class="text-sm opacity-70 font-mono mb-1">Dashboard.razor</div>

```razor
@inject IDashboardService Dashboard
@inject HttpClient Http
```

<div class="text-sm opacity-70 font-mono mb-1">Dashboard.razor.cs</div>

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

# Cascading values

Pass a value once at the top; any descendant pulls it out — no matter how deep, no parameter passing in between.

<div class="text-sm opacity-70 font-mono mb-1">MainLayout.razor</div>

```razor
@* Provider — anywhere up the tree *@
<CascadingValue Value="theme">
    <ThemedCard>
        <ThemedHeader>...</ThemedHeader>
        <ThemedButton>...</ThemedButton>
    </ThemedCard>
</CascadingValue>
```

<div class="text-sm opacity-70 font-mono mb-1">ThemedButton.razor</div>

```csharp
// Consumer — any depth below, no Theme parameter from the parent
[CascadingParameter] public Theme Theme { get; set; }
```

<br>

Multiple cascades of the same type? Distinguish them with `[CascadingParameter(Name = "...")]`.

<!--
Open /cascading. Toggle light ↔ dark. Every nested component re-renders — none of the intermediate components declares a Theme parameter; they pick it out of the ambient cascade.

Real-world uses: theming, current-user context, feature flags, any "ambient" thing a whole subtree cares about.

If the cascaded value never changes, mark `IsFixed="true"` on `<CascadingValue>` for a perf win — Blazor skips the re-render notification dance.
-->

---
layout: section
---

# Prerender superpowers

<div class="absolute left-0 right-0 bottom-0 top-24 flex items-center justify-center">
  <div class="text-6xl font-bold text-center">
    Two attributes that fix the prerender pipeline's biggest problems
  </div>
</div>

---

# `[StreamRendering]`

Static SSR doesn't have to mean slow blocking.

<div class="text-sm opacity-70 font-mono mb-1">Dashboard.razor</div>

```csharp
@page "/dashboard"
@attribute [StreamRendering]

<h1>Dashboard</h1>
<DataPanel />
```

The initial HTML streams immediately; async work patches in once ready.

<!--
Fixes the "blank tab for 3 seconds" problem. Server flushes the initial HTML right away, then patches in each section as its async work completes.

Demo: /stream vs /stream-blocking — same component, mode banner makes the difference obvious.
-->

---

# `[PersistentState]`

**Before — fetched value flickers across prerender → interactive boundary:**

<div class="text-sm opacity-70 font-mono mb-1">Dashboard.razor</div>

```csharp
[Inject] public IDashboardService Dashboard { get; set; } = default!;
public List<WidgetData>? Items { get; set; }

protected override async Task OnInitializedAsync()
{
    Items = await Dashboard.GetWidgetsAsync();
}
```

**After — value persists, no refetch on hydration:**

<div class="text-sm opacity-70 font-mono mb-1">Dashboard.razor</div>

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
layout: section
---

# Forms

<div class="absolute left-0 right-0 bottom-0 top-24 flex items-center justify-center">
  <div class="flex flex-col gap-5">
    <div class="flex items-baseline gap-8">
      <span class="text-6xl font-bold opacity-25 tabular-nums">01</span>
      <span class="text-5xl font-semibold">EditForm</span>
    </div>
    <div class="flex items-baseline gap-8">
      <span class="text-6xl font-bold opacity-25 tabular-nums">02</span>
      <span class="text-5xl font-semibold">Validation</span>
    </div>
    <div class="flex items-baseline gap-8">
      <span class="text-6xl font-bold opacity-25 tabular-nums">03</span>
      <span class="text-5xl font-semibold">Source-gen (.NET 10)</span>
    </div>
  </div>
</div>

---

# EditForm anatomy

<div class="text-sm opacity-70 font-mono mb-1">Submit.razor</div>

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

<div class="text-sm opacity-70 font-mono mb-1">Program.cs</div>

```csharp
builder.Services.AddValidation();
```

<div class="text-sm opacity-70 font-mono mb-1">Comparison.cs &nbsp;<span class="opacity-70">← .cs, not .razor</span></div>

```csharp
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

# Reaching outside Blazor

<div class="absolute left-0 right-0 bottom-0 top-24 flex items-center justify-center">
  <div class="text-6xl font-bold text-center">
    When you need the browser
  </div>
</div>

---

# JavaScript interop

<div class="text-sm opacity-70 font-mono mb-1">JsInterop.razor</div>

```csharp
@inject IJSRuntime JS

async Task LogIt()
{
    await JS.InvokeVoidAsync("console.log", "Hello from C#");
}

var width = await JS.InvokeAsync<int>("getViewportWidth");
```

Use it when you need the browser; avoid it when you don't.

<!--
Demo: /js-interop shows both directions — C# → JS (clipboard) and JS → C# (window resize listener calling [JSInvokable] method).
-->

---
layout: section
---

# Shipping

<div class="absolute left-0 right-0 bottom-0 top-24 flex items-center justify-center">
  <div class="flex flex-col gap-5">
    <div class="flex items-baseline gap-8">
      <span class="text-6xl font-bold opacity-25 tabular-nums">01</span>
      <span class="text-5xl font-semibold">Dev experience</span>
    </div>
    <div class="flex items-baseline gap-8">
      <span class="text-6xl font-bold opacity-25 tabular-nums">02</span>
      <span class="text-5xl font-semibold">Project types</span>
    </div>
    <div class="flex items-baseline gap-8">
      <span class="text-6xl font-bold opacity-25 tabular-nums">03</span>
      <span class="text-5xl font-semibold">When to pick which</span>
    </div>
  </div>
</div>

---

# WASM Hot Reload + a lighter runtime

- **WASM Hot Reload (.NET 10)** — edit a `.razor` file with `/wasm` open, browser auto updates. This didn't work in .NET 9.
- **`blazor.web.js` is 76% smaller** — the script that bootstraps every Blazor Web App page is now a fraction of its prior size.

<br>

The perception that "WASM is heavy" is getting less true every release.

---

# Three ways to ship Blazor

| Project type | What it gives you |
|---|---|
| **Blazor Web App** | What we built today — Server + WebAssembly render modes, per-component choice |
| **Standalone WebAssembly** | Pure WASM, static hosting, PWA-able. No server backend required. |
| **Blazor Hybrid** | Render Blazor components inside .NET MAUI, WPF, or WinForms — desktop and mobile apps in C# |

<br>

Render modes are a choice *within* Blazor Web App. The other two project types live on their own tracks.

<!--
Five-second tour, not a deep dive. The takeaway: Blazor isn't just web — Hybrid puts your component code inside a desktop or mobile app shell.

Standalone WASM is closer to the React/Vue/Angular deployment story — build static assets, drop on a CDN, no backend needed.

Most .NET devs find out Hybrid exists years too late. Plant the seed.
-->

---

# When to pick which

| You want… | Reach for |
|---|---|
| Server-rendered, low client work | **InteractiveServer** |
| Rich client UI, no SignalR | **InteractiveWebAssembly** |
| Server first, WASM later | **InteractiveAuto** |
| Form-only, no live interactivity | **Static SSR** |
| Installable PWA, static hosting | **Standalone WASM** (separate template) |
| Native desktop / mobile shell | **Blazor Hybrid** (.NET MAUI / WPF / WinForms) |

Most apps pick one and live there. Pick deliberately.

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
