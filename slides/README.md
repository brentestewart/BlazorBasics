# Slides — Blazor Fundamentals

Slidev deck for *Blazor Fundamentals: Getting Started with Modern .NET Web Development*.

## Local dev

```sh
npm install
npm run dev
```

Open the URL it prints. Edit `slides.md` — the dev server hot-reloads.

## Build / export

```sh
npm run build          # static site under dist/
npm run export         # PDF (requires Playwright Chromium: npx playwright install chromium)
```

## Structure

Single-file Markdown deck (`slides.md`) — slide separator is `---` on its own line. Slide-level YAML frontmatter (theme, layout) goes above each slide.

Speaker notes live inside `<!-- ... -->` blocks at the end of each slide. They show in presenter mode (`?presenter` URL or via `P` keyboard shortcut once running).
