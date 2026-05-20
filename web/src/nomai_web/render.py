import html
import re

_COLOR_RE = re.compile(r"<color=([^\n>]+)>(.*?)</color>", re.DOTALL)
_COLOR_MAP: dict[str, str] = {
    "black":     "#666",
    "grey":      "#666",
    "lightblue": "#87ceeb",
    "orange":    "#e8871a",
    "red":       "#ff5f5f",
}


def _escape(text: str) -> str:
    return html.escape(text).replace("\n", "<br>")


def text_to_html(raw: str) -> str:
    raw = re.sub(r"\\+n", "\n", raw, flags=re.IGNORECASE)
    parts: list[str] = []
    pos = 0
    for m in _COLOR_RE.finditer(raw):
        if m.start() > pos:
            parts.append(_escape(raw[pos:m.start()]))
        color = _COLOR_MAP.get(m.group(1).lower(), m.group(1))
        parts.append(f'<span style="color:{color}">{_escape(m.group(2))}</span>')
        pos = m.end()
    if pos < len(raw):
        parts.append(_escape(raw[pos:]))
    return "".join(parts)
