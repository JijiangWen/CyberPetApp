import os
import glob
from collections import Counter, deque
from PIL import Image, ImageDraw

# Directories
ART_DIR = r"C:\Users\wen.jijiang\.gemini\antigravity-ide\brain\5bcd1895-0ed7-458d-9f9e-ba57f8294659"
OUT_DIR = r"c:\Users\wen.jijiang\Desktop\blazor_test\CyberPetApp\wwwroot\assets\icons"
os.makedirs(OUT_DIR, exist_ok=True)

# Helper function to find a file by prefix in artifacts directory
def find_art_file(prefix):
    pattern = os.path.join(ART_DIR, f"{prefix}_*.png")
    files = glob.glob(pattern)
    if not files:
        # Check without underscore or other formats just in case
        pattern2 = os.path.join(ART_DIR, f"{prefix}*.png")
        files = glob.glob(pattern2)
    return files[0] if files else None

# Background stripping & padding function
def process_ai_icon(src_path, out_name):
    if not src_path or not os.path.exists(src_path):
        print(f"Warning: Source path {src_path} not found for {out_name}")
        return False
        
    img = Image.open(src_path).convert("RGBA")
    w, h = img.size
    px = img.load()
    
    # Strip background
    border = [(x, y) for x in range(w) for y in (0, h - 1)] + \
             [(x, y) for x in (0, w - 1) for y in range(1, h - 1)]
    counts = Counter(px[x, y][:3] for x, y in border)
    total = len(border)
    bg_colors = [c for c, n in counts.most_common(8) if n / total >= 0.01]
    
    TOL = 32
    def close(c, ref):
        return abs(c[0] - ref[0]) <= TOL and abs(c[1] - ref[1]) <= TOL and abs(c[2] - ref[2]) <= TOL
        
    def is_bg(x, y):
        c = px[x, y]
        return c[3] != 0 and any(close(c, ref) for ref in bg_colors)
        
    seen = bytearray(w * h)
    q = deque((x, y) for x, y in border if is_bg(x, y))
    for x, y in q:
        seen[y * w + x] = 1
        
    while q:
        x, y = q.popleft()
        px[x, y] = (0, 0, 0, 0)
        for nx, ny in ((x-1, y), (x+1, y), (x, y-1), (x, y+1)):
            if 0 <= nx < w and 0 <= ny < h and not seen[ny * w + nx] and is_bg(nx, ny):
                seen[ny * w + nx] = 1
                q.append((nx, ny))
                
    # Crop
    bbox = img.getbbox()
    if bbox:
        img = img.crop(bbox)
        
    # Scale to fit inside 28x28 (leaving 2px pad)
    cw, ch = img.size
    scale = min(28.0 / cw, 28.0 / ch)
    nw = int(cw * scale)
    nh = int(ch * scale)
    img_resized = img.resize((nw, nh), Image.Resampling.LANCZOS)
    
    # Pad to 32x32 transparent
    out_img = Image.new("RGBA", (32, 32), (0, 0, 0, 0))
    out_img.paste(img_resized, ((32 - nw) // 2, (32 - nh) // 2), img_resized)
    
    dest_path = os.path.join(OUT_DIR, f"{out_name}.png")
    out_img.save(dest_path)
    print(f"Processed AI icon: {out_name} -> {dest_path}")
    return True

# Programmatic Drawing Helpers
CELL = 64
OL = (18, 22, 34)          # dark outline
CY = (0, 229, 200)         # #00e5c8
CY_D = (0, 180, 158)
CY_L = (120, 255, 235)
PU = (167, 139, 250)       # #a78bfa
PU_D = (120, 90, 200)
PU_L = (200, 180, 255)
PK = (244, 114, 182)       # #f472b6
PK_D = (200, 70, 140)
PK_L = (255, 180, 210)
GD = (255, 200, 80)
GD_D = (220, 150, 40)
GD_L = (255, 230, 150)
WH = (255, 252, 245)
GR = (72, 200, 120)
GR_D = (40, 140, 80)
GR_L = (140, 240, 170)
RD = (240, 90, 90)
RD_D = (180, 50, 50)
RD_L = (255, 180, 180)
BL = (60, 100, 180)
BL_D = (35, 65, 130)
BL_L = (120, 170, 240)
SL = (140, 155, 175)
SL_D = (90, 105, 125)
SL_L = (200, 210, 225)
BR = (130, 90, 55)
BR_D = (90, 60, 35)
BR_L = (180, 130, 80)

def blank():
    return Image.new("RGBA", (CELL, CELL), (184, 184, 184, 255))

def _d(img):
    return ImageDraw.Draw(img)

def r(img, x, y, w, h, c, outline=True):
    d = _d(img)
    if outline and w > 0 and h > 0:
        d.rectangle([x - 1, y - 1, x + w, y + h], fill=OL)
    d.rectangle([x, y, x + w - 1, y + h - 1], fill=c)

def rr(img, rects, outline=True):
    for item in rects:
        x, y, w, h, c = item
        r(img, x, y, w, h, c, outline)

def draw_alchemy():
    img = blank()
    rr(img, [
        # Base glass bulb
        (20, 26, 24, 24, SL_L),
        # Potion liquid (purple) inside the base
        (22, 30, 20, 18, PU_D),
        (24, 34, 16, 12, PU),
        (26, 38, 12, 6, PU_L),
        # Glass neck
        (27, 16, 10, 12, SL_L),
        # Light vapor
        (29, 20, 6, 8, CY_L),
        # Cork
        (28, 12, 8, 5, BR),
        (30, 10, 4, 2, BR_D),
    ])
    # Glass highlights
    rr(img, [
        (22, 28, 3, 3, WH),
        (39, 28, 2, 4, WH),
        (29, 22, 2, 2, WH),
        (26, 32, 2, 2, WH),
        (36, 35, 2, 2, WH),
    ], outline=False)
    return img

def draw_milestone():
    img = blank()
    rr(img, [
        # Red ribbon background
        (22, 24, 20, 24, RD_D),
        (24, 22, 16, 26, RD),
        # Gold medal rim
        (20, 16, 24, 24, GD_D),
        (22, 18, 20, 20, GD),
        (24, 20, 16, 16, GD_L),
        # Cyan gem / star in center
        (29, 25, 6, 6, CY),
        (31, 27, 2, 2, CY_L),
    ])
    # Ribbon stripes
    rr(img, [
        (26, 22, 2, 24, RD_L),
        (36, 22, 2, 24, RD_L),
    ], outline=False)
    return img

# We can reuse the draw functions from draw_sprites.py directly by copying them here
def draw_can():
    img = blank()
    rr(img, [
        (20, 12, 16, 22, OL), (22, 14, 12, 18, SL_L),
        (22, 12, 12, 2, SL_D), (22, 32, 12, 2, SL_D),
        (22, 16, 12, 4, CY), (24, 22, 8, 6, PK_L),
        (26, 24, 4, 2, WH),
    ])
    return img

def draw_rod():
    img = blank()
    rr(img, [
        (8, 34, 24, 2, BR_D), (30, 12, 2, 22, BR),
        (32, 10, 6, 6, OL), (34, 12, 2, 10, CY),
        (28, 8, 4, 3, SL), (36, 14, 2, 2, CY_L),
    ])
    return img

def draw_pouch():
    img = blank()
    rr(img, [
        (18, 14, 20, 22, BR_D), (20, 12, 16, 6, BR),
        (22, 20, 12, 10, BR_L), (26, 24, 6, 4, GD),
        (24, 26, 3, 2, GD_L), (28, 14, 4, 2, CY),
    ])
    return img

def process_programmatic_icon(draw_fn, out_name):
    img = draw_fn()
    # Strip background BG (184, 184, 184) to transparent
    w, h = img.size
    px = img.load()
    for x in range(w):
        for y in range(h):
            if px[x, y][:3] == (184, 184, 184):
                px[x, y] = (0, 0, 0, 0)
                
    # Crop
    bbox = img.getbbox()
    if bbox:
        img = img.crop(bbox)
        
    # Scale to fit inside 28x28 (leaving 2px pad)
    cw, ch = img.size
    scale = min(28.0 / cw, 28.0 / ch)
    nw = int(cw * scale)
    nh = int(ch * scale)
    img_resized = img.resize((nw, nh), Image.Resampling.NEAREST) # Nearest neighbor keeps pixel art crisp
    
    # Pad to 32x32 transparent
    out_img = Image.new("RGBA", (32, 32), (0, 0, 0, 0))
    out_img.paste(img_resized, ((32 - nw) // 2, (32 - nh) // 2), img_resized)
    
    dest_path = os.path.join(OUT_DIR, f"{out_name}.png")
    out_img.save(dest_path)
    print(f"Processed Programmatic icon: {out_name} -> {dest_path}")

def main():
    print("=== Processing AI Icons from Artifacts ===")
    ai_icons = {
        "icon_house": "house",
        "icon_fish": "fish",
        "icon_hammer": "hammer",
        "icon_cat": "cat",
        "icon_shop": "shop"
    }
    for prefix, name in ai_icons.items():
        src_path = find_art_file(prefix)
        if src_path:
            process_ai_icon(src_path, name)
        else:
            print(f"Error: Could not find generated file for prefix {prefix}")
            
    print("\n=== Generating Programmatic Icons ===")
    prog_icons = {
        "rod": draw_rod,
        "food": draw_can,
        "bag": draw_pouch,
        "alchemy": draw_alchemy,
        "milestone": draw_milestone
    }
    for name, fn in prog_icons.items():
        process_programmatic_icon(fn, name)
        
    print("\nAll icons completed successfully!")

if __name__ == "__main__":
    main()
