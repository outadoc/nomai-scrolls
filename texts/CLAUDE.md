# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

This project parses Outer Wilds game dialogue XML files (extracted via AssetRipper) into an SQLite database, and provides a terminal UI to browse them.

## Setup and commands

```bash
python -m venv .venv && source .venv/bin/activate
pip install -e .

# Rebuild the database from XML sources (required after schema or parser changes)
python -m nomai_db

# Launch the TUI browser
nomai-db-tui --db nomai.db
```

The database (`nomai.db`) is a derived artifact — delete and re-run `python -m nomai_db` whenever schema or parsing logic changes. `INSERT OR IGNORE` means re-running without deleting will not update existing rows.

## Architecture

Two input directories feed one database:

- `translations/{id}-Translation-{lang}.xml` — `<TranslationTable_XML>` with `<entry><key>/<value>` pairs. Language code comes from the filename suffix.
- `xml/{id}-{Name}.xml` — `<NomaiObject>` dialogue trees with `<TextBlock>` nodes linked by `<ParentID>`.

**Key invariant:** `text_blocks.text == translations.key` (after stripping) — the dialogue text is the join key between tables. Both are stripped of speaker prefixes at import time so they stay in sync.

### Database schema

```
languages        (code)
translations     (lang, key, value)          -- key = stripped dialogue text
dialogue_files   (file_id, name)
text_blocks      (file_id, block_id, parent_block_id, text, default_font_override, speaker)
```

`parent_block_id` references `block_id` within the same `file_id` (not a global FK), forming a comment-thread-style tree. `speaker` is extracted from the `UPPERCASE NAME:` prefix and `NULL` for non-dialogue blocks (UI labels, system messages — ~17% of blocks).

### Parsing notes

- `text_el.itertext()` is used (not `.text`) to handle mixed XML content with inline child elements.
- Game color markup `<color=orange>…</color>` is stored as-is in the DB; the TUI parses it at display time via `_parse_text()` in `tui.py`.
- `\\N` / `\\n` in text is a literal two-character escape sequence from the C# source, not a real newline — converted by `re.sub(r"\\+n", "\n", raw, flags=re.IGNORECASE)`.
- Speaker prefix regex: `^([A-Z][A-Z ]+):\s` — must start with uppercase letters/spaces and be followed by a colon and whitespace. Strips to `text` at import; stored separately in `speaker`.

### TUI (`tui.py`)

Built with [Textual](https://github.com/Textualize/textual). All data is loaded from SQLite into memory in `__init__` (before `compose()` runs — required by Textual's lifecycle).

- Left/top panel: `ListView` of dialogue file names.
- Right/bottom panel: `VerticalScroll` containing a `Static` updated with a Rich `Text` object.
- `_render_tree()` builds the Rich `Text` with ASCII box-drawing tree characters (`├──`, `└──`, `│`). Word-wrapping is done manually using `Text.wrap()` so continuation lines get the correct `│   ` prefix.
- Language switching (←/→) and terminal resize both call `_update_tree()`, which re-queries the content width from the widget before re-rendering.
