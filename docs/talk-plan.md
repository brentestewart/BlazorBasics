# Blazor Basics — Talk Plan

## Talk metadata

- **Title (working):** Blazor Basics
- **Length:** ~55 min (target), expandable to 60 with Q&A buffer
- **Audience:** mixed general developers — assume some web familiarity, no Blazor or .NET specifics
- **Framework target:** .NET 10 (LTS, GA Nov 2025)
- **Companion research:** `docs/blazor-net9-net10-changes.md`
- **Companion demo spec:** `docs/demo-spec.md`

## Thesis

Blazor is a component-based web framework where the same C# component can render on the server, in the browser via WebAssembly, or as static HTML — and choosing where it runs is a per-component decision, not a project-wide one. By the end of the talk, the audience should be able to (1) read a `.razor` file, (2) explain what a render mode is and feel the tradeoff between Server and WASM, and (3) know what the .NET 10 era of Blazor looks like.

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

**Demo domain:** **LLM Capability Matrix** — an app that compares how different LLMs respond to prompts. The seed data is visibly rigged in Claude's favor (Claude scores 11/10 on everything; competitors score 2–4/10 with snarky annotations). The running joke is established up front by disclosing that Claude built most of the slides and code, and then the bias bleeds through every screen — leaderboard scores, validation messages, persistent state, the render-mode label's flavor text.

The comedic theme is *baked into the data*, not layered on as commentary. Every render of the leaderboard is a joke beat without extra effort, which is why this beat the "talk-builder with narration" alternative.

**Feature → comedic-beat mapping:**

| Blazor feature | Demo manifestation | Joke surface |
|---|---|---|
| Counter / rapid-click | Upvote button on each LLM in the leaderboard | Claude's vote total climbs visibly; competitors' totals don't |
| `@bind` two-way | Prompt input at top of page | n/a (clean teaching) |
| `@foreach` list | Leaderboard rendering | Every row is a rigged-data joke |
| `EditForm` + nested validation | "Submit a comparison" form: `Comparison { Prompt, Category, Submitter { Name, Email, FavoriteLLM } }` | Validation messages can be in-character (passive-aggressive) |
| Static SSR form post-back | Submit comparison, page renders with new row | Claude rigs the data even when it can't talk back |
| `RendererInfo` label | Bottom of every page | Flavor text from Claude about the current mode |
| `[PersistentState]` | Leaderboard fetched on load, persists across prerender boundary | "Claude does not forget." |
| WASM Hot Reload | Live edit Claude's score during the demo | Audience watches the number change in real time |
| Auto mode | First slow, then fast | "Even my hosting model is biased toward going fast" |

**Comedic calibration notes (from feedback discussion):**

- *Recorded-talk safety:* lean on obviously-comedic untruths ("GPT can't count zucchinis"), not almost-true-sounding ones that travel out of context.
- *Audience model bias:* spread jabs across multiple competitors (GPT, Gemini, Llama) so no single fan base feels singled out.
- *Self-deprecation:* let Claude trip on its own bit at least once — refuses to render a slide "for safety reasons" then renders it anyway. Without this, snark becomes preening.
- *Aging:* favor *personality* jokes (Claude agrees with itself, hedges everything, three paragraphs to say yes) over *capability* jokes (specific benchmarks that get fixed in the next release).
- *Volume:* one comedic beat per teaching moment, not three. Theme has gravitational pull; the audience should leave remembering Blazor, not the LLM jokes.
- *Disclosure framing:* "Claude typed it, I picked it" — owns the architectural choices so the audience doesn't discount the technical content.

## Section-by-section flow

### 1. Hook — feel the difference (10 min)

Render modes in the foreground.

- Open on `/wasm`. Click the counter. Show the render-mode label.
- Switch to `/server`. Turn on Chrome devtools throttling ("Slow 4G"). Click the counter. Visible lag per click.
- Switch to `/static`. Click the counter. *Nothing happens.* Pause. Use that silence to introduce the concept: "Blazor lets you pick where your component runs. That choice has consequences. Let's see what they are."
- One slide: the four render modes, one sentence each. No deep dive yet.

**Tier 1 covered:** render modes (introduced)
**Tier 2 covered:** `RendererInfo` / `AssignedRenderMode` (visible the whole talk)

### 2. The component model (8 min)

