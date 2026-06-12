#!/usr/bin/env python3
import os
import re
import colorsys
import hashlib
import random
from PIL import Image, ImageDraw

# Setup paths
ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SPOT_CATALOG_PATH = os.path.join(ROOT, "Models", "FishingSpotCatalog.Generated.cs")
OUT_DIR = os.path.join(ROOT, "wwwroot", "assets", "fish")
ORIGINAL_SET_PATH = os.path.join(ROOT, "wwwroot", "assets", "fish-set.png")

os.makedirs(OUT_DIR, exist_ok=True)

# Load the original high-quality fish sheet
print(f"Loading original sheet: {ORIGINAL_SET_PATH}")
original_sheet = Image.open(ORIGINAL_SET_PATH).convert("RGBA")

# 1. Parse all fish from FishingSpotCatalog.Generated.cs
print("Parsing fish templates from Spot Catalog...")
with open(SPOT_CATALOG_PATH, "r", encoding="utf-8") as f:
    content = f.read()

# Find: new FishTemplate("溪边小白条", 14, 9, 5, FishRarity.Common, ...
pattern = r'new FishTemplate\("([^"]+)",\s*\d+,\s*\d+,\s*\d+,\s*FishRarity\.(\w+)'
matches = re.findall(pattern, content)
unique_fish = sorted(list(set(matches)), key=lambda x: x[0])
print(f"Found {len(unique_fish)} unique fish species in code.")

def get_sprite_class(name, rarity_str):
    # ── 神话层 (Row3: fish-25~32) ──────────────────────────────
    if "神话" in name:
        if "镜湖神鲤" in name or "翠影鳗王" in name or "废弃鱼塘幻鳞" in name: return "fish-25"
        if "雾海古神" in name or "芦苇幽歌" in name: return "fish-26"
        if "引渠幻龙" in name or "深水海湾兽" in name: return "fish-26"
        if "极光霜龙" in name: return "fish-28"
        if "沉船亡魂" in name: return "fish-27"
        if "珊瑚心海" in name: return "fish-29"
        if "深渊巡礼" in name: return "fish-30"
        if "星潮巨兽" in name: return "fish-31"
        if "远海沧龙" in name or "沧龙" in name: return "fish-30"
        if "金鳞海皇" in name or "海皇" in name: return "fish-29"
        if "虚空钓主" in name: return "fish-30"
        if "终焉鲸歌" in name: return "fish-31"
        return "fish-32"

    # ── 荧光/奇幻层 (Row2: fish-17~24) ────────────────────────
    if "星潮" in name or "星尘巡游" in name: return "fish-24"
    if "潮汐" in name:
        return "fish-24" if rarity_str in ("Epic", "Legendary") else "fish-23"
    if "虚空" in name or "终焉" in name or "裂隙" in name:
        return "fish-32" if rarity_str == "Legendary" else "fish-21" if rarity_str == "Epic" else "fish-20"
    if "回廊" in name or "凝胶" in name or "安康" in name:
        return "fish-30" if rarity_str == "Legendary" else "fish-20" if rarity_str == "Epic" else "fish-19"
    if "墓场" in name or "亡魂" in name or "幽灵" in name or "锈" in name:
        return "fish-19" if rarity_str in ("Epic", "Legendary") else "fish-18"
    if "冰" in name or "霜" in name or "寒" in name or "雪" in name or "晶" in name or "冻" in name or "极光" in name or "归潮" in name:
        return "fish-28" if rarity_str == "Legendary" else "fish-18" if rarity_str == "Epic" else "fish-17"
    if "夜光" in name or "荧光" in name or "彩光" in name or "流光" in name or "霁光" in name or "磷光" in name:
        return "fish-22" if rarity_str in ("Epic", "Legendary") else "fish-17"
    if "珊瑚" in name or "礁" in name or "小丑" in name:
        return "fish-29" if rarity_str == "Legendary" else "fish-23" if rarity_str == "Epic" else "fish-22"
    if "裂谷" in name or "暗涌" in name or "矿渣" in name:
        return "fish-21" if rarity_str in ("Epic", "Legendary") else "fish-20"
    if "幽蓝" in name or "深渊" in name or "暗流" in name or "深湾" in name:
        return "fish-30" if rarity_str == "Legendary" else "fish-20" if rarity_str == "Epic" else "fish-19"
    if "雾海" in name:
        return "fish-24" if rarity_str == "Legendary" else "fish-21" if rarity_str == "Epic" else "fish-17"

    # ── 海洋中级层 (Row1: fish-09~16) ─────────────────────────
    if "鳗" in name:
        if "电" in name or "电鳗" in name: return "fish-21"
        if "银" in name or "跨域" in name: return "fish-06"
        return "fish-09"
    if "鱿" in name or "乌贼" in name:
        return "fish-22" if rarity_str in ("Epic", "Legendary") else "fish-10"
    if "鳐" in name or "蝠鲼" in name:
        return "fish-24" if rarity_str in ("Epic", "Legendary") else "fish-11"
    if "旗鱼" in name or "马鲛" in name or "金枪" in name:
        return "fish-23" if rarity_str in ("Epic", "Legendary") else "fish-12"
    if "石斑" in name: return "fish-13"
    if "小丑鱼" in name: return "fish-14"
    if "鲨" in name:
        return "fish-29" if rarity_str == "Legendary" else "fish-15"
    if "锦鲤" in name or "金鳍" in name or "锦鳞" in name:
        return "fish-25" if rarity_str == "Legendary" else "fish-16"
    if "礁湾" in name or "外海" in name or "远礁" in name: return "fish-13"

    # ── 淡水普通层 (Row0: fish-01~08) ─────────────────────────
    if "鲫" in name: return "fish-01"
    if "鲤" in name and "锦鲤" not in name and "传说" not in name: return "fish-01"
    if "鲈" in name: return "fish-02"
    if "虾" in name or "蟹" in name or "浮游" in name or "沙蚕" in name or "蝌蚪" in name: return "fish-03"
    if "泥鳅" in name or "鱥" in name: return "fish-04"
    if "鳟" in name or "鲑" in name: return "fish-05"
    if "鳕" in name or "鲢" in name or "沙丁" in name or ("银" in name and rarity_str == "Common"): return "fish-06"
    if "鲶" in name or "龟" in name: return "fish-07"
    if "金" in name: return "fish-08"

    # fallback by rarity
    if rarity_str == "Common": return "fish-01"
    if rarity_str == "Rare": return "fish-05"
    if rarity_str == "Epic": return "fish-17"
    if rarity_str == "Legendary": return "fish-24"
    return "fish-01"

