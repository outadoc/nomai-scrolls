# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this directory.

## Project overview

TUI browser for Outer Wilds Nomai dialogue, backed by an SQLite database populated by the `nomai-import` tool (see `../import/`).

## Setup and commands

```bash
python -m venv .venv && source .venv/bin/activate
pip install -e .

# Launch the TUI browser
nomai-db-tui --db nomai.db
```

The database (`nomai.db`) is a derived artifact produced by `nomai-import` in `../import/`. Delete it and re-run the import whenever the schema or parser changes.

## Architecture

The TUI reads entirely from `nomai.db` — it has no dependency on parsing code or XML files.

### Database schema (read-only from the TUI's perspective)

```
languages        (code)
translations     (lang, key, value)          -- key = stripped dialogue text
dialogue_files   (file_id, name)
text_blocks      (file_id, block_id, parent_block_id, text, default_font_override, speaker)
```

`parent_block_id` references `block_id` within the same `file_id`, forming a comment-thread-style tree. `speaker` is `NULL` for non-dialogue blocks (UI labels, system messages).

### TUI (`tui.py`)

Built with [Textual](https://github.com/Textualize/textual). All data is loaded from SQLite into memory in `__init__` (before `compose()` runs — required by Textual's lifecycle).

- Left/top panel: `ListView` of dialogue file names.
- Right/bottom panel: `VerticalScroll` containing a `Static` updated with a Rich `Text` object.
- `_render_tree()` builds the Rich `Text` with ASCII box-drawing tree characters (`├──`, `└──`, `│`). Word-wrapping is done manually using `Text.wrap()` so continuation lines get the correct `│   ` prefix.
- Language switching (←/→) and terminal resize both call `_update_tree()`, which re-queries the content width from the widget before re-rendering.

### Display notes

- Game color markup `<color=orange>…</color>` is stored as-is in the DB and parsed at display time via `_parse_text()`.
- `\\N` / `\\n` in text is a literal two-character escape from C# source, converted by `re.sub(r"\\+n", "\n", raw, flags=re.IGNORECASE)`.
