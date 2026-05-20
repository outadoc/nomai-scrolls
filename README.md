# nomai-scrolls

A set of tools for browsing Nomai dialogue trees, as seen in Outer Wilds.

I wanted to visualize them as Reddit-like comment threads, as this is a model that makes sense to me and seems to match the way they write.

**[Browse the site](https://outadoc.github.io/nomai-scrolls/)**

## How it works

Raw XML assets (exported via [AssetRipper](https://github.com/AssetRipper/AssetRipper)) are parsed into an SQLite database. From there, the dialogue can be explored through a terminal UI or browsed as a static website.

## Tools

| Directory | Purpose |
|-----------|---------|
| `extract/` | Scrapes asset IDs and fetches raw XML files from a running AssetRipper server |
| `import/`  | Parses XML files into `nomai.db` (SQLite) |
| `app/`     | Terminal UI built with [Textual](https://github.com/Textualize/textual) |
| `web/`     | Static site generator; output is deployed to GitHub Pages |

## Usage

```bash
# Build the database from XML sources
cd import && pip install -e . && nomai-import --db ../nomai.db

# Browse in the terminal
cd app && pip install -e . && nomai-db-tui --db ../nomai.db

# Generate and preview the website locally
cd web && pip install -e . && nomai-web serve --db ../nomai.db
```

## Disclosure

The dialogue data belongs to [Mobius Digital](https://mobiusdigitalgames.com). 
This project is an unofficial fan tool and is not affiliated with or endorsed by the developers.

Most of this codebase was written with the assistance of [Claude](https://claude.ai).