def get_base_sprite(name, rarity_str):
    # Check special categories first and map to our high-quality generated shapes
    if "蟹" in name:
        return Image.open(os.path.join(ROOT, "tools", "assets_src", "fish", "crab_base.png")).convert("RGBA")
    if "龟" in name:
        return Image.open(os.path.join(ROOT, "tools", "assets_src", "fish", "turtle_base.png")).convert("RGBA")
    if "水母" in name:
        return Image.open(os.path.join(ROOT, "tools", "assets_src", "fish", "jellyfish_base.png")).convert("RGBA")
    if "蝌蚪" in name:
        return Image.open(os.path.join(ROOT, "tools", "assets_src", "fish", "tadpole_base.png")).convert("RGBA")
    if "虾" in name or "浮游" in name or "磷虾" in name or "沙蚕" in name:
        return Image.open(os.path.join(ROOT, "tools", "assets_src", "fish", "shrimp_base.png")).convert("RGBA")
    if "泥鳅" in name or "鱥" in name or "鳗" in name:
        return Image.open(os.path.join(ROOT, "tools", "assets_src", "fish", "loach_base.png")).convert("RGBA")
    if "鱿" in name or "乌贼" in name or "墨鱼" in name or "章鱼" in name or "八爪" in name:
        return Image.open(os.path.join(ROOT, "tools", "assets_src", "fish", "squid_base.png")).convert("RGBA")
    if "鳐" in name or "蝠鲼" in name:
        return Image.open(os.path.join(ROOT, "tools", "assets_src", "fish", "ray_base.png")).convert("RGBA")
    if "安康" in name or "琵琶" in name:
        return Image.open(os.path.join(ROOT, "tools", "assets_src", "fish", "angler_base.png")).convert("RGBA")
        
    # Standard fish - crop from the original fish-set.png
    sprite_class = get_sprite_class(name, rarity_str)
    slot_idx = int(sprite_class.split('-')[1]) - 1
    col = slot_idx % 8
    row = slot_idx // 8
    
    cell_w, cell_h = 192, 256
    box = (col * cell_w, row * cell_h, (col + 1) * cell_w, (row + 1) * cell_h)
    return original_sheet.crop(box)

def sanitize_filename(name):
    return name.replace("\"", "").replace("“", "").replace("”", "").replace(":", "").replace("*", "").replace("?", "").replace("<", "").replace(">", "").replace("|", "")

