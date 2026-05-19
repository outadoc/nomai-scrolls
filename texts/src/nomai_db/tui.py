import argparse
import re
import sqlite3
from dataclasses import dataclass
from pathlib import Path

from rich.console import Console
from rich.text import Text
from textual.app import App, ComposeResult
from textual.binding import Binding
from textual.containers import VerticalScroll
from textual.widgets import Footer, Header, Label, ListItem, ListView, Static

_CONSOLE = Console(width=9999, highlight=False)

_COLOR_MAP: dict[str, str] = {
    "black":     "bright_black",
    "grey":      "bright_black",
    "lightblue": "bright_cyan",
    "orange":    "orange1",
    "red":       "bright_red",
}

_COLOR_RE = re.compile(r"<color=([^\n>]+)>(.*?)</color>", re.DOTALL)
_TAG_RE   = re.compile(r"<[^>]*>")


@dataclass
class Block:
    block_id: int
    parent_block_id: int | None
    text: str


def _strip_tags(text: str) -> str:
    return _TAG_RE.sub("", text).replace("\\n", " ").strip()


def _parse_text(raw: str) -> Text:
    raw = re.sub(r"\\+n", "\n", raw, flags=re.IGNORECASE)
    result = Text()
    pos = 0
    for m in _COLOR_RE.finditer(raw):
        if m.start() > pos:
            result.append(_TAG_RE.sub("", raw[pos:m.start()]))
        color = _COLOR_MAP.get(m.group(1).lower(), m.group(1))
        result.append(_TAG_RE.sub("", m.group(2)), style=color)
        pos = m.end()
    if pos < len(raw):
        result.append(_TAG_RE.sub("", raw[pos:]))
    return result


def _render_tree(
    blocks: list[Block],
    lang: str,
    translations: dict[tuple[str, str], str],
    width: int = 80,
) -> Text:
    children: dict[int | None, list[Block]] = {}
    for b in blocks:
        children.setdefault(b.parent_block_id, []).append(b)

    result = Text()

    def add_block(first_prefix: str, cont_prefix: str, content: Text) -> None:
        available = max(10, width - len(first_prefix))
        for j, line in enumerate(content.wrap(_CONSOLE, available)):
            result.append(first_prefix if j == 0 else cont_prefix, style="dim")
            result.append_text(line)
            result.append("\n")

    def render(parent_id: int | None, prefix: str) -> None:
        siblings = children.get(parent_id, [])
        for i, b in enumerate(siblings):
            is_last = i == len(siblings) - 1
            content = _parse_text(translations.get((lang, b.text), b.text))

            if parent_id is None:
                if i > 0:
                    result.append("\n")
                add_block("", "", content)
                render(b.block_id, "")
            else:
                connector    = "└── " if is_last else "├── "
                continuation = "    " if is_last else "│   "
                add_block(prefix + connector, prefix + continuation, content)
                render(b.block_id, prefix + continuation)

    render(None, "")
    result.rstrip()
    return result


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
        scroll = self.query_one(VerticalScroll)
        width = scroll.content_size.width or self.size.width
        content = _render_tree(blocks, self._current_lang(), self._translations, width=width)
        self.query_one("#tree", Static).update(content if content.plain else "(no content)")

    def on_list_view_highlighted(self, _: ListView.Highlighted) -> None:
        self._update_tree()

    def on_resize(self, _) -> None:
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
