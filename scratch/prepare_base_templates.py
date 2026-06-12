import os
import sys
from collections import Counter, deque
from PIL import Image

sys.stdout.reconfigure(encoding='utf-8')

# Source paths
shrimp_src = r"C:\Users\wen.jijiang\.gemini\antigravity-ide\brain\5bcd1895-0ed7-458d-9f9e-ba57f8294659\gen_shrimp_1781252013582.png"
crab_src = r"C:\Users\wen.jijiang\.gemini\antigravity-ide\brain\5bcd1895-0ed7-458d-9f9e-ba57f8294659\gen_crab_1781252029452.png"
turtle_src = r"C:\Users\wen.jijiang\.gemini\antigravity-ide\brain\5bcd1895-0ed7-458d-9f9e-ba57f8294659\gen_turtle_1781252044587.png"
jellyfish_src = r"C:\Users\wen.jijiang\.gemini\antigravity-ide\brain\5bcd1895-0ed7-458d-9f9e-ba57f8294659\gen_jellyfish_1781252068175.png"
tadpole_src = r"C:\Users\wen.jijiang\.gemini\antigravity-ide\brain\5bcd1895-0ed7-458d-9f9e-ba57f8294659\gen_tadpole_1781252093255.png"
loach_src = r"C:\Users\wen.jijiang\.gemini\antigravity-ide\brain\5bcd1895-0ed7-458d-9f9e-ba57f8294659\gen_loach_1781252119725.png"
squid_src = r"C:\Users\wen.jijiang\.gemini\antigravity-ide\brain\5bcd1895-0ed7-458d-9f9e-ba57f8294659\gen_squid_1781252147354.png"
ray_src = r"C:\Users\wen.jijiang\.gemini\antigravity-ide\brain\5bcd1895-0ed7-458d-9f9e-ba57f8294659\gen_ray_1781252175624.png"
angler_src = r"C:\Users\wen.jijiang\.gemini\antigravity-ide\brain\5bcd1895-0ed7-458d-9f9e-ba57f8294659\gen_angler_1781252259237.png"

targets = {
    "shrimp_base.png": shrimp_src,
    "crab_base.png": crab_src,
    "turtle_base.png": turtle_src,
    "jellyfish_base.png": jellyfish_src,
    "tadpole_base.png": tadpole_src,
    "loach_base.png": loach_src,
    "squid_base.png": squid_src,
    "ray_base.png": ray_src,
    "angler_base.png": angler_src
}

out_dir = r"c:\Users\wen.jijiang\Desktop\blazor_test\CyberPetApp\tools\assets_src\fish"
os.makedirs(out_dir, exist_ok=True)

def process_gen_image(src_path):
    # 1. Load image
    img = Image.open(src_path).convert("RGBA")
    w, h = img.size
    px = img.load()
    
    # 2. Strip background (dominant colors on borders)
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
                
    # 3. Find bounding box of non-transparent pixels
    bbox = img.getbbox()
    if bbox:
        img = img.crop(bbox)
        
    # 4. Resize and pad to 192x256
    cw, ch = img.size
    scale = min(150.0 / cw, 140.0 / ch)
    nw = int(cw * scale)
    nh = int(ch * scale)
    img_resized = img.resize((nw, nh), Image.Resampling.LANCZOS)
    
    # Create new 192x256 transparent canvas
    out_img = Image.new("RGBA", (192, 256), (0, 0, 0, 0))
    px_pos = (192 - nw) // 2
    py_pos = (256 - nh) // 2
    out_img.paste(img_resized, (px_pos, py_pos), img_resized)
    
    return out_img

for name, src in targets.items():
    if not os.path.exists(src):
        print(f"Error: {src} does not exist!")
        continue
    try:
        processed = process_gen_image(src)
        out_path = os.path.join(out_dir, name)
        processed.save(out_path)
        print(f"Successfully processed {src} -> {out_path}")
    except Exception as e:
        print(f"Error processing {src}: {e}")

print("All base templates prepared.")
