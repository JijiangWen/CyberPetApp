"""Remove fake checkerboard/striped backgrounds baked into AI-generated PNGs.

Strategy: sample dominant colors along the image border (assumed background),
then flood-fill from every border pixel, clearing any pixel whose color is
close to one of those background colors. Sprites are protected by their dark
outlines, and enclosed light areas (eyes, chest fur) are never reached.
"""
import sys
from collections import Counter, deque
from PIL import Image

TOL = 28          # per-channel tolerance when matching a background color
BORDER_MIN = 0.02 # border color must cover >=2% of border pixels to count as bg


def close(c, ref, tol=TOL):
    return abs(c[0] - ref[0]) <= tol and abs(c[1] - ref[1]) <= tol and abs(c[2] - ref[2]) <= tol


def strip(path):
    img = Image.open(path).convert("RGBA")
    w, h = img.size
    px = img.load()

    border = [(x, y) for x in range(w) for y in (0, h - 1)] + \
             [(x, y) for x in (0, w - 1) for y in range(1, h - 1)]
    counts = Counter(px[x, y][:3] for x, y in border)
    total = len(border)
    bg_colors = [c for c, n in counts.most_common(8) if n / total >= BORDER_MIN]
    if not bg_colors:
        print(f"  {path}: no dominant border color, skipped")
        return

    def is_bg(x, y):
        c = px[x, y]
        return c[3] != 0 and any(close(c, ref) for ref in bg_colors)

    seen = bytearray(w * h)
    q = deque((x, y) for x, y in border if is_bg(x, y))
    for x, y in q:
        seen[y * w + x] = 1
    cleared = 0
    while q:
        x, y = q.popleft()
        px[x, y] = (0, 0, 0, 0)
        cleared += 1
        for nx, ny in ((x-1, y), (x+1, y), (x, y-1), (x, y+1)):
            if 0 <= nx < w and 0 <= ny < h and not seen[ny * w + nx] and is_bg(nx, ny):
                seen[ny * w + nx] = 1
                q.append((nx, ny))

    img.save(path)
    print(f"  {path}: bg colors {bg_colors[:4]}..., cleared {cleared}/{w*h} px")


if __name__ == "__main__":
    for p in sys.argv[1:]:
        strip(p)
