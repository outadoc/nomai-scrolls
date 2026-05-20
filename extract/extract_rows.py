#!/usr/bin/env python3
import argparse
import csv
import sys

from bs4 import BeautifulSoup


def main() -> None:
    parser = argparse.ArgumentParser(
        description=(
            "Parse an AssetRipper search results page and emit a CSV of text asset paths to stdout. "
            "Pipe the output into fetch_texts.sh to download the assets."
        ),
        formatter_class=argparse.ArgumentDefaultsHelpFormatter,
    )
    parser.add_argument(
        "html_file",
        nargs="?",
        default="Search_ textasset.html",
        metavar="FILE",
        help="AssetRipper search results HTML file",
    )
    args = parser.parse_args()

    with open(args.html_file, encoding="utf-8") as f:
        soup = BeautifulSoup(f, "html.parser")

    rows = soup.select("tbody#resultsTable tr")

    writer = csv.writer(sys.stdout)
    writer.writerow(["path_id", "class", "name", "name_href", "collection", "collection_href"])

    for row in rows:
        cells = row.find_all("td")
        path_id = cells[0].get_text(strip=True)
        cls = cells[1].get_text(strip=True)
        name_a = cells[2].find("a")
        name = name_a.get_text(strip=True)
        name_href = name_a["href"]
        coll_a = cells[3].find("a")
        collection = coll_a.get_text(strip=True)
        collection_href = coll_a["href"]
        writer.writerow([path_id, cls, name, name_href, collection, collection_href])


if __name__ == "__main__":
    main()
