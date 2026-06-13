import os
from PIL import Image, ImageEnhance
import colorsys

def hue_shift_image(img, shift_amount, saturation_boost=1.0, value_boost=1.0):
    """
    Shifts the hue of an RGBA image, adjusting saturation and value.
    shift_amount: 0.0 to 1.0
    """
    img = img.convert('RGBA')
    pixels = img.load()
    
    for i in range(img.width):
        for j in range(img.height):
            r, g, b, a = pixels[i, j]
            if a == 0:
                continue
                
            # Convert RGB (0-255) to HSV (0.0-1.0)
            h, s, v = colorsys.rgb_to_hsv(r/255.0, g/255.0, b/255.0)
            
            # Skip very low saturation (grays/whites/blacks) if we don't want to colorize them
            # But let's colorize everything slightly for a cooler effect.
            
            h = (h + shift_amount) % 1.0
            s = min(1.0, s * saturation_boost)
            v = min(1.0, v * value_boost)
            
            # Convert back to RGB
            nr, ng, nb = colorsys.hsv_to_rgb(h, s, v)
            pixels[i, j] = (int(nr * 255), int(ng * 255), int(nb * 255), a)
            
    return img

def apply_tint(img, tint_color, strength=0.5):
    img = img.convert('RGBA')
    pixels = img.load()
    tr, tg, tb = tint_color
    
    for i in range(img.width):
        for j in range(img.height):
            r, g, b, a = pixels[i, j]
            if a == 0:
                continue
            nr = int(r * (1 - strength) + tr * strength)
            ng = int(g * (1 - strength) + tg * strength)
            nb = int(b * (1 - strength) + tb * strength)
            pixels[i, j] = (nr, ng, nb, a)
    return img

base_dir = r"d:\Dev\CyberPetApp\wwwroot\assets\cat"
out_dir = r"d:\Dev\CyberPetApp\wwwroot\assets\cat_skins"

skins = {
    "pili": {"hue": 0.5, "sat": 1.5, "val": 1.2, "tint": (0, 255, 255), "tint_strength": 0.2}, # Cyan/Blue Lightning
    "void": {"hue": 0.8, "sat": 0.5, "val": 0.6, "tint": (128, 0, 128), "tint_strength": 0.5}, # Dark Purple
    "sakura": {"hue": 0.85, "sat": 1.2, "val": 1.1, "tint": (255, 182, 193), "tint_strength": 0.3}, # Pink
    "gold": {"hue": 0.15, "sat": 2.0, "val": 1.5, "tint": (255, 215, 0), "tint_strength": 0.3}, # Gold
}

os.makedirs(out_dir, exist_ok=True)

files = ["content.png", "fishing.png", "happy.png", "hungry.png", "sleep.png", "wink.png"]

for skin_id, props in skins.items():
    skin_dir = os.path.join(out_dir, skin_id)
    os.makedirs(skin_dir, exist_ok=True)
    
    for f in files:
        in_path = os.path.join(base_dir, f)
        out_path = os.path.join(skin_dir, f)
        
        if not os.path.exists(in_path):
            continue
            
        img = Image.open(in_path)
        img = hue_shift_image(img, props["hue"], props["sat"], props["val"])
        if props.get("tint"):
            img = apply_tint(img, props["tint"], props["tint_strength"])
            
        img.save(out_path)
        print(f"Generated {out_path}")

print("Done!")
