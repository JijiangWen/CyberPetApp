import os
import sys
from PIL import Image

sys.stdout.reconfigure(encoding='utf-8')

files = [
    "河湾青壳虾.png",
    "溪底石蟹.png",
    "滑溜大泥鳅.png",
    "异变·铁骨溪鳗.png",
    "小透明鱿鱼仔.png",
    "玳瑁巨海龟.png",
    "暗河玻璃蝌蚪.png",
    "透明凝胶水母.png",
    "大灯笼安康鱼.png"
]

fish_dir = "wwwroot/assets/fish"
for f in files:
    path = os.path.join(fish_dir, f)
    if os.path.exists(path):
        img = Image.open(path)
        print(f"{f}: size={img.size}, mode={img.mode}")
    else:
        print(f"{f}: NOT FOUND")
