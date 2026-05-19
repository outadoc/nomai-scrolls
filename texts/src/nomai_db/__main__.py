import argparse
import sqlite3
from pathlib import Path

from nomai_db import parse_translations, parse_xml, schema


def main() -> None:
    parser = argparse.ArgumentParser(description="Parse Nomai XML files into an SQLite database")
    parser.add_argument("--db", default="nomai.db", help="Output database path (default: nomai.db)")
    parser.add_argument("--xml", default="xml", help="Directory containing NomaiObject XML files (default: xml)")
    parser.add_argument("--translations", default="translations", help="Directory containing translation XML files (default: translations)")
    args = parser.parse_args()

    db_path = Path(args.db)
    xml_dir = Path(args.xml)
    translations_dir = Path(args.translations)

    print(f"Database:     {db_path}")
    print(f"XML dir:      {xml_dir}")
    print(f"Translations: {translations_dir}")
    print()

    conn = sqlite3.connect(db_path)
    try:
        print("Initializing schema...")
        schema.init_db(conn)

        print("Loading translations...")
        n_translations = parse_translations.load_all(conn, translations_dir)
        print(f"  {n_translations} translation entries loaded")

        print("Loading dialogue XML files...")
        n_blocks = parse_xml.load_all(conn, xml_dir)
        print(f"  {n_blocks} text blocks loaded")

        print()
        print("Done.")
    finally:
        conn.close()


if __name__ == "__main__":
    main()
