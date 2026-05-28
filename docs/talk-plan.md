# Blazor Basics — Talk Plan

## Talk metadata

- **Title:** Blazor Fundamentals: Getting Started with Modern .NET Web Development
- **Length:** ~55 min (target), expandable to 60 with Q&A buffer
- **Audience:** mixed general developers — assume some web familiarity, no Blazor or .NET specifics
- **Framework target:** .NET 10 (LTS, GA Nov 2025)
- **Companion research:** `docs/blazor-net9-net10-changes.md`
- **Companion demo spec:** `docs/demo-spec.md`

## Thesis

Blazor is a component-based web framework where the same C# component can render on the server, in the browser via WebAssembly, or as static HTML. Render mode is configurable at every level — globally, per page, per component, or per instance — with children inheriting from their parent's mode. In practice, most apps pick one mode and live there; the demo shows all four because the audience should feel each before committing. By the end of the talk, the audience should be able to (1) read a `.razor` file, (2) explain what a render mode is and feel the tradeoff between Server and WASM, and (3) pick a render mode for their own app deliberately.

## Structural approach: fade-spine

Render modes are the talk's hook and synthesis, not a recurring drumbeat. The demo opens by driving the Server vs WASM comparison hard so the audience *feels* the difference. The middle of the talk teaches Blazor concepts on one mode (WASM, for instant feedback during teaching). Render modes resurface in the final third for the moments that genuinely depend on them — forms under Static SSR, `[PersistentState]` across the prerender boundary, and Auto mode as the synthesis.

The render-mode label and mode switcher remain on screen the whole talk, so the audience never loses the frame — but they don't get poked with it on every demo.

## Demo project shape

**One small component** (call it the "Component Lab") mounted on four pages, each with a different render mode:

| Page route | Render mode | Purpose |
|---|---|---|
| `/static` | Static SSR | Form-post baseline; the "click does nothing" reveal |
| `/server` | InteractiveServer | The throttled-latency mode |
| `/wasm` | InteractiveWebAssembly | The instant-feedback mode used to teach concepts |
| `/auto` | InteractiveAuto | The synthesis: first slow, then fast |

The component itself needs to contain (at minimum):

- A counter button (feels latency under Server)
- A text input with two-way binding (teaches `@bind`)
- A small form with validation (teaches forms + SSR vs Interactive behavior)
- A fetched value displayed on load (anchor for `[PersistentState]` demo)
- A visible header showing `RendererInfo.Name` + `AssignedRenderMode`

A persistent top-nav switcher links the four pages. The render-mode label is always visible.

**Demo domain:** **Dashboard with widgets** — a parent `Dashboard` component renders a collection of child `Widget` components. The shape (one parent, many configurable children) is exactly the parent/child component story the talk needs to teach, and the visual metaphor (tiles on a board) is familiar to any developer.

**Feature → demo mapping:**

| Blazor feature | Demo manifestation |
|---|---|
| Counter / rapid-click | Refresh button on each widget |
| `@bind` two-way | Filter / search input above the dashboard |
| `@foreach` list | Dashboard renders its widget collection |
| `[Parameter]` + parent/child | `Dashboard` passes `WidgetData` into each `Widget` |
| `ChildContent` / `RenderFragment` | `WidgetCard` wraps arbitrary content inside a card chrome |
| `EditForm` + nested validation | "Create a widget" form: `WidgetRequest { Title, Category, Owner { Name, Email }, Metrics[] }` |
| Static SSR form post-back | Submit a widget request, page renders with new tile |
| `RendererInfo` label | Render-mode badge at the bottom of every page |
| `[PersistentState]` | Widget collection fetched on load, persists across prerender boundary |
| WASM Hot Reload | Live edit a widget's markup or style during the demo |
| Auto mode | Dashboard first slow, then fast |

## Section-by-section flow

### 1. Hook — feel the difference (10 min)

Render modes in the foreground.

- Open on `/wasm`. Click the counter. Show the render-mode label.
- Switch to `/server`. Turn on Chrome devtools throttling ("Slow 4G"). Click the counter. Visible lag per click.
- Switch to `/static`. Click the counter. *Nothing happens.* Pause. Use that silence to introduce the concept: "Blazor lets you pick where your component runs. That choice has consequences. Let's see what they are."
- One slide: the four render modes, one sentence each. No deep dive yet.
- Brief follow-up beat: Static SSR doesn't have to mean slow blocking — `[StreamRendering]` lets a component stream initial HTML while async work completes. One sentence + the attribute on screen.

