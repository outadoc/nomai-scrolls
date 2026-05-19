#!/usr/bin/env python3
from bs4 import BeautifulSoup
import csv
import sys

html_file = "Search_ textasset.html"

with open(html_file, encoding="utf-8") as f:
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
