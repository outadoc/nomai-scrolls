import hashlib
import sqlite3
from dataclasses import dataclass, field
from pathlib import Path

from jinja2 import Environment, PackageLoader
from markupsafe import Markup

from nomai_web.render import text_to_html


@dataclass
class _Block:
    block_id: int
    parent_block_id: int | None
    text: str
    speaker: str | None


@dataclass
class CommentNode:
    block_id: int
    speaker: str | None
    speaker_color: str | None
    body_html: Markup
    children: list["CommentNode"] = field(default_factory=list)


@dataclass
class Post:
    name: str
    display_name: str
    block_count: int


def _display_name(name: str) -> str:
    return name.replace("_", " ")


_SPEAKER_COLORS = [
    "#c0392b",  # crimson
    "#d35400",  # burnt orange
    "#b7770d",  # amber
    "#27ae60",  # emerald
    "#16a085",  # teal
    "#2980b9",  # steel blue
    "#8e44ad",  # purple
    "#c0168a",  # magenta
    "#1f7a8c",  # slate cyan
    "#6d4c41",  # warm brown
    "#00838f",  # dark cyan
    "#558b2f",  # olive green
]


def _speaker_color(name: str) -> str:
    digest = hashlib.md5(name.upper().encode()).digest()
    return _SPEAKER_COLORS[int.from_bytes(digest[:2], "big") % len(_SPEAKER_COLORS)]


def _build_tree(blocks: list[_Block], translations: dict[str, str]) -> list[CommentNode]:
    nodes: dict[int, CommentNode] = {}
    children_map: dict[int | None, list[CommentNode]] = {}

    for b in blocks:
        translated = translations.get(b.text, b.text)
        node = CommentNode(
            block_id=b.block_id,
            speaker=b.speaker,
            speaker_color=_speaker_color(b.speaker) if b.speaker else None,
            body_html=Markup(text_to_html(translated)),
        )
        nodes[b.block_id] = node
        children_map.setdefault(b.parent_block_id, []).append(node)

    for parent_id, child_nodes in children_map.items():
        if parent_id is not None and parent_id in nodes:
            nodes[parent_id].children = child_nodes

    return children_map.get(None, [])


def generate(db_path: Path, out_dir: Path) -> None:
    out_dir.mkdir(parents=True, exist_ok=True)

    with sqlite3.connect(db_path) as conn:
        languages: list[str] = [
            r[0] for r in conn.execute("SELECT code FROM languages ORDER BY code")
        ]

        file_names: list[str] = [
            r[0] for r in conn.execute("SELECT name FROM dialogue_files ORDER BY name")
        ]

        file_blocks: dict[str, list[_Block]] = {name: [] for name in file_names}
        for name, block_id, parent_block_id, text, speaker in conn.execute("""
            SELECT df.name, tb.block_id, tb.parent_block_id, tb.text, tb.speaker
            FROM text_blocks tb JOIN dialogue_files df USING (file_id)
            ORDER BY df.name, tb.block_id
        """):
            file_blocks[name].append(_Block(block_id, parent_block_id, text, speaker))

        all_translations: dict[str, dict[str, str]] = {}
        for lang_code, key, value in conn.execute("SELECT lang, key, value FROM translations"):
            all_translations.setdefault(lang_code, {})[key] = value

    env = Environment(
        loader=PackageLoader("nomai_web", "templates"),
        autoescape=True,
    )

    posts = [
        Post(name=name, display_name=_display_name(name), block_count=len(file_blocks[name]))
        for name in file_names
    ]

    # Root index: redirect to English (or first available language)
    default_lang = "en" if "en" in languages else languages[0]
    (out_dir / "index.html").write_text(
        f'<!DOCTYPE html><html><head><meta charset="utf-8">'
        f'<meta http-equiv="refresh" content="0;url={default_lang}/index.html">'
        f'</head><body></body></html>\n',
        encoding="utf-8",
    )

    index_tmpl = env.get_template("index.html")
    post_tmpl = env.get_template("post.html")

    for lang in languages:
        translations = all_translations.get(lang, {})
        lang_dir = out_dir / lang
        lang_dir.mkdir(exist_ok=True)

        lang_links = [(code, f"../{code}/index.html") for code in languages]

        (lang_dir / "index.html").write_text(
            index_tmpl.render(
                posts=posts,
                lang=lang,
                lang_links=lang_links,
                home_path="../index.html",
            ),
            encoding="utf-8",
        )

        for name in file_names:
            tree = _build_tree(file_blocks[name], translations)
            post_dir = lang_dir / name
            post_dir.mkdir(exist_ok=True)
            (post_dir / "index.html").write_text(
                post_tmpl.render(
                    display_name=_display_name(name),
                    tree=tree,
                    lang=lang,
                    lang_links=[(code, f"../../{code}/{name}/index.html") for code in languages],
                    home_path="../../index.html",
                    feed_path="../index.html",
                ),
                encoding="utf-8",
            )

        print(f"  {lang}: {len(file_names)} pages")

    print(f"\nDone → {out_dir}/")
