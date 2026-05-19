import argparse
import re
import sqlite3
from dataclasses import dataclass
from pathlib import Path

from rich.markup import escape
from textual.app import App, ComposeResult
from textual.binding import Binding
from textual.containers import VerticalScroll
from textual.widgets import Footer, Header, Label, ListItem, ListView, Static


@dataclass
class Block:
    block_id: int
    parent_block_id: int | None
    text: str


def _strip_tags(text: str) -> str:
    return re.sub(r"<[^>]+>", "", text).replace("\\n", "\n").strip()


def _render_tree(
    blocks: list[Block],
    lang: str,
    translations: dict[tuple[str, str], str],
) -> str:
    children: dict[int | None, list[Block]] = {}
    for b in blocks:
        children.setdefault(b.parent_block_id, []).append(b)

    lines: list[str] = []

    def render(parent_id: int | None, depth: int) -> None:
        for b in children.get(parent_id, []):
            text = _strip_tags(translations.get((lang, b.text), b.text))
            prefix = "  " * (depth - 1) + "↳ " if depth > 0 else ""
            continuation_indent = "  " * depth
            text_lines = text.splitlines() or [""]
            for i, line in enumerate(text_lines):
                lines.append(escape((prefix if i == 0 else continuation_indent) + line))
            lines.append("")
            render(b.block_id, depth + 1)

    render(None, 0)
    return "\n".join(lines).strip()


class NomaiApp(App):
    TITLE = "Nomai Texts"
    BINDINGS = [
        Binding("q", "quit", "Quit"),
        Binding("left", "prev_lang", "Prev lang"),
        Binding("right", "next_lang", "Next lang"),
    ]
    DEFAULT_CSS = """
    ListView {
        height: 30%;
        border: solid $accent;
    }
    VerticalScroll {
        height: 1fr;
        border: solid $accent;
        padding: 1;
    }
    """

    def __init__(self, db_path: Path) -> None:
        super().__init__()
        self._lang_idx: int = 0
        with sqlite3.connect(db_path) as conn:
            self._languages: list[str] = [
                r[0] for r in conn.execute("SELECT code FROM languages ORDER BY code")
            ]
            if "en" in self._languages:
                self._lang_idx = self._languages.index("en")

            self._files: list[str] = [
                r[0] for r in conn.execute("SELECT name FROM dialogue_files ORDER BY name")
            ]
            self._file_blocks: dict[str, list[Block]] = {name: [] for name in self._files}
            for name, block_id, parent_block_id, text in conn.execute("""
                SELECT df.name, tb.block_id, tb.parent_block_id, tb.text
                FROM text_blocks tb JOIN dialogue_files df USING (file_id)
                ORDER BY df.name, tb.block_id
            """):
                self._file_blocks[name].append(Block(block_id, parent_block_id, text))

            self._translations: dict[tuple[str, str], str] = {
                (lang, key): value
                for lang, key, value in conn.execute("SELECT lang, key, value FROM translations")
            }

    def on_mount(self) -> None:
        self._refresh_subtitle()

    def compose(self) -> ComposeResult:
        yield Header()
        yield ListView(
            *[ListItem(Label(name)) for name in self._files],
            id="files",
        )
        yield VerticalScroll(Static("", id="tree"))
        yield Footer()

    def _current_lang(self) -> str:
        return self._languages[self._lang_idx] if self._languages else "?"

    def _refresh_subtitle(self) -> None:
        n = len(self._languages)
        self.sub_title = f"Language: {self._current_lang()} ({self._lang_idx + 1}/{n})"

    def _update_tree(self) -> None:
        lv = self.query_one("#files", ListView)
        idx = lv.index
        if idx is None or idx >= len(self._files):
            return
        blocks = self._file_blocks[self._files[idx]]
        content = _render_tree(blocks, self._current_lang(), self._translations)
        self.query_one("#tree", Static).update(content or "(no content)")

    def on_list_view_highlighted(self, _: ListView.Highlighted) -> None:
        self._update_tree()

    def action_next_lang(self) -> None:
        if self._languages:
            self._lang_idx = (self._lang_idx + 1) % len(self._languages)
            self._refresh_subtitle()
            self._update_tree()

    def action_prev_lang(self) -> None:
        if self._languages:
            self._lang_idx = (self._lang_idx - 1) % len(self._languages)
            self._refresh_subtitle()
            self._update_tree()


def main() -> None:
    parser = argparse.ArgumentParser(description="Browse Nomai dialogue texts")
    parser.add_argument("--db", default="nomai.db")
    args = parser.parse_args()
    NomaiApp(Path(args.db)).run()


if __name__ == "__main__":
    main()