**Tier 1 covered:** render modes (introduced)
**Tier 2 covered:** `RendererInfo` / `AssignedRenderMode` (visible the whole talk), `[StreamRendering]`

### 2. The component model (8 min)

Render modes recede. Work on `/wasm` so feedback is instant.

- Open the `.razor` file behind the page. Walk through: markup + `@code` block in one file. Point out the sibling `.razor.css` for scoped styles + `::deep` for piercing into descendants.
- Introduce `[Parameter]` by passing a value into the component. Then show `[Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? Attributes { get; set; }` for forwarding arbitrary HTML attributes onto a wrapped element (attribute splatting).
- Show `ChildContent` / `RenderFragment` briefly via a wrapper.
- Two-way binding with `@bind` on the text input.
- Component lifecycle: `OnInitialized(Async)`, `OnParametersSet(Async)`, `OnAfterRender(Async)`, and when `StateHasChanged()` is needed. One slide + a brief in-component demo that logs each lifecycle hook.

**Tier 1 covered:** component model, parameters, catch-all parameter, child content, data binding, lifecycle, CSS isolation

### 3. Events, control flow, routing, DI (12 min)

Still on `/wasm`. Linear teaching, no mode-flipping.

- `@onclick` with lambdas and `EventCallback<T>`.
- `@if` / `@foreach` rendering a list.
- `@page` directive — show how `/wasm` route is just an attribute on the file. Include a route parameter with a constraint (e.g., `@page "/comparison/{id:int}"`).
- Route constraints reference slide — full list (`int`, `long`, `float`, `double`, `decimal`, `bool`, `datetime`, `guid`, `alpha`, `regex`, `minlength`, `maxlength`, `min`, `max`, `range`, `length`). Audience-photographs-it slide; you keep moving.
- `[SupplyParameterFromQuery]` — bind a property directly to a query string parameter. One-line aside in the routing flow.
- `NavigationManager` and `NavLink`. Mention `NotFoundPage` in passing (no demo).
- `@inject` a typed `HttpClient`. Quick aside on service lifetimes.
- One sentence on JS interop: `IJSRuntime.InvokeAsync` — show a one-line `console.log` from C#. Move on.

**Tier 1 covered:** events, control flow, routing, route constraints, query-string parameters, DI, JS interop
**Tier 2 covered:** `NotFoundPage` (mention)

### 4. Forms and validation (10 min)

Render modes return — this is where the comparison earns its keep.

