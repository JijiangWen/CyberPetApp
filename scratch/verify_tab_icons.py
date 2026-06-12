import os
from PIL import Image

ICONS_DIR = r"c:\Users\wen.jijiang\Desktop\blazor_test\CyberPetApp\wwwroot\assets\icons"
REQUIRED_ICONS = [
    "house", "fish", "hammer", "cat", "shop", "rod", "food", "bag", "alchemy", "milestone"
]

print("=== Verifying generated tab icons ===")
errors = 0
for name in REQUIRED_ICONS:
    path = os.path.join(ICONS_DIR, f"{name}.png")
    if not os.path.exists(path):
        print(f"[ERROR] Icon '{name}.png' not found at {path}!")
        errors += 1
        continue
        
    try:
        img = Image.open(path)
        w, h = img.size
        mode = img.mode
        if w == 32 and h == 32 and mode == "RGBA":
            print(f"[OK] Icon '{name}.png': (32x32, RGBA)")
        else:
            print(f"[ERROR] Icon '{name}.png' has invalid format: {w}x{h}, mode={mode}!")
            errors += 1
    except Exception as e:
        print(f"[ERROR] reading '{name}.png': {e}")
        errors += 1

if errors == 0:
    print("\nSUCCESS! All 10 tab icons exist and are correctly formatted.")
else:
    print(f"\nFAILED with {errors} errors.")
