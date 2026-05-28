# Slides ↔ Demo gap analysis

Comparison of `slides/slides.md` against what's actually built in the demo. Three categories:

1. **Slides referencing things that no longer exist** (must fix — slide will show code that isn't in the repo).
2. **Slides with no live demo** (need a small demo built, OR explicitly accept "slide-only").
3. **Demos with no slide** (need a slide written).

A final section addresses **order alignment** — the demo and slides currently teach the same material in different orders.

---

## Built demo inventory

For reference, here's what's currently in the demo:

| Step | Routes | Demonstrates |
|---|---|---|
| 1 Render modes | `/static`, `/server`, `/wasm`, `/wasm-no-prerender`, `/auto` | All four render modes + the `prerender: false` variant; each shows a `RenderModeBadge` timeline |
| 2 Component anatomy | `/binding`, `/events`, `/lifecycle`, `/di` | `@bind` family, events + event args, all lifecycle hooks, `@inject` (GreetingService) |
| 3 SSR features | `/stream`, `/stream-blocking`, `/submit` | `[StreamRendering]` vs blocking; `EditForm` with `[ValidatableType]` source-gen validation on `PersonalInfo` |
| 4 Persistent state | `/product/with-persistence`, `/product/without-persistence`, `/cascading` | `[PersistentState]` (server vs browser label flip), `<CascadingValue>` / `[CascadingParameter]` |
| 5 Browser superpowers | `/js-interop` | C# → JS (clipboard) and JS → C# (window resize via `[JSInvokable]`) |

---

## 1. Slides referencing things that no longer exist

These slides will compile-fail if you open the referenced file, or will reference a route/type that doesn't load. **Highest priority — these will embarrass on stage.**

### Slide: "Parameters"

References `WidgetData` and `OnRefresh` in a `Widget` component:
```csharp
[Parameter] public WidgetData? Data { get; set; }
[Parameter] public EventCallback<WidgetData> OnRefresh { get; set; }
```

**Reality:** No `Widget` or `WidgetData` exists in the demo. The Dashboard/Widget theme from `talk-plan.md` was replaced with per-feature pages.

**Fix:** Rewrite the example using something that's actually in the codebase. The `ThemeProvider` / `ThemedCard` components are real `[Parameter]` consumers — they accept `CurrentTheme` and `ChildContent`. Or write the example as illustrative-only (no claim about which file it lives in).

### Slide: "ChildContent / RenderFragment"

References `WidgetCard Title="Sales"` and `$42,580` content. No `WidgetCard` exists.

**Fix:** Re-anchor on `ThemedCard` from the cascading values demo — it takes `ChildContent` exactly like the slide describes. The slide can show `<ThemedCard><ThemedHeader>...</ThemedHeader>...</ThemedCard>` as the real example. Or keep the slide generic if a teaching example reads cleaner.

### Slide: "Events"

References `EventCallback<WidgetData>` and `Widget.razor`. The pattern is real Blazor, but the named example doesn't exist.

**Fix:** Either re-anchor on `ThemedButton.OnClick` (which is a real `EventCallback`) or strip the named file references so the slide is teaching the pattern without claiming a specific file.

### Slide: "Routing"

References `@page "/comparison/{id:int}"` and `ComparisonDetail.razor`. The page still exists in the repo but is **orphaned** — its parent form was rewritten to `PersonalInfo`, the comparison model/service are unused, and there's no working flow into the page.

**Fix:** Either:
- (a) **Quick:** repurpose to `@page "/product/{id:int}"` and modify `WithPersistence.razor` to accept a route parameter (currently hardcoded to `id: 1`). This makes the slide live and gives the audience a working route-parameter demo.
- (b) **Slower:** remove `ComparisonDetail.razor` and the comparison plumbing entirely, then write the slide as illustrative-only.

I'd recommend (a) — small lift, real demo to point at.

### Slide: "Source-gen validation (.NET 10)"

```csharp
// Comparison.cs   ← .cs file, NOT .razor
[ValidatableType]
public class Comparison { ... Submitter ... Responses ... }
```

**Reality:** `Comparison.cs`, `Submitter.cs`, and `ComparisonResponse.cs` are orphaned. The demo's active validated model is `PersonalInfo.cs` (also `[ValidatableType]`, also has the `[Required]` / `[EmailAddress]` annotations).

**Fix:** Rewrite the slide using `PersonalInfo`. The `.cs`-not-`.razor` gotcha is still the lesson. To preserve the "nested object traversal" point (which `PersonalInfo` doesn't currently demonstrate), either:
- Add a nested object to `PersonalInfo` (e.g., an `Address` sub-object with its own `[Required]` fields).
- Or accept that the live demo only shows the flat case and the slide hints at nested traversal as a feature.

I'd recommend adding a nested `Address` — small lift, makes the slide truthful, and adds one more `InputXxx` field to the form.

### Slide: "Dependency injection"

References `IDashboardService Dashboard` and `HttpClient Http`. `IDashboardService` doesn't exist. `HttpClient` is registered in `.Client/Program.cs`.

**Fix:** Replace `IDashboardService` with `ProductService` or `GreetingService`. Both are real, both have the `@inject` pattern, both demonstrate scoped lifetime.

---

## 2. Slides with no matching live demo

These slides teach a concept but there's no code in the project for the audience to *see* it run. Either build a tiny demo or accept them as "slide-only reference material."

### Slide: "Parameters"

Beyond just fixing the type names, there's **no parent-passes-parameter demo on stage**. The `ThemeProvider` does use `[Parameter]`, but you'd be opening a component-library-style file, not showing a parameter being *passed* and consumed in the same view.

**Recommendation:** Cascading values demo already shows `[Parameter]` on `ThemeProvider` and `ThemedCard` (ChildContent). Walk through one of those files when this slide comes up — no new demo needed if you're willing to context-switch. If you want a dedicated demo, add a tiny `<HelloName Name="World" />` example on one of the existing pages.

### Slide: "ChildContent / RenderFragment"

Same situation — `ThemedCard` uses `ChildContent`, so you have a real example to point at, but it lives inside the cascading values demo rather than a focused slide-companion file.

**Recommendation:** Open `ThemedCard.razor` during the slide; the `[Parameter] public RenderFragment? ChildContent { get; set; }` line is right there. No new demo needed.

### Slide: "Control flow"

`@if` and `@foreach` are used throughout the codebase but there's no focused demo where the audience watches a list iterate or a conditional toggle.

**Recommendation:** Cut as a dedicated demo — point at the existing `@foreach (var entry in log)` in `Lifecycle.razor` and the `@if (Product is null)` in the persistence pages. Or skip the slide entirely; the audience already saw these throughout the demo.

### Slide: "Routing"

Route parameters and constraints have no live demo. Every `@page` directive in the codebase is static (no `{id:int}` etc.).

**Recommendation:** See fix in section 1 — convert `WithPersistence.razor` to take `@page "/product/{id:int}"` and bind the param. Small lift, real demo for both this slide and the "Route constraints" slide.

### Slide: "Route constraints"

Pure reference slide — explicitly marked "photograph it and keep moving." No demo needed.

### Slide: "Query string parameters"

`[SupplyParameterFromQuery]` has no live demo.

**Recommendation:** Either (a) add a small `@page "/dashboard"` with `[SupplyParameterFromQuery] Filter` to demo it concretely, or (b) cut from the talk — query-string binding is small enough that a slide-only treatment is fine for an intro audience.

### Slide: "Navigation"

`NavLink` is used heavily in `NavMenu.razor` (visible the whole talk). `NavigationManager.NavigateTo` is **not** demoed. `NotFoundPage` is mentioned but not wired.

**Recommendation:** Point at `NavMenu.razor` for `NavLink`. Either add a one-line `Nav.NavigateTo(...)` on the `Submit` form (it currently doesn't navigate after submit) or leave the slide showing the API and move on.

### Slide: "JavaScript interop"

The slide is intentionally minimal ("one sentence + a one-line console.log"). The demo at `/js-interop` is much richer — covers both directions, uses `IJSObjectReference`, has `DotNetObjectReference`.

**Recommendation:** Either expand the slide to match what you'll show (both directions, module pattern), or split into two slides — one for the basic `IJSRuntime.InvokeAsync` call and one for the more advanced "but you can also call C# from JS" pattern. The current slide undersells the demo.

### Slide: "WASM Hot Reload + a lighter runtime"

WASM Hot Reload is a live moment, not a built demo. The 76% reduction is a stat.

**Recommendation:** No demo to build. Practice the live-edit moment ahead of time. Have a fallback gif if it fails.

### Slide: "Miscellaneous Blazor"

`<PageTitle>` is used on every page (just not highlighted). `<HeadContent>` is not used anywhere.

**Recommendation:** No demo needed — slide is explicitly a survey. Could optionally add a `<HeadContent>` block to one page for the audience to see it in source.

---

## 3. Demos with no matching slide

Things that are built and would be shown on stage, but the slide deck has no companion slide explaining the concept.

### Cascading values (`/cascading`) — **major gap**

You have a fully-built theme cascade demo (`ThemeProvider` → `ThemedCard` → nested `ThemedHeader`, `ThemedButton`) that demonstrates `<CascadingValue>` and `[CascadingParameter]`. **There is no slide in the deck for this.**

**Recommendation:** Add a slide. Suggested content:

```razor
@* Provider — somewhere up the tree *@
<CascadingValue Value="theme">
  <ThemedCard>
    <ThemedHeader>...</ThemedHeader>
  </ThemedCard>
</CascadingValue>

@* Consumer — any depth below *@
[CascadingParameter] public Theme Theme { get; set; }
```

The slide should land that cascading values are the escape hatch from prop-drilling — the value is available at any depth without parent-to-child parameter passing.

Place this slide near the Components section (after ChildContent feels natural) OR in the "Beyond basics" section paired with `[PersistentState]`.

### WASM (no prerender) page (`/wasm-no-prerender`)

The page exists and demonstrates `@rendermode @(new InteractiveWebAssemblyRenderMode(prerender: false))`.

**Recommendation:** Either (a) add a brief aside in the "Four render modes" slide noting that prerender can be disabled per-instance, or (b) drop the page if the prerender-disable detail isn't worth the air time. The `RenderModeBadge` timeline does show the difference between prerendered and not-prerendered, which is the lesson.

### Stream vs blocking comparison (`/stream`, `/stream-blocking`)

The `[StreamRendering]` slide exists but only shows the attribute. The demo's killer feature — the **visual difference** between "blank screen for 3 seconds then everything appears" vs "placeholders that fill in one by one" — isn't on a slide.

**Recommendation:** Either extend the existing `[StreamRendering]` slide with one more sentence describing the UX difference, OR add a small companion slide with a `[STREAMING / BLOCKING]` mode-tag visual (since the demo's color-coded banners are the visceral lesson).

### Personal info form (`/submit`)

The `EditForm anatomy` slide exists. The actual form has more depth than the slide shows — multiple `InputXxx` components (`InputCheckbox`, `InputSelect`, `InputTextArea`, `InputDate`), `<ValidationSummary>`, programmatic cross-field validation via `ValidationMessageStore` (the 18+ check).

**Recommendation:** Either expand the slide to mention these (`<ValidationSummary>` and the broader `InputXxx` family in particular), or add a second forms slide covering programmatic validation via `EditContext` + `ValidationMessageStore`. The programmatic validation is the real-world lesson — annotations alone never cover everything.

### `RenderModeBadge` timeline

Not really a teach point — it's a visualization tool to make the prerender → interactive transition visible. No slide needed. Mention briefly during the cold-open demo: "the badge on each page shows the render path the component took."

---

## 4. Order alignment

The user noted slides and demo aren't in the same order. Here's the current mismatch:

| Topic | Slide section | Demo step |
|---|---|---|
| Render modes | Opening (slides 3-4) | Step 1 |
| Component anatomy | Components section (slides 6-7) | Step 2 (file walkthrough) |
| Data binding | Components section (slide 9) | Step 2 (`/binding`) |
| Lifecycle | Components section (slide 10) | Step 2 (`/lifecycle`) |
| Events | Behavior section (slide 12) | Step 2 (`/events`) |
| Control flow | Behavior section (slide 13) | (no focused demo) |
| Routing / nav / DI | Behavior section (slides 14-18) | Step 2 (`/di`); routing has no focused demo |
| JS interop | Behavior section (slide 19) | **Step 5** |
| Forms | Forms section (slides 21-22) | **Step 3** |
| `[StreamRendering]` | Opening (slide 4) | **Step 3** |
| `[PersistentState]` | Beyond basics (slide 24) | **Step 4** |
| Cascading values | (no slide) | **Step 4** |

The biggest order mismatches:
- **JS interop** is taught mid-talk in slides (behavior section) but is the last demo step. Either move the slide later in the deck, or move JS interop earlier in the demo flow.
- **Forms / `[StreamRendering]`** are split across the deck (intro + forms section) but are one demo step. The slide order makes sense conceptually; the demo just needs to be sequenced so streaming + form land together when you reach the Forms section.
- **`[PersistentState]`** sits in "Beyond basics" in slides but is one of the most visceral demos. Suggest moving the slide earlier OR accept that the demo waits for the late beat (the original `talk-plan.md` puts it in the "Persistent state, Auto mode, WASM hot reload" section 5, which matches the slide order).
- **Cascading values** is in the demo but missing from slides — once a slide is added, decide if it goes with Components (natural conceptual home) or with `[PersistentState]` in "Beyond basics" (where it'd pair with another late-talk concept).

The order conflicts mostly come down to: **the slide deck preserves the original `talk-plan.md` flow; the demo got built feature-first, by step number, without rechecking the talk plan's intended sequencing.** The demo can be navigated in any order during the talk — that's what the step-folder structure was designed for — so this is fixable by deciding the on-stage path through the demo at delivery time, not by reshuffling code.

---

## 5. Punch list for tomorrow (recommended order)

In order of "smallest lift, biggest payoff":

1. **Update slide code references** to point at types that exist (`PersonalInfo` instead of `Comparison`, `ProductService` instead of `IDashboardService`, etc.). 30 min of slide edits.
2. **Write a Cascading Values slide.** The demo is ready; this is just slide-writing.
3. **Add nested `Address` to `PersonalInfo`** so the "Source-gen validation traverses nested objects" claim on the validation slide is provable live. ~15 min.
4. **Repurpose `ComparisonDetail.razor` → `Product` route parameter demo** for the Routing slide. ~15 min.
5. **Decide whether to keep `[SupplyParameterFromQuery]`** in the talk. If yes, build a small demo. If no, cut the slide.
6. **Delete orphaned LLM/Comparison code** (`Comparison.cs`, `Submitter.cs`, `ComparisonResponse.cs`, `IComparisonService`, `ComparisonService(Client).cs`, `ResponseCard.razor`, `ComparisonDetail.razor`, the `/api/comparisons*` endpoints, the `ComparisonResponse` model) — only after deciding whether to repurpose `ComparisonDetail` for the routing demo.
7. **Decide the on-stage path through the demo** so it matches the slide flow (or vice versa).
8. **Practice the WASM Hot Reload moment** and have a fallback recording.