- Submit the form on `/static`. It works via post-back. SSR isn't dead — it's the baseline.
- Switch to `/wasm`. Same form, live validation, no page reload.
- Open `Program.cs`. Show `builder.Services.AddValidation()`.
- Open the `Order.cs` (or equivalent) model file. Walk through `[ValidatableType]`, nested objects, the `.cs`-not-`.razor` rule (call this out — it's a real gotcha).
- Trigger a nested validation error to show the new source-gen validator traversing collections.

**Tier 1 covered:** forms, validation
**Tier 2 covered:** source-gen validation (`AddValidation`, `[ValidatableType]`)

### 5. Persistent state, Auto mode, WASM hot reload (10 min)

The synthesis. Render modes back in the foreground.

- Show the fetched-on-load value disappearing across the prerender → interactive transition (the .NET 8/9 way). Use this to motivate the problem.
- Add `[PersistentState]` to the property. Reload. Value persists. Side-by-side the before/after code (from the research doc).
- Navigate to `/auto`. First load is slow (WASM downloading). Reload. Instant. Show the render-mode label flipping from Server to WebAssembly mid-session.
- Live moment: edit a `.razor` file with the WASM page open. Browser updates without refresh. ("This didn't work in .NET 9.")
- One slide on the 76% `blazor.web.js` reduction — frames the perception of WASM weight.

**Tier 2 covered:** `[PersistentState]`, Auto mode behavior, WASM Hot Reload, script size reduction

### 6. Close — when to pick which (5 min)

Tie it back to moments they saw, not generic tradeoffs.

- "Remember when Server lagged under throttle? That's why a drawing pad doesn't live there."
- "Remember when the form still submitted under Static SSR? That's where you start — interactivity is opt-in."
- "Auto is the answer when you don't want to choose."
- One slide of decision heuristics. Include a one-liner: "If you need a true offline / installable PWA / static-only hosting, reach for the standalone WASM template instead of Blazor Web App."
- One "Miscellaneous Blazor features" slide — quick survey of things the talk didn't have time to demo: `<PageTitle>` and `<HeadContent>` for setting page title and head content from a component; pointers to QuickGrid, JS interop deeper APIs, and component libraries.
- One slide of "where to go next" (docs, samples, the research doc URL).

## Feature checklist

Track these against the demo project as it's built.

**Tier 1 — core Blazor (intro spine):**

- [ ] Component model: `.razor` file, `[Parameter]`, `ChildContent` / `RenderFragment`
- [ ] Catch-all parameter: `[Parameter(CaptureUnmatchedValues = true)]` for attribute splatting
- [ ] CSS isolation: `.razor.css`, `::deep` selector
- [ ] Component lifecycle: `OnInitialized(Async)`, `OnParametersSet(Async)`, `OnAfterRender(Async)`, `StateHasChanged()`
- [ ] Data binding: `@bind`, `@bind:after`
- [ ] Event handling: `@onclick`, lambdas, `EventCallback<T>`
- [ ] Control flow: `@if`, `@foreach`
- [ ] Routing: `@page`, route params, route constraints (reference slide), `NavLink`, `NavigationManager`
- [ ] Query-string params: `[SupplyParameterFromQuery]`
- [ ] DI: `@inject`, typed `HttpClient`
- [ ] Render modes: Static SSR, Server, WASM, Auto (per-component)
- [ ] Static SSR + enhanced navigation: form post-back works under SSR
- [ ] Forms and validation: `EditForm`, `InputText`, `DataAnnotationsValidator`, `ValidationMessage`
- [ ] JS interop: one `IJSRuntime.InvokeAsync` moment
- [ ] Header components: `<PageTitle>`, `<HeadContent>` (Miscellaneous closer)
- [ ] Standalone WASM template mention (one line in heuristics)

**Tier 2 — must-show .NET 9/10 features:**

- [ ] `RendererInfo.Name` and `AssignedRenderMode` (visible label, all four pages)
- [ ] `[StreamRendering]` (Section 1 follow-up beat)
- [ ] `[PersistentState]` (before/after comparison)
- [ ] `AddValidation()` + `[ValidatableType]` with nested model
- [ ] `NotFoundPage` parameter on `Router` (mention)
- [ ] WASM Hot Reload (live edit moment)
- [ ] 76% `blazor.web.js` reduction (one slide bullet)

**Tier 3 — optional, only if time:**

- [ ] Reconnection UI (kill server, show modal, recover) — only if Section 5 finishes under time
- [ ] Circuit state persistence — bullet mention paired with reconnection
- [ ] `MapStaticAssets` — mention during project tour

## Cut from this talk

Explicitly excluded so the scope stays honest:

- JS interop constructor / `GetValueAsync` / `SetValueAsync` (too niche for intro)
- QuickGrid `RowClass`, `HideColumnOptionsAsync`
- `InputHidden`
- Cookie-auth 401 behavior change, passkeys / WebAuthn (auth is its own talk)
- WASM environment configuration via `<WasmApplicationEnvironmentName>`
- Custom `PersistentComponentStateSerializer<T>`
- Cross-assembly validation models
- Bundler-friendly output, fingerprinting internals, boot config inlining
- MAUI Blazor Hybrid template
- Breaking changes list — kept as reference material in research doc, not stage material
- Daniel Roth "modern front end web framework" quote + "Browser + Razor" etymology reveal (prior deck slides 5–6) — cold-open demo is the hook now

## Risks and mitigations

- **Localhost is too fast to show Server latency.** Use Chrome devtools throttling ("Slow 4G"). Rehearse with throttling on. If the venue has restrictive networking, throttling still works because it's client-side.
- **Mode switcher breaks mid-talk.** If a page fails, flip to a working mode and keep going. The demo is one component, so resilience is built in.
- **WASM Hot Reload fails on stage.** Have a pre-recorded gif as backup; mention it as a feature even if the live demo doesn't land.
- **Source-gen validation gotcha (`.cs` not `.razor`)** is easy to forget in the heat of the moment. Pre-stage the model file open in a tab.
- **`[PersistentState]` requires public properties.** Pre-stage the property as public.
- **Auto-mode "first slow, then fast"** is invisible if the WASM bundle is already cached. Clear browser cache before the section, or use an incognito window for that page.

## Open decisions

1. **Whether to use the .NET 10 Blazor Web App template as-is or strip it down.** Default template includes Identity scaffolding — useful for realism, noisy for an intro. Lean toward stripping unless auth is needed for the demo.
2. **Whether to ship the demo repo for attendees.** If yes, the project structure becomes part of the artifact — affects how much boilerplate to leave in.
3. **Slot for `NavigateTo` scroll-behavior change and other breaking changes.** Currently cut. Reconsider only if the audience skews toward existing .NET 8 Blazor users.
