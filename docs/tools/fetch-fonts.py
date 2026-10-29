"""
fetch-fonts.py — vendor the two web fonts used by the docs site.

Downloads only the latin / latin-ext / vietnamese subsets of the variable fonts
(Plus Jakarta Sans, JetBrains Mono) and rewrites a local fonts.css, so the wiki
renders identically with no network access.

Usage: python docs/tools/fetch-fonts.py
"""
import os
import re
import urllib.request

UA = ("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 "
      "(KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36")

DOCS = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
CSS_URL = ("https://fonts.googleapis.com/css2?"
           "family=Plus+Jakarta+Sans:wght@300..800"
           "&family=JetBrains+Mono:wght@400..600"
           "&display=swap")

KEEP = {"latin", "latin-ext", "vietnamese"}
FONT_DIR = os.path.join(DOCS, "vendor", "fonts")
OUT_CSS = os.path.join(DOCS, "vendor", "fonts.css")

BLOCK_RE = re.compile(r"/\*\s*([\w-]+)\s*\*/\s*(@font-face\s*\{.*?\})", re.S)
URL_RE = re.compile(r"url\((https://[^)]+\.woff2)\)")
FAMILY_RE = re.compile(r"font-family:\s*['\"]?([^;'\"]+?)['\"]?\s*;")
WEIGHT_RE = re.compile(r"font-weight:\s*([^;]+);")


def fetch(url):
    req = urllib.request.Request(url, headers={"User-Agent": UA})
    with urllib.request.urlopen(req, timeout=40) as response:
        return response.read()


def main():
    css = fetch(CSS_URL).decode("utf-8")
    blocks = BLOCK_RE.findall(css)
    os.makedirs(FONT_DIR, exist_ok=True)

    out, downloaded, skipped = [], 0, 0
    for subset, block in blocks:
        if subset not in KEEP:
            skipped += 1
            continue

        url = URL_RE.search(block)
        family = FAMILY_RE.search(block)
        weight = WEIGHT_RE.search(block)
        if not (url and family and weight):
            skipped += 1
            continue

        name = "{}-{}-{}.woff2".format(
            family.group(1).replace(" ", ""),
            weight.group(1).strip().replace(" ", ""),
            subset,
        )
        data = fetch(url.group(1))
        with open(os.path.join(FONT_DIR, name), "wb") as fh:
            fh.write(data)

        out.append("/* {} */\n{}".format(subset, block.replace(url.group(1), "fonts/" + name)))
        downloaded += 1

    with open(OUT_CSS, "w", encoding="utf-8", newline="\n") as fh:
        fh.write("\n".join(out) + "\n")

    total = sum(os.path.getsize(os.path.join(FONT_DIR, f)) for f in os.listdir(FONT_DIR))
    print("fonts downloaded: {} (skipped {} non-latin blocks)".format(downloaded, skipped))
    print("total size: {:.0f} KB".format(total / 1024))
    for f in sorted(os.listdir(FONT_DIR)):
        print("  ", f, os.path.getsize(os.path.join(FONT_DIR, f)))


if __name__ == "__main__":
    main()
