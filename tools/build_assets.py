#!/usr/bin/env python3
"""Generate centered pixel-art sprites, strip backgrounds, composite sprite sheets.

Usage:
    python tools/build_assets.py

Pipeline per sprite:
  1. Draw on solid BG (#b8b8b8) — 64×64, 4–6px safe margin, visually centered
  2. python tools/strip_bg.py <file>  (flood-fill transparent)
  3. Composite into wwwroot/assets/*.png
"""
from __future__ import annotations

import subprocess
import sys
from pathlib import Path

from PIL import Image

from draw_sprites import BG, CATS, CELL, FISH, FURNITURE, ITEMS

ROOT = Path(__file__).resolve().parent.parent
SRC = ROOT / "tools" / "assets_src"
OUT = ROOT / "wwwroot" / "assets"
STRIP = ROOT / "tools" / "strip_bg.py"


def save_and_strip(path: Path, img: Image.Image) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    img.save(path)
    subprocess.run([sys.executable, str(STRIP), str(path)], check=True)


def build_sheet(name: str, sprites: list[Image.Image], cols: int) -> Image.Image:
    rows = (len(sprites) + cols - 1) // cols
    sheet = Image.new("RGBA", (cols * CELL, rows * CELL), (0, 0, 0, 0))
    for i, sp in enumerate(sprites):
        c, r = i % cols, i // cols
        sheet.paste(sp, (c * CELL, r * CELL), sp)
    return sheet


def process_group(subdir: str, items: list[tuple[str, callable]]) -> list[Image.Image]:
    sprites: list[Image.Image] = []
    for slug, fn in items:
        raw = fn()
        path = SRC / subdir / f"{slug}.png"
        save_and_strip(path, raw)
        sprites.append(Image.open(path).convert("RGBA"))
        print(f"  ok {subdir}/{slug}.png")
    return sprites


def main():
    print("=== CyberPet asset pipeline ===")
    print(f"CELL={CELL}px  palette=cyber-neon  strip_bg={STRIP.name}")

    furn = process_group("furniture", FURNITURE)
    items = process_group("items", ITEMS)
    fish = process_group("fish", FISH)
    cats = process_group("cat", CATS)

    sheets = {
        "furniture-set.png": build_sheet("furniture", furn, 6),
        "item-set.png": build_sheet("items", items, 3),
        "fish-set.png": build_sheet("fish", fish, 6),
        "cat-states.png": build_sheet("cat", cats, 3),
    }

    OUT.mkdir(parents=True, exist_ok=True)
    cat_out = OUT / "cat"
    cat_out.mkdir(parents=True, exist_ok=True)
    for slug, _ in CATS:
        src = SRC / "cat" / f"{slug}.png"
        dest = cat_out / f"{slug}.png"
        Image.open(src).convert("RGBA").save(dest)
        print(f"  cat -> {dest}")

    for fname, sheet in sheets.items():
        path = OUT / fname
        sheet.save(path)
        print(f"  sheet -> {path} ({sheet.size[0]}x{sheet.size[1]})")

    portrait = cats[0].resize((128, 128), Image.NEAREST)
    ppath = OUT / "cat-portrait.png"
    portrait.save(ppath)
    print(f"  portrait -> {ppath}")

    print("Done.")


if __name__ == "__main__":
    main()
