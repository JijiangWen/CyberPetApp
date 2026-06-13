import os
import hashlib
import random
import math
from PIL import Image, ImageDraw, ImageFilter

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
DEST_DIR = os.path.join(ROOT, "wwwroot", "assets", "gear")
os.makedirs(DEST_DIR, exist_ok=True)

def draw_rod_base():
    img = Image.new('RGBA', (128, 128), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    # Pole (diagonal from bottom-left to top-right)
    draw.line([(20, 108), (110, 18)], fill=(150, 150, 150, 255), width=6)
    # Handle
    draw.line([(10, 118), (30, 98)], fill=(100, 80, 50, 255), width=12)
    # Guides
    draw.ellipse([(60, 60), (68, 68)], outline=(200, 200, 200, 255), width=2)
    draw.ellipse([(85, 35), (93, 43)], outline=(200, 200, 200, 255), width=2)
    draw.ellipse([(110, 10), (118, 18)], outline=(200, 200, 200, 255), width=2)
    return img

def draw_reel_base():
    img = Image.new('RGBA', (128, 128), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    # Spool
    draw.ellipse([(30, 30), (98, 98)], fill=(120, 120, 120, 255))
    draw.ellipse([(45, 45), (83, 83)], fill=(80, 80, 80, 255))
    draw.ellipse([(59, 59), (69, 69)], fill=(200, 200, 200, 255))
    # Stand
    draw.rectangle([(60, 15), (68, 30)], fill=(150, 150, 150, 255))
    draw.rectangle([(40, 10), (88, 15)], fill=(150, 150, 150, 255))
    # Crank handle
    draw.line([(64, 64), (100, 80)], fill=(180, 180, 180, 255), width=6)
    draw.ellipse([(95, 75), (105, 85)], fill=(50, 50, 50, 255))
    return img

def draw_line_base():
    img = Image.new('RGBA', (128, 128), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    # Spool body
    draw.rectangle([(40, 30), (88, 98)], fill=(200, 200, 200, 255)) # string
    # Top and bottom flanges
    draw.ellipse([(30, 20), (98, 40)], fill=(50, 50, 50, 255))
    draw.ellipse([(30, 88), (98, 108)], fill=(50, 50, 50, 255))
    # Line unwinding
    draw.line([(88, 64), (110, 64), (110, 110)], fill=(220, 220, 220, 255), width=2)
    return img

def draw_lure_base():
    img = Image.new('RGBA', (128, 128), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    # Fish body
    draw.ellipse([(30, 50), (90, 78)], fill=(100, 200, 100, 255))
    # Tail
    draw.polygon([(35, 64), (15, 45), (15, 83)], fill=(100, 200, 100, 255))
    # Eye
    draw.ellipse([(75, 58), (81, 64)], fill=(255, 255, 255, 255))
    draw.ellipse([(78, 60), (80, 62)], fill=(0, 0, 0, 255))
    # Hooks
    draw.arc([(40, 78), (60, 98)], start=0, end=180, fill=(150, 150, 150, 255), width=3)
    draw.line([(50, 78), (50, 88)], fill=(150, 150, 150, 255), width=3)
    return img

def get_tier_color(tier):
    tier = int(tier)
    if tier == 1: return (141, 85, 36)     # T1: Bronze Wood
    if tier == 2: return (176, 125, 92)    # T2: Copper Bronze
    if tier == 3: return (144, 164, 174)   # T3: Steel/Iron
    if tier == 4: return (79, 154, 235)    # T4: Blue Steel
    if tier == 5: return (16, 185, 129)    # T5: Emerald / Jade Green
    if tier == 6: return (245, 158, 11)    # T6: Amber Gold
    if tier == 7: return (239, 68, 68)     # T7: Molten Red
    if tier == 8: return (6, 182, 212)     # T8: Diamond Cyan (glowing)
    if tier == 9: return (139, 92, 246)    # T9: Void Purple (glowing)
    return (236, 72, 153)                  # T10: Cosmic Pink (glowing)

def process_gear(img, name, tier):
    name_hash = int(hashlib.md5(name.encode('utf-8')).hexdigest(), 16)
    random.seed(name_hash)
    
    # Random deformation
    w, h = img.size
    scale_x = random.uniform(0.85, 1.15)
    scale_y = random.uniform(0.85, 1.15)
    new_w = int(w * scale_x)
    new_h = int(h * scale_y)
    img = img.resize((new_w, new_h), Image.Resampling.BICUBIC)
    
    # Tint based on tier
    target_color = get_tier_color(tier)
    pixels = img.load()
    for y in range(img.size[1]):
        for x in range(img.size[0]):
            r, g, b, a = pixels[x, y]
            if a > 0:
                # Blend with tier color
                intensity = (r + g + b) / (3.0 * 255.0)
                # Keep some original brightness, but apply tint
                nr = int(target_color[0] * intensity * 1.5)
                ng = int(target_color[1] * intensity * 1.5)
                nb = int(target_color[2] * intensity * 1.5)
                nr = min(255, max(0, nr))
                ng = min(255, max(0, ng))
                nb = min(255, max(0, nb))
                pixels[x, y] = (nr, ng, nb, a)
                
    # Add glow for T5+
    tier = int(tier)
    if tier >= 5:
        glow_color = target_color
        glow = img.filter(ImageFilter.GaussianBlur(radius=tier))
        out = Image.new('RGBA', img.size, (0,0,0,0))
        # Add glow layer
        out.paste(glow_color + (255,), (0,0), mask=glow.split()[3])
        # Add original on top
        out.paste(img, (0,0), mask=img.split()[3])
        img = out

    img.thumbnail((64, 64), Image.Resampling.LANCZOS)
    return img

def main():
    import re
    # Read catalog
    with open(os.path.join(ROOT, 'Models', 'GearProgressionCatalog.cs'), 'r', encoding='utf-8') as f:
        text1 = f.read()
    with open(os.path.join(ROOT, 'Models', 'GearProgressionCatalog.T6T10.cs'), 'r', encoding='utf-8') as f:
        text2 = f.read()
        
    def extract_from_list(text, list_name):
        pattern = rf'(?:public|internal) static readonly.*? {list_name}\s*=\s*\[(.*?)\];'
        match = re.search(pattern, text, re.DOTALL)
        if not match: return []
        block = match.group(1)
        return re.findall(r'new\(\s*\x22([^\x22]+)\x22.*?GearTier\.T(\d+)', block, re.DOTALL)

    rods = extract_from_list(text1, 'Rods') + extract_from_list(text2, 'ExtraRods')
    reels = extract_from_list(text1, 'Reels') + extract_from_list(text2, 'ExtraReels')
    lines = extract_from_list(text1, 'Lines') + extract_from_list(text2, 'ExtraLines')
    lures = extract_from_list(text1, 'Lures') + extract_from_list(text2, 'ExtraLures')
    
    def sanitize(n):
        return n.replace("\"", "").replace("“", "").replace("”", "").replace(":", "").replace("*", "").replace("?", "").replace("<", "").replace(">", "").replace("|", "")
        
    for name, t in rods:
        img = process_gear(draw_rod_base(), name, t)
        img.save(os.path.join(DEST_DIR, f'rod_{sanitize(name)}.png'))
        
    for name, t in reels:
        img = process_gear(draw_reel_base(), name, t)
        img.save(os.path.join(DEST_DIR, f'reel_{sanitize(name)}.png'))
        
    for name, t in lines:
        img = process_gear(draw_line_base(), name, t)
        img.save(os.path.join(DEST_DIR, f'line_{sanitize(name)}.png'))
        
    for name, t in lures:
        img = process_gear(draw_lure_base(), name, t)
        img.save(os.path.join(DEST_DIR, f'lure_{sanitize(name)}.png'))

    print(f"Generated {len(rods)} rods, {len(reels)} reels, {len(lines)} lines, {len(lures)} lures.")

if __name__ == '__main__':
    main()
