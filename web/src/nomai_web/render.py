import html
import re

_COLOR_MAP: dict[str, str] = {
    "black":     "#666",
    "grey":      "#666",
    "lightblue": "#87ceeb",
    "orange":    "#e8871a",
    "red":       "#ff5f5f",
}

# Inline markup tags found in Nomai dialogue (delivered inside CDATA sections).
# Alternatives are tried left-to-right; named groups identify which branch matched.
#   <color=name>…</color>  → <span style="color:…">
#   <i>…</i>               → <em>
#   <size=N>…</size>       → content only (game UI sizing, no web equivalent)
_MARKUP_RE = re.compile(
    r"<color=(?P<color>[^\n>]+)>(?P<color_body>.*?)</color>"
    r"|<i>(?P<italic_body>.*?)</i>"
    r"|<size=[^>]+>(?P<size_body>.*?)</size>",
    re.DOTALL,
)

# Strips any remaining tags not matched above (e.g. unclosed or unknown tags)
# so they never appear as raw HTML in the output.
_STRAY_TAG_RE = re.compile(r"<[^>]+>")


def _escape(text: str) -> str:
    return html.escape(text).replace("\n", "<br>")


def _process(text: str) -> str:
    parts: list[str] = []
    pos = 0
    for m in _MARKUP_RE.finditer(text):
        if m.start() > pos:
            parts.append(_escape(_STRAY_TAG_RE.sub("", text[pos:m.start()])))
        if m.group("color") is not None:
            color = _COLOR_MAP.get(m.group("color").lower(), m.group("color"))
            parts.append(f'<strong style="color:{color}">{_process(m.group("color_body"))}</strong>')
        elif m.group("italic_body") is not None:
            parts.append(f"<em>{_process(m.group('italic_body'))}</em>")
        else:
            parts.append(_process(m.group("size_body")))
        pos = m.end()
    if pos < len(text):
        parts.append(_escape(_STRAY_TAG_RE.sub("", text[pos:])))
    return "".join(parts)


def text_to_html(raw: str) -> str:
    raw = re.sub(r"\\+n", "\n", raw, flags=re.IGNORECASE)
    return _process(raw)