# 3. Procedural HSV adjustments
def get_hsv_adjustment(name, rarity_str):
    h_shift = 0.0
    s_mult = 1.0
    v_mult = 1.0
    a_mult = 1.0
    has_color = False
    
    if any(k in name for k in ("红", "赤", "朱", "烈焰", "火", "罗非")):
        h_shift = 0.0 # Red
        has_color = True
    elif any(k in name for k in ("黄", "金", "锦", "琥珀", "橘", "姑", "黄骨")):
        h_shift = 0.12 # Gold/Yellow/Orange
        has_color = True
    elif any(k in name for k in ("绿", "青", "草", "苔", "柳", "竹", "青壳")):
        h_shift = 0.33 # Green
        has_color = True
    elif any(k in name for k in ("蓝", "冰", "溪", "海", "水", "潮", "霜", "极", "霓", "霁", "白化")):
        h_shift = 0.58 # Blue/Cyan
        has_color = True
    elif any(k in name for k in ("紫", "霓虹", "魔鬼", "幻", "斑斓", "霓", "鲵")):
        h_shift = 0.78 # Purple/Magenta
        has_color = True
        
    if not has_color:
        h_hash = int(hashlib.md5(name.encode('utf-8')).hexdigest(), 16)
        h_shift = (h_hash % 360) / 360.0
        s_mult = 0.75 + ((h_hash >> 8) % 60) / 100.0  # 0.75 to 1.35
        v_mult = 0.8 + ((h_hash >> 16) % 35) / 100.0  # 0.8 to 1.15

    if any(k in name for k in ("黑", "暗", "阴", "深渊", "回廊", "火山", "泥", "沼", "斑", "岩缝", "礁影")):
        v_mult *= 0.55
        s_mult *= 1.2
    if any(k in name for k in ("白", "银", "雪", "霜", "盲", "白化")):
        s_mult *= 0.15
        v_mult *= 1.35
    if any(k in name for k in ("透明", "玻璃", "介虫", "发光")):
        s_mult *= 0.2
        v_mult *= 1.1
        a_mult *= 0.65
        
    if rarity_str == "Legendary":
        s_mult *= 1.35
        v_mult *= 1.25
    elif rarity_str == "Epic":
        s_mult *= 1.2
        v_mult *= 1.15
        
    return h_shift, s_mult, v_mult, a_mult

def color_shift_image(img, h_shift, s_mult, v_mult, a_mult):
    img = img.convert("RGBA")
    pixels = img.load()
    w, h = img.size
    
    for y in range(h):
        for x in range(w):
            r, g, b, a = pixels[x, y]
            if a == 0:
                continue
            
            brightness = (r + g + b) / 3.0
            if brightness < 45:
                continue
                
            if r > 240 and g > 240 and b > 240:
                continue
                
            fh, fs, fv = colorsys.rgb_to_hsv(r/255.0, g/255.0, b/255.0)
            fh = h_shift
            fs = min(1.0, fs * s_mult)
            fv = min(1.0, fv * v_mult)
            
            nr, ng, nb = colorsys.hsv_to_rgb(fh, fs, fv)
            na = min(255, int(a * a_mult))
            pixels[x, y] = (int(nr*255), int(ng*255), int(nb*255), na)
            
    return img

