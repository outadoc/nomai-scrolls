import re
import sqlite3
from dataclasses import dataclass
from pathlib import Path
from xml.etree import ElementTree

_SPEAKER_RE = re.compile(r"^([A-Z][A-Z ]+):\s")


@dataclass
class TextBlock:
    block_id: int
    parent_block_id: int | None
    text: str
    default_font_override: bool
    speaker: str | None


def _parse_stem(stem: str) -> tuple[int, str]:
    # e.g. "7565-BH_City_Forum_2" → (7565, "BH_City_Forum_2")
    file_id_str, name = stem.split("-", 1)
    return int(file_id_str), name


def parse_file(path: Path) -> tuple[int, str, list[TextBlock]]:
    file_id, name = _parse_stem(path.stem)
    tree = ElementTree.parse(path)
    blocks = []
    for tb in tree.iterfind("TextBlock"):
        id_el = tb.find("ID")
        if id_el is None or id_el.text is None:
            continue
        block_id = int(id_el.text.strip())

        parent_el = tb.find("ParentID")
        parent_block_id = int(parent_el.text.strip()) if parent_el is not None and parent_el.text else None

        # itertext() handles mixed content: plain text, CDATA, and inline tags like <em>
        text_el = tb.find("Text")
        text = "".join(text_el.itertext()).strip() if text_el is not None else ""

        default_font_override = tb.find("DefaultFontOverride") is not None

        m = _SPEAKER_RE.match(text)
        speaker = m.group(1).rstrip() if m else None
        if m:
            text = text[m.end():]

        blocks.append(TextBlock(block_id, parent_block_id, text, default_font_override, speaker))

    # Recording files store a linear audio log as a flat list — the XML has no
    # <ParentID> elements, but the in-file comment (e.g. "1 - 2 - 3 - 4 - 5")
    # confirms the blocks are meant to form a single chain. Synthesise the
    # parent links so they render as a thread instead of disconnected root blocks.
    if "Recording" in name:
        for i in range(1, len(blocks)):
            blocks[i].parent_block_id = blocks[i - 1].block_id

    return file_id, name, blocks


def load_all(conn: sqlite3.Connection, xml_dir: Path) -> int:
    total = 0
    for path in sorted(xml_dir.glob("*.xml")):
        file_id, name, blocks = parse_file(path)
        conn.execute(
            "INSERT OR IGNORE INTO dialogue_files (file_id, name) VALUES (?, ?)",
            (file_id, name),
        )
        conn.executemany(
            """INSERT OR IGNORE INTO text_blocks
               (file_id, block_id, parent_block_id, text, default_font_override, speaker)
               VALUES (?, ?, ?, ?, ?, ?)""",
            (
                (file_id, b.block_id, b.parent_block_id, b.text, int(b.default_font_override), b.speaker)
                for b in blocks
            ),
        )
        total += len(blocks)
    conn.commit()
    return total
