# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

Tools for exploring Outer Wilds Nomai dialogue. Raw XML assets (extracted via AssetRipper) are parsed into an SQLite database, which the TUI browses.

## Repository layout

Three independent Python projects, each with its own `.venv`:

| Directory | Purpose |
|-----------|---------|
| `extract/` | One-shot scripts to scrape asset IDs and fetch raw XML files from a running AssetRipper server |
| `import/`  | Parses the XML files into an SQLite database (`nomai_import` package) |
| `app/`     | Textual TUI that browses the database (`nomai_db` package) |

Each project has its own `pyproject.toml`. The `.venv/` directories are gitignored at the root.

## Setup

```bash
# import tool (no third-party deps)
cd import && python -m venv .venv && source .venv/bin/activate && pip install -e .

# TUI app (requires textual)
cd app && python -m venv .venv && source .venv/bin/activate && pip install -e .
```

## Common commands

```bash
# Rebuild the database from XML sources (run from repo root)
cd import && python -m nomai_import --db ../nomai.db

# Launch the TUI (run from repo root)
nomai-db-tui --db nomai.db

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
