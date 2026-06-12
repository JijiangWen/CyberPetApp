import os
from collections import Counter, deque
from PIL import Image

src_path = r"C:\Users\wen.jijiang\.gemini\antigravity-ide\brain\5bcd1895-0ed7-458d-9f9e-ba57f8294659\icon_cat_1781253210772.png"
out_dir = r"c:\Users\wen.jijiang\Desktop\blazor_test\CyberPetApp\wwwroot\assets\icons"
os.makedirs(out_dir, exist_ok=True)

# Process cat icon
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

# Resize to 32x32
cw, ch = img.size
scale = min(28.0 / cw, 28.0 / ch)
nw = int(cw * scale)
nh = int(ch * scale)
img_resized = img.resize((nw, nh), Image.Resampling.LANCZOS)

# Pad to 32x32
out_img = Image.new("RGBA", (32, 32), (0, 0, 0, 0))
out_img.paste(img_resized, ((32 - nw) // 2, (32 - nh) // 2), img_resized)

out_path = os.path.join(out_dir, "cat.png")
out_img.save(out_path)
print(f"Cat icon saved to {out_path}")
