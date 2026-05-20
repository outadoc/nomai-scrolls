import sqlite3
from dataclasses import dataclass, field
from pathlib import Path

from jinja2 import Environment, PackageLoader
from markupsafe import Markup

from nomai_web.render import text_to_html

_CSS = """\
:root {
    --bg: #1a1a1b;
    --surface: #272729;
    --border: #343536;
    --text: #d7dadc;
    --muted: #818384;
    --accent: #ff4500;
    --link: #4fbdff;
}

*, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }

body {
    background: var(--bg);
    color: var(--text);
    font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
    font-size: 14px;
    line-height: 1.5;
    min-height: 100vh;
}

a { color: var(--link); }
a:hover { text-decoration: underline; }

header {
    background: var(--surface);
    border-bottom: 1px solid var(--border);
    padding: 8px 20px;
    position: sticky;
    top: 0;
    z-index: 10;
}

.site-title {
    color: var(--accent);
    font-weight: 700;
    font-size: 18px;
    font-style: italic;
    text-decoration: none;
}
.site-title:hover { opacity: 0.8; text-decoration: none; }

main {
    max-width: 740px;
    margin: 16px auto;
    padding: 0 8px;
}

/* Feed */
.feed { display: flex; flex-direction: column; gap: 4px; }

.post {
    background: var(--surface);
    border: 1px solid var(--border);
    border-radius: 4px;
    display: flex;
    align-items: flex-start;
    padding: 8px;
    gap: 8px;
    transition: border-color 0.1s;
}
.post:hover { border-color: var(--muted); }

.post-score {
    display: flex;
    flex-direction: column;
    align-items: center;
    min-width: 36px;
    padding-top: 2px;
    gap: 2px;
}
.post-score .arrow { color: var(--accent); font-size: 18px; line-height: 1; }
.post-score .count { color: var(--text); font-size: 12px; font-weight: 700; }

.post-info { flex: 1; min-width: 0; }

.post-title {
    color: var(--text);
    font-size: 18px;
    font-weight: 500;
    text-decoration: none;
    line-height: 1.3;
}
.post-title:hover { color: var(--link); text-decoration: none; }

.post-meta { color: var(--muted); font-size: 12px; margin-top: 4px; }

/* Post page */
.post-header {
    margin-bottom: 20px;
    padding-bottom: 12px;
    border-bottom: 1px solid var(--border);
}

.back-link {
    display: inline-block;
    color: var(--muted);
    text-decoration: none;
    font-size: 12px;
    margin-bottom: 8px;
}
.back-link:hover { color: var(--link); }

.post-header h1 { font-size: 22px; font-weight: 500; }

/* Comments */
.comments { display: flex; flex-direction: column; }

.comment {
    border-left: 2px solid var(--border);
    padding: 8px 0 4px 12px;
    margin: 4px 0;
}

.comment-author {
    color: var(--accent);
    font-size: 12px;
    font-weight: 700;
    margin-bottom: 4px;
    text-transform: uppercase;
    letter-spacing: 0.05em;
}

.comment-body { color: var(--text); line-height: 1.6; word-break: break-word; }

.replies { margin-top: 8px; }
"""


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
    body_html: Markup
    children: list["CommentNode"] = field(default_factory=list)


@dataclass
class Post:
    name: str
    display_name: str
    block_count: int


def _display_name(name: str) -> str:
    return name.replace("_", " ")


def _build_tree(blocks: list[_Block], translations: dict[str, str]) -> list[CommentNode]:
    nodes: dict[int, CommentNode] = {}
    children_map: dict[int | None, list[CommentNode]] = {}

    for b in blocks:
        translated = translations.get(b.text, b.text)
        node = CommentNode(
            block_id=b.block_id,
            speaker=b.speaker,
            body_html=Markup(text_to_html(translated)),
        )
        nodes[b.block_id] = node
        children_map.setdefault(b.parent_block_id, []).append(node)

    for parent_id, child_nodes in children_map.items():
        if parent_id is not None and parent_id in nodes:
            nodes[parent_id].children = child_nodes

    return children_map.get(None, [])


def generate(db_path: Path, out_dir: Path, lang: str) -> None:
    out_dir.mkdir(parents=True, exist_ok=True)

    with sqlite3.connect(db_path) as conn:
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

        translations: dict[str, str] = {
            key: value
            for key, value in conn.execute(
                "SELECT key, value FROM translations WHERE lang = ?", (lang,)
            )
        }

    env = Environment(
        loader=PackageLoader("nomai_web", "templates"),
        autoescape=True,
    )

    posts = [
        Post(
            name=name,
            display_name=_display_name(name),
            block_count=len(file_blocks[name]),
        )
        for name in file_names
    ]

    (out_dir / "style.css").write_text(_CSS, encoding="utf-8")

    index_tmpl = env.get_template("index.html")
    (out_dir / "index.html").write_text(
        index_tmpl.render(posts=posts, lang=lang, css_path="style.css", root_path="./"),
        encoding="utf-8",
    )
    print(f"  index.html ({len(posts)} posts)")

    post_tmpl = env.get_template("post.html")
    for name in file_names:
        tree = _build_tree(file_blocks[name], translations)
        post_dir = out_dir / name
        post_dir.mkdir(exist_ok=True)
        (post_dir / "index.html").write_text(
            post_tmpl.render(
                display_name=_display_name(name),
                tree=tree,
                lang=lang,
                css_path="../style.css",
                root_path="../",
            ),
            encoding="utf-8",
        )

    print(f"  {len(file_names)} post pages")
    print(f"\nDone → {out_dir}/")
