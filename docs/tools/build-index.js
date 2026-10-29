#!/usr/bin/env node
/**
 * build-index.js — regenerate docs/README.md (page map + maintenance notes)
 * from the live documentation data.
 *
 * Usage:  node docs/tools/build-index.js
 */
const fs = require("fs");
const path = require("path");
const vm = require("vm");

const DOCS = path.resolve(__dirname, "..");
const FILES = [
    "data/legacy.js",
    "data/guides.js",
    "data/examples.js",
    "data/custom.js",
    "data/basic.js",
    "data/enrich.js",
    "data/auto.generated.js",
    "data/nav.js",
    "data.js",
    "data/en/ui.js",
    "data/en/guides.js",
    "data/en/examples.js",
    "data/en/engineering.js",
    "data/en/data.js",
    "data/en/input.js",
    "data/en/navigation.js",
    "data/en/feedback.js",
    "data/en/misc.js"
];

const ctx = { window: {}, console };
vm.createContext(ctx);
for (const rel of FILES) {
    const file = path.join(DOCS, rel);
    vm.runInContext(fs.readFileSync(file, "utf8"), ctx, { filename: rel });
}

const DATA = ctx.window.DOCS_DATA;
const AUTO = ctx.window.DOCS_AUTO || {};
const NAV = ctx.window.DOCS_NAV;
const EN = ctx.window.DOCS_EN.pages;

const plain = (s) => String(s || "")
    .replace(/<[^>]+>/g, "")
    .replace(/\|/g, "\\|")
    .replace(/\s+/g, " ")
    .trim();

const pages = Object.keys(DATA).length;
let props = 0;
for (const id of Object.keys(DATA)) {
    const page = DATA[id];
    props += (page.props && page.props.length) ? page.props.length : ((AUTO[id] || {}).props || []).length;
}

const out = [];
const push = (...lines) => out.push(...lines);

push("# CONCOST.UI — Documentation", "");
push("The documentation is a self-contained static site: open [`index.html`](index.html) in any browser (no build step, no server required).", "");
push("**Live site:** <https://thanhphuong-dev.github.io/CONCOST.UI/> — published from this folder (`main` → `/docs`) through GitHub Pages. Add `?lang=en` or `?lang=vi` to force a language.", "");
push("Features: command palette search (`Ctrl K` or `/`), per-page table of contents with reading progress, XAML ↔ C# code inspector with copy, light/dark themes, five accent colours, keyboard navigation and deep links (`index.html#splitbutton`).", "");
push(`**${pages} pages · ${props} documented properties · ${Object.keys(AUTO).length} controls mapped to source · 2 languages (VI / EN).**`, "");
push("### Languages", "");
push("The wiki ships in Vietnamese and English. The **VI / EN** button in the top bar switches instantly (persisted in `localStorage`), and the language can be forced through the URL:", "");
push("```text", "docs/index.html?lang=en#splitbutton", "docs/index.html?lang=vi#splitbutton", "```", "");
push("Vietnamese is the base language (`data/*.js`); English lives in the overlay files under `data/en/`. A page renders in English when its overlay entry exists, otherwise it falls back to Vietnamese, so a half-translated page never breaks the build.", "");
push("## Page map", "");

for (const group of NAV) {
    push(`### ${group.titleEn || group.title}`, "");
    push("| Page | Nội dung |", "| --- | --- |");
    for (const item of group.items) {
        const page = DATA[item.id] || {};
        const desc = plain(page.desc).slice(0, 150);
        push(`| [${plain(item.name)}](index.html#${item.id}) | ${desc} |`);
    }
    push("");
}

push("## How this documentation is maintained", "");
push("| File | Role |", "| --- | --- |");
push("| `data/legacy.js` | Original control pages (prose + XAML). |");
push("| `data/guides.js` | FAQ and migration guides. |");
push("| `data/examples.js` | Cookbook page with complete, combined examples. |");
push("| `data/custom.js` | Newer control pages and C# overrides. |");
push("| `data/basic.js` | Styling-only basic controls + attached property tables. |");
push("| `data/enrich.js` | Remaining property tables and C# snippets. |");
push("| `data/auto.generated.js` | **Generated** property tables, C# samples and English property descriptions. Do not edit. |");
push("| `data/nav.js` | Sidebar order, icons, badges (VI + EN labels). |");
push("| `data.js` | Merges every dataset into `DOCS_DATA`. |");
push("| `data/en/ui.js` | UI chrome strings (VI + EN), badge labels and the `DOCS_EN` container. |");
push("| `data/en/*.js` | English overlay per page: title, description, section text, callouts and tables. |");
push("| `app.js` / `style.css` / `index.html` | Application shell, design system and language switch. |");
push("");
push("### Regenerate the generated parts", "");
push("```bash");
push("python docs/tools/build-props.py     # property tables + C# samples + EN property text from src/CONCOST.UI/**/*.cs");
push("python docs/tools/check-xaml.py      # validate every XAML snippet against the real API");
push("node   docs/tools/build-index.js     # rebuild this page map");
push("```", "");
push("Run all three after adding or renaming a `DependencyProperty`; the checker must report **0 issues** before committing.", "");
push("### Adding a page", "");
push("1. Add the page object to the matching `data/*.js` file (`title`, `badge`, `desc`, `sections[]` with `xaml`/`csharp`).");
push("2. Add the English strings to the matching `data/en/*.js` file (same `sections` index order).");
push("3. Register it in `data/nav.js` — the order there drives the sidebar and prev/next links.");
push("4. Run `build-props.py` if the page documents a control that is new to `PAGE_CLASS`, then `check-xaml.py` and `build-index.js`.", "");
push("## Related documents", "");
push("- [Getting Started](GettingStarted.md) — integration, theming, plugin hosting, build.");
push("- [Migration Guide](MigrationGuide.md) — WinForms/WPF mapping and deprecation policy.");
push("- [Roadmap](roadmap/) — active roadmap (revision 3) and archived plans.");
push("- [Changelog](../CHANGELOG.md).", "");

fs.writeFileSync(path.join(DOCS, "README.md"), out.join("\n"), "utf8");
console.log(`docs/README.md written — ${pages} pages, ${props} properties.`);