# 4. Procedural texture drawings
def apply_patterns(img, name, rarity_str):
    img = img.convert("RGBA")
    w, h = img.size
    pixels = img.load()
    
    body_pixels = []
    outline_pixels = []
    top_contour = {}
    
    for y in range(h):
        for x in range(w):
            r, g, b, a = pixels[x, y]
            if a > 0:
                brightness = (r + g + b) / 3.0
                if brightness < 45:
                    outline_pixels.append((x, y))
                else:
                    body_pixels.append((x, y))
                    if x not in top_contour or y < top_contour[x]:
                        top_contour[x] = y
                        
    if not body_pixels:
        return img
        
    name_hash = int(hashlib.md5(name.encode('utf-8')).hexdigest(), 16)
    random.seed(name_hash)
    draw = ImageDraw.Draw(img)
    
    # 1. Spikes / Thorns (e.g. for "棘", "刺", "骨", "鳄", "虎", "岩", "岩缝", "礁")
    if any(k in name for k in ("棘", "刺", "骨", "鳄", "虎", "岩", "礁")):
        xs = sorted(list(top_contour.keys()))
        spike_count = random.randint(3, 6)
        spike_xs = random.sample(xs[5:-5], min(len(xs) - 10, spike_count)) if len(xs) > 10 else []
        for sx in spike_xs:
            sy = top_contour[sx]
            draw.line([(sx, sy), (sx, sy - 3)], fill=(18, 22, 34, 255), width=1)
            draw.point((sx, sy - 4), fill=(244, 114, 182, 255))
            
    # 2. Spots (e.g. for "斑", "星", "花", "锈", "沙丁", "斑斓")
    if any(k in name for k in ("斑", "星", "花", "锈", "沙丁", "斑斓")):
        spot_color = (255, 200, 80, 255) # gold
        if "黑" in name or "暗" in name:
            spot_color = (18, 22, 34, 255)
        elif "红" in name or "赤" in name:
            spot_color = (240, 90, 90, 255)
        elif "锈" in name:
            spot_color = (130, 90, 55, 255)
            
        num_spots = random.randint(5, 12)
        for _ in range(num_spots):
            px, py = random.choice(body_pixels)
            for dx in (0, 1):
                for dy in (0, 1):
                    if 0 <= px+dx < w and 0 <= py+dy < h:
                        pa = pixels[px+dx, py+dy][3]
                        if pa > 0:
                            pixels[px+dx, py+dy] = spot_color
                            
    # 3. Stripes / Waves (e.g. for "纹", "带", "条", "剪", "箭", "波")
    if any(k in name for k in ("纹", "带", "条", "剪", "箭", "波")):
        stripe_color = (0, 229, 200, 255) # cyan
        if "风" in name or "白" in name or "银" in name:
            stripe_color = (255, 252, 245, 255)
        elif "深海" in name or "暗" in name or "黑" in name:
            stripe_color = (167, 139, 250, 255)
            
        min_x = min(p[0] for p in body_pixels)
        max_x = max(p[0] for p in body_pixels)
        
        num_stripes = random.randint(2, 3)
        for i in range(num_stripes):
            sx = min_x + (max_x - min_x) // (num_stripes + 1) * (i + 1)
            for offset in range(-6, 7):
                lx = sx + offset
                ly = 24 + offset
                if 0 <= lx < w and 0 <= ly < h:
                    if pixels[lx, ly][3] > 0 and (lx, ly) not in outline_pixels:
                        pixels[lx, ly] = stripe_color
 
    # 4. Glow / Bioluminescence (e.g. for "光", "荧", "亮", "霓虹", "霓", "夜", "发光")
    if any(k in name for k in ("光", "荧", "亮", "霓", "夜", "发光")):
        glow_color = (120, 255, 235, 255)
        if "红" in name or "粉" in name:
            glow_color = (255, 180, 210, 255)
        elif "金" in name or "黄" in name:
            glow_color = (255, 230, 150, 255)
            
        min_x = min(p[0] for p in body_pixels)
        max_x = max(p[0] for p in body_pixels)
        
        for lx in range(min_x + 5, max_x - 5):
            for ly in (24, 25, 26, 23, 27):
                if 0 <= lx < w and 0 <= ly < h:
                    if pixels[lx, ly][3] > 0 and (lx, ly) not in outline_pixels:
                        pixels[lx, ly] = glow_color
                        break
 
    # 5. Glowing aura shadow outlines for Mythical / Mutant fish
    if "神话" in name or "异变" in name or "塘主" in name or "仙" in name:
        aura_color = (255, 200, 80, 100) # gold aura
        if "异变" in name:
            aura_color = (167, 139, 250, 120) # purple aura
        elif "冰" in name or "雪" in name or "霜" in name:
            aura_color = (180, 230, 255, 120) # ice aura
            
        aura_img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
        apix = aura_img.load()
        
        for y in range(h):
            for x in range(w):
                if pixels[x, y][3] > 0:
                    for dx in range(-2, 3):
                        for dy in range(-2, 3):
                            nx, ny = x + dx, y + dy
                            if 0 <= nx < w and 0 <= ny < h:
                                if pixels[nx, ny][3] < 150:
                                    dist = (dx*dx + dy*dy)**0.5
                                    alpha = int(aura_color[3] * (1.0 - dist/3.0))
                                    if alpha > apix[nx, ny][3]:
                                        apix[nx, ny] = (aura_color[0], aura_color[1], aura_color[2], alpha)
                                        
        aura_img.paste(img, (0, 0), img)
        img = aura_img
        
    return img

# 5. Process and generate
print("Generating individual species-accurate fish assets...")
generated_count = 0
for name, rarity in unique_fish:
    # Load correct base sprite shape (drawn programmatically or from tools/assets_src/fish)
    sprite = get_base_sprite(name, rarity)
    
    # Process color shifts
    h_shift, s_mult, v_mult, a_mult = get_hsv_adjustment(name, rarity)
    processed = color_shift_image(sprite, h_shift, s_mult, v_mult, a_mult)
    
    # Apply custom textures
    patterned = apply_patterns(processed, name, rarity)
    
    # Save
    safe_name = sanitize_filename(name)
    out_path = os.path.join(OUT_DIR, f"{safe_name}.png")
    patterned.save(out_path)
    generated_count += 1
    
print(f"Done! Generated {generated_count} species-accurate fish image assets under {OUT_DIR}.")
