import argparse
import re
import sqlite3
from dataclasses import dataclass
from pathlib import Path

from textual.app import App, ComposeResult
from textual.binding import Binding
from textual.widgets import Footer, Header, Label, ListItem, ListView, Static


@dataclass
class Block:
    file_name: str
    block_id: int
    parent_block_id: int | None
    text: str


def _strip_tags(text: str) -> str:
    return re.sub(r"<[^>]+>", "", text).replace("\\n", " ").strip()


class NomaiApp(App):
    TITLE = "Nomai Texts"
    BINDINGS = [
        Binding("q", "quit", "Quit"),
        Binding("left", "prev_lang", "Prev lang"),
        Binding("right", "next_lang", "Next lang"),
    ]

    def __init__(self, db_path: Path) -> None:
        super().__init__()
        self._lang_idx: int = 0
        with sqlite3.connect(db_path) as conn:
            self._languages: list[str] = [
                r[0] for r in conn.execute("SELECT code FROM languages ORDER BY code")
            ]
            if "en" in self._languages:
                self._lang_idx = self._languages.index("en")
            self._blocks: list[Block] = [
                Block(*r)
                for r in conn.execute("""
                    SELECT df.name, tb.block_id, tb.parent_block_id, tb.text
                    FROM text_blocks tb JOIN dialogue_files df USING (file_id)
                    ORDER BY df.name, tb.block_id
                """)
            ]
            self._translations: dict[tuple[str, str], str] = {
                (lang, key): value
                for lang, key, value in conn.execute("SELECT lang, key, value FROM translations")
            }

    def on_mount(self) -> None:
        self._refresh_subtitle()

    def compose(self) -> ComposeResult:
        yield Header()
        yield ListView(
            *[
                ListItem(Label(f"{b.file_name} · {b.block_id}  {_strip_tags(b.text)}"))
                for b in self._blocks
            ],
            id="blocks",
        )
        yield Static("", id="detail")
        yield Footer()

    def _current_lang(self) -> str:
        return self._languages[self._lang_idx] if self._languages else "?"

    def _refresh_subtitle(self) -> None:
        n = len(self._languages)
        self.sub_title = f"Language: {self._current_lang()} ({self._lang_idx + 1}/{n})"

    def _update_detail(self) -> None:
        lv = self.query_one("#blocks", ListView)
        idx = lv.index
        if idx is None or idx >= len(self._blocks):
            return
        block = self._blocks[idx]
        lang = self._current_lang()
        translation = self._translations.get((lang, block.text), "(no translation)")
        parent = f"parent: {block.parent_block_id}" if block.parent_block_id else "root"
        self.query_one("#detail", Static).update(
            f"[bold]{block.file_name}[/bold]  Block {block.block_id}  ({parent})\n\n{translation}"
        )

    def on_list_view_highlighted(self, _: ListView.Highlighted) -> None:
        self._update_detail()

    def action_next_lang(self) -> None:
        if self._languages:
            self._lang_idx = (self._lang_idx + 1) % len(self._languages)
            self._refresh_subtitle()
            self._update_detail()

    def action_prev_lang(self) -> None:
        if self._languages:
            self._lang_idx = (self._lang_idx - 1) % len(self._languages)
            self._refresh_subtitle()
            self._update_detail()


def main() -> None:
    parser = argparse.ArgumentParser(description="Browse Nomai dialogue texts")
    parser.add_argument("--db", default="nomai.db")
    args = parser.parse_args()
    NomaiApp(Path(args.db)).run()


if __name__ == "__main__":
    main()
