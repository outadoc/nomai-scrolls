import sqlite3


def init_db(conn: sqlite3.Connection) -> None:
    conn.execute("PRAGMA foreign_keys = ON")
    conn.executescript("""
        CREATE TABLE IF NOT EXISTS languages (
            code TEXT PRIMARY KEY
        );

        CREATE TABLE IF NOT EXISTS translations (
            lang  TEXT NOT NULL REFERENCES languages(code),
            key   TEXT NOT NULL,
            value TEXT NOT NULL,
            PRIMARY KEY (lang, key)
        );

        CREATE TABLE IF NOT EXISTS dialogue_files (
            file_id INTEGER PRIMARY KEY,
            name    TEXT NOT NULL UNIQUE
        );

        CREATE TABLE IF NOT EXISTS text_blocks (
            id                    INTEGER PRIMARY KEY AUTOINCREMENT,
            file_id               INTEGER NOT NULL REFERENCES dialogue_files(file_id),
            block_id              INTEGER NOT NULL,
            parent_block_id       INTEGER,
            text                  TEXT NOT NULL,
            default_font_override INTEGER NOT NULL DEFAULT 0,
            UNIQUE (file_id, block_id)
        );
    """)
    conn.commit()
