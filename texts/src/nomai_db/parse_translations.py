import sqlite3
from pathlib import Path
from xml.etree import ElementTree


def _lang_from_stem(stem: str) -> str:
    # e.g. "1325-Translation-en" → "en"
    return stem.rsplit("-", 1)[-1]


def parse_file(path: Path) -> tuple[str, list[tuple[str, str]]]:
    lang = _lang_from_stem(path.stem)
    tree = ElementTree.parse(path)
    entries = []
    for entry in tree.iterfind("entry"):
        key_el = entry.find("key")
        val_el = entry.find("value")
        if key_el is None or val_el is None:
            continue
        key = (key_el.text or "").strip()
        value = (val_el.text or "").strip()
        if key:
            entries.append((key, value))
    return lang, entries


def load_all(conn: sqlite3.Connection, translations_dir: Path) -> int:
    total = 0
    for path in sorted(translations_dir.glob("*.xml")):
        lang, entries = parse_file(path)
        conn.execute("INSERT OR IGNORE INTO languages (code) VALUES (?)", (lang,))
        conn.executemany(
            "INSERT OR IGNORE INTO translations (lang, key, value) VALUES (?, ?, ?)",
            ((lang, key, value) for key, value in entries),
        )
        total += len(entries)
    conn.commit()
    return total
