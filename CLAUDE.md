# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

Tools for exploring Outer Wilds Nomai dialogue. Raw XML assets (extracted via AssetRipper) are parsed into an SQLite database, which the TUI and static site generator can browse.

## Repository layout

Four independent Python projects, each with its own `.venv`:

| Directory | Purpose |
|-----------|---------|
| `extract/` | One-shot scripts to scrape asset IDs and fetch raw XML files from a running AssetRipper server |
| `import/`  | Parses the XML files into an SQLite database (`nomai_import` package) |
| `app/`     | Textual TUI that browses the database (`nomai_db` package) |
| `web/`     | Static site generator producing a Reddit-like HTML site (`nomai_web` package) |

Each project has its own `pyproject.toml`. The `.venv/` directories are gitignored at the root.

## Setup

```bash
# import tool (no third-party deps)
cd import && python -m venv .venv && source .venv/bin/activate && pip install -e .

# TUI app (requires textual)
cd app && python -m venv .venv && source .venv/bin/activate && pip install -e .

# Static site generator (requires jinja2)
cd web && python -m venv .venv && source .venv/bin/activate && pip install -e .
```

## Common commands

```bash
# Rebuild the database from XML sources (run from repo root)
cd import && python -m nomai_import --db ../nomai.db

# Launch the TUI (run from repo root)
nomai-db-tui --db nomai.db

# Generate static HTML site for all languages (run from repo root)
nomai-web generate --db nomai.db --out out

# Generate and serve for local development
nomai-web serve --db nomai.db             # → http://localhost:8000/
nomai-web serve --db nomai.db --port 9000

# Fetch new XML assets from AssetRipper (requires a running instance on :40913)
cd extract
./run.sh                              # installs deps into extract/.venv, then runs extract_rows.py
./fetch_texts.sh parsed.csv xml/      # downloads individual XML files
```

`nomai.db` lives at the repo root and is a derived artifact — delete it and re-run the import whenever the schema or parsing logic changes. `INSERT OR IGNORE` means re-running without deleting will not update existing rows.

## Data flow

```
AssetRipper → extract/ scripts → import/xml/ + import/translations/
                                       ↓
                               nomai-import (import/)
                                       ↓
                                  nomai.db (SQLite, repo root)
                                       ↓
                               nomai-db-tui (app/)
```

## Key invariant

`text_blocks.text == translations.key` — both are stripped of their `UPPERCASE NAME:` speaker prefix at import time so they stay in sync as the translation join key.

## Database schema

```
languages        (code)
translations     (lang, key, value)          -- key = stripped dialogue text
dialogue_files   (file_id, name)
text_blocks      (file_id, block_id, parent_block_id, text, default_font_override, speaker)
```

`parent_block_id` references `block_id` within the same `file_id`, forming a comment-thread-style tree. `speaker` is `NULL` for non-dialogue blocks (UI labels, system messages).

## import/ internals

- `parse_xml.py` — parses `<NomaiObject>` files; `TextBlock` nodes linked by `<ParentID>` form trees within a file
- `parse_translations.py` — parses `<TranslationTable_XML>` files; language code from filename suffix (e.g. `1325-Translation-en.xml` → `en`)
- `schema.py` — `init_db()` creates tables; includes an `ALTER TABLE` guard for the `speaker` column so re-running on an existing DB is safe

## app/ internals

Built with [Textual](https://github.com/Textualize/textual). All data is loaded from SQLite into memory in `NomaiApp.__init__()` before `compose()` runs — required by Textual's lifecycle.

- Left panel: `ListView` of dialogue file names; right panel: `VerticalScroll` with a `Static` updated on selection
- `_render_tree()` builds a Rich `Text` with ASCII box-drawing (`├──`, `└──`, `│`); word-wrap is manual so continuation lines get the correct `│   ` indent
- Language switching (←/→) and terminal resize both call `_update_tree()`, which re-queries the content width from the widget before re-rendering
- Game color markup `<color=orange>…</color>` is stored as-is and parsed at render time in `_parse_text()`
- `\\n` in raw text is a two-character C# escape, not a real newline — converted by `re.sub(r"\\+n", "\n", raw, flags=re.IGNORECASE)`

## web/ internals

Reads from `nomai.db`, writes a self-contained static site to `--out` (default `out/`).

- `render.py` — `text_to_html()` mirrors the TUI's `_parse_text()`: handles `\\n` escapes, maps `<color=name>…</color>` to `<span style="color:…">`, and wraps everything in `html.escape()`
- `site.py` — queries DB, builds `CommentNode` trees (same `parent_block_id` → children logic as the TUI), renders Jinja2 templates for all languages; output layout is `out/{lang}/index.html` + `out/{lang}/{name}/index.html`; `out/index.html` redirects to English
- `templates/` — `base.html` (shell with lang selector nav), `index.html` (feed), `post.html` (recursive `render_comment` macro for nested replies)
- CSS lives inline in `site.py` as `_CSS` and is written to `out/style.css`
- `body_html` fields are `markupsafe.Markup` objects so Jinja2's autoescape doesn't re-escape already-rendered HTML
- Lang selector links use relative paths (`../../{lang}/{name}/index.html`) so the site works without a server