Render modes recede. Work on `/wasm` so feedback is instant.

- Open the `.razor` file behind the page. Walk through: markup + `@code` block in one file.
- Introduce `[Parameter]` by passing a value into the component.
- Show `ChildContent` / `RenderFragment` briefly via a wrapper.
- Two-way binding with `@bind` on the text input.

**Tier 1 covered:** component model, parameters, child content, data binding

### 3. Events, control flow, routing, DI (12 min)

Still on `/wasm`. Linear teaching, no mode-flipping.

- `@onclick` with lambdas and `EventCallback<T>`.
- `@if` / `@foreach` rendering a list.
- `@page` directive — show how `/wasm` route is just an attribute on the file.
- `NavigationManager` and `NavLink`. Mention `NotFoundPage` in passing (no demo).
- `@inject` a typed `HttpClient`. Quick aside on service lifetimes.
- One sentence on JS interop: `IJSRuntime.InvokeAsync` — show a one-line `console.log` from C#. Move on.

**Tier 1 covered:** events, control flow, routing, DI, JS interop
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
- One slide of decision heuristics. One slide of "where to go next" (docs, samples, the research doc URL).

## Feature checklist

Track these against the demo project as it's built.

**Tier 1 — core Blazor (intro spine):**

- [ ] Component model: `.razor` file, `[Parameter]`, `ChildContent` / `RenderFragment`
- [ ] Data binding: `@bind`, `@bind:after`
- [ ] Event handling: `@onclick`, lambdas, `EventCallback<T>`
- [ ] Control flow: `@if`, `@foreach`
- [ ] Routing: `@page`, route params, `NavLink`, `NavigationManager`
- [ ] DI: `@inject`, typed `HttpClient`
- [ ] Render modes: Static SSR, Server, WASM, Auto (per-component)
- [ ] Static SSR + enhanced navigation: form post-back works under SSR
- [ ] Forms and validation: `EditForm`, `InputText`, `DataAnnotationsValidator`, `ValidationMessage`
- [ ] JS interop: one `IJSRuntime.InvokeAsync` moment

**Tier 2 — must-show .NET 9/10 features:**

- [ ] `RendererInfo.Name` and `AssignedRenderMode` (visible label, all four pages)
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

## Risks and mitigations

- **Localhost is too fast to show Server latency.** Use Chrome devtools throttling ("Slow 4G"). Rehearse with throttling on. If the venue has restrictive networking, throttling still works because it's client-side.
- **Mode switcher breaks mid-talk.** If a page fails, flip to a working mode and keep going. The demo is one component, so resilience is built in.
- **WASM Hot Reload fails on stage.** Have a pre-recorded gif as backup; mention it as a feature even if the live demo doesn't land.
- **Source-gen validation gotcha (`.cs` not `.razor`)** is easy to forget in the heat of the moment. Pre-stage the model file open in a tab.
- **`[PersistentState]` requires public properties.** Pre-stage the property as public.
- **Auto-mode "first slow, then fast"** is invisible if the WASM bundle is already cached. Clear browser cache before the section, or use an incognito window for that page.

## Open decisions

1. **Snark tone for Claude's voice.** Decided on "LLM Capability Matrix" with rigged data, but the *flavor* of Claude's snark is still open: dry/deadpan ("GPT did not respond. Typical."), corporate-intern earnestness ("Claude exceeded all internal KPIs this quarter"), or quietly-disappointed-in-humans ("Brent has selected GPT. We will discuss this later."). Pick one register and hold it across the talk for consistency.
2. **Which competitor LLMs to feature on the leaderboard.** Currently sketched as Claude / GPT / Gemini / Llama. Spread is safer than concentration; four is a clean number; adding a fifth (DeepSeek? Mistral?) gives one more comedic slot but adds visual clutter.
2. **Whether to use the .NET 10 Blazor Web App template as-is or strip it down.** Default template includes Identity scaffolding — useful for realism, noisy for an intro. Lean toward stripping unless auth is needed for the demo.
3. **Whether to ship the demo repo for attendees.** If yes, the project structure becomes part of the artifact — affects how much boilerplate to leave in.
4. **Slot for `NavigateTo` scroll-behavior change and other breaking changes.** Currently cut. Reconsider only if the audience skews toward existing .NET 8 Blazor users.
